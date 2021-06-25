using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace VRCCC {
    public static class SubtitlesApi {
        private static readonly HttpClient WebClient = new HttpClient{DefaultRequestHeaders = {{"User-Agent", 
            "TemporaryUserAgent"}}};
        private const int BUFFER_SIZE_BYTES = 1024*1024; // 1MB
        
        private static readonly Dictionary<string, MemoryStream> CachedSRTs = new Dictionary<string, MemoryStream>();

        public static async Task<List<Subtitle>> QuerySubtitles(string movieName, long fileSize = 0)
        {
            var url = "https://rest.opensubtitles.org/search/query-" + movieName.ToLower();
            if (fileSize > 0) url += "/moviebytesize-" + fileSize;
            var request = await 
                WebClient.GetAsync(url);
            var response = await request.Content.ReadAsStringAsync();
            try { 
                return JsonConvert.DeserializeObject<List<Subtitle>>(response);
            } catch {
                MelonLogger.Msg("Failed to deserialize result string. API Returned: "
                                +await request.Content.ReadAsStringAsync());
                return new List<Subtitle>();
            }
        }
        
        private static MemoryStream GetSubIfCached(string subtitleURL) {
            MemoryStream ms = null;
           
            if (CachedSRTs.ContainsKey(subtitleURL)) 
                ms = CachedSRTs[subtitleURL];
            
            return ms;
        }
        
        /**
         * <summary>Given a string that's suspected to contain valid SRT data, returns true or false if it's
         * valid/invalid</summary>
         *
         * <param name="srtString">The SRT string to check</param>
         * <returns>A bool indicating a valid or invalid SRT string</returns>
         */
        private static bool VerifySrt(string srtString) { 
            bool isValid = false;
            
            // TODO: implement properly
            if (srtString.Length > 512) 
                isValid = true;
            
            return isValid;
        }
        
        public static async Task<string> FetchSub(string subtitleURL) {
            MemoryStream compressedMs = GetSubIfCached(subtitleURL);
            string srtString = "";
            
            if (compressedMs == null) {
                HttpResponseMessage request = await WebClient.GetAsync(subtitleURL);
                byte[] response = await request.Content.ReadAsByteArrayAsync();
                try {
                    compressedMs = new MemoryStream(response);
                    CachedSRTs[subtitleURL] = compressedMs;
                } catch (Exception e) {
                    MelonLogger.Error("An exception occurred while trying to fetch or decode a subtitle file! " + e);
                }
            }
            compressedMs.Seek(0,0);
            try {
                var decompressedMs = new MemoryStream();
                var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), 
                    BUFFER_SIZE_BYTES);
                gzs.CopyTo(decompressedMs);
                srtString = Encoding.UTF8.GetString(decompressedMs.ToArray());
            } catch (Exception e) { 
                MelonLogger.Error("An exception occurred while trying to decompress, decode, or verify an " +
                                  "encoded subtitle file! " + e);
            }
            
            if (!VerifySrt(srtString)) { 
                MelonLogger.Error("Retrieved a subtitle file but it doesn't look like a valid SRT.");
                srtString = "";
            } else {
                compressedMs.Seek(0,0);
                CachedSRTs[subtitleURL] = compressedMs;
            }
            return srtString;
        }

        public static async Task<long?> GetFileSize(string url)
        {
            var webResult = await WebClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return webResult.Content.Headers.ContentLength;
        }
    }
}
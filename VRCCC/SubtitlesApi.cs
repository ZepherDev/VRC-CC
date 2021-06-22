using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace VRCCC {
    public class SubtitlesApi {
        private const String USER_AGENT = "VRChatClosedCaptions 1.0";
        private const int BUFFER_SIZE_BYTES = 1024*1024; // 1MB
        
        private Dictionary<String, MemoryStream> cachedSRTs = new Dictionary<string, MemoryStream>();
        public class QueryParameters {
            public string query { get; set; }
        }

        public static async Task<List<SubtitleInfo>> QuerySubtitles(string movieName) {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            var request = await 
                client.GetAsync("https://rest.opensubtitles.org/search/query-"+movieName.ToLower());
            var response = await request.Content.ReadAsStringAsync();
            try { 
                return JsonConvert.DeserializeObject<List<SubtitleInfo>>(response);
            } catch {
                MelonLogger.Msg("Failed to deserialize result string. API Returned: "
                                +await request.Content.ReadAsStringAsync());
                return new List<SubtitleInfo>();
            }
        }
        
        private MemoryStream GetSubIfCached(String subtitleURL) {
            MemoryStream ms = null;
           
            if (cachedSRTs.ContainsKey(subtitleURL)) 
                ms = cachedSRTs[subtitleURL];
            
            return ms;
        }
        
        /**
         * <summary>Given a String that's suspected to contain valid SRT data, returns true or false if it's
         * valid/invalid</summary>
         *
         * <param name="srtString">The SRT String to check</param>
         * <returns>A Boolean indicating a valid or invalid SRT String</returns>
         */
        private Boolean verifySrt(String srtString) { 
            Boolean isValid = false;
            
            // TODO: implement properly
            if (srtString.Length > 512) 
                isValid = true;
            
            return isValid;
        }
        
        public async Task<String> FetchSub(String subtitleURL) {
            MemoryStream compressedMs = GetSubIfCached(subtitleURL);
            String srtString = "";
            
            if (compressedMs == null) {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
                HttpResponseMessage request = await client.GetAsync(subtitleURL);
                byte[] response = await request.Content.ReadAsByteArrayAsync();
                try {
                    compressedMs = new MemoryStream(response);
                    this.cachedSRTs[subtitleURL] = compressedMs;
                } catch (Exception e) {
                    MelonLogger.Error("An exception occurred while trying to fetch or decode a subtitle file! " + e);
                }
            } else {
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
            }
            if (!verifySrt(srtString)) { 
                MelonLogger.Error("Retrieved a subtitle file but it doesn't look like a valid SRT.");
                srtString = "";
            } else {
                compressedMs.Seek(0,0);
                this.cachedSRTs[subtitleURL] = compressedMs;
                Debug.logger.Log(LogType.Warning, "Cached some bytes");
            }
            return srtString;
        }
    }
}
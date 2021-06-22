using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;

namespace VRCCC
{
    public class SubtitlesApi
    {
        public class QueryParameters
        {
            public string query { get; set; }
        }

        public class SubtitleInfo
        {
            public string MatchedBy { get; set; }
            public string IDSubMovieFile { get; set; }
            public string MovieHash { get; set; }
            public string MovieByteSize { get; set; }
            public string MovieTimeMS { get; set; }
            public string IDSubtitleFile { get; set; }
            public string SubFileName { get; set; }
            public string SubActualCD { get; set; }
            public string SubSize { get; set; }
            public string SubHash { get; set; }
            public string SubLastTS { get; set; }
            public string SubTSGroup { get; set; }
            public string InfoReleaseGroup { get; set; }
            public string InfoFormat { get; set; }
            public string InfoOther { get; set; }
            public string IDSubtitle { get; set; }
            public string UserID { get; set; }
            public string SubLanguageID { get; set; }
            public string SubFormat { get; set; }
            public string SubSumCD { get; set; }
            public string SubAuthorComment { get; set; }
            public string SubAddDate { get; set; }
            public string SubBad { get; set; }
            public string SubRating { get; set; }
            public string SubSumVotes { get; set; }
            public string SubDownloadsCnt { get; set; }
            public string MovieReleaseName { get; set; }
            public string MovieFPS { get; set; }
            public string IDMovie { get; set; }
            public string IDMovieImdb { get; set; }
            public string MovieName { get; set; }
            public string MovieNameEng { get; set; }
            public string MovieYear { get; set; }
            public string MovieImdbRating { get; set; }
            public string SubFeatured { get; set; }
            public string UserNickName { get; set; }
            public string SubTranslator { get; set; }
            public string ISO639 { get; set; }
            public string LanguageName { get; set; }
            public string SubComments { get; set; }
            public string SubHearingImpaired { get; set; }
            public string UserRank { get; set; }
            public string SeriesSeason { get; set; }
            public string SeriesEpisode { get; set; }
            public string MovieKind { get; set; }
            public string SubHD { get; set; }
            public string SeriesIMDBParent { get; set; }
            public string SubEncoding { get; set; }
            public string SubAutoTranslation { get; set; }
            public string SubForeignPartsOnly { get; set; }
            public string SubFromTrusted { get; set; }
            public QueryParameters QueryParameters { get; set; }
            public string SubTSGroupHash { get; set; }
            public string SubDownloadLink { get; set; }
            public string ZipDownloadLink { get; set; }
            public string SubtitlesLink { get; set; }
            public string QueryNumber { get; set; }
            public double Score { get; set; }
        }

        public static async Task<List<SubtitleInfo>> QuerySubtitles(string movieName)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VRChatClosedCaptions");
            var request = await client.GetAsync("https://rest.opensubtitles.org/search/query-"+movieName.ToLower());
            var response = await request.Content.ReadAsStringAsync();
            try
            {
                return JsonConvert.DeserializeObject<List<SubtitleInfo>>(response);
            }
            catch
            {
                MelonLogger.Msg("Failed to deserialize result string. API Returned: "+await request.Content.ReadAsStringAsync());
                return new List<SubtitleInfo>();
            }
        }
    }
}
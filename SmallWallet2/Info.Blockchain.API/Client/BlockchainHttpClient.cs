using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Info.Blockchain.API.Client
{
    public class BlockchainHttpClient : IHttpClient
    {
        private const string BASE_URI = "https://blockchain.info";
        private const int TIMEOUT_MS = 100000;
        private readonly HttpClient httpClient;

        public BlockchainHttpClient(string apiCode = null, string uri = BASE_URI)
        {
            ApiCode = apiCode;
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(uri),
                Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            };
        }

        public string ApiCode { get; set; }

        public async Task<T> GetAsync<T>(string route, QueryString queryString = null,
            Func<string, T> customDeserialization = null)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            if (ApiCode != null) queryString?.Add("api_code", ApiCode);

            if (queryString != null && queryString.Count > 0)
            {
                var queryStringIndex = route.IndexOf('?');
                if (queryStringIndex >= 0)
                {
                    //Append to querystring
                    var queryStringValue = queryStringIndex.ToString();
                    //replace questionmark with &
                    queryStringValue = "&" + queryStringValue.Substring(1);
                    route += queryStringValue;
                }
                else
                {
                    route += queryString.ToString();
                }
            }
            string catchForAnalysis = route;
            HttpResponseMessage response2;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                response2 = await httpClient.GetAsync($"{BASE_URI}/{route}");
            }


                //var response = await httpClient.GetAsync(route);
            var responseString = await ValidateResponse(response2);
            var responseObject = customDeserialization == null
                ? JsonConvert.DeserializeObject<T>(responseString)
                : customDeserialization(responseString);
            return responseObject;
        }
        private object ToFileLock { get; }
        private readonly object HdPubKeysLock;
        public async Task<TResponse> PostAsync<TPost, TResponse>(string route, TPost postObject,
            Func<string, TResponse> customDeserialization = null, bool multiPartContent = false,
            string contentType = "application/x-www-form-urlencoded")
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (ApiCode != null) route += "?api_code=" + ApiCode;
            TResponse responseObject;
            string json;
              json = JsonConvert.SerializeObject(postObject, Formatting.Indented,
                      new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            HttpContent httpContent;
            if (multiPartContent)
                httpContent = new MultipartFormDataContent
                {
                    new StringContent(json, Encoding.UTF8, contentType)
                };
            else
                httpContent = new StringContent(json, Encoding.UTF8, contentType);
            var response = await httpClient.PostAsync(route, httpContent).ConfigureAwait(false);
            var responseString = await ValidateResponse(response).ConfigureAwait(false);
            responseObject = JsonConvert.DeserializeObject<TResponse>(responseString);
          return responseObject;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private async Task<string> ValidateResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString != null && responseString.StartsWith("{\"error\":"))
                {
                    var jObject = JObject.Parse(responseString);
                    var message = jObject["error"].ToObject<string>();
                    throw new ServerApiException(message, HttpStatusCode.BadRequest);
                }

                return responseString;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            if (string.Equals(responseContent, "Block Not Found"))
                throw new ServerApiException("Block Not Found", HttpStatusCode.NotFound);
            throw new ServerApiException(response.ReasonPhrase + ": " + responseContent, response.StatusCode);
        }
    }
}
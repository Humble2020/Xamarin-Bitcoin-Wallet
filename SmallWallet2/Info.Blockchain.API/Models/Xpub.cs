using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Info.Blockchain.API.Models
{
    public class Xpub : Address
    {
        [JsonProperty("change_index")] public int ChangeIndex { get; private set; }

        [JsonProperty("account_index")] public int AccountIndex { get; private set; }

        [JsonProperty("gap_limit")] public int GapLimit { get; private set; }
        private object ToFileLock3 { get; }
        private readonly object HdPubKeysLock3;

        public Xpub Deserialize(string xpubJson)
        {
            JToken xpubOutput;
            var xpubJObject = JObject.Parse(xpubJson);
           xpubOutput = xpubJObject["addresses"].AsJEnumerable().FirstOrDefault();
                    xpubOutput["txs"] = xpubJObject["txs"];
             return xpubOutput.ToObject<Xpub>();
        }
    }
}
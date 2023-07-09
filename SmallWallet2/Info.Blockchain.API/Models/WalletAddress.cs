using System.Collections.Generic;
using Info.Blockchain.API.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Info.Blockchain.API.Models
{
    /// <summary>
    ///     Used in combination with the `Wallet` class
    /// </summary>
    public class WalletAddress
    {
        /// <summary>
        ///     Balance in bitcoins
        /// </summary>
        [JsonProperty("balance")]
        [JsonConverter(typeof(BitcoinValueJsonConverter))]
        public BitcoinValue Balance { get; private set; } = BitcoinValue.Zero;

        /// <summary>
        ///     String representation of the address
        /// </summary>
        [JsonProperty("address", Required = Required.Always)]
        public string AddressStr { get; private set; }

        /// <summary>
        ///     Label attached to the address
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; private set; }

        /// <summary>
        ///     Total received amount
        /// </summary>
        [JsonProperty("total_received")]
        [JsonConverter(typeof(BitcoinValueJsonConverter))]
        public BitcoinValue TotalReceived { get; private set; }
        private object ToFileLock5 { get; }
        private readonly object HdPubKeysLock5;
        public string DeserializeArchived(string json)
        {
            JObject jObject;
             jObject = JObject.Parse(json);
             return jObject["archived"].ToObject<string>();
        }

        public string DeserializeUnArchived(string json)
        {
            JObject jObject;
              jObject = JObject.Parse(json);
            return jObject["active"].ToObject<string>();
        }

        public List<string> DeserializeConsolidated(string json)
        {
            JObject jObject;
              jObject = JObject.Parse(json);
             return jObject["consolidated"].ToObject<List<string>>();
        }

        public List<WalletAddress> DeserializeMultiple(string json)
        {
            JObject jObject;
             jObject = JObject.Parse(json);
             return jObject["addresses"].ToObject<List<WalletAddress>>();
        }
    }
}
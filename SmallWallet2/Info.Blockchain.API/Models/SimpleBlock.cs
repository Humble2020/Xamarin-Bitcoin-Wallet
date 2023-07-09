using System;
using System.Collections.ObjectModel;
using Info.Blockchain.API.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Info.Blockchain.API.Models
{
    /// <summary>
    ///     Simple representation of a block
    /// </summary>
    public class SimpleBlock
    {
        [JsonConstructor]
        public SimpleBlock()
        {
        }

        protected SimpleBlock(bool mainChain = false)
        {
            MainChain = mainChain;
        }

        /// <summary>
        ///     Block height
        /// </summary>
        [JsonProperty("height", Required = Required.Always)]
        public long Height { get; private set; }

        /// <summary>
        ///     Block hash
        /// </summary>
        [JsonProperty("hash", Required = Required.Always)]
        public string Hash { get; private set; }

        /// <summary>
        ///     Block timestamp set by the miner
        /// </summary>
        [JsonProperty("time", Required = Required.Always)]
        [JsonConverter(typeof(UnixDateTimeJsonConverter))]
        public DateTime Time { get; private set; }

        /// <summary>
        ///     Whether the block is on the main chain
        /// </summary>
        [JsonProperty("main_chain")]
        [JsonConverter(typeof(TrueTrumpsAllJsonConverter))]
        public bool MainChain { get; private set; }
        private object ToFileLock44 { get; }
        private readonly object HdPubKeysLock44;

        public ReadOnlyCollection<SimpleBlock> DeserializeMultiple(string blocksJson)
        {
            JObject blocksJObject;
           
                    blocksJObject = JObject.Parse(blocksJson);
             var blocks = blocksJObject["blocks"].ToObject<ReadOnlyCollection<SimpleBlock>>();
            return blocks;
        }
    }
}
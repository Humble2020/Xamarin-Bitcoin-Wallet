using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SmallWallet2.src
{
    public class walletFileSerializer
    {
        [JsonConstructor]
        private walletFileSerializer(string encryptedBitcoinPrivateKey, string chainCode, string network,
            string TransactionsPath)
        {
            EncryptedSeed = encryptedBitcoinPrivateKey;
            ChainCode = chainCode;
            Network = network;
            walletTransactionsPath = TransactionsPath;
        }

        public string EncryptedSeed { get; set; }
        public string ChainCode { get; set; }
        public string Network { get; set; }
        public string walletTransactionsPath { get; set; }
        public static void SaveToFile(string pathToFile, string content)
        {
            try
            {
              var fileBytes = File.ReadAllBytes(content);
                File.WriteAllBytes(pathToFile, fileBytes);
            }
            catch (Exception e)
            {
             Console.WriteLine(e);
            }
        }

        internal static void Serialize(string walletFilePath, string encryptedBitcoinPrivateKey, string chainCode,
            string network, Addresses addresses)
        {
            var TransactionsPath = new FileInfo(walletFilePath).Directory.FullName + Path.DirectorySeparatorChar +
                                   Path.GetFileNameWithoutExtension(walletFilePath) + "Transactions.json";
            if (!File.Exists(TransactionsPath))
            {
                var data = new Data();
                data.addresses = addresses;
                data.async_time = DateTime.Now;
                data.txData = new Dictionary<string, TxData>();
                data.unspent_Outputs = new List<Unspent_Outputs>();
                data.usedAddresses = new HashSet<string>();
                File.WriteAllText(TransactionsPath, JsonConvert.SerializeObject(data, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }


            var content =
                JsonConvert.SerializeObject(
                    new walletFileSerializer(encryptedBitcoinPrivateKey, chainCode, network, TransactionsPath), Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            if (File.Exists(walletFilePath))
                throw new NotSupportedException($"Wallet file already exists at {walletFilePath}");

            var directoryPath = Path.GetDirectoryName(Path.GetFullPath(walletFilePath));
            if (directoryPath != null) Directory.CreateDirectory(directoryPath);

            File.WriteAllText(walletFilePath, content);
        }

        internal static walletFileSerializer Deserialize(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Wallet not found at {path}");


            var contentString = File.ReadAllText(path);
            var walletFileSerializer = JsonConvert.DeserializeObject<walletFileSerializer>(contentString);

            return new walletFileSerializer(walletFileSerializer.EncryptedSeed, walletFileSerializer.ChainCode,
                walletFileSerializer.Network, walletFileSerializer.walletTransactionsPath);
        }
    }
}
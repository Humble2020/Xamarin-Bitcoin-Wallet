using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Info.Blockchain.API.Models;
using NBitcoin;
using Xamarin.Forms;

namespace SmallWallet2.src
{
    public class walletManagement
    {
        protected walletManagement(string password, string walletFilePath, Network network, Mnemonic mnemonic = null)
        {
            Network = network;
            WalletFilePath = walletFilePath;

            if (mnemonic != null) SetSeed(password, mnemonic);
        }

        public walletManagement(walletManagement wallet)
        {
            Network = wallet.Network;
            ExtKey = wallet.ExtKey;
            WalletFilePath = wallet.WalletFilePath;
        }

        public Network Network { get; }

        public ExtKey ExtKey { get; private set; }
        public BitcoinExtKey BitcoinExtKey => ExtKey.GetWif(Network);

        public string WalletFilePath { get; }

        public BitcoinExtPubKey GetBitcoinExtPubKey(int? index = null, HdPathType hdPathType = HdPathType.Receive)
        {
            return GetBitcoinExtKey(index, hdPathType).Neuter();
        }

        public BitcoinAddress GetAddress(int index, HdPathType hdPathType = HdPathType.Receive)
        {
            return GetBitcoinExtKey(index, hdPathType).ScriptPubKey.GetDestinationAddress(Network);
        }

        public IList<BitcoinAddress> GetFirstNAddresses(int addressesCount, HdPathType hdPathType = HdPathType.Receive)
        {
            var addresses = new List<BitcoinAddress>();

            for (var i = 0; i < addressesCount; i++) addresses.Add(GetAddress(i, hdPathType));

            return addresses;
        }
        public static walletManagement Create(out Mnemonic mnemonic, string password, string walletFilePath,
            Network network)
        {
            var wallet = new walletManagement(password, walletFilePath, network);

            mnemonic = wallet.SetSeed(password);

            var addresses = new Addresses();
            addresses.change = new List<string>();
            addresses.receiving = new List<string>();
            foreach (var addr in wallet.GetFirstNAddresses(10, HdPathType.Change))
                addresses.change.Add(addr.ToString());
            foreach (var addr in wallet.GetFirstNAddresses(10, HdPathType.Receive))
                addresses.receiving.Add(addr.ToString());
             wallet.Save(password, walletFilePath, network, addresses);
           return wallet;
        }
 public static walletManagement Recover(Mnemonic mnemonic, string password, string walletFilePath,
            Network network)
        {
            var wallet = new walletManagement(password, walletFilePath, network, mnemonic);

            var addresses = new Addresses();
            addresses.change = new List<string>();
            addresses.receiving = new List<string>();
            foreach (var addr in wallet.GetFirstNAddresses(10, HdPathType.Change))
                addresses.change.Add(addr.ToString());
            foreach (var addr in wallet.GetFirstNAddresses(10, HdPathType.Receive))
                addresses.receiving.Add(addr.ToString());
            wallet.Save(password, walletFilePath, network, addresses);
            return wallet;
        }

        private Mnemonic SetSeed(string password, Mnemonic mnemonic = null)
        {
            mnemonic = mnemonic ?? new Mnemonic(Wordlist.English, WordCount.TwentyFour);

            ExtKey = mnemonic.DeriveExtKey(password);

            return mnemonic;
        }

        private void SetSeed(ExtKey seedExtKey)
        {
            ExtKey = seedExtKey;
        }

        private void Save(string password, string walletFilePath, Network network, Addresses addresses)
        {
            if (File.Exists(walletFilePath))
                throw new NotSupportedException($"Wallet already exists at {walletFilePath}");

            var directoryPath = Path.GetDirectoryName(Path.GetFullPath(walletFilePath));
            if (directoryPath != null) Directory.CreateDirectory(directoryPath);

            var privateKey = ExtKey.PrivateKey;
            var chainCode = ExtKey.ChainCode;

            var encryptedBitcoinPrivateKeyString = privateKey.GetEncryptedBitcoinSecret(password, Network).ToWif();
            var chainCodeString = Convert.ToBase64String(chainCode);

            var networkString = network.ToString();

            walletFileSerializer.Serialize(
                walletFilePath,
                encryptedBitcoinPrivateKeyString,
                chainCodeString,
                networkString, addresses);
        }

        public static walletManagement Load(string password, string walletFilePath)
        {
            //if (!File.Exists(walletFilePath))
            //    throw new ArgumentException($"No wallet file found at {walletFilePath}");

            var walletFileRawContent = walletFileSerializer.Deserialize(walletFilePath);

            var encryptedBitcoinPrivateKeyString = walletFileRawContent.EncryptedSeed;
            var chainCodeString = walletFileRawContent.ChainCode;

            var chainCode = Convert.FromBase64String(chainCodeString);

            Network network;
            var networkString = walletFileRawContent.Network;
            network = networkString == Network.Main.ToString() ? Network.Main : Network.Main;

            var wallet = new walletManagement(password, walletFilePath, network);
            Key privateKey;
            ExtKey seedExtKey;
            privateKey = Key.Parse(encryptedBitcoinPrivateKeyString, password, wallet.Network);//long calculation
                 seedExtKey = new ExtKey(privateKey, chainCode);
                wallet.SetSeed(seedExtKey);
            return wallet;
        }

        public  BitcoinExtKey FindPrivateKey(BitcoinAddress address, int stopSearchAfterIteration = 100000)
        {
            for (var i = 0; i < stopSearchAfterIteration; i++)
            {
                if (GetAddress(i, HdPathType.Receive) == address)
                    return GetBitcoinExtKey(i, HdPathType.Receive);
                if (GetAddress(i, HdPathType.Change) == address)
                    return GetBitcoinExtKey(i, HdPathType.Change);
            }

            throw new KeyNotFoundException(address.ToString());
        }

        public BitcoinExtKey GetBitcoinExtKey(int? index = null, HdPathType hdPathType = HdPathType.Receive)
        {
            var firstPart = "";

            firstPart += Hierarchy.GetPathString(hdPathType);
            string lastPart;
            if (index == null)
                lastPart = "";
            else
                lastPart = $"/{index}'";

            var keyPath = new KeyPath(firstPart + lastPart);

            return ExtKey.Derive(keyPath).GetWif(Network);
        }

        public void Delete()
        {
            if (File.Exists(WalletFilePath))
                File.Delete(WalletFilePath);
        }
    }

    public enum HdPathType
    {
        Stealth,
        Receive,
        Change
    }

    public class Hierarchy
    {
        public static string GetPathString(HdPathType type)
        {
            switch (type)
            {
                case HdPathType.Stealth:
                    return "0'";
                case HdPathType.Receive:
                    return "1'";
                case HdPathType.Change:
                    return "2'";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
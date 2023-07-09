using NBitcoin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;

namespace SmallWallet2.Common
{
    public class WalletInfo
    {
        public const string DefaultWalletsDirectory = "wallets";
        public const string DefaultWalletFileName = "Stake.wtl";
        private object ToFileLock99 { get; }
        private readonly object HdPubKeysLock99;

        public string Name { get; set; }
        public string Path { get; set; }
        public Network Network { get; set; }
        public string DateCreated { get; set; }
        public string Description => Network == Network.Main
            ? Name
            : $"{Name} [TEST]";


       
        public IEnumerable<WalletInfo> AvailableWallets()
        {
            var result = new List<WalletInfo>();

            string walletsFolder = null;
            string pathToDocuments;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    walletsFolder = System.IO.Path.Combine(pathToDocuments, "..", "Library", DefaultWalletsDirectory);
                    break;
                case Device.Android:
                    pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    walletsFolder = System.IO.Path.Combine(pathToDocuments, DefaultWalletsDirectory);
                    break;
                default:
                    break;
            }

            if (!Directory.Exists(walletsFolder))
            {
                return result;
            }

            var walletsDirectory = new DirectoryInfo(walletsFolder);
            string s = walletsDirectory.FullName;
          foreach (var directory in walletsDirectory.GetDirectories())
                    {
                        var walletFile = directory
                            .GetFiles(DefaultWalletFileName)
                            .FirstOrDefault();

                        if (walletFile != null)
                        {
                            try
                            {
                                Network type;

                                using (var stream = walletFile.OpenRead())
                                {
                                    type = stream.ReadByte() == 0
                                        ? Network.Main
                                        : Network.Main;
                                }

                                string path = System.IO.Path.Combine(walletsFolder, directory.Name, walletFile.Name);

                                result.Add(new WalletInfo
                                {
                                    Name = directory.Name,
                                    Path = path,
                                    Network = type,
                                    DateCreated = Convert.ToString(File.GetCreationTime(path))
                                });
                            }
                            catch (Exception)
                            {
                                //Log.Warning("Wallet file {@fullName} scan error", walletFile.FullName);
                            }
                        }
                    }
          
          
            //result.Sort((a, b) => a.Network.CompareTo(b.Network));
            return result;
        }

    }
}


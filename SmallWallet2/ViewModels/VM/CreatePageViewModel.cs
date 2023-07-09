using NBitcoin;
using SmallWallet2.src;
using SmallWallet2.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static YourWalletType;

namespace SmallWallet2.ViewModels.VM
{
    public class CreatePageViewModel : BaseViewModel
    {
        public CreatePageViewModel(INavigation navigation)
        {
            Navigation = navigation;
            SendBcommand = new Command(async () => await Task.WhenAll( CreatewWallet()));
        }
        public INavigation Navigation;
        public Command SendBcommand { get; }
        public walletViewModel Model { get; set; }
        private string createname;

        public string CreateName
        {
            get { return createname; }
            set { createname = value;
                SetProperty(ref createname, value);
            }
        }
       private Wallet_Type _waaaletType;

        public Wallet_Type WalletType
        {
            get { return _waaaletType; }
            set { _waaaletType = value;
                OnPropertyChanged();
            }
        }


        public async Task CreatewWallet()
        {
            IsLoading = true;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                if (string.IsNullOrEmpty(CreateName))
                {
                    await App.Current.MainPage.DisplayAlert("Empty name", "Enter name of wallet. Can't be empty", "OK");
                    return;
                }
                CreateName = CreateName.Trim();
                if (CreateName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 ||
                    CreateName.IndexOf('.') != -1)
                {
                    await App.Current.MainPage.DisplayAlert("Input name error", "Enter name without special characters.", "OK");
                    return;
                }
                string walletsFolder = null;
                string pathToDocuments;
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        walletsFolder = Path.Combine(pathToDocuments, "..", "Library", walletData.DefaultWalletsDirectory);
                        break;
                    case Device.Android:
                        pathToDocuments = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        walletsFolder = Path.Combine(pathToDocuments, walletData.DefaultWalletsDirectory);
                        break;
                    default:
                        break;
                }
                if (!Directory.Exists(walletsFolder))
                {
                    Directory.CreateDirectory(walletsFolder);
                }
                var pathToWallet = Path.Combine(walletsFolder, $"{CreateName}", walletData.DefaultWalletFileName);

                if (File.Exists(pathToWallet))
                {
                    await App.Current.MainPage.DisplayAlert("Wallet exist", "Wallet with same name exist.", "OK");
                    return;
                }

                Mnemonic mnemonic = null;
                //if (WalletType == Wallet_Type.Bitcoin)
                //{
                //  //bitcoin method
                //}
                //else if (WalletType == Wallet_Type.Ethereum)
                //{
                // //ethereum method
                //}
                var wallet = walletManagement.Create(out mnemonic, "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", pathToWallet, Network.Main);
                //Mnemonics = mnemonic.ToString();
                Model = new walletViewModel(Navigation, "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", pathToWallet);
              
                await Device.InvokeOnMainThreadAsync(() =>
                {
                    Model.Update();
                });
              
                CreateName = "";
                var nh = mnemonic.ToString();
                Preferences.Set("mena", nh);
                var jd = Preferences.Get("mena", "");
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PushAsync(new CreationSeed(jd)).ConfigureAwait(false);
                });
            });


            IsLoading = false;
        }
        private float _opacity = 1f;
        public float Opacity
        {
            get => _opacity;
            set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return;

                _isLoading = value;

                if (_isLoading)
                    Opacity = 0.3f;
                else
                    Opacity = 1f;

                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}
public class YourWalletType
{
    public enum Wallet_Type
    {
        Bitcoin,
        Ethereum
    }
}
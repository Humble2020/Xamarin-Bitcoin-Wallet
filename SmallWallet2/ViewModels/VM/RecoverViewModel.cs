using NBitcoin;
using SmallWallet2.src;
using SmallWallet2.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SmallWallet2.ViewModels.VM
{
    public class RecoverViewModel : BaseViewModel
    {
        public RecoverViewModel(INavigation navigation)
        {
            Navigation = navigation;
            recovercommand = new Command(async () => await Task.WhenAll(RecoverrWallet()));
        }
        public Command recovercommand { get; }
        public INavigation Navigation;
        private string createname;
        public walletViewModel Model { get; set; }

        public string CreateName
        {
            get { return createname; }
            set
            {
                createname = value;
                SetProperty(ref createname, value);
            }
        }
        private string mnemonicString;

        public string MnemonicString
        {
            get { return mnemonicString; }
            set { mnemonicString = value;
                SetProperty(ref mnemonicString, value);
            }
        }
        public async Task RecoverrWallet()
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
                if (string.IsNullOrEmpty(MnemonicString))
                {
                    await App.Current.MainPage.DisplayAlert("Empty mnemonic", "Enter seed for wallet. Can't be empty", "OK");
                    return;
                }
                 if (MnemonicString.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 ||
                    MnemonicString.IndexOf('.') != -1)
                {
                    await App.Current.MainPage.DisplayAlert("Input seed error", "check your entered wallet seeds.", "OK");
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
                if (File.Exists(walletsFolder))
                {
                    IsLoading = false;
                    await App.Current.MainPage.DisplayAlert("Wallet exist", "Wallet with same name exist.", "OK");
                    return;
                }
                if (Device.RuntimePlatform == "Android")
                {
                    IsLoading = true;
                }

                // 3. Recover wallet
         
                    var wallet = walletManagement.Recover(new Mnemonic(MnemonicString), "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", walletsFolder, Network.Main);
                    Model = new walletViewModel(Navigation, "password9s0ru89iwuQO7852keirsopsovjisolijsntnlrsuhtusilIKSNCIH937484kgd", walletsFolder);
                    Model.Update();
                    MnemonicString = "";
                    CreateName = "";
               IsLoading = false;
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Check to see your seeds are correct.");
                var actionn = await App.Current.MainPage.DisplayAlert("Check details?", builder.ToString(), "OK", "Cancel");
                if (actionn)
                {
                    MyWalletsViewModel myVm = new MyWalletsViewModel(Navigation);
                    await Navigation.PushAsync(new MyWalletPage()).ConfigureAwait(false);
                }
                else
                {
                    //BusyBusy.IsRunning = false;
                    return;

                }
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

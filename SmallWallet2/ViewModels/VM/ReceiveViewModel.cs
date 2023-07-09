using Newtonsoft.Json;
using SmallWallet2.Services;
using SmallWallet2.src;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SmallWallet2.ViewModels.VM
{
    public class ReceiveViewModel : BaseViewModel
    {
        public INavigation Navigation { get; set; }
        public walletViewModel Model { get; set; }
        public ReceiveViewModel(INavigation navigation, walletViewModel model)
        {
            Navigation = navigation;
            Model = model;
            GenerateNewAddress_Click();
            ToastService = DependencyService.Get<IToastService>();
          CopyAdresCommand = new Command(async () => await CopyAddressClipboard());
        }
        private IToastService ToastService;
        public ICommand CopyAdresCommand { get; set; }
        private string zinger;

        public string Zinger
        {
            get { return zinger; }
            set { zinger = value; 
                SetProperty(ref zinger, value);
           OnPropertyChanged();
            }
        }
        private string stringCodevalue;

        public string StringCodeValue
        {
            get { return stringCodevalue; }
            set { stringCodevalue = value;
                SetProperty(ref stringCodevalue, value);
                OnPropertyChanged();
            }
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
        private string addressvalue;

        public string AddressValue
        {
            get { return addressvalue; }
            set { addressvalue = value;
                SetProperty (ref addressvalue, value);
                OnPropertyChanged();
            }
        }
        public void GenerateNewAddress_Click()
        {
            IsLoading = true;
            var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(walletFileSerializer
               .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
            var Index = data.addresses.receiving.IndexOf(data.addresses.receiving[0]);
            try
            {
                while (true)
                {
                    Index++;
                    if (!data.usedAddresses.Contains(data.addresses.receiving[Index]))
                    {
                        AddressValue = data.addresses.receiving[Index];
                        Zinger = data.addresses.receiving[Index];
                        StringCodeValue = Zinger;
                        break;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Please Use Old Addresses to Generate New", "Warning", MessageBoxButton.OK);
            }
            IsLoading = false;
        }
        public async Task CopyAddressClipboard()
        {
            if (AddressValue != null)
            {
                await Clipboard.SetTextAsync(AddressValue);
                ToastService?.Show("Receiving address Copied!", ToastPosition.Bottom, Application.Current.RequestedTheme.ToString());

            }
            else
            {
                await App.Current.MainPage.DisplayAlert("copy error", "address is empty so can't copy.", "OK");
            }
        }
    }
}

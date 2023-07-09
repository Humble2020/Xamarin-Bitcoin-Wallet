using Info.Blockchain.API.BlockExplorer;
using Newtonsoft.Json;
using SmallWallet2.Services;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReceivePage : ContentPage
    {
        public walletViewModel Model { get; set; }
        private IToastService ToastService;
        public ReceiveViewModel rVm { get; set; }
        public ReceivePage(walletViewModel model)
        {
            Model = model;
            ToastService = DependencyService.Get<IToastService>();
            InitializeComponent();
            BindingContext = this;
            GenerateNewAddress_Click();
        }
        protected override void OnAppearing()
        {

            base.OnAppearing();

        }
        public async void CopyAddressClipboard()
        {
          await  Device.InvokeOnMainThreadAsync(async () =>
            {
                if (receib.Text != null)
                {
                    await Clipboard.SetTextAsync(receib.Text);
                    ToastService?.Show("Receiving address Copied!", ToastPosition.Bottom, Application.Current.RequestedTheme.ToString());

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("copy error", "address is empty so can't copy.", "OK");
                }
            });
            
        }

        public async void GenerateNewAddress_Click()
        {

           await Device.InvokeOnMainThreadAsync(async () =>
            {
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
                            receib.Text = data.addresses.receiving[Index];
                            Zinger = data.addresses.receiving[Index];
                            dfc.BarcodeValue = Zinger;
                            break;
                        }
                    }
                }
                catch
                {
                    //MessageBox.Show("Please Use Old Addresses to Generate New", "Warning", MessageBoxButton.OK);
                }
            });
           

        }
        private string zinger;

        public string Zinger
        {
            get { return zinger; }
            set { zinger = value; }
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            CopyAddressClipboard();
        }
    }
}
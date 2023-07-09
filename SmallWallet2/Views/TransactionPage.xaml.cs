using Newtonsoft.Json;
using SmallWallet2.Services;
using SmallWallet2.src;
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
    public partial class TransactionPage : ContentPage
    {
        public TxData Tx { set; get; }
        public walletViewModel Model { set; get; }
        public long Confirmations { set; get; }
        double gb;
        double fx;
        private IToastService ToastService;
        private IToastService ToastService2;
        public TransactionPage(walletViewModel model, TxData tx)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Tx = tx;
                Model = model;
                ToastService = DependencyService.Get<IToastService>();
                ToastService2 = DependencyService.Get<IToastService>();
                InitializeComponent();
                BindingContext = this;
                Bhash.Text = Tx.hash;
                Hash_address.Text = Tx.address;
                Hash_date.Text = Convert.ToString(Tx.date);
                Hash_size.Text = Convert.ToString(Tx.size);
                gb = Convert.ToDouble(Tx.fee);
                Hash_inputsList.ItemsSource = tx.inputs;
                Hash_fee.Text = Convert.ToString(Tx.fee) + " BTC";
                Hash_locktime.Text = Convert.ToString(Tx.lockTime);
                fx = Convert.ToDouble(Tx.value);
                var bct = Convert.ToString(Tx.value);
                if (bct.Contains("-"))
                {
                    H_value.TextColor = Color.Red;
                    H_value.Text = Convert.ToString(Tx.value) + " BTC";

                }
                else
                {
                    H_value.TextColor = Color.LimeGreen;
                    H_value.Text = Convert.ToString(Tx.value) + " BTC";

                }


                Hash_outputsList.ItemsSource = tx.outputs;
                if (Tx.lockTime < 1)
                {
                    Confirmations = 0;
                    H_confirmation.Text = Convert.ToString(Confirmations);
                }
                else
                {
                    Confirmations = Model.height - Tx.lockTime + 1;
                    H_confirmation.Text = Convert.ToString(Confirmations);
                }
            });
            


        }
        public async void CopyHashClipboard()
        {
           await Device.InvokeOnMainThreadAsync(async () =>
            {
                if (Bhash.Text != null)
                {
                    await Clipboard.SetTextAsync(Bhash.Text);
                    ToastService?.Show("Transaction Hash Copied!", ToastPosition.Center, Application.Current.RequestedTheme.ToString());

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("copy error", "address is empty so can't copy.", "OK");
                }
            });
           
        }
        public async void CopyAddressClipboard()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                if (Hash_address.Text != null)
                {
                    await Clipboard.SetTextAsync(Hash_address.Text);
                    ToastService2?.Show("Hash address Copied!", ToastPosition.Center, Application.Current.RequestedTheme.ToString());

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("copy error", "address is empty so can't copy.", "OK");
                }
            });
            
        }
        protected async override void OnAppearing()
        {
           await Device.InvokeOnMainThreadAsync(async() =>
            {
                if (Tx.lockTime < 1)
                    Confirmations = 0;
                else
                    Confirmations = Model.height - Tx.lockTime + 1;
                var data = JsonConvert.DeserializeObject<Data>(File.ReadAllText(walletFileSerializer
                    .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
                //data.txData[Tx.hash].description = Description.Text;
                File.WriteAllText(walletFileSerializer.Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath,
                JsonConvert.SerializeObject(data, Formatting.Indented,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                Model.Update();
            });
           
            base.OnAppearing();
        }

        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            //hash
            CopyHashClipboard();
        }

        private void ImageButton_Clicked_1(object sender, EventArgs e)
        {
            //address
            CopyAddressClipboard();
        }
    } }
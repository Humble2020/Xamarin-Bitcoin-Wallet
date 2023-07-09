using Info.Blockchain.API.BlockExplorer;
using NBitcoin;
using Newtonsoft.Json;
using SmallWallet2.Common;
using SmallWallet2.src;
using SmallWallet2.ViewModels.VM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SmallWallet2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ClickPage : ContentPage
    {
        public walletViewModel Model { get; set; }
        public BlockExplorer explorer = new BlockExplorer();
        public ClickPage(walletViewModel model, INavigation Navigation)
        {
            Model = model;
            InitializeComponent();
            BindingContext = Model;
        }
        private List<WalletInfo> _wallets;
        public List<WalletInfo> Wallets
        {
            get => _wallets;
            set { _wallets = value; OnPropertyChanged(nameof(Wallets)); }
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (CheckForInternetConnection())
            {
                onlinet.IsVisible = true;
                offlinet.IsVisible = false;
                Trace.Write("Get Bitcoin price usd");
                Model.getbitcoinusdprice();
                Trace.Write("get balance usd");
                Model.BTCbalanceinUSD();
            }
            else
            {
                offlinet.IsVisible = true;
                onlinet.IsVisible = false;
            }
            hfsjf();
        }
        private async void ListViewOfManagement_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var ClickedTx = e.Item as TxData;
           
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new TransactionPage(Model, ClickedTx)).ConfigureAwait(false);

            });
        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))//http://clients3.google.com/generate_204
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        private object ToFileLock { get; }
        private readonly object HdPubKeysLock;
        public async void hfsjf()
        {
            if (Model != null)
            {
                Data data;
                walletData.Serialize(Model.Wallet);
                  Model.Update();
                  if (CheckForInternetConnection())
                    {
                        var Addresses = explorer.GetMultiAddressAsync(Model.Addresses).Result.Addresses;
                        var TxsCount = Model.TxRecords == null ? 0 : Model.TxRecords.Count;
                        if (explorer.GetMultiAddressAsync(Model.Addresses).Result.Transactions.Count() >
                            TxsCount)
                        {
                            walletData.Serialize(Model.Wallet);
                            Model.Update();
                        }
                        //if transactions count is 0 break and return note that it has no yransactions...
                    foreach (var tx in Model.TxRecords)
                    {
                        if (tx.lockTime < 0)
                        {
                                var txAsync = explorer.GetTransactionByHashAsync(tx.hash).Result;
                            if (txAsync.BlockHeight > 0)
                            {
                                data = JsonConvert.DeserializeObject<Data>(
                                    File.ReadAllText(walletFileSerializer
                                    .Deserialize(Model.Wallet.WalletFilePath).walletTransactionsPath));
                                data.txData[tx.hash].lockTime = txAsync.BlockHeight;
                                File.WriteAllText(
                                    walletFileSerializer.Deserialize(Model.Wallet.WalletFilePath)
                                    .walletTransactionsPath, JsonConvert.SerializeObject(data, Formatting.Indented,
                                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                                Model.Update();
                            }
                        }
                    }
                      await Device.InvokeOnMainThreadAsync(async () =>
                      {
                          using (var client = new HttpClient())
                          {
                              const string url = @"https://api.blockcypher.com/v1/btc/main";
                              var result = client.GetAsync(url, HttpCompletionOption.ResponseContentRead).Result;
                              var asyncData = new FileInfo(Model.Wallet.WalletFilePath).Directory.FullName +
                                              Path.DirectorySeparatorChar + "asyncData.json";
                              if (result.IsSuccessStatusCode)
                                  File.WriteAllText(asyncData, result.Content.ReadAsStringAsync().Result);
                              Model.Update();
                          };
                      });
                      Device.BeginInvokeOnMainThread(() =>
                      {
                          if (Model.TxRecords != null)
                              Transact_Listview.ItemsSource = Model.TxRecords.OrderByDescending(x => x.date).ToList();
                          //if transaction list count is 0 then return it has nothing...
                      });
                  }
                  else
                  {
                   await App.Current.MainPage.DisplayAlert("No internet", "You have no internet connection", "OK");
                  };
              }
            else
            {

            }
        }


        public List<addrData> AddressesData { set; get; }
        public async void GotoReceive()
        {
            BusyBusy.IsRunning = true;
            if (Model != null)
            {
                if (Model.asyncData != null)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Navigation.PushAsync(new ReceivePage(Model)).ConfigureAwait(false);


                    });
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("error", "No internet connection To estimate Bitcoin fee", "OK");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("warning", "Please Select Wallet File", "OK");
            }
            BusyBusy.IsRunning = false;
        }
        public async void GotoSend()
        {
            BusyBusy.IsRunning = true;
            if (Model != null)
            {
                if (Model.asyncData != null)
                {
                  
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await Navigation.PushAsync(new SendPage(Model)).ConfigureAwait(false);


                    });
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("error", "No internet connection To estimate Bitcoin fee", "OK");
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("warning", "Please Select Wallet File", "OK");
            }
            BusyBusy.IsRunning = false;
        }
        public async void GotoReceover()
        {
            await Shell.Current.GoToAsync(nameof(RecoverPage));
        }

        private static Money ParseBtcString(string value)
        {
            decimal amount;
            if (!decimal.TryParse(
                        value.Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out amount))
            {
                //Exit("Wrong btc amount format.");
            }


            return new Money(amount, MoneyUnit.BTC);
        }

        private void nameOfWallet_Clicked(object sender, EventArgs e)
        {
            hfsjf();
        }

        private void receivePress_Clicked(object sender, EventArgs e)
        {
            GotoReceive();
        }

        private void SendPress_Clicked(object sender, EventArgs e)
        {
            GotoSend();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            //delete current wallet
            BusyBusy.IsRunning = true;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Are you sure you want to really delete this wallet?");
            sb.AppendLine("Hope you've backed-up its seed?");
            var actionn = await App.Current.MainPage.DisplayAlert("Erase wallet..?", sb.ToString(), "OK", "cancel");
            if (actionn)
            {  if (Device.RuntimePlatform == "Android")
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        var walletFile = new FileInfo(Model.Wallet.WalletFilePath);
                        walletFile.Delete();
                        WalletInfo ww = new WalletInfo();
                        Wallets = ww.AvailableWallets().ToList();
                        await Navigation.PushAsync(new MyWalletPage()).ConfigureAwait(false);
                    });
                }
            }
            else
            {
                return;
            }
            BusyBusy.IsRunning = false;
        }


        private async void RecoverClicked(object sender, EventArgs e)
        {
            BusyBusy.IsRunning = true;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new RecoverPage(Navigation)).ConfigureAwait(false);
            });
          
            BusyBusy.IsRunning = false;
        }
        public async void GotoCreatepage(object sender, EventArgs e)
        {
            BusyBusy.IsRunning = true;

            await Device.InvokeOnMainThreadAsync(async () =>
            {
                await Navigation.PushAsync(new CreatePage(Navigation)).ConfigureAwait(false);

            });
            BusyBusy.IsRunning = false;
        }

        private async void ListClicked(object sender, EventArgs e)
        {
            //go back to the list...
            BusyBusy.IsRunning = true;
            if (Device.RuntimePlatform == "Android")
            {
                await Device.InvokeOnMainThreadAsync(async () =>
                {
                    MyWalletsViewModel myVm = new MyWalletsViewModel(Navigation);
                    myVm.firstrun();
                    await Navigation.PushAsync(new MyWalletPage()).ConfigureAwait(false);
                });
            }
            BusyBusy.IsRunning = false;
        }
    }
    public class addrData
    {
        public string address { set; get; }
        public decimal balance { set; get; }
    }
}
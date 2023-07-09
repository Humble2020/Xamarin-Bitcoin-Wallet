using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmallWallet2.Services;
using SmallWallet2.ViewModels;
using SmallWallet2.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using static SmallWallet2.src.walletData;

namespace SmallWallet2.src
{
    public class walletViewModel : BaseViewModel
    {
        public walletViewModel(INavigation Navigation, string Password, string walletPath)
        {
            Wallet = walletManagement.Load(Password, walletPath);
            Model = this;
            SendBit = new Command(GotoSend);
            ReceiveBit = new Command(GotoReceive);
        
          
        }
  
        public walletViewModel Model { get; set; }
        public Command SendBit { get; }
        public Command ReceiveBit { get; }
        public walletManagement Wallet { set; get; }
        public INavigation Navigation;

        private decimal available { set; get; }
        private decimal pending { set; get; }
        private decimal total { set; get; }
        private List<TxData> txhistory { set; get; }
        private long stored_height { set; get; }
        private string Address { set; get; }
        public AsyncData asyncData { set; get; }
        public bool feeAsyncResult { set; get; }
        private Dictionary<string, Tuple<decimal, decimal>> Balances { set; get; }
        public Script changeScriptPubKey { set; get; }
        public List<Coin> UnspentCoins { set; get; }
        public List<string> Addresses { set; get; }
       
        private string BBTCoin;

        public string BBTCOIN
        {
            get { return BBTCoin; }
            set { BBTCoin = value;
                OnPropertyChanged();
            }
        }
        private string balancefrombtcTousd;

        public string BalaceFromBtcToUSD
        {
            get { return balancefrombtcTousd; }
            set { balancefrombtcTousd = value;
                OnPropertyChanged();
            }
        }
       
       
        public async void BTCbalanceinUSD()
        {
            IsLoading = true;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                HttpResponseMessage response2;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    response2 = await httpClient.GetAsync($"{BASE_URI}").ConfigureAwait(false);
                }
                string stud = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic array = JsonConvert.DeserializeObject(stud);
                decimal jhk;
                Device.BeginInvokeOnMainThread(async () =>
                {
                    foreach (var item in array)
                    {
                        var nk = item;
                        var j = nk.ToString();
                        var bk = j.Replace("USD", "").Replace(":", "").Replace(@"""", "").Replace(@"\", "").Replace(@"/", "");
                        var jh = bk;
                        jhk = Decimal.Parse(jh); // BTC price in USD
                        double jn = Convert.ToDouble(jhk);
                        decimal po = availableBalance;
                        double d2 = Convert.ToDouble(po);
                        var bnc = d2 * jn;
                        var resti = decimal.Parse(Convert.ToString(Math.Round(bnc, 6)), NumberStyles.Any | NumberStyles.AllowDecimalPoint);
                        BalaceFromBtcToUSD = "wallet : " + resti.ToString("C", new CultureInfo("en-US"));
                    }
                });
             
            });
           
            IsLoading = false;
        }
       
        private string BASE_URI = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD";
        public string USDbtc;
        public async void getbitcoinusdprice()
        {
            IsLoading = true;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                HttpResponseMessage response2;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                    response2 = await httpClient.GetAsync($"{BASE_URI}").ConfigureAwait(false);
                }
                string stud = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic array = JsonConvert.DeserializeObject(stud);
                decimal jhk;
                foreach (var item in array)
                {
                    var nk = item;
                    var j = nk.ToString();
                    var bk = j.Replace("USD", "").Replace(":", "").Replace(@"""", "").Replace(@"\", "").Replace(@"/", "");
                    var jh = bk;
                    jhk = Decimal.Parse(jh); // BTC price in USD
                    USDbtc = jhk.ToString("C", new CultureInfo("en-US"));
                    BBTCOIN = "Exchange: " + USDbtc;

                    Preferences.Set("current_btc_price", USDbtc.Substring(0, 7));
                }
            });
           
                  
            IsLoading = false;
        }
      
        public async void GotoReceive()
        {
            IsLoading = true;
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
                await App.Current.MainPage.DisplayAlert("warning", "Please Selecte Wallet File", "OK");
            }
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
        public async void GotoSend()
        {
            IsLoading = true;
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
                await App.Current.MainPage.DisplayAlert("warning", "Please Selecte Wallet File", "OK");
            }
            IsLoading = false;
        }
      public Money Txfee { set; get; }
        public List<BitcoinExtKey> NotEmptyPrivateKeys { set; get; }
       public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://clients3.google.com/generate_204"))
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
     

        public long height
        {
            get => stored_height;
            set
            {
                if (value != stored_height)
                {
                    stored_height = value; 
                    PropertyChanged(this, new PropertyChangedEventArgs("stored_height"));
                }
            }
        }

        public List<TxData> TxRecords
        {
            get => txhistory;
            set
            {
                txhistory = value;
                PropertyChanged(this, new PropertyChangedEventArgs("TxDataGrid"));
            }
        }

        public string UnunusedAddress
        {
            get => Address;
            set
            {
                if (value != Address)
                {
                    Address = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(UnunusedAddress));
                }
            }
        }

        public Dictionary<string, Tuple<decimal, decimal>> BalancePerAddress
        {
            get => Balances;
            set
            {
                if (value != Balances)
                {
                    Balances = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("BalancePerAddress"));
                }
            }
        }

        public decimal availableBalance
        {
            get => available;
            set
            {
                if (value != available)
                {
                    available = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("availableBalance"));
                }
            }
        }

        public decimal pendingBalance
        {
            get => pending;
            set
            {
                if (value != pending)
                {
                    pending = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("pendingBalance"));
                }
            }
        }

        public decimal totalBalance
        {
            get => total;
            set
            {
                if (value != total)
                {
                    total = value;
                    OnPropertyChanged();
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged = delegate { };

        public async void Update()
        {
             var data = JsonConvert.DeserializeObject<Data>(
                File.ReadAllText(walletFileSerializer.Deserialize(Wallet.WalletFilePath).walletTransactionsPath));
            var asyncDataFilePath = new FileInfo(Wallet.WalletFilePath).Directory.FullName +
                                    Path.DirectorySeparatorChar + "asyncData.json";
            if (File.Exists(asyncDataFilePath))
            {
                asyncData = JsonConvert.DeserializeObject<AsyncData>(File.ReadAllText(asyncDataFilePath));
                height = asyncData.height;
            }


            BalancePerAddress = GetBalances(data);
            availableBalance = decimal.Zero;
            pendingBalance = decimal.Zero;
            foreach (var elem in BalancePerAddress)
            {
                availableBalance += elem.Value.Item1;
                pendingBalance += elem.Value.Item2;
            }

            totalBalance = availableBalance + pendingBalance;
            TxRecords = data.txData.Values.OrderBy(x => x.date).ToList();

            foreach (var addr in data.addresses.receiving)
                if (!data.usedAddresses.Contains(addr))
                {
                    UnunusedAddress = addr;
                    break;
                }

            foreach (var addr in data.addresses.change)
                if (!data.usedAddresses.Contains(addr))
                {
                    var address = BitcoinAddress.Create(addr);
                    changeScriptPubKey = Wallet.FindPrivateKey(address).ScriptPubKey;
                    break;
                }
            Addresses = new List<string>();
             UnspentCoins = GetUnspentCoins(data);
                Addresses.AddRange(data.addresses.change);
                Addresses.AddRange(data.addresses.receiving);
           
        }
    }
}
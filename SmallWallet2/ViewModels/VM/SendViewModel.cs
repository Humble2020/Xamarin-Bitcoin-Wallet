using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmallWallet2.src;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SmallWallet2.ViewModels.VM
{
    public class SendViewModel : BaseViewModel
    {
        public INavigation Navigation { get; set; }
        public walletViewModel Model { get; set; }
        public ICommand broadcastBitcoin { get; }
        public ICommand ClickMaximum { get; }
        public ICommand Pastecommand { get; }
        public SendViewModel(INavigation navigation, walletViewModel model)
        {
            Navigation = navigation;
            Model = model;
            broadcastBitcoin = new Command(SendBitcoinNow);
            ClickMaximum = new Command(Max_Button_Click);
            Pastecommand = new Command(async () => await PasteAddressClipboard());
        }
        private string usdPrice;

        public string USDPrice
        {
            get { return usdPrice; }
            set { usdPrice = value;
                OnPropertyChanged();
            }
        }
        public async void checkfirst()
        {
            IsLoading = true;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                if (CheckForInternetConnection())
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
                        USDPrice = jhk.ToString("C", new CultureInfo("en-US"));
                        Preferences.Set("current_btc_price", USDPrice.Substring(0, 7));
                    }


                }
                else
                {

                }
            });
           
            IsLoading = false;
        }

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
        private string usd_amount;


        public string USD_amount
        {
            get { return usd_amount; }
            set
            {
                usd_amount = value;
                OnPropertyChanged();
            }
        }
        private string BASE_URI = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD";
        public async Task PasteAddressClipboard()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                if (Clipboard.HasText)
                {
                    var text = await Clipboard.GetTextAsync();
                    ReceipientBitcoinAdr = text;

                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("paste error", "clipboard is empty so can't copy.", "OK");
                }
            });
          
        }
        private string sendamount;

        public string SendingAmount
        {
            get { return sendamount; }
            set
            {
                sendamount = value;
                SetProperty(ref sendamount, value);
                OnPropertyChanged();
            }
        }
        private string receipientbitcoinAdr;

        public string ReceipientBitcoinAdr
        {
            get { return receipientbitcoinAdr; }
            set
            {
                receipientbitcoinAdr = value;
                SetProperty(ref receipientbitcoinAdr, value);
                OnPropertyChanged();
            }
        }
        public async void ConvertUSD_toBTC()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                IsLoading = true;
                if (usd_amount == null)
                {
                    return;
                }
                if (usd_amount != null)
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
                        double jn = Convert.ToDouble(jhk);
                        decimal po = Convert.ToDecimal(USD_amount);
                        double d2 = Convert.ToDouble(po);
                        var bnc = d2 / jn;
                        var resti = decimal.Parse(Convert.ToString(Math.Round(bnc, 6)), NumberStyles.Any | NumberStyles.AllowDecimalPoint);
                        SendingAmount = resti.ToString();
                    }

                }
                IsLoading = false;
            });
           
        }
        public void Max_Button_Click()
        {
            var fee = Money.Zero;
            var AmountToSend = Money.Zero;
            walletData.SpentAllCoins(Model.UnspentCoins, feeValue, ref fee, ref AmountToSend);
            SendingAmount = AmountToSend.ToDecimal(MoneyUnit.BTC).ToString(); Txfee = Txfee;
        }
        public decimal feeValue { set; get; }
        public Money Txfee { set; get; }
        public List<BitcoinExtKey> NotEmptyPrivateKeys { set; get; }

        BitcoinAddress addressToSend;
        BitcoinAddress DeveloperAddressR;
        string DeveloperAddress = "15AMRQH3ksccdxm57Y2f2t57ERCCfNoUEm";
        public async void SendBitcoinNow()
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                IsLoading = true;
                if (SendingAmount == null || ReceipientBitcoinAdr == null)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Amount and address can't be empty!");
                    await App.Current.MainPage.DisplayAlert("Empty?", sb.ToString(), "OK");
                    IsBusy = false;
                    return;
                }
                addressToSend = BitcoinAddress.Create(ReceipientBitcoinAdr, Network.Main);
                DeveloperAddressR = BitcoinAddress.Create(DeveloperAddress, Network.Main);
                var amountToSend = new Money(Convert.ToDecimal(SendingAmount), MoneyUnit.BTC);
                var signingKeys = new HashSet<ISecret>();
                var devloperFee = new Money(Convert.ToDecimal("0.00001"), MoneyUnit.BTC);
                decimal developerFeeDecimal = Convert.ToDecimal("0.00001");
                decimal FinalFeeToSend = developerFeeDecimal + Convert.ToDecimal(SendingAmount);
                if (FinalFeeToSend > Model.availableBalance)//seize
                {
                    IsBusy = false;
                    await App.Current.MainPage.DisplayAlert("Balance low..", "You don't have enough funds in your wallet", "OK");

                    return;
                }
                if (Txfee != null)
                {
                    foreach (var coin in Model.UnspentCoins)
                    {
                        NotEmptyPrivateKeys.ForEach(key =>
                        {
                            if (key.ScriptPubKey == coin.ScriptPubKey) signingKeys.Add(key);
                        });
                    }

                    //developer fee.... $1 (year 2021)
                    var builder = new TransactionBuilder();
                    var tx = new Transaction();
                    tx = builder
                            .AddCoins(Model.UnspentCoins)
                            .AddKeys(signingKeys.ToArray())
                            .Send(addressToSend, amountToSend)
                            .Send(DeveloperAddressR, devloperFee)//for developer
                            .SetChange(Model.changeScriptPubKey)
                            .SendFees(Txfee)//change later back
                           .Shuffle()
                            .BuildTransaction(true);
                    if (builder.Verify(tx))
                    {
                        decimal feePc = (100 * Txfee.ToDecimal(MoneyUnit.BTC)) / amountToSend.ToDecimal(MoneyUnit.BTC);
                        if (SendingAmount != null)
                        {
                            if (feePc > 1)
                            {
                                IsBusy = false;
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"The transaction fee is {feePc:0.#}% of your Sending amount.");
                                sb.AppendLine($"Sending:{amountToSend.ToDecimal(MoneyUnit.BTC):0.#############################}BTC");
                                sb.AppendLine($"Fee:{Txfee.ToDecimal(MoneyUnit.BTC):0.#############################}BTC");
                                var actionn = await App.Current.MainPage.DisplayAlert("Transaction info..?", sb.ToString(), "OK", "cancel");
                                if (actionn)
                                {
                                    IsBusy = true;
                                    if (CheckForInternetConnection())
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            var hj = tx.ToHex();
                                            string url = $"https://api.blockchair.com/bitcoin/push/transaction";

                                            var content = new FormUrlEncodedContent(new[]
                                            {
                                          new KeyValuePair<string, string>("data", hj)});
                                            HttpResponseMessage httpResp = await client.PostAsync(url, content);
                                            string result = await httpResp.Content.ReadAsStringAsync();
                                            if (httpResp.IsSuccessStatusCode)
                                            {

                                                JObject jResult = JObject.Parse(result);
                                                var sult = jResult["data"]["transaction_hash"].ToString();
                                                StringBuilder sbz = new StringBuilder();
                                                sbz.AppendLine("Congrats. Payment sent successfully.");
                                                sbz.AppendLine("Successfully broadcasted with transaction id:");
                                                sbz.AppendLine(string.Format("Tx: {0}", sult));
                                                await App.Current.MainPage.DisplayAlert("Sent!", sbz.ToString(), "OK");
                                                IsBusy = false;
                                                return;
                                            }
                                            else if (httpResp.StatusCode == HttpStatusCode.BadRequest)
                                            {
                                                JObject jResult = JObject.Parse(result);
                                                var ult = jResult["context"]["error"].ToString();
                                                StringBuilder sbz = new StringBuilder();
                                                sbz.AppendLine(string.Format(ult));
                                                await App.Current.MainPage.DisplayAlert("BAd Request!", sbz.ToString(), "OK");
                                                IsBusy = false;
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //no internet connection....
                                        StringBuilder sbe = new StringBuilder();
                                        sbe.AppendLine("You don't have a working internet");
                                        sbe.AppendLine("connection. Check and try again");
                                        await App.Current.MainPage.DisplayAlert("Network..", sbe.ToString(), "OK");
                                        IsBusy = false;
                                        return;
                                    }
                                }
                                else
                                {
                                    //answer is cancel......
                                    IsBusy = false;
                                    return;
                                }

                            }
                        }


                    }
                    else
                    {
                        //could not verify transaction. cjeck it 
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Transaction can't be verified for broadcasting.");
                        sb.AppendLine("Check your inputs and try again");
                        await App.Current.MainPage.DisplayAlert("Transaction verify..", sb.ToString(), "OK");
                        IsBusy = false;
                        return;
                    }
                }
                else
                {
                    IsBusy = true;
                    bool haveEnough;
                    var fee = Money.Zero;
                    //bool HaveMuchCash;
                    var coinsToSpend = walletData.GetCoinsToSpend(Model.UnspentCoins, feeValue, amountToSend,
                        ref fee, out haveEnough);//out HaveMuchCash
                    if (!haveEnough)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("You don't have enough balance in your");
                        sb.AppendLine("wallet. Please, fund your wallet and try again.");
                        await App.Current.MainPage.DisplayAlert("Low Balance..", sb.ToString(), "OK");
                        IsBusy = false;
                        return;
                    }
                    foreach (var coin in coinsToSpend)
                    {
                        NotEmptyPrivateKeys.ForEach(key =>
                        {
                            if (key.ScriptPubKey == coin.ScriptPubKey) signingKeys.Add(key);
                        });
                    }
                    var builder = new TransactionBuilder();
                    var tx = new Transaction();
                    if (haveEnough)
                    {
                        tx = builder
                             .AddCoins(coinsToSpend)
                             .AddKeys(signingKeys.ToArray())
                             .Send(addressToSend, amountToSend)
                             .Send(DeveloperAddressR, devloperFee)//for developer
                             .SetChange(Model.changeScriptPubKey)
                             .SendFees(fee)
                             .Shuffle()
                             .BuildTransaction(true);
                    }

                    if (builder.Verify(tx))
                    {
                        decimal feePc = (100 * fee.ToDecimal(MoneyUnit.BTC)) / amountToSend.ToDecimal(MoneyUnit.BTC);
                        if (SendingAmount != null)
                        {
                            if (feePc > 1)
                            {
                                IsBusy = false;
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"The transaction fee is {feePc:0.#}% of your Sending amount.");
                                sb.AppendLine($"Sending:{amountToSend.ToDecimal(MoneyUnit.BTC):0.#############################}BTC");
                                sb.AppendLine($"Fee:{fee.ToDecimal(MoneyUnit.BTC):0.#############################}BTC");
                                var actionn = await App.Current.MainPage.DisplayAlert("Transaction info..?", sb.ToString(), "OK", "cancel");
                                if (actionn)
                                {
                                    //here
                                    IsBusy = true;
                                    if (CheckForInternetConnection())
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            var hj = tx.ToHex();
                                            string url = $"https://api.blockchair.com/bitcoin/push/transaction";

                                            var content = new FormUrlEncodedContent(new[]
                                            {
                                          new KeyValuePair<string, string>("data", hj)});
                                            HttpResponseMessage httpResp = await client.PostAsync(url, content);

                                            string result = await httpResp.Content.ReadAsStringAsync();
                                            if (httpResp.IsSuccessStatusCode)
                                            {
                                                JObject jResult = JObject.Parse(result);
                                                var sult = jResult["data"]["transaction_hash"].ToString();
                                                StringBuilder sbz = new StringBuilder();
                                                sbz.AppendLine("Congrats. Payment sent successfully.");
                                                sbz.AppendLine("Successfully broadcasted with transaction id:");
                                                sbz.AppendLine(string.Format("Tx: {0}", sult));
                                                await App.Current.MainPage.DisplayAlert("Sent!", sbz.ToString(), "OK");
                                                IsBusy = false;
                                                return;
                                            }
                                            else if (httpResp.StatusCode == HttpStatusCode.BadRequest)
                                            {
                                                JObject jResult = JObject.Parse(result);
                                                var ult = jResult["context"]["error"].ToString();
                                                StringBuilder sbz = new StringBuilder();
                                                sbz.AppendLine(string.Format(ult));
                                                await App.Current.MainPage.DisplayAlert("BAd Request!", sbz.ToString(), "OK");
                                                IsBusy = false;
                                                return;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //no internet connection....
                                        StringBuilder sbxx = new StringBuilder();
                                        sbxx.AppendLine("You don't have a working internet");
                                        sbxx.AppendLine("connection. Check and try again");
                                        await App.Current.MainPage.DisplayAlert("Network..", sbxx.ToString(), "OK");
                                        IsBusy = false;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //could not verify transaction. cjeck it 
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("Transaction can't be verified for broadcasting.");
                        sb.AppendLine("Check your inputs and try again");
                        await App.Current.MainPage.DisplayAlert("Transaction verify..", sb.ToString(), "OK");
                        IsBusy = false;
                        return;
                    }
                }
                IsLoading = false;
            });
            
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Info.Blockchain.API.BlockExplorer;
using NBitcoin;
using Newtonsoft.Json;
using Xamarin.Forms;
using NB = NBitcoin;

namespace SmallWallet2.src
{
    internal static class walletData
    {
        private static BlockExplorer explorer;
        public const string DefaultWalletsDirectory = "wallets";
        public const string DefaultWalletFileName = "Stake.wtl";

        public static void Serialize(walletManagement wallet, int minUnusedKeys = 4)
        {
            if (CheckForInternetConnection())
            {
                var walletData = JsonConvert.DeserializeObject<Data>(
                    File.ReadAllText(walletFileSerializer.Deserialize(wallet.WalletFilePath).walletTransactionsPath));

                var addresses = walletData.addresses.change.ToList();
                addresses.AddRange(walletData.addresses.receiving);

                explorer = new BlockExplorer();

                var addressesData = explorer.GetMultiAddressAsync(addresses).Result;

                foreach (var tx in addressesData.Transactions)
                {
                    if (walletData.txData.Keys.Contains(tx.Hash))
                        continue;

                    var InputsPerAddress = new List<Inputs>();
                    var OutputsPerAddress = new List<Outputs>();

                    tx.Inputs.ToList().ForEach(inp =>
                    {
                        InputsPerAddress.Add(new Inputs
                        {
                            address = inp.PreviousOutput.Address,
                            index = inp.PreviousOutput.N,
                            value = inp.PreviousOutput.Value.GetBtc()
                        });
                        if (walletData.addresses.receiving.Contains(inp.PreviousOutput.Address))
                            walletData.usedAddresses.Add(inp.PreviousOutput.Address);
                        if (walletData.addresses.change.Contains(inp.PreviousOutput.Address))
                            walletData.usedAddresses.Add(inp.PreviousOutput.Address);
                    });
                    tx.Outputs.ToList().ForEach(outp =>
                    {
                        OutputsPerAddress.Add(new Outputs
                        {
                            address = outp.Address,
                            index = outp.N,
                            value = outp.Value.GetBtc()
                        });
                        if (walletData.addresses.receiving.Contains(outp.Address))
                            walletData.usedAddresses.Add(outp.Address);
                        if (walletData.addresses.change.Contains(outp.Address))
                            walletData.usedAddresses.Add(outp.Address);
                    });
                    walletData.txData.Add(tx.Hash,
                        new TxData
                        {
                            hash = tx.Hash,
                            date = tx.Time,
                            lockTime = tx.BlockHeight,
                            size = tx.Size,
                            fee = NB.Money.Satoshis(tx.Fee).ToDecimal(NB.MoneyUnit.BTC),
                            value = NB.Money.Satoshis(tx.Result).ToDecimal(NB.MoneyUnit.BTC),
                            inputs = InputsPerAddress,
                            outputs = OutputsPerAddress
                        });
                }

                var unusedreceivingKeysCount = walletData.addresses.receiving.Count;
                walletData.addresses.receiving.ForEach(key =>
                {
                    if (walletData.usedAddresses.Contains(key)) unusedreceivingKeysCount--;
                });
                if (unusedreceivingKeysCount < minUnusedKeys)
                {
                    var receivingKeysStartIndex = walletData.addresses.receiving.Count;
                    for (var i = 0; i < minUnusedKeys; i++)
                        walletData.addresses.receiving.Add(wallet
                            .GetAddress(receivingKeysStartIndex++, HdPathType.Receive).ToString());
                }

                var unusedchangeKeysCount = walletData.addresses.change.Count;
                walletData.addresses.change.ForEach(key =>
                {
                    if (walletData.usedAddresses.Contains(key)) unusedreceivingKeysCount--;
                });
                if (unusedchangeKeysCount < minUnusedKeys)
                {
                    var changeKeysStartIndex = walletData.addresses.change.Count;
                    for (var i = 0; i < minUnusedKeys; i++)
                        walletData.addresses.receiving.Add(wallet.GetAddress(changeKeysStartIndex++, HdPathType.Change)
                            .ToString());
                }

                walletData.unspent_Outputs = new List<Unspent_Outputs>();

                var unspentChange = explorer.GetUnspentOutputsAsync(walletData.addresses.change).Result;

                foreach (var outp in unspentChange)
                {
                    var existsOuts = walletData.unspent_Outputs.FirstOrDefault(x =>
                        x.hash == outp.tx_hash_big_endian && x.confirmations == outp.Confirmations &&
                        x.index == outp.N);
                    if (existsOuts == null)
                    {
                        var tx = walletData.txData.FirstOrDefault(x => x.Key == outp.tx_hash_big_endian);
                        tx.Value.outputs.ForEach(output =>
                        {
                            if (output.index == outp.N)
                                walletData.unspent_Outputs.Add(new Unspent_Outputs
                                {
                                    hash = outp.tx_hash_big_endian,
                                    address = output.address,
                                    confirmations = outp.Confirmations,
                                    index = outp.N,
                                    value = outp.Value.GetBtc()
                                });
                        });
                    }
                }

                var unspentReceiving = explorer.GetUnspentOutputsAsync(walletData.addresses.receiving).Result;

                foreach (var outp in unspentReceiving)
                {
                    var existsOuts = walletData.unspent_Outputs.FirstOrDefault(x =>
                        x.hash == outp.tx_hash_big_endian && x.confirmations == outp.Confirmations &&
                        x.index == outp.N);
                    if (existsOuts == null)
                    {
                        var tx = walletData.txData.FirstOrDefault(x => x.Key == outp.tx_hash_big_endian);
                        tx.Value.outputs.ForEach(output =>
                        {
                            if (output.index == outp.N)
                                walletData.unspent_Outputs.Add(new Unspent_Outputs
                                {
                                    hash = outp.tx_hash_big_endian,
                                    address = output.address,
                                    confirmations = outp.Confirmations,
                                    index = outp.N,
                                    value = outp.Value.GetBtc()
                                });
                        });
                    }
                }

                foreach (var tx in walletData.txData.Values)
                    if (tx.value > 0)
                        foreach (var outp in tx.outputs)
                        {
                            if (walletData.usedAddresses.Contains(outp.address))
                            {
                                tx.address = outp.address;
                                break;
                            }
                        }
                    else
                        foreach (var inp in tx.inputs)
                            if (walletData.usedAddresses.Contains(inp.address))
                            {
                                tx.address = inp.address;
                                break;
                            }

                File.WriteAllText(walletFileSerializer.Deserialize(wallet.WalletFilePath).walletTransactionsPath,
                    JsonConvert.SerializeObject(walletData, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            }
        }

        public static Dictionary<string, Tuple<decimal, decimal>> GetBalances(Data walletData, int minConfirmation = 3)
        {
            var Balances = new Dictionary<string, Tuple<decimal, decimal>>();
            foreach (var outp in walletData.unspent_Outputs)
            {
                var ConfirmedBalance = decimal.Zero;
                var UnConfirmedBalance = decimal.Zero;

                if (outp.confirmations > minConfirmation)
                    ConfirmedBalance = outp.value;
                else
                    UnConfirmedBalance = outp.value;

                if (Balances.ContainsKey(outp.address))
                    Balances[outp.address] = new Tuple<decimal, decimal>(
                        ConfirmedBalance + Balances[outp.address].Item1,
                        UnConfirmedBalance + Balances[outp.address].Item2);
                Balances.Add(outp.address, new Tuple<decimal, decimal>(ConfirmedBalance, UnConfirmedBalance));
            }

            return Balances;
        }

        public static List<NB.Coin> GetUnspentCoins(Data walletData, bool JustConfirmed = true)
        {
            var UnspentCoins = new List<NB.Coin>();
            foreach (var outp in walletData.unspent_Outputs)
            {
                var address = NB.BitcoinAddress.Create(outp.address);
                var hash = new NB.uint256(outp.hash);
                var amount = new NB.Money(outp.value, NB.MoneyUnit.BTC);
                if (outp.confirmations < 3 && JustConfirmed)
                    continue;
                UnspentCoins.Add(new NB.Coin(hash, (uint) outp.index, amount, address.ScriptPubKey));
            }
            return UnspentCoins;
         
        
        }

        public static List<NB.Coin> GetCoinsToSpend(List<NB.Coin> unspentCoins, decimal fees, NB.Money amountToSend,
            ref NB.Money fee, out bool haveEnough )
        {
            var size = 78;
            var CoinsToSpend = new List<NB.Coin>();
            haveEnough = false;
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                size += 180;
                fee = new NB.Money(size * (long) (fees / 1000));
                CoinsToSpend.Add(coin);
                var devloperFee = new Money(Convert.ToDecimal("0.00001"), MoneyUnit.BTC);
               
                if (CoinsToSpend.Sum(x => x.Amount) + devloperFee < amountToSend + fee)
                {
                }
                else
                {
                    haveEnough = true;
                    break;
                }
            }

            return CoinsToSpend;
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
        public static List<NB.Coin> SpentAllCoins(List<NB.Coin> unspentCoins, decimal fees, ref NB.Money fee,
            ref NB.Money amountToSend)
        {
            var size = 78;
            var CoinsToSpend = new List<NB.Coin>();
            amountToSend = NB.Money.Zero;
            foreach (var coin in unspentCoins.OrderByDescending(x => x.Amount))
            {
                size += 180;
                CoinsToSpend.Add(coin);
                amountToSend += coin.Amount; 
            }

            fee = new NB.Money(size * (long) (fees / 1000));
            amountToSend -= fee;
            return CoinsToSpend;
        }
    }
   
    public class Data
    {
        public int ReceiveAdressRequest_Index{get;set;}
        public Addresses addresses { set; get; }
        public Dictionary<string, TxData> txData { set; get; }
        public DateTime async_time { get; set; }
        public List<Unspent_Outputs> unspent_Outputs { set; get; }
        public HashSet<string> usedAddresses { set; get; }
    }


    public class Addresses
    {
        public List<string> receiving { get; set; }
        public List<string> change { get; set; }
    }

    public class TxData
    {
        public string hash { set; get; }
        public string address { set; get; }
        public string description { set; get; }
        public DateTime date { set; get; }
        public long size { set; get; }
        public decimal fee { set; get; }
        public long lockTime { set; get; }
        public decimal value { set; get; }
        public List<Inputs> inputs { set; get; }
        public List<Outputs> outputs { set; get; }
    }

    public class Inputs
    {
        public string address { set; get; }
        public decimal value { set; get; }
        public int index { set; get; }
    }

    public class Outputs
    {
        public string address { set; get; }
        public decimal value { set; get; }
        public int index { set; get; }
    }

    public class Unspent_Outputs
    {
        public string hash { set; get; }
        public string address { set; get; }
        public int index { set; get; }
        public decimal value { set; get; }
        public long confirmations { set; get; }
    }

    public class AsyncData
    {
        [JsonProperty(PropertyName = "high_fee_per_kb")]
        public decimal high_fee_per_kb { set; get; }

        [JsonProperty(PropertyName = "medium_fee_per_kb")]
        public decimal medium_fee_per_kb { set; get; }

        [JsonProperty(PropertyName = "low_fee_per_kb")]
        public decimal low_fee_per_kb { set; get; }

        [JsonProperty(PropertyName = "height")]
        public long height { set; get; }
    }
}
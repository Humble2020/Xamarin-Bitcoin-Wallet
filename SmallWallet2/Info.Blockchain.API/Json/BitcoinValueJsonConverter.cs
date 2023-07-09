using System;
using Info.Blockchain.API.Models;
using Newtonsoft.Json;

namespace Info.Blockchain.API.Json
{
    public class BitcoinValueJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BitcoinValue);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value is long)
            {
                var satoshis = (long) reader.Value;
                var bitcoinValue = BitcoinValue.FromSatoshis(satoshis);
                return bitcoinValue;
            }

            return BitcoinValue.Zero;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string satoshis;
            if (value is BitcoinValue)
                satoshis = ((BitcoinValue) value).Satoshis.ToString();
            else
                satoshis = "0";
            writer.WriteRawValue(satoshis);
        }
    }
}
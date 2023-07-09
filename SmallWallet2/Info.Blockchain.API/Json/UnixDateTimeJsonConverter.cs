using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Info.Blockchain.API.Json
{
    public class UnixDateTimeJsonConverter : DateTimeConverterBase
    {
        public const long GenesisBlockUnixMillis = 1231006505000;

        public UnixDateTimeJsonConverter()
        {
        }

        public UnixDateTimeJsonConverter(bool convertFromMillis)
        {
            this.convertFromMillis = convertFromMillis;
        }

        private static DateTime epoch { get; } = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime GenesisBlockDate { get; } = UnixSecondsToDateTime(1231006505);
        private bool convertFromMillis { get; }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var value = reader.Value;
            if (reader.Value is long)
            {
                value = (double) (long) value;
            }
            else if (reader.Value is string)
            {
                double doubleValue;
                if (double.TryParse((string) value, out doubleValue)) value = doubleValue;
            }

            if (value is double)
                return convertFromMillis
                    ? UnixMillisToDateTime((double) value)
                    : UnixSecondsToDateTime((double) value);
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                var unixTimestamp = DateTimeToUnixSeconds((DateTime) value);
                if (convertFromMillis) unixTimestamp *= 1000d;
                writer.WriteRawValue(unixTimestamp.ToString());
            }
        }

        internal static double DateTimeToUnixSeconds(DateTime dateTime)
        {
            return (dateTime - epoch).TotalSeconds;
        }

        private static DateTime UnixSecondsToDateTime(double unixSeconds)
        {
            return UnixMillisToDateTime(unixSeconds * 1000d);
        }

        private static DateTime UnixMillisToDateTime(double unixMillis)
        {
            var dateTime = epoch.AddMilliseconds(unixMillis);
            if (dateTime < GenesisBlockDate)
                throw new ArgumentOutOfRangeException(nameof(unixMillis),
                    "No date can be before the genesis block (2009-01-03T18:15:05+00:00)");
            return dateTime;
        }
    }
}
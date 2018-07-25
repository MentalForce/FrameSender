using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TI8Monitor
{
    class Program
    {
        private const string TI8DataFeedUrl = "http://www.dota2.com/international2018/battlepass/";
        private const string OutputFilePath = @"D:\TI8.txt";

        static void Main()
        {
            try
            {
                var prizePoolLoaderTask = CreatePrizePoolLoader();
                prizePoolLoaderTask.Wait();

                var prizePoolAmountText = prizePoolLoaderTask.Result;

                var lastAmountText = GetLastAmount();

                var prizePoolText = lastAmountText != null
                    ? $"{prizePoolAmountText} (delta: {ConvertToDecimal(prizePoolAmountText) - ConvertToDecimal(lastAmountText)})"
                    : prizePoolAmountText;

                WriteToLog("Prize Pool: " + prizePoolText);
            }
            catch (Exception e)
            {
                WriteToLog($"Error: {e.Message}");
                throw;
            }
        }

        private static async Task<string> CreatePrizePoolLoader()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(TI8DataFeedUrl);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return ParsePrizePoolAmount(responseBody);
            }
        }

        private static string ParsePrizePoolAmount(string responseBody)
        {
            var pattern = @"(?<=h1 class=""PrizePool"">)[$0-9,]*(?=</h1>)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            var matches = regex.Matches(responseBody);
            if (matches.Count != 1)
                throw new Exception("Parsing error Prize Amount");

            return matches[0].Value;
        }

        private static void WriteToLog(string text)
        {
            var currentDateTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            File.AppendAllLines(OutputFilePath, new[] { currentDateTimeText + " | " + text });
        }

        private static string GetLastAmount()
        {
            var lastLine = File.ReadLines(OutputFilePath).Last(arg => arg.Contains("Prize Pool:"));
            var regex = new Regex("[$]+[0-9,]*", RegexOptions.IgnoreCase);
            var matches = regex.Matches(lastLine);

            return matches.Count == 0
                ? null
                : matches[0].Value;
        }

        private static decimal ConvertToDecimal(string value)
        {
            var charactersToRemove = new[] { "$", "," };
            var cleanValue = value;

            foreach (var characterToRemove in charactersToRemove)
                cleanValue = cleanValue.Replace(characterToRemove, string.Empty);

            return decimal.Parse(cleanValue);
        }
    }
}
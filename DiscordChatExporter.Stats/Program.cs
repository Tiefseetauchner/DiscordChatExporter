using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Exporting.Writers;

namespace DiscordChatExporter.Stats
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var arguments = new Arguments(args);
            var statsDateCsv = args[0] + " stats_date.csv";
            var statsTimeCsv = args[0] + " stats_time.csv";
            var dateStats = new LongCounter<string>();
            var timeStats = new LongCounter<int>();

            var client = new DiscordClient(new AuthToken(AuthTokenType.User,
                args[1]));

            var messages = client.GetMessagesAsync(args[0]);

            await foreach (var message in messages)
            {
                var messageDate = message.Timestamp.ToString().Substring(0, 10);
                var messageHour = int.Parse(message.Timestamp.ToString().Substring(11, 2));
                dateStats.Add(messageDate);
                timeStats.Add(messageHour);
            }


            await using (File.Create(statsDateCsv))
            {
            }

            await using (File.Create(statsTimeCsv))
            {
            }
            
            var statsDateWriter = new StreamWriter(statsDateCsv);
            foreach (var (key, value) in dateStats)
            {
                await statsDateWriter.WriteLineAsync(key + "," + value);
            }
            
            await statsDateWriter.FlushAsync();
            
            var statsTimeWriter = new StreamWriter(statsTimeCsv);
            foreach (var (key, value) in timeStats)
            {
                await statsTimeWriter.WriteLineAsync(key + "," + value);
            }
            
            await statsTimeWriter.FlushAsync();
        }
    }
}
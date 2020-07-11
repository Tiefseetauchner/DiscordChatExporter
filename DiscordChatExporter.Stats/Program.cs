using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DiscordChatExporter.Domain.Discord;

namespace DiscordChatExporter.Stats
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length != 4)
            {
                throw new ArgumentException("Not enough arguments");
            }
            
            var channelId = args[0];
            var authToken = new AuthToken(AuthTokenType.User, args[1]);
            var templatePath = args[2];
            var statsXlsxPath = args[3];
            
            var dateStats = new LongCounter<string>();
            var timeStats = new LongCounter<int>();
            
            var client = new DiscordClient(authToken);

            var messages = client.GetMessagesAsync(channelId);

            await foreach (var message in messages)
            {
                var messageDate = message.Timestamp.ToString().Substring(0, 10);
                var messageHour = int.Parse(message.Timestamp.ToString().Substring(11, 2));
                dateStats.Add(messageDate);
                timeStats.Add(messageHour);
            }

            using var workbook = new XLWorkbook(templatePath);

            var worksheets = workbook.Worksheets;
            var dateStatSheet = worksheets.Worksheet("DateStats");
            var timeStatSheet = worksheets.Worksheet("TimeStats");

            var row = 1;
            foreach (var (key, value) in dateStats)
            {
                row++;
                dateStatSheet.Cell(row, 1).Value = key;
                dateStatSheet.Cell(row, 2).Value = value.ToString();
            }

            foreach (var (key, value) in timeStats)
            {
                timeStatSheet.Cell(key + 2, 1).Value = key.ToString();
                timeStatSheet.Cell(key + 2, 2).Value = value.ToString();
            }

            workbook.SaveAs(statsXlsxPath);
        }
    }
}
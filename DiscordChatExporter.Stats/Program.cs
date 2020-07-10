using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DiscordChatExporter.Domain.Discord;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml.Core.ExcelPackage;

namespace DiscordChatExporter.Stats
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new ArgumentException("Not enough arguments");
            }

            var statsXlsxPath = args[0] + "_stats.xlsx";
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

            using var workbook = new XLWorkbook();

            var worksheets = workbook.Worksheets;
            var dateStatSheet = worksheets.Add("DateStats");
            var timeStatSheet = worksheets.Add("TimeStats");

            var row = 0;
            foreach (var (key, value) in dateStats)
            {
                row++;
                dateStatSheet.Cell(row, 1).Value = key;
                dateStatSheet.Cell(row, 2).Value = value.ToString();
            }

            row = 0;
            foreach (var (key, value) in timeStats)
            {
                row++;
                timeStatSheet.Cell(row, 1).Value = key.ToString();
                timeStatSheet.Cell(row, 2).Value = value.ToString();
            }

            workbook.SaveAs(statsXlsxPath);
        }
    }
}
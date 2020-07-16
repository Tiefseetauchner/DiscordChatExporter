using System;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DiscordChatExporter.Domain.Discord;
using PowerArgs;

namespace DiscordChatExporter.Stats
{
  internal static class Program
  {
    public static async Task Main(string[] args)
    {
      var arguments = Args.Parse<StatisticArguments>(args);
      
      var channelId = arguments.ChannelId;
      var authToken = new AuthToken(AuthTokenType.User, arguments.AuthToken);
      var templatePath = arguments.TemplatePath;
      var statsXlsxPath = arguments.ExportPath;

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

  [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
  public class StatisticArguments
  {
    [ArgRequired(PromptIfMissing = true)] public string ChannelId { get; set; }
    [ArgRequired(PromptIfMissing = true)] public string AuthToken { get; set; }
    [ArgRequired(PromptIfMissing = true)] public string TemplatePath { get; set; }
    [ArgRequired(PromptIfMissing = true)] public string ExportPath { get; set; }
  }
}
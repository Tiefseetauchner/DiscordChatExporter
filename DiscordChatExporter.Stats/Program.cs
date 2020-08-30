using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DiscordChatExporter.Domain.Discord;
using DiscordChatExporter.Domain.Discord.Models;
using PowerArgs;
using Tyrrrz.Extensions;

namespace DiscordChatExporter.Stats
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var arguments = Args.Parse<StatisticArguments>(args);

            var channelId = arguments.ChannelId;
            var authToken = new AuthToken(AuthTokenType.User, arguments.AuthToken);
            var templatePath = arguments.TemplatePath;
            var statsXlsxPath = arguments.ExportPath;
            var wordsToKeepTrackOf = arguments.Words;

            var dateStats = new LongCounter<string>();
            var timeStats = new LongCounter<int>();
            var weekdayStats = new LongCounter<int>();
            var characterStats = new LongCounter<char>();
            var wordStats = new LongCounter<string>();
            var userStats = new LongCounter<string>();
            var customWordStats = new Dictionary<string, LongCounter<string>>();

            var client = new DiscordClient(authToken);

            var channel = client.GetChannelAsync(channelId).Result;
            var messages = client.GetMessagesAsync(channelId);
            var totalMessageCount = 0;
            long totalCharacterCount = 0;
            Message firstMessage = null;
            Message lastMessage = null;

            await foreach (var message in messages)
            {
                totalMessageCount++;
                totalCharacterCount += message.Content.Length;

                var messageDate = message.Timestamp.ToString().Substring(0, 10);
                var messageHour = int.Parse(message.Timestamp.ToString().Substring(11, 2));
                var messageWeekday = (int) message.Timestamp.DayOfWeek - 1 >= 0
                    ? (int) message.Timestamp.DayOfWeek - 1
                    : 6;
                var author = message.Author.Name;

                dateStats.Add(messageDate);
                timeStats.Add(messageHour);
                weekdayStats.Add(messageWeekday);
                userStats.Add(author);

                foreach (var character in message.Content.Replace(" ", ""))
                {
                    characterStats.Add(character);
                }

                foreach (var word in message.Content.Split(new[] {" ", "\n", "\r"},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    wordStats.Add(word.ToLower());
                }

                foreach (var word in wordsToKeepTrackOf)
                {
                    var count = Regex.Matches(message.Content.ToLower(), word.ToLower()).Count;
                    if (count > 0)
                    {
                        if (!customWordStats.ContainsKey(word))
                        {
                            customWordStats.Add(word, new LongCounter<string>());
                        }

                        customWordStats[word].Add(messageDate, count);
                    }
                }

                if (firstMessage == null || firstMessage.Timestamp > message.Timestamp)
                {
                    firstMessage = message;
                }

                if (lastMessage == null || lastMessage.Timestamp < message.Timestamp)
                {
                    lastMessage = message;
                }
            }

            using var workbook = new XLWorkbook(templatePath);

            var worksheets = workbook.Worksheets;
            var welcomeSheet = worksheets.Worksheet("WelcomeSheet");
            var dateStatSheet = worksheets.Worksheet("DateStats");
            var timeStatSheet = worksheets.Worksheet("TimeStats");
            var weekdayStatSheet = worksheets.Worksheet("WeekdayStats");
            var charStatSheet = worksheets.Worksheet("CharacterStats");
            var wordStatSheet = worksheets.Worksheet("WordStats");

            var row = 1;
            foreach (var word in wordsToKeepTrackOf)
            {
                row = 1;
                var customWordStatSheet = dateStatSheet.CopyTo(word + "Stats");
                if (customWordStats.ContainsKey(word))
                {
                    foreach (var (key, value) in customWordStats[word])
                    {
                        row++;

                        customWordStatSheet.Cell(row, 1).Value = key;
                        customWordStatSheet.Cell(row, 2).Value = value.ToString();
                    }
                }
            }

            welcomeSheet.Cell(2, 2).Value = channel.Name;
            welcomeSheet.Cell(2, 4).Value = "\"" + channel.Id + "\"";

            welcomeSheet.Cell(3, 2).Value = totalMessageCount;

            if (firstMessage != null)
            {
                welcomeSheet.Cell(4, 2).Value = firstMessage.Timestamp.ToString().Substring(0, 10);
                welcomeSheet.Cell(4, 4).Value = firstMessage.Author.ToString();
            }

            if (lastMessage != null)
            {
                welcomeSheet.Cell(5, 2).Value = lastMessage.Timestamp.ToString().Substring(0, 10);
                welcomeSheet.Cell(5, 4).Value = lastMessage.Author.ToString();
            }

            welcomeSheet.Cell(6, 2).Value = totalMessageCount / dateStats.Count;

            welcomeSheet.Cell(7, 2).Value = dateStats.OrderByDescending(pair => pair.Value).First().Key;
            welcomeSheet.Cell(7, 4).Value = dateStats.OrderByDescending(pair => pair.Value).First().Value;

            welcomeSheet.Cell(8, 2).Value = totalCharacterCount / totalMessageCount;

            welcomeSheet.Cell(9, 2).Value = totalCharacterCount;

            welcomeSheet.Cell(10, 2).Value =
                "\"" + characterStats.OrderByDescending(pair => pair.Value).First().Key + "\"";
            welcomeSheet.Cell(10, 4).Value = characterStats.OrderByDescending(pair => pair.Value).First().Value;

            welcomeSheet.Cell(11, 2).Value = wordStats.OrderByDescending(pair => pair.Value).First().Key;
            welcomeSheet.Cell(11, 4).Value = wordStats.OrderByDescending(pair => pair.Value).First().Value;

            welcomeSheet.Cell(13, 2).Value = userStats.OrderByDescending(pair => pair.Value).First().Key;
            welcomeSheet.Cell(13, 4).Value = userStats.OrderByDescending(pair => pair.Value).First().Value;

            welcomeSheet.Cell(14, 2).Value = userStats.Count;

            row = 1;
            foreach (var (key, value) in dateStats)
            {
                row++;
                dateStatSheet.Cell(row, 1).Value = key;
                dateStatSheet.Cell(row, 2).Value = value.ToString();
            }

            foreach (var (key, value) in timeStats)
            {
                timeStatSheet.Cell(key + 2, 2).Value = value.ToString();
            }

            foreach (var (key, value) in weekdayStats)
            {
                weekdayStatSheet.Cell(key + 2, 2).Value = value.ToString();
            }

            row = 1;
            foreach (var (key, value) in characterStats.OrderByDescending(pair => pair.Value))
            {
                row++;
                charStatSheet.Cell(row, 1).Value = key;
                charStatSheet.Cell(row, 2).Value = value.ToString();
            }

            row = 1;
            foreach (var (key, value) in wordStats.OrderByDescending(pair => pair.Value))
            {
                row++;
                wordStatSheet.Cell(row, 1).Value = key;
                wordStatSheet.Cell(row, 2).Value = value.ToString();
            }

            welcomeSheet.Cell(12, 2).Value = stopwatch.ElapsedMilliseconds / 1000.0;

            workbook.SaveAs(statsXlsxPath + "\\export_" + channel.Name + ".xlsx");
        }
    }

    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class StatisticArguments
    {
        [ArgRequired(PromptIfMissing = true)] public string ChannelId { get; set; }
        [ArgRequired(PromptIfMissing = true)] public string AuthToken { get; set; }
        [ArgRequired(PromptIfMissing = true)] public string TemplatePath { get; set; }
        [ArgRequired(PromptIfMissing = true)] public string ExportPath { get; set; }

        [ArgDescription("Some funny business, don't @ me")]
        public List<string> Words { get; set; }
    }
}
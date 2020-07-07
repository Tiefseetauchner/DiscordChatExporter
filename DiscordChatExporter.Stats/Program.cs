using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using DiscordChatExporter.Domain.Exporting.Writers;

namespace DiscordChatExporter.Stats
{
    class Program
    {
        static void Main(string[] args)
        {
            var csvPath = "/home/tauchner/Documents/Direct Messages - Private - Annakamisama [535518065610457118].csv";
            var statsDateCsv = csvPath + " stats_date.csv";
            var statsTimeCsv = csvPath + " stats_time.csv";
            var dateStats = new LongCounter<string>();
            var timeStats = new LongCounter<int>();

            var messageWriter = new CsvMessageWriter();

            using (var reader = new StreamReader(csvPath))
            {
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<Message>();

                foreach (var record in records)
                {
                    dateStats.Add(record.Date.ToShortDateString());
                    timeStats.Add(record.Date.Hour);

                    if (record.Date.Hour == 4)
                    {
                        Console.Out.WriteLine(record);
                    }
                }
            }

            using (File.Create(statsDateCsv))
            {
            }

            using (File.Create(statsTimeCsv))
            {
            }

            var statsDateWriter = new StreamWriter(statsDateCsv);
            foreach (var dateStat in dateStats)
            {
                statsDateWriter.WriteLine(dateStat.Key + "," + dateStat.Value);
            }
            statsDateWriter.Flush();
            
            var statsTimeWriter = new StreamWriter(statsTimeCsv);
            foreach (var timeStat in timeStats)
            {
                statsTimeWriter.WriteLine(timeStat.Key + "," + timeStat.Value);
            }
            statsTimeWriter.Flush();
        }
    }

    class LongCounter<K> : Dictionary<K, long>
    {
        public void Add(K key)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, 1);
            }
            else
            {
                this[key] += 1;
            }
        }


        public void Add(K key, long value)
        {
            if (!ContainsKey(key))
            {
                base.Add(key, value);
            }
            else
            {
                this[key] += value;
            }
        }
    }

    class Message
    {
        public string AuthorID { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string Attachments { get; set; }
        public string Reactions { get; set; }

        public Message(string authorId, string author, DateTime date, string content, string attachments,
            string reactions)
        {
            AuthorID = authorId;
            this.Author = author;
            this.Date = date;
            this.Content = content;
            this.Attachments = attachments;
            this.Reactions = reactions;
        }

        public Message()
        {
        }

        public override string ToString()
        {
            return Author + " wrote on " + Date.ToShortDateString() + ":\r\n" + Content;
        }
    }
}
using System;
using System.Collections.Generic;

namespace DiscordChatExporter.Stats
{
    internal class Arguments
    {
        public static HashSet<Argument> Arguments1 { get; } = new HashSet<Argument>
        {
            new Argument("channelID")
        };

        public Arguments(string[] args)
        {
            if (args.Length != 2)
            {
                
            }
            // ArgumentsValidator.ValidateChannelID();
        }
    }

    internal class Argument
    {
        private readonly string _argument;
        private readonly string _description;
        private readonly string _helpText;

        public Argument(string argument, string description, string helpText)
        {
            _argument = argument;
            _description = description;
            _helpText = helpText;
        }
    }

    internal class ArgumentsValidator
    {
    }
}
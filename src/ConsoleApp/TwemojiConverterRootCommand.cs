using System.CommandLine;
using TwemojiConverter.ConsoleApp.Commands;

namespace TwemojiConverter.ConsoleApp;

public class TwemojiConverterRootCommand : CliRootCommand
{
    public TwemojiConverterRootCommand() : base("Twemoji Converter")
    {
        Description = "Convert Twemoji SVG files to PNG files.";

        Add(new ConvertCommand());
    }
}
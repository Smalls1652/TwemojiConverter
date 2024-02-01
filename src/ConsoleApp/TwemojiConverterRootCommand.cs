using System.CommandLine;
using TwemojiConverter.ConsoleApp.Commands;

namespace TwemojiConverter.ConsoleApp;

/// <summary>
/// The root command for the Twemoji Converter CLI.
/// </summary>
public class TwemojiConverterRootCommand : CliRootCommand
{
    public TwemojiConverterRootCommand() : base("Twemoji Converter")
    {
        Description = "Convert Twemoji SVG files to PNG files.";

        Add(new ConvertCommand());
        Add(new InfoCommand());
    }
}
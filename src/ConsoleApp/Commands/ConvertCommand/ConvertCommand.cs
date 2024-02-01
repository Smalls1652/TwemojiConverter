using System.CommandLine;

namespace TwemojiConverter.ConsoleApp.Commands;

/// <summary>
/// CLI command for running the conversion process.
/// </summary>
public class ConvertCommand : CliCommand
{
    public ConvertCommand() : base("convert")
    {
        Description = "Runs the conversion process.";

        Options.Add(
            new CliOption<string>("--svg-directory")
            {
                Description = "The path to the directory containing the Twemoji SVG files.",
                Required = true
            }
        );

        Options.Add(
            new CliOption<string>("--output-directory")
            {
                Description = "The path to where the converted images will be saved.",
                Required = true
            }
        );

        Options.Add(
            new CliOption<int>("--output-resolution")
            {
                Description = "The output resolution for the converted images.",
                DefaultValueFactory = (argResult) => 512
            }
        );

        Action = new ConvertCommandAction();
    }
}
using System.CommandLine;

namespace TwemojiConverter.ConsoleApp.Commands;

/// <summary>
/// CLI command for getting emoji information from Unicode.
/// </summary>
public class InfoCommand : CliCommand
{
    public InfoCommand() : base("get-info")
    {
        Description = "Gets emoji information from Unicode.";

        Options.Add(
            new CliOption<string>("--output-path")
            {
                Description = "The path to output the JSON file to.",
                Required = false
            }
        );

        Options.Add(
            new CliOption<string>("--image-directory-path")
            {
                Description = "The path to the directory containing the emoji images.",
                Required = false
            }
        );

        Action = new InfoCommandAction();
    }
}
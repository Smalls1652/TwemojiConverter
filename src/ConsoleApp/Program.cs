using System.CommandLine;
using TwemojiConverter.ConsoleApp;
using TwemojiConverter.ConsoleApp.Utilities;

using CancellationTokenSource cancellationTokenSource = new();

TwemojiConverterRootCommand rootCommand = new TwemojiConverterRootCommand();

CliConfiguration cliConfig = new(rootCommand);

try
{
    return await cliConfig.InvokeAsync(
        args: args,
        cancellationToken: cancellationTokenSource.Token
    );
}
catch (Exception)
{
    throw;
}

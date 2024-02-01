using System.CommandLine;
using TwemojiConverter.ConsoleApp;

using CancellationTokenSource cancellationTokenSource = new();

TwemojiConverterRootCommand rootCommand = new();

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
using System.CommandLine;
using System.CommandLine.Invocation;
using ImageMagick;
using TwemojiConverter.ConsoleApp.Utilities;

namespace TwemojiConverter.ConsoleApp.Commands;

/// <summary>
/// CLI command action for running the conversion process.
/// </summary>
/// <remarks>
/// This is used by <see cref="ConvertCommand"/>.
/// </remarks>
public class ConvertCommandAction : AsynchronousCliAction
{
    /// <summary>
    /// Invokes the action for the 'convert' command.
    /// </summary>
    /// <param name="parseResult">The <see cref="ParseResult"/> for the command.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the command.</param>
    /// <returns>The exit code for the command.</returns>
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        string svgDirectoryPath;
        string outputDirectoryPath;
        int outputResolution;

        try
        {
            svgDirectoryPath = ParseSvgDirectoryArgument(parseResult);
            outputDirectoryPath = ParseOutputDirectoryArgument(parseResult);
            outputResolution = ParseOutputResolutionArgument(parseResult);
        }
        catch (ArgumentNullException ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        string resolvedSvgDirectoryPath = Path.GetFullPath(svgDirectoryPath);

        if (!Directory.Exists(resolvedSvgDirectoryPath))
        {
            ConsoleUtils.WriteError($"The directory '{resolvedSvgDirectoryPath}' does not exist.");
            return 1;
        }

        string[] svgFiles;
        try
        {
            svgFiles = GetSvgFiles(resolvedSvgDirectoryPath);
        }
        catch (Exception ex)
        {
            ConsoleUtils.WriteError(ex.Message);
            return 1;
        }

        string resolvedOutputDirectoryPath = Path.GetFullPath(outputDirectoryPath);

        if (Directory.Exists(resolvedOutputDirectoryPath))
        {
            ConsoleUtils.WriteWarning($"The directory '{resolvedOutputDirectoryPath}' already exists.");

            Console.Write("Do you want to overwrite the existing files? (y/N) ");
            string? overwriteResponse = Console.ReadLine();

            if (overwriteResponse?.ToLower() != "y")
            {
                ConsoleUtils.WriteError("Conversion process aborted.");
                return 10;
            }

            ConsoleUtils.WriteWarning("Clearing the output directory...");
            Directory.Delete(
                path: resolvedOutputDirectoryPath,
                recursive: true
            );
        }

        ConsoleUtils.WriteInfo("Creating the output directory...");
        Directory.CreateDirectory(resolvedOutputDirectoryPath);

        MagickNET.Initialize();

        ImageOptimizer imageOptimizer = new()
        {
            OptimalCompression = true
        };

        using SemaphoreSlim semaphoreSlim = new(
            initialCount: 2,
            maxCount: 5
        );

        Task<bool>[] optimizeTasks = new Task<bool>[svgFiles.Length];
        try
        {
            for (int i = 0; i < svgFiles.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                string convertedImagePath = Path.Combine(
                    path1: outputDirectoryPath,
                    path2: $"{Path.GetFileNameWithoutExtension(svgFiles[i])}.png"
                );

                using MagickImage image = new();

                image.Format = MagickFormat.Svg;
                image.BackgroundColor = MagickColors.None;

                image.Read(svgFiles[i], outputResolution, outputResolution);
                image.Density = new Density(10, DensityUnit.PixelsPerInch);

                image.Settings.Compression = CompressionMethod.LZMA;

                image.Settings.AntiAlias = true;
                image.Settings.TextAntiAlias = true;

                ConsoleUtils.WriteOutput($"[{i + 1}/{svgFiles.Length}] {Path.GetFileName(svgFiles[i])} -> {Path.GetRelativePath(Environment.CurrentDirectory, convertedImagePath)}");
                image.Write(convertedImagePath, MagickFormat.Png8);

                optimizeTasks[i] = OptimizeImageAsync(
                    semaphoreSlim: semaphoreSlim,
                    imageOptimizer: imageOptimizer,
                    imagePath: convertedImagePath,
                    cancellationToken: cancellationToken
                );
            }

            ConsoleUtils.WriteOutput("Waiting for image optimization tasks to complete... 👀");
            await Task.WhenAll(optimizeTasks);
        }
        catch (OperationCanceledException)
        {
            ConsoleUtils.WriteError("\nOperation was canceled. 🤯");
            return 1;
        }
        catch (Exception)
        {
            ConsoleUtils.WriteError("\nAn unknown error occurred. 😢");
            throw;
        }

        ConsoleUtils.WriteOutput("\nConversion process completed successfully. 🎉");

        return 0;
    }

    private static string ParseSvgDirectoryArgument(ParseResult parseResult)
    {
        string? svgDirectoryPath = parseResult.GetValue<string>("--svg-directory");

        if (svgDirectoryPath is null || string.IsNullOrEmpty(svgDirectoryPath))
        {
            throw new ArgumentNullException("'--svg-directory' argument was empty.");
        }

        return svgDirectoryPath;
    }

    private static string ParseOutputDirectoryArgument(ParseResult parseResult)
    {
        string? outputDirectoryPath = parseResult.GetValue<string>("--output-directory");

        if (outputDirectoryPath is null || string.IsNullOrEmpty(outputDirectoryPath))
        {
            throw new ArgumentNullException("'--output-directory' argument was empty.");
        }

        return outputDirectoryPath;
    }

    private static int ParseOutputResolutionArgument(ParseResult parseResult)
    {
        int outputResolution = parseResult.GetValue<int>("--output-resolution");

        return outputResolution;
    }

    private static string[] GetSvgFiles(string directoryPath)
    {
        string[] svgFiles = Directory.GetFiles(
            path: directoryPath,
            searchPattern: "*.svg"
        );

        if (svgFiles.Length == 0)
        {
            throw new Exception($"No SVG files were found in the directory '{directoryPath}'.");
        }

        return svgFiles;
    }

    private static Task<bool> OptimizeImageAsync(SemaphoreSlim semaphoreSlim, ImageOptimizer imageOptimizer, string imagePath, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                semaphoreSlim.Wait(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                return imageOptimizer.Compress(imagePath);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        });
    }
}
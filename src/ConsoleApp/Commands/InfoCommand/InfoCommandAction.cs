using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TwemojiConverter.ConsoleApp.Models;
using TwemojiConverter.ConsoleApp.Utilities;

namespace TwemojiConverter.ConsoleApp.Commands;

/// <summary>
/// CLI command action for getting emoji information from Unicode.
/// </summary>
/// <remarks>
/// This is used by <see cref="InfoCommand"/>.
/// </remarks>
public class InfoCommandAction : AsynchronousCliAction, IDisposable
{
    private bool _disposedValue;
    private readonly HttpClient _httpClient;

    public InfoCommandAction()
    {
        HttpMessageHandler httpMessageHandler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2)
        };

        _httpClient = new(
            handler: httpMessageHandler,
            disposeHandler: true
        );

        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue(
                productName: "TwemojiConverter",
                productVersion: null
            )
        );
    }

    /// <summary>
    /// Invokes the action for the 'info' command.
    /// </summary>
    /// <param name="parseResult">The <see cref="ParseResult"/> for the command.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the command.</param>
    /// <returns>The exit code for the command.</returns>
    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        // Get the '--output-path' and '--image-directory-path' options
        // from the parse result.
        string? outputPath = ParseOutputPath(parseResult);
        string? imageDirectoryPath = ParseImageDirectoryPath(parseResult);

        // Resolve the output path to an absolute path.
        string? resolvedOutputPath = null;
        if (outputPath is not null && !string.IsNullOrEmpty(outputPath))
        {
            resolvedOutputPath = Path.GetFullPath(outputPath);
        }

        // If the image directory path is specified, but the output path is not,
        // write an error message and return an exit code of 1.
        if (imageDirectoryPath is not null && outputPath is null)
        {
            ConsoleUtils.WriteError("The '--image-directory-path' option requires the '--output-path' option to be specified.");
            return 1;
        }

        // Get the emoji list from the Unicode website.
        string responseContent = await GetUnicodeEmojiListAsync(cancellationToken);
        string[] lines = responseContent.Split('\n');

        // Get the JoyPixel emoji data from the GitHub repository.
        JsonDocument joyPixelEmojiData = await GetJoyPixelEmojiDataAsync(cancellationToken);

        // Parse the emoji list and get the emoji information.
        List<EmojiItem> emojiItems = [];
        foreach (string line in lines)
        {
            try
            {
                EmojiItem emojiItem = new(line);

                // If the image directory path is specified, resolve the image path
                // and set it on the emoji item.
                if (imageDirectoryPath is not null && resolvedOutputPath is not null)
                {
                    string imagePath = Path.Combine(
                        path1: imageDirectoryPath,
                        path2: $"{string.Join('-', emojiItem.CodePoints).ToLower()}.png"
                    );

                    string imagePathRelative = Path.GetRelativePath(
                        relativeTo: Path.GetDirectoryName(resolvedOutputPath)!,
                        path: imagePath
                    );

                    emojiItem.ImagePath = File.Exists(imagePath) ? imagePathRelative : null;
                }

                // Attempt to parse the shortnames from the JoyPixel emoji data.
                try
                {
                    string[] codepoints = Array.FindAll(emojiItem.CodePoints, codepoint => codepoint != "200D" && codepoint != "FE0F");

                    Tuple<string, string[]> shortnames = ParseShortnamesFromJoyPixelData(
                        jsonDocument: joyPixelEmojiData,
                        codepoint: string.Join('-', codepoints).ToLower()
                    );

                    emojiItem.Shortname = shortnames.Item1;
                    emojiItem.ShortnameAlternatives = shortnames.Item2;
                }
                catch (InvalidOperationException)
                {
                    ConsoleUtils.WriteWarning($"[E{emojiItem.EmojiVersion} - {emojiItem.Name}] No shortname found for emoji.");
                }
                catch (Exception ex)
                {
                    ConsoleUtils.WriteError(ex.Message);
                }
                
                emojiItems.Add(emojiItem);
            }
            catch (ArgumentException)
            {
                continue;
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteError(ex.Message);
                return 1;
            }
        }

        int emojiCount = emojiItems.Count;
        int fullyQualifiedEmojiCount = emojiItems.Count(item => item.Status == "fully-qualified");
        int minimallyQualifiedEmojiCount = emojiItems.Count(item => item.Status == "minimally-qualified");
        int unqualifiedEmojiCount = emojiItems.Count(item => item.Status == "unqualified");
        int componentEmojiCount = emojiItems.Count(item => item.Status == "component");

        StringBuilder stringBuilder = new();

        stringBuilder.AppendLine($"Emoji count: {emojiCount}");
        stringBuilder.AppendLine($"Fully-qualified emoji count: {fullyQualifiedEmojiCount}");
        stringBuilder.AppendLine($"Minimally-qualified emoji count: {minimallyQualifiedEmojiCount}");
        stringBuilder.AppendLine($"Unqualified emoji count: {unqualifiedEmojiCount}");
        stringBuilder.AppendLine($"Component emoji count: {componentEmojiCount}");

        ConsoleUtils.WriteInfo(stringBuilder.ToString());

        // If the output path is specified, write the emoji information to a JSON file.
        if (resolvedOutputPath is not null)
        {
            string outputJson = JsonSerializer.Serialize(
                value: emojiItems,
                jsonTypeInfo: CoreJsonContext.Default.ListEmojiItem
            );

            try
            {
                await File.WriteAllTextAsync(
                    path: resolvedOutputPath,
                    contents: outputJson,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteError(ex.Message);
                return 1;
            }

            ConsoleUtils.WriteSuccess($"Emoji information written successfully to '{Path.GetRelativePath(Environment.CurrentDirectory, resolvedOutputPath)}'.");
        }

        return 0;
    }

    /// <summary>
    /// Parses the output path from the <see cref="ParseResult"/>.
    /// </summary>
    /// <param name="parseResult">The <see cref="ParseResult"/> for the command.</param>
    /// <returns>The supplied output path.</returns>
    private static string? ParseOutputPath(ParseResult parseResult)
    {
        string? outputPath = parseResult.GetValue<string>("--output-path");

        return outputPath;
    }

    /// <summary>
    /// Parses the image directory path from the <see cref="ParseResult"/>.
    /// </summary>
    /// <param name="parseResult">The <see cref="ParseResult"/> for the command.</param>
    /// <returns>The supplied image directory path.</returns>
    private static string? ParseImageDirectoryPath(ParseResult parseResult)
    {
        string? imageDirectoryPath = parseResult.GetValue<string>("--image-directory-path");

        return imageDirectoryPath;
    }

    /// <summary>
    /// Get the Unicode emoji list from the Unicode website.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
    /// <returns>The Unicode emoji list.</returns>
    /// <exception cref="HttpRequestException">The request to the Unicode website failed.</exception>
    private async Task<string> GetUnicodeEmojiListAsync(CancellationToken cancellationToken = default)
    {
        Uri emojiListUri = new("https://www.unicode.org/Public/emoji/15.1/emoji-test.txt");

        HttpRequestMessage requestMessage = new(
            method: HttpMethod.Get,
            requestUri: emojiListUri
        );

        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"The request to '{requestMessage.RequestUri}' failed with status code {responseMessage.StatusCode}.");
        }

        string responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return responseContent;
    }

    /// <summary>
    /// Get the JoyPixel emoji data from the GitHub repository.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
    /// <returns>The <see cref="JsonDocument"/> containing the JoyPixel emoji data.</returns>
    /// <exception cref="HttpRequestException">The request to the GitHub repository failed.</exception>
    private async Task<JsonDocument> GetJoyPixelEmojiDataAsync(CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage requestMessage = new(
            method: HttpMethod.Get,
            requestUri: new Uri("https://raw.githubusercontent.com/joypixels/emoji-toolkit/master/emoji.json")
        );

        HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!responseMessage.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"The request to '{requestMessage.RequestUri}' failed with status code {responseMessage.StatusCode}.");
        }

        string responseContent = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        JsonDocument jsonDocument = JsonDocument.Parse(
            json: responseContent,
            options: new JsonDocumentOptions()
            {
                MaxDepth = 3
            }
        );

        return jsonDocument;
    }

    /// <summary>
    /// Parses the shortnames from the JoyPixel emoji data.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> containing the JoyPixel emoji data.</param>
    /// <param name="codepoint">The codepoint of the emoji.</param>
    /// <returns>A <see cref="Tuple{T1, T2}"/> containing the shortname and shortname alternates.</returns>
    /// <exception cref="InvalidOperationException">The codepoint was not found in the JoyPixel emoji data.</exception>
    private static Tuple<string, string[]> ParseShortnamesFromJoyPixelData(JsonDocument jsonDocument, string codepoint)
    {
        JsonElement codepointElement;
        try
        {
            codepointElement = jsonDocument.RootElement.GetProperty(codepoint);
        }
        catch (KeyNotFoundException)
        {
            throw new InvalidOperationException($"The codepoint '{codepoint}' was not found in the JoyPixel emoji data.");
        }

        JoyPixelEmojiData item = JsonSerializer.Deserialize(
            json: codepointElement.GetRawText(),
            jsonTypeInfo: CoreJsonContext.Default.JoyPixelEmojiData
        ) ?? throw new InvalidOperationException("The deserialized object is null.");

        return new Tuple<string, string[]>(item.Shortname, item.ShortnameAlternates);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposedValue, this);

        _httpClient.Dispose();

        _disposedValue = true;

        GC.SuppressFinalize(this);
    }
}
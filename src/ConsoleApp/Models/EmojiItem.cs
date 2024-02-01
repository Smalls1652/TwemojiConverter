using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TwemojiConverter.ConsoleApp.Models;

/// <summary>
/// Represents an emoji item.
/// </summary>
public partial class EmojiItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmojiItem"/> class.
    /// </summary>
    [JsonConstructor]
    public EmojiItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmojiItem"/> class.
    /// </summary>
    /// <param name="inputText">The input line from the Unicode emoji list.</param>
    public EmojiItem(string inputText)
    {
        var parsedInput = ParseInputText(inputText);

        CodePoints = GetParsedCodePoints(parsedInput);
        Status = GetParsedStatus(parsedInput);
        Emoji = GetParsedEmoji(parsedInput);
        EmojiVersion = GetParsedEmojiVersion(parsedInput);
        Name = GetParsedName(parsedInput);
    }

    /// <summary>
    /// The path to the emoji image.
    /// </summary>
    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }

    /// <summary>
    /// The code points used to represent the emoji.
    /// </summary>
    [JsonPropertyName("codePoints")]
    public string[] CodePoints { get; set; } = [];

    /// <summary>
    /// The status of the emoji.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// The representation of the emoji.
    /// </summary>
    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = null!;

    /// <summary>
    /// The version the emoji was added.
    /// </summary>
    [JsonPropertyName("emojiVersion")]
    public string EmojiVersion { get; set; } = null!;

    /// <summary>
    /// The name of the emoji.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The short name of the emoji.
    /// </summary>
    [JsonPropertyName("shortname")]
    public string? Shortname { get; set; }

    /// <summary>
    /// Alternative short names for the emoji.
    /// </summary>
    [JsonPropertyName("shortnameAlternatives")]
    public string[] ShortnameAlternatives { get; set; } = [];

    /// <summary>
    /// Parses the input text to get information about the emoji.
    /// </summary>
    /// <param name="inputText">The input line from the Unicode emoji list.</param>
    /// <returns>A <see cref="Match"/> object containing the parsed input.</returns>
    /// <exception cref="ArgumentException">Thrown when the input text is not a valid emoji item.</exception>
    private Match ParseInputText(string inputText)
    {
        Match match = EmojiItemRegex().Match(inputText);

        if (!match.Success)
        {
            throw new ArgumentException("The input text is not a valid emoji item.", nameof(inputText));
        }

        return match;
    }

    /// <summary>
    /// Gets the parsed code points from the input text.
    /// </summary>
    /// <param name="parsedInput">The parsed input text.</param>
    /// <returns>An array of code points.</returns>
    private static string[] GetParsedCodePoints(Match parsedInput)
    {
        string codepoint = parsedInput.Groups["codepoint"].Value;

        string[] codepoints = codepoint.Split(' ');

        return codepoints;
    }

    /// <summary>
    /// Gets the parsed status from the input text.
    /// </summary>
    /// <param name="parsedInput">The parsed input text.</param>
    /// <returns>The status of the emoji.</returns>
    private static string GetParsedStatus(Match parsedInput)
    {
        string status = parsedInput.Groups["status"].Value;

        return status;
    }

    /// <summary>
    /// Gets the parsed emoji from the input text.
    /// </summary>
    /// <param name="parsedInput">The parsed input text.</param>
    /// <returns>The representation of the emoji.</returns>
    private static string GetParsedEmoji(Match parsedInput)
    {
        string emoji = parsedInput.Groups["emoji"].Value;

        return emoji;
    }

    /// <summary>
    /// Gets the parsed emoji version from the input text.
    /// </summary>
    /// <param name="parsedInput">The parsed input text.</param>
    /// <returns>The version the emoji was added.</returns>
    private static string GetParsedEmojiVersion(Match parsedInput)
    {
        string emojiVersion = parsedInput.Groups["emojiVersion"].Value;

        return emojiVersion;
    }

    /// <summary>
    /// Gets the parsed name from the input text.
    /// </summary>
    /// <param name="parsedInput">The parsed input text.</param>
    /// <returns>The name of the emoji.</returns>
    private static string GetParsedName(Match parsedInput)
    {
        string name = parsedInput.Groups["name"].Value;

        return name;
    }

    [GeneratedRegex(
        pattern: @"^(?'codepoint'.+?)\s+;\s(?'status'.+?)\s+#\s(?'emoji'.+?)\s(?'emojiVersionFull'[Ee](?'emojiVersion'\d{1,}\.\d{1,}))\s(?'name'.+?)$",
        options: RegexOptions.Multiline
    )]
    private static partial Regex EmojiItemRegex();
}
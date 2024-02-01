using System.Text.Json.Serialization;

namespace TwemojiConverter.ConsoleApp.Models;

/// <summary>
/// Data for a JoyPixel emoji.
/// </summary>
public record JoyPixelEmojiData
{
    /// <summary>
    /// The short name of the emoji.
    /// </summary>
    [JsonPropertyName("shortname")]
    public string Shortname { get; init; } = null!;

    /// <summary>
    /// Alternative short names for the emoji.
    /// </summary>
    [JsonPropertyName("shortname_alternates")]
    public string[] ShortnameAlternates { get; init; } = [];
}
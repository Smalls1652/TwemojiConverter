using System.Text.Json.Serialization;
using TwemojiConverter.ConsoleApp.Models;

namespace TwemojiConverter.ConsoleApp;

/// <summary>
/// Source generation context for JSON serialization/deserialization
/// of core data types.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never
)]
[JsonSerializable(typeof(EmojiItem))]
[JsonSerializable(typeof(EmojiItem[]))]
[JsonSerializable(typeof(List<EmojiItem>))]
[JsonSerializable(typeof(JoyPixelEmojiData))]
[JsonSerializable(typeof(JoyPixelEmojiData[]))]
[JsonSerializable(typeof(List<JoyPixelEmojiData>))]
internal partial class CoreJsonContext : JsonSerializerContext
{}
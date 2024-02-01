namespace TwemojiConverter.ConsoleApp.Utilities;

/// <summary>
/// Utilities for writing to the console.
/// </summary>
public static class ConsoleUtils
{
    /// <summary>
    /// Writes a message to the console.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public static void WriteOutput(string message)
    {
        Console.WriteLine(message);
    }

    /// <summary>
    /// Writes an error message to the console.
    /// </summary>
    /// <param name="message">The error message to write.</param>
    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes a warning message to the console.
    /// </summary>
    /// <param name="message">The warning message to write.</param>
    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes an informational message to the console.
    /// </summary>
    /// <param name="message">The informational message to write.</param>
    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Writes a success message to the console.
    /// </summary>
    /// <param name="message">The success message to write.</param>
    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
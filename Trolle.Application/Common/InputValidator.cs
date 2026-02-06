using System.Text.RegularExpressions;

namespace Trolle.Application.Common;

/// <summary>
/// Utility class for input validation and sanitization.
/// </summary>
public static class InputValidator
{
    private const int MAX_TITLE_LENGTH = 200;
    private const int MAX_DESCRIPTION_LENGTH = 5000;
    private const int MAX_COLOR_LENGTH = 50;
    private const int MAX_BULK_OPERATIONS = 100;

    private static readonly Regex ColorRegex = new(@"^(#[0-9A-Fa-f]{3,8}|rgb\(.*?\)|rgba\(.*?\)|hsl\(.*?\)|hsla\(.*?\)|transparent|[a-z]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex HtmlTagRegex = new(@"<[^>]*>", RegexOptions.Compiled);

    /// <summary>
    /// Validates and sanitizes a title input.
    /// </summary>
    /// <param name="title">The title to validate.</param>
    /// <param name="defaultValue">The default value if title is invalid.</param>
    /// <returns>A sanitized title.</returns>
    public static string ValidateTitle(string? title, string defaultValue = "Untitled")
    {
        if (string.IsNullOrWhiteSpace(title))
            return defaultValue;

        // Remove HTML tags
        var sanitized = HtmlTagRegex.Replace(title, string.Empty);

        // Trim and limit length
        sanitized = sanitized.Trim();
        if (sanitized.Length > MAX_TITLE_LENGTH)
            sanitized = sanitized.Substring(0, MAX_TITLE_LENGTH);

        return string.IsNullOrWhiteSpace(sanitized) ? defaultValue : sanitized;
    }

    /// <summary>
    /// Validates and sanitizes a description input.
    /// </summary>
    /// <param name="description">The description to validate.</param>
    /// <returns>A sanitized description.</returns>
    public static string ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        // Remove HTML tags
        var sanitized = HtmlTagRegex.Replace(description, string.Empty);

        // Trim and limit length
        sanitized = sanitized.Trim();
        if (sanitized.Length > MAX_DESCRIPTION_LENGTH)
            sanitized = sanitized.Substring(0, MAX_DESCRIPTION_LENGTH);

        return sanitized;
    }

    /// <summary>
    /// Validates a color value.
    /// </summary>
    /// <param name="color">The color to validate.</param>
    /// <param name="defaultValue">The default value if color is invalid.</param>
    /// <returns>A valid color value.</returns>
    public static string ValidateColor(string? color, string defaultValue = "#1e293b")
    {
        if (string.IsNullOrWhiteSpace(color))
            return defaultValue;

        var sanitized = color.Trim();

        // Limit length
        if (sanitized.Length > MAX_COLOR_LENGTH)
            return defaultValue;

        // Basic color format validation
        if (!ColorRegex.IsMatch(sanitized))
            return defaultValue;

        return sanitized;
    }

    /// <summary>
    /// Validates bulk operation count.
    /// </summary>
    /// <param name="count">The number of items in the bulk operation.</param>
    /// <exception cref="ArgumentException">Thrown when count exceeds maximum.</exception>
    public static void ValidateBulkOperationCount(int count)
    {
        if (count > MAX_BULK_OPERATIONS)
            throw new ArgumentException($"Bulk operations are limited to {MAX_BULK_OPERATIONS} items at a time.");
    }

    /// <summary>
    /// Validates an order value.
    /// </summary>
    /// <param name="order">The order value to validate.</param>
    /// <returns>A valid order value (non-negative).</returns>
    public static int ValidateOrder(int order)
    {
        return Math.Max(0, order);
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Trolle.Domain.Common;

/// <summary>
/// Represents a valid CSS color (Hex, RGB, HSL, or named).
/// </summary>
public class CssColor : ValueObject
{
    private static readonly Regex ColorRegex = new(@"^(#[0-9A-Fa-f]{3,8}|rgb\(.*?\)|rgba\(.*?\)|hsl\(.*?\)|hsla\(.*?\)|transparent|[a-z]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the CSS color value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CssColor"/> class.
    /// </summary>
    /// <param name="value">The CSS color string.</param>
    private CssColor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Color value cannot be empty");

        if (!ColorRegex.IsMatch(value))
            throw new ArgumentException($"Invalid color format: {value}");

        Value = value;
    }

    /// <summary>
    /// Gets the equality components for the color.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    /// <summary>
    /// Creates a new <see cref="CssColor"/> instance.
    /// </summary>
    /// <param name="value">The CSS color string.</param>
    /// <returns>A new <see cref="CssColor"/> instance.</returns>
    public static CssColor Create(string value) => new(value);
    
    /// <summary>
    /// Gets the default board background color.
    /// </summary>
    public static CssColor DefaultBoardBackground => new("#1e293b");

    /// <summary>
    /// Gets the default board title color.
    /// </summary>
    public static CssColor DefaultTitleColor => new("#ffffff");

    /// <summary>
    /// Gets the transparent color.
    /// </summary>
    public static CssColor Transparent => new("transparent");
}
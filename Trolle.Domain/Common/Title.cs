using System;
using System.Collections.Generic;

namespace Trolle.Domain.Common;

/// <summary>
/// Represents a validated title/name string.
/// </summary>
public class Title : ValueObject
{
    private const int MinLength = 1;
    private const int MaxLength = 200;

    /// <summary>
    /// Gets the string value of the title.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Title"/> class.
    /// </summary>
    /// <param name="value">The title string.</param>
    private Title(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Title cannot be empty");

        var trimmed = value.Trim();
        if (trimmed.Length < MinLength || trimmed.Length > MaxLength)
            throw new ArgumentException($"Title must be between {MinLength} and {MaxLength} characters");

        Value = trimmed;
    }

    /// <summary>
    /// Gets the equality components for the title.
    /// </summary>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Creates a new <see cref="Title"/> instance.
    /// </summary>
    /// <param name="value">The title string.</param>
    /// <returns>A new <see cref="Title"/> instance.</returns>
    public static Title Create(string value) => new(value);
}
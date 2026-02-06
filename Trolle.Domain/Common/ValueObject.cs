using System.Collections.Generic;
using System.Linq;

namespace Trolle.Domain.Common;

/// <summary>
/// Base class for Value Objects (DDD).
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the components that should be used for equality comparison.
    /// </summary>
    /// <returns>An enumerable of equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>True if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code for the current value object based on its equality components.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(0, HashCode.Combine);
    }
}
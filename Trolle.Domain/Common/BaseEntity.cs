using System;

namespace Trolle.Domain.Common;

/// <summary>
/// Base class for entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// Gets the date and time when the entity was last modified.
    /// </summary>
    public DateTime? LastModifiedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency control.
    /// </summary>
    public uint RowVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        RowVersion = 1;
    }

    /// <summary>
    /// Updates the audit information.
    /// </summary>
    public void UpdateAudit()
    {
        LastModifiedAt = DateTime.UtcNow;
    }
}
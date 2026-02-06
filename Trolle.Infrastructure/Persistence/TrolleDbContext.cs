using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Trolle.Domain.Common;
using Trolle.Domain.Entities;

namespace Trolle.Infrastructure.Persistence;

public class TrolleDbContext : DbContext
{
    public TrolleDbContext(DbContextOptions<TrolleDbContext> options) : base(options)
    {
    }

    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<Label> Labels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Optimistic Concurrency for BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .HasColumnName("row_version")
                    .IsConcurrencyToken();
            }
        }

        // Value Converters
        var titleConverter = new ValueConverter<Title, string>(
            v => v.Value,
            v => Title.Create(v));

        var colorConverter = new ValueConverter<CssColor, string>(
            v => v.Value,
            v => CssColor.Create(v));

        // =======================
        // Board
        // =======================
        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("board");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.IsFavorite)
                .HasColumnName("is_favorite")
                .HasDefaultValue(false);

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasConversion(titleConverter)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TitleColor)
                .HasColumnName("title_color")
                .HasConversion(colorConverter);

            entity.Property(e => e.BackgroundColor)
                .HasColumnName("background_color")
                .HasConversion(colorConverter);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.LastModifiedAt)
                .HasColumnName("last_modified_at");

            entity.HasMany(e => e.Columns)
                .WithOne()
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Labels)
                .WithOne()
                .HasForeignKey(l => l.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // For private backing field
            entity.Metadata.FindNavigation(nameof(Board.Columns))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);

            entity.Metadata.FindNavigation(nameof(Board.Labels))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // =======================
        // Column
        // =======================
        modelBuilder.Entity<Column>(entity =>
        {
            entity.ToTable("column");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasConversion(titleConverter)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TitleColor)
                .HasColumnName("title_color")
                .HasConversion(colorConverter);

            entity.Property(e => e.HeaderColor)
                .HasColumnName("header_color")
                .HasConversion(colorConverter);

            entity.Property(e => e.Order)
                .HasColumnName("order");

            entity.Property(e => e.BoardId)
                .HasColumnName("board_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.LastModifiedAt)
                .HasColumnName("last_modified_at");

            entity.HasMany(e => e.Cards)
                .WithOne()
                .HasForeignKey(c => c.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            // For private backing field
            entity.Metadata.FindNavigation(nameof(Column.Cards))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // =======================
        // Card
        // =======================
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("card");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.Title)
                .HasColumnName("title")
                .HasConversion(titleConverter)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.Order)
                .HasColumnName("order");

            entity.Property(e => e.ColumnId)
                .HasColumnName("column_id");

            entity.Property(e => e.IsArchived)
                .HasColumnName("is_archived")
                .HasDefaultValue(false);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.LastModifiedAt)
                .HasColumnName("last_modified_at");

            entity.HasMany(e => e.Labels)
                .WithMany(l => l.Cards)
                .UsingEntity(j => j.ToTable("card_label"));

            // For private backing field
            entity.Metadata.FindNavigation(nameof(Card.Labels))
                ?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        // =======================
        // Label
        // =======================
        modelBuilder.Entity<Label>(entity =>
        {
            entity.ToTable("label");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasConversion(titleConverter)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TextColor)
                .HasColumnName("text_color")
                .HasConversion(colorConverter)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Color)
                .HasColumnName("color")
                .HasConversion(colorConverter)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.BoardId)
                .HasColumnName("board_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.LastModifiedAt)
                .HasColumnName("last_modified_at");
        }); 
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                // Increment version manually for Optimistic Concurrency
                entry.Entity.RowVersion++;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
using NUnit.Framework;
using Trolle.Domain.Entities;

namespace Trolle.UnitTests.Domain;

[TestFixture]
public class CardTests
{
    [Test]
    public void Constructor_WithValidTitle_ShouldCreateCard()
    {
        // Arrange
        var title = "Task 1";
        var colId = Guid.NewGuid();

        // Act
        var card = new Card(title, "Desc", 0, colId);

        // Assert
        Assert.That(card.Title, Is.EqualTo(title));
        Assert.That(card.IsArchived, Is.False);
    }

    [Test]
    public void Archive_ShouldSetIsArchivedToTrue()
    {
        // Arrange
        var card = new Card("Title", "Desc", 0, Guid.NewGuid());

        // Act
        card.Archive();

        // Assert
        Assert.That(card.IsArchived, Is.True);
    }

    [Test]
    public void MoveToColumn_ShouldUpdateColumnId()
    {
        // Arrange
        var initialColId = Guid.NewGuid();
        var card = new Card("Title", "Desc", 0, initialColId);
        var targetColId = Guid.NewGuid();

        // Act
        card.MoveToColumn(targetColId);

        // Assert
        Assert.That(card.ColumnId, Is.EqualTo(targetColId));
    }
}

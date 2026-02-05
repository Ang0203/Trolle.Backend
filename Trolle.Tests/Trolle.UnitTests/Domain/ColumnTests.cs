using NUnit.Framework;
using Trolle.Domain.Entities;

namespace Trolle.UnitTests.Domain;

[TestFixture]
public class ColumnTests
{
    [Test]
    public void Constructor_WithValidData_ShouldCreateColumn()
    {
        // Arrange
        var title = "To Do";
        var boardId = Guid.NewGuid();
        var order = 1;

        // Act
        var column = new Column(title, order, boardId);

        // Assert
        Assert.That(column.Title, Is.EqualTo(title));
        Assert.That(column.Order, Is.EqualTo(order));
        Assert.That(column.BoardId, Is.EqualTo(boardId));
    }

    [Test]
    public void UpdateTitle_WithEmptyTitle_ShouldThrowException()
    {
        // Arrange
        var column = new Column("Title", 1, Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => column.UpdateTitle(""));
    }

    [Test]
    public void SetOrder_ShouldUpdateOrder()
    {
        // Arrange
        var column = new Column("Title", 1, Guid.NewGuid());

        // Act
        column.SetOrder(5);

        // Assert
        Assert.That(column.Order, Is.EqualTo(5));
    }
}

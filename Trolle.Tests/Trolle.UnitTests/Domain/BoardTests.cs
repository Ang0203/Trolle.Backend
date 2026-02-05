using NUnit.Framework;
using Trolle.Domain.Entities;

namespace Trolle.UnitTests.Domain;

[TestFixture]
public class BoardTests
{
    [Test]
    public void Constructor_WithValidTitle_ShouldCreateBoard()
    {
        // Arrange
        var title = "Test Board";
        var backgroundImage = "bg.jpg";

        // Act
        var board = new Board(title, backgroundImage);

        // Assert
        Assert.That(board.Title, Is.EqualTo(title));
        Assert.That(board.BackgroundImage, Is.EqualTo(backgroundImage));
        Assert.That(board.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Constructor_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var title = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Board(title));
    }

    [Test]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitle()
    {
        // Arrange
        var board = new Board("Old Title");
        var newTitle = "New Title";

        // Act
        board.UpdateTitle(newTitle);

        // Assert
        Assert.That(board.Title, Is.EqualTo(newTitle));
    }

    [Test]
    public void ToggleFavorite_ShouldFlipIsFavorite()
    {
        // Arrange
        var board = new Board("Test Board");
        var initial = board.IsFavorite;

        // Act
        board.ToggleFavorite();

        // Assert
        Assert.That(board.IsFavorite, Is.EqualTo(!initial));
        
        // Act again
        board.ToggleFavorite();
        Assert.That(board.IsFavorite, Is.EqualTo(initial));
    }
}

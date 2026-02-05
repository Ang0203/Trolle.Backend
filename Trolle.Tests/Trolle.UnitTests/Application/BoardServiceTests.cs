using Moq;
using NUnit.Framework;
using Trolle.Application.Interfaces.Persistence;
using Trolle.Application.Services;
using Trolle.Domain.Entities;

namespace Trolle.UnitTests.Application;

[TestFixture]
public class BoardServiceTests
{
    private Mock<IBoardRepository> _boardRepoMock;
    private Mock<ICardRepository> _cardRepoMock;
    private Mock<ILabelRepository> _labelRepoMock;
    private BoardService _boardService;

    [SetUp]
    public void SetUp()
    {
        _boardRepoMock = new Mock<IBoardRepository>();
        _cardRepoMock = new Mock<ICardRepository>();
        _labelRepoMock = new Mock<ILabelRepository>();
        _boardService = new BoardService(_boardRepoMock.Object, _cardRepoMock.Object, _labelRepoMock.Object);
    }

    [Test]
    public async Task GetBoardsAsync_ShouldReturnOrderedBoards()
    {
        // Arrange
        var boards = new List<Board>
        {
            new Board("Board 1") {  },
            new Board("Board 2") {  }
        };
        // Use reflection to set IsFavorite if it's private set, but Board has ToggleFavorite
        boards[1].ToggleFavorite(); // Board 2 is favorite

        _boardRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(boards);

        // Act
        var result = await _boardService.GetBoardsAsync();

        // Assert
        var resultList = result.ToList();
        Assert.That(resultList.Count, Is.EqualTo(2));
        Assert.That(resultList[0].Title, Is.EqualTo("Board 2")); // Favorite first
        Assert.That(resultList[1].Title, Is.EqualTo("Board 1"));
    }

    [Test]
    public async Task CreateBoardAsync_ShouldCallAddAsyncAndReturnId()
    {
        // Arrange
        var title = "New Board";
        _boardRepoMock.Setup(r => r.AddAsync(It.IsAny<Board>())).Returns(Task.CompletedTask);

        // Act
        var id = await _boardService.CreateBoardAsync(title);

        // Assert
        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        _boardRepoMock.Verify(r => r.AddAsync(It.Is<Board>(b => b.Title == title)), Times.Once);
    }

    [Test]
    public void UpdateTitleAsync_WhenBoardNotFound_ShouldThrowException()
    {
        // Arrange
        var boardId = Guid.NewGuid();
        _boardRepoMock.Setup(r => r.GetByIdAsync(boardId)).ReturnsAsync((Board?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _boardService.UpdateTitleAsync(boardId, "New Title"));
        Assert.That(ex.Message, Is.EqualTo("Board not found"));
    }
}

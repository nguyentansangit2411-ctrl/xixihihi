using FluentAssertions;
using Moq;
using Xixihihi.Application.Features.SavedSearches.Commands;
using Xixihihi.Domain.Entities;
using Xixihihi.Domain.Interfaces.Repositories;

namespace Xixihihi.Application.Tests.Features.SavedSearches.Commands;

public class CreateSavedSearchCommandHandlerTests
{
    private readonly Mock<IBaseRepository<SavedSearch>> _mockSavedSearchRepository;
    private readonly CreateSavedSearchCommandHandler _handler;

    public CreateSavedSearchCommandHandlerTests()
    {
        _mockSavedSearchRepository = new Mock<IBaseRepository<SavedSearch>>();
        _handler = new CreateSavedSearchCommandHandler(_mockSavedSearchRepository.Object, new Mock<Xixihihi.Domain.Interfaces.IUnitOfWork>().Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesSavedSearch()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateSavedSearchCommand 
        { 
            UserId = userId, 
            Name = "Find iPhone",
            SearchTerm = "iphone" 
        };

        _mockSavedSearchRepository.Setup(repo => repo.AddAsync(It.IsAny<SavedSearch>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SavedSearch s, CancellationToken ct) => { s.Id = Guid.NewGuid(); return s; });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Find iPhone");
        result.Data.SearchTerm.Should().Be("iphone");
        result.Data.IsActive.Should().BeTrue();
    }
}


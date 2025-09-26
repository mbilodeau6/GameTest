using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class TestHelpersTests
{
    [Fact]
    public void ValidateId_rejectsEmptyString()
    {
        // Arrange
        string testId = string.Empty;

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsNullString()
    {
        // Arrange
        string? testId = null;

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId!, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsWrongStartingLetter()
    {
        // Arrange
        string testId = "E1";

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsNoNumberPart()
    {
        // Arrange
        string testId = "T";

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsNonNumericPart()
    {
        // Arrange
        string testId = "TABC";

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsZeroNumber()
    {
        // Arrange
        string testId = "T0";

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }

    [Fact]
    public void ValidateId_rejectsNegativeNumber()
    {
        // Arrange
        string testId = "T-5";

        // Act & Assert
        Assert.False(TestHelpers.ValidateId(testId, 'T'));
    }
    
    [Fact]
    public void ValidateId_acceptsValidId()
    {
        // Arrange
        string testId = "G23";

        // Act & Assert
        Assert.True(TestHelpers.ValidateId(testId, 'G'));
    }
}
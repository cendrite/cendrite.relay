using ChatServer.Application.Features.Media.Commands.Create;
using FluentValidation.TestHelper;

namespace ChatServer.Test.Unit.CommandTest.CreateMediaCommandTest;

public class CreateMediaCommandValidatorTest
{
    private readonly CreateMediaCommandValidator _validator = new CreateMediaCommandValidator();

    [Fact]
    public void Should_Have_Error_When_ClientId_Is_Empty()
    {
        // Arrange
        var command = new CreateMediaCommand { ClientId = Guid.Empty, Size = 1024 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void Should_Have_Error_When_Size_Is_Zero()
    {
        // Arrange
        var command = new CreateMediaCommand { ClientId = Guid.NewGuid(), Size = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Fact]
    public void Should_Have_Error_When_Size_Exceeds_Limit()
    {
        // Arrange
        var command = new CreateMediaCommand { ClientId = Guid.NewGuid(), Size = 100 * 1024 * 1024 + 1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateMediaCommand { ClientId = Guid.NewGuid(), Size = 100 * 1024 * 1024 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClientId);
        result.ShouldNotHaveValidationErrorFor(x => x.Size);
    }
}

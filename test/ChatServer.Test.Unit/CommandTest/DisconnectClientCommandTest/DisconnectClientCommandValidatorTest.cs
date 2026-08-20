using ChatServer.Application.Enums;
using ChatServer.Application.Features.Client.Commands.Disconnect;
using FluentValidation.TestHelper;

namespace ChatServer.Test.Unit.CommandTest.DisconnectClientCommandTest;

public class DisconnectClientCommandValidatorTest
{
    private readonly DisconnectCommandValidator _validator = new DisconnectCommandValidator();

    [Fact]
    public void Should_Have_Error_When_TransientId_Is_Empty()
    {
        // Arrange
        var command = new DisconnectClientCommand { TransientId = string.Empty, HubType = HubType.ChangeEvent };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TransientId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TransientId_Is_Valid()
    {
        // Arrange
        var command = new DisconnectClientCommand { TransientId = "validTransientId", HubType = HubType.ChangeEvent };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TransientId);
    }
}
using System.Security.Claims;

namespace ChatServer.API.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId?.Value, out var parsedGuid)
            ? parsedGuid
            : throw new UnauthorizedAccessException("User ID not found or invalid.");
    }

    public static Guid GetDeviceId(this ClaimsPrincipal principal)
    {
        var deviceId = principal.FindFirst("sub");
        return Guid.TryParse(deviceId?.Value, out var parsedGuid)
            ? parsedGuid
            : throw new UnauthorizedAccessException("Device ID not found or invalid.");
    }
}
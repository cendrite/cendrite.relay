using System.Text.RegularExpressions;

namespace ChatServer.API.Common;

internal static partial class MediaIdValidator
{
    public static bool IsValid(string? id) => !string.IsNullOrEmpty(id) && MediaIdRegex().IsMatch(id);

    [GeneratedRegex("^[a-zA-Z0-9_-]{4,128}$")]
    private static partial Regex MediaIdRegex();
}

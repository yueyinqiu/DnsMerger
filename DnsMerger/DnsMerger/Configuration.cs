namespace DnsMerger;

public sealed record Configuration(
    string? EndPoint, 
    IReadOnlyList<string?>? Servers, 
    TimeSpan Timeout);
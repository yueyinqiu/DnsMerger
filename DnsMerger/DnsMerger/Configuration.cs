namespace DnsMerger;

public sealed record Configuration(
    string? ListeningEndPoint, 
    IReadOnlyList<string?>? ServersToMerge, 
    TimeSpan Timeout,
    TimeSpan TimeToLive);
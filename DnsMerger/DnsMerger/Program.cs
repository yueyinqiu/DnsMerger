using DNS.Server;
using DnsMerger;
using System.Net;
using System.Text.Json;
using System.Threading;

if (args.Length is 0)
    Console.WriteLine("Please provide the path of configuration file.");

FileInfo file;
try
{
    file = new FileInfo(args[0]);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to load the configuration file '{args[0]}': {ex}");
    return;
}

Configuration? configuration = null;
if (file.Exists)
{
    using var stream = file.Open(FileMode.Open, FileAccess.Read);
    try
    {
        configuration = await JsonSerializer.DeserializeAsync(stream,
            ConfigurationSerializerContext.Default.Configuration);
        if (configuration is null)
            throw new JsonException();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to load the configuration file '{file.FullName}': {ex}");
    }
    Console.WriteLine($"Configuration Read From {file.FullName}");
}

if (configuration is null)
{
    configuration = new Configuration(
        "0.0.0.0:53", ["8.8.8.8:53", "114.114.114.114:53"], TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    using var stream = file.Open(FileMode.CreateNew, FileAccess.Write);
    await JsonSerializer.SerializeAsync(stream, configuration,
        ConfigurationSerializerContext.Default.Configuration);
    Console.WriteLine($"Configuration Written To {file.FullName}");
}

if (configuration.ListeningEndPoint is not null && 
    IPEndPoint.TryParse(configuration.ListeningEndPoint, out var endpoint))
{
    Console.WriteLine($"Will Listen On: {endpoint}");
}
else
{
    Console.WriteLine($"Cannot Resolve Listening End Point '{configuration.ListeningEndPoint}'");
    return;
}

var servers = new List<IPEndPoint>();
foreach(var server in configuration.ServersToMerge ?? [])
{
    if (server is not null &&
        IPEndPoint.TryParse(server, out var serverEndPoint))
    {
        servers.Add(serverEndPoint);
    }
    else
    {
        Console.WriteLine($"Cannot Resolve Server To Merge '{server}'");
        return;
    }
}
if (servers.Count is 0)
{
    Console.WriteLine($"No Server Is Found To Merge");
    return;
}
Console.WriteLine($"Servers To Merge: {string.Join(", ", servers)}");

if(configuration.Timeout.Ticks / TimeSpan.TicksPerMillisecond > int.MaxValue)
{
    Console.WriteLine($"Timeout Is Too Large");
    return;
}
Console.WriteLine($"Timeout: {configuration.Timeout}");
Console.WriteLine($"Time To Live: {configuration.TimeToLive}");

var resolver = new DnsRequestResolver(servers, configuration.Timeout, configuration.TimeToLive);
DnsServer dnsServer = new DnsServer(resolver);
dnsServer.Errored += (sender, e) =>
{
    Console.WriteLine($"Error:");
    Console.WriteLine($"{e.Exception}");
    Console.WriteLine();
};
await dnsServer.Listen(endpoint);

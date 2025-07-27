
using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using System.Net;
using System.Text;

namespace DnsMerger;

public sealed class DnsRequestResolver(IEnumerable<IPEndPoint> servers, TimeSpan timeout) : IRequestResolver
{
    private readonly IReadOnlyList<DnsClient> clients = [.. servers.Select(x => new DnsClient(
        new UdpRequestResolver(x, new TcpRequestResolver(x), (int)timeout.TotalMilliseconds)))];

    public async Task<IResponse> Resolve(
        IRequest request, 
        CancellationToken cancellationToken = default)
    {
        var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new List<Task<IResponse>>();
        foreach (var client in clients)
        {
            tasks.Add(client.Create(request).Resolve(token.Token));
        }

        await Task.WhenAny(tasks[0], Task.Delay(timeout, token.Token));
        cancellationToken.ThrowIfCancellationRequested();
        await token.CancelAsync();

        int index = tasks.FindIndex(x => x.IsCompletedSuccessfully);
        var result = index is -1 ? Response.FromRequest(request) : tasks[index].Result;

        var builder = new StringBuilder();
        _ = builder.AppendLine($"Request:");
        _ = builder.AppendLine($"{request}");
        _ = builder.AppendLine();
        _ = builder.AppendLine($"Result From {index}:");
        _ = builder.AppendLine($"{result}");
        _ = builder.AppendLine();
        Console.WriteLine(builder);
        return result;
    }
}

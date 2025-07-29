using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Linq;
using System.Net;
using System.Text;

namespace DnsMerger;

public sealed class DnsRequestResolver(IEnumerable<IPEndPoint> servers, TimeSpan timeout, TimeSpan ttl) : IRequestResolver
{
    private readonly IReadOnlyList<DnsClient> clients = [.. servers.Select(x => new DnsClient(
        new UdpRequestResolver(x, new TcpRequestResolver(x), (int)(timeout.Ticks / TimeSpan.TicksPerMillisecond))))];

    public async Task<IResponse> Resolve(
        IRequest request, 
        CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<IResponse>>();
        foreach (var client in clients)
        {
            tasks.Add(client.Create(request).Resolve(cancellationToken));
        }

        IResponse? result = null;
        List<Exception> failures = [];
        foreach (var task in tasks)
        {
            try
            {
                result = await task;
            }
            catch (Exception e)
            {
                failures.Add(e);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();

        var builder = new StringBuilder();
        _ = builder.AppendLine($"====================");
        _ = builder.AppendLine($"Request:");
        _ = builder.AppendLine($"{request}");
        _ = builder.AppendLine();
        for (int i = 0; i < failures.Count; i++)
        {
            var failure = failures[i];
            if (failure is ResponseException responseException)
            {
                _ = builder.AppendLine($"[{i}] Response (Exception):");
                _ = builder.AppendLine($"{responseException.Response}");
                _ = builder.AppendLine();
            }
            else
            {
                _ = builder.AppendLine($"[{i}] Exception:");
                _ = builder.AppendLine($"{failure}");
                _ = builder.AppendLine();
            }
        }

        if (result is not null)
        {
            _ = builder.AppendLine($"[{failures.Count}] Response:");
            _ = builder.AppendLine($"{result}");
            _ = builder.AppendLine();
        }
        else
        {
            foreach (var failure in failures)
            {
                if (failure is ResponseException responseException)
                {
                    result = responseException.Response;
                    break;
                }
            }

            if (result is null)
            {
                result = Response.FromRequest(request);
                result.ResponseCode = ResponseCode.ServerFailure;
            }
        }

        for (int i = 0; i < result.AnswerRecords.Count; i++)
            result.AnswerRecords[i] = result.AnswerRecords[i].WithShorterTtl(ttl);

        _ = builder.AppendLine($"Final Response:");
        _ = builder.AppendLine($"{result}");
        _ = builder.AppendLine($"====================");
        _ = builder.AppendLine();
        _ = builder.AppendLine();
        Console.WriteLine(builder);
        return result;
    }
}

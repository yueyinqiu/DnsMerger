
using DNS.Client;
using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using System.Net;
using System.Text;

namespace DnsMerger;

public static class ResourceRecordExtensions
{
    public static ResourceRecord WithShorterTtl(this IResourceRecord record, TimeSpan ttl)
    {
        if (record.TimeToLive < ttl)
            ttl = record.TimeToLive;
        return new ResourceRecord(record.Name, record.Data, record.Type, record.Class, ttl);
    }
}

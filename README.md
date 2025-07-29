# DnsMerger

A DNS server redirecting requests to other DNS servers with priorities. It can solve the DNS problem of Clash when switching between internal and external networks.

Usage:

Create a `config.json`:

```json
{
    "ListeningEndPoint": "0.0.0.0:7891", // end point to listen
    "ServersToMerge": [
        "127.0.0.1:53", // internal DNS
        "8.8.8.8:53" // external DNS
    ],
    "Timeout": "00:00:01", // timeout when accessing the above servers
    "TimeToLive": "00:00:00" // time to live (TTL) override (some applications may always use the previous response although the network has been switched, if TTL is set too long. but low TTL also cause DNS services to be accessed too frequently.)
}
```

Run `dotnet ./DnsMerger.dll ./config.json`.
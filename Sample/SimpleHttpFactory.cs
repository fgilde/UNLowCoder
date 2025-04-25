using System.Collections.Concurrent;

namespace Sample;

public class SimpleHttpFactory: IHttpClientFactory
{
    private ConcurrentDictionary<string, HttpClient> _clients = new();

    public HttpClient CreateClient(string name)
    {
        if (_clients.TryGetValue(name, out var client))
            return client;
        client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        return _clients.GetOrAdd(name, client);
    }
}
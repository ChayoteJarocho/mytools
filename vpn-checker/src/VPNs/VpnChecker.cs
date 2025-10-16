using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DNS.Client;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

namespace VpnChecker;

public class VpnChecker : IDisposable
{
    private readonly HttpClient _client;
    public VpnChecker()
    {
        _client = new HttpClient();
    }

    public void Dispose() => _client.Dispose();

    public async ValueTask CheckNordVpnAsync(bool verbose)
    {
        NordVpnData? vpn = await _client.GetFromJsonAsync<NordVpnData>(NordVpnData.URL);

        if (vpn == null)
        {
            Console.WriteLine("Could not retrieve the NordVPN status json.");
            Environment.Exit(1);
        }

        if (verbose)
        {
            Console.WriteLine(JsonSerializer.Serialize(vpn, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            Console.WriteLine(vpn.Status);
        }
    }

    public async ValueTask CheckSurfsharkAsync(bool verbose)
    {
        // DnsClient client = new("8.8.8.8");
        // ClientRequest request = client.Create(); // Needed for the reverse
        // string myIP = await GetMyIpAsync();
        // Console.WriteLine($"My IP: {myIP}");
        // string domain = await client.Reverse(myIP);
        // Console.WriteLine($"My domain: {domain}");

        string domain = "us-sea.prod.surfshark.com";
        ClientRequest request = new("8.8.8.8");
        request.Questions.Add(new Question(Domain.FromString(domain), RecordType.A));
        request.RecursionDesired = true;
        Console.WriteLine("Resolving...");
        var response = await request.Resolve();

        Console.WriteLine("Resolved!");
        IEnumerable<IPAddress> ips = response.AnswerRecords
            .Where(r => r.Type == RecordType.A)
            .Cast<IPAddressResourceRecord>()
            .Select(r => r.IPAddress);
        
        Console.WriteLine("IP Addresses:");
        foreach (IPAddress ip in ips)
        {
            Console.WriteLine(ip);
        }

    }

    private Task<string> GetMyIpAsync() => _client.GetStringAsync("https://api.ipify.org");
}
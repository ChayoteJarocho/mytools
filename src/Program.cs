using System.Net.Http.Json;
using VpnChecker;

using HttpClient client = new HttpClient();
NordVpn? vpn = await client.GetFromJsonAsync<NordVpn>(NordVpn.URL);

if (vpn == null)
{
    Console.WriteLine("Could not retrieve the vpn status json.");
    Environment.Exit(1);
}

Console.WriteLine(vpn.Status);


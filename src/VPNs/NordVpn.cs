using System.Text.Json.Serialization;

namespace VpnChecker;

/*
Example:
{
    "coordinates": {
        "latitude": 12.3456,
        "longitude": -12.3456
    },
    "ip": "123.45.678.901",
    "isp": "Packethub",
    "host": {
        "ip_address": "123.45.678.901",
        "prefix_len": 19
    },
    "status": true,
    "country": "Country Name",
    "region": "Region Name",
    "city": "City Name",
    "location": "Country Name, Region Name, City Name",
    "area_code": "12345",
    "country_code": "XX"
}
*/

public class NordVpn
{
    public const string URL = "https://nordvpn.com/wp-admin/admin-ajax.php?action=get_user_info_data";

    [JsonPropertyName("coordinates")]
    public NordVpnCoordinates? Coordinates { get; set; }
    [JsonPropertyName("ip")]
    public string? IP { get; set; }
    [JsonPropertyName("isp")]
    public string? ISP { get; set; }
    [JsonPropertyName("host")]
    public NordVpnHost? Host { get; set; }
    [JsonPropertyName("status")]
    public bool Status { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    [JsonPropertyName("city")]
    public string? City { get; set; }
    [JsonPropertyName("location")]
    public string? Location { get; set; }
    [JsonPropertyName("area_code")]
    public int AreaCode { get; set; }
    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }
    
    public class NordVpnCoordinates
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }
    public class NordVpnHost
    {
        [JsonPropertyName("ip_address")]
        public string? IP { get; set; }
        [JsonPropertyName("prefix_len")]
        public int PrefixLength { get; set; }
    }
}
using System.Text.Json.Serialization;

namespace VpnChecker;

public class SurfsharkServer
{
    public const string Url = "https://api.surfshark.com/v4/server/clusters";
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
    [JsonPropertyName("region")]
    public string? Region { get; set; }
    [JsonPropertyName("regionCode")]
    public string? RegionCode { get; set; }
    [JsonPropertyName("location")]
    public string? Location { get; set; }
    // This is the field containing the server URL
    [JsonPropertyName("connectionName")]
    public string? ConnectionName { get; set; }
}

/*
Example:

114:
    country	"United States"
    countryCode	"US"
    region	"The Americas"
    regionCode	"AM"
    load	36
    createdAt	"2018-12-18T10:58:16+00:00"
    updatedAt	"2025-02-15T20:29:41+00:00"
    id	"dbe53459-2fe7-4679-b318-f65b4d704902"
    coordinates:
        longitude	-122.3308333
        latitude	47.6063889
    info:
        0:
            createdAt	"2023-04-25T14:14:45+00:00"
            updatedAt	"2025-02-15T20:29:40+00:00"
            id	"0b941812-db96-4f96-b98d-6495704cb04d"
            entry:
                value	"U2FsdGVkX1+mqsb3ErUk1NMPSpC3bk5uXu+OCHfrZd0="
    type	"generic"
    location	"Seattle"
    connectionName	"us-sea.prod.surfshark.com"
    pubKey	"SpMH/p90bg9ZAG6V2DWJQ9csWPVnKcDVppIp9Xul5G8="
    tags:
        0	"p2p"
    transitCluster	null
    flagUrl	"https://cdn.ss-cdn.com/assets/flags/US.png"
*/
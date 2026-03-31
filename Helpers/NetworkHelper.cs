using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class NetworkHelper
{
    private static readonly HttpClient _httpClient = new HttpClient();

    public static async Task<string> GetPublicIpAsync()
    {
        return await _httpClient.GetStringAsync("https://api.ipify.org");
    }

    public static async Task<string> GetGeoLocationAsync(string ip)
    {
        var json = await _httpClient.GetStringAsync($"https://ip-api.io/json/{ip}");
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var city = doc.RootElement.GetProperty("city").GetString();
        var country = doc.RootElement.GetProperty("country_name").GetString();
        return $"{city}, {country}";
    }

    /// <summary>
    /// Orchestrates both IP and Geolocation lookups. 
    /// Returns "City, Country" or "Unknown" on failure.
    /// </summary>
    public static async Task<string> GetCurrentLocationAsync()
    {
        try
        {
            var ip = await GetPublicIpAsync();
            return await GetGeoLocationAsync(ip);
        }
        catch
        {
            return "Unknown";
        }
    }
}


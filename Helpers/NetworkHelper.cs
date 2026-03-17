using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using System.Threading.Tasks;

public static class NetworkHelper
{
    public static async Task<string> GetPublicIpAsync()
    {
        using var http = new HttpClient();
        return await http.GetStringAsync("https://api.ipify.org");
    }

    public static async Task<string> GetGeoLocationAsync(string ip)
    {
        using var http = new HttpClient();
        var json = await http.GetStringAsync($"http://ip-api.com/json/{ip}");
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var city = doc.RootElement.GetProperty("city").GetString();
        var country = doc.RootElement.GetProperty("country").GetString();
        return $"{city}, {country}";
    }
}


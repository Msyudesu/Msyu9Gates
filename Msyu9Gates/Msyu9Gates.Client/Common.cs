using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Msyu9Gates.Lib;

namespace Msyu9Gates.Client;

public static class Common
{
    public static async Task<string> GetNarrative(NavigationManager nav, HttpClient http, int gateNumber, int chapter = 0)
    {
        GateRequest request = new GateRequest(key: string.Empty, gate: gateNumber, chapter: chapter);
        
        Uri uri = new Uri(nav.BaseUri + (chapter == 0 ? $"api/gates/{gateNumber}/narrative" 
                                                      : $"api/chapters/{gateNumber}/{chapter}/narrative"));
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(request)
        };

        try
        {
            var response = await http.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                GateResponse? gateResponse = await response.Content.ReadFromJsonAsync<GateResponse>();
                return gateResponse?.Message ?? "Missing Content";
            }
            else
                return "Failed to load narrative from Server";
        }
        catch
        {
            return $"Error - request for narrative failed";
        }
    }
}

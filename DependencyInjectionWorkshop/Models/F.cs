using System;
using System.Net.Http;

static internal class F
{
    public static void ResetFailCounter(string accountId)
    {
        var httpClient2 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
        var response2 = httpClient2.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
        response2.EnsureSuccessStatusCode();
        if (response2.IsSuccessStatusCode == false)
        {
            throw new Exception($"web api FailCounter reset, accountId:{accountId}");
        }
    }
}
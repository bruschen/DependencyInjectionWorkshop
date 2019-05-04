using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public string GetOneTimePassword(string accountId)
        {
            var httpClient1 = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var httpClient1Response = httpClient1.PostAsJsonAsync("api/otps", accountId).Result;
            httpClient1Response.EnsureSuccessStatusCode();

            if (httpClient1Response.IsSuccessStatusCode)
            {
                return httpClient1Response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api get opt error, accountId:{accountId}");
            }
        }
    }
}
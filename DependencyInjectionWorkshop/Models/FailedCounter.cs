using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounter
    {
        public int GetFailCounterValue(string accountId)
        {
            var httpClient3 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response4 = httpClient3.PostAsJsonAsync("api/FailCounter/Get", accountId).Result;
            response4.EnsureSuccessStatusCode();
            int failCount = response4.Content.ReadAsAsync<int>().Result;
            return failCount;
        }

        public void AddFailCounter(string accountId)
        {
            var httpClient3 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response3 = httpClient3.PostAsJsonAsync("api/FailCounter/Add", accountId).Result;
            response3.EnsureSuccessStatusCode();
            if (response3.IsSuccessStatusCode == false)
            {
            }
            else
            {
                throw new Exception($"web api FailCounter Add error, accountId:{accountId}");
            }
        }
    }
}
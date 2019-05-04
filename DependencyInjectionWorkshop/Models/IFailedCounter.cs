using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        int GetFailCounterValue(string accountId);

        void AddFailCounter(string accountId);

        void ResetFailCounter(string accountId);

        void GetAccountIsLock(string accountId);
    }

    public class FailedCounter : IFailedCounter
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

        public void ResetFailCounter(string accountId)
        {
            var httpClient2 = new HttpClient() { BaseAddress = new Uri("http://joey.dev/") };
            var response2 = httpClient2.PostAsJsonAsync("api/FailCounter/Reset", accountId).Result;
            response2.EnsureSuccessStatusCode();
            if (response2.IsSuccessStatusCode == false)
            {
                throw new Exception($"web api FailCounter reset, accountId:{accountId}");
            }
        }

        public void GetAccountIsLock(string accountId)
        {
            var httpClient4 = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var httpClient4Response = httpClient4.PostAsJsonAsync("api/Account/IsLock", accountId).Result;
            httpClient4Response.EnsureSuccessStatusCode();
            if (httpClient4Response.IsSuccessStatusCode)
            {
                if (Boolean.Parse(httpClient4Response.Content.ReadAsAsync<string>().Result) == true)
                {
                    throw new Exception($"Account Fail too many time , accountId:{accountId}");
                }
            }
            else
            {
                throw new Exception($"web api fail too many error, accountId:{accountId}");
            }
        }
    }
}
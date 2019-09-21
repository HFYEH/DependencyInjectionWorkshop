using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void ResetFailedCount(string accountId);
        bool GetAccountIsLocked(string accountID);
        void AddFailedCount(string accountID);
        string GetFailedCount(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        public void ResetFailedCount(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsync("api/failedCounter/Reset", new StringContent("account"))
                .Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string accountID)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                .PostAsync("api/failedCounter/IsLocked", new StringContent("account")).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = Convert.ToBoolean(isLockedResponse.Content.ReadAsStringAsync().Result);
            return isLocked;
        }

        public void AddFailedCount(string accountID)
        {
            var addFailedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                    .PostAsync("api/failedCounter/Add", new StringContent("account")).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public string GetFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}
                    .PostAsync("api/failedCounter/GetFailedCount", new StringContent("account")).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsStringAsync().Result;
            return failedCount;
        }
    }
}
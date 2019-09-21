using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string accountId, string password, string otp)
        {
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            
            // Check acount isLock
            var isLockedResponse = httpClient.PostAsync("api/failedCounter/IsLocked", new StringContent("account")).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            if (Convert.ToBoolean(isLockedResponse.Content.ReadAsStringAsync().Result))
            {
                throw new FailedTooManyTimesException();
            }
            
            // Get password from DB
            var passwordFromDb = GetPasswordFromDb(accountId);

            // Get hash
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();


            // Get OTP
            // JsonConvert.SerializeObject(account)
            var response = httpClient.PostAsync("api/otps", new StringContent("account")).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsStringAsync().Result;


            // Compare password
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                // Reset fail count
                var resetResponse = httpClient.PostAsync("api/failedCounter/Reset", new StringContent("account"))
                    .Result;
                resetResponse.EnsureSuccessStatusCode();

                return true;
            }
            else
            {
                // Add fail count
                var addFailedCountResponse =
                    httpClient.PostAsync("api/failedCounter/Add", new StringContent("account")).Result;
                addFailedCountResponse.EnsureSuccessStatusCode();
                
                // Add logger                
                var failedCountResponse =
                    httpClient.PostAsync("api/failedCounter/GetFailedCount", new StringContent("account")).Result;
                failedCountResponse.EnsureSuccessStatusCode();

                var failedCount = failedCountResponse.Content.ReadAsStringAsync().Result;
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{accountId} failed times:{failedCount}");

                // Add notify
                string message = $"{accountId} try to login failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");

                return false;
            }

            //throw new NotImplementedException();
        }

        private static string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordFromDb;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
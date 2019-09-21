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
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly OtpService _otpService;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            // Check acount isLock
            var isLocked = GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            
            // Get password from DB
            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);

            // Get hash
            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            // Get OTP
            // JsonConvert.SerializeObject(account)
            var currentOtp = _otpService.GetCurrentOtp(accountId);

            // Compare password
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                // Reset fail count
                ResetFailedCount(accountId);

                return true;
            }
            else
            {
                // Add fail count
                AddFailedCount(accountId);

                // Add logger                
                LogFailedCount(accountId);

                // Add notify
                _slackAdapter.Notify(accountId);

                return false;
            }

            //throw new NotImplementedException();
        }

        private static bool GetAccountIsLocked(string accountID)
        {
            var isLockedResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsync("api/failedCounter/IsLocked", new StringContent("account")).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = Convert.ToBoolean(isLockedResponse.Content.ReadAsStringAsync().Result);
            return isLocked;
        }

        private static void ResetFailedCount(string accountId)
        {
            var resetResponse = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsync("api/failedCounter/Reset", new StringContent("account"))
                .Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static void LogFailedCount(string accountId)
        {
            var failedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsync("api/failedCounter/GetFailedCount", new StringContent("account")).Result;
            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsStringAsync().Result;
            // var message = $"accountId:{accountId} failed times:{failedCount}";
            LogMessage($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void LogMessage(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }

        private static void AddFailedCount(string accountID)
        {
            var addFailedCountResponse =
                new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsync("api/failedCounter/Add", new StringContent("account")).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }
    }

    internal class OtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            var response = new HttpClient() {BaseAddress = new Uri("http://joey.com/")}.PostAsync("api/otps", new StringContent("account")).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{accountId}");
            }

            var currentOtp = response.Content.ReadAsStringAsync().Result;
            return currentOtp;
        }
    }

    internal class SlackAdapter
    {
        public void Notify(string accountId)
        {
            string message = $"{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }

    internal class Sha256Adapter
    {
        public string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }

    internal class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
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
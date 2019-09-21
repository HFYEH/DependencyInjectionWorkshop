using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using Newtonsoft.Json;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            // Get password from DB
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

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
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            // JsonConvert.SerializeObject(account)
            var response = httpClient.PostAsync("api/otps",new StringContent("account") ).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }
            
            var currentOtp = response.Content.ReadAsStringAsync().Result;

            
            
            // Compare password
            if (currentOtp == otp && hashedPassword == passwordFromDb)
            {
                return true;
            }
            else
            {
                return false;
            }

            //throw new NotImplementedException();
        }
        
    }
}
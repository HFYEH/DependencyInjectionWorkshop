using Newtonsoft.Json;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        private readonly ILogger _logger;
        //private FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService(IOtpService otpService, IProfile profile, IHash hash)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _logger = new Logger();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            // Get password from DB
            var passwordFromDb = _profile.GetPassword(accountId);

            // Get hash
            var hashedPassword = _hash.Compute(password);

            // Get OTP
            // JsonConvert.SerializeObject(account)
            var currentOtp = _otpService.GetCurrentOtp(accountId);

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
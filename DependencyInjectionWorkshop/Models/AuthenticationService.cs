using Newtonsoft.Json;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;
        //private FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService, IProfile profile, IHash hash)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _logger = logger;
        }
        
        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new Logger();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            // Check acount isLock
            var isLocked = _failedCounter.GetAccountIsLocked(accountId);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

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
                // Add fail count
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                // Add logger                
                _logger.Info($"accountID:{accountId} failed times:{failedCount}");

                return false;
            }

            //throw new NotImplementedException();
        }

    }
}
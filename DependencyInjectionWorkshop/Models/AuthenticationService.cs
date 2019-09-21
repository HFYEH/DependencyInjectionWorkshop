using Newtonsoft.Json;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly INotification _notification;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, INotification notification, IOtpService otpService, IFailedCounter failedCounter, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _notification = notification;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _logger = logger;
        }
        
        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _notification = new SlackAdapter();
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
                // Reset fail count
                _failedCounter.ResetFailedCount(accountId);

                return true;
            }
            else
            {
                // Add fail count
                _failedCounter.AddFailedCount(accountId);

                var failedCount = _failedCounter.GetFailedCount(accountId);
                // Add logger                
                _logger.LogMessage($"accountID:{accountId} failed times:{failedCount}");

                // Add notify
                _notification.Send(accountId);

                return false;
            }

            //throw new NotImplementedException();
        }
    }
}
namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authenticationService, ILogger logger,
            IFailedCounter failedCounter) : base(authenticationService)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogFailedCount(accountId);
            }

            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.GetFailedCount(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}
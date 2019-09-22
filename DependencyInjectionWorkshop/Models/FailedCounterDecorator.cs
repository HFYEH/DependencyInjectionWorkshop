namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (isValid)
            {
                Reset(accountId);
            }

            return isValid;
        }

        private void Reset(string accountId)
        {
            _failedCounter.ResetFailedCount(accountId);
        }
    }
}
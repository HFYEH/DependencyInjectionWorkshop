namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodInfoDecorator : AuthenticationBaseDecorator
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }
        
        public override bool Verify(string accountId, string password, string otp)
        {
            _logger.Info($"== {accountId} {password} {otp} ==");
            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"== isValid: {isValid} ==");
            return isValid;
        }
    }
}

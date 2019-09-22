using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class AuditLogDecorator : AuthenticationBaseDecorator
    {
        private readonly IContext _context;
        private readonly ILogger _logger;

        public AuditLogDecorator(IAuthentication authentication, ILogger logger, IContext context) : base(
            authentication)
        {
            _logger = logger;
            _context = context;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var currentUser = _context.GetCurrentUser();
            _logger.Info($"user:{currentUser.Name} invoke with parameters: {accountId} | {password} | {otp}");

            var isValid = base.Verify(accountId, password, otp);
            _logger.Info($"isValid:{isValid}");
            return isValid;
        }
    }

    public interface IContext
    {
        Account GetCurrentUser();
        void SetCurrentUser(Account account);
    }

    public class Account
    {
        public string Name { get; set; }
    }
}
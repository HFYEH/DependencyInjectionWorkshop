namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : AuthenticationBaseDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }
        
        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid1 = base.Verify(accountId, password, otp);
            var isValid = isValid1;
            if (!isValid)
            {
                Send(accountId);
            }

            return isValid;
        }
        
        private void Send(string accountId)
        {
            _notification.Send(accountId);
        }
    }
}
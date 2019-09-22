namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly INotification _notification;
        private readonly IAuthentication _authentication;
        
        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }
        
        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
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
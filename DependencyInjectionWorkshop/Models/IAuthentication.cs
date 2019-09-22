using System;
namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        [AuditLog]
        bool Verify(string accountId, string password, string otp);
    }
    
    public class AuditLogAttribute : Attribute
    {
    }
}
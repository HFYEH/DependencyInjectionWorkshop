namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPassword(string accountId);
    }
}
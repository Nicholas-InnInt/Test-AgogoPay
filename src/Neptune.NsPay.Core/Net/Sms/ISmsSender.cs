using System.Threading.Tasks;

namespace Neptune.NsPay.Net.Sms
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}
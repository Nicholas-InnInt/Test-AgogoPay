using Neptune.NsPay.MongoDbExtensions.Models;
using System.Text.Json;

namespace Neptune.NsPay.Web.TransferApi
{
    public class ExtensionClass
    {
    }

    public static class MongoEntityExtension
    {
        public static T DeepClone<T>(this T str) where T : BaseMongoEntity
        {
            var json = JsonSerializer.Serialize(str);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

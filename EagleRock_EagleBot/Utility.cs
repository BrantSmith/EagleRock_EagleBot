using System.Text.Json;

namespace EagleRock_EagleBot
{
    public static class Utility
    {
        public static T? MapByteArrayTo<T>(byte[] data)
        {
            using MemoryStream ms = new(data);
            return JsonSerializer.Deserialize<T>(ms);
        }
    }
}

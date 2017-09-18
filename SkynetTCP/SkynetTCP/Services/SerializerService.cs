using System;
using System.Text;
using Newtonsoft.Json;

using SkynetTCP.Services.Interfaces;

namespace SkynetTCP.Services
{
    public class SerializerService : ISerializerService
    {
        public T Deserialize<T>(byte[] bytes)
        {
            var json = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            Console.WriteLine(json);
            
            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] Serialize<T>(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}

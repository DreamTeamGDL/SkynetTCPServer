using System;
using System.Collections.Generic;
using System.Text;

namespace SkynetTCP.Services.Interfaces
{
    public interface ISerializerService
    {
        T Deserialize<T>(byte[] bytes);

        byte[] Serialize<T>(T obj);
    }
}

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectSerialization
{
    public static byte[] Serialize<T>(T obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T Deserialize<T>(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(ms);
        }
    }
}
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Client.Util;

public class StorageUtil
{
    public static async void storeData<T>(string filename, T toSave)
    {
        await Task.Run(() =>
        {
            lock (toSave)
            {
                using var storage = IsolatedStorageFile.GetUserStoreForApplication();
                try
                {
                    using var fs = storage.OpenFile(filename, FileMode.Create);
                    if (fs != null)
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        serializer.WriteObject(fs, toSave);
                    }
                }
                catch (IsolatedStorageException ise)
                {
                    Console.WriteLine($"Error while storing {filename}:");
                    Console.WriteLine(ise);
                }
            }
        });
    }

    public static object loadData<T>(string filename)
    {
        using var storage = IsolatedStorageFile.GetUserStoreForApplication();
        try
        {
            if (storage.FileExists(filename))
            {
                using var fs = storage.OpenFile(filename, FileMode.Open);
                {
                    if (fs != null)
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        return (T)serializer.ReadObject(fs);
                    }
                }
            }
        }
        catch (IsolatedStorageException ise)
        {
            Console.WriteLine($"Error while loading {filename}:");
            Console.WriteLine(ise);
        }

        return null;
    }
}
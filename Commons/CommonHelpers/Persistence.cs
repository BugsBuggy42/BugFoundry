namespace Buggary.Commons.CommonHelpers
{
    using System.IO;
    using Newtonsoft.Json;
    using UnityEngine;

    public class Persistence<T> where T : new()
    {
        private string fileName;

        public Persistence(string fileNameIn)
        {
            this.fileName = fileNameIn;
            if (!this.fileName.EndsWith(".json")) this.fileName += ".json";
        }

        public T Get(string name = null)
        {
            if (name == null)
                name = this.fileName;

            string path = Path.Combine(Application.persistentDataPath, name);
            if (!File.Exists(path))
            {
                Debug.Log("File does not exist");
                return new T();
            }

            string json = File.ReadAllText(path);
            T result = JsonConvert.DeserializeObject<T>(json);
            return result;
        }

        public string Set(T data, string name = null)
        {
            if (name == null)
                name = this.fileName;

            string path = Path.Combine(Application.persistentDataPath, name);
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, json);
            return path;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace GameEngine
{
    public class Settings
    {
        private Dictionary<string, object> _data;
        private string _path;
        private JavaScriptSerializer _jsSerializer;

        public Settings(string path)
        {
            _path = path;
            _data = new Dictionary<string, object>();
            _jsSerializer = new JavaScriptSerializer();
            Load();
        }

        public void Load()
        {
            _data.Clear();
            if (!File.Exists(_path)) Save();
            var lines = File.ReadAllLines(_path);
            foreach (var line in lines)
            {
                var data = line.Split('=');
                var key = data[0];
                var type = Type.GetType(data[1]);
                var json = line.Substring(data[0].Length + 1 + data[1].Length + 1);
                _data[key] = _jsSerializer.Deserialize(json, type);
            }
        }

        public void Save()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var kvp in _data)
            {
                builder.Append(kvp.Key).Append("=");
                builder.Append(kvp.Value.GetType()).Append("=");
                builder.AppendLine(_jsSerializer.Serialize(kvp.Value));
            }
            File.WriteAllText(_path, builder.ToString());
        }

        public T Get<T>(string key, T @default)
        {
            if (key.Contains("=")) throw new ArgumentException("Key cannot contain the character '='");

            object objValue;
            if (!_data.TryGetValue(key, out objValue))
            {
                var def = @default;
                if (def == null) def = default(T);
                objValue = def;
                Set(key, objValue);
            }
            return (T)objValue;
        }

        public T Get<T>(string key) where T : new()
        {
            return Get(key, default(T));
        }

        public void Set<T>(string key, T value)
        {
            if (key.Contains("=")) throw new ArgumentException("Key cannot contain the character '='");

            _data[key] = value;
        }
    }
}

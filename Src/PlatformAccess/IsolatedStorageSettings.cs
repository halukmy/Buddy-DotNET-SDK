using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BuddySDK
{
    internal abstract class IsolatedStorageSettings
    {
        protected abstract IsolatedStorageFile GetIsolatedStorageFile();


        public IDictionary<string, string> LoadSettings()
        {
            var isoStore = GetIsolatedStorageFile();
            string existing = "";

            if (isoStore.FileExists("_buddy"))
            {
                using (var fs = isoStore.OpenFile("_buddy", FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        existing = sr.ReadToEnd();
                    }
                }
            }

            var d = new Dictionary<string, string>();
            var parts = Regex.Match(existing, "(?<key>\\w*)=(?<value>.*?);");

            while (parts.Success)
            {
                d[parts.Groups["key"].Value] = parts.Groups["value"].Value;

                parts = parts.NextMatch();
            }

            return d;
        }

        public void SaveSettings(IDictionary<string, string> values)
        {
            var isoStore = GetIsolatedStorageFile();

            var sb = new StringBuilder();

            foreach (var kvp in values)
            {
                sb.AppendFormat("{0}={1};", kvp.Key, kvp.Value ?? "");
            }

            using (var fs = isoStore.OpenFile("_buddy", FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sb.ToString());

                    sw.Flush();
                    fs.Flush();
                }
            }
        }

        public void SetUserSetting(string key, string value, DateTime? expires = default(DateTime?))
        {
            if (key == null) throw new ArgumentNullException("key");

           
            // parse it
            var parsed = LoadSettings();
            string encodedValue = PlatformAccess.EncodeUserSetting(value, expires);
            parsed[key] = encodedValue;

            SaveSettings( parsed);
        }

        public string GetUserSetting(string key)
        {
            
            var parsed = LoadSettings();

            if (parsed.ContainsKey(key))
            {
                var value = PlatformAccess.DecodeUserSetting((string)parsed[key]);

                if (value == null)
                {
                    ClearUserSetting(key);
                }

                return value;
            }

            return null;
        }

        public void ClearUserSetting(string key)
        {
           
            var parsed = LoadSettings();

            if (parsed.ContainsKey(key))
            {
                parsed.Remove(key);
                SaveSettings(parsed);
            }
        }
    }
}

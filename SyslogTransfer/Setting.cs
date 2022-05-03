using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using SyslogTransfer.Log.Syslog;

namespace SyslogTransfer
{
    internal class Setting
    {
        public string SyslogServer { get; set; }
        public bool SyslogSslEncrypt { get; set; }
        public int SyslogSslTimeout { get; set; }
        public SyslogFormat? SyslogFormat { get; set; }

        public void Init()
        {
            this.SyslogServer = "udp://localhost:514";
            this.SyslogSslEncrypt = false;
            this.SyslogSslTimeout = 1000;
            this.SyslogFormat = SyslogTransfer.Log.Syslog.SyslogFormat.RFC3164;
        }

        public static Setting Deserialize(string filePath)
        {
            Setting setting = null;
            try
            {
                using (var sr = new StreamReader(filePath, Encoding.UTF8))
                {
                    setting = JsonSerializer.Deserialize<Setting>(sr.ReadToEnd(),
                        new JsonSerializerOptions()
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            //IgnoreReadOnlyProperties = true,
                            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            //WriteIndented = true,
                            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                        });
                }
            }
            catch { }
            if (setting == null)
            {
                setting = new Setting();
                setting.Init();
            }

            return setting;
        }

        public void Serialize(string filePath)
        {
            if (filePath.Contains(Path.DirectorySeparatorChar))
            {
                string parent = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }
            }
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    string json = JsonSerializer.Serialize(this,
                         new JsonSerializerOptions()
                         {
                             Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                             //IgnoreReadOnlyProperties = true,
                             //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                             WriteIndented = true,
                             Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                         });
                    sw.WriteLine(json);
                }
            }
            catch { }
        }
    }
}

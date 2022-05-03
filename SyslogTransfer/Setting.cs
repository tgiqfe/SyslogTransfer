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
        /// <summary>
        /// ログ転送先のSyslogサーバのアドレス
        /// 例)
        /// udp://192.168.1.100:514
        /// tcp://192.168.1.100:514
        /// </summary>
        public string SyslogServer { get; set; }

        /// <summary>
        /// アプリケーション内で送信するSyslogファシリティ
        /// </summary>
        public string SyslogFacility { get; set; }

        /// <summary>
        /// Syslog転送時のフォーマット
        /// RFC3164、RFC5424の2種類から選択可能。無指定の場合はRFC3164
        /// </summary>
        public SyslogFormat? SyslogFormat { get; set; }

        /// <summary>
        /// TCP接続時、暗号化通信を有効にするかどうか
        /// </summary>
        public bool SyslogSslEncrypt { get; set; }

        /// <summary>
        /// 暗号化通信時の接続施行タイムアウト時間(ミリ秒)
        /// </summary>
        public int? SyslogSslTimeout { get; set; }

        /// <summary>
        /// 暗号化通信時に使用する、クライアント証明書ファイルへのパス
        /// (.pfxファイル)
        /// </summary>
        public string SyslogSslCertFile { get; set; }

        /// <summary>
        /// クライアント証明書ファイルのパスワード(平文)
        /// </summary>
        public string SyslogSslCertPassword { get; set; }

        /// <summary>
        /// 証明書ストア内で使用する証明書のフレンドリ名
        /// パスワード記載したくない場合はこちらを使用することを推奨
        /// 証明書ストアは、[現在のユーザー][ローカルコンピュータ]の両方の、[個人]ストア配下を参照し、
        /// フレンドリ名が一致する証明書を使用
        /// </summary>
        public string SyslogSslCertFriendryName { get; set; }

        /// <summary>
        /// TCP接続で暗号化通信時、証明書チェックを無効化するかどうか
        /// 無効化していた場合、クライアント証明書は使用できないので注意
        /// </summary>
        public bool SyslogSslIgnoreCheck { get; set; }

        public void Init()
        {
            this.SyslogServer = "udp://localhost:514";
            this.SyslogFacility = "user";
            this.SyslogFormat = SyslogTransfer.Log.Syslog.SyslogFormat.RFC3164;
            this.SyslogSslEncrypt = false;
            this.SyslogSslTimeout = 1000;
            this.SyslogSslCertFile = null;
            this.SyslogSslCertPassword = null;
            this.SyslogSslCertFriendryName = "syslog";
            this.SyslogSslIgnoreCheck = false;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using SyslogTransfer.Lib.Syslog;
using SyslogTransfer.Logs;
using System.Reflection;
using SyslogTransfer.Lib;

namespace SyslogTransfer.PowerShell.Cmdlet
{
    [Cmdlet(VerbsCommunications.Send, "SyslogMessage")]
    internal class SendSyslogMessage : PSCmdlet
    {
        #region Public parameter

        /// <summary>
        /// Syslog送信先サーバ。
        /// udp://192.168.1.100:514 形式、もしくは、
        /// 192.168.1.154のようにIPアドレスor干すおt名のみを記述し、
        /// PortとProtocolにパラメータをセットしてもOK。
        /// </summary>
        [Parameter]
        public string Server { get; set; }

        /// <summary>
        /// Syslog送信先サーバのポート番号。
        /// Serverの値を優先
        /// </summary>
        [Parameter]
        public int Port { get; set; } = 514;

        /// <summary>
        /// Syslog送信時のプロトコル。
        /// Serverの値を優先
        /// </summary>
        [Parameter, ValidateSet("Udp", "Tcp")]
        public string Protocol { get; set; } = "Udp";

        /// <summary>
        /// Syslogデータに含む日時情報。
        /// 指定しない場合は現在日時。
        /// </summary>
        [Parameter]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Facility。Tab補完できるので、FacilityMapperは利用しない。
        /// </summary>
        [Parameter]
        public Facility? Facility { get; set; }

        /// <summary>
        /// Severity。Tab補完できるので、SeverityMapperは利用しない。
        /// </summary>
        [Parameter]
        public Severity? Severity { get; set; }

        /// <summary>
        /// Syslog送信元のホスト名
        /// </summary>
        [Parameter]
        public string HostName { get; set; }

        /// <summary>
        /// Syslog送信元のアプリケーション名
        /// </summary>
        [Parameter]
        public string AppName { get; set; }

        /// <summary>
        /// Syslog送信元のプロセスID
        /// </summary>
        [Parameter]
        public string ProcId { get; set; }

        /// <summary>
        /// SyslogメッセージID
        /// </summary>
        [Parameter]
        public string MsgId { get; set; }

        /// <summary>
        /// Syslogメッセージ本体
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Message { get; set; }

        /// <summary>
        /// Syslogフォーマット。RFC3164 or RFC5424
        /// </summary>
        [Parameter]
        public Format? Format { get; set; }

        /// <summary>
        /// SSL暗号化するかどうか
        /// </summary>
        [Parameter]
        public SwitchParameter SslEncrypt { get; set; }

        /// <summary>
        /// SSL接続時の読み取り/書き込み操作のタイムアウト。ミリ秒
        /// </summary>
        [Parameter]
        public int? SslTimeout { get; set; }

        /// <summary>
        /// SSL暗号化時に使用するクライアント証明書へのパス。PKCS12
        /// </summary>
        [Parameter]
        public string SslCertFile { get; set; }

        /// <summary>
        /// SSL暗号化時に使用する証明書のパスワード。平文なので注意
        /// </summary>
        [Parameter]
        public string SslCertPassword { get; set; }

        /// <summary>
        /// SSL暗号化時に使用する、インポート済み証明書のフレンドリ名
        /// こちらを使用する場合は証明書パスワード不要
        /// </summary>
        [Parameter]
        public string SslCertFriendryName { get; set; }

        /// <summary>
        /// Syslog送信先サーバの証明書のエラーを無視。
        /// </summary>
        [Parameter]
        public SwitchParameter SslIgnoreCheck { get; set; }

        [Parameter]
        public Lib.SyslogSession Session { get; set; }

        #endregion

        private string _currentDirectory = null;

        protected override void BeginProcessing()
        {
            //  カレントディレクトリカレントディレクトリの一時変更
            _currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = this.SessionState.Path.CurrentFileSystemLocation.Path;
        }

        protected override void ProcessRecord()
        {
            var info = new ServerInfo(Server, defaultPort: Port, defaultProtocol: Protocol);
            var msg = new SyslogMessage(
                this.Date ?? DateTime.Now,
                this.Facility ?? SyslogTransfer.Lib.Syslog.Facility.UserLevelMessages,
                this.Severity ?? SyslogTransfer.Lib.Syslog.Severity.Informational,
                this.HostName ?? Environment.MachineName,
                this.AppName ?? "SyslogTransfer.PowerShell",
                this.ProcId ?? System.Diagnostics.Process.GetCurrentProcess().Id.ToString(),
                this.MsgId ?? "-",
                this.Message);

            if (info.Protocol == "udp")
            {
                //  UDPでSyslog転送
                using (var sender = new SyslogUdpSender(info.Server, info.Port, Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164))
                {
                    sender.SendAsync(msg).Wait();
                }
            }
            else if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
            {
                //  TCPでSyslog転送
                using (SyslogSender sender = this.SslEncrypt ?
                    new SyslogTcpSenderTLS(
                        info.Server,
                        info.Port,
                        Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164,
                        SslTimeout ?? 1000,
                        SslCertFile,
                        SslCertPassword,
                        SslCertFriendryName,
                        SslIgnoreCheck) :
                    new SyslogTcpSender(
                        info.Server,
                        info.Port,
                        Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164))
                {
                    sender.SendAsync(msg).Wait();
                }
            }
            else
            {
                //  プロトコル不明
            }
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}

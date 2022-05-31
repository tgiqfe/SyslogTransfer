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
using SyslogTransfer.PowerShell.Lib;

namespace SyslogTransfer.PowerShell.Cmdlet
{
    [Cmdlet(VerbsCommon.New, "SyslogSession")]
    internal class NewSyslogSession : PSCmdlet
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
        public int? Port { get; set; }

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
        //[Parameter(Mandatory = true)]
        //public string Message { get; set; }

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

        /// <summary>
        /// コマンドレット実行時に、同時にセッション開始
        /// </summary>
        [Parameter]
        public SwitchParameter SessionStart { get; set; }

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
            var session = new SyslogSession();

            if (!string.IsNullOrEmpty(this.Server))
            {
                var info = new ServerInfo(Server, defaultPort: Port ?? 514, defaultProtocol: Protocol ?? "udp");
                session.Server = info.Server;
                session.Port = info.Port;
                session.Protocol = info.Protocol;
            }
            else if (this.Port != null)
            {
                session.Port = Port;
            }
            else if (!string.IsNullOrEmpty(this.Protocol))
            {
                session.Protocol = Protocol;
            }

            if (this.Date != null)
            {
                session.Date = Date;
            }
            if (this.Facility != null)
            {
                session.Facility = Facility;
            }
            if (this.Severity != null)
            {
                session.Severity = Severity;
            }
            if (!string.IsNullOrEmpty(this.HostName))
            {
                session.HostName = HostName;
            }
            if (!string.IsNullOrEmpty(this.AppName))
            {
                session.AppName = AppName;
            }
            if (!string.IsNullOrEmpty(this.ProcId))
            {
                session.ProcId = ProcId;
            }
            if (!string.IsNullOrEmpty(this.MsgId))
            {
                session.MsgId = MsgId;
            }
            if (this.Format != null)
            {
                session.Format = Format;
            }

            session.SslEncrypt = SslEncrypt;
            if (this.SslTimeout != null)
            {
                session.SslTimeout = SslTimeout;
            }
            if (!string.IsNullOrEmpty(this.SslCertFile))
            {
                session.SslCertFile = SslCertFile;
            }
            if (!string.IsNullOrEmpty(this.SslCertPassword))
            {
                session.SslCertPassword = SslCertPassword;
            }
            if (!string.IsNullOrEmpty(this.SslCertFriendryName))
            {
                session.SslCertFriendryName = SslCertFriendryName;
            }
            session.SslIgnoreCheck = SslIgnoreCheck;

            if (this.SessionStart)
            {
                session.Start();
            }

            WriteObject(session);
        }

        protected override void EndProcessing()
        {
            //  カレントディレクトリを戻す
            Environment.CurrentDirectory = _currentDirectory;
        }
    }
}

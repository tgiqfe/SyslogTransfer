using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Lib.Syslog;
using SyslogTransfer.Lib;

namespace SyslogTransfer.PowerShell.Lib
{
    public class SyslogSession : IDisposable
    {
        #region Parameter

        private string _server = null;
        private int? _port = null;
        private string _protocol = null;
        private DateTime? _date = null;
        private Facility? _facility = null;
        private Severity? _severity = null;
        private string _hostName = null;
        private string _appName = null;
        private string _procId = null;
        private string _msgId = null;
        private Format? _format = null;
        private bool _sslEncrypt = false;
        private int? _sslTimeout = null;
        private string _sslCertFile = null;
        private string _sslCertPassword = null;
        private string _sslCertFriendryName = null;
        private bool _sslIgnoreCheck = false;

        public string Server
        {
            get { return _server; }
            set { if (!string.IsNullOrEmpty(value)) _server = value; }
        }
        public int? Port
        {
            get { return _port; }
            set { if (value != null) _port = value; }
        }
        public string Protocol
        {
            get { return _protocol; }
            set { if (!string.IsNullOrEmpty(value)) _protocol = value; }
        }
        public DateTime? Date
        {
            get { return _date; }
            set { if (value != null) _date = value; }
        }
        public Facility? Facility
        {
            get { return _facility; }
            set { if (value != null) _facility = value; }
        }
        public Severity? Severity
        {
            get { return _severity; }
            set { if (value != null) _severity = value; }
        }
        public string HostName
        {
            get { return _hostName; }
            set { if (!string.IsNullOrEmpty(value)) _hostName = value; }
        }
        public string AppName
        {
            get { return _appName; }
            set { if (!string.IsNullOrEmpty(value)) _appName = value; }
        }
        public string ProcId
        {
            get { return _procId; }
            set { if (!string.IsNullOrEmpty(value)) _procId = value; }
        }
        public string MsgId
        {
            get { return _msgId; }
            set { if (!string.IsNullOrEmpty(value)) _msgId = value; }
        }
        public Format? Format
        {
            get { return _format; }
            set { if (value != null) _format = value; }
        }
        public bool SslEncrypt
        {
            get { return _sslEncrypt; }
            set { _sslEncrypt = value; }
        }
        public int? SslTimeout
        {
            get { return _sslTimeout; }
            set { if (value != null) _sslTimeout = value; }
        }
        public string SslCertFile
        {
            get { return _sslCertFile; }
            set { if (!string.IsNullOrEmpty(value)) _sslCertFile = value; }
        }
        public string SslCertPassword
        {
            get { return _sslCertPassword; }
            set { if (!string.IsNullOrEmpty(value)) _sslCertPassword = value; }
        }
        public string SslCertFriendryName
        {
            get { return _sslCertFriendryName; }
            set { if (!string.IsNullOrEmpty(value)) _sslCertFriendryName = value; }
        }
        public bool SslIgnoreCheck
        {
            get { return _sslIgnoreCheck; }
            set { _sslIgnoreCheck = value; }
        }

        #endregion

        public bool IsOpen { get; private set; }

        public bool Effemeral { get; private set; }

        private SyslogSender _sender = null;

        public SyslogSession() { }

        public SyslogSession(bool effemeral)
        {
            this.Effemeral = effemeral;
        }

        public void Send(string message)
        {
            _sender?.Send(
                new SyslogMessage(
                    this.Date ?? DateTime.Now,
                    this.Facility ?? SyslogTransfer.Lib.Syslog.Facility.UserLevelMessages,
                    this.Severity ?? SyslogTransfer.Lib.Syslog.Severity.Informational,
                    this.HostName ?? Environment.MachineName,
                    this.AppName ?? "SyslogTransfer.PowerShell",
                    this.ProcId ?? System.Diagnostics.Process.GetCurrentProcess().Id.ToString(),
                    this.MsgId ?? "-",
                    message));
        }

        public void Open()
        {
            if (IsOpen) { return; }

            var info = new ServerInfo(Server, defaultPort: Port ?? 514, defaultProtocol: Protocol ?? "udp");

            if (info.Protocol == "udp")
            {
                //  UDPでSyslog転送
                this.IsOpen = true;
                this._sender = new SyslogUdpSender(info.Server, info.Port, this.Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164);
            }
            else if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
            {
                //  TCPでSyslog転送
                this.IsOpen = true;
                this._sender = this.SslEncrypt ?
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
                        Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164);
            }
            else
            {
                //  プロトコル不明
            }
        }

        ~SyslogSession()
        {
            Close();
        }

        public void Close()
        {
            if (_sender != null) _sender.Dispose();
        }

        public void CloseIfEffemeral()
        {
            if (Effemeral) Close();
        }

        #region Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

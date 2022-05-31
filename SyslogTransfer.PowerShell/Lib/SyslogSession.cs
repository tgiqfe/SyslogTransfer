using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Lib.Syslog;
using SyslogTransfer.Lib;

namespace SyslogTransfer.PowerShell.Lib
{
    internal class SyslogSession : IDisposable
    {
        public bool Enabled { get; private set; }

        public string Server { get; set; }
        public int? Port { get; set; }
        public string Protocol { get; set; }
        public DateTime? Date { get; set; }
        public Facility? Facility { get; set; }
        public Severity? Severity { get; set; }
        public string HostName { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }
        public Format? Format { get; set; }
        public bool SslEncrypt { get; set; }
        public int? SslTimeout { get; set; }
        public string SslCertFile { get; set; }
        public string SslCertPassword { get; set; }
        public string SslCertFriendryName { get; set; }
        public bool SslIgnoreCheck { get; set; }

        private SyslogSender _sender = null;

        public SyslogSession() { }

        public void Start()
        {
            var info = new ServerInfo(Server, defaultPort: Port ?? 514, defaultProtocol: Protocol);

            if (info.Protocol == "udp")
            {
                //  UDPでSyslog転送
                this.Enabled = true;
                this._sender = new SyslogUdpSender(info.Server, info.Port, this.Format ?? SyslogTransfer.Lib.Syslog.Format.RFC3164);
            }
            else if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
            {
                //  TCPでSyslog転送
                this.Enabled = true;
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
            if (_sender != null)
            {
                _sender.Dispose();
            }
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

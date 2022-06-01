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
        public bool IsOpen { get; private set; }

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

        #region Set parameter methods

        public void SetServer(string server)
        {
            if (!string.IsNullOrEmpty(server)) { this.Server = server; }
        }

        public void SetPort(int? port)
        {
            if (port != null) { this.Port = port; }
        }

        public void SetProtocol(string protocol)
        {
            if (!string.IsNullOrEmpty(protocol)) { this.Protocol = protocol; }
        }

        public void SetDate(DateTime? date)
        {
            if (date != null) { this.Date = date; }
        }

        public void SetFacility(Facility? facility)
        {
            if (facility != null) { this.Facility = facility; }
        }

        public void SetSeverity(Severity? severity)
        {
            if (severity != null) { this.Severity = severity; }
        }

        public void SetHostName(string hostName)
        {
            if (!string.IsNullOrEmpty(hostName)) { this.HostName = hostName; }
        }

        public void SetAppName(string appName)
        {
            if (!string.IsNullOrEmpty(appName)) { this.AppName = appName; }
        }

        public void SetProcId(string procId)
        {
            if (!string.IsNullOrEmpty(procId)) { this.ProcId = procId; }
        }

        public void SetMsgId(string msgId)
        {
            if (!string.IsNullOrEmpty(msgId)) { this.MsgId = msgId; }
        }

        public void SetFormat(Format? format)
        {
            if (format != null) { this.Format = format; }
        }

        public void SetSslEncrypt(bool sslEncrypt)
        {
            this.SslEncrypt = sslEncrypt;
        }

        public void SetSslTimeout(int? sslTimeout)
        {
            if (sslTimeout != null) { this.SslTimeout = sslTimeout; }
        }

        public void SetSslCertFile(string sslCertFile)
        {
            if (!string.IsNullOrEmpty(sslCertFile)) { this.SslCertFile = sslCertFile; }
        }

        public void SetSslCertPassword(string sslCertPassword)
        {
            if (!string.IsNullOrEmpty(sslCertPassword)) { this.SslCertPassword = sslCertPassword; }
        }

        public void SetSslCertFriendryName(string sslCertFriendryName)
        {
            if (!string.IsNullOrEmpty(sslCertFriendryName)) { this.SslCertFriendryName = sslCertFriendryName; }
        }

        public void SetSslIgnoreCheck(bool sslIgnoreCheck)
        {
            this.SslIgnoreCheck = sslIgnoreCheck;
        }

        #endregion

        public void Start()
        {
            if (IsOpen) { return; }

            var info = new ServerInfo(Server, defaultPort: Port ?? 514, defaultProtocol: Protocol);

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

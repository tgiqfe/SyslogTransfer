using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Log.Syslog;
using System.Text.RegularExpressions;

namespace SyslogTransfer.Log
{
    internal class SyslogTransport : IDisposable
    {
        #region Server info (ip,port,protocol)

        private class ServerInfo
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public SyslogProtocol Protocol { get; set; }

            public ServerInfo() { }
            public ServerInfo(string syslogServer)
            {
                string tempServer = syslogServer;
                string tempPort = "514";

                Match match;
                if ((match = Regex.Match(tempServer, "^.+(?=://)")).Success)
                {
                    this.Protocol = match.Value switch
                    {
                        "tcp" => SyslogProtocol.TCP,
                        "udp" => SyslogProtocol.UDP,
                        _ => SyslogProtocol.UDP,
                    };
                    tempServer = tempServer.Substring(tempServer.IndexOf("://") + 3);
                }
                else
                {
                    this.Protocol = SyslogProtocol.UDP;
                }

                if ((match = Regex.Match(tempServer, @"(?<=:)\d+")).Success)
                {
                    tempPort = match.Value;
                    tempServer = tempServer.Substring(0, tempServer.IndexOf(":"));
                }
                this.Port = int.Parse(tempPort);

                this.Server = tempServer;
            }
        }

        #endregion

        private SyslogSenderBase _sender = null;

        public SyslogTransport() { }
        public SyslogTransport(Setting setting)
        {
            var info = new ServerInfo(setting.SyslogServer);
            SyslogFormat format = setting.SyslogFormat ?? SyslogFormat.RFC3164;

            if (info.Protocol == SyslogProtocol.UDP)
            {
                //  UDP
                _sender = new SyslogUdpSender(info.Server, info.Port, format);
            }
            else
            {
                if (setting.SyslogSslEncrypt)
                {
                    //  暗号化TCP
                    _sender = new SyslogTcpSenderTLS(info.Server, info.Port, setting.SyslogSslTimeout, format);
                }
                else
                {
                    //  TCP
                    _sender = new SyslogTcpSender(info.Server, info.Port, format);
                }
            }
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

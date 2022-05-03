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

        public SyslogSender Sender { get; set; }
        public Facility Facility { get; set; }
        public Severity Severity { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }
        public StructuredData[] StructuredDataParams { get; set; }

        public SyslogTransport() { }
        public SyslogTransport(Setting setting)
        {
            var info = new ServerInfo(setting.SyslogServer);
            SyslogFormat format = setting.SyslogFormat ?? SyslogFormat.RFC3164;

            this.Sender = info.Protocol == SyslogProtocol.UDP ?
                new SyslogUdpSender(info.Server, info.Port, format) :
                setting.SyslogSslEncrypt ?
                    new SyslogTcpSenderTLS(info.Server, info.Port, setting.SyslogSslTimeout, format) :
                    new SyslogTcpSender(info.Server, info.Port, format);

            /*
            if (info.Protocol == SyslogProtocol.UDP)
            {
                //  UDP
                Sender = new SyslogUdpSender(info.Server, info.Port, format);
            }
            else
            {
                if (setting.SyslogSslEncrypt)
                {
                    //  暗号化TCP
                    Sender = new SyslogTcpSenderTLS(info.Server, info.Port, setting.SyslogSslTimeout, format);
                }
                else
                {
                    //  TCP
                    Sender = new SyslogTcpSender(info.Server, info.Port, format);
                }
            }
            */
        }

        public void Write(string message)
        {
            Sender.Send(
                new SyslogMessage(
                    DateTime.Now,
                    this.Facility,
                    this.Severity,
                    Environment.MachineName,
                    this.AppName,
                    this.ProcId,
                    this.MsgId,
                    message,
                    this.StructuredDataParams));
        }

        public void Write(Severity severity, string message)
        {
            Sender.Send(
                new SyslogMessage(
                    DateTime.Now,
                    this.Facility,
                    severity,
                    Environment.MachineName,
                    this.AppName,
                    this.ProcId,
                    this.MsgId,
                    message,
                    this.StructuredDataParams));
        }

        public void Write(Facility facility,
            Severity severity,
            string hostName,
            string appname,
            string procId,
            string MsgId,
            string message,
            StructuredData[] structuredDataParams)
        {
            Sender.Send(
                new SyslogMessage(
                    DateTime.Now,
                    this.Facility,
                    severity,
                    Environment.MachineName,
                    this.AppName,
                    this.ProcId,
                    this.MsgId,
                    message,
                    this.StructuredDataParams));
        }







        public void Close()
        {
            if (Sender != null)
            {
                Sender.Dispose();
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

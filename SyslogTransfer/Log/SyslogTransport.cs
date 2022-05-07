﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Lib.Syslog;
using SyslogTransfer.Lib;
using System.Text.RegularExpressions;

namespace SyslogTransfer.Log
{
    internal class SyslogTransport : IDisposable
    {
        public bool Enabled { get; set; }

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
            var info = new ServerInfo(setting.Syslog.Server, defaultPort: 514, defaultProtocol: "udp");
            Format format = FormatMapper.ToFormat(setting.Syslog.Format);

            if (info.Protocol == "udp")
            {
                this.Enabled = true;
                this.Sender = new SyslogUdpSender(info.Server, info.Port, format);
            }
            else
            {
                if (new TcpConnect(info.Server, info.Port).TcpConnectSuccess)
                {
                    this.Enabled = true;
                    this.Sender = (setting.Syslog.SslEncrypt ?? false) ?
                        new SyslogTcpSenderTLS(
                            info.Server,
                            info.Port,
                            format,
                            setting.Syslog.SslTimeout,
                            setting.Syslog.SslCertFile,
                            setting.Syslog.SslCertPassword,
                            setting.Syslog.SslCertFriendryName,
                            setting.Syslog.SslIgnoreCheck ?? false) :
                        new SyslogTcpSender(
                            info.Server,
                            info.Port,
                            format);
                }
            }
        }

        #region Write

        public void Write(string message)
        {
            Write(DateTime.Now, this.Facility, this.Severity, Environment.MachineName, this.AppName, this.ProcId, this.MsgId, message, this.StructuredDataParams);
        }

        public void Write(Severity severity, string message)
        {
            Write(DateTime.Now, this.Facility, severity, Environment.MachineName, this.AppName, this.ProcId, this.MsgId, message, this.StructuredDataParams);
        }

        public void Write(DateTime dt, Facility facility, Severity severity, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            Sender.Send(
                new SyslogMessage(
                    dt,
                    facility,
                    severity,
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public void Write(DateTime dt, Facility facility, LogLevel level, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            Sender.Send(
                new SyslogMessage(
                    dt,
                    facility,
                    LogLevelMapper.ToSeverity(level),
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public async Task WriteAsync(string message)
        {
            await WriteAsync(DateTime.Now, this.Facility, this.Severity, Environment.MachineName, this.AppName, this.ProcId, this.MsgId, message, this.StructuredDataParams);
        }

        public async Task WriteAsync(Severity severity, string message)
        {
            await WriteAsync(DateTime.Now, this.Facility, severity, Environment.MachineName, this.AppName, this.ProcId, this.MsgId, message, this.StructuredDataParams);
        }

        public async Task WriteAsync(DateTime dt, Facility facility, Severity severity, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            await Sender.SendAsync(
                new SyslogMessage(
                    dt,
                    facility,
                    severity,
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        public async Task WriteAsync(DateTime dt, Facility facility, LogLevel level, string hostName, string appName, string procId, string msgId, string message, StructuredData[] structuredDataParams)
        {
            await Sender.SendAsync(
                new SyslogMessage(
                    dt,
                    facility,
                    LogLevelMapper.ToSeverity(level),
                    hostName,
                    appName,
                    procId,
                    msgId,
                    message,
                    structuredDataParams));
        }

        #endregion

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

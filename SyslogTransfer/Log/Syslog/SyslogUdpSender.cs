using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SyslogTransfer.Log.Syslog
{
    internal class SyslogUdpSender : IDisposable
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public SyslogFormat Format { get; set; }

        private static int _defaultPort = 514;
        private static readonly SyslogFormat _defaultFormat = SyslogFormat.RFC3164;

        private UdpClient _client = null;

        public SyslogUdpSender() { }
        public SyslogUdpSender(string server) : this(server, _defaultPort, _defaultFormat) { }
        public SyslogUdpSender(string server, int port) : this(server, port, _defaultFormat) { }
        public SyslogUdpSender(string server, int port, SyslogFormat format)
        {
            this.Server = server;
            this.Port = port;
            this.Format = format;
        }

        public void Send(SyslogMessage message)
        {
            Send(message, _defaultFormat);
        }

        public void Send(SyslogMessage message, SyslogFormat format)
        {
            _client ??= new UdpClient(Server, Port);

            byte[] datagram = format switch
            {
                SyslogFormat.RFC3164 => SyslogSerializer.GetRfc3624(message),
                SyslogFormat.RFC5424 => SyslogSerializer.GetRfc5424(message),
                _ => null,
            };
            //_client.Send(datagram, datagram.Length);

            //  デバッグ用
            Console.WriteLine(Encoding.UTF8.GetString(datagram));
        }

        public void Close()
        {
            if (_client != null)
            {
                _client.Dispose();
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

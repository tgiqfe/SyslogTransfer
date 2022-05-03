using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;

namespace SyslogTransfer.Log.Syslog
{
    /// <summary>
    /// 暗号化のみ。
    /// クライアント認証無し
    /// </summary>
    internal class SyslogTcpSenderTLS : SyslogSender
    {
        private enum MessageTransfer
        {
            OctetCouting,
            NonTransportFraming,
        }

        public int Timeout { get; set; }

        private TcpClient _client = null;
        private SslStream _stream = null;
        private MessageTransfer _messageTransfer { get; set; }

        public SyslogTcpSenderTLS() { }
        public SyslogTcpSenderTLS(string server, bool octetCouting = true) : this(server, _defaultPort, _defaultPort, _defaultFormat, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, bool octetCouting = true) : this(server, port, _defaultTimeout, _defaultFormat, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, int timeout, bool octetCouting = true) : this(server, port, timeout, _defaultFormat, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, int timeout, SyslogFormat format, bool octetCouting = true)
        {
            this.Server = server;
            this.Port = port;
            this.Timeout = timeout;
            this.Format = format;
            this._messageTransfer = octetCouting ?
                MessageTransfer.OctetCouting :
                MessageTransfer.NonTransportFraming;

            Connect();
        }


        public override void Connect()
        {
            try
            {
                _client = new TcpClient(Server, Port);
                _stream = new SslStream(_client.GetStream(), false, delegate { return true; })
                {
                    ReadTimeout = Timeout,
                    WriteTimeout = Timeout
                };
                _stream.AuthenticateAsClient(
                    Server,
                    null,
                    SslProtocols.Tls12 | SslProtocols.Tls13,
                    false);
                if (_stream.IsEncrypted)
                {
                    throw new SecurityException("Could not establish an encrypted connection.");
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public override void Disconnect()
        {
            if (_stream != null) { _stream.Dispose(); }
            if (_client != null) { _client.Dispose(); }
        }

        public override void Send(SyslogMessage message, SyslogFormat format)
        {
            if (_stream == null)
            {
                throw new IOException("No transport stream.");
            }

            using (var ms = new MemoryStream())
            {
                byte[] datagram = format switch
                {
                    SyslogFormat.RFC3164 => SyslogSerializer.GetRfc3624(message),
                    SyslogFormat.RFC5424 => SyslogSerializer.GetRfc5424_ascii(message),
                    _ => null,
                };

                if (this._messageTransfer == MessageTransfer.OctetCouting)
                {
                    byte[] messageLength = Encoding.ASCII.GetBytes(datagram.Length.ToString());
                    ms.Write(messageLength, 0, messageLength.Length);
                    ms.WriteByte(32);   //  0x20 Space
                }
                ms.Write(datagram, 0, datagram.Length);

                if (this._messageTransfer == MessageTransfer.NonTransportFraming)
                {
                    ms.WriteByte(10);   //  0xA LF
                }
                _stream.Write(ms.GetBuffer(), 0, (int)ms.Length);

                _stream.Flush();
            }
        }

        public override async Task SendAsync(SyslogMessage message, SyslogFormat format)
        {
            if (_stream == null)
            {
                throw new IOException("No transport stream.");
            }

            using (var ms = new MemoryStream())
            {
                byte[] datagram = format switch
                {
                    SyslogFormat.RFC3164 => SyslogSerializer.GetRfc3624(message),
                    SyslogFormat.RFC5424 => SyslogSerializer.GetRfc5424_ascii(message),
                    _ => null,
                };

                if (this._messageTransfer == MessageTransfer.OctetCouting)
                {
                    byte[] messageLength = Encoding.ASCII.GetBytes(datagram.Length.ToString());
                    ms.Write(messageLength, 0, messageLength.Length);
                    ms.WriteByte(32);   //  0x20 Space
                }
                ms.Write(datagram, 0, datagram.Length);

                if (this._messageTransfer == MessageTransfer.NonTransportFraming)
                {
                    ms.WriteByte(10);   //  0xA LF
                }
                await _stream.WriteAsync(ms.GetBuffer(), 0, (int)ms.Length);
                await _stream.FlushAsync();
            }
        }

        public override void Close()
        {
            Disconnect();
        }
    }
}

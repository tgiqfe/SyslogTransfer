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
    internal class SyslogTcpSenderTLS : IDisposable
    {
        private enum MessageTransfer
        {
            OctetCouting,
            NonTransportFraming,
        }


        public string Server { get; set; }
        public int Port { get; set; }
        public SyslogFormat Format { get; set; }

        private static int _defaultPort = 514;
        private static readonly SyslogFormat _defaultFormat = SyslogFormat.RFC3164;




        private string _server = null;
        private int _port = 514;
        private int _timeout = 0;
        private TcpClient _client = null;
        private SslStream _stream = null;

        private MessageTransfer _messageTransfer { get; set; }

        public SyslogTcpSenderTLS(string server, int port, int timeout, bool octetCouting = true)
        {
            this._server = server;
            this._port = port;
            this._timeout = timeout;
            this._messageTransfer = octetCouting ?
                MessageTransfer.OctetCouting :
                MessageTransfer.NonTransportFraming;

            Connect();
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient(_server, _port);
                _stream = new SslStream(_client.GetStream(), false, delegate { return true; })
                {
                    ReadTimeout = _timeout,
                    WriteTimeout = _timeout
                };
                _stream.AuthenticateAsClient(
                    _server,
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

        public void Disconnect()
        {
            if (_stream != null) { _stream.Dispose(); }
            if (_client != null) { _client.Dispose(); }
        }

        public void Send(SyslogMessage message, SyslogFormat format)
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

        public void Close()
        {
            Disconnect();
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

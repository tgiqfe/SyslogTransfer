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
using System.Threading;

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
        public string CertFile { get; set; }
        public string CertPassword { get; set; }

        private TcpClient _client = null;
        private SslStream _stream = null;
        private MessageTransfer _messageTransfer { get; set; }

        public SyslogTcpSenderTLS() { }
        public SyslogTcpSenderTLS(string server, bool octetCouting = true) : this(server, _defaultPort, _defaultFormat, _defaultTimeout, null, null, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, bool octetCouting = true) : this(server, port, _defaultFormat, _defaultTimeout, null, null, octetCouting) { }
        public SyslogTcpSenderTLS(string server, int port, SyslogFormat format, bool octetCounting = true) : this(server, port, format, _defaultTimeout, null, null, octetCounting) { }
        public SyslogTcpSenderTLS(string server, int port, SyslogFormat format, int? timeout, string certFile, string certPassword, bool octetCouting = true)
        {
            this.Server = server;
            this.Port = port;
            this.Format = format;
            this.Timeout = timeout ?? _defaultTimeout;
            this.CertFile = certFile;
            this.CertPassword = certPassword;
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
                _stream = new SslStream(_client.GetStream(), false, RemoteCertificateValidationCallback)
                {
                    ReadTimeout = Timeout,
                    WriteTimeout = Timeout
                };
                _stream.AuthenticateAsClient(
                    Server,
                    GetCollection(),
                    SslProtocols.Tls12 | SslProtocols.Tls13,
                    false);

                if (!_stream.IsEncrypted)
                {
                    throw new SecurityException("Could not establish an encrypted connection.");
                }
            }
            catch
            {
                Disconnect();
            }
        }


        private X509Certificate2Collection GetCollection()
        {
            var collection = new X509Certificate2Collection();
            //X509Certificate2 cert = new X509Certificate2(this.CertFile, this.CertPassword);
            //collection.Add(cert);


            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            //collection.AddRange(store.Certificates);

            foreach (var cert in store.Certificates)
            {
                Console.WriteLine(cert.FriendlyName);
                if (cert.FriendlyName == "syslog")
                {
                    //collection.Add(cert);
                    Console.WriteLine(cert.SubjectName.Name);

                }
            }

            return collection;
        }


        //証明書の内容を表示するメソッド
        private static void PrintCertificate(X509Certificate certificate)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("Subject={0}", certificate.Subject);
            Console.WriteLine("Issuer={0}", certificate.Issuer);
            Console.WriteLine("Format={0}", certificate.GetFormat());
            Console.WriteLine("ExpirationDate={0}", certificate.GetExpirationDateString());
            Console.WriteLine("EffectiveDate={0}", certificate.GetEffectiveDateString());
            Console.WriteLine("KeyAlgorithm={0}", certificate.GetKeyAlgorithm());
            Console.WriteLine("PublicKey={0}", certificate.GetPublicKeyString());
            Console.WriteLine("SerialNumber={0}", certificate.GetSerialNumberString());
            Console.WriteLine("===========================================");
        }

        //サーバー証明書を検証するためのコールバックメソッド
        private static Boolean RemoteCertificateValidationCallback(Object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            PrintCertificate(certificate);

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                //Console.WriteLine("Successful verification of server certificate.");
                return true;
            }
            else
            {
                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    //Console.WriteLine("ChainStatus returned a non-empty array.");
                }

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) == SslPolicyErrors.RemoteCertificateNameMismatch)
                {
                    //Console.WriteLine("Certificate names do not match.");
                }

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable)
                {
                    //Console.WriteLine("Certificate not available.");
                }

                //検証失敗とする
                return false;
            }
        }























        /*
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None || (IgnoreTLSChainErrors && sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors))
                return true;

            CertificateErrorHandler(String.Format("Certificate error: {0}", sslPolicyErrors));
            return false;
        }

        public Boolean IgnoreTLSChainErrors { get; private set; }
        public static Action<string> CertificateErrorHandler = err => { };
        */


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

                //  デバッグ用
                //Console.WriteLine(Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length));

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

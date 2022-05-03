using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace SyslogTransfer
{
    /// <summary>
    /// TCP接続可否チェック用
    /// </summary>
    internal class TcpConnect
    {
        public bool Reachable { get; set; }
        public bool Success { get; set; }

        public TcpConnect(string server, bool startTest = true)
        {
            if (startTest)
            {
                this.TestAsync(server).Wait();
            }
        }

        public TcpConnect(string server, int port, bool startTest = true)
        {
            if (startTest)
            {
                this.TestAsync(server, port).Wait();
            }
        }

        public bool Test(string server)
        {
            this.TestAsync(server).Wait();
            return Reachable;
        }

        public async Task<bool> TestAsync(string server)
        {
            int maxCount = 4;       //  最大4回チェック
            int interval = 500;     //  インターバル500ミリ秒
            Ping ping = new Ping();
            for (int i = 0; i < maxCount; i++)
            {
                PingReply reply = await ping.SendPingAsync(server);
                if (reply.Status == IPStatus.Success)
                {
                    this.Reachable = true;
                    return true;
                }
                await Task.Delay(interval);
            }
            this.Reachable = false;
            return false;
        }

        public bool Test(string server, int port)
        {
            this.TestAsync(server, port).Wait();
            return this.Success;
        }

        public async Task<bool> TestAsync(string server, int port)
        {
            using (var client = new TcpClient())
            {
                int timeout = 3000;
                try
                {
                    Task task = (client.ConnectAsync(server, port));
                    if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
                    {
                        throw new SocketException(10060);
                    }
                }
                catch { }

                this.Success = client.Connected;
                return client.Connected;
            }
        }
    }
}

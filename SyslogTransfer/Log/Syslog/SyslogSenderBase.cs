using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyslogTransfer.Log.Syslog
{
    internal class SyslogSenderBase : IDisposable
    {
        protected const int _defaultPort = 514;
        protected static readonly SyslogFormat _defaultFormat = SyslogFormat.RFC3164;
        protected const int _defaultTimeout = 1000;

        public virtual void Connect() { }
        public virtual void Disconnect() { }
        public virtual void Send(SyslogMessage message) { Send(message, _defaultFormat); }
        public virtual void Send(SyslogMessage message, SyslogFormat format) { }
        public virtual async Task SendAsync(SyslogMessage message) { await SendAsync(message, _defaultFormat); }
        public virtual async Task SendAsync(SyslogMessage message, SyslogFormat format) { await Task.Run(() => { }); }
        public virtual void Close() { }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogTransfer.Lib.Syslog;

namespace SyslogTransfer.Cmd
{
    internal class SyslogTransferOption
    {
        public string Server { get; set; }          //  -s
        public string Port { get; set; }            //  -p  ※このオプション不採用
        public string Protocol { get; set; }        //  -p  ※このオプション不採用
        public DateTime? Date { get; set; }         //  -d
        public Facility? Facility { get; set; }     //  -f
        public Severity? Severity { get; set; }     //  -s
        public string HostName { get; set; }        //  -h
        public string AppName { get; set; }         //  -a
        public string ProcId { get; set; }          //  -i
        public string MsgId { get; set; }           //  -t
        public string Message { get; set; }         //  (引数無し)
        public Format? Format { get; set; }         //  -m
        public SslEncryptParameter Encrypt { get; set; }    //  -e

        public class SslEncryptParameter
        {
            //  記述例)
            //  -e "timeout:10,cert:"C:\Users\Desktop\Sample example test\cert.crt",password:1234,friendryName:syslog,sslIgnoreCheck:true"
            public bool SslEncrypt { get; set; }            //  (↓が指定されている場合にtrue)
            public int? SslTimeout { get; set; }            //  timeout:****
            public string SslCertFile { get; set; }         //  cert:****.crt
            public string SslCertPassword { get; set; }     //  password:*******
            public string SslCertFriendryName { get; set; } //  friendryName:********
            public bool SslIgnoreCheck { get; set; }        //  sslIgnoreCheck:***
        }
    }
}

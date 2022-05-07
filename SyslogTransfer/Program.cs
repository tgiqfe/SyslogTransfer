﻿using SyslogTransfer;
using SyslogTransfer.Log;
using SyslogTransfer.Lib.Syslog;

bool save = false;
if (save)
{
    var settingTemp = Setting.Deserialize("setting.json");
    settingTemp.Serialize("setting.json");
    Console.ReadLine();
    Environment.Exit(0);
}


var setting = Setting.Deserialize("setting.json");

using (var syslog = new SyslogTransport(setting))
{
    syslog.Facility = FacilityMapper.ToFacility(setting.Syslog.Facility);
    syslog.Severity = Severity.Informational;
    syslog.AppName = "AppName-Tag";
    syslog.ProcId = "Process-ID";
    syslog.MsgId = "Message-ID";

    //syslog.WriteAsync("Message Content. [" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "]").ConfigureAwait(false);
    syslog.Write("MessageContent. to " + setting.Syslog.Server);
}





Console.ReadLine();


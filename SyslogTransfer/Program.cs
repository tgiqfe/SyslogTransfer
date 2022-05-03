using SyslogTransfer;
using SyslogTransfer.Log;
using SyslogTransfer.Log.Syslog;

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

}





Console.ReadLine();


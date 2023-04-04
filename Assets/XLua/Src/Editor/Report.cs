#if UNITY_5_6_OR_NEWER

namespace XLua
{
    using UnityEngine;
    using UnityEditor;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    [InitializeOnLoad]
    public class Report
    {
        static Report()
        {
            var version = "2.1.16";
            var engine = Application.unityVersion;
            var machine = SystemInfo.deviceUniqueIdentifier;
            var msg = string.Format("cmd=0&tag=glcoud.xlua.report&version={0}&engine={1}&machine_name={2}", version, engine, machine);

            new Thread(() =>
            {
                var data = Encoding.UTF8.GetBytes(msg);
                var client = new UdpClient();
                client.Send(data, data.Length, "101.226.141.148", 8080);
                client.Close();
            }).Start();
        }
    }
}

#endif

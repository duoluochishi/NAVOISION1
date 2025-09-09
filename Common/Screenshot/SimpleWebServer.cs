using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Screenshot
{
    public class SimpleWebServer
    {
        Server server = new Server();
        string _strIP = "0.0.0.0";
        int _intPort = 30070;
        string _strPath = @"WebRoot\"; 
        public SimpleWebServer()
        {
        }
        public string strIP { get => _strIP; set => _strIP = value; }
        public int intPort { get => _intPort; set => _intPort = value; }
        public string strPath { get => _strPath; set => _strPath = value; }
        public bool Start(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            IPAddress ipAddress = IPAddress.Parse(_strIP);
            return server.start(ipAddress, _intPort, 100, dirPath);
        }
        public void Stop()
        {
            server.stop();
        }
    }
}

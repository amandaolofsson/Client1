using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Client1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string adress = "127.0.0.1";
                int port = 800;
                Console.WriteLine("Connecting...");
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(adress, port);
                Console.WriteLine("Connected.");
                NetworkStream tcpStream = tcpClient.GetStream();

                Session s = new Session(tcpStream);
                s.Run();
            }
            catch (DisconnectExceptions)
            {
                Console.WriteLine("Disconnected");
            }
            
        }

        

    }
}

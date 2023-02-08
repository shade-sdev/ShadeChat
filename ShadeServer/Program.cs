using ShadeServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ShadeServer
{
    internal class Program

    {
        static void Main(string[] args)
        {
            new Server().Start();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }
    }
}

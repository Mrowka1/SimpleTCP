using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Obsluga_Siecix3
{
    class MainBehaviour
    {
        TCPClientSide clienthndl;
        TCPServerSide serverhndl;
        public MainBehaviour()
        {
            
            Console.WriteLine("> "+"Główna klasa aplikacji zainicjowana!");
            string command = Console.ReadLine();
            if (command.Equals("s"))
            {
                serverhndl = new TCPServerSide();
                serverhndl.Init(GetLocalIPAddress(), 7777);
            }
            else if (command.Equals("c"))
            {
                clienthndl = new TCPClientSide();
                clienthndl.OnConnect += Okokok;
                clienthndl.ReceivedServerMessage += Client_ReceivedServerMessage;
         //       clienthndl.OnDisconnected += Kokoko;
                clienthndl.ConnectTo("localhost", 7777);
                while (true)
                {
                    command = Console.ReadLine();
                    Console.WriteLine(">> "+command);
                    clienthndl.SendMessage(command);
                    Console.WriteLine("> "+" wysyłanie: "+command);
                }
            }
            //  Console.ReadKey();
        }
        void Client_ReceivedServerMessage(string m)
        {
            Console.WriteLine(m);
        }

        void Okokok()
        {
            Console.WriteLine("połączono");
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}

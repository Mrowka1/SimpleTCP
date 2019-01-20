using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;



/*Test całości*/

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
                serverhndl.E_StartedServer += ServerStarted;
                serverhndl.E_OnReceivedTCPMessage += ReceivedMessage;
                

                serverhndl.Init(GetLocalIPAddress(), 7777);
                while (true)
                {
                    command = Console.ReadLine();
                    if (command.Equals("stop"))
                    {
                        serverhndl.StopServer();
                    }
                }
            }
            else if (command.Equals("c"))
            {
                clienthndl = new TCPClientSide();
                clienthndl.OnConnect += Okokok;
                clienthndl.ReceivedServerMessage += Client_ReceivedServerMessage;
                clienthndl.OnDisconnected += (s) =>  { Console.WriteLine("Rozłączono "+s.ToString()); };

                clienthndl.ConnectTo(Console.ReadLine(), 7777);
                while (true)
                {
                    command = Console.ReadLine();
                    Console.WriteLine(">> "+command);
                    clienthndl.SendMessage(command);
                    Console.WriteLine("> "+" wysyłanie: "+command);
                }
            }
        }
     
        void ServerStarted()
        {

        }
        void ReceivedMessage(TcpClient client, string message)
        {
            Console.WriteLine("["+client.Client.RemoteEndPoint.ToString()+"] : "+message);

        }
        void Client_ReceivedServerMessage(string m)
        {
            Console.WriteLine(m);
        }

        void Okokok(Boolean success, string error)
        {
            if (success) Console.WriteLine("połączono");
            else Console.WriteLine("Problem z połączeniem: "+error);
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
            throw new Exception("Brak poprawnego interfejsu sieciowego!");
        }


        //
    }
}

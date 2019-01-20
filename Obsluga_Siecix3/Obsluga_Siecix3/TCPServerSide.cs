using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;


namespace Obsluga_Siecix3
{
    class TCPServerSide
    {

        public delegate void OnReceivedTCPMessage(TcpClient client, string message);
        public event OnReceivedTCPMessage E_OnReceivedTCPMessage;

        public delegate void StartedServer();
        public event StartedServer E_StartedServer;


        int _port { get; set; }
       TcpListener ServerListener;
       System.Timers.Timer EchoClientsTimer;
       public struct ClientInst
        {
           public TcpClient Client;
            public int lastresponse;
        }
        List<ClientInst> clients = new List<ClientInst>();
        Thread MainServerThread;

        bool ServerAlive = false;

        void PrepareTimer()
        {
            EchoClientsTimer = new System.Timers.Timer(2000);
            EchoClientsTimer.Elapsed += EchoClients;
            EchoClientsTimer.AutoReset = true;
            EchoClientsTimer.Enabled = false;
        }

        public void Init(string ip, int port)
        {
            
            _port = port;
            PrepareTimer();
            Console.WriteLine("> "+"Uruchamianie serwera TCP...");
            try
            {
                ServerListener = new TcpListener(IPAddress.Parse(ip), port);
                ServerListener.Start();
                ServerAlive = true;
                Console.WriteLine("> "+"Uruchomiono serwer TCP: "+ip+":"+port);
                Console.WriteLine("> " + "Oczekiwanie na połączenia...");
                MainServerThread = new Thread(MainLoop);
                MainServerThread.Start();
                EchoClientsTimer.Enabled = true;
                E_StartedServer();

            }
            catch
            {
                Console.WriteLine("> "+"Nie można uruchomić serwera TCP");
                Console.ReadKey();
            }
        }

       public void StopServer()
        {
            for(int i = 0; i < clients.Count(); i++)
            {
                DisconnectClient(i);
            }
            ServerAlive = false;
            ServerListener.Stop();
            MainServerThread.Abort();
            ServerListener = null;
        }

        private void EchoClients(Object source, ElapsedEventArgs e)
        {
            if(clients.Count == 0) return;
            for (int i = 0; i < clients.Count; i++)
            {
                ClientInst tmp = clients[i];
                tmp.lastresponse += 2;
                clients[i] = tmp;
                if (tmp.lastresponse > 10)
                {
                    DisconnectClient(i);
                    return;
                }
                SendMessage(tmp.Client, "ECHO");
            }
        }

        bool SendMessage(TcpClient client, string message)
        {
            
            try
            {
                NetworkStream stream = client.GetStream();
                if (stream.CanWrite)
                {
                    Console.WriteLine("Sending: " + message + " | to: " + client.Client.RemoteEndPoint.ToString());
                    Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    return true;
                }
                else return false;
            }
            catch
            {
                Console.WriteLine("NIE WYSŁANO WIADOMOŚĆI DO: "+client.Client.RemoteEndPoint.ToString());
                return false;
            }
        }

        void DisconnectClient(int id)
        {
       
            TcpClient tmp = clients[id].Client;
            if (!tmp.Connected)
            {
                try
                {
                    SendMessage(tmp, "DISCONNECT");

                    tmp.GetStream().Close();
                    tmp.Close();
                }
                catch { }
            }
            clients.RemoveAt(id);
        }

        private void MainLoop()
        {
            while (ServerAlive)
            {
                AcceptClients();
                ChceckForIncomingData();
            }
        }

        private void ChceckForIncomingData()
        {
            foreach(ClientInst element in clients.ToList())
            {
                try
                {
                    NetworkStream stream;
                    if (element.Client.Connected) stream = element.Client.GetStream();
                    else continue;
                    if (stream.CanRead && stream.DataAvailable)
                    {
                        Byte[] data = new Byte[256];

                        String responseData = String.Empty;
                        int bytes = stream.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                        ReceiveMessage(element.Client, responseData);
                    }
                }
                catch(SocketException e)
                {
                    Console.WriteLine("Treść błędu: "+e.ToString());
                }
            }
        }

        void ReceiveMessage(TcpClient client, string message)
        {
            E_OnReceivedTCPMessage(client, message);
            if (message.Equals("ECHO"))
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (client.Equals(clients[i].Client))
                    {
                        ClientInst tmp = clients[i];
                        tmp.lastresponse = 0;
                        clients[i] = tmp;
                        break;
                    }
                }
            }
        }

        private void AcceptClients()
        {


            if (ServerListener.Pending())
                {
                    TcpClient client = ServerListener.AcceptTcpClient();

                var tmp = new ClientInst
                {
                    Client = client,
                    lastresponse = 0
                };

                clients.Add(tmp);

                   Console.WriteLine("> "+"Connected "+client.Client.RemoteEndPoint.ToString());
                    int count = 0;
                    foreach (ClientInst element in clients)
                    {
                        count++;
                        Console.Write("["+count+"]"+element.Client.Client.RemoteEndPoint.ToString()+" ");
                    }
                    Console.WriteLine(); 
            }
            else
            {
                return;
            }

        }
        void CloseServer()
        {
            EchoClientsTimer.Stop();
            EchoClientsTimer.Dispose();
        }
    }
}

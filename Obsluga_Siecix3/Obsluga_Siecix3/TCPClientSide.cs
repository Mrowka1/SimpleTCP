using System;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Diagnostics;

namespace Obsluga_Siecix3
{
    class TCPClientSide
    {
        TcpClient LocalClient;
        Thread DataReceivingThread;
        System.Timers.Timer EchoClientsTimer;
        int LastResponse = 0;

        bool connected = false;

        public delegate void OnConnectedToServer(Boolean success, string error);
        public event OnConnectedToServer OnConnect;

        public delegate void OnRemoteDisconnect(DisconnectReason r);
        public event OnRemoteDisconnect OnDisconnected;

        public delegate void OnReceivedTCPMessage(string message);
        public event OnReceivedTCPMessage ReceivedServerMessage;
        public enum DisconnectReason : byte { TimeOut=1, Request=2 };


        void PrepareTimer()
        {
            EchoClientsTimer = new System.Timers.Timer(2000);
            EchoClientsTimer.Elapsed += EchoClients;
            EchoClientsTimer.AutoReset = true;
            EchoClientsTimer.Enabled = false;
        }

        void EchoClients(Object source, ElapsedEventArgs e)
        {

            LastResponse += 2;
           // Console.Beep();
            Debug.WriteLine("Ostatnia odpowiedź " + LastResponse + " sekund temu.");
            if (LastResponse > 10)
            {
                Debug.WriteLine("> " + "Utracono połączenie z serwerem! " + connected.ToString());
                // EchoClientsTimer.Dispose();
                Disconnected(DisconnectReason.TimeOut);


            }
        }

        public TCPClientSide()
        {
        }

        public void ConnectTo(string serverIP, int serverPort)
        {
            PrepareTimer();
            Debug.WriteLine("> " + "Próba połączenia z " + serverIP + ":" + serverPort);
            try
            {
                LocalClient = new TcpClient(serverIP, serverPort);
                Debug.WriteLine("> " + "Połączono z serverem TCP");
                connected = true;
                DataReceivingThread = new Thread(AcceptDataFromServer);
                DataReceivingThread.Start();
                EchoClientsTimer.Enabled = true;
                OnConnect(true, "ok");
            }
            catch (SocketException s)
            {
                OnConnect(false, s.ToString());
                Debug.WriteLine("> " + "nie można się połączyć z " + serverIP + ":" + serverPort + " - " + s.ToString());
            }
        }

        private void AcceptDataFromServer()
        {
            NetworkStream stream;
            Debug.WriteLine("> " + "Wątek zainicjowany. Oczekiwanie na dane od serwera TCP");
            stream = LocalClient.GetStream();

            while (connected)
            {
            
                Byte[] data = new Byte[256];

                String responseData = String.Empty;


                if (stream.CanRead && connected)
                {

                    try
                    {
                        Int32 bytes = stream.Read(data, 0, data.Length);
                        responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
                        ReceiveMessage(responseData);
                    }
                    catch { }
                    finally
                    {
                        data = null;
                    }     
                }
            }
            Reset();

        }

        void ReceiveMessage(string message)
        {
            if (message.Equals("ECHO"))
            {
                LastResponse = 0;
                SendMessage("ECHO");
            }
            else if (message.Equals("DISCONNECT"))
            {
                Disconnected(DisconnectReason.Request);
            }
            else
            {
                ReceivedServerMessage(message);
            }
        }

        void Reset()
        {
            connected = false;
            EchoClientsTimer.Dispose();
            LastResponse = 0;
            DataReceivingThread.Abort();

            try { LocalClient.GetStream().Close(); }catch { }
            LocalClient.Close();

            EchoClientsTimer = null;
            LocalClient = null;
            DataReceivingThread = null;
        }

        void Disconnected(DisconnectReason request)
        {   
            Reset();
            Debug.WriteLine("Odebrano żądanie zerwania połączenia.");
            OnDisconnected(DisconnectReason.Request);
        }

        public void SendMessage(string message)
        {
            if (!connected) return;


            String responseData = String.Empty;
            NetworkStream stream;
            stream = LocalClient?.GetStream();


     
                try
                {
                    Byte[] data = new Byte[256];
                    data = System.Text.Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch
                {

                }
            
        }




    }
}

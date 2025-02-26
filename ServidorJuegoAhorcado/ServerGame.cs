using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServidorJuegoAhorcado
{

    internal class ServerGame
    {
        public List<string> words = new List<string>();
        public List<Record> records = new List<Record>();
        public string wordsPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), "words.txt");
        public string recordPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), "recordsAhorcado.txt");
        public int port = 31416;
        public int portInitial = 1025;
        public int portFinal = 655354;
        public string ipServer = "127.0.0.1";
        Socket socketServer;
        Socket socketClient;
        public static Random random = new Random();
        static void Main(string[] args)
        {
            ServerGame server = new ServerGame();
            server.LoadWords(server.wordsPath);
            server.InitServer();
        }
        public void InitServer()
        {
            //comprobacion de puertos y ip
            if (CheckPort(port) == -1)
            {
                return;
            }
            else
            {
                port = CheckPort(port);
            }
            //creacion de ip-puerto
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, port);

            //creacion de socket
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                //enlance 
                socketServer.Bind(ie);
                //escucha y en espera a clientes
                socketServer.Listen(10);
                Console.WriteLine("SERVER WAITING");

                while (true)
                {
                    socketClient = socketServer.Accept();
                    Console.WriteLine($"Client connected from {socketClient.RemoteEndPoint}:{port}");

                    Thread clientThread = new Thread(() => HandlerClient(socketClient));
                    clientThread.IsBackground = true;
                    clientThread.Start();

                }

            }
            catch (Exception e) when (e is IOException | e is SocketException | e is ArgumentException)
            {
                Console.WriteLine($"[DEBUG] {e.Message}");
                throw;
            }
        }
        public void HandlerClient(object socket)
        {
            Socket clientSocket = socket as Socket;
            IPEndPoint ieClient = clientSocket.RemoteEndPoint as IPEndPoint;

            try
            {
                using (NetworkStream network = new NetworkStream(clientSocket))
                using (StreamReader sr = new StreamReader(network))
                using (StreamWriter sw = new StreamWriter(network))
                {
                    sw.WriteLine("WELCOME TO HANGED MAN GAME SERVER :D .Enter command (getword,sendword ,getrecords, sendrecord, closeserver) ");
                    sw.Flush();

                    string command = sr.ReadLine();
                    if (string.IsNullOrEmpty(command))
                    {
                        sw.WriteLine("Wrong command. Disconnecting....");
                        sw.Flush();
                        clientSocket.Close();
                        return;
                    }

                    string[] partsCommand = command.Split(' ');

                    switch (command)
                    {
                        case "getword":
                            sw.WriteLine($"{getWord()}");
                            sw.Flush();
                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("sendword"):
                                
                            
                            break;
                        case "getrecords":

                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("sendrecord"):


                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("serverclose"):


                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception e) when (e is IOException | e is SocketException | e is ArgumentException)
            {

                throw;
            }
        }

        public string getWord()
        {
            return words[random.Next(0, words.Count - 1)];
        }

        public int CheckPort(int portCheck)
        {
            if (CheckPortAvaliable(portCheck))
            {
                return portCheck;
            }
            //otra forma teniendo array de ports...
            for (int i = portInitial; i < portFinal; i++)
            {
                if (CheckPortAvaliable(i))
                {
                    return i;
                }
            }
            return -1;
        }


        public bool CheckPortAvaliable(int portCheck)
        {
            try
            {
                IPEndPoint ie = new IPEndPoint(IPAddress.Any, portCheck);
                using (Socket testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    testSocket.Bind(ie);
                }
                return true;

            }
            catch (Exception e) when (e is SocketException | e is ArgumentException)
            {
                return false;
                throw;
            }
        }

        public void LoadRecords(string pathFile)
        {
            if (!File.Exists(pathFile))
            {
                return;
            }
            try
            {
                using (BinReaderRecord reader = new BinReaderRecord(new FileStream(pathFile, FileMode.Open)))
                {
                    records.Add(reader.ReadRecord());
                }
            }
            catch (Exception e) when (e is IOException | e is ArgumentException | e is ArgumentNullException)
            {

                throw;
            }
        }
        public void LoadWords(string pathFile)
        {
            if (!File.Exists(pathFile))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(pathFile))
                {
                    string data = reader.ReadToEnd().ToUpper();
                    words = data.Split(',').ToList();
                }
            }
            catch (Exception e) when (e is IOException | e is ArgumentException | e is ArgumentNullException)
            {
                Console.WriteLine($"[DEBUG] Error open file : {e.Message}");
            }
        }
    }
}

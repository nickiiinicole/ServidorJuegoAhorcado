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
        public string recordPath = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), "recordsAhorcado.bin");
        public int port = 31416;
        public int portInitial = 1025;
        public int portFinal = 655354;
        public string ipServer = "127.0.0.1";
        Socket socketServer;
        Socket socketClient;
        public static Random random = new Random();
        private int pin = 2439;
        static void Main(string[] args)
        {

            ServerGame server = new ServerGame();

            //server.SaveRecord(new Record("nic", 100), server.recordPath);
            //server.SaveRecord(new Record("car", 150), server.recordPath);
            //server.SaveRecord(new Record("adr", 160), server.recordPath);
            server.LoadWords(server.wordsPath);
            server.LoadRecords(server.recordPath);
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
                    sw.WriteLine("WELCOME TO HANGED MAN GAME SERVER :D .Enter command (getword,sendword ,getrecords, sendrecord, serverclose) ");
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
                            clientSocket.Close();
                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("sendword"):

                            if (SendWord(partsCommand[1], wordsPath))
                            {
                                sw.WriteLine("OK");
                                sw.Flush();
                            }
                            else
                            {
                                sw.WriteLine("ERROR");
                                sw.Flush();
                            }
                            clientSocket.Close();
                            break;
                        case "getrecords":
                            sw.WriteLine("RECORDS LIST:");
                            sw.Flush();
                            sw.WriteLine(getRecords());
                            sw.Flush();
                            clientSocket.Close();
                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("sendrecord"):
                            //la manera que veo que es un record , una cadena donde va primero el  nombre separado por ; y segundos .

                            string[] dataRecord = partsCommand[1].Split(';');
                            if (dataRecord.Length != 2)
                            {
                                sw.WriteLine("REJECT");
                                sw.Flush();
                                clientSocket.Close();
                            }
                            if (int.TryParse(dataRecord[1], out int seconds))
                            {
                                Record record = new Record(dataRecord[0], seconds);
                                if (SendRecord(record))
                                {
                                    sw.WriteLine("ACCEPT");
                                    sw.Flush();
                                    clientSocket.Close();
                                }
                            }
                            sw.WriteLine("REJECT");
                            sw.Flush();
                            clientSocket.Close();
                            break;
                        case string com when partsCommand.Length == 2 && partsCommand[0].StartsWith("serverclose"):
                            if (int.TryParse(partsCommand[1], out int pinSend) && pinSend == pin)
                            {
                                clientSocket.Close();
                                socketServer.Close();
                                return;
                            }
                            break;

                        default:
                            sw.WriteLine("Unknown command. Disconnecting...");
                            sw.Flush();
                            clientSocket.Close();
                            break;
                    }

                }
            }
            catch (Exception e) when (e is IOException | e is SocketException | e is ArgumentException)
            {

                Console.WriteLine($"[DEBUG] {e.Message}");
            }
        }
        public bool SendRecord(Record recordSave)
        {
            lock (this)
            {
                if (records.Count < 3)
                {
                    records.Add(recordSave);
                    sortRecords();
                    return true;
                }
                sortRecords();//de ewsta manera los tengo ordenados , y comparo con el ulitmo directasmente.
                if (recordSave.Seconds < records[2].Seconds)
                {
                    records[2] = recordSave;
                    sortRecords();
                    return true;

                }

            }
            return false;
        }


        public void sortRecords()
        {


            // Ordenamos de menor a mayor, ya que queremos que el mejor récord esté al principio y asi comprarar direct
            records.Sort((record1, record2) => record1.Seconds.CompareTo(record2.Seconds));

        }

        public String getRecords()
        {
            StringBuilder listRecords = new StringBuilder();

            lock (this)
            {

                foreach (Record record in records)
                {
                    listRecords.AppendLine($"-Name: {record.Name} , Score : {record.Seconds} ");
                }
            }
            return listRecords.ToString();
        }

        public string getWord()
        {
            return words[random.Next(0, words.Count)];
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
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        records.Add(reader.ReadRecord());
                    }
                }

                //ordenar nuestro list para que ya quede ordenador de mayor puntuacion a menor
                records.Sort((record1, record2) => record1.Seconds.CompareTo(record2.Seconds));
            }
            catch (Exception e) when (e is IOException | e is ArgumentException | e is ArgumentNullException)
            {

                Console.WriteLine($"[DEBUG]{e.Message}");
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



        public bool SendWord(string word, string pathFile)
        {
            if (!File.Exists(pathFile))
            {
                return false;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(pathFile, true))
                {
                    writer.Write($"{word},");
                }
                return true;
            }
            catch (Exception e) when (e is IOException | e is ArgumentNullException | e is ArgumentException)
            {
                Console.WriteLine($"[DEBUG] {e.Message}");
                return false;
            }
        }

        public bool SaveRecord(Record record, string pathFile)
        {
            if (!File.Exists(pathFile))
            {
                return false;
            }

            try
            {
                using (BinWriterRecords writer = new BinWriterRecords(new FileStream(pathFile, FileMode.Append)))
                {

                    writer.WriteRecord(record);

                }
                return true;
            }
            catch (Exception e) when (e is IOException | e is ArgumentException | e is ArgumentNullException)
            {
                Console.WriteLine($"[DEBUG] {e.Message}");
                return false;
            }
        }
    }
}

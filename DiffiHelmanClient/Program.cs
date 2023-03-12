using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace DiffiHelmanClient
{
    class Program
    {
        private const string LocalHost = "localhost";
        private static int Port;
        private static Parameters Parameters;
        private static Random random;
        private static long MyX;
        static void Main(string[] args)
        {
            random = new Random();
            try
            {
                GetParametersFromArgs(args);
                CalculateKey();
                
                var client = GetClient();
                Trace.WriteLine($"Подключение к {Port} установлено");
                
                SendMessage(client);
                Trace.WriteLine($"Сообщение отправленно порту: {Port}");
                
                var response = GetResponse(client);
                Trace.WriteLine($"Получили ответ от порта: {Port}");

                var answer = PrepareAnswer(response);
                Console.WriteLine($"Ответ: {answer}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static long PrepareAnswer(string response)
        {
            var b = long.Parse(response);
            var p = int.Parse(Parameters.p);
            return (long)Math.Pow(b, MyX) % p;
        }

        private static string GetResponse(Socket client)
        {
            var responseBytes = new byte[512];
            var bytes = client.Receive(responseBytes);
            string response = Encoding.UTF8.GetString(responseBytes, 0, bytes);
            return response;
        }

        private static void SendMessage(Socket client)
        {
            var message = JsonSerializer.Serialize(Parameters);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            client.Send(messageBytes);
        }

        private static Socket GetClient()
        {
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(LocalHost, Port);
            return client;
        }

        private static void CalculateKey()
        {
            try
            {
                var g = long.Parse(Parameters.g);
                var p = int.Parse(Parameters.p);
                MyX = GetRandomSimpleNumber(p - 1);
                Parameters.X = ((long)Math.Pow(g, MyX) % p).ToString();
                
            }
            catch (Exception)
            {
                throw new ArgumentException("Введены некоректные g или/и p!");
            }
        }

        private static int GetRandomSimpleNumber(int p)
        {
            while (true)
            {
                var counter = 0;
                var rndInt = random.Next(2, p);
                for (int i = 2; i < rndInt / 2 + 1; i++)
                {
                    if (rndInt % i == 0)
                    {
                        counter++;
                    }
                }
                if (counter == 0)
                {
                    Trace.WriteLine(rndInt);
                    return rndInt;
                }
            }
            
        }

        private static void GetParametersFromArgs(string[] args)
        {
            var filepath = string.Empty;
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-l":
                        Port = int.Parse(args[i + 1]);
                        break;
                    case "-p":
                        filepath = args[i + 1];
                        break;
                    default:
                        throw new ArgumentException("Введены некоректные данные!");
                }
            }
            if (string.IsNullOrWhiteSpace(filepath))
            {
                throw new ArgumentException("Нужно ввести параметры для алгоритма!");
            }
            var line = File.ReadAllText(filepath);
            Parameters = JsonSerializer.Deserialize<Parameters>(line);
            if (Port == 0
                || Parameters == null
                || String.IsNullOrWhiteSpace(Parameters.g)
                || String.IsNullOrWhiteSpace(Parameters.p))
            {
                throw new ArgumentException("Введены некоректные данные, параметры или порт отсутствуют!");
            }
        }
    }
}

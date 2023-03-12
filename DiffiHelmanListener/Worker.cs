using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DiffiHelmanClient;
using Microsoft.Extensions.Hosting;

namespace DiffiHelmanListener
{
    public class Worker : BackgroundService
    {
        private static readonly Random random = new Random();
        private static int MyX;
        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
            using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpListener.Bind(ipPoint);
            tcpListener.Listen(1000);    

            while (!stoppingToken.IsCancellationRequested)
            {
                using var tcpClient = await tcpListener.AcceptAsync();
                Parameters parameters = GetRequest(tcpClient);
                
                var B = CalculateKey(parameters);

                var bytes = Encoding.UTF8.GetBytes(B);
                tcpClient.Send(bytes);

                var b = int.Parse(parameters.X);
                var p = int.Parse(parameters.p);
                var result = ((long)Math.Pow(b, MyX) % p);
                Console.WriteLine(result);
            }   
        }

        private static Parameters GetRequest(Socket tcpClient)
        {
            byte[] data = new byte[512];
            var bytes = tcpClient.Receive(data);
            string request = Encoding.UTF8.GetString(data, 0, bytes);
            var parameters = JsonSerializer.Deserialize<Parameters>(request);
            return parameters;
        }

        private static string CalculateKey(Parameters parameters)
        {
            try
            {
                MyX = GetRandomSimpleNumber();
                var g = int.Parse(parameters.g);
                var p = int.Parse(parameters.p);
                var temp = (Math.Pow(g, MyX) % p);
                var answer = temp.ToString();
                return answer;
            }
            catch (Exception)
            {
                throw new ArgumentException("¬ведены некоректные g или/и p!");
            }
        }

        private static int GetRandomSimpleNumber()
        {
            while (true)
            {
                var counter = 0;
                var rndInt = random.Next(1, 30);
                for (int i = 2; i < Math.Sqrt(rndInt); i++)
                {
                    if (rndInt % i == 0)
                    {
                        counter++;
                    }
                }
                if (counter == 0)
                {
                    return rndInt;
                }
            }
        }
    }
}

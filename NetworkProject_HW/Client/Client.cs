using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public static class Client
    {
        private static IPEndPoint local = new IPEndPoint(IPAddress.Any, 0);
        private static TcpClient client = new TcpClient(local);
        private static InfoAboutClient? info;

        public static void Start(IPEndPoint removeEndPoint, string name)
        {
            try
            {
                //Залушка для имени. Выбирается рандомное значение из перечисления Names
                Names namePlaceholder = (Names)new Random().Next(0, Enum.GetValues(typeof(Names)).Length);

                info = new InfoAboutClient() { Name = namePlaceholder.ToString() };
                string json = JsonSerializer.Serialize(info);

                client.Connect(removeEndPoint);

                Console.WriteLine("Подключен");

                Chat();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Подключение не удалось.");
            }
            finally
            {
                client.Close();
            }
        }

        private static async Task Chat()
        {
            string json = JsonSerializer.Serialize<InfoAboutClient>(info);
            using (StreamReader reader = new StreamReader(client.GetStream()))
            {
                using (StreamWriter writer = new StreamWriter(client.GetStream()))
                {
                    writer.WriteLine(json);
                    writer.Flush();

                    while (true)
                    {
                        Task.Run(() => GetMessage(reader));

                        Console.Write("Введите сообщение: ");
                        string? msg = Console.ReadLine();

                        await writer.WriteLineAsync(msg);
                        writer.Flush();
                        Console.WriteLine("Сообщение отправлено");
                    }
                }
            }
        }
        private static async Task GetMessage(StreamReader reader)
        {
            while (true)
            {
                if (reader.BaseStream.CanRead)
                {
                    string? msg = await reader.ReadLineAsync();
                    Console.WriteLine("Получено сообщение");
                    Console.WriteLine(msg);
                }
                else
                {
                    await Console.Out.WriteLineAsync("Потеряно соединение с сервером");
                }
            }
        }
    }
}

using Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    public static class Server
    {
        private static int count = 0;
        private static TcpListener tcpListener = new TcpListener(IPAddress.Any, 5050);
        private static ConcurrentDictionary<TcpClient, InfoAboutClient> tcpClients = new();
        private static CancellationTokenSource cancellation = new CancellationTokenSource();

        public static async Task Start()
        {
            Task.Run(StartServerAsync);

            await Task.Run(AwaitCancelToken);
        }
        private static async Task StartServerAsync()
        {
            try
            {
                tcpListener.Start();
                await Console.Out.WriteLineAsync($"Данные сервера: {tcpListener.LocalEndpoint}");
                await Console.Out.WriteLineAsync("Ожидание подключения...");

                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                    await Console.Out.WriteLineAsync("Новое подключение");
                    //Запускаем отдельный поток для нового клиента без ожидания.
                    Task.Run(() => Chat(tcpClient));
                }
            }
            catch (Exception)
            {
                await Console.Out.WriteLineAsync("Сокеты закрыты");
            }
        }
        private static void Chat(TcpClient client)
        {
            Console.WriteLine($"Работаю в потоке {Thread.CurrentThread.ManagedThreadId}");

            using (StreamReader reader = new StreamReader(client.GetStream()))
            {
                using (StreamWriter writer = new StreamWriter(client.GetStream()))
                {
                    //Получаем от клиента Json файл с информацией о клиенте (имя, Guid)
                    string? json = reader.ReadLine();
                    InfoAboutClient infoAboutClient = JsonSerializer.Deserialize<InfoAboutClient>(json);
                    //Сохраняем StreamWriter клиента в объекте типа InfoAboutClient.
                    infoAboutClient.Writer = writer;
                    tcpClients.TryAdd(client, infoAboutClient);

                    Interlocked.Increment(ref count);
                    Console.WriteLine($"\nК нам присоединился: {client.Client.RemoteEndPoint}");
                    Console.WriteLine($"\nИнформация: {infoAboutClient}");

                    while (true)
                    {
                        try
                        {
                            //Ждем сообщения от любого из клиентов для пересылки всем участникам.
                            string? msg = reader.ReadLine();
                            BroabcastAllClients(client, msg);
                        }
                        catch
                        {
                            tcpClients.TryRemove(client, out infoAboutClient);
                            client.Close();

                            Interlocked.Decrement(ref count);

                            Console.WriteLine($"\nНас покинул {infoAboutClient}");

                            return;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Отправка сообщения всем участникам.
        /// </summary>
        /// <param name="client">Клиент, который отправил сообщение</param>
        /// <param name="msg">Сообщение</param>
        private static void BroabcastAllClients(TcpClient client, string msg)
        {
            foreach (var cl in tcpClients.Keys)
            {
                if (!client.Equals(cl))
                {
                    tcpClients[cl].Writer.WriteLine(msg);
                    tcpClients[cl].Writer.Flush();
                }
            }
        }
        private static async Task AwaitCancelToken()
        {
            try
            {
                while (true)
                {
                    if (!cancellation.IsCancellationRequested)
                    {
                        await Console.Out.WriteLineAsync("Введите exit для завершения работы сервера");
                        string? command = await Console.In.ReadLineAsync();

                        if (command.Equals("exit", StringComparison.OrdinalIgnoreCase))
                        {
                            string msg = "Сервер отключен.\nДо скорой встречи";

                            foreach (var valuePair in tcpClients)
                            {
                                await valuePair.Value.Writer.WriteLineAsync(msg);
                                valuePair.Value.Writer.Flush();
                                valuePair.Key.Close();
                            }
                            cancellation.Cancel();
                            tcpListener.Stop();
                        }
                    }
                    else
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                await Console.Out.WriteLineAsync("Отключение сервера");
            }
            
        }
    }
}

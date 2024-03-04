using Client;
using System.Net.Sockets;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Run(() =>  Server.Start());

            Console.ReadKey();
        }
    }
}

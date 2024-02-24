using System.Net;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050);
            Client.Start(iPEndPoint, "Artem");
            Console.ReadLine();
        }
    }
}

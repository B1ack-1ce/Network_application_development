﻿using Client;
using System.Net.Sockets;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server.StartAsync();

            Console.ReadKey();
        }
    }
}

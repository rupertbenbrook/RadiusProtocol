using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RadiusTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Start().Wait();
        }

        public static async Task Start()
        {
            var client = new UdpClient(1812);
            while (true)
            {
                var result = await client.ReceiveAsync();
                Console.WriteLine("{0}: {1} bytes", result.RemoteEndPoint.Address, result.Buffer.Length);
            }
        }
    }
}

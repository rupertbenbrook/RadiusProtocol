using System;
using System.Net.Sockets;
using System.Threading.Tasks;

// https://en.wikipedia.org/wiki/RADIUS
// https://tools.ietf.org/html/rfc2865

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
            var serializer = new RadiusPacketSerializer();
            while (true)
            {
                var result = await client.ReceiveAsync();
                var request = serializer.Read(result.Buffer);
                if (request.Code == RadiusPacketCode.AccessRequest)
                {
                    var response = new RadiusPacket
                    {
                        Code = RadiusPacketCode.AccessReject,
                        Identifier = request.Identifier,
                        Authenticator = request.Authenticator
                    };
                    var buffer = serializer.Write(response);
                    await client.SendAsync(buffer, buffer.Length, result.RemoteEndPoint);
                }
                Console.WriteLine(request.Dump());
            }
        }
    }
}

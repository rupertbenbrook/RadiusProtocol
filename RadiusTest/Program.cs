using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using RadiusProtocol;

// https://en.wikipedia.org/wiki/RADIUS
// https://tools.ietf.org/html/rfc2865
// http://www.iana.org/assignments/radius-types/radius-types.xhtml

namespace RadiusTest
{
    public class Program
    {
        private const string Secret = "This Is A Secure Secret";
        private const string Password = "This Is A Secure Password";

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
                try
                {
                    var result = await client.ReceiveAsync();
                    var request = serializer.Read(result.Buffer);
                    Console.WriteLine(request.Dump());
                    if (request.Code == RadiusPacketCode.AccessRequest)
                    {
                        var password = (RadiusBinaryAttribute)request.Attributes.FirstOrDefault(a => a.Type == RadiusAttributeType.UserPassword);
                        var code = ((password == null) || (string.Compare(RadiusPacketSerializer.DecodePassword(Secret, request.Authenticator, password.Value), Password, StringComparison.InvariantCulture) != 0))
                            ? RadiusPacketCode.AccessReject
                            : RadiusPacketCode.AccessAccept;
                        var response = new RadiusPacket
                        {
                            Code = code,
                            Identifier = request.Identifier,
                            Authenticator = request.Authenticator
                        };
                        var buffer = serializer.Write(response);
                        await client.SendAsync(buffer, buffer.Length, result.RemoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}

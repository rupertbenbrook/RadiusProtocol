using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
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
                    var password = request.Attributes.FirstOrDefault(a => a.Type == RadiusAttributeType.UserPassword);
                    var hash = GetPasswordHash("secretsecretsecretsecretsecretsecret", request.Authenticator, "secretsecretsecretsecretsecretsecret");
                    for (var pos = 0; pos < password.Value.Length; pos++)
                    {
                        Console.WriteLine(password.Value[pos] + " - " + hash[pos]);
                    }
                    var response = new RadiusPacket
                    {
                        Code = RadiusPacketCode.AccessReject,
                        Identifier = request.Identifier,
                        Authenticator = request.Authenticator
                    };
                    var buffer = serializer.Write(response);
                    await client.SendAsync(buffer, buffer.Length, result.RemoteEndPoint);
                }
            }
        }

        public static byte[] GetPasswordHash(string secret, byte[] authenticator, string password)
        {
            var secretBuffer = Encoding.UTF8.GetBytes(secret);
            var passwordBuffer = Encoding.UTF8.GetBytes(password);
            Array.Resize(ref passwordBuffer, passwordBuffer.Length + (16 - (passwordBuffer.Length % 16)));
            var hashBuffer = new byte[passwordBuffer.Length];
            var passwordPos = 0;
            var cipher = authenticator;
            while (passwordPos < passwordBuffer.Length)
            {
                var md5 = MD5.Create();
                md5.TransformBlock(secretBuffer, 0, secretBuffer.Length, secretBuffer, 0);
                md5.TransformFinalBlock(cipher, 0, cipher.Length);
                for (var pos = 0; pos < md5.Hash.Length; pos++)
                {
                    hashBuffer[passwordPos + pos] = (byte)(passwordBuffer[passwordPos + pos] ^ md5.Hash[pos]);
                }
                cipher = hashBuffer.Segment(passwordPos, md5.Hash.Length);
                passwordPos += md5.Hash.Length;
            }
            return hashBuffer;
        }
    }
}

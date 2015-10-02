using System;
using System.Linq;
using System.Net;
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
                var result = await client.ReceiveAsync();
                var request = serializer.Read(result.Buffer);
                Console.WriteLine(request.Dump());
                if (request.Code == RadiusPacketCode.AccessRequest)
                {
                    var password = (RadiusBinaryAttribute)request.Attributes.FirstOrDefault(a => a.Type == RadiusAttributeType.UserPassword);
                    var code = ((password == null) || (string.Compare(DecodePassword(Secret, request.Authenticator, password.Value), Password, StringComparison.InvariantCulture) != 0))
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
        }

        public static byte[] EncodePassword(string secret, byte[] authenticator, string password)
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

        public static string DecodePassword(string secret, byte[] authenticator, byte[] passwordBuffer)
        {
            var secretBuffer = Encoding.UTF8.GetBytes(secret);
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
                cipher = passwordBuffer.Segment(passwordPos, md5.Hash.Length);
                passwordPos += md5.Hash.Length;
            }
            return Encoding.UTF8.GetString(hashBuffer);
        }
    }
}

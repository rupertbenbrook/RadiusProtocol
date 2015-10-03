using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RadiusProtocol
{
    public class RadiusPacketSerializer
    {
        public static Dictionary<RadiusAttributeType, Type> TypeMap = typeof(RadiusAttributeType).Assembly.GetTypes()
            .Where(t => typeof(RadiusAttribute).IsAssignableFrom(t))
            .SelectMany(t => t.GetCustomAttributes(typeof(RadiusAttributeTypeAttribute)).Select(a => new { Attrib = (RadiusAttributeTypeAttribute)a, Type = t }))
            .ToDictionary(a => a.Attrib.AttributeType, a => a.Type);

        public RadiusPacket Read(byte[] buffer)
        {
            if (buffer.Length < 20)
            {
                throw new ArgumentException("Buffer must be at least 20 bytes long.");
            }
            var packet = new RadiusPacket
            {
                Code = (RadiusPacketCode)buffer[0],
                Identifier = buffer[1],
                Authenticator = Segment(buffer, 4, 16)
            };
            var pos = 20;
            while (pos < buffer.Length)
            {
                var type = typeof(RadiusBinaryAttribute);
                if (TypeMap.ContainsKey((RadiusAttributeType) buffer[pos]))
                {
                    type = TypeMap[(RadiusAttributeType)buffer[pos]];
                }
                var attrib = (RadiusAttribute)Activator.CreateInstance(type);
                attrib.Type = (RadiusAttributeType) buffer[pos];
                attrib.SetValueFromBuffer(Segment(buffer, pos + 2, buffer[pos + 1] - 2));
                packet.Attributes.Add(attrib);
                pos += buffer[pos + 1];
            }
            return packet;
        }

        public byte[] Write(RadiusPacket packet)
        {
            var buffer = new byte[packet.Length];
            buffer[0] = (byte)packet.Code;
            buffer[1] = packet.Identifier;
            Buffer.BlockCopy(ReverseSegment(BitConverter.GetBytes(packet.Length), 0, 2), 0, buffer, 2, 2);
            Buffer.BlockCopy(packet.Authenticator, 0, buffer, 4, packet.Authenticator.Length);
            return buffer;
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
                cipher = Segment(hashBuffer, passwordPos, md5.Hash.Length);
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
                cipher = Segment(passwordBuffer, passwordPos, md5.Hash.Length);
                passwordPos += md5.Hash.Length;
            }
            return Encoding.UTF8.GetString(hashBuffer);
        }

        public static T[] Segment<T>(T[] array, int offset, int count)
        {
            var segment = new T[count];
            Buffer.BlockCopy(array, offset, segment, 0, count);
            return segment;
        }

        public static T[] ReverseSegment<T>(T[] array, int offset, int count)
        {
            var segment = Segment(array, offset, count);
            Array.Reverse(segment);
            return segment;
        }
    }
}
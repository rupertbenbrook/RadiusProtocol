using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
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
            var reader = new RadiusPacketReader();
            while (true)
            {
                var result = await client.ReceiveAsync();
                var packet = reader.Read(result.Buffer);
                Console.WriteLine(packet.Dump());
            }
        }
    }

    public class RadiusPacketReader
    {
        public RadiusPacket Read(byte[] buffer)
        {
            if (buffer.Length < 20)
            {
                throw new ArgumentException("Buffer must be at least 20 bytes long.");
            }
            var packet = new RadiusPacket
            {
                Code = buffer[0],
                Identifier = buffer[1],
                Length = BitConverter.ToUInt16(buffer.ReverseSegment(2, 2), 0),
                Authenticator = buffer.Segment(4, 16)
            };
            var attribs = new List<RadiusAttribute>();
            var pos = 20;
            while (pos < buffer.Length)
            {
                var attrib = new RadiusAttribute
                {
                    Type = buffer[pos],
                    Length = buffer[pos + 1],
                    Value = buffer.Segment(pos + 2, buffer[pos + 1] - 2)
                };
                attribs.Add(attrib);
                pos += attrib.Length;
            }
            packet.Attributes = attribs.ToArray();
            return packet;
        }
    }

    public class RadiusPacket
    {
        public byte Code { get; set; }
        public byte Identifier { get; set; }
        public ushort Length { get; set; }
        public byte[] Authenticator { get; set; }
        public RadiusAttribute[] Attributes { get; set; }
    }

    public class RadiusAttribute
    {
        public byte Type { get; set; }
        public byte Length { get; set; }
        public byte[] Value { get; set; }
    }

    public static class ExtensionMethods
    {
        public static string Dump(this object value)
        {
            var builder = new StringBuilder();
            if (value == null)
            {
                return "<null>";
            }
            var valType = value.GetType();
            if (valType.IsValueType)
            {
                return value.ToString();
            }
            if (valType.IsArray)
            {
                var array = (Array)value;
                builder.AppendFormat("{0}[{1}]: ", valType.GetElementType(), array.Length);
                foreach (var item in array)
                {
                    builder.Append(item);
                    builder.Append(", ");
                }
                builder.Length = builder.Length - 2;
                return builder.ToString();
            }
            foreach (var prop in value.GetType().GetProperties())
            {
                builder.Append(prop.Name);
                builder.Append(": ");
                builder.Append(prop.GetValue(value).Dump());
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public static T[] Segment<T>(this T[] array, int offset, int count)
        {
            var segment = new T[count];
            Buffer.BlockCopy(array, offset, segment, 0, count);
            return segment;
        }
        public static T[] ReverseSegment<T>(this T[] array, int offset, int count)
        {
            var segment = array.Segment(offset, count);
            Array.Reverse(segment);
            return segment;
        }
    }

}

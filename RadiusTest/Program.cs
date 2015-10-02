using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                Code = (RadiusPacketCode)buffer[0],
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
                    Type = (RadiusAttributeType)buffer[pos],
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

    public enum RadiusPacketCode : byte
    {
        AccessRequest = 1,
        AccessAccept = 2,
        AccessReject = 3,
        AccountingRequest = 4,
        AccountingResponse = 5,
        AccessChallenge = 11,
        StatusServer = 12,
        StatusClient = 13,
        Reserved = 255
    }

    public enum RadiusAttributeType : byte
    {
        UserName = 1,
        UserPassword = 2,
        CHAPPassword = 3,
        NASIPAddress = 4,
        NASPort = 5,
        ServiceType = 6,
        FramedProtocol = 7,
        FramedIPAddress = 8,
        FramedIPNetmask = 9,
        FramedRouting = 10,
        FilterId = 11,
        FramedMTU = 12,
        FramedCompression = 13,
        LoginIPHost = 14,
        LoginService = 15,
        LoginTCPPort = 16,
        ReplyMessage = 18,
        CallbackNumber = 19,
        CallbackId = 20,
        FramedRoute = 22,
        FramedIPXNetwork = 23,
        State = 24,
        Class = 25,
        VendorSpecific = 26,
        SessionTimeout = 27,
        IdleTimeout = 28,
        TerminationAction = 29,
        CalledStationId = 30,
        CallingStationId = 31,
        NASIdentifier = 32,
        ProxyState = 33,
        LoginLATService = 34,
        LoginLATNode = 35,
        LoginLATGroup = 36,
        FramedAppleTalkLink = 37,
        FramedAppleTalkNetwork = 38,
        FramedAppleTalkZone = 39,
        CHAPChallenge = 60,
        NASPortType = 61,
        PortLimit = 62,
        LoginLATPort = 63,
    }

    public class RadiusPacket
    {
        public RadiusPacketCode Code { get; set; }
        public byte Identifier { get; set; }
        public ushort Length { get; set; }
        public byte[] Authenticator { get; set; }
        public RadiusAttribute[] Attributes { get; set; }
    }

    public class RadiusAttribute
    {
        public RadiusAttributeType Type { get; set; }
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
                    builder.Append(item.Dump());
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

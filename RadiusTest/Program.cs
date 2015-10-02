using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                if (packet.Code == RadiusPacketCode.AccessRequest)
                {
                    var response = new RadiusPacket
                    {
                        Code = RadiusPacketCode.AccessAccept,
                        Identifier = packet.Identifier,
                    };
                }
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
                Authenticator = buffer.Segment(4, 16)
            };
            var pos = 20;
            while (pos < buffer.Length)
            {
                var attrib = new RadiusAttribute
                {
                    Type = (RadiusAttributeType)buffer[pos],
                    Length = buffer[pos + 1],
                    Value = buffer.Segment(pos + 2, buffer[pos + 1] - 2)
                };
                packet.Attributes.Add(attrib);
                pos += attrib.Length;
            }
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
        private readonly List<RadiusAttribute> _attributes = new List<RadiusAttribute>();
        public RadiusPacketCode Code { get; set; }
        public byte Identifier { get; set; }
        public ushort Length
        {
            get { return (ushort)(20 + Attributes.Sum(a => a.Length)); }
        }
        public byte[] Authenticator { get; set; }
        public IList<RadiusAttribute> Attributes
        {
            get { return _attributes; }
        }
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
            DumpInternal(builder, value, 0);
            return builder.ToString();
        }

        public static void DumpInternal(StringBuilder builder, object value, int depth)
        {
            if (value == null)
            {
                builder.Append("<null>");
                return;
            }
            var valType = value.GetType();
            if (valType.IsValueType)
            {
                builder.Append(value.ToString());
                return;
            }
            builder.Append(valType);
            builder.AppendLine(" {");
            int lastValue = builder.Length;
            if (typeof(IEnumerable).IsAssignableFrom(valType))
            {
                foreach (var item in (IEnumerable)value)
                {
                    builder.Append(new string(' ', depth + 1));
                    DumpInternal(builder, item, depth + 1);
                    lastValue = builder.Length;
                    builder.AppendLine(",");
                }
                builder.Length = lastValue;
                builder.AppendLine();
                builder.Append(new string(' ', depth) + "}");
                return;
            }
            foreach (var prop in valType.GetProperties())
            {
                builder.Append(new string(' ', depth + 1));
                builder.Append(prop.Name);
                builder.Append(" = ");
                DumpInternal(builder, prop.GetValue(value), depth + 1);
                lastValue = builder.Length;
                builder.AppendLine(",");
            }
            builder.Length = lastValue;
            builder.AppendLine();
            builder.Append(new string(' ', depth) + "}");
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RadiusTest
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
                Authenticator = buffer.Segment(4, 16)
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
                attrib.SetValueFromBuffer(buffer.Segment(pos + 2, buffer[pos + 1] - 2));
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
            Buffer.BlockCopy(BitConverter.GetBytes(packet.Length).ReverseSegment(0, 2), 0, buffer, 2, 2);
            Buffer.BlockCopy(packet.Authenticator, 0, buffer, 4, packet.Authenticator.Length);
            return buffer;
        }
    }
}
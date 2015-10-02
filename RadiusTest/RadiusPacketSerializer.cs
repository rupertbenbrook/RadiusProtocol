using System;

namespace RadiusTest
{
    public class RadiusPacketSerializer
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
                    Value = buffer.Segment(pos + 2, buffer[pos + 1] - 2)
                };
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
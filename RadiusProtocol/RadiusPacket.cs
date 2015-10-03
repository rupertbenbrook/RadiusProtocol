using System.Collections.Generic;
using System.Linq;

namespace RadiusProtocol
{
    public class RadiusPacket
    {
        private readonly List<RadiusAttribute> _attributes = new List<RadiusAttribute>();
        public RadiusPacketCode Code { get; set; }
        public byte Identifier { get; set; }
        public ushort Length => (ushort)(20 + Attributes.Sum(a => a.Length));
        public byte[] Authenticator { get; set; }
        public IList<RadiusAttribute> Attributes => _attributes;
    }
}
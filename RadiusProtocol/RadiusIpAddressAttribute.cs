using System.Net;

namespace RadiusProtocol
{
    [RadiusAttributeType(RadiusAttributeType.NasIpAddress)]
    [RadiusAttributeType(RadiusAttributeType.FramedIpAddress)]
    [RadiusAttributeType(RadiusAttributeType.FramedIpNetmask)]
    [RadiusAttributeType(RadiusAttributeType.LoginIpHost)]
    public class RadiusIpAddressAttribute : RadiusAttribute<IPAddress>
    {
        protected override byte ValueLength => 4;
        public override void SetValueFromBuffer(byte[] buffer)
        {
            Value = new IPAddress(buffer).MapToIPv4();
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Value;
        }
    }
}
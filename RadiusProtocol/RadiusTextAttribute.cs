using System.Text;

namespace RadiusProtocol
{
    [RadiusAttributeType(RadiusAttributeType.UserName)]
    [RadiusAttributeType(RadiusAttributeType.FilterId)]
    [RadiusAttributeType(RadiusAttributeType.ReplyMessage)]
    [RadiusAttributeType(RadiusAttributeType.FramedRoute)]
    public class RadiusTextAttribute : RadiusAttribute<string>
    {
        protected override byte ValueLength => (byte)Value.Length;
        public override void SetValueFromBuffer(byte[] buffer)
        {
            Value = Encoding.UTF8.GetString(buffer);
        }

        public override string ToString()
        {
            return base.ToString() + ": " + Value;
        }
    }
}
namespace RadiusTest
{
    public class RadiusAttribute
    {
        public RadiusAttributeType Type { get; set; }
        public byte Length => (byte)(1 + Value.Length);
        public byte[] Value { get; set; }
    }
}
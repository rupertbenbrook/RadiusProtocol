namespace RadiusTest
{
    [RadiusAttributeType(RadiusAttributeType.UserPassword)]
    public class RadiusBinaryAttribute : RadiusAttribute<byte[]>
    {
        protected override byte ValueLength => (byte)Value.Length;
        public override void SetValueFromBuffer(byte[] buffer)
        {
            Value = buffer;
        }
    }
}
namespace RadiusTest
{
    public class RadiusBinaryAttribute : RadiusAttribute<byte[]>
    {
        protected override byte ValueLength => (byte)Value.Length;
    }
}
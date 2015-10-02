namespace RadiusTest
{
    public abstract class RadiusAttribute
    {
        public RadiusAttributeType Type { get; set; }
        public byte Length => (byte)(1 + ValueLength);
        protected abstract byte ValueLength { get; }
        public object Value { get; set; }
    }

    public abstract class RadiusAttribute<T> : RadiusAttribute
    {
        public new T Value { get; set; }
    }
}
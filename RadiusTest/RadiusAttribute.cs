namespace RadiusTest
{
    public abstract class RadiusAttribute
    {
        public RadiusAttributeType Type { get; set; }
        public byte Length => (byte)(1 + ValueLength);
        protected abstract byte ValueLength { get; }
        public abstract void SetValueFromBuffer(byte[] buffer);

        public override string ToString()
        {
            return "[" + Type + "]";
        }
    }

    public abstract class RadiusAttribute<T> : RadiusAttribute
    {
        public T Value { get; set; }
    }
}
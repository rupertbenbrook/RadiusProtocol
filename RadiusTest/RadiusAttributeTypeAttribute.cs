using System;

namespace RadiusTest
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RadiusAttributeTypeAttribute : Attribute
    {
        public RadiusAttributeType AttributeType { get; set; }

        public RadiusAttributeTypeAttribute(RadiusAttributeType attributeType)
        {
            AttributeType = attributeType;
        }
    }
}
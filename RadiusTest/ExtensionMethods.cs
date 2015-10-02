using System;
using System.Collections;
using System.Text;

namespace RadiusTest
{
    public static class ExtensionMethods
    {
        public static string Dump(this object value)
        {
            var builder = new StringBuilder();
            DumpInternal(builder, value, 0);
            return builder.ToString();
        }

        public static void DumpInternal(StringBuilder builder, object value, int depth)
        {
            if (value == null)
            {
                builder.Append("<null>");
                return;
            }
            var valType = value.GetType();
            if (valType.IsValueType)
            {
                builder.Append(value);
                return;
            }
            builder.Append(valType);
            builder.AppendLine(" {");
            int lastValue = builder.Length;
            if (typeof(IEnumerable).IsAssignableFrom(valType))
            {
                foreach (var item in (IEnumerable)value)
                {
                    builder.Append(new string(' ', depth + 1));
                    DumpInternal(builder, item, depth + 1);
                    lastValue = builder.Length;
                    builder.AppendLine(",");
                }
                builder.Length = lastValue;
                builder.AppendLine();
                builder.Append(new string(' ', depth) + "}");
                return;
            }
            foreach (var prop in valType.GetProperties())
            {
                builder.Append(new string(' ', depth + 1));
                builder.Append(prop.Name);
                builder.Append(" = ");
                DumpInternal(builder, prop.GetValue(value), depth + 1);
                lastValue = builder.Length;
                builder.AppendLine(",");
            }
            builder.Length = lastValue;
            builder.AppendLine();
            builder.Append(new string(' ', depth) + "}");
        }

        public static T[] Segment<T>(this T[] array, int offset, int count)
        {
            var segment = new T[count];
            Buffer.BlockCopy(array, offset, segment, 0, count);
            return segment;
        }
        public static T[] ReverseSegment<T>(this T[] array, int offset, int count)
        {
            var segment = array.Segment(offset, count);
            Array.Reverse(segment);
            return segment;
        }
    }
}
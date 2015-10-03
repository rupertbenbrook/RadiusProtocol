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
            try
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
                if (valType == typeof (string))
                {
                    builder.Append('"');
                    builder.Append(value);
                    builder.Append('"');
                    return;
                }
                var valString = value.ToString();
                builder.Append(valType);
                if (valString != valType.ToString())
                {
                    builder.Append(" \"");
                    builder.Append(valString);
                    builder.Append('"');
                }
                builder.AppendLine(" {");
                var lastValue = builder.Length;
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
            catch (Exception ex)
            {
                builder.AppendLine("*** EXCEPTION ***");
                builder.AppendLine(ex.ToString());
                builder.AppendLine("*** EXCEPTION ***");
            }
        }
    }
}
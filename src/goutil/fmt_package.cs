// Temporary stand-in for "fmt" package until Go Library is converted

using System;
using System.Linq;

namespace go
{
    public static class fmt_package
    {
        public static void Println(params object[] values)
        {
            string toString(object value)
            {
                return value is null ? "nill" : $"{(value.GetType().IsByRef ? " &" : "")}{value}";
            }

            Console.WriteLine(string.Join(", ", values.Select(toString)));
        }
    }
}

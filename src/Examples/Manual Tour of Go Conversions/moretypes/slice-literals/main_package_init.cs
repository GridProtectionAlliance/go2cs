using System.Collections.Generic;
using System.Linq;
using go;

static partial class main_package
{
    // Inline structure defined for Main::s
    public struct __inline_struct_0
    {
        public int i;
        public @bool b;

        public __inline_struct_0((int, @bool) inputs)
        {
            (int __i, bool __b) = inputs;
            i = __i;
            b = __b;
        }

        public __inline_struct_0(int __i = default, @bool __b = default)
        {
            i = __i;
            b = __b;
        }

        public override string ToString() => $"{{{i} {b}}}";
    }

    public static __inline_struct_0[] @struct(IReadOnlyCollection<(int, bool)> inputs)
    {
        return inputs.Select(input => new __inline_struct_0(input)).ToArray();
    }
}

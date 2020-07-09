using System.Runtime.CompilerServices;
using go;

public static partial class main_package
{
    public partial struct T
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T(@string S = default) {
            this.S = S;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{S}}}";

        // T to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in T obj, NilType _) => obj.Equals( default(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in T obj, NilType nil) => !(obj == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, in T obj) => obj == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, in T obj) => obj != nil;
    }
}

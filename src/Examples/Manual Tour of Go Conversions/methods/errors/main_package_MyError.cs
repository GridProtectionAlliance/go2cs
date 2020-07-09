using System;
using System.Runtime.CompilerServices;
using go;

public static partial class main_package
{
    public partial struct MyError
    {
        public MyError((DateTime, @string) i) :
            this(i.Item1, i.Item2) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MyError(DateTime When = default, @string What = default) {
            this.When = When;
            this.What = What;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{When}}} {{{What}}}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator MyError((DateTime, @string) value) => new MyError(value);

        // Person to nil comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(MyError obj, NilType _) => obj.Equals( default(MyError));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(MyError obj, NilType nil) => !(obj == nil);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(NilType nil, MyError obj) => obj == nil;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(NilType nil, MyError obj) => obj != nil;
    }
}

using System.Runtime.CompilerServices;
using go;
using sync = go.sync_package;

static partial class main_package
{
    public partial struct SafeCounter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SafeCounter((sync.Mutex, map<@string, int>) value) :
            this(value.Item1, value.Item2)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SafeCounter(sync.Mutex mu = default, map<@string, int> v = default)
        {
            this.mu = mu;
            this.v = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{mu} {v}}}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SafeCounter((sync.Mutex, map<@string, int>) value) => new SafeCounter(value);
    }
}

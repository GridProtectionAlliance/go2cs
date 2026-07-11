using System.Runtime.CompilerServices;
using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex((float64, float64) value) :
            this(value.Item1, value.Item2)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex(float64 X = default, float64 Y = default)
        {
            this.X = X;
            this.Y = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{X} {Y}}}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vertex((float64, float64) value) => new Vertex(value);
    }
}

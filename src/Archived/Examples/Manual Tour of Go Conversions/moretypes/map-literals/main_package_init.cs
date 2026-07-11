using System.Runtime.CompilerServices;
using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex((float64, float64) i) :
            this(i.Item1, i.Item2)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex(float64 Lat = default, float64 Long = default)
        {
            this.Lat = Lat;
            this.Long = Long;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"{{{Lat} {Long}}}";
    }
}

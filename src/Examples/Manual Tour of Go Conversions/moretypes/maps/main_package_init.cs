using float64 = System.Double;

static partial class main_package
{
    partial struct Vertex
    {
        public Vertex(float64 __Lat = default, float64 __Long = default)
        {
            Lat = __Lat;
            Long = __Long;
        }

        public override string ToString() => $"{{{Lat} {Long}}}";
    }
}

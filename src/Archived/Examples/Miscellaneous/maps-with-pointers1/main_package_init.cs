using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex
    {
        public Vertex((float64, float64) i) :
            this(i.Item1, i.Item2)
        {
        }

        public Vertex(float64 Lat = default, float64 Long = default)
        {
            this.Lat = Lat;
            this.Long = Long;
        }

        public override string ToString() => $"{{{Lat} {Long}}}";
    }
}

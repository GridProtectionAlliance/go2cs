using System.IO.Compression;

static partial class main_package
{
    partial struct Vertex
    {
        public Vertex(int __X = default, int __Y = default)
        {
            X = __X;
            Y = __Y;
        }

        public override string ToString() => $"{{{X} {Y}}}";
    }

}

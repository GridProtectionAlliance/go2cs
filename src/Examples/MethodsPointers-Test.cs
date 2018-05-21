using System;

public static partial class main_package
{
    public struct Vertex
    {
        public double X;
        public double Y;
    }

    public static double Abs(this Vertex v)
    {
        return Math.Sqrt(v.X * v.X + v.Y * v.Y);
    }

    public static void Scale(this ref Vertex v, double f)
    {
        v.X = v.X * f;
        v.Y = v.Y * f;
    }

    public static void Main()
    {
        Vertex v = new Vertex { X = 3, Y = 4 };
        v.Scale(10);
        Console.WriteLine(v.Abs());
    }
}
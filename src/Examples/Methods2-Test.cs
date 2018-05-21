using System;

using MyFloat = System.Single;

namespace Main_namespace
{
    public static class Main_class
    {
        public static float Abs(this MyFloat f)
        {
            if (f < 0)
                return (float)-f;

            return (float)f;
        }

        static void Main(string[] args)
        {
            var f = (MyFloat)(-Math.Sqrt(2));

            Console.WriteLine(f.Abs());
        }
    }
}
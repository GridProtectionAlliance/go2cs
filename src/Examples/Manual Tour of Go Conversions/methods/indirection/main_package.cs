/*
package main

import "fmt"

type Vertex struct {
    X, Y float64
}

func (v *Vertex) Scale(f float64) {
    v.X = v.X * f
    v.Y = v.Y * f
}

func ScaleFunc(v *Vertex, f float64) {
    v.X = v.X * f
    v.Y = v.Y * f
}

func main() {
    v := Vertex{3, 4}
    v.Scale(2)
    ScaleFunc(&v, 10)

    p := &Vertex{4, 3}
    p.Scale(3)
    ScaleFunc(p, 8)

    fmt.Println(v, p)
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;
using float64 = System.Double;

static partial class main
{
    public partial struct Vertex {
        public float64 X;
        public float64 Y;
    }

    public static void Scale(this ref Vertex v, float64 f) {
        v.X = v.X * f;
        v.Y = v.Y * f;
    }

    public static void ScaleFunc(ptr<Vertex> v__ptr, float64 f) {
        ref var v = ref v__ptr.val;
        v.X = v.X * f;
        v.Y = v.Y * f;
    }

    static void Main() {
        // Address of v taken in this function, so we heap allocate and get a pointer
        ref var v = ref heap(new Vertex(3, 4), out var v__ptr);
        v.Scale(2);
        ScaleFunc(v__ptr, 10);

        var p = addr(new Vertex(4, 3));
        p.val.Scale(3);
        ScaleFunc(p, 8);

        fmt.Println(v, p);
    }
}
#endregion

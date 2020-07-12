/*
package main

import "fmt"

type Vertex struct {
	X, Y int
}

var (
	v1 = Vertex{1, 2}  // has type Vertex
	v2 = Vertex{X: 1}  // Y:0 is implicit
	v3 = Vertex{}      // X:0 and Y:0
	p  = &Vertex{1, 2} // has type *Vertex
)

func main() {
	fmt.Println(v1, p, v2, v3)
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;

static partial class main_package
{
    partial struct Vertex {
        public int X;
        public int Y;
    }

    // As of C# 8.0, implicit typing of member variables is not available
    static Vertex v1 = new Vertex(1, 2);            // has type Vertex
    static Vertex v2 = new Vertex { X = 1 };        // Y:0 is implicit
    static Vertex v3 = new Vertex();                // X:0 and Y:0
    static ptr<Vertex> p = ptr(new Vertex(1, 2));   // has type *Vertex

    static void Main() {
        fmt.Println(v1, p, v2, v3);
    }
}
#endregion
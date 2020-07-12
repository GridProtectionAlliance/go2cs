/*
package main

import "fmt"

type Vertex struct {
	X int
	Y int
}

func main() {
	v := Vertex{1, 2}
	v.X = 4
	fmt.Println(v.X)
}
*/
#region source
using fmt = go.fmt_package;

static partial class main_package
{
    partial struct Vertex {
        public int X;
        public int Y;
    }

    static void Main() {
        var v = new Vertex(1, 2);
        v.X = 4;
        fmt.Println(v.X);
    }
}
#endregion
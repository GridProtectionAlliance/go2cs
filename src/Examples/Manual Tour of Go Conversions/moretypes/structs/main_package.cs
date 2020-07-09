/*
package main

import "fmt"

type Vertex struct {
	X int
	Y int
}

func main() {
	fmt.Println(Vertex{1, 2})
}
*/
using fmt = go.fmt_package;

static partial class main_package
{
    partial struct Vertex {
        public int X;
        public int Y;
    }

    static void Main()  {
        fmt.Println(new Vertex(1, 2));
    }
}

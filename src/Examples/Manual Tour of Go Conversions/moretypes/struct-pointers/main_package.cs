/*
package main

import "fmt"

type Vertex struct {
	X int
	Y int
}

func main() {
	v := Vertex{1, 2}
	p := &v
	p.X = 1e9
	fmt.Println(v)
}
*/
//using go;
//using static go.builtin;
using fmt = go.fmt_package;

static /*unsafe*/ partial class main_package
{
    partial struct Vertex {
        public int X;
        public int Y;
    }

    static void Main() {
        // This simple version works fine when variables are constrained to stack:
        //var v = new Vertex(1, 2);
        //var p = &v;
        //p->X = (int)1e9;
        //fmt.Println(v);

        // This is a simple safe version that works for any unmanaged types:
        var v = new Vertex(1, 2);
        ref var p = ref v;
        p.X = (int)1e9;
        fmt.Println(v);

        // If structure needs to leave the stack, start with heap-allocated instance of
        // value. Code analysis should check that pointer does not escape the stack in
        // which case simple local pointers can be used instead as this is optimal.
        // If this is complex, then until conversion tool becomes sophisticated enough
        // for this type of dynamic analysis, the following pattern will always work:
        //var v__ptr = @ref(new Vertex(1, 2));
        //ref var v = ref heap(new Vertex(1, 2), out var v__ptr).Value;
        //var p = v__ptr;
        //p.Value.X = (int)1e9;
        //fmt.Println(v);
    }
}

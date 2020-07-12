/*
package main

import "fmt"

type Vertex struct {
	Lat, Long float64
}

var m map[string]Vertex

func main() {
	m = make(map[string]Vertex)
	m["Bell Labs"] = Vertex{
		40.68433, -74.39967,
	}
	fmt.Println(m["Bell Labs"])
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;
using float64 = System.Double;

static partial class main_package
{
    partial struct Vertex {
        public float64 Lat;
        public float64 Long;
    }

    static map<@string, Vertex> m;

    static void Main(){
        m = make_map<@string, Vertex>();
        m["Bell Labs"] = new Vertex(
            40.68433, -74.39967
        );
        fmt.Println(m["Bell Labs"]);
    }
}
#endregion
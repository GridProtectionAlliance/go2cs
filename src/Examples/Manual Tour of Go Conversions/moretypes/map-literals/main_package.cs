/*
package main

import "fmt"

type Vertex struct {
    Lat, Long float64
}

var m = map[string]Vertex{
    "Bell Labs": Vertex{
        40.68433, -74.39967,
    },
    "Google": Vertex{
        37.42202, -122.08408,
    },
}

func main() {
    fmt.Println(m)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex {
        public float64 Lat;
        public float64 Long;
    }

    static map<@string, Vertex> m = new map<@string, Vertex> {
        ["Bell Labs"] = new Vertex(
            40.68433, -74.39967
        ),
        ["Google"] = new Vertex(
            37.42202, -122.08408
        ),
    };

    static void Main() {
        fmt.Println(m);
    }
}
#endregion
/*
package main

import "fmt"

type Vertex struct {
    Lat, Long float64
}

var m map[string]*Vertex

func update(val *float64) {
  *val++;
}

func getVertex() *Vertex {
    v1 := Vertex{
        40.68433, -74.39967,
    }
    
    m["Bell Labs"] = &v1
    fmt.Println(v1, m["Bell Labs"])

    v1.Long = -99
    fmt.Println(v1, m["Bell Labs"])

    var v = m["Bell Labs"]
    v.Lat = 99    
    fmt.Println(v1, m["Bell Labs"])
    
    return &v1
}

func main() {
    m = make(map[string]*Vertex)
    
    v1 := getVertex()
    
    v2 := m["Bell Labs"]
    v2.Lat = 999    
    fmt.Println(v1, m["Bell Labs"])
    
    m["Bell Labs"].Lat = 1000
    fmt.Println(v1, m["Bell Labs"])
    
    update(&v2.Lat);
    fmt.Println(v1, m["Bell Labs"])
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;
using float64 = System.Double;

static partial class main_package
{
    public partial struct Vertex
    {
        public float64 Lat;
        public float64 Long;
    }

    // Generic arguments cannot be pointers, so a class "reference"
    // of the structure type works in this case
    static map<@string, ptr<Vertex>> m;

    static void update(ref float64 val) {
        val++;
    }

    static ptr<Vertex> getVertex() {
        // Here be dragons - don't let the local struct leave the stack! At the very point
        // where code detects an escape route for the stack allocated structure in the local
        // function, code has to use a heap allocated instance of the structure. That said,
        // the detection "should" be as simple as encountering any address-of operator "&"
        // for the variable within the local function. A detailed analysis of the code path
        // might show that a simple pointer (or safe ref var) would do the trick as long as
        // pointer doesn't escape to the heap later. Such an operation could be optimal
        // because there would be no required heap allocation. Interestingly, Go actually
        // has a compiler directive called "go.noescape" which indicates exactly this, but
        // it's only used with external CGO imports.
        ref var v1 = ref heap(new Vertex(
            40.68433, -74.39967
        ), out var p__v1);

        m["Bell Labs"] = p__v1;
        fmt.Println(v1, m["Bell Labs"]);

        v1.Long = -99;
        fmt.Println(v1, m["Bell Labs"]);

        var v = m["Bell Labs"];
        v.val.Lat = 99;
        fmt.Println(v1, m["Bell Labs"]);

        // Escape detected here with address of operator for "&v1"
        return p__v1;
    }

    static void Main() {
        m = new map<@string, ptr<Vertex>>();

        var v1 = getVertex();

        var v2 = m["Bell Labs"];
        v2.val.Lat = 999;
        fmt.Println(v1, m["Bell Labs"]);

        m["Bell Labs"].val.Lat = 1000;
        fmt.Println(v1, m["Bell Labs"]);

        update(ref v2.val.Lat);
        fmt.Println(v1, m["Bell Labs"]);
    }
}
#endregion

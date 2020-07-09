// From https://yourbasic.org/golang/maps-explained/
/*
package main

import "fmt"

func main() {
    var m map[string]int                // nil map of string-int pairs

    m1 := make(map[string]float64)      // Empty map of string-float64 pairs
    m2 := make(map[string]float64, 100) // Preallocate room for 100 entries

    m3 := map[string]float64{           // Map literal
        "e":  2.71828,
        "pi": 3.1416,
    }
    fmt.Println(len(m3))                // Size of map: 2
}
*/
using go;
using static go.builtin;
using fmt = go.fmt_package;
using float64 = System.Double;

static class main_package
{
    static void Main() {
        map<@string, int> m;    // nil map of string-int pairs

        var m1 = make_map<@string, float64>();          // Empty map of string-float64 pairs
        var m2 = make_map<@string, float64>(100);   // Preallocate room for 100 entries

        var m3 = new map<@string, float64> {    // Map literal
            ["e"] = 2.71828,
            ["pi"] = 3.1416,
        };

        fmt.Println(len(m3));   // Size of map: 2
    }
}
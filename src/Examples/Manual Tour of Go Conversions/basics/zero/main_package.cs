/*
package main

import "fmt"

func main() {
	var i int
	var f float64
	var b bool
	var s string
	fmt.Printf("%v %v %v %q\n", i, f, b, s)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using float64 = System.Double;

static class main_package
{
    static void Main() {
		// Go auto initializes locals where C# requires an initial value
		int i = default;
        float64 f = default;
		bool b = default;
		@string s = default;

        fmt.Printf("{0} {1} {2} {3}\n", i, f, b, s);
    }
}
#endregion
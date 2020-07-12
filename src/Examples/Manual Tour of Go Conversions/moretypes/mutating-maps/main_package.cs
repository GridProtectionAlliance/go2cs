/*
package main

import "fmt"

func main() {
	m := make(map[string]int)

	m["Answer"] = 42
	fmt.Println("The value:", m["Answer"])

	m["Answer"] = 48
	fmt.Println("The value:", m["Answer"])

	delete(m, "Answer")
	fmt.Println("The value:", m["Answer"])

	v, ok := m["Answer"]
	fmt.Println("The value:", v, "Present?", ok)
}
*/
#region source
using go;
using static go.builtin;
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
        var m = make_map<@string, int>();

        m["Answer"] = 42;
        fmt.Println("The value:", m["Answer"]);

        m["Answer"] = 48;
        fmt.Println("The value:", m["Answer"]);

        delete(m, "Answer");
        fmt.Println("The value:", m["Answer"]);

        var (v, ok) = m["Answer", WithOK];
        fmt.Println("The value:", v, "Present?", ok);
    }
}
#endregion
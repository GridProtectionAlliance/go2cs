// https://go101.org/article/details.html#default-branch-position
/*
package main

import "net/http"

func main() {
    type P = *bool
    type M = map[int]int
    var x struct {
        string // a defined non-pointer type
        error  // a defined interface type
        *int   // a non-defined pointer type
        P      // an alias of a non-defined pointer type
        M      // an alias of a non-defined type

        http.Header // a defined map type
    }
    x.string = "Go"
    x.error = nil
    x.int = new(int)
    x.P = new(bool)
    x.M = make(M)
    x.Header = http.Header{}
}
*/
#region source
using go;
using P__main = go.ptr<bool>;
using M__main = go.map<int, int>;
using static go.builtin;

struct x__main {
    public @string @string;
    public error error;
    public ptr<int> @int;
    public P__main P;
    public M__main M;
}

static class main_package
{
    static void Main() {
        x__main x;
        x.@string = "Go";
        x.error = null;
        x.@int = @new<int>();
        x.P = @new<bool>();
        x.M = make<M__main>();
    }
}
#endregion

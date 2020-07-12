// From https://go101.org/article/details.html#defer-modify-results
/*
package main

import "fmt"
import "io"
import inout "io"

func main() {
	fmt.Println(&inout.EOF == &io.EOF) // true
}
*/
#region source
using fmt = go.fmt_package;
using io = go.io_package;
using inout = go.io_package;

static class main_package
{
    static void Main() {
        fmt.Println(inout.EOF == io.EOF); // true
    }
}
#endregion
// Important conversion takeaway:
//   Getting address of interface is not necessary in C#
//   since interface is already a reference. Double indirection
//   should result in use of ptr<interface>
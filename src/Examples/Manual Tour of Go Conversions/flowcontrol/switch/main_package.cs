/*
package main

import (
    "fmt"
    "runtime"
)

func main() {
    fmt.Print("Go runs on ")
    switch os := runtime.GOOS; os {
    case "darwin":
        fmt.Println("OS X.")
    case "linux":
        fmt.Println("Linux.")
    default:
        // freebsd, openbsd,
        // plan9, windows...
        fmt.Printf("%s.\n", os)
    }
}
*/
#region source
using fmt = go.fmt_package;
using runtime = go.runtime_package;

static class main_package
{
    static void Main() {
        fmt.Print("go2cs runs on ");
        {
            var os = runtime.GOOS;
            switch (os) {
                case "darwin":
                    fmt.Println("OS X.");
                    break;
                case "linux":
                    fmt.Println("Linux.");
                    break;
                default:
                    // freebsd, openbsd,
                    // plan9, windows...
                    fmt.Printf("{0}.\n", os);
                    break;
            }
        }
    }
}
#endregion

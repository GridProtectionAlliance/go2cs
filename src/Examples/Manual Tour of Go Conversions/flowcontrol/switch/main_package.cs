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

using System;
using fmt = go.fmt_package;

static class main_package
{
    static void Main() {
        fmt.Print("Go runs on ");
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

// runtime proxy function
public static class runtime
{
    public static string GOOS
    {
        get
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    return "windows";
                case PlatformID.Unix:
                    return "linux";
                case PlatformID.MacOSX:
                    return "darwin";
                default:
                    return "undetermined";
            }
        }
    }
}

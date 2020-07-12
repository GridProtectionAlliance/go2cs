/*
package main

import (
	"fmt"
	"time"
)

func main() {
	t := time.Now()
	switch {
	case t.Hour() < 12:
		fmt.Println("Good morning!")
	case t.Hour() < 17:
		fmt.Println("Good afternoon.")
	default:
		fmt.Println("Good evening.")
	}
}
*/
#region source
using go;
using fmt = go.fmt_package;
using time = go.time_package;

static class main_package
{
    static void Main() {
		var t = time.Now();
        if (t.Hour() < 12) {
            fmt.Println("Good morning!");
        }
		else if (t.Hour() < 17) {
            fmt.Println("Good afternoon.");
        }
		else {
            fmt.Println("Good evening.");
        }
    }
}
#endregion
// This would work as well, at least in this simple case
//switch (true) {
//	case true when t.Hour() < 12:
//		fmt.Println("Good morning!");
//		break;
//    case true when t.Hour() < 17:
//        fmt.Println("Good afternoon.");
//        break;
//	default:
//		fmt.Println("Good evening.");
//		break;
//}

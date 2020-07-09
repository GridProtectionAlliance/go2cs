/*
package main

import (
	"fmt"
	"time"
)

func main() {
	fmt.Println("When's Saturday?")
	today := time.Now().Weekday()
	switch time.Saturday {
	case today + 0:
		fmt.Println("Today.")
	case today + 1:
		fmt.Println("Tomorrow.")
	case today + 2:
		fmt.Println("In two days.")
	default:
		fmt.Println("Too far away.")
	}
}
*/

using System;
using fmt = go.fmt_package;
using time = System.DateTime;

static class main_package
{
    static void Main() {
		fmt.Println("When's Saturday?");
		var today = (int)time.Now.DayOfWeek;

        // Option 1:
        //switch (true) {
		//	  case true when (int)DayOfWeek.Saturday == (int)today + 0:
		//		  fmt.Println("Today.");
		//		  break;
        //    case true when (int)DayOfWeek.Saturday == (int)today + 1:
        //        fmt.Println("Tomorrow.");
        //        break;
        //    case true when (int)DayOfWeek.Saturday == (int)today + 2:
        //        fmt.Println("In two days.");
        //        break;
        //    default:
		//		  fmt.Println("Too far away.");
		//		  break;
		//}

        // Option 2:
        if ((int)DayOfWeek.Saturday == today + 0) {
            fmt.Println("Today.");
        }
        else if ((int)DayOfWeek.Saturday == today + 1) {
            fmt.Println("Tomorrow.");
        }
        else if ((int)DayOfWeek.Saturday == today + 2) {
            fmt.Println("In two days.");
        }
        else {
            fmt.Println("Too far away.");
        }

        // Option 3: with C# 9.0
        //(int)DayOfWeek.Saturday switch
        //{
        //    == (int)today + 0 => fmt.Println("Today.");
        //    == (int)today + 1 => fmt.Println("Tomorrow.");
        //    == (int)today + 2 => fmt.Println("In two days.");
        //	  _ => fmt.Println("Too far away.");
        //};
    }
}

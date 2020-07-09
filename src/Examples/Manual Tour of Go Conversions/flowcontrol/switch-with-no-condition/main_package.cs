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
using fmt = go.fmt_package;
using time = System.DateTime;

static class main_package
{
    static void Main() {
		var t = time.Now;
		switch (true) {
			case true when t.Hour < 12:
				fmt.Println("Good morning!");
				break;
            case true when t.Hour < 17:
                fmt.Println("Good afternoon.");
                break;
			default:
				fmt.Println("Good evening.");
				break;
        }
	}
}

/*
package main

import (
	"fmt"
	"time"
)

func say(s string) {
	for i := 0; i < 5; i++ {
		time.Sleep(100 * time.Millisecond)
		fmt.Println(s)
	}
}

func main() {
	go say("world")
	say("hello")
}
*/
#region source
using go;
using fmt = go.fmt_package;
using time = go.time_package;
using static go.builtin;

static class main_package
{
	static void say(@string s) {
        for (int i = 0; i < 5; i++) {
			time.Sleep(100 * time.Millisecond);
			fmt.Println(s);
        }
    }

    static void Main()  {
		go_(() => say("world"));
		say("hello");
    }
}
#endregion
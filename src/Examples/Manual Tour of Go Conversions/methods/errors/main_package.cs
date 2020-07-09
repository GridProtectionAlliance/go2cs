/*
package main

import (
	"fmt"
	"time"
)

type MyError struct {
	When time.Time
	What string
}

func (e *MyError) Error() string {
	return fmt.Sprintf("at %v, %s",
		e.When, e.What)
}

func run() error {
	return &MyError{
		time.Now(),
		"it didn't work",
	}
}

func main() {
	if err := run(); err != nil {
		fmt.Println(err)
	}
}
*/

using System;
using go;
using fmt = go.fmt_package;
using static go.builtin;

static partial class main_package
{
	partial struct MyError {
		public DateTime When;
		public @string What;
	}

	static @string Error(this ref MyError e) {
		return fmt.Sprintf("at {0}, {1}",
			e.When, e.What);
	}

	static error run() {
		return error.As(new MyError(
			DateTime.Now,
			"it didn't work"));
    }

    static void Main() {
		{
			var err = run();

			if (err != nil)
                fmt.Println(err);
		}
    }
}
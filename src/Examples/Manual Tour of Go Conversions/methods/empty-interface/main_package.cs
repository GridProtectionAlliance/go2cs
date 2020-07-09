/*
package main

import "fmt"

func test(i *interface{}) {
	describe(*i)
}

func main() {
	var i interface{}
	describe(i)

	i = 42
	describe(i)

	i = "hello"
	describe(i)
	
	test(&i)
}

func describe(i interface{}) {
	fmt.Printf("(%v, %T)\n", i, i)
}
*/

using go;
using static go.builtin;

static class main_package
{
	static void test(ptr<object> i) {
		describe(~i);
	}

	static void Main() {
        object i = default!;
		describe(i);

		i = 42;
		describe(i);

		i = "hello";
		describe(i);

		test(ptr(i));
    }

	static void describe(object? i) {
		println($"({i?.ToString() ?? "<nil>"} {i?.GetType().Name ?? "<nil>"})");
	}
}
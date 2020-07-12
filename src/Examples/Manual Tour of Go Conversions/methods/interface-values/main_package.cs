/*
package main

import (
	"fmt"
	"math"
)

type I interface {
	M()
}

type T struct {
	S string
}

func (t *T) M() {
	fmt.Println(t.S)
}

type F float64

func (f F) M() {
	fmt.Println(f)
}

func main() {
	var i I

	i = &T{"Hello"}
	describe(i)
	i.M()

	i = F(math.Pi)
	describe(i)
	i.M()
}

func describe(i I) {
	fmt.Printf("(%v, %T)\n", i, i)
}
*/
#region source
using go;
using fmt = go.fmt_package;
using math = System.Math;
using static go.builtin;

using F = System.Double;

static partial class main_package
{
    partial interface I {
		void M();
    }

    partial struct T {
        public @string S;
    }

	public static void M(this ref T t) {
		fmt.Println(t.S);
	}

	public static void M(this F f) {
        fmt.Println(f);
	}

	static void Main() {
		I i;

		i = I.As(ptr(new T("Hello")));
		describe(i);
		i.M();

		i = I.As((F)(math.PI));
		describe(i);
        i.M();
    }

	static void describe(I i) {
		fmt.Println($"({i:v}, {i:T})");
	}
}
#endregion
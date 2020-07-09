/*
package main

import "fmt"

type Person struct {
	Name string
	Age  int
}

func (p Person) String() string {
	return fmt.Sprintf("%v (%v years)", p.Name, p.Age)
}

func main() {
	a := Person{"Arthur Dent", 42}
	z := Person{"Zaphod Beeblebrox", 9001}
	fmt.Println(a, z)
}
*/

using fmt = go.fmt_package;
using go;

static partial class main_package
{
	partial struct Person {
		public @string Name;
		public int Age;
	}

	static @string String(this in Person p) {
		return fmt.Sprintf("{0} ({1} years)", p.Name, p.Age);
    }

    static void Main() {
		Person a = ("Arthur", 42);
		Person z = ("Zaphod Beeblebrox", 9001);
		fmt.Println(a, z);
    }
}
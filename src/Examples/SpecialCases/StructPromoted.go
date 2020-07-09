package main

import "fmt"

type DoDad struct {
	I int
	O string
	a [2][2]string
}

type Vertex struct {
	X, Y int "Hi"	
	DoDad
}

func main() {
	v := Vertex{1, 2, DoDad{12, "Hello", [2][2]string{{"one", "two"}, {"three", "four"}}}}
	v.X = 4
	v.DoDad.O = "Bye"
	v.a[1][0] = "another"
	fmt.Println(v.X, v.O, v.a[1][0])
}
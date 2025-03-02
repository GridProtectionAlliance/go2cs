package main

import "fmt"

func main() {
	var tbl map[any]string

	tbl = make(map[any]string)

	tbl["12"] = "1"
	tbl[12] = "2"
	tbl[12.0] = "3"

	fmt.Println(tbl["12"])
	fmt.Println(tbl[12])
	fmt.Println(tbl[12.0])
}

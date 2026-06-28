package main
import ("fmt"; "unsafe")
type stdFunction unsafe.Pointer
var handler stdFunction
var other stdFunction
func main() {
	handler = other
	fmt.Println(handler == other)
}

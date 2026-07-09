package main

import "fmt"

func valid(flag bool) (_ bool) {
	if flag {
		return true
	}
	return
}

func pair(tag string) (_ int, label string) {
	label = "empty:" + tag
	if tag == "set" {
		return 7, "set"
	}
	return
}

func main() {
	fmt.Println(valid(true), valid(false))
	n, label := pair("base")
	fmt.Println(n, label)
	n, label = pair("set")
	fmt.Println(n, label)
}
package main

import "fmt"

type Node interface {
	Pos() int
}

type Item struct {
	pos int
}

func (i *Item) Pos() int {
	return i.pos
}

func main() {
	item := &Item{pos: 7}
	seen := map[Node]string{}

	seen[item] = "kept"
	value, ok := seen[item]

	fmt.Println(value, ok, item.Pos())
}
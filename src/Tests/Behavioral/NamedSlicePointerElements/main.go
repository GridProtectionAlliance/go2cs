package main

import "fmt"

type item struct {
	v int
}

type Array struct {
	label string
}

type queue []*item

func main() {
	q := queue{&item{v: 1}, &item{v: 2}}
	q = append(q, &item{v: 3})
	q[1].v = 5

	sum := 0
	for _, it := range q {
		sum += it.v
	}

	fmt.Println(len(q), q[0].v, q[1].v, q[2].v, sum)
}

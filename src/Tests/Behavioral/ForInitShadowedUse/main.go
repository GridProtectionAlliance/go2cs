package main

import "fmt"

type counter struct{ n int }

func (c *counter) start() int { return c.n }

// shadowedForInit declares an inner `b` that shadows the outer `b` (renamed `bΔ1` in C#), then
// references it in a FOR-LOOP INIT's RHS (`for i, x := 0, b.start(); …`). The init RHS must be
// traversed by the variable analysis so that `b` there is renamed too — otherwise it keeps the raw
// name and binds to the wrong (outer/forward) `b` (CS0841 / wrong value). Mirrors runtime's
// map_faststr.go (`b := (*bmap)(…); for …, kptr := …, b.keys(); …`).
func shadowedForInit() int {
	b := &counter{n: 100}
	total := b.n // use outer b so it stays in scope
	{
		b := &counter{n: 5} // shadows outer b
		for i, x := 0, b.start(); i < 3; i++ {
			total += x + b.n + i
		}
	}
	return total
}

func main() {
	fmt.Println(shadowedForInit())
}

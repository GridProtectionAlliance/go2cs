package main

import "fmt"

// Slicing a POINTER-TO-ARRAY (`p[lo:hi:max]`, p of type `*[N]T`) auto-derefs in Go. The C#
// box `ж<array<T>>` has no slice/range members — its underlying `array<T>` does — so the
// converter must operate on the dereferenced array: `(~p).slice(…)` / `(~p)[..]`, not
// `p.slice(…)` (CS1929). Mirrors the runtime's select.go `cas1[:ncases:ncases]` and
// mprof.go `stk[:n:n]`, where the operand is a `*[N]T`.
func main() {
	arr := [5]int{10, 20, 30, 40, 50}
	p := &arr

	full := p[:]            // (~p)[..]
	low := p[2:]            // (~p)[2..]
	high := p[:3]           // (~p)[..3]
	mid := p[1:4]           // (~p)[1..4]
	three := p[1:3:4]       // (~p).slice(1, 3, 4)

	fmt.Println(full)
	fmt.Println(low)
	fmt.Println(high)
	fmt.Println(mid)
	fmt.Println(three, len(three), cap(three))

	// A slice over the array pointer shares the array's backing storage.
	full[0] = 99
	fmt.Println(arr[0])
}

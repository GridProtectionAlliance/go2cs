package main

import (
	"fmt"
	"time"
)

func main() {
	go fmt.Println("First")
	go fmt.Println("Second")
	go fmt.Println("Third")

	f1 := fmt.Println
	go f1("Fourth")

	go GetPrintLn()("Fifth")

	go fmt.Println("Function result:", add(3, 4))

	printSquare(5)

	count := 1
	go func() {
		fmt.Println("Go count (closure):", count)
	}()
	count = 10
	fmt.Println("Count before Go:", count)

	time.Sleep(200)

	done := make(chan struct{})
	runPair(done)
	<-done

	acc := &accum{}
	bindAdd(acc)
	fmt.Println("accum total:", acc.total) // 12

	fmt.Println("Main function")
}

type accum struct{ total int }

func (a *accum) add(n int) int {
	a.total += n
	return a.total
}

// bindAdd mirrors net lookup.go's `resolverFunc := r.lookupIP`: a POINTER-receiver method
// VALUE taken from an ALREADY-pointer receiver — the binding must go through the box
// (`Ꮡa.add`), never the deref'd value alias (`Ꮡa.Value.add` is a struct VALUE against the
// [GoRecv] ж<T> extension, CS1929).
func bindAdd(a *accum) {
	add := a.add
	fmt.Println("bound add:", add(5), add(7)) // 5 12
}

func GetPrintLn() func(string) {
	return func(src string) {
		fmt.Println(src)
	}
}

func add(x, y int) int {
	result := x + y
	fmt.Println("Calculate:", result)
	return result
}

// runPair mirrors net lookup.go's `go dnsWaitGroupDone(ch, func() {})`: a FUNC-VALUE
// callee with a func-literal ARGUMENT — the literal's capture-snapshot declarations must
// hoist BEFORE the goǃ statement, not into the argument list (CS1003 x4).
func runPair(done chan struct{}) {
	tag := "pair"
	handler := func(ch chan struct{}, fn func()) {
		fn()
		fmt.Println("handled:", tag)
		ch <- struct{}{}
	}
	go handler(done, func() { fmt.Println("inner fn ran") })
}

func printSquare(n int) {
	go fmt.Println("Go thread square:", n*n)
	n++
	fmt.Println("Immediate n:", n)
}

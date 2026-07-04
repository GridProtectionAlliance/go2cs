package main

import (
	"fmt"
)

func main() {
	defer fmt.Println("First")
	defer fmt.Println("Second")
	defer fmt.Println("Third")

	f1 := fmt.Println
	defer f1("Fourth")

	defer GetPrintLn()("Fifth")

	c := &acc{}
	s1, e1 := c.add(5)
	fmt.Println(s1, e1)
	s2, e2 := c.add(-1)
	fmt.Println(s2, e2, c.total)

	fmt.Println("Main function")
}

type acc struct{ total int }

// add: a pointer-receiver method with a function-level defer+recover and NAMED results - the
// whole body is emitted inside the synthesized execution-context lambda, so the receiver must
// take the direct-ж box form (a `ref acc` reference inside the lambda is CS1628; fmt ss.Token).
func (a *acc) add(n int) (sum int, err error) {
	defer func() {
		if e := recover(); e != nil {
			err = fmt.Errorf("boom")
		}
	}()
	a.total += n
	if n < 0 {
		panic("negative")
	}
	sum = a.total
	return
}

func GetPrintLn() func(string) {
	return func(src string) {
		fmt.Println(src)
	}
}

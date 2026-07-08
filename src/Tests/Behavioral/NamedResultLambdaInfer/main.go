package main

import (
	"errors"
	"fmt"
)

// parse reproduces crypto/x509 parseNameConstraintsExtension's `getValues` shape: a closure assigned
// to a local with MULTIPLE NAMED results where EVERY return arm carries a typeless element — the
// error arms return `nil, nil, err` (two untyped nils) and the success arm returns `e, o, nil` (one
// untyped nil). A C# tuple literal with any untyped element has no natural type, so no arm fixes the
// lambda's return type and delegate-type inference fails (CS8917). The fix states the lambda's tuple
// return type explicitly for a multi-result literal with no fully-typed arm — INCLUDING named
// results — so each nil takes its target element type.
func parse(items []int) (evens []int, odds []int, err error) {
	classify := func(vals []int) (e []int, o []int, err error) {
		for _, v := range vals {
			if v < 0 {
				return nil, nil, errors.New("negative value")
			}
			if v%2 == 0 {
				e = append(e, v)
			} else {
				o = append(o, v)
			}
		}
		return e, o, nil
	}

	return classify(items)
}

func main() {
	e, o, err := parse([]int{1, 2, 3, 4, 5, 6})
	fmt.Println(e, o, err)
}

package main

import (
	"fmt"
)

func main() {
	var kind Kind
	kind = Slice
	fmt.Printf("kind = %s\n", kind.String())

    var bitmap IntArgRegBitmap

    bitmap.Set(3)
    fmt.Printf("bitmap[0] = %t\n", bitmap.Get(0))
    fmt.Printf("bitmap[3] = %t\n", bitmap.Get(3))
}

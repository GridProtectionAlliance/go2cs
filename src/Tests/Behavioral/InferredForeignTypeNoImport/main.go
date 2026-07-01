// main.go imports ONLY fmt. `r`'s type is INFERRED *strings.Reader (never writing `strings.` textually),
// and the multi-name `var` forces the explicit type `ж<strings.Reader> r`, so the converter must supply
// the `using strings` alias without any strings import in this file.
package main

import "fmt"

func main() {
	var r, k = makeReader(), 5 // r : *strings.Reader (inferred) -> ж<strings.Reader>, needs `using strings`

	fmt.Println(r != nil) // true
	fmt.Println(k)        // 5
}

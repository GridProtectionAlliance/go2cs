package main

import "fmt"

type writer struct {
	out []string
}

// typ's range var shadows the method's parameter — which shares the METHOD's own name — so a
// name-keyed body rename also rewrote the recursive `w.typ(…)` selector; only genuine uses of
// the range variable may rename.
func (w *writer) typ(typ string) {
	w.out = append(w.out, "t:"+typ)

	if typ == "top" {
		for _, typ := range []string{"a", "b"} {
			w.typ(typ)
		}
	}
}

func main() {
	w := &writer{}
	w.typ("top")

	for _, line := range w.out {
		fmt.Println(line)
	}
}

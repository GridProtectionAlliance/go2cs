package main

import "fmt"

// A local declared by a tuple destructure (`list, delta := makeList()`) whose address is then taken
// (`use(&list)`) must be heap-boxed so its `Ꮡlist` companion exists. The combined deconstruction
// `var (list, delta) = makeList()` cannot create the box; the converter instead emits the escaping
// element's heap declaration first and a mixed deconstruction-assignment:
//   ref var list = ref heap<gList>(out var Ꮡlist);
//   (list, var delta) = makeList();
// Without this, `&list` emits `Ꮡlist` with no box (CS0103), and a `Ꮡ(value)` copy fallback would
// silently lose writes through the pointer. Mirrors runtime's `list, delta := netpoll(0);
// injectglist(&list)`. The mutate-through-pointer below proves the real local is updated.

type gList struct{ n int }

func makeList() (gList, int) { return gList{n: 7}, 3 }

func use(g *gList) { g.n++ } // writes through the pointer

func run() (int, int) {
	list, delta := makeList()
	use(&list) // &list -> Ꮡlist (the heap box), so the write lands on the real local
	use(&list)
	return list.n, delta // 9, 3
}

func main() {
	n, delta := run()
	fmt.Println(n, delta) // 9 3
}

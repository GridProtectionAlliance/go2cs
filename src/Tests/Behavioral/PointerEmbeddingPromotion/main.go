package main

import "fmt"

type leaf struct{ n int }

func (l *leaf) bump()    { l.n++ }
func (l *leaf) get() int { return l.n }

// One-level POINTER embed: holder embeds *leaf, so leaf's pointer-receiver methods (and its field
// `n`) are promoted onto holder.
type holder struct {
	*leaf
	tag string
}

// Two-level TRANSITIVE through a pointer embed: top embeds mid (value) which embeds *leaf (pointer).
// Go promotes leaf's methods all the way up to top. The generator must recurse through the nested
// pointer embed `*leaf` — its field type is `ж<leaf>`, whose simple name carries a `.val` suffix, so
// the embed-marker comparison must deref first. Mirrors runtime's
// `traceExpWriter`→`traceWriter`→`*traceBuf` (the `varint`/`byte` methods).
type mid struct {
	*leaf
}

type top struct {
	mid
	label string
}

func main() {
	h := holder{leaf: &leaf{n: 0}, tag: "h"}
	h.bump() // one-level: h.leaf.bump()
	h.bump()
	fmt.Println(h.get(), h.n, h.tag) // 2 2 h

	t := top{mid: mid{leaf: &leaf{n: 10}}, label: "t"}
	t.bump() // transitive: top -> mid -> *leaf -> leaf.bump()
	fmt.Println(t.get(), t.n, t.label) // 11 11 t
}

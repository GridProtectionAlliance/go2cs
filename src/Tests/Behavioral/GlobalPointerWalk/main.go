package main

import "fmt"

// Locks in the &GLOBAL / double-pointer family (runtime proc.go's allm walk, iface.go's
// itabTable, mheap.go's specialsIter):
//   - a package GLOBAL of POINTER type (`var head *node`) is identity-boxed when addressed:
//     `&head` must reference the REAL slot (`Ꮡhead`, a ж<ж<node>>), never box a copy — the
//     walk below REMOVES entries by writing `*pp = n.next` through it;
//   - `&(*pp).next` advances the walk by taking the address of a field THROUGH the double
//     pointer (`pp.val.of(node.Ꮡnext)`);
//   - `*i.pprev`-style single stars of a double pointer deref exactly ONE level;
//   - a method called through the pointer GLOBAL binds on the pointer value (the property),
//     not the slot box (`head.sum()` — ж<node> receiver, not ж<ж<node>>).

type node struct {
	val  int
	next *node
}

func (n *node) sum() int {
	total := 0
	for p := n; p != nil; p = p.next {
		total += p.val
	}
	return total
}

var head *node

// insert adds a node keeping ascending order, walking with a **node exactly like the
// runtime's allm insertion/removal pattern.
func insert(v int) {
	n := &node{val: v}
	pp := &head
	for *pp != nil && (*pp).val < v {
		pp = &(*pp).next
	}
	n.next = *pp
	*pp = n
}

// remove deletes the first node with the given value via the same walk; returns whether
// a node was removed. Removal through *pp is THE write-through proof: it must repoint
// either the global head slot or the previous node's next field.
func remove(v int) bool {
	for pp := &head; *pp != nil; pp = &(*pp).next {
		if (*pp).val == v {
			*pp = (*pp).next
			return true
		}
	}
	return false
}

func list() string {
	s := ""
	for p := head; p != nil; p = p.next {
		s += fmt.Sprintf("%d ", p.val)
	}
	return s
}

func main() {
	insert(30)
	insert(10)
	insert(20)
	insert(40)
	fmt.Println(list())       // 10 20 30 40
	fmt.Println(head.sum())   // 100 — method through the pointer global
	fmt.Println(remove(10))   // true — removes the HEAD (writes the global slot through *pp)
	fmt.Println(remove(30))   // true — removes a MIDDLE node (writes prev.next through *pp)
	fmt.Println(remove(99))   // false
	fmt.Println(list())       // 20 40
	fmt.Println(head.sum())   // 60
	insert(5)
	fmt.Println(list())       // 5 20 40 — head re-insertion through the slot
	fmt.Println(head.val)     // 5
}

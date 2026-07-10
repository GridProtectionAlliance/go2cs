package main

import "fmt"

type Inner struct {
	n int
}

type Thing struct {
	val   int
	inner Inner
}

type HasName interface {
	Name() string
}

type Tag struct {
	tag string
}

func (g Tag) Name() string {
	return g.tag
}

type Box struct {
	HasName
	id int
}

func (t *Thing) n() int {
	return t.val
}

// Sum is a VALUE-receiver method — called through a pointer local it must auto-deref.
func (t Thing) Sum() int {
	return t.val + t.inner.n
}

// PtrShadowCall: a pointer-to-struct local shadowing the receiver name calls a VALUE-receiver
// method (auto-deref) — the shadow must deref its box, not pose as the deref-aliased receiver.
func (t *Thing) PtrShadowCall(other *Thing) int {
	sum := t.Sum() // genuine receiver: value method on deref-aliased receiver
	{
		t := other
		sum += t.Sum() // shadow: needs the box deref
	}
	return sum
}

// PtrShadowChain: &shadow.inner.n — a value field-chain rooted at the shadowing pointer local
// must alias through the local's box, not copy via the receiver fallback.
func (t *Thing) PtrShadowChain(other *Thing) int {
	t.inner.n = 1
	{
		t := other
		q := &t.inner.n
		*q = 77 // must be visible through other
	}
	return t.inner.n // receiver's own field, still 1
}

// ValShadowField: &shadow.val on a HEAP-BOXED value-struct local shadowing the receiver
// name must field-ref through the local's box, not copy-box via the receiver form.
func (t *Thing) ValShadowField() int {
	t.val = 2
	got := 0
	{
		t := Thing{val: 7}
		q := &t // heap-boxes the local
		p := &t.val
		*p = 99
		got = q.n() // must read 99 through the box
	}
	return got + t.val
}

// SliceShadowIndex: &shadow[1] on a slice local shadowing the receiver name must alias the
// element, not box a copy of it.
func (t *Thing) SliceShadowIndex() int {
	{
		t := []int{10, 20, 30}
		p := &t[1]
		*p = 55
		return t[1] // must read 55
	}
}

// IfaceShadowEmbed: a method promoted through an embedded INTERFACE field, called on a
// pointer local shadowing the receiver name — the shadow needs the box-deref hop.
func (b *Box) IfaceShadowEmbed(other *Box) string {
	name := b.Name() // genuine receiver promotion
	{
		b := other
		name += b.Name() // shadow: deref hop through the local's box
	}
	return name
}

func main() {
	recv := &Thing{val: 5, inner: Inner{n: 3}}
	other := &Thing{val: 4, inner: Inner{n: 2}}

	fmt.Println(recv.PtrShadowCall(other))
	fmt.Println(recv.PtrShadowChain(other))
	fmt.Println(other.inner.n)
	fmt.Println(recv.ValShadowField())
	fmt.Println(recv.SliceShadowIndex())

	box := &Box{HasName: Tag{tag: "R"}, id: 1}
	obox := &Box{HasName: Tag{tag: "S"}, id: 2}
	fmt.Println(box.IfaceShadowEmbed(obox))
}

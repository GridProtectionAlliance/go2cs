package main

import "fmt"

// Guards the escape-analysis SelectorExpr arm: a value-struct LOCAL whose FIELD
// address is taken in plain assignment (or composite/return) position must be
// heap-boxed so the pointer aliases the local's own storage — the copy-box form
// `Ꮡ(x).of(T.Ꮡval)` silently drops writes made through the pointer.

type Thing struct {
	val int
}

type Wrap struct {
	inner Thing
	ptr   *Thing
}

// plain assignment position: write through &x.val, read back through x
func plainFunc() int {
	x := Thing{val: 7}
	p := &x.val
	*p = 99
	return x.val
}

// multi-hop value-field chain: &w.inner.val roots at w
func nestedChain() int {
	w := Wrap{inner: Thing{val: 5}}
	p := &w.inner.val
	*p = 42
	return w.inner.val
}

// negative control: the chain crosses a POINTER field, so w must NOT be boxed —
// the address aliases the pointee (t), and t sees the write
func pointerHop() int {
	t := Thing{val: 3}
	w := Wrap{ptr: &t}
	p := &w.ptr.val
	*p = 11
	return t.val
}

// same shape inside a method body
func (t *Thing) methodBody() int {
	y := Thing{val: 1}
	p := &y.val
	*p = 88
	return y.val
}

type Embed struct{ ev int }

type Outer struct {
	Embed
	other int
}

// field promoted through a VALUE embed still aliases the root local
func promotedValueEmbed() int {
	o := Outer{}
	p := &o.ev
	*p = 33
	return o.ev
}

type PtrHolder struct{ p *int }

// composite-literal position: the stored pointer aliases x
func compositePos() int {
	x := Thing{val: 2}
	h := PtrHolder{p: &x.val}
	*h.p = 55
	return x.val
}

// return position: the escaping field pointer stays writable/readable
func returnPos() *int {
	x := Thing{val: 4}
	return &x.val
}

func main() {
	fmt.Println(plainFunc())
	fmt.Println(nestedChain())
	fmt.Println(pointerHop())
	t := Thing{}
	fmt.Println(t.methodBody())
	fmt.Println(promotedValueEmbed())
	fmt.Println(compositePos())
	rp := returnPos()
	*rp = 66
	fmt.Println(*rp)
}

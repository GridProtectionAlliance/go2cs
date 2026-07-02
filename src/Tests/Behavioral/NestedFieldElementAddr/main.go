// Locks in the element-address of an array field reached through NESTED value fields of a pointer:
// `&pp.wbBuf.buf[0]` (runtime mwbbuf.go), the same inside a capturing closure (runtime trace.go
// `&mp.trace.buf[gen%2]`), and the 2-D form `&cache.entries[ck][i]` (runtime symtab.go). The
// element-address routing checked pointer-ness only ONE level up the chain, so a chain rooted at a
// pointer through intermediate VALUE fields fell to a naive `Ꮡ` prefix (`Ꮡpp.wbBuf…` — CS1061 on
// the box); a 2-D base is an IndexExpr the selector gate never saw. Both now route through the
// proven `&`-of-chain machinery: `pp.of(pstate.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0)` /
// `cache.at(cacheT.Ꮡentries, 1).at<nint>(2)`. Write-through verified against Go on every shape.
// (Arrays are explicitly initialized — a ZERO-VALUED struct's array-field backing is null in the
// C# emulation, a separate pre-existing latent.)
//
// Also: the element-address of a SLICE returned by a METHOD CALL — `&st.stk()[0]` (runtime
// mprof.go iterate_memprof's `&b.stk()[0]`). A slice-typed base without a bare identifier (a call
// result) fell out of the ident-gated slice arm into the ARRAY branch (a slice's type name also
// starts with "["), whose naive fallback textually prefixed `Ꮡ` onto the postfix chain —
// `Ꮡst.stk().at<uintptr>(0)` binds as `(Ꮡst).stk()…`, a nonexistent box (CS0103). The slice arm
// now takes ANY slice-typed base: `Ꮡ(st.stk(), 0)`. Write-through reaches the underlying storage
// (the returned slice VIEW shares its backing array per Go aliasing).

package main

import "fmt"

type wbBuf struct {
	next uintptr
	buf  [4]uintptr
}

type pstate struct {
	id    int
	wbBuf wbBuf
}

type cacheT struct {
	entries [2][3]int
}

type store struct {
	id   int
	data []uintptr
}

// a method returning a slice — the mprof `(*bucket).stk()` shape
func (s *store) stk() []uintptr { return s.data }

//go:noinline
func bump(p *uintptr) { *p = *p + 7 }

//go:noinline
func bumpInt(p *int) { *p = *p + 3 }

func main() {
	// mwbbuf shape: &ptr.valueField.arrayField[i] (arrays explicitly initialized — the
	// zero-value struct array-field backing is a separate, pre-existing latent)
	pp := &pstate{wbBuf: wbBuf{buf: [4]uintptr{10, 20, 30, 40}}}
	bump(&pp.wbBuf.buf[0])
	fmt.Println(pp.wbBuf.buf[0], pp.wbBuf.buf[1]) // 17 20

	// trace shape (via closure capture): &captured.valueField.arrayField[i]
	got := uintptr(0)
	func() {
		bump(&pp.wbBuf.buf[1])
		got = pp.wbBuf.buf[1]
	}()
	fmt.Println(got) // 27

	// symtab shape: &ptr.field[j][i] (2-D)
	cache := &cacheT{entries: [2][3]int{{1, 2, 3}, {4, 5, 6}}}
	bumpInt(&cache.entries[1][2])
	fmt.Println(cache.entries[1][2], cache.entries[0][0]) // 9 1

	// mprof shape: &ptr.method()[i] — element address of a method-call-result slice via a
	// pointer local; the write reaches the shared backing storage
	st := &store{id: 5, data: []uintptr{11, 22, 33}}
	bump(&st.stk()[0])
	fmt.Println(st.data[0], st.stk()[0]) // 18 18 — write-through via the returned slice view
	bump(&st.stk()[2])
	fmt.Println(st.data[2]) // 40
}

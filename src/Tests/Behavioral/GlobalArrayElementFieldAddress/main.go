// GlobalArrayElementFieldAddress guards taking the address of a NESTED field of an
// indexed element of an address-taken package global ARRAY — `&pool[i].inner.mu`. The
// address must chain `Ꮡpool.at<T>(i).of(item.Ꮡinner).of(sub.Ꮡmu)`; the converter must
// recurse on the IndexExpr base rather than prefixing `Ꮡ` onto `pool[i]` (which binds
// to the box variable `Ꮡpool`, indexed → CS0021). It also covers an ANONYMOUS-struct
// element, whose lifted type name must be resolved for the `at<…>` form (a raw
// `struct{…}` would be an invalid-C# parse error that masks the whole assembly).
// Mirrors runtime's `&stackpool[i].item.mu` (stack.go) and `&mheap.central[i].mcentral`
// (mgcsweep.go).
package main

import "fmt"

type sub struct{ mu, n int }

type item struct {
	inner sub
}

var pool [3]item // global array, named element type

// anonymous-struct element type (lifted to a synthesized name)
var grid [3]struct {
	cell sub
	pad  [4]byte
}

//go:noinline
func setInt(p *int) { *p = 7 }

func main() {
	pool[1].inner.n = 5
	setInt(&pool[1].inner.mu) // named element
	setInt(&pool[2].inner.mu)

	grid[0].cell.n = 9
	setInt(&grid[0].cell.mu) // anonymous-struct element

	fmt.Println(pool[1].inner.mu, pool[1].inner.n, pool[2].inner.mu) // 7 5 7
	fmt.Println(grid[0].cell.mu, grid[0].cell.n)                     // 7 9
}

// SparseArrayNamedIntKey guards the converter's handling of a keyed slice/array
// literal whose index keys are constants of a DEFINED integer type. Such a literal
// is emitted as a SparseArray<T> with an int indexer; a key whose Go type is
// `int`/`int64`/`uint`/`uint32`/`uint64`/`uintptr` cannot implicitly narrow to that
// int parameter (CS1503), so the converter casts it to `(int)`. A key type that
// widens to int (here `uint8`) is left uncast. Mirrors runtime/lockrank.go's
// `lockNames` (a `[]string` keyed by `lockRank`, a `type lockRank int`).
package main

import "fmt"

type rank int // defined over int -> [GoType("num:nint")]; key needs the (int) cast

const (
	rankLow rank = iota
	rankMid
	rankHigh
)

type code uint8 // defined over uint8 -> widens to int; key is left uncast

const (
	codeA code = iota
	codeB
)

var rankNames = []string{
	rankLow:  "low",
	rankMid:  "mid",
	rankHigh: "high",
}

var codeNames = []string{
	codeA: "a",
	codeB: "b",
}

func main() {
	fmt.Println(rankNames[rankLow], rankNames[rankMid], rankNames[rankHigh]) // low mid high
	fmt.Println(codeNames[codeA], codeNames[codeB])                          // a b
	fmt.Println(len(rankNames))                                              // 3
}

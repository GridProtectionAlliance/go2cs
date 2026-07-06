// Guards a methodless named func TYPE whose parameter list carries a named MAP parameter, used as
// a function parameter so the type collapses to its base C# delegate. The delegate's parameter
// types are rendered by convertToCSFullTypeName's func-handler: extractTypes already returns each
// param in C# form (`map<@string, ж<Node>>`), so a SECOND convertToCSTypeName pass re-fed that
// through the `map<` arm's splitMapKeyValue, mis-parsing it to `map<@string, ж<Node>, >` — a spurious
// trailing empty type arg (CS1031 "Type expected"). Same shape as go/ast's `type Importer
// func(imports map[string]*Object, path string) (pkg *Object, err error)` used by NewPackage.
package main

import "fmt"

type Node struct {
	name string
}

// methodless named func type: named MAP param + a plain param + named results.
type Importer func(imports map[string]*Node, path string) (pkg *Node, err error)

// importer's type collapses to the base delegate here (the double-conversion site).
func newPackage(importer Importer, files map[string]*Node) *Node {
	n, _ := importer(files, "root")
	return n
}

// a named function of the same signature — passed as the Importer value.
func lookup(imports map[string]*Node, path string) (pkg *Node, err error) {
	return imports["a"], nil
}

func main() {
	files := map[string]*Node{"a": {name: "alpha"}}
	pkg := newPackage(lookup, files)
	fmt.Println(pkg.name) // alpha
}

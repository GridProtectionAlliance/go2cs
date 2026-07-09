// Synthesized-delegate cross-package rename guard: passing a package-qualified func where a
// methodless named func type is expected synthesizes `new Func<…>(arg)` — and the delegate's
// element types must render through the foreign-rename alias table even when THIS package never
// imports the element's package. go/types hit this with `ast.Fprint(…, ast.NotNilFilter)`:
// FieldFilter's `reflect.Value` (ΔValue in reflect) rendered raw (`reflect.Value`, CS0426) and
// the delegate then failed the method-group conversion (CS0123). Here CrossPkgFuncLib.Picker's
// signature names CrossPkgLib.Status (ΔStatus), and this package imports only CrossPkgFuncLib.
package main

import (
	"fmt"

	"CrossPkgFuncLib"
)

func main() {
	// Method-group conversion into the named func type parameter: the wrap must render
	// `new Func<CrossPkgLibꓸStatus, bool>(CrossPkgFuncLib.Hot)`.
	fmt.Println(CrossPkgFuncLib.Count(CrossPkgFuncLib.Hot))

	// A local of the named func type, assigned the foreign func, then passed.
	var pick CrossPkgFuncLib.Picker = CrossPkgFuncLib.Hot
	fmt.Println(CrossPkgFuncLib.Count(pick))
}

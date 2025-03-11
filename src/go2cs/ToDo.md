# To Do List

01) Update README / Conversion Strategies to reflect recent work

## For "go2cs2" transpiler:

01) ~~Fix out of order shadowing, when higher scope variable is declared later but prior variable should have been shadowed~~
02) ~~Add captured lambda variables to temporary variables before lambda call and use temp variabes in lambda (Go always copied) - make sure array is full copy~~
03) ~~Fix range implementation to account for define / assign~~
04) ~~Check implementation of standalone `convEllipsis` visitor - add test code for when is it encountered - or remove~~
05) ~~Check implementation of standalone `convFieldList` visitor - add test code for when is it encountered - or remove~~
06) ~~Check implementation of standalone `convInterfaceType` visitor - add test code for when is it encountered - or remove~~
07) ~~Check implementation of standalone `convStructType` visitor - add test code for when is it encountered - or remove~~
08) Complete map type implementation (`visitMapType`)
09) ~~Complete type switch implementation (`visitTypeSwitchStmt`) -- see `visitSwitchStmt`~~
10) ~~Complete select statement implementation (`visitSelectStmt`) Handle edge cases~~
  a) ~~Handle `case i3, ok := (<-c3):  // same as: i3, ok := <-c`~~
  b) ~~Handle `case a[f()] = <-c4: // same as: case t := <-c4 { a[f()] = t }`~~
  c) ~~Handle multi-valued assignment form of (with OK to test for closed channel).~~
  d) ~~Test `nil` channel which is never ready for communication~~
  e) ~~Handle channels with specified direction (send or receive)~~
11) ~~Complete send statement implementation (`visitSendStmt`)~~
12) ~~Complete struct interfaces and embedding (will need C# GoType code converter work)~~
13) ~~Complete interface inheritance (will need C# GoType code converter work)~~
14) ~~Complete channel implementation (`visitChanType` / `visitCommClause`)~~
  a) ~~Suspected complete through existing paths - add test code for when is it encountered - or remove~~
15) ~~Handle dynamic struct type lifting~~
  a) Implement remaining dynamic struct implicit cast checks `v.checkForDynamicStructs(argType, targetType)` in the following visitors: `AssignStmt`, `CompositeLit`, `IndexExpr`, `BinaryExpr`, `UnaryExpr`, `SelectorExpr`, `TypeSwitchStmt`, `ValueSpec` 
16) ~~Handle intra-function type declaration lifting (see TypeInference)~~
17) ~~Handle generics conversion~~
18) ~~Always include pre-package comments during conversion~~
19) Reduce `return ~Ꮡ(new errorString(text));` expressions to `return new errorString(text);`
20) ~~Check using imports in generated related to spread operators - needs fully qualify namespaces - see if Roslyn can expand these in source generation~~
21) For manually converted code, e.g., `unsafe` package, need to let transpiler which files to ignore during conversion - may be a standard list from standard library
22) Handle certain `unsafe` function conversions manually, e.g., `Offsetof` and `Alignof` which need some special parameter handling, e.g.: `unsafe.Offsetof(x)` to `@unsafe.Offsetof(x.GetType())`
22) ~~For package names, e.g., `unsafe_package` - currently prefix is being unnecessarily sanitized, e.g., `@unsafe_package` - need to check name as a whole~~
23) ~~Left side of an assign with a pointer de-ref, e.g., `(~e)` needs to be `e.val` instead~~
24) Update auto-interface implementation to ignore various invalid implement targets

xx) Setup reference code packages / path options for Go modules
    1) Assume code builds in Go / toolchain executed, i.e., local source exists
    2) Define target location for converted code, e.g., `$GOPATH/../go2cs/pkg/mod`
    3) Match original sub-path path for source starting from new root, e.g.:
       a) If original path is: `$GOPATH/pkg/mod/github.com/cosiner\argv@v0.1.0`
       b) Converted path is: `$GOPATH/../go2cs/pkg/mod/github.com/cosiner\argv@v0.1.0`
xx) ~~Add support for comment based directives~~
xx) Add support for "cgo" targets
xx) Complete code comment conversions, this may be predicated on the following:
    1) Some code exists for this, but it was becoming cumbersome to do correctly
    2) There is an ongoing effort to improve AST for parsing free floating comments:
       a) https://github.com/golang/go/issues/20744 
       b) Alternately there is a DST package that can handle this
xx) Add suport for Go assembler targets (*.s files)
    1) Current thinking is to let Go compile to object code for a platform then
       wrap in a .dll/.so/.dynlib with callable entry points options from C# code
    2) Note that for Go library there are often pure Go implementations to lean on initally

~~Fix the following sparse array initialization use case - see "Interface Implementation":~~
~~```go~~
~~vowels := [128]bool{'a': true, 'e': true, 'i': true, 'o': true, 'u': true, 'y': true}~~
~~```~~
~~```cs~~
~~var vowels = new bool[]{vowels['a'] = true, vowels['e'] = true, vowels['i'] = true, vowels['o'] = true, vowels['u'] = true, vowels['y'] = true}.array();~~
~~```~~
~~Fix lambda shadow and missed shadow -- see "First Class Functions"~~

## For C# code operations:

01) ~~Restructure T4 templates for Roslyn type inputs (or simplified versions) since using code will only be source generators~~
    ~~1) This will involve dropping unused code from go2cs.Common~~
02) Update source generators to accommodate remaining GoType attribute implementations, e.g.:
    1) ~~Struct embedding (inheritance)~~
    2) ~~Struct interface implementations~~
    3) ~~Interface inheritance~~
    4) map type definitions (IMap implementation)
    5) channel type definitions (IChannel implementation)
    6) other...
03) ~~Remove current C# version of go2cs - determine if any should remain as proxy to Go version~~
    ~~1) Remove related dependencies, e.g., Antlr / command line parsing code~~
04) ~~Restructure behavioral tests:~~
    1) ~~Mode to compare raw code to target file, ignoring comments~~
       ~~1) Set this up soon to better handle regression testing of go2cs changes~~
    2) Future tests can be setup to compare with comments once go2c2 has better support
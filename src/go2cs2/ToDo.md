# To Do List

01) Update README / Conversion Strategies to reflect recent work

## For "go2cs2" transpiler:

01) ~~Fix out of order shadowing, when higher scope variable is declared later but prior variable should have been shadowed~~
02) Add captured lambda variables to temporary variables before lambda call and use temp variabes in lambda (Go always copied) - make sure array is full copy
03) ~~Fix range implementation to account for define / assign~~
04) ~~Check implementation of standalone `convEllipsis` visitor - add test code for when is it encountered - or remove~~
05) Check implementation of standalone `convFieldList` visitor - add test code for when is it encountered - or remove
06) Check implementation of standalone `convInterfaceType` visitor - add test code for when is it encountered - or remove
07) Check implementation of standalone `convStructType` visitor - add test code for when is it encountered - or remove
08) Complete map type implementation (`visitMapType`)
09) Complete type switch implementation (`visitTypeSwitchStmt`) -- see `visitSwitchStmt`
10) Complete select statement implementation (`visitSelectStmt`)
11) Complete send statement implementation (`visitSendStmt`)
12) Complete struct interfaces and embedding (will need C# GoType code converter work)
13) Complete interface inheritance (will need C# GoType code converter work)
14) Complete channel implementation (`visitChanType` / `visitCommClause`)
15) Handle generics conversion

xx) Setup reference code packages / path options for Go modules
    1) Assume code builds in Go / toolchain executed, i.e., local source exists
    2) Define target location for converted code, e.g., `$GOPATH/pkg/mod/go2cs`
    3) Match original sub-path path for source starting from new root, e.g.:
       a) If original path is: `$GOPATH/pkg/mod/github.com/cosiner\argv@v0.1.0`
       b) Converted path is: `$GOPATH/pkg/mod/go2cs/github.com/cosiner\argv@v0.1.0`
xx) Add support for comment based directives
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

## For C# code operations:

01) Restructure T4 templates for Roslyn type inputs (or simplified versions) since using code will only be source generators
    1) This will involve dropping unused code from go2cs.Common
02) Update source generators to accommodate remaining GoType attribute implementations, e.g.:
    1) Struct embedding (inheritance)
    2) Struct interface implementations
    3) Interface inheritance
    4) map type definitions (IMap implementation)
    5) channel type definitions (IChannel implementation)
    6) other...
03) Remove current C# version of go2cs - determine if any should remain as proxy to Go version
    1) Remove related dependencies, e.g., Antlr / command line parsing code
04) Restructure behavioral tests:
    1) Mode to compare raw code to target file, ignoring comments
       1) Set this up soon to better handle regression testing of go2cs changes
    2) Future tests can be setup to compare with comments once go2c2 has better support
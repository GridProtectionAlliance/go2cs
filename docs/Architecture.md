# go2cs Architecture

Companion to [`/CLAUDE.md`](../CLAUDE.md). Detailed map of the converter pipeline, the `visit*`/`conv*`
file taxonomy, the analysis passes, the Roslyn source generators, and the runtime type map.

## Pipeline overview

```
Go source ──► go/parser + go/types ──► typed AST ──► analysis passes ──► visit*/conv* emit ──► C# files + .csproj
              (official Go toolchain)                 (escape, shadow,                          (per package)
                                                       collisions, generics)
```

The converter (`src/go2cs/`, Go) leans on Go's own front-end (`go/ast`, `go/types`, `go/token`,
`go/constant`, `golang.org/x/tools/go/packages`) for parsing and full semantic type information, then
walks the typed AST emitting C#. Because types are fully resolved, conversion decisions (overload
selection, implicit conversions, constant folding, unsigned detection) are semantic, not syntactic.

- **Entry point:** `main.go` — flag parsing, GOROOT/GOPATH resolution, single-file/dir vs `-stdlib` mode.
- **Symbol constants:** `symbols.go` — the cross-language naming/marker constants (`RootNamespace`,
  `PackageSuffix`, `PointerPrefix` `ж`, `AddressPrefix` `Ꮡ`, …). Generated — together with its C# twin
  `src/core/go2cs/Symbols.cs` (class `go2cs.Symbols`, shared into golib and the `go2cs-gen` analyzer via
  `go2cs.projitems`) — from the **canonical symbol table `src/core/go2cs/symbols.json`** by
  `internal/gensymbols`. Edit the JSON, never the generated files; regenerate with `go generate .` from
  `src/go2cs`, or run `src/check-symbol-sync.ps1` (regenerates + exits 1 on drift).
- **Stdlib driver:** `stdLibConverter.go` — scans stdlib packages, builds the dependency graph, produces a
  topologically sorted queue (`sortedQueue`), and converts in dependency order (optionally filtered to a
  package list). Conversion is sequential — it relies on package-level converter state, so a converted
  importer must observe its dependency's finished `package_info.cs`. The topo order is reusable for bottom-up builds.
- **Output:** `writeOperations.go` emits `.cs` files and generates each package's `.csproj` from a template,
  substituting markers (project references derived from detected imports, `unsafe` usage flag, etc.).

## `visit*.go` — AST node → C# declaration/statement

Each `visit*` file handles one Go AST node category, emitting the corresponding C# construct.

| File | Handles |
|---|---|
| `visitFile.go` | Top-level file: namespace, usings, package partial class. |
| `visitDecl.go` / `visitGenDecl.go` / `visitFuncDecl.go` | Declarations: general (var/const/type/import), functions & methods (receivers → `[GoRecv]`). |
| `visitTypeSpec.go` / `visitValueSpec.go` / `visitImportSpec.go` | Type/value/import specs. |
| `visitStructType.go` / `visitInterfaceType.go` | Struct & interface type defs (embedding, method sets → generators). |
| `visitArrayType.go` / `visitMapType.go` / `visitChanType.go` / `visitFuncType.go` | Composite type forms. |
| `visitStmt.go` / `visitBlockStmt.go` / `visitDeclStmt.go` / `visitExprStmt.go` | Statement dispatch & blocks. |
| `visitAssignStmt.go` / `visitIncDecStmt.go` / `visitSendStmt.go` | Assignment, `++/--`, channel send. |
| `visitIfStmt.go` / `visitForStmt.go` / `visitRangeStmt.go` | Conditionals & loops (range handles ptr deref). |
| `visitSwitchStmt.go` / `visitTypeSwitchStmt.go` / `visitSelectStmt.go` / `visitCommClause.go` | switch / type-switch / select + comm clauses. |
| `visitReturnStmt.go` / `visitBranchStmt.go` / `visitLabeledStmt.go` | return, break/continue/goto, labels. |
| `visitDeferStmt.go` / `visitGoStmt.go` | `defer` (LIFO closure stack), `go` (goroutine → thread pool/Task). |
| `visitIdent.go` | Identifier emission / name sanitization. |

## `conv*.go` — expression & type converters

| File | Converts |
|---|---|
| `convExpr.go` / `convExprList.go` | Expression dispatch / lists. |
| `convBasicLit.go` / `convCompositeLit.go` / `convFuncLit.go` / `convKeyValueExpr.go` | Literals & composites. |
| `convIdent.go` / `convSelectorExpr.go` / `convParenExpr.go` | Identifiers, selectors, parentheses. |
| `convCallExpr.go` / `convIndexExpr.go` / `convIndexListExpr.go` / `convSliceExpr.go` | Calls, indexing, generic index lists, slicing. |
| `convStarExpr.go` / `convUnaryExpr.go` / `convBinaryExpr.go` | Pointer deref / unary / binary ops. |
| `convTypeAssertExpr.go` | Type assertions (single-value panic vs `(value, ok)` overloads). |
| `convArrayType.go` / `convMapType.go` / `convChanType.go` / `convFuncType.go` | Type expressions. |
| `convStructType.go` / `convInterfaceType.go` | Struct/interface type expressions. |

Supporting: `Stack.go`, `HashSet.go` (generic data structures), `directiveOperations.go` (build tags /
`+build ignore` / cgo directives).

## Analysis passes

- `escapeAnalysisOperations.go` — stack-vs-heap escape detection, informs allocation strategy (`ж<T>` boxing).
- `variableAnalysisOperations.go` — lexical scope + shadowing; generates save/restore (`i__prev1` …) so
  Go's shadowing semantics survive in C#.
- `nameCollisionAnalysisOperations.go` — resolves C#-keyword / cross-symbol name collisions.
- `constraintOperations.go` — generic type constraint conversion (Go 1.18+).
- `importOperations.go` — import resolution & dependency tracking; drives generated `.csproj` references.

## Source generators (`src/gen/go2cs-gen/`, Roslyn)

Compile-time emission so converted C# stays visually close to Go. Referenced as an analyzer
(`OutputItemType="Analyzer"`) by every converted project.

| Generator | Trigger | Produces |
|---|---|---|
| `ImplementGenerator` | `[assembly: GoImplement<TStruct, TInterface>]` | Explicit interface implementations / promoted methods (Go duck-typing). |
| `RecvGenerator` | `[GoRecv]` on methods | Value/pointer receiver overloads handling `ptr<T>`/`ж<T>` deref. |
| `ImplicitConvGenerator` | `[GoImplicitConv]` | Implicit conversion operators between Go type aliases. |
| `TypeGenerator` | `[GoType]` | Wrapper types + field promotion for struct embedding. |

## Runtime type map (`src/core/golib/`)

| Go | C# (golib) |
|---|---|
| slice | `slice<T>` (array + low/high/cap) — `slice.cs` |
| array | `array<T>` — `array.cs` |
| map | `map<K,V>` (Dictionary + lock) — `map.cs` |
| channel | `channel<T>` (queue + wait handles) — `channel.cs` |
| string | `@string` (UTF-8 backed); experimental ref-struct `sstring` — `string.cs` |
| `interface{}` / `any` | `object` |
| builtins (`append`,`len`,`cap`,`make`,`copy`,`panic`,`recover`,`close`,`delete`,`...` spread) | `builtin.cs` (the large one) |
| `nil` | `NilType` + `null` for heap refs |
| pointer-to-value boxing | `ж<T>` heap box |
| `int`/`uint` (platform-sized) | `nint`/`nuint` |
| `uint8`/`rune`/`uintptr`/`complex128`/… | aliases (`byte`/`int32`/`UIntPtr`/`Complex`) via global usings |
| panic / runtime error | `PanicException` / `RuntimeErrorPanic` |

See [`ConversionStrategies.md`](ConversionStrategies.md) for a high-level, example-driven tour of the
per-construct mapping, and [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md) for the
exhaustive rationale and edge cases behind each one.

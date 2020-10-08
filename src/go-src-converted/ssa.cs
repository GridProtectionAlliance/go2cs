// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:57:10 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\ssa.go
// This package defines a high-level intermediate representation for
// Go programs using static single-assignment (SSA) form.

using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using sync = go.sync_package;

using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // A Program is a partial or complete Go program converted to SSA form.
        public partial struct Program
        {
            public ptr<token.FileSet> Fset; // position information for the files of this Program
            public map<@string, ptr<Package>> imported; // all importable Packages, keyed by import path
            public map<ptr<types.Package>, ptr<Package>> packages; // all loaded Packages, keyed by object
            public BuilderMode mode; // set of mode bits for SSA construction
            public typeutil.MethodSetCache MethodSets; // cache of type-checker's method-sets

            public sync.Mutex methodsMu; // guards the following maps:
            public typeutil.Map methodSets; // maps type to its concrete methodSet
            public typeutil.Map runtimeTypes; // types for which rtypes are needed
            public typeutil.Map canon; // type canonicalization map
            public map<ptr<types.Func>, ptr<Function>> bounds; // bounds for curried x.Method closures
            public map<selectionKey, ptr<Function>> thunks; // thunks for T.Method expressions
        }

        // A Package is a single analyzed Go package containing Members for
        // all package-level functions, variables, constants and types it
        // declares.  These may be accessed directly via Members, or via the
        // type-specific accessor methods Func, Type, Var and Const.
        //
        // Members also contains entries for "init" (the synthetic package
        // initializer) and "init#%d", the nth declared init function,
        // and unspecified other things too.
        //
        public partial struct Package
        {
            public ptr<Program> Prog; // the owning program
            public ptr<types.Package> Pkg; // the corresponding go/types.Package
            public map<@string, Member> Members; // all package members keyed by name (incl. init and init#%d)
            public map<types.Object, Value> values; // package members (incl. types and methods), keyed by object
            public ptr<Function> init; // Func("init"); the package's init function
            public bool debug; // include full debug info in this package

// The following fields are set transiently, then cleared
// after building.
            public sync.Once buildOnce; // ensures package building occurs once
            public int ninit; // number of init functions
            public ptr<types.Info> info; // package type information
            public slice<ptr<ast.File>> files; // package ASTs
        }

        // A Member is a member of a Go package, implemented by *NamedConst,
        // *Global, *Function, or *Type; they are created by package-level
        // const, var, func and type declarations respectively.
        //
        public partial interface Member
        {
            ptr<Package> Name(); // declared name of the package member
            ptr<Package> String(); // package-qualified name of the package member
            ptr<Package> RelString(ptr<types.Package> _p0); // like String, but relative refs are unqualified
            ptr<Package> Object(); // typechecker's object for this member, if any
            ptr<Package> Pos(); // position of member's declaration, if known
            ptr<Package> Type(); // type of the package member
            ptr<Package> Token(); // token.{VAR,FUNC,CONST,TYPE}
            ptr<Package> Package(); // the containing package
        }

        // A Type is a Member of a Package representing a package-level named type.
        public partial struct Type
        {
            public ptr<types.TypeName> @object;
            public ptr<Package> pkg;
        }

        // A NamedConst is a Member of a Package representing a package-level
        // named constant.
        //
        // Pos() returns the position of the declaring ast.ValueSpec.Names[*]
        // identifier.
        //
        // NB: a NamedConst is not a Value; it contains a constant Value, which
        // it augments with the name and position of its 'const' declaration.
        //
        public partial struct NamedConst
        {
            public ptr<types.Const> @object;
            public ptr<Const> Value;
            public ptr<Package> pkg;
        }

        // A Value is an SSA value that can be referenced by an instruction.
        public partial interface Value
        {
            token.Pos Name(); // If this value is an Instruction, String returns its
// disassembled form; otherwise it returns unspecified
// human-readable information about the Value, such as its
// kind, name and type.
            token.Pos String(); // Type returns the type of this value.  Many instructions
// (e.g. IndexAddr) change their behaviour depending on the
// types of their operands.
            token.Pos Type(); // Parent returns the function to which this Value belongs.
// It returns nil for named Functions, Builtin, Const and Global.
            token.Pos Parent(); // Referrers returns the list of instructions that have this
// value as one of their operands; it may contain duplicates
// if an instruction has a repeated operand.
//
// Referrers actually returns a pointer through which the
// caller may perform mutations to the object's state.
//
// Referrers is currently only defined if Parent()!=nil,
// i.e. for the function-local values FreeVar, Parameter,
// Functions (iff anonymous) and all value-defining instructions.
// It returns nil for named Functions, Builtin, Const and Global.
//
// Instruction.Operands contains the inverse of this relation.
            token.Pos Referrers(); // Pos returns the location of the AST token most closely
// associated with the operation that gave rise to this value,
// or token.NoPos if it was not explicit in the source.
//
// For each ast.Node type, a particular token is designated as
// the closest location for the expression, e.g. the Lparen
// for an *ast.CallExpr.  This permits a compact but
// approximate mapping from Values to source positions for use
// in diagnostic messages, for example.
//
// (Do not use this position to determine which Value
// corresponds to an ast.Expr; use Function.ValueForExpr
// instead.  NB: it requires that the function was built with
// debug information.)
            token.Pos Pos();
        }

        // An Instruction is an SSA instruction that computes a new Value or
        // has some effect.
        //
        // An Instruction that defines a value (e.g. BinOp) also implements
        // the Value interface; an Instruction that only has an effect (e.g. Store)
        // does not.
        //
        public partial interface Instruction
        {
            token.Pos String(); // Parent returns the function to which this instruction
// belongs.
            token.Pos Parent(); // Block returns the basic block to which this instruction
// belongs.
            token.Pos Block(); // setBlock sets the basic block to which this instruction belongs.
            token.Pos setBlock(ptr<BasicBlock> _p0); // Operands returns the operands of this instruction: the
// set of Values it references.
//
// Specifically, it appends their addresses to rands, a
// user-provided slice, and returns the resulting slice,
// permitting avoidance of memory allocation.
//
// The operands are appended in undefined order, but the order
// is consistent for a given Instruction; the addresses are
// always non-nil but may point to a nil Value.  Clients may
// store through the pointers, e.g. to effect a value
// renaming.
//
// Value.Referrers is a subset of the inverse of this
// relation.  (Referrers are not tracked for all types of
// Values.)
            token.Pos Operands(slice<ptr<Value>> rands); // Pos returns the location of the AST token most closely
// associated with the operation that gave rise to this
// instruction, or token.NoPos if it was not explicit in the
// source.
//
// For each ast.Node type, a particular token is designated as
// the closest location for the expression, e.g. the Go token
// for an *ast.GoStmt.  This permits a compact but approximate
// mapping from Instructions to source positions for use in
// diagnostic messages, for example.
//
// (Do not use this position to determine which Instruction
// corresponds to an ast.Expr; see the notes for Value.Pos.
// This position may be used to determine which non-Value
// Instruction corresponds to some ast.Stmts, but not all: If
// and Jump instructions have no Pos(), for example.)
            token.Pos Pos();
        }

        // A Node is a node in the SSA value graph.  Every concrete type that
        // implements Node is also either a Value, an Instruction, or both.
        //
        // Node contains the methods common to Value and Instruction, plus the
        // Operands and Referrers methods generalized to return nil for
        // non-Instructions and non-Values, respectively.
        //
        // Node is provided to simplify SSA graph algorithms.  Clients should
        // use the more specific and informative Value or Instruction
        // interfaces where appropriate.
        //
        public partial interface Node
        {
            ptr<slice<Instruction>> String();
            ptr<slice<Instruction>> Pos();
            ptr<slice<Instruction>> Parent(); // Partial methods:
            ptr<slice<Instruction>> Operands(slice<ptr<Value>> rands); // nil for non-Instructions
            ptr<slice<Instruction>> Referrers(); // nil for non-Values
        }

        // Function represents the parameters, results, and code of a function
        // or method.
        //
        // If Blocks is nil, this indicates an external function for which no
        // Go source code is available.  In this case, FreeVars and Locals
        // are nil too.  Clients performing whole-program analysis must
        // handle external functions specially.
        //
        // Blocks contains the function's control-flow graph (CFG).
        // Blocks[0] is the function entry point; block order is not otherwise
        // semantically significant, though it may affect the readability of
        // the disassembly.
        // To iterate over the blocks in dominance order, use DomPreorder().
        //
        // Recover is an optional second entry point to which control resumes
        // after a recovered panic.  The Recover block may contain only a return
        // statement, preceded by a load of the function's named return
        // parameters, if any.
        //
        // A nested function (Parent()!=nil) that refers to one or more
        // lexically enclosing local variables ("free variables") has FreeVars.
        // Such functions cannot be called directly but require a
        // value created by MakeClosure which, via its Bindings, supplies
        // values for these parameters.
        //
        // If the function is a method (Signature.Recv() != nil) then the first
        // element of Params is the receiver parameter.
        //
        // A Go package may declare many functions called "init".
        // For each one, Object().Name() returns "init" but Name() returns
        // "init#1", etc, in declaration order.
        //
        // Pos() returns the declaring ast.FuncLit.Type.Func or the position
        // of the ast.FuncDecl.Name, if the function was explicit in the
        // source.  Synthetic wrappers, for which Synthetic != "", may share
        // the same position as the function they wrap.
        // Syntax.Pos() always returns the position of the declaring "func" token.
        //
        // Type() returns the function's Signature.
        //
        public partial struct Function
        {
            public @string name;
            public types.Object @object; // a declared *types.Func or one of its wrappers
            public ptr<types.Selection> method; // info about provenance of synthetic methods
            public ptr<types.Signature> Signature;
            public token.Pos pos;
            public @string Synthetic; // provenance of synthetic function; "" for true source functions
            public ast.Node syntax; // *ast.Func{Decl,Lit}; replaced with simple ast.Node after build, unless debug mode
            public ptr<Function> parent; // enclosing function if anon; nil if global
            public ptr<Package> Pkg; // enclosing package; nil for shared funcs (wrappers and error.Error)
            public ptr<Program> Prog; // enclosing program
            public slice<ptr<Parameter>> Params; // function parameters; for methods, includes receiver
            public slice<ptr<FreeVar>> FreeVars; // free variables whose values must be supplied by closure
            public slice<ptr<Alloc>> Locals; // local variables of this function
            public slice<ptr<BasicBlock>> Blocks; // basic blocks of the function; nil => external
            public ptr<BasicBlock> Recover; // optional; control transfers here after recovered panic
            public slice<ptr<Function>> AnonFuncs; // anonymous functions directly beneath this one
            public slice<Instruction> referrers; // referring instructions (iff Parent() != nil)

// The following fields are set transiently during building,
// then cleared.
            public ptr<BasicBlock> currentBlock; // where to emit code
            public map<types.Object, Value> objects; // addresses of local variables
            public slice<ptr<Alloc>> namedResults; // tuple of named results
            public ptr<targets> targets; // linked stack of branch targets
            public map<ptr<ast.Object>, ptr<lblock>> lblocks; // labelled blocks
        }

        // BasicBlock represents an SSA basic block.
        //
        // The final element of Instrs is always an explicit transfer of
        // control (If, Jump, Return, or Panic).
        //
        // A block may contain no Instructions only if it is unreachable,
        // i.e., Preds is nil.  Empty blocks are typically pruned.
        //
        // BasicBlocks and their Preds/Succs relation form a (possibly cyclic)
        // graph independent of the SSA Value graph: the control-flow graph or
        // CFG.  It is illegal for multiple edges to exist between the same
        // pair of blocks.
        //
        // Each BasicBlock is also a node in the dominator tree of the CFG.
        // The tree may be navigated using Idom()/Dominees() and queried using
        // Dominates().
        //
        // The order of Preds and Succs is significant (to Phi and If
        // instructions, respectively).
        //
        public partial struct BasicBlock
        {
            public long Index; // index of this block within Parent().Blocks
            public @string Comment; // optional label; no semantic significance
            public ptr<Function> parent; // parent function
            public slice<Instruction> Instrs; // instructions in order
            public slice<ptr<BasicBlock>> Preds; // predecessors and successors
            public slice<ptr<BasicBlock>> Succs; // predecessors and successors
            public array<ptr<BasicBlock>> succs2; // initial space for Succs
            public domInfo dom; // dominator tree info
            public long gaps; // number of nil Instrs (transient)
            public long rundefers; // number of rundefers (transient)
        }

        // Pure values ----------------------------------------

        // A FreeVar represents a free variable of the function to which it
        // belongs.
        //
        // FreeVars are used to implement anonymous functions, whose free
        // variables are lexically captured in a closure formed by
        // MakeClosure.  The value of such a free var is an Alloc or another
        // FreeVar and is considered a potentially escaping heap address, with
        // pointer type.
        //
        // FreeVars are also used to implement bound method closures.  Such a
        // free var represents the receiver value and may be of any type that
        // has concrete methods.
        //
        // Pos() returns the position of the value that was captured, which
        // belongs to an enclosing function.
        //
        public partial struct FreeVar
        {
            public @string name;
            public types.Type typ;
            public token.Pos pos;
            public ptr<Function> parent;
            public slice<Instruction> referrers; // Transiently needed during building.
            public Value outer; // the Value captured from the enclosing context.
        }

        // A Parameter represents an input parameter of a function.
        //
        public partial struct Parameter
        {
            public @string name;
            public types.Object @object; // a *types.Var; nil for non-source locals
            public types.Type typ;
            public token.Pos pos;
            public ptr<Function> parent;
            public slice<Instruction> referrers;
        }

        // A Const represents the value of a constant expression.
        //
        // The underlying type of a constant may be any boolean, numeric, or
        // string type.  In addition, a Const may represent the nil value of
        // any reference type---interface, map, channel, pointer, slice, or
        // function---but not "untyped nil".
        //
        // All source-level constant expressions are represented by a Const
        // of the same type and value.
        //
        // Value holds the value of the constant, independent of its Type(),
        // using go/constant representation, or nil for a typed nil value.
        //
        // Pos() returns token.NoPos.
        //
        // Example printed form:
        //     42:int
        //    "hello":untyped string
        //    3+4i:MyComplex
        //
        public partial struct Const
        {
            public types.Type typ;
            public constant.Value Value;
        }

        // A Global is a named Value holding the address of a package-level
        // variable.
        //
        // Pos() returns the position of the ast.ValueSpec.Names[*]
        // identifier.
        //
        public partial struct Global
        {
            public @string name;
            public types.Object @object; // a *types.Var; may be nil for synthetics e.g. init$guard
            public types.Type typ;
            public token.Pos pos;
            public ptr<Package> Pkg;
        }

        // A Builtin represents a specific use of a built-in function, e.g. len.
        //
        // Builtins are immutable values.  Builtins do not have addresses.
        // Builtins can only appear in CallCommon.Func.
        //
        // Name() indicates the function: one of the built-in functions from the
        // Go spec (excluding "make" and "new") or one of these ssa-defined
        // intrinsics:
        //
        //   // wrapnilchk returns ptr if non-nil, panics otherwise.
        //   // (For use in indirection wrappers.)
        //   func ssa:wrapnilchk(ptr *T, recvType, methodName string) *T
        //
        // Object() returns a *types.Builtin for built-ins defined by the spec,
        // nil for others.
        //
        // Type() returns a *types.Signature representing the effective
        // signature of the built-in for this call.
        //
        public partial struct Builtin
        {
            public @string name;
            public ptr<types.Signature> sig;
        }

        // Value-defining instructions  ----------------------------------------

        // The Alloc instruction reserves space for a variable of the given type,
        // zero-initializes it, and yields its address.
        //
        // Alloc values are always addresses, and have pointer types, so the
        // type of the allocated variable is actually
        // Type().Underlying().(*types.Pointer).Elem().
        //
        // If Heap is false, Alloc allocates space in the function's
        // activation record (frame); we refer to an Alloc(Heap=false) as a
        // "local" alloc.  Each local Alloc returns the same address each time
        // it is executed within the same activation; the space is
        // re-initialized to zero.
        //
        // If Heap is true, Alloc allocates space in the heap; we
        // refer to an Alloc(Heap=true) as a "new" alloc.  Each new Alloc
        // returns a different address each time it is executed.
        //
        // When Alloc is applied to a channel, map or slice type, it returns
        // the address of an uninitialized (nil) reference of that kind; store
        // the result of MakeSlice, MakeMap or MakeChan in that location to
        // instantiate these types.
        //
        // Pos() returns the ast.CompositeLit.Lbrace for a composite literal,
        // or the ast.CallExpr.Rparen for a call to new() or for a call that
        // allocates a varargs slice.
        //
        // Example printed form:
        //     t0 = local int
        //     t1 = new int
        //
        public partial struct Alloc
        {
            public ref register register => ref register_val;
            public @string Comment;
            public bool Heap;
            public long index; // dense numbering; for lifting
        }

        // The Phi instruction represents an SSA φ-node, which combines values
        // that differ across incoming control-flow edges and yields a new
        // value.  Within a block, all φ-nodes must appear before all non-φ
        // nodes.
        //
        // Pos() returns the position of the && or || for short-circuit
        // control-flow joins, or that of the *Alloc for φ-nodes inserted
        // during SSA renaming.
        //
        // Example printed form:
        //     t2 = phi [0: t0, 1: t1]
        //
        public partial struct Phi
        {
            public ref register register => ref register_val;
            public @string Comment; // a hint as to its purpose
            public slice<Value> Edges; // Edges[i] is value for Block().Preds[i]
        }

        // The Call instruction represents a function or method call.
        //
        // The Call instruction yields the function result if there is exactly
        // one.  Otherwise it returns a tuple, the components of which are
        // accessed via Extract.
        //
        // See CallCommon for generic function call documentation.
        //
        // Pos() returns the ast.CallExpr.Lparen, if explicit in the source.
        //
        // Example printed form:
        //     t2 = println(t0, t1)
        //     t4 = t3()
        //     t7 = invoke t5.Println(...t6)
        //
        public partial struct Call
        {
            public ref register register => ref register_val;
            public CallCommon Call;
        }

        // The BinOp instruction yields the result of binary operation X Op Y.
        //
        // Pos() returns the ast.BinaryExpr.OpPos, if explicit in the source.
        //
        // Example printed form:
        //     t1 = t0 + 1:int
        //
        public partial struct BinOp
        {
            public ref register register => ref register_val; // One of:
// ADD SUB MUL QUO REM          + - * / %
// AND OR XOR SHL SHR AND_NOT   & | ^ << >> &^
// EQL NEQ LSS LEQ GTR GEQ      == != < <= < >=
            public token.Token Op;
            public Value X;
            public Value Y;
        }

        // The UnOp instruction yields the result of Op X.
        // ARROW is channel receive.
        // MUL is pointer indirection (load).
        // XOR is bitwise complement.
        // SUB is negation.
        // NOT is logical negation.
        //
        // If CommaOk and Op=ARROW, the result is a 2-tuple of the value above
        // and a boolean indicating the success of the receive.  The
        // components of the tuple are accessed using Extract.
        //
        // Pos() returns the ast.UnaryExpr.OpPos, if explicit in the source.
        // For receive operations (ARROW) implicit in ranging over a channel,
        // Pos() returns the ast.RangeStmt.For.
        // For implicit memory loads (STAR), Pos() returns the position of the
        // most closely associated source-level construct; the details are not
        // specified.
        //
        // Example printed form:
        //     t0 = *x
        //     t2 = <-t1,ok
        //
        public partial struct UnOp
        {
            public ref register register => ref register_val;
            public token.Token Op; // One of: NOT SUB ARROW MUL XOR ! - <- * ^
            public Value X;
            public bool CommaOk;
        }

        // The ChangeType instruction applies to X a value-preserving type
        // change to Type().
        //
        // Type changes are permitted:
        //    - between a named type and its underlying type.
        //    - between two named types of the same underlying type.
        //    - between (possibly named) pointers to identical base types.
        //    - from a bidirectional channel to a read- or write-channel,
        //      optionally adding/removing a name.
        //
        // This operation cannot fail dynamically.
        //
        // Pos() returns the ast.CallExpr.Lparen, if the instruction arose
        // from an explicit conversion in the source.
        //
        // Example printed form:
        //     t1 = changetype *int <- IntPtr (t0)
        //
        public partial struct ChangeType
        {
            public ref register register => ref register_val;
            public Value X;
        }

        // The Convert instruction yields the conversion of value X to type
        // Type().  One or both of those types is basic (but possibly named).
        //
        // A conversion may change the value and representation of its operand.
        // Conversions are permitted:
        //    - between real numeric types.
        //    - between complex numeric types.
        //    - between string and []byte or []rune.
        //    - between pointers and unsafe.Pointer.
        //    - between unsafe.Pointer and uintptr.
        //    - from (Unicode) integer to (UTF-8) string.
        // A conversion may imply a type name change also.
        //
        // This operation cannot fail dynamically.
        //
        // Conversions of untyped string/number/bool constants to a specific
        // representation are eliminated during SSA construction.
        //
        // Pos() returns the ast.CallExpr.Lparen, if the instruction arose
        // from an explicit conversion in the source.
        //
        // Example printed form:
        //     t1 = convert []byte <- string (t0)
        //
        public partial struct Convert
        {
            public ref register register => ref register_val;
            public Value X;
        }

        // ChangeInterface constructs a value of one interface type from a
        // value of another interface type known to be assignable to it.
        // This operation cannot fail.
        //
        // Pos() returns the ast.CallExpr.Lparen if the instruction arose from
        // an explicit T(e) conversion; the ast.TypeAssertExpr.Lparen if the
        // instruction arose from an explicit e.(T) operation; or token.NoPos
        // otherwise.
        //
        // Example printed form:
        //     t1 = change interface interface{} <- I (t0)
        //
        public partial struct ChangeInterface
        {
            public ref register register => ref register_val;
            public Value X;
        }

        // MakeInterface constructs an instance of an interface type from a
        // value of a concrete type.
        //
        // Use Program.MethodSets.MethodSet(X.Type()) to find the method-set
        // of X, and Program.MethodValue(m) to find the implementation of a method.
        //
        // To construct the zero value of an interface type T, use:
        //     NewConst(constant.MakeNil(), T, pos)
        //
        // Pos() returns the ast.CallExpr.Lparen, if the instruction arose
        // from an explicit conversion in the source.
        //
        // Example printed form:
        //     t1 = make interface{} <- int (42:int)
        //     t2 = make Stringer <- t0
        //
        public partial struct MakeInterface
        {
            public ref register register => ref register_val;
            public Value X;
        }

        // The MakeClosure instruction yields a closure value whose code is
        // Fn and whose free variables' values are supplied by Bindings.
        //
        // Type() returns a (possibly named) *types.Signature.
        //
        // Pos() returns the ast.FuncLit.Type.Func for a function literal
        // closure or the ast.SelectorExpr.Sel for a bound method closure.
        //
        // Example printed form:
        //     t0 = make closure anon@1.2 [x y z]
        //     t1 = make closure bound$(main.I).add [i]
        //
        public partial struct MakeClosure
        {
            public ref register register => ref register_val;
            public Value Fn; // always a *Function
            public slice<Value> Bindings; // values for each free variable in Fn.FreeVars
        }

        // The MakeMap instruction creates a new hash-table-based map object
        // and yields a value of kind map.
        //
        // Type() returns a (possibly named) *types.Map.
        //
        // Pos() returns the ast.CallExpr.Lparen, if created by make(map), or
        // the ast.CompositeLit.Lbrack if created by a literal.
        //
        // Example printed form:
        //     t1 = make map[string]int t0
        //     t1 = make StringIntMap t0
        //
        public partial struct MakeMap
        {
            public ref register register => ref register_val;
            public Value Reserve; // initial space reservation; nil => default
        }

        // The MakeChan instruction creates a new channel object and yields a
        // value of kind chan.
        //
        // Type() returns a (possibly named) *types.Chan.
        //
        // Pos() returns the ast.CallExpr.Lparen for the make(chan) that
        // created it.
        //
        // Example printed form:
        //     t0 = make chan int 0
        //     t0 = make IntChan 0
        //
        public partial struct MakeChan
        {
            public ref register register => ref register_val;
            public Value Size; // int; size of buffer; zero => synchronous.
        }

        // The MakeSlice instruction yields a slice of length Len backed by a
        // newly allocated array of length Cap.
        //
        // Both Len and Cap must be non-nil Values of integer type.
        //
        // (Alloc(types.Array) followed by Slice will not suffice because
        // Alloc can only create arrays of constant length.)
        //
        // Type() returns a (possibly named) *types.Slice.
        //
        // Pos() returns the ast.CallExpr.Lparen for the make([]T) that
        // created it.
        //
        // Example printed form:
        //     t1 = make []string 1:int t0
        //     t1 = make StringSlice 1:int t0
        //
        public partial struct MakeSlice
        {
            public ref register register => ref register_val;
            public Value Len;
            public Value Cap;
        }

        // The Slice instruction yields a slice of an existing string, slice
        // or *array X between optional integer bounds Low and High.
        //
        // Dynamically, this instruction panics if X evaluates to a nil *array
        // pointer.
        //
        // Type() returns string if the type of X was string, otherwise a
        // *types.Slice with the same element type as X.
        //
        // Pos() returns the ast.SliceExpr.Lbrack if created by a x[:] slice
        // operation, the ast.CompositeLit.Lbrace if created by a literal, or
        // NoPos if not explicit in the source (e.g. a variadic argument slice).
        //
        // Example printed form:
        //     t1 = slice t0[1:]
        //
        public partial struct Slice
        {
            public ref register register => ref register_val;
            public Value X; // slice, string, or *array
            public Value Low; // each may be nil
            public Value High; // each may be nil
            public Value Max; // each may be nil
        }

        // The FieldAddr instruction yields the address of Field of *struct X.
        //
        // The field is identified by its index within the field list of the
        // struct type of X.
        //
        // Dynamically, this instruction panics if X evaluates to a nil
        // pointer.
        //
        // Type() returns a (possibly named) *types.Pointer.
        //
        // Pos() returns the position of the ast.SelectorExpr.Sel for the
        // field, if explicit in the source.
        //
        // Example printed form:
        //     t1 = &t0.name [#1]
        //
        public partial struct FieldAddr
        {
            public ref register register => ref register_val;
            public Value X; // *struct
            public long Field; // field is X.Type().Underlying().(*types.Pointer).Elem().Underlying().(*types.Struct).Field(Field)
        }

        // The Field instruction yields the Field of struct X.
        //
        // The field is identified by its index within the field list of the
        // struct type of X; by using numeric indices we avoid ambiguity of
        // package-local identifiers and permit compact representations.
        //
        // Pos() returns the position of the ast.SelectorExpr.Sel for the
        // field, if explicit in the source.
        //
        // Example printed form:
        //     t1 = t0.name [#1]
        //
        public partial struct Field
        {
            public ref register register => ref register_val;
            public Value X; // struct
            public long Field; // index into X.Type().(*types.Struct).Fields
        }

        // The IndexAddr instruction yields the address of the element at
        // index Index of collection X.  Index is an integer expression.
        //
        // The elements of maps and strings are not addressable; use Lookup or
        // MapUpdate instead.
        //
        // Dynamically, this instruction panics if X evaluates to a nil *array
        // pointer.
        //
        // Type() returns a (possibly named) *types.Pointer.
        //
        // Pos() returns the ast.IndexExpr.Lbrack for the index operation, if
        // explicit in the source.
        //
        // Example printed form:
        //     t2 = &t0[t1]
        //
        public partial struct IndexAddr
        {
            public ref register register => ref register_val;
            public Value X; // slice or *array,
            public Value Index; // numeric index
        }

        // The Index instruction yields element Index of array X.
        //
        // Pos() returns the ast.IndexExpr.Lbrack for the index operation, if
        // explicit in the source.
        //
        // Example printed form:
        //     t2 = t0[t1]
        //
        public partial struct Index
        {
            public ref register register => ref register_val;
            public Value X; // array
            public Value Index; // integer index
        }

        // The Lookup instruction yields element Index of collection X, a map
        // or string.  Index is an integer expression if X is a string or the
        // appropriate key type if X is a map.
        //
        // If CommaOk, the result is a 2-tuple of the value above and a
        // boolean indicating the result of a map membership test for the key.
        // The components of the tuple are accessed using Extract.
        //
        // Pos() returns the ast.IndexExpr.Lbrack, if explicit in the source.
        //
        // Example printed form:
        //     t2 = t0[t1]
        //     t5 = t3[t4],ok
        //
        public partial struct Lookup
        {
            public ref register register => ref register_val;
            public Value X; // string or map
            public Value Index; // numeric or key-typed index
            public bool CommaOk; // return a value,ok pair
        }

        // SelectState is a helper for Select.
        // It represents one goal state and its corresponding communication.
        //
        public partial struct SelectState
        {
            public types.ChanDir Dir; // direction of case (SendOnly or RecvOnly)
            public Value Chan; // channel to use (for send or receive)
            public Value Send; // value to send (for send)
            public token.Pos Pos; // position of token.ARROW
            public ast.Node DebugNode; // ast.SendStmt or ast.UnaryExpr(<-) [debug mode]
        }

        // The Select instruction tests whether (or blocks until) one
        // of the specified sent or received states is entered.
        //
        // Let n be the number of States for which Dir==RECV and T_i (0<=i<n)
        // be the element type of each such state's Chan.
        // Select returns an n+2-tuple
        //    (index int, recvOk bool, r_0 T_0, ... r_n-1 T_n-1)
        // The tuple's components, described below, must be accessed via the
        // Extract instruction.
        //
        // If Blocking, select waits until exactly one state holds, i.e. a
        // channel becomes ready for the designated operation of sending or
        // receiving; select chooses one among the ready states
        // pseudorandomly, performs the send or receive operation, and sets
        // 'index' to the index of the chosen channel.
        //
        // If !Blocking, select doesn't block if no states hold; instead it
        // returns immediately with index equal to -1.
        //
        // If the chosen channel was used for a receive, the r_i component is
        // set to the received value, where i is the index of that state among
        // all n receive states; otherwise r_i has the zero value of type T_i.
        // Note that the receive index i is not the same as the state
        // index index.
        //
        // The second component of the triple, recvOk, is a boolean whose value
        // is true iff the selected operation was a receive and the receive
        // successfully yielded a value.
        //
        // Pos() returns the ast.SelectStmt.Select.
        //
        // Example printed form:
        //     t3 = select nonblocking [<-t0, t1<-t2]
        //     t4 = select blocking []
        //
        public partial struct Select
        {
            public ref register register => ref register_val;
            public slice<ptr<SelectState>> States;
            public bool Blocking;
        }

        // The Range instruction yields an iterator over the domain and range
        // of X, which must be a string or map.
        //
        // Elements are accessed via Next.
        //
        // Type() returns an opaque and degenerate "rangeIter" type.
        //
        // Pos() returns the ast.RangeStmt.For.
        //
        // Example printed form:
        //     t0 = range "hello":string
        //
        public partial struct Range
        {
            public ref register register => ref register_val;
            public Value X; // string or map
        }

        // The Next instruction reads and advances the (map or string)
        // iterator Iter and returns a 3-tuple value (ok, k, v).  If the
        // iterator is not exhausted, ok is true and k and v are the next
        // elements of the domain and range, respectively.  Otherwise ok is
        // false and k and v are undefined.
        //
        // Components of the tuple are accessed using Extract.
        //
        // The IsString field distinguishes iterators over strings from those
        // over maps, as the Type() alone is insufficient: consider
        // map[int]rune.
        //
        // Type() returns a *types.Tuple for the triple (ok, k, v).
        // The types of k and/or v may be types.Invalid.
        //
        // Example printed form:
        //     t1 = next t0
        //
        public partial struct Next
        {
            public ref register register => ref register_val;
            public Value Iter;
            public bool IsString; // true => string iterator; false => map iterator.
        }

        // The TypeAssert instruction tests whether interface value X has type
        // AssertedType.
        //
        // If !CommaOk, on success it returns v, the result of the conversion
        // (defined below); on failure it panics.
        //
        // If CommaOk: on success it returns a pair (v, true) where v is the
        // result of the conversion; on failure it returns (z, false) where z
        // is AssertedType's zero value.  The components of the pair must be
        // accessed using the Extract instruction.
        //
        // If AssertedType is a concrete type, TypeAssert checks whether the
        // dynamic type in interface X is equal to it, and if so, the result
        // of the conversion is a copy of the value in the interface.
        //
        // If AssertedType is an interface, TypeAssert checks whether the
        // dynamic type of the interface is assignable to it, and if so, the
        // result of the conversion is a copy of the interface value X.
        // If AssertedType is a superinterface of X.Type(), the operation will
        // fail iff the operand is nil.  (Contrast with ChangeInterface, which
        // performs no nil-check.)
        //
        // Type() reflects the actual type of the result, possibly a
        // 2-types.Tuple; AssertedType is the asserted type.
        //
        // Pos() returns the ast.CallExpr.Lparen if the instruction arose from
        // an explicit T(e) conversion; the ast.TypeAssertExpr.Lparen if the
        // instruction arose from an explicit e.(T) operation; or the
        // ast.CaseClause.Case if the instruction arose from a case of a
        // type-switch statement.
        //
        // Example printed form:
        //     t1 = typeassert t0.(int)
        //     t3 = typeassert,ok t2.(T)
        //
        public partial struct TypeAssert
        {
            public ref register register => ref register_val;
            public Value X;
            public types.Type AssertedType;
            public bool CommaOk;
        }

        // The Extract instruction yields component Index of Tuple.
        //
        // This is used to access the results of instructions with multiple
        // return values, such as Call, TypeAssert, Next, UnOp(ARROW) and
        // IndexExpr(Map).
        //
        // Example printed form:
        //     t1 = extract t0 #1
        //
        public partial struct Extract
        {
            public ref register register => ref register_val;
            public Value Tuple;
            public long Index;
        }

        // Instructions executed for effect.  They do not yield a value. --------------------

        // The Jump instruction transfers control to the sole successor of its
        // owning block.
        //
        // A Jump must be the last instruction of its containing BasicBlock.
        //
        // Pos() returns NoPos.
        //
        // Example printed form:
        //     jump done
        //
        public partial struct Jump
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
        }

        // The If instruction transfers control to one of the two successors
        // of its owning block, depending on the boolean Cond: the first if
        // true, the second if false.
        //
        // An If instruction must be the last instruction of its containing
        // BasicBlock.
        //
        // Pos() returns NoPos.
        //
        // Example printed form:
        //     if t0 goto done else body
        //
        public partial struct If
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public Value Cond;
        }

        // The Return instruction returns values and control back to the calling
        // function.
        //
        // len(Results) is always equal to the number of results in the
        // function's signature.
        //
        // If len(Results) > 1, Return returns a tuple value with the specified
        // components which the caller must access using Extract instructions.
        //
        // There is no instruction to return a ready-made tuple like those
        // returned by a "value,ok"-mode TypeAssert, Lookup or UnOp(ARROW) or
        // a tail-call to a function with multiple result parameters.
        //
        // Return must be the last instruction of its containing BasicBlock.
        // Such a block has no successors.
        //
        // Pos() returns the ast.ReturnStmt.Return, if explicit in the source.
        //
        // Example printed form:
        //     return
        //     return nil:I, 2:int
        //
        public partial struct Return
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public slice<Value> Results;
            public token.Pos pos;
        }

        // The RunDefers instruction pops and invokes the entire stack of
        // procedure calls pushed by Defer instructions in this function.
        //
        // It is legal to encounter multiple 'rundefers' instructions in a
        // single control-flow path through a function; this is useful in
        // the combined init() function, for example.
        //
        // Pos() returns NoPos.
        //
        // Example printed form:
        //    rundefers
        //
        public partial struct RunDefers
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
        }

        // The Panic instruction initiates a panic with value X.
        //
        // A Panic instruction must be the last instruction of its containing
        // BasicBlock, which must have no successors.
        //
        // NB: 'go panic(x)' and 'defer panic(x)' do not use this instruction;
        // they are treated as calls to a built-in function.
        //
        // Pos() returns the ast.CallExpr.Lparen if this panic was explicit
        // in the source.
        //
        // Example printed form:
        //     panic t0
        //
        public partial struct Panic
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public Value X; // an interface{}
            public token.Pos pos;
        }

        // The Go instruction creates a new goroutine and calls the specified
        // function within it.
        //
        // See CallCommon for generic function call documentation.
        //
        // Pos() returns the ast.GoStmt.Go.
        //
        // Example printed form:
        //     go println(t0, t1)
        //     go t3()
        //     go invoke t5.Println(...t6)
        //
        public partial struct Go
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public CallCommon Call;
            public token.Pos pos;
        }

        // The Defer instruction pushes the specified call onto a stack of
        // functions to be called by a RunDefers instruction or by a panic.
        //
        // See CallCommon for generic function call documentation.
        //
        // Pos() returns the ast.DeferStmt.Defer.
        //
        // Example printed form:
        //     defer println(t0, t1)
        //     defer t3()
        //     defer invoke t5.Println(...t6)
        //
        public partial struct Defer
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public CallCommon Call;
            public token.Pos pos;
        }

        // The Send instruction sends X on channel Chan.
        //
        // Pos() returns the ast.SendStmt.Arrow, if explicit in the source.
        //
        // Example printed form:
        //     send t0 <- t1
        //
        public partial struct Send
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public Value Chan;
            public Value X;
            public token.Pos pos;
        }

        // The Store instruction stores Val at address Addr.
        // Stores can be of arbitrary types.
        //
        // Pos() returns the position of the source-level construct most closely
        // associated with the memory store operation.
        // Since implicit memory stores are numerous and varied and depend upon
        // implementation choices, the details are not specified.
        //
        // Example printed form:
        //     *x = y
        //
        public partial struct Store
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public Value Addr;
            public Value Val;
            public token.Pos pos;
        }

        // The MapUpdate instruction updates the association of Map[Key] to
        // Value.
        //
        // Pos() returns the ast.KeyValueExpr.Colon or ast.IndexExpr.Lbrack,
        // if explicit in the source.
        //
        // Example printed form:
        //    t0[t1] = t2
        //
        public partial struct MapUpdate
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public Value Map;
            public Value Key;
            public Value Value;
            public token.Pos pos;
        }

        // A DebugRef instruction maps a source-level expression Expr to the
        // SSA value X that represents the value (!IsAddr) or address (IsAddr)
        // of that expression.
        //
        // DebugRef is a pseudo-instruction: it has no dynamic effect.
        //
        // Pos() returns Expr.Pos(), the start position of the source-level
        // expression.  This is not the same as the "designated" token as
        // documented at Value.Pos(). e.g. CallExpr.Pos() does not return the
        // position of the ("designated") Lparen token.
        //
        // If Expr is an *ast.Ident denoting a var or func, Object() returns
        // the object; though this information can be obtained from the type
        // checker, including it here greatly facilitates debugging.
        // For non-Ident expressions, Object() returns nil.
        //
        // DebugRefs are generated only for functions built with debugging
        // enabled; see Package.SetDebugMode() and the GlobalDebug builder
        // mode flag.
        //
        // DebugRefs are not emitted for ast.Idents referring to constants or
        // predeclared identifiers, since they are trivial and numerous.
        // Nor are they emitted for ast.ParenExprs.
        //
        // (By representing these as instructions, rather than out-of-band,
        // consistency is maintained during transformation passes by the
        // ordinary SSA renaming machinery.)
        //
        // Example printed form:
        //      ; *ast.CallExpr @ 102:9 is t5
        //      ; var x float64 @ 109:72 is x
        //      ; address of *ast.CompositeLit @ 216:10 is t0
        //
        public partial struct DebugRef
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public ast.Expr Expr; // the referring expression (never *ast.ParenExpr)
            public types.Object @object; // the identity of the source var/func
            public bool IsAddr; // Expr is addressable and X is the address it denotes
            public Value X; // the value or address of Expr
        }

        // Embeddable mix-ins and helpers for common parts of other structs. -----------

        // register is a mix-in embedded by all SSA values that are also
        // instructions, i.e. virtual registers, and provides a uniform
        // implementation of most of the Value interface: Value.Name() is a
        // numbered register (e.g. "t0"); the other methods are field accessors.
        //
        // Temporary names are automatically assigned to each register on
        // completion of building a function in SSA form.
        //
        // Clients must not assume that the 'id' value (and the Name() derived
        // from it) is unique within a function.  As always in this API,
        // semantics are determined only by identity; names exist only to
        // facilitate debugging.
        //
        private partial struct register
        {
            public ref anInstruction anInstruction => ref anInstruction_val;
            public long num; // "name" of virtual register, e.g. "t0".  Not guaranteed unique.
            public types.Type typ; // type of virtual register
            public token.Pos pos; // position of source expression, or NoPos
            public slice<Instruction> referrers;
        }

        // anInstruction is a mix-in embedded by all Instructions.
        // It provides the implementations of the Block and setBlock methods.
        private partial struct anInstruction
        {
            public ptr<BasicBlock> block; // the basic block of this instruction
        }

        // CallCommon is contained by Go, Defer and Call to hold the
        // common parts of a function or method call.
        //
        // Each CallCommon exists in one of two modes, function call and
        // interface method invocation, or "call" and "invoke" for short.
        //
        // 1. "call" mode: when Method is nil (!IsInvoke), a CallCommon
        // represents an ordinary function call of the value in Value,
        // which may be a *Builtin, a *Function or any other value of kind
        // 'func'.
        //
        // Value may be one of:
        //    (a) a *Function, indicating a statically dispatched call
        //        to a package-level function, an anonymous function, or
        //        a method of a named type.
        //    (b) a *MakeClosure, indicating an immediately applied
        //        function literal with free variables.
        //    (c) a *Builtin, indicating a statically dispatched call
        //        to a built-in function.
        //    (d) any other value, indicating a dynamically dispatched
        //        function call.
        // StaticCallee returns the identity of the callee in cases
        // (a) and (b), nil otherwise.
        //
        // Args contains the arguments to the call.  If Value is a method,
        // Args[0] contains the receiver parameter.
        //
        // Example printed form:
        //     t2 = println(t0, t1)
        //     go t3()
        //    defer t5(...t6)
        //
        // 2. "invoke" mode: when Method is non-nil (IsInvoke), a CallCommon
        // represents a dynamically dispatched call to an interface method.
        // In this mode, Value is the interface value and Method is the
        // interface's abstract method.  Note: an abstract method may be
        // shared by multiple interfaces due to embedding; Value.Type()
        // provides the specific interface used for this call.
        //
        // Value is implicitly supplied to the concrete method implementation
        // as the receiver parameter; in other words, Args[0] holds not the
        // receiver but the first true argument.
        //
        // Example printed form:
        //     t1 = invoke t0.String()
        //     go invoke t3.Run(t2)
        //     defer invoke t4.Handle(...t5)
        //
        // For all calls to variadic functions (Signature().Variadic()),
        // the last element of Args is a slice.
        //
        public partial struct CallCommon
        {
            public Value Value; // receiver (invoke mode) or func value (call mode)
            public ptr<types.Func> Method; // abstract method (invoke mode)
            public slice<Value> Args; // actual parameters (in static method call, includes receiver)
            public token.Pos pos; // position of CallExpr.Lparen, iff explicit in source
        }

        // IsInvoke returns true if this call has "invoke" (not "call") mode.
        private static bool IsInvoke(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            return c.Method != null;
        }

        private static token.Pos Pos(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            return c.pos;
        }

        // Signature returns the signature of the called function.
        //
        // For an "invoke"-mode call, the signature of the interface method is
        // returned.
        //
        // In either "call" or "invoke" mode, if the callee is a method, its
        // receiver is represented by sig.Recv, not sig.Params().At(0).
        //
        private static ptr<types.Signature> Signature(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            if (c.Method != null)
            {
                return c.Method.Type()._<ptr<types.Signature>>();
            }

            return c.Value.Type().Underlying()._<ptr<types.Signature>>();

        }

        // StaticCallee returns the callee if this is a trivially static
        // "call"-mode call to a function.
        private static ptr<Function> StaticCallee(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            switch (c.Value.type())
            {
                case ptr<Function> fn:
                    return _addr_fn!;
                    break;
                case ptr<MakeClosure> fn:
                    return fn.Fn._<ptr<Function>>();
                    break;
            }
            return _addr_null!;

        }

        // Description returns a description of the mode of this call suitable
        // for a user interface, e.g., "static method call".
        private static @string Description(this ptr<CallCommon> _addr_c)
        {
            ref CallCommon c = ref _addr_c.val;

            switch (c.Value.type())
            {
                case ptr<Builtin> fn:
                    return "built-in function call";
                    break;
                case ptr<MakeClosure> fn:
                    return "static function closure call";
                    break;
                case ptr<Function> fn:
                    if (fn.Signature.Recv() != null)
                    {
                        return "static method call";
                    }

                    return "static function call";
                    break;
            }
            if (c.IsInvoke())
            {
                return "dynamic method call"; // ("invoke" mode)
            }

            return "dynamic function call";

        }

        // The CallInstruction interface, implemented by *Go, *Defer and *Call,
        // exposes the common parts of function-calling instructions,
        // yet provides a way back to the Value defined by *Call alone.
        //
        public partial interface CallInstruction : Instruction
        {
            ptr<Call> Common(); // returns the common parts of the call
            ptr<Call> Value(); // returns the result value of the call (*Call) or nil (*Go, *Defer)
        }

        private static ptr<CallCommon> Common(this ptr<Call> _addr_s)
        {
            ref Call s = ref _addr_s.val;

            return _addr__addr_s.Call!;
        }
        private static ptr<CallCommon> Common(this ptr<Defer> _addr_s)
        {
            ref Defer s = ref _addr_s.val;

            return _addr__addr_s.Call!;
        }
        private static ptr<CallCommon> Common(this ptr<Go> _addr_s)
        {
            ref Go s = ref _addr_s.val;

            return _addr__addr_s.Call!;
        }

        private static ptr<Call> Value(this ptr<Call> _addr_s)
        {
            ref Call s = ref _addr_s.val;

            return _addr_s!;
        }
        private static ptr<Call> Value(this ptr<Defer> _addr_s)
        {
            ref Defer s = ref _addr_s.val;

            return _addr_null!;
        }
        private static ptr<Call> Value(this ptr<Go> _addr_s)
        {
            ref Go s = ref _addr_s.val;

            return _addr_null!;
        }

        private static types.Type Type(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return v.sig;
        }
        private static @string Name(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return v.name;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<Builtin> _addr__p0)
        {
            ref Builtin _p0 = ref _addr__p0.val;

            return _addr_null!;
        }
        private static token.Pos Pos(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return token.NoPos;
        }
        private static types.Object Object(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return types.Universe.Lookup(v.name);
        }
        private static ptr<Function> Parent(this ptr<Builtin> _addr_v)
        {
            ref Builtin v = ref _addr_v.val;

            return _addr_null!;
        }

        private static types.Type Type(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            return v.typ;
        }
        private static @string Name(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            return v.name;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            return _addr__addr_v.referrers!;
        }
        private static token.Pos Pos(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            return v.pos;
        }
        private static ptr<Function> Parent(this ptr<FreeVar> _addr_v)
        {
            ref FreeVar v = ref _addr_v.val;

            return _addr_v.parent!;
        }

        private static types.Type Type(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return v.typ;
        }
        private static @string Name(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return v.name;
        }
        private static ptr<Function> Parent(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return _addr_null!;
        }
        private static token.Pos Pos(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return v.pos;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return _addr_null!;
        }
        private static token.Token Token(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return token.VAR;
        }
        private static types.Object Object(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return v.@object;
        }
        private static @string String(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return v.RelString(null);
        }
        private static ptr<Package> Package(this ptr<Global> _addr_v)
        {
            ref Global v = ref _addr_v.val;

            return _addr_v.Pkg!;
        }
        private static @string RelString(this ptr<Global> _addr_v, ptr<types.Package> _addr_from)
        {
            ref Global v = ref _addr_v.val;
            ref types.Package from = ref _addr_from.val;

            return relString(v, from);
        }

        private static @string Name(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return v.name;
        }
        private static types.Type Type(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return v.Signature;
        }
        private static token.Pos Pos(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return v.pos;
        }
        private static token.Token Token(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return token.FUNC;
        }
        private static types.Object Object(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return v.@object;
        }
        private static @string String(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return v.RelString(null);
        }
        private static ptr<Package> Package(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return _addr_v.Pkg!;
        }
        private static ptr<Function> Parent(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            return _addr_v.parent!;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<Function> _addr_v)
        {
            ref Function v = ref _addr_v.val;

            if (v.parent != null)
            {
                return _addr__addr_v.referrers!;
            }

            return _addr_null!;

        }

        private static types.Type Type(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return v.typ;
        }
        private static @string Name(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return v.name;
        }
        private static types.Object Object(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return v.@object;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return _addr__addr_v.referrers!;
        }
        private static token.Pos Pos(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return v.pos;
        }
        private static ptr<Function> Parent(this ptr<Parameter> _addr_v)
        {
            ref Parameter v = ref _addr_v.val;

            return _addr_v.parent!;
        }

        private static types.Type Type(this ptr<Alloc> _addr_v)
        {
            ref Alloc v = ref _addr_v.val;

            return v.typ;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<Alloc> _addr_v)
        {
            ref Alloc v = ref _addr_v.val;

            return _addr__addr_v.referrers!;
        }
        private static token.Pos Pos(this ptr<Alloc> _addr_v)
        {
            ref Alloc v = ref _addr_v.val;

            return v.pos;
        }

        private static types.Type Type(this ptr<register> _addr_v)
        {
            ref register v = ref _addr_v.val;

            return v.typ;
        }
        private static void setType(this ptr<register> _addr_v, types.Type typ)
        {
            ref register v = ref _addr_v.val;

            v.typ = typ;
        }
        private static @string Name(this ptr<register> _addr_v)
        {
            ref register v = ref _addr_v.val;

            return fmt.Sprintf("t%d", v.num);
        }
        private static void setNum(this ptr<register> _addr_v, long num)
        {
            ref register v = ref _addr_v.val;

            v.num = num;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<register> _addr_v)
        {
            ref register v = ref _addr_v.val;

            return _addr__addr_v.referrers!;
        }
        private static token.Pos Pos(this ptr<register> _addr_v)
        {
            ref register v = ref _addr_v.val;

            return v.pos;
        }
        private static void setPos(this ptr<register> _addr_v, token.Pos pos)
        {
            ref register v = ref _addr_v.val;

            v.pos = pos;
        }

        private static ptr<Function> Parent(this ptr<anInstruction> _addr_v)
        {
            ref anInstruction v = ref _addr_v.val;

            return _addr_v.block.parent!;
        }
        private static ptr<BasicBlock> Block(this ptr<anInstruction> _addr_v)
        {
            ref anInstruction v = ref _addr_v.val;

            return _addr_v.block!;
        }
        private static void setBlock(this ptr<anInstruction> _addr_v, ptr<BasicBlock> _addr_block)
        {
            ref anInstruction v = ref _addr_v.val;
            ref BasicBlock block = ref _addr_block.val;

            v.block = block;
        }
        private static ptr<slice<Instruction>> Referrers(this ptr<anInstruction> _addr_v)
        {
            ref anInstruction v = ref _addr_v.val;

            return _addr_null!;
        }

        private static @string Name(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.@object.Name();
        }
        private static token.Pos Pos(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.@object.Pos();
        }
        private static types.Type Type(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.@object.Type();
        }
        private static token.Token Token(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return token.TYPE;
        }
        private static types.Object Object(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.@object;
        }
        private static @string String(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.RelString(null);
        }
        private static ptr<Package> Package(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_t.pkg!;
        }
        private static @string RelString(this ptr<Type> _addr_t, ptr<types.Package> _addr_from)
        {
            ref Type t = ref _addr_t.val;
            ref types.Package from = ref _addr_from.val;

            return relString(t, from);
        }

        private static @string Name(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return c.@object.Name();
        }
        private static token.Pos Pos(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return c.@object.Pos();
        }
        private static @string String(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return c.RelString(null);
        }
        private static types.Type Type(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return c.@object.Type();
        }
        private static token.Token Token(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return token.CONST;
        }
        private static types.Object Object(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return c.@object;
        }
        private static ptr<Package> Package(this ptr<NamedConst> _addr_c)
        {
            ref NamedConst c = ref _addr_c.val;

            return _addr_c.pkg!;
        }
        private static @string RelString(this ptr<NamedConst> _addr_c, ptr<types.Package> _addr_from)
        {
            ref NamedConst c = ref _addr_c.val;
            ref types.Package from = ref _addr_from.val;

            return relString(c, from);
        }

        private static types.Object Object(this ptr<DebugRef> _addr_d)
        {
            ref DebugRef d = ref _addr_d.val;

            return d.@object;
        }

        // Func returns the package-level function of the specified name,
        // or nil if not found.
        //
        private static ptr<Function> Func(this ptr<Package> _addr_p, @string name)
        {
            ptr<Function> f = default!;
            ref Package p = ref _addr_p.val;

            f, _ = p.Members[name]._<ptr<Function>>();
            return ;
        }

        // Var returns the package-level variable of the specified name,
        // or nil if not found.
        //
        private static ptr<Global> Var(this ptr<Package> _addr_p, @string name)
        {
            ptr<Global> g = default!;
            ref Package p = ref _addr_p.val;

            g, _ = p.Members[name]._<ptr<Global>>();
            return ;
        }

        // Const returns the package-level constant of the specified name,
        // or nil if not found.
        //
        private static ptr<NamedConst> Const(this ptr<Package> _addr_p, @string name)
        {
            ptr<NamedConst> c = default!;
            ref Package p = ref _addr_p.val;

            c, _ = p.Members[name]._<ptr<NamedConst>>();
            return ;
        }

        // Type returns the package-level type of the specified name,
        // or nil if not found.
        //
        private static ptr<Type> Type(this ptr<Package> _addr_p, @string name)
        {
            ptr<Type> t = default!;
            ref Package p = ref _addr_p.val;

            t, _ = p.Members[name]._<ptr<Type>>();
            return ;
        }

        private static token.Pos Pos(this ptr<Call> _addr_v)
        {
            ref Call v = ref _addr_v.val;

            return v.Call.pos;
        }
        private static token.Pos Pos(this ptr<Defer> _addr_s)
        {
            ref Defer s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<Go> _addr_s)
        {
            ref Go s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<MapUpdate> _addr_s)
        {
            ref MapUpdate s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<Panic> _addr_s)
        {
            ref Panic s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<Return> _addr_s)
        {
            ref Return s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<Send> _addr_s)
        {
            ref Send s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<Store> _addr_s)
        {
            ref Store s = ref _addr_s.val;

            return s.pos;
        }
        private static token.Pos Pos(this ptr<If> _addr_s)
        {
            ref If s = ref _addr_s.val;

            return token.NoPos;
        }
        private static token.Pos Pos(this ptr<Jump> _addr_s)
        {
            ref Jump s = ref _addr_s.val;

            return token.NoPos;
        }
        private static token.Pos Pos(this ptr<RunDefers> _addr_s)
        {
            ref RunDefers s = ref _addr_s.val;

            return token.NoPos;
        }
        private static token.Pos Pos(this ptr<DebugRef> _addr_s)
        {
            ref DebugRef s = ref _addr_s.val;

            return s.Expr.Pos();
        }

        // Operands.

        private static slice<ptr<Value>> Operands(this ptr<Alloc> _addr_v, slice<ptr<Value>> rands)
        {
            ref Alloc v = ref _addr_v.val;

            return rands;
        }

        private static slice<ptr<Value>> Operands(this ptr<BinOp> _addr_v, slice<ptr<Value>> rands)
        {
            ref BinOp v = ref _addr_v.val;

            return append(rands, _addr_v.X, _addr_v.Y);
        }

        private static slice<ptr<Value>> Operands(this ptr<CallCommon> _addr_c, slice<ptr<Value>> rands)
        {
            ref CallCommon c = ref _addr_c.val;

            rands = append(rands, _addr_c.Value);
            foreach (var (i) in c.Args)
            {
                rands = append(rands, _addr_c.Args[i]);
            }
            return rands;

        }

        private static slice<ptr<Value>> Operands(this ptr<Go> _addr_s, slice<ptr<Value>> rands)
        {
            ref Go s = ref _addr_s.val;

            return s.Call.Operands(rands);
        }

        private static slice<ptr<Value>> Operands(this ptr<Call> _addr_s, slice<ptr<Value>> rands)
        {
            ref Call s = ref _addr_s.val;

            return s.Call.Operands(rands);
        }

        private static slice<ptr<Value>> Operands(this ptr<Defer> _addr_s, slice<ptr<Value>> rands)
        {
            ref Defer s = ref _addr_s.val;

            return s.Call.Operands(rands);
        }

        private static slice<ptr<Value>> Operands(this ptr<ChangeInterface> _addr_v, slice<ptr<Value>> rands)
        {
            ref ChangeInterface v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<ChangeType> _addr_v, slice<ptr<Value>> rands)
        {
            ref ChangeType v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<Convert> _addr_v, slice<ptr<Value>> rands)
        {
            ref Convert v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<DebugRef> _addr_s, slice<ptr<Value>> rands)
        {
            ref DebugRef s = ref _addr_s.val;

            return append(rands, _addr_s.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<Extract> _addr_v, slice<ptr<Value>> rands)
        {
            ref Extract v = ref _addr_v.val;

            return append(rands, _addr_v.Tuple);
        }

        private static slice<ptr<Value>> Operands(this ptr<Field> _addr_v, slice<ptr<Value>> rands)
        {
            ref Field v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<FieldAddr> _addr_v, slice<ptr<Value>> rands)
        {
            ref FieldAddr v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<If> _addr_s, slice<ptr<Value>> rands)
        {
            ref If s = ref _addr_s.val;

            return append(rands, _addr_s.Cond);
        }

        private static slice<ptr<Value>> Operands(this ptr<Index> _addr_v, slice<ptr<Value>> rands)
        {
            ref Index v = ref _addr_v.val;

            return append(rands, _addr_v.X, _addr_v.Index);
        }

        private static slice<ptr<Value>> Operands(this ptr<IndexAddr> _addr_v, slice<ptr<Value>> rands)
        {
            ref IndexAddr v = ref _addr_v.val;

            return append(rands, _addr_v.X, _addr_v.Index);
        }

        private static slice<ptr<Value>> Operands(this ptr<Jump> _addr__p0, slice<ptr<Value>> rands)
        {
            ref Jump _p0 = ref _addr__p0.val;

            return rands;
        }

        private static slice<ptr<Value>> Operands(this ptr<Lookup> _addr_v, slice<ptr<Value>> rands)
        {
            ref Lookup v = ref _addr_v.val;

            return append(rands, _addr_v.X, _addr_v.Index);
        }

        private static slice<ptr<Value>> Operands(this ptr<MakeChan> _addr_v, slice<ptr<Value>> rands)
        {
            ref MakeChan v = ref _addr_v.val;

            return append(rands, _addr_v.Size);
        }

        private static slice<ptr<Value>> Operands(this ptr<MakeClosure> _addr_v, slice<ptr<Value>> rands)
        {
            ref MakeClosure v = ref _addr_v.val;

            rands = append(rands, _addr_v.Fn);
            foreach (var (i) in v.Bindings)
            {
                rands = append(rands, _addr_v.Bindings[i]);
            }
            return rands;

        }

        private static slice<ptr<Value>> Operands(this ptr<MakeInterface> _addr_v, slice<ptr<Value>> rands)
        {
            ref MakeInterface v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<MakeMap> _addr_v, slice<ptr<Value>> rands)
        {
            ref MakeMap v = ref _addr_v.val;

            return append(rands, _addr_v.Reserve);
        }

        private static slice<ptr<Value>> Operands(this ptr<MakeSlice> _addr_v, slice<ptr<Value>> rands)
        {
            ref MakeSlice v = ref _addr_v.val;

            return append(rands, _addr_v.Len, _addr_v.Cap);
        }

        private static slice<ptr<Value>> Operands(this ptr<MapUpdate> _addr_v, slice<ptr<Value>> rands)
        {
            ref MapUpdate v = ref _addr_v.val;

            return append(rands, _addr_v.Map, _addr_v.Key, _addr_v.Value);
        }

        private static slice<ptr<Value>> Operands(this ptr<Next> _addr_v, slice<ptr<Value>> rands)
        {
            ref Next v = ref _addr_v.val;

            return append(rands, _addr_v.Iter);
        }

        private static slice<ptr<Value>> Operands(this ptr<Panic> _addr_s, slice<ptr<Value>> rands)
        {
            ref Panic s = ref _addr_s.val;

            return append(rands, _addr_s.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<Phi> _addr_v, slice<ptr<Value>> rands)
        {
            ref Phi v = ref _addr_v.val;

            foreach (var (i) in v.Edges)
            {
                rands = append(rands, _addr_v.Edges[i]);
            }
            return rands;

        }

        private static slice<ptr<Value>> Operands(this ptr<Range> _addr_v, slice<ptr<Value>> rands)
        {
            ref Range v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<Return> _addr_s, slice<ptr<Value>> rands)
        {
            ref Return s = ref _addr_s.val;

            foreach (var (i) in s.Results)
            {
                rands = append(rands, _addr_s.Results[i]);
            }
            return rands;

        }

        private static slice<ptr<Value>> Operands(this ptr<RunDefers> _addr__p0, slice<ptr<Value>> rands)
        {
            ref RunDefers _p0 = ref _addr__p0.val;

            return rands;
        }

        private static slice<ptr<Value>> Operands(this ptr<Select> _addr_v, slice<ptr<Value>> rands)
        {
            ref Select v = ref _addr_v.val;

            foreach (var (i) in v.States)
            {
                rands = append(rands, _addr_v.States[i].Chan, _addr_v.States[i].Send);
            }
            return rands;

        }

        private static slice<ptr<Value>> Operands(this ptr<Send> _addr_s, slice<ptr<Value>> rands)
        {
            ref Send s = ref _addr_s.val;

            return append(rands, _addr_s.Chan, _addr_s.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<Slice> _addr_v, slice<ptr<Value>> rands)
        {
            ref Slice v = ref _addr_v.val;

            return append(rands, _addr_v.X, _addr_v.Low, _addr_v.High, _addr_v.Max);
        }

        private static slice<ptr<Value>> Operands(this ptr<Store> _addr_s, slice<ptr<Value>> rands)
        {
            ref Store s = ref _addr_s.val;

            return append(rands, _addr_s.Addr, _addr_s.Val);
        }

        private static slice<ptr<Value>> Operands(this ptr<TypeAssert> _addr_v, slice<ptr<Value>> rands)
        {
            ref TypeAssert v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        private static slice<ptr<Value>> Operands(this ptr<UnOp> _addr_v, slice<ptr<Value>> rands)
        {
            ref UnOp v = ref _addr_v.val;

            return append(rands, _addr_v.X);
        }

        // Non-Instruction Values:
        private static slice<ptr<Value>> Operands(this ptr<Builtin> _addr_v, slice<ptr<Value>> rands)
        {
            ref Builtin v = ref _addr_v.val;

            return rands;
        }
        private static slice<ptr<Value>> Operands(this ptr<FreeVar> _addr_v, slice<ptr<Value>> rands)
        {
            ref FreeVar v = ref _addr_v.val;

            return rands;
        }
        private static slice<ptr<Value>> Operands(this ptr<Const> _addr_v, slice<ptr<Value>> rands)
        {
            ref Const v = ref _addr_v.val;

            return rands;
        }
        private static slice<ptr<Value>> Operands(this ptr<Function> _addr_v, slice<ptr<Value>> rands)
        {
            ref Function v = ref _addr_v.val;

            return rands;
        }
        private static slice<ptr<Value>> Operands(this ptr<Global> _addr_v, slice<ptr<Value>> rands)
        {
            ref Global v = ref _addr_v.val;

            return rands;
        }
        private static slice<ptr<Value>> Operands(this ptr<Parameter> _addr_v, slice<ptr<Value>> rands)
        {
            ref Parameter v = ref _addr_v.val;

            return rands;
        }
    }
}}}}}

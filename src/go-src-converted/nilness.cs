// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package nilness inspects the control-flow graph of an SSA function
// and reports errors such as nil pointer dereferences and degenerate
// nil pointer comparisons.
// package nilness -- go2cs converted at 2020 October 09 06:04:06 UTC
// import "golang.org/x/tools/go/analysis/passes/nilness" ==> using nilness = go.golang.org.x.tools.go.analysis.passes.nilness_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\nilness\nilness.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using buildssa = go.golang.org.x.tools.go.analysis.passes.buildssa_package;
using ssa = go.golang.org.x.tools.go.ssa_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class nilness_package
    {
        public static readonly @string Doc = (@string)@"check for redundant or impossible nil comparisons

The nilness checker inspects the control-flow graph of each function in
a package and reports nil pointer dereferences, degenerate nil
pointers, and panics with nil values. A degenerate comparison is of the form
x==nil or x!=nil where x is statically known to be nil or non-nil. These are
often a mistake, especially in control flow related to errors. Panics with nil
values are checked because they are not detectable by

	if r := recover(); r != nil {

This check reports conditions such as:

	if f == nil { // impossible condition (f is a function)
	}

and:

	p := &v
	...
	if p != nil { // tautological condition
	}

and:

	if p == nil {
		print(*p) // nil dereference
	}

and:

	if p == nil {
		panic(p)
	}
";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"nilness",Doc:Doc,Run:run,Requires:[]*analysis.Analyzer{buildssa.Analyzer},));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<buildssa.SSA> ssainput = pass.ResultOf[buildssa.Analyzer]._<ptr<buildssa.SSA>>();
            foreach (var (_, fn) in ssainput.SrcFuncs)
            {
                runFunc(_addr_pass, _addr_fn);
            }
            return (null, error.As(null!)!);

        }

        private static void runFunc(ptr<analysis.Pass> _addr_pass, ptr<ssa.Function> _addr_fn)
        {
            ref analysis.Pass pass = ref _addr_pass.val;
            ref ssa.Function fn = ref _addr_fn.val;

            Action<@string, token.Pos, @string, object[]> reportf = (category, pos, format, args) =>
            {
                pass.Report(new analysis.Diagnostic(Pos:pos,Category:category,Message:fmt.Sprintf(format,args...),));
            } 

            // notNil reports an error if v is provably nil.
; 

            // notNil reports an error if v is provably nil.
            Action<slice<fact>, ssa.Instruction, ssa.Value, @string> notNil = (stack, instr, v, descr) =>
            {
                if (nilnessOf(stack, v) == isnil)
                {
                    reportf("nilderef", instr.Pos(), "nil dereference in " + descr);
                }

            } 

            // visit visits reachable blocks of the CFG in dominance order,
            // maintaining a stack of dominating nilness facts.
            //
            // By traversing the dom tree, we can pop facts off the stack as
            // soon as we've visited a subtree.  Had we traversed the CFG,
            // we would need to retain the set of facts for each block.
; 

            // visit visits reachable blocks of the CFG in dominance order,
            // maintaining a stack of dominating nilness facts.
            //
            // By traversing the dom tree, we can pop facts off the stack as
            // soon as we've visited a subtree.  Had we traversed the CFG,
            // we would need to retain the set of facts for each block.
            var seen = make_slice<bool>(len(fn.Blocks)); // seen[i] means visit should ignore block i
            Action<ptr<ssa.BasicBlock>, slice<fact>> visit = default;
            visit = (b, stack) =>
            {
                if (seen[b.Index])
                {
                    return ;
                }

                seen[b.Index] = true; 

                // Report nil dereferences.
                {
                    var instr__prev1 = instr;

                    foreach (var (_, __instr) in b.Instrs)
                    {
                        instr = __instr;
                        switch (instr.type())
                        {
                            case ssa.CallInstruction instr:
                                notNil(stack, instr, instr.Common().Value, instr.Common().Description());
                                break;
                            case ptr<ssa.FieldAddr> instr:
                                notNil(stack, instr, instr.X, "field selection");
                                break;
                            case ptr<ssa.IndexAddr> instr:
                                notNil(stack, instr, instr.X, "index operation");
                                break;
                            case ptr<ssa.MapUpdate> instr:
                                notNil(stack, instr, instr.Map, "map update");
                                break;
                            case ptr<ssa.Slice> instr:
                                {
                                    ptr<types.Pointer> (_, ok) = instr.X.Type().Underlying()._<ptr<types.Pointer>>();

                                    if (ok)
                                    {
                                        notNil(stack, instr, instr.X, "slice operation");
                                    }

                                }

                                break;
                            case ptr<ssa.Store> instr:
                                notNil(stack, instr, instr.Addr, "store");
                                break;
                            case ptr<ssa.TypeAssert> instr:
                                if (!instr.CommaOk)
                                {
                                    notNil(stack, instr, instr.X, "type assertion");
                                }

                                break;
                            case ptr<ssa.UnOp> instr:
                                if (instr.Op == token.MUL)
                                { // *X
                                    notNil(stack, instr, instr.X, "load");

                                }

                                break;
                        }

                    } 

                    // Look for panics with nil value

                    instr = instr__prev1;
                }

                {
                    var instr__prev1 = instr;

                    foreach (var (_, __instr) in b.Instrs)
                    {
                        instr = __instr;
                        switch (instr.type())
                        {
                            case ptr<ssa.Panic> instr:
                                if (nilnessOf(stack, instr.X) == isnil)
                                {
                                    reportf("nilpanic", instr.Pos(), "panic with nil value");
                                }

                                break;
                        }

                    } 

                    // For nil comparison blocks, report an error if the condition
                    // is degenerate, and push a nilness fact on the stack when
                    // visiting its true and false successor blocks.

                    instr = instr__prev1;
                }

                {
                    var (binop, tsucc, fsucc) = eq(_addr_b);

                    if (binop != null)
                    {
                        var xnil = nilnessOf(stack, binop.X);
                        var ynil = nilnessOf(stack, binop.Y);

                        if (ynil != unknown && xnil != unknown && (xnil == isnil || ynil == isnil))
                        { 
                            // Degenerate condition:
                            // the nilness of both operands is known,
                            // and at least one of them is nil.
                            @string adj = default;
                            if ((xnil == ynil) == (binop.Op == token.EQL))
                            {
                                adj = "tautological";
                            }
                            else
                            {
                                adj = "impossible";
                            }

                            reportf("cond", binop.Pos(), "%s condition: %s %s %s", adj, xnil, binop.Op, ynil); 

                            // If tsucc's or fsucc's sole incoming edge is impossible,
                            // it is unreachable.  Prune traversal of it and
                            // all the blocks it dominates.
                            // (We could be more precise with full dataflow
                            // analysis of control-flow joins.)
                            ptr<ssa.BasicBlock> skip;
                            if (xnil == ynil)
                            {
                                skip = fsucc;
                            }
                            else
                            {
                                skip = tsucc;
                            }

                            {
                                var d__prev1 = d;

                                foreach (var (_, __d) in b.Dominees())
                                {
                                    d = __d;
                                    if (d == skip && len(d.Preds) == 1L)
                                    {
                                        continue;
                                    }

                                    visit(d, stack);

                                }

                                d = d__prev1;
                            }

                            return ;

                        } 

                        // "if x == nil" or "if nil == y" condition; x, y are unknown.
                        if (xnil == isnil || ynil == isnil)
                        {
                            facts newFacts = default;
                            if (xnil == isnil)
                            { 
                                // x is nil, y is unknown:
                                // t successor learns y is nil.
                                newFacts = expandFacts(new fact(binop.Y,isnil));

                            }
                            else
                            { 
                                // x is nil, y is unknown:
                                // t successor learns x is nil.
                                newFacts = expandFacts(new fact(binop.X,isnil));

                            }

                            {
                                var d__prev1 = d;

                                foreach (var (_, __d) in b.Dominees())
                                {
                                    d = __d; 
                                    // Successor blocks learn a fact
                                    // only at non-critical edges.
                                    // (We could do be more precise with full dataflow
                                    // analysis of control-flow joins.)
                                    var s = stack;
                                    if (len(d.Preds) == 1L)
                                    {
                                        if (d == tsucc)
                                        {
                                            s = append(s, newFacts);
                                        }
                                        else if (d == fsucc)
                                        {
                                            s = append(s, newFacts.negate());
                                        }

                                    }

                                    visit(d, s);

                                }

                                d = d__prev1;
                            }

                            return ;

                        }

                    }

                }


                {
                    var d__prev1 = d;

                    foreach (var (_, __d) in b.Dominees())
                    {
                        d = __d;
                        visit(d, stack);
                    }

                    d = d__prev1;
                }
            } 

            // Visit the entry block.  No need to visit fn.Recover.
; 

            // Visit the entry block.  No need to visit fn.Recover.
            if (fn.Blocks != null)
            {
                visit(fn.Blocks[0L], make_slice<fact>(0L, 20L)); // 20 is plenty
            }

        }

        // A fact records that a block is dominated
        // by the condition v == nil or v != nil.
        private partial struct fact
        {
            public ssa.Value value;
            public nilness nilness;
        }

        private static fact negate(this fact f)
        {
            return new fact(f.value,-f.nilness);
        }

        private partial struct nilness // : long
        {
        }

        private static readonly long isnonnil = (long)-1L;
        private static readonly nilness unknown = (nilness)0L;
        private static readonly long isnil = (long)1L;


        private static @string nilnessStrings = new slice<@string>(new @string[] { "non-nil", "unknown", "nil" });

        private static @string String(this nilness n)
        {
            return nilnessStrings[n + 1L];
        }

        // nilnessOf reports whether v is definitely nil, definitely not nil,
        // or unknown given the dominating stack of facts.
        private static nilness nilnessOf(slice<fact> stack, ssa.Value v)
        {
            switch (v.type())
            {
                case ptr<ssa.ChangeInterface> v:
                    {
                        var underlying = nilnessOf(stack, v.X);

                        if (underlying != unknown)
                        {
                            return underlying;
                        }

                    }

                    break; 

                // Is value intrinsically nil or non-nil?
            } 

            // Is value intrinsically nil or non-nil?
            switch (v.type())
            {
                case ptr<ssa.Alloc> v:
                    return isnonnil;
                    break;
                case ptr<ssa.FieldAddr> v:
                    return isnonnil;
                    break;
                case ptr<ssa.FreeVar> v:
                    return isnonnil;
                    break;
                case ptr<ssa.Function> v:
                    return isnonnil;
                    break;
                case ptr<ssa.Global> v:
                    return isnonnil;
                    break;
                case ptr<ssa.IndexAddr> v:
                    return isnonnil;
                    break;
                case ptr<ssa.MakeChan> v:
                    return isnonnil;
                    break;
                case ptr<ssa.MakeClosure> v:
                    return isnonnil;
                    break;
                case ptr<ssa.MakeInterface> v:
                    return isnonnil;
                    break;
                case ptr<ssa.MakeMap> v:
                    return isnonnil;
                    break;
                case ptr<ssa.MakeSlice> v:
                    return isnonnil;
                    break;
                case ptr<ssa.Const> v:
                    if (v.IsNil())
                    {
                        return isnil;
                    }
                    else
                    {
                        return isnonnil;
                    }

                    break; 

                // Search dominating control-flow facts.
            } 

            // Search dominating control-flow facts.
            foreach (var (_, f) in stack)
            {
                if (f.value == v)
                {
                    return f.nilness;
                }

            }
            return unknown;

        }

        // If b ends with an equality comparison, eq returns the operation and
        // its true (equal) and false (not equal) successors.
        private static (ptr<ssa.BinOp>, ptr<ssa.BasicBlock>, ptr<ssa.BasicBlock>) eq(ptr<ssa.BasicBlock> _addr_b)
        {
            ptr<ssa.BinOp> op = default!;
            ptr<ssa.BasicBlock> tsucc = default!;
            ptr<ssa.BasicBlock> fsucc = default!;
            ref ssa.BasicBlock b = ref _addr_b.val;

            {
                ptr<ssa.If> (If, ok) = b.Instrs[len(b.Instrs) - 1L]._<ptr<ssa.If>>();

                if (ok)
                {
                    {
                        ptr<ssa.BinOp> (binop, ok) = If.Cond._<ptr<ssa.BinOp>>();

                        if (ok)
                        {

                            if (binop.Op == token.EQL) 
                                return (_addr_binop!, _addr_b.Succs[0L]!, _addr_b.Succs[1L]!);
                            else if (binop.Op == token.NEQ) 
                                return (_addr_binop!, _addr_b.Succs[1L]!, _addr_b.Succs[0L]!);
                            
                        }

                    }

                }

            }

            return (_addr_null!, _addr_null!, _addr_null!);

        }

        // expandFacts takes a single fact and returns the set of facts that can be
        // known about it or any of its related values. Some operations, like
        // ChangeInterface, have transitive nilness, such that if you know the
        // underlying value is nil, you also know the value itself is nil, and vice
        // versa. This operation allows callers to match on any of the related values
        // in analyses, rather than just the one form of the value that happend to
        // appear in a comparison.
        //
        // This work must be in addition to unwrapping values within nilnessOf because
        // while this work helps give facts about transitively known values based on
        // inferred facts, the recursive check within nilnessOf covers cases where
        // nilness facts are intrinsic to the underlying value, such as a zero value
        // interface variables.
        //
        // ChangeInterface is the only expansion currently supported, but others, like
        // Slice, could be added. At this time, this tool does not check slice
        // operations in a way this expansion could help. See
        // https://play.golang.org/p/mGqXEp7w4fR for an example.
        private static slice<fact> expandFacts(fact f)
        {
            fact ff = new slice<fact>(new fact[] { f });

Loop:

            while (true)
            {
                switch (f.value.type())
                {
                    case ptr<ssa.ChangeInterface> v:
                        f = new fact(v.X,f.nilness);
                        ff = append(ff, f);
                        break;
                    default:
                    {
                        var v = f.value.type();
                        _breakLoop = true;
                        break;
                        break;
                    }
                }

            }

            return ff;

        }

        private partial struct facts // : slice<fact>
        {
        }

        private static facts negate(this facts ff)
        {
            var nn = make_slice<fact>(len(ff));
            foreach (var (i, f) in ff)
            {
                nn[i] = f.negate();
            }
            return nn;

        }
    }
}}}}}}}

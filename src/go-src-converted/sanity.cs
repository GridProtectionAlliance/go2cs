// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:57:08 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\sanity.go
// An optional pass for sanity-checking invariants of the SSA representation.
// Currently it checks CFG invariants but little at the instruction level.

using fmt = go.fmt_package;
using types = go.go.types_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        private partial struct sanity
        {
            public io.Writer reporter;
            public ptr<Function> fn;
            public ptr<BasicBlock> block;
            public bool insane;
        }

        // sanityCheck performs integrity checking of the SSA representation
        // of the function fn and returns true if it was valid.  Diagnostics
        // are written to reporter if non-nil, os.Stderr otherwise.  Some
        // diagnostics are only warnings and do not imply a negative result.
        //
        // Sanity-checking is intended to facilitate the debugging of code
        // transformation passes.
        //
        private static bool sanityCheck(ptr<Function> _addr_fn, io.Writer reporter)
        {
            ref Function fn = ref _addr_fn.val;

            if (reporter == null)
            {
                reporter = os.Stderr;
            }

            return (addr(new sanity(reporter:reporter))).checkFunction(fn);

        }

        // mustSanityCheck is like sanityCheck but panics instead of returning
        // a negative result.
        //
        private static void mustSanityCheck(ptr<Function> _addr_fn, io.Writer reporter) => func((_, panic, __) =>
        {
            ref Function fn = ref _addr_fn.val;

            if (!sanityCheck(_addr_fn, reporter))
            {
                fn.WriteTo(os.Stderr);
                panic("SanityCheck failed");
            }

        });

        private static void diagnostic(this ptr<sanity> _addr_s, @string prefix, @string format, params object[] args)
        {
            args = args.Clone();
            ref sanity s = ref _addr_s.val;

            fmt.Fprintf(s.reporter, "%s: function %s", prefix, s.fn);
            if (s.block != null)
            {
                fmt.Fprintf(s.reporter, ", block %s", s.block);
            }

            io.WriteString(s.reporter, ": ");
            fmt.Fprintf(s.reporter, format, args);
            io.WriteString(s.reporter, "\n");

        }

        private static void errorf(this ptr<sanity> _addr_s, @string format, params object[] args)
        {
            args = args.Clone();
            ref sanity s = ref _addr_s.val;

            s.insane = true;
            s.diagnostic("Error", format, args);
        }

        private static void warnf(this ptr<sanity> _addr_s, @string format, params object[] args)
        {
            args = args.Clone();
            ref sanity s = ref _addr_s.val;

            s.diagnostic("Warning", format, args);
        }

        // findDuplicate returns an arbitrary basic block that appeared more
        // than once in blocks, or nil if all were unique.
        private static ptr<BasicBlock> findDuplicate(slice<ptr<BasicBlock>> blocks)
        {
            if (len(blocks) < 2L)
            {
                return _addr_null!;
            }

            if (blocks[0L] == blocks[1L])
            {
                return _addr_blocks[0L]!;
            } 
            // Slow path:
            var m = make_map<ptr<BasicBlock>, bool>();
            foreach (var (_, b) in blocks)
            {
                if (m[b])
                {
                    return _addr_b!;
                }

                m[b] = true;

            }
            return _addr_null!;

        }

        private static void checkInstr(this ptr<sanity> _addr_s, long idx, Instruction instr) => func((_, panic, __) =>
        {
            ref sanity s = ref _addr_s.val;

            switch (instr.type())
            {
                case ptr<If> instr:
                    s.errorf("control flow instruction not at end of block");
                    break;
                case ptr<Jump> instr:
                    s.errorf("control flow instruction not at end of block");
                    break;
                case ptr<Return> instr:
                    s.errorf("control flow instruction not at end of block");
                    break;
                case ptr<Panic> instr:
                    s.errorf("control flow instruction not at end of block");
                    break;
                case ptr<Phi> instr:
                    if (idx == 0L)
                    { 
                        // It suffices to apply this check to just the first phi node.
                        {
                            var dup = findDuplicate(s.block.Preds);

                            if (dup != null)
                            {
                                s.errorf("phi node in block with duplicate predecessor %s", dup);
                            }

                        }

                    }
                    else
                    {
                        var prev = s.block.Instrs[idx - 1L];
                        {
                            ptr<Phi> (_, ok) = prev._<ptr<Phi>>();

                            if (!ok)
                            {
                                s.errorf("Phi instruction follows a non-Phi: %T", prev);
                            }

                        }

                    }

                    {
                        var ne = len(instr.Edges);
                        var np = len(s.block.Preds);

                        if (ne != np)
                        {
                            s.errorf("phi node has %d edges but %d predecessors", ne, np);
                        }
                        else
                        {
                            foreach (var (i, e) in instr.Edges)
                            {
                                if (e == null)
                                {
                                    s.errorf("phi node '%s' has no value for edge #%d from %s", instr.Comment, i, s.block.Preds[i]);
                                }

                            }

                        }

                    }


                    break;
                case ptr<Alloc> instr:
                    if (!instr.Heap)
                    {
                        var found = false;
                        foreach (var (_, l) in s.fn.Locals)
                        {
                            if (l == instr)
                            {
                                found = true;
                                break;
                            }

                        }
                        if (!found)
                        {
                            s.errorf("local alloc %s = %s does not appear in Function.Locals", instr.Name(), instr);
                        }

                    }

                    break;
                case ptr<BinOp> instr:
                    break;
                case ptr<Call> instr:
                    break;
                case ptr<ChangeInterface> instr:
                    break;
                case ptr<ChangeType> instr:
                    break;
                case ptr<Convert> instr:
                    {
                        (_, ok) = instr.X.Type().Underlying()._<ptr<types.Basic>>();

                        if (!ok)
                        {
                            {
                                (_, ok) = instr.Type().Underlying()._<ptr<types.Basic>>();

                                if (!ok)
                                {
                                    s.errorf("convert %s -> %s: at least one type must be basic", instr.X.Type(), instr.Type());
                                }

                            }

                        }

                    }


                    break;
                case ptr<Defer> instr:
                    break;
                case ptr<Extract> instr:
                    break;
                case ptr<Field> instr:
                    break;
                case ptr<FieldAddr> instr:
                    break;
                case ptr<Go> instr:
                    break;
                case ptr<Index> instr:
                    break;
                case ptr<IndexAddr> instr:
                    break;
                case ptr<Lookup> instr:
                    break;
                case ptr<MakeChan> instr:
                    break;
                case ptr<MakeClosure> instr:
                    var numFree = len(instr.Fn._<ptr<Function>>().FreeVars);
                    var numBind = len(instr.Bindings);
                    if (numFree != numBind)
                    {
                        s.errorf("MakeClosure has %d Bindings for function %s with %d free vars", numBind, instr.Fn, numFree);
                    }

                    {
                        ptr<types.Signature> recv = instr.Type()._<ptr<types.Signature>>().Recv();

                        if (recv != null)
                        {
                            s.errorf("MakeClosure's type includes receiver %s", recv.Type());
                        }

                    }


                    break;
                case ptr<MakeInterface> instr:
                    break;
                case ptr<MakeMap> instr:
                    break;
                case ptr<MakeSlice> instr:
                    break;
                case ptr<MapUpdate> instr:
                    break;
                case ptr<Next> instr:
                    break;
                case ptr<Range> instr:
                    break;
                case ptr<RunDefers> instr:
                    break;
                case ptr<Select> instr:
                    break;
                case ptr<Send> instr:
                    break;
                case ptr<Slice> instr:
                    break;
                case ptr<Store> instr:
                    break;
                case ptr<TypeAssert> instr:
                    break;
                case ptr<UnOp> instr:
                    break;
                case ptr<DebugRef> instr:
                    break;
                default:
                {
                    var instr = instr.type();
                    panic(fmt.Sprintf("Unknown instruction type: %T", instr));
                    break;
                }

            }

            {
                CallInstruction (call, ok) = instr._<CallInstruction>();

                if (ok)
                {
                    if (call.Common().Signature() == null)
                    {
                        s.errorf("nil signature: %s", call);
                    }

                } 

                // Check that value-defining instructions have valid types
                // and a valid referrer list.

            } 

            // Check that value-defining instructions have valid types
            // and a valid referrer list.
            {
                Value (v, ok) = instr._<Value>();

                if (ok)
                {
                    var t = v.Type();
                    if (t == null)
                    {
                        s.errorf("no type: %s = %s", v.Name(), v);
                    }
                    else if (t == tRangeIter)
                    { 
                        // not a proper type; ignore.
                    }                    {
                        ptr<types.Basic> (b, ok) = t.Underlying()._<ptr<types.Basic>>();


                        else if (ok && b.Info() & types.IsUntyped != 0L)
                        {
                            s.errorf("instruction has 'untyped' result: %s = %s : %s", v.Name(), v, t);
                        }

                    }

                    s.checkReferrerList(v);

                } 

                // Untyped constants are legal as instruction Operands(),
                // for example:
                //   _ = "foo"[0]
                // or:
                //   if wordsize==64 {...}

                // All other non-Instruction Values can be found via their
                // enclosing Function or Package.

            } 

            // Untyped constants are legal as instruction Operands(),
            // for example:
            //   _ = "foo"[0]
            // or:
            //   if wordsize==64 {...}

            // All other non-Instruction Values can be found via their
            // enclosing Function or Package.
        });

        private static void checkFinalInstr(this ptr<sanity> _addr_s, Instruction instr)
        {
            ref sanity s = ref _addr_s.val;

            switch (instr.type())
            {
                case ptr<If> instr:
                    {
                        var nsuccs__prev1 = nsuccs;

                        var nsuccs = len(s.block.Succs);

                        if (nsuccs != 2L)
                        {
                            s.errorf("If-terminated block has %d successors; expected 2", nsuccs);
                            return ;
                        }

                        nsuccs = nsuccs__prev1;

                    }

                    if (s.block.Succs[0L] == s.block.Succs[1L])
                    {
                        s.errorf("If-instruction has same True, False target blocks: %s", s.block.Succs[0L]);
                        return ;
                    }

                    break;
                case ptr<Jump> instr:
                    {
                        var nsuccs__prev1 = nsuccs;

                        nsuccs = len(s.block.Succs);

                        if (nsuccs != 1L)
                        {
                            s.errorf("Jump-terminated block has %d successors; expected 1", nsuccs);
                            return ;
                        }

                        nsuccs = nsuccs__prev1;

                    }


                    break;
                case ptr<Return> instr:
                    {
                        var nsuccs__prev1 = nsuccs;

                        nsuccs = len(s.block.Succs);

                        if (nsuccs != 0L)
                        {
                            s.errorf("Return-terminated block has %d successors; expected none", nsuccs);
                            return ;
                        }

                        nsuccs = nsuccs__prev1;

                    }

                    {
                        var na = len(instr.Results);
                        var nf = s.fn.Signature.Results().Len();

                        if (nf != na)
                        {
                            s.errorf("%d-ary return in %d-ary function", na, nf);
                        }

                    }


                    break;
                case ptr<Panic> instr:
                    {
                        var nsuccs__prev1 = nsuccs;

                        nsuccs = len(s.block.Succs);

                        if (nsuccs != 0L)
                        {
                            s.errorf("Panic-terminated block has %d successors; expected none", nsuccs);
                            return ;
                        }

                        nsuccs = nsuccs__prev1;

                    }


                    break;
                default:
                {
                    var instr = instr.type();
                    s.errorf("non-control flow instruction at end of block");
                    break;
                }
            }

        }

        private static void checkBlock(this ptr<sanity> _addr_s, ptr<BasicBlock> _addr_b, long index)
        {
            ref sanity s = ref _addr_s.val;
            ref BasicBlock b = ref _addr_b.val;

            s.block = b;

            if (b.Index != index)
            {
                s.errorf("block has incorrect Index %d", b.Index);
            }

            if (b.parent != s.fn)
            {
                s.errorf("block has incorrect parent %s", b.parent);
            } 

            // Check all blocks are reachable.
            // (The entry block is always implicitly reachable,
            // as is the Recover block, if any.)
            if ((index > 0L && b != b.parent.Recover) && len(b.Preds) == 0L)
            {
                s.warnf("unreachable block");
                if (b.Instrs == null)
                { 
                    // Since this block is about to be pruned,
                    // tolerating transient problems in it
                    // simplifies other optimizations.
                    return ;

                }

            } 

            // Check predecessor and successor relations are dual,
            // and that all blocks in CFG belong to same function.
            foreach (var (_, a) in b.Preds)
            {
                var found = false;
                {
                    var bb__prev2 = bb;

                    foreach (var (_, __bb) in a.Succs)
                    {
                        bb = __bb;
                        if (bb == b)
                        {
                            found = true;
                            break;
                        }

                    }

                    bb = bb__prev2;
                }

                if (!found)
                {
                    s.errorf("expected successor edge in predecessor %s; found only: %s", a, a.Succs);
                }

                if (a.parent != s.fn)
                {
                    s.errorf("predecessor %s belongs to different function %s", a, a.parent);
                }

            }
            foreach (var (_, c) in b.Succs)
            {
                found = false;
                {
                    var bb__prev2 = bb;

                    foreach (var (_, __bb) in c.Preds)
                    {
                        bb = __bb;
                        if (bb == b)
                        {
                            found = true;
                            break;
                        }

                    }

                    bb = bb__prev2;
                }

                if (!found)
                {
                    s.errorf("expected predecessor edge in successor %s; found only: %s", c, c.Preds);
                }

                if (c.parent != s.fn)
                {
                    s.errorf("successor %s belongs to different function %s", c, c.parent);
                }

            } 

            // Check each instruction is sane.
            var n = len(b.Instrs);
            if (n == 0L)
            {
                s.errorf("basic block contains no instructions");
            }

            array<ptr<Value>> rands = new array<ptr<Value>>(10L); // reuse storage
            foreach (var (j, instr) in b.Instrs)
            {
                if (instr == null)
                {
                    s.errorf("nil instruction at index %d", j);
                    continue;
                }

                {
                    var b2 = instr.Block();

                    if (b2 == null)
                    {
                        s.errorf("nil Block() for instruction at index %d", j);
                        continue;
                    }
                    else if (b2 != b)
                    {
                        s.errorf("wrong Block() (%s) for instruction at index %d ", b2, j);
                        continue;
                    }


                }

                if (j < n - 1L)
                {
                    s.checkInstr(j, instr);
                }
                else
                {
                    s.checkFinalInstr(instr);
                } 

                // Check Instruction.Operands.
operands:
                foreach (var (i, op) in instr.Operands(rands[..0L]))
                {
                    if (op == null)
                    {
                        s.errorf("nil operand pointer %d of %s", i, instr);
                        continue;
                    }

                    var val = op.val;
                    if (val == null)
                    {
                        continue; // a nil operand is ok
                    } 

                    // Check that "untyped" types only appear on constant operands.
                    {
                        ptr<Const> (_, ok) = (op.val)._<ptr<Const>>();

                        if (!ok)
                        {
                            {
                                ptr<types.Basic> (basic, ok) = ptr<op>()._<ptr<types.Basic>>();

                                if (ok)
                                {
                                    if (basic.Info() & types.IsUntyped != 0L)
                                    {
                                        s.errorf("operand #%d of %s is untyped: %s", i, instr, basic);
                                    }

                                }

                            }

                        } 

                        // Check that Operands that are also Instructions belong to same function.
                        // TODO(adonovan): also check their block dominates block b.

                    } 

                    // Check that Operands that are also Instructions belong to same function.
                    // TODO(adonovan): also check their block dominates block b.
                    {
                        var val__prev1 = val;

                        Instruction (val, ok) = val._<Instruction>();

                        if (ok)
                        {
                            if (val.Block() == null)
                            {
                                s.errorf("operand %d of %s is an instruction (%s) that belongs to no block", i, instr, val);
                            }
                            else if (val.Parent() != s.fn)
                            {
                                s.errorf("operand %d of %s is an instruction (%s) from function %s", i, instr, val, val.Parent());
                            }

                        } 

                        // Check that each function-local operand of
                        // instr refers back to instr.  (NB: quadratic)

                        val = val__prev1;

                    } 

                    // Check that each function-local operand of
                    // instr refers back to instr.  (NB: quadratic)
                    switch (val.type())
                    {
                        case ptr<Const> val:
                            continue; // not local
                            break;
                        case ptr<Global> val:
                            continue; // not local
                            break;
                        case ptr<Builtin> val:
                            continue; // not local
                            break;
                        case ptr<Function> val:
                            if (val.parent == null)
                            {
                                continue; // only anon functions are local
                            }

                            break; 

                        // TODO(adonovan): check val.Parent() != nil <=> val.Referrers() is defined.

                    } 

                    // TODO(adonovan): check val.Parent() != nil <=> val.Referrers() is defined.

                    {
                        var refs = val.Referrers();

                        if (refs != null)
                        {
                            foreach (var (_, ref) in refs.val)
                            {
                                if (ref == instr)
                                {
                                    _continueoperands = true;
                                    break;
                                }

                            }
                        else
                            s.errorf("operand %d of %s (%s) does not refer to us", i, instr, val);

                        }                        {
                            s.errorf("operand %d of %s (%s) has no referrers", i, instr, val);
                        }

                    }

                }

            }

        }

        private static void checkReferrerList(this ptr<sanity> _addr_s, Value v)
        {
            ref sanity s = ref _addr_s.val;

            var refs = v.Referrers();
            if (refs == null)
            {
                s.errorf("%s has missing referrer list", v.Name());
                return ;
            }

            foreach (var (i, ref) in refs.val)
            {
                {
                    var (_, ok) = s.instrs[ref];

                    if (!ok)
                    {
                        s.errorf("%s.Referrers()[%d] = %s is not an instruction belonging to this function", v.Name(), i, ref);
                    }

                }

            }

        }

        private static bool checkFunction(this ptr<sanity> _addr_s, ptr<Function> _addr_fn)
        {
            ref sanity s = ref _addr_s.val;
            ref Function fn = ref _addr_fn.val;
 
            // TODO(adonovan): check Function invariants:
            // - check params match signature
            // - check transient fields are nil
            // - warn if any fn.Locals do not appear among block instructions.
            s.fn = fn;
            if (fn.Prog == null)
            {
                s.errorf("nil Prog");
            }

            _ = fn.String(); // must not crash
            _ = fn.RelString(fn.pkg()); // must not crash

            // All functions have a package, except delegates (which are
            // shared across packages, or duplicated as weak symbols in a
            // separate-compilation model), and error.Error.
            if (fn.Pkg == null)
            {
                if (strings.HasPrefix(fn.Synthetic, "wrapper ") || strings.HasPrefix(fn.Synthetic, "bound ") || strings.HasPrefix(fn.Synthetic, "thunk ") || strings.HasSuffix(fn.name, "Error"))
                { 
                    // ok
                }
                else
                {
                    s.errorf("nil Pkg");
                }

            }

            {
                var src = fn.Synthetic == "";
                var syn = fn.Syntax() != null;

                if (src != syn)
                {
                    s.errorf("got fromSource=%t, hasSyntax=%t; want same values", src, syn);
                }

            }

            {
                var i__prev1 = i;

                foreach (var (__i, __l) in fn.Locals)
                {
                    i = __i;
                    l = __l;
                    if (l.Parent() != fn)
                    {
                        s.errorf("Local %s at index %d has wrong parent", l.Name(), i);
                    }

                    if (l.Heap)
                    {
                        s.errorf("Local %s at index %d has Heap flag set", l.Name(), i);
                    }

                } 
                // Build the set of valid referrers.

                i = i__prev1;
            }

            s.instrs = make();
            {
                var b__prev1 = b;

                foreach (var (_, __b) in fn.Blocks)
                {
                    b = __b;
                    foreach (var (_, instr) in b.Instrs)
                    {
                        s.instrs[instr] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
                    }

                }

                b = b__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i, __p) in fn.Params)
                {
                    i = __i;
                    p = __p;
                    if (p.Parent() != fn)
                    {
                        s.errorf("Param %s at index %d has wrong parent", p.Name(), i);
                    } 
                    // Check common suffix of Signature and Params match type.
                    {
                        var sig = fn.Signature;

                        if (sig != null)
                        {
                            var j = i - len(fn.Params) + sig.Params().Len(); // index within sig.Params
                            if (j < 0L)
                            {
                                continue;
                            }

                            if (!types.Identical(p.Type(), sig.Params().At(j).Type()))
                            {
                                s.errorf("Param %s at index %d has wrong type (%s, versus %s in Signature)", p.Name(), i, p.Type(), sig.Params().At(j).Type());
                            }

                        }

                    }

                    s.checkReferrerList(p);

                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i, __fv) in fn.FreeVars)
                {
                    i = __i;
                    fv = __fv;
                    if (fv.Parent() != fn)
                    {
                        s.errorf("FreeVar %s at index %d has wrong parent", fv.Name(), i);
                    }

                    s.checkReferrerList(fv);

                }

                i = i__prev1;
            }

            if (fn.Blocks != null && len(fn.Blocks) == 0L)
            { 
                // Function _had_ blocks (so it's not external) but
                // they were "optimized" away, even the entry block.
                s.errorf("Blocks slice is non-nil but empty");

            }

            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in fn.Blocks)
                {
                    i = __i;
                    b = __b;
                    if (b == null)
                    {
                        s.warnf("nil *BasicBlock at f.Blocks[%d]", i);
                        continue;
                    }

                    s.checkBlock(b, i);

                }

                i = i__prev1;
                b = b__prev1;
            }

            if (fn.Recover != null && fn.Blocks[fn.Recover.Index] != fn.Recover)
            {
                s.errorf("Recover block is not in Blocks slice");
            }

            s.block = null;
            {
                var i__prev1 = i;

                foreach (var (__i, __anon) in fn.AnonFuncs)
                {
                    i = __i;
                    anon = __anon;
                    if (anon.Parent() != fn)
                    {
                        s.errorf("AnonFuncs[%d]=%s but %s.Parent()=%s", i, anon, anon, anon.Parent());
                    }

                }

                i = i__prev1;
            }

            s.fn = null;
            return !s.insane;

        }

        // sanityCheckPackage checks invariants of packages upon creation.
        // It does not require that the package is built.
        // Unlike sanityCheck (for functions), it just panics at the first error.
        private static void sanityCheckPackage(ptr<Package> _addr_pkg) => func((_, panic, __) =>
        {
            ref Package pkg = ref _addr_pkg.val;

            if (pkg.Pkg == null)
            {
                panic(fmt.Sprintf("Package %s has no Object", pkg));
            }

            _ = pkg.String(); // must not crash

            foreach (var (name, mem) in pkg.Members)
            {
                if (name != mem.Name())
                {
                    panic(fmt.Sprintf("%s: %T.Name() = %s, want %s", pkg.Pkg.Path(), mem, mem.Name(), name));
                }

                var obj = mem.Object();
                if (obj == null)
                { 
                    // This check is sound because fields
                    // {Global,Function}.object have type
                    // types.Object.  (If they were declared as
                    // *types.{Var,Func}, we'd have a non-empty
                    // interface containing a nil pointer.)

                    continue; // not all members have typechecker objects
                }

                if (obj.Name() != name)
                {
                    if (obj.Name() == "init" && strings.HasPrefix(mem.Name(), "init#"))
                    { 
                        // Ok.  The name of a declared init function varies between
                        // its types.Func ("init") and its ssa.Function ("init#%d").
                    }
                    else
                    {
                        panic(fmt.Sprintf("%s: %T.Object().Name() = %s, want %s", pkg.Pkg.Path(), mem, obj.Name(), name));
                    }

                }

                if (obj.Pos() != mem.Pos())
                {
                    panic(fmt.Sprintf("%s Pos=%d obj.Pos=%d", mem, mem.Pos(), obj.Pos()));
                }

            }

        });
    }
}}}}}

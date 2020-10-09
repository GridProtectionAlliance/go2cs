// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssautil -- go2cs converted at 2020 October 09 06:03:53 UTC
// import "golang.org/x/tools/go/ssa/ssautil" ==> using ssautil = go.golang.org.x.tools.go.ssa.ssautil_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\ssautil\switch.go
// This file implements discovery of switch and type-switch constructs
// from low-level control flow.
//
// Many techniques exist for compiling a high-level switch with
// constant cases to efficient machine code.  The optimal choice will
// depend on the data type, the specific case values, the code in the
// body of each case, and the hardware.
// Some examples:
// - a lookup table (for a switch that maps constants to constants)
// - a computed goto
// - a binary tree
// - a perfect hash
// - a two-level switch (to partition constant strings by their first byte).

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;

using ssa = go.golang.org.x.tools.go.ssa_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace ssa
{
    public static partial class ssautil_package
    {
        // A ConstCase represents a single constant comparison.
        // It is part of a Switch.
        public partial struct ConstCase
        {
            public ptr<ssa.BasicBlock> Block; // block performing the comparison
            public ptr<ssa.BasicBlock> Body; // body of the case
            public ptr<ssa.Const> Value; // case comparand
        }

        // A TypeCase represents a single type assertion.
        // It is part of a Switch.
        public partial struct TypeCase
        {
            public ptr<ssa.BasicBlock> Block; // block performing the type assert
            public ptr<ssa.BasicBlock> Body; // body of the case
            public types.Type Type; // case type
            public ssa.Value Binding; // value bound by this case
        }

        // A Switch is a logical high-level control flow operation
        // (a multiway branch) discovered by analysis of a CFG containing
        // only if/else chains.  It is not part of the ssa.Instruction set.
        //
        // One of ConstCases and TypeCases has length >= 2;
        // the other is nil.
        //
        // In a value switch, the list of cases may contain duplicate constants.
        // A type switch may contain duplicate types, or types assignable
        // to an interface type also in the list.
        // TODO(adonovan): eliminate such duplicates.
        //
        public partial struct Switch
        {
            public ptr<ssa.BasicBlock> Start; // block containing start of if/else chain
            public ssa.Value X; // the switch operand
            public slice<ConstCase> ConstCases; // ordered list of constant comparisons
            public slice<TypeCase> TypeCases; // ordered list of type assertions
            public ptr<ssa.BasicBlock> Default; // successor if all comparisons fail
        }

        private static @string String(this ptr<Switch> _addr_sw)
        {
            ref Switch sw = ref _addr_sw.val;
 
            // We represent each block by the String() of its
            // first Instruction, e.g. "print(42:int)".
            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            if (sw.ConstCases != null)
            {
                fmt.Fprintf(_addr_buf, "switch %s {\n", sw.X.Name());
                {
                    var c__prev1 = c;

                    foreach (var (_, __c) in sw.ConstCases)
                    {
                        c = __c;
                        fmt.Fprintf(_addr_buf, "case %s: %s\n", c.Value, c.Body.Instrs[0L]);
                    }
            else

                    c = c__prev1;
                }
            }            {
                fmt.Fprintf(_addr_buf, "switch %s.(type) {\n", sw.X.Name());
                {
                    var c__prev1 = c;

                    foreach (var (_, __c) in sw.TypeCases)
                    {
                        c = __c;
                        fmt.Fprintf(_addr_buf, "case %s %s: %s\n", c.Binding.Name(), c.Type, c.Body.Instrs[0L]);
                    }

                    c = c__prev1;
                }
            }

            if (sw.Default != null)
            {
                fmt.Fprintf(_addr_buf, "default: %s\n", sw.Default.Instrs[0L]);
            }

            fmt.Fprintf(_addr_buf, "}");
            return buf.String();

        }

        // Switches examines the control-flow graph of fn and returns the
        // set of inferred value and type switches.  A value switch tests an
        // ssa.Value for equality against two or more compile-time constant
        // values.  Switches involving link-time constants (addresses) are
        // ignored.  A type switch type-asserts an ssa.Value against two or
        // more types.
        //
        // The switches are returned in dominance order.
        //
        // The resulting switches do not necessarily correspond to uses of the
        // 'switch' keyword in the source: for example, a single source-level
        // switch statement with non-constant cases may result in zero, one or
        // many Switches, one per plural sequence of constant cases.
        // Switches may even be inferred from if/else- or goto-based control flow.
        // (In general, the control flow constructs of the source program
        // cannot be faithfully reproduced from the SSA representation.)
        //
        public static slice<Switch> Switches(ptr<ssa.Function> _addr_fn)
        {
            ref ssa.Function fn = ref _addr_fn.val;
 
            // Traverse the CFG in dominance order, so we don't
            // enter an if/else-chain in the middle.
            slice<Switch> switches = default;
            var seen = make_map<ptr<ssa.BasicBlock>, bool>(); // TODO(adonovan): opt: use ssa.blockSet
            foreach (var (_, b) in fn.DomPreorder())
            {
                {
                    var (x, k) = isComparisonBlock(_addr_b);

                    if (x != null)
                    { 
                        // Block b starts a switch.
                        ref Switch sw = ref heap(new Switch(Start:b,X:x), out ptr<Switch> _addr_sw);
                        valueSwitch(_addr_sw, _addr_k, seen);
                        if (len(sw.ConstCases) > 1L)
                        {
                            switches = append(switches, sw);
                        }

                    }

                }


                {
                    var (y, x, T) = isTypeAssertBlock(_addr_b);

                    if (y != null)
                    { 
                        // Block b starts a type switch.
                        sw = new Switch(Start:b,X:x);
                        typeSwitch(_addr_sw, y, T, seen);
                        if (len(sw.TypeCases) > 1L)
                        {
                            switches = append(switches, sw);
                        }

                    }

                }

            }
            return switches;

        }

        private static void valueSwitch(ptr<Switch> _addr_sw, ptr<ssa.Const> _addr_k, map<ptr<ssa.BasicBlock>, bool> seen)
        {
            ref Switch sw = ref _addr_sw.val;
            ref ssa.Const k = ref _addr_k.val;

            var b = sw.Start;
            var x = sw.X;
            while (x == sw.X)
            {
                if (seen[b])
                {
                    break;
                }

                seen[b] = true;

                sw.ConstCases = append(sw.ConstCases, new ConstCase(Block:b,Body:b.Succs[0],Value:k,));
                b = b.Succs[1L];
                if (len(b.Instrs) > 2L)
                { 
                    // Block b contains not just 'if x == k',
                    // so it may have side effects that
                    // make it unsafe to elide.
                    break;

                }

                if (len(b.Preds) != 1L)
                { 
                    // Block b has multiple predecessors,
                    // so it cannot be treated as a case.
                    break;

                }

                x, k = isComparisonBlock(_addr_b);

            }

            sw.Default = b;

        }

        private static void typeSwitch(ptr<Switch> _addr_sw, ssa.Value y, types.Type T, map<ptr<ssa.BasicBlock>, bool> seen)
        {
            ref Switch sw = ref _addr_sw.val;

            var b = sw.Start;
            var x = sw.X;
            while (x == sw.X)
            {
                if (seen[b])
                {
                    break;
                }

                seen[b] = true;

                sw.TypeCases = append(sw.TypeCases, new TypeCase(Block:b,Body:b.Succs[0],Type:T,Binding:y,));
                b = b.Succs[1L];
                if (len(b.Instrs) > 4L)
                { 
                    // Block b contains not just
                    //  {TypeAssert; Extract #0; Extract #1; If}
                    // so it may have side effects that
                    // make it unsafe to elide.
                    break;

                }

                if (len(b.Preds) != 1L)
                { 
                    // Block b has multiple predecessors,
                    // so it cannot be treated as a case.
                    break;

                }

                y, x, T = isTypeAssertBlock(_addr_b);

            }

            sw.Default = b;

        }

        // isComparisonBlock returns the operands (v, k) if a block ends with
        // a comparison v==k, where k is a compile-time constant.
        //
        private static (ssa.Value, ptr<ssa.Const>) isComparisonBlock(ptr<ssa.BasicBlock> _addr_b)
        {
            ssa.Value v = default;
            ptr<ssa.Const> k = default!;
            ref ssa.BasicBlock b = ref _addr_b.val;

            {
                var n = len(b.Instrs);

                if (n >= 2L)
                {
                    {
                        ptr<ssa.If> (i, ok) = b.Instrs[n - 1L]._<ptr<ssa.If>>();

                        if (ok)
                        {
                            {
                                ptr<ssa.BinOp> (binop, ok) = i.Cond._<ptr<ssa.BinOp>>();

                                if (ok && binop.Block() == b && binop.Op == token.EQL)
                                {
                                    {
                                        ptr<ssa.Const> k__prev4 = k;

                                        ptr<ssa.Const> (k, ok) = binop.Y._<ptr<ssa.Const>>();

                                        if (ok)
                                        {
                                            return (binop.X, _addr_k!);
                                        }

                                        k = k__prev4;

                                    }

                                    {
                                        ptr<ssa.Const> k__prev4 = k;

                                        (k, ok) = binop.X._<ptr<ssa.Const>>();

                                        if (ok)
                                        {
                                            return (binop.Y, _addr_k!);
                                        }

                                        k = k__prev4;

                                    }

                                }

                            }

                        }

                    }

                }

            }

            return ;

        }

        // isTypeAssertBlock returns the operands (y, x, T) if a block ends with
        // a type assertion "if y, ok := x.(T); ok {".
        //
        private static (ssa.Value, ssa.Value, types.Type) isTypeAssertBlock(ptr<ssa.BasicBlock> _addr_b)
        {
            ssa.Value y = default;
            ssa.Value x = default;
            types.Type T = default;
            ref ssa.BasicBlock b = ref _addr_b.val;

            {
                var n = len(b.Instrs);

                if (n >= 4L)
                {
                    {
                        ptr<ssa.If> (i, ok) = b.Instrs[n - 1L]._<ptr<ssa.If>>();

                        if (ok)
                        {
                            {
                                ptr<ssa.Extract> (ext1, ok) = i.Cond._<ptr<ssa.Extract>>();

                                if (ok && ext1.Block() == b && ext1.Index == 1L)
                                {
                                    {
                                        ptr<ssa.TypeAssert> (ta, ok) = ext1.Tuple._<ptr<ssa.TypeAssert>>();

                                        if (ok && ta.Block() == b)
                                        { 
                                            // hack: relies upon instruction ordering.
                                            {
                                                ptr<ssa.Extract> (ext0, ok) = b.Instrs[n - 3L]._<ptr<ssa.Extract>>();

                                                if (ok)
                                                {
                                                    return (ext0, ta.X, ta.AssertedType);
                                                }

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }

            return ;

        }
    }
}}}}}}

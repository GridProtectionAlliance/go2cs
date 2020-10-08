// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 08 04:56:59 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\func.go
// This file implements the Function and BasicBlock types.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
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
        // addEdge adds a control-flow graph edge from from to to.
        private static void addEdge(ptr<BasicBlock> _addr_from, ptr<BasicBlock> _addr_to)
        {
            ref BasicBlock from = ref _addr_from.val;
            ref BasicBlock to = ref _addr_to.val;

            from.Succs = append(from.Succs, to);
            to.Preds = append(to.Preds, from);
        }

        // Parent returns the function that contains block b.
        private static ptr<Function> Parent(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            return _addr_b.parent!;
        }

        // String returns a human-readable label of this block.
        // It is not guaranteed unique within the function.
        //
        private static @string String(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            return fmt.Sprintf("%d", b.Index);
        }

        // emit appends an instruction to the current basic block.
        // If the instruction defines a Value, it is returned.
        //
        private static Value emit(this ptr<BasicBlock> _addr_b, Instruction i)
        {
            ref BasicBlock b = ref _addr_b.val;

            i.setBlock(b);
            b.Instrs = append(b.Instrs, i);
            Value (v, _) = i._<Value>();
            return v;
        }

        // predIndex returns the i such that b.Preds[i] == c or panics if
        // there is none.
        private static long predIndex(this ptr<BasicBlock> _addr_b, ptr<BasicBlock> _addr_c) => func((_, panic, __) =>
        {
            ref BasicBlock b = ref _addr_b.val;
            ref BasicBlock c = ref _addr_c.val;

            foreach (var (i, pred) in b.Preds)
            {
                if (pred == c)
                {
                    return i;
                }

            }
            panic(fmt.Sprintf("no edge %s -> %s", c, b));

        });

        // hasPhi returns true if b.Instrs contains φ-nodes.
        private static bool hasPhi(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            ptr<Phi> (_, ok) = b.Instrs[0L]._<ptr<Phi>>();
            return ok;
        }

        // phis returns the prefix of b.Instrs containing all the block's φ-nodes.
        private static slice<Instruction> phis(this ptr<BasicBlock> _addr_b)
        {
            ref BasicBlock b = ref _addr_b.val;

            foreach (var (i, instr) in b.Instrs)
            {
                {
                    ptr<Phi> (_, ok) = instr._<ptr<Phi>>();

                    if (!ok)
                    {
                        return b.Instrs[..i];
                    }

                }

            }
            return null; // unreachable in well-formed blocks
        }

        // replacePred replaces all occurrences of p in b's predecessor list with q.
        // Ordinarily there should be at most one.
        //
        private static void replacePred(this ptr<BasicBlock> _addr_b, ptr<BasicBlock> _addr_p, ptr<BasicBlock> _addr_q)
        {
            ref BasicBlock b = ref _addr_b.val;
            ref BasicBlock p = ref _addr_p.val;
            ref BasicBlock q = ref _addr_q.val;

            foreach (var (i, pred) in b.Preds)
            {
                if (pred == p)
                {
                    b.Preds[i] = q;
                }

            }

        }

        // replaceSucc replaces all occurrences of p in b's successor list with q.
        // Ordinarily there should be at most one.
        //
        private static void replaceSucc(this ptr<BasicBlock> _addr_b, ptr<BasicBlock> _addr_p, ptr<BasicBlock> _addr_q)
        {
            ref BasicBlock b = ref _addr_b.val;
            ref BasicBlock p = ref _addr_p.val;
            ref BasicBlock q = ref _addr_q.val;

            foreach (var (i, succ) in b.Succs)
            {
                if (succ == p)
                {
                    b.Succs[i] = q;
                }

            }

        }

        // removePred removes all occurrences of p in b's
        // predecessor list and φ-nodes.
        // Ordinarily there should be at most one.
        //
        private static void removePred(this ptr<BasicBlock> _addr_b, ptr<BasicBlock> _addr_p)
        {
            ref BasicBlock b = ref _addr_b.val;
            ref BasicBlock p = ref _addr_p.val;

            var phis = b.phis(); 

            // We must preserve edge order for φ-nodes.
            long j = 0L;
            {
                var i__prev1 = i;

                foreach (var (__i, __pred) in b.Preds)
                {
                    i = __i;
                    pred = __pred;
                    if (pred != p)
                    {
                        b.Preds[j] = b.Preds[i]; 
                        // Strike out φ-edge too.
                        {
                            var instr__prev2 = instr;

                            foreach (var (_, __instr) in phis)
                            {
                                instr = __instr;
                                ptr<Phi> phi = instr._<ptr<Phi>>();
                                phi.Edges[j] = phi.Edges[i];
                            }

                            instr = instr__prev2;
                        }

                        j++;

                    }

                } 
                // Nil out b.Preds[j:] and φ-edges[j:] to aid GC.

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                for (var i = j; i < len(b.Preds); i++)
                {
                    b.Preds[i] = null;
                    {
                        var instr__prev2 = instr;

                        foreach (var (_, __instr) in phis)
                        {
                            instr = __instr;
                            instr._<ptr<Phi>>().Edges[i] = null;
                        }

                        instr = instr__prev2;
                    }
                }


                i = i__prev1;
            }
            b.Preds = b.Preds[..j];
            {
                var instr__prev1 = instr;

                foreach (var (_, __instr) in phis)
                {
                    instr = __instr;
                    phi = instr._<ptr<Phi>>();
                    phi.Edges = phi.Edges[..j];
                }

                instr = instr__prev1;
            }
        }

        // Destinations associated with unlabelled for/switch/select stmts.
        // We push/pop one of these as we enter/leave each construct and for
        // each BranchStmt we scan for the innermost target of the right type.
        //
        private partial struct targets
        {
            public ptr<targets> tail; // rest of stack
            public ptr<BasicBlock> _break;
            public ptr<BasicBlock> _continue;
            public ptr<BasicBlock> _fallthrough;
        }

        // Destinations associated with a labelled block.
        // We populate these as labels are encountered in forward gotos or
        // labelled statements.
        //
        private partial struct lblock
        {
            public ptr<BasicBlock> _goto;
            public ptr<BasicBlock> _break;
            public ptr<BasicBlock> _continue;
        }

        // labelledBlock returns the branch target associated with the
        // specified label, creating it if needed.
        //
        private static ptr<lblock> labelledBlock(this ptr<Function> _addr_f, ptr<ast.Ident> _addr_label)
        {
            ref Function f = ref _addr_f.val;
            ref ast.Ident label = ref _addr_label.val;

            var lb = f.lblocks[label.Obj];
            if (lb == null)
            {
                lb = addr(new lblock(_goto:f.newBasicBlock(label.Name)));
                if (f.lblocks == null)
                {
                    f.lblocks = make_map<ptr<ast.Object>, ptr<lblock>>();
                }

                f.lblocks[label.Obj] = lb;

            }

            return _addr_lb!;

        }

        // addParam adds a (non-escaping) parameter to f.Params of the
        // specified name, type and source position.
        //
        private static ptr<Parameter> addParam(this ptr<Function> _addr_f, @string name, types.Type typ, token.Pos pos)
        {
            ref Function f = ref _addr_f.val;

            ptr<Parameter> v = addr(new Parameter(name:name,typ:typ,pos:pos,parent:f,));
            f.Params = append(f.Params, v);
            return _addr_v!;
        }

        private static ptr<Parameter> addParamObj(this ptr<Function> _addr_f, types.Object obj)
        {
            ref Function f = ref _addr_f.val;

            var name = obj.Name();
            if (name == "")
            {
                name = fmt.Sprintf("arg%d", len(f.Params));
            }

            var param = f.addParam(name, obj.Type(), obj.Pos());
            param.@object = obj;
            return _addr_param!;

        }

        // addSpilledParam declares a parameter that is pre-spilled to the
        // stack; the function body will load/store the spilled location.
        // Subsequent lifting will eliminate spills where possible.
        //
        private static void addSpilledParam(this ptr<Function> _addr_f, types.Object obj)
        {
            ref Function f = ref _addr_f.val;

            var param = f.addParamObj(obj);
            ptr<Alloc> spill = addr(new Alloc(Comment:obj.Name()));
            spill.setType(types.NewPointer(obj.Type()));
            spill.setPos(obj.Pos());
            f.objects[obj] = spill;
            f.Locals = append(f.Locals, spill);
            f.emit(spill);
            f.emit(addr(new Store(Addr:spill,Val:param)));
        }

        // startBody initializes the function prior to generating SSA code for its body.
        // Precondition: f.Type() already set.
        //
        private static void startBody(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            f.currentBlock = f.newBasicBlock("entry");
            f.objects = make_map<types.Object, Value>(); // needed for some synthetics, e.g. init
        }

        // createSyntacticParams populates f.Params and generates code (spills
        // and named result locals) for all the parameters declared in the
        // syntax.  In addition it populates the f.objects mapping.
        //
        // Preconditions:
        // f.startBody() was called.
        // Postcondition:
        // len(f.Params) == len(f.Signature.Params) + (f.Signature.Recv() ? 1 : 0)
        //
        private static void createSyntacticParams(this ptr<Function> _addr_f, ptr<ast.FieldList> _addr_recv, ptr<ast.FuncType> _addr_functype)
        {
            ref Function f = ref _addr_f.val;
            ref ast.FieldList recv = ref _addr_recv.val;
            ref ast.FuncType functype = ref _addr_functype.val;
 
            // Receiver (at most one inner iteration).
            if (recv != null)
            {
                {
                    var field__prev1 = field;

                    foreach (var (_, __field) in recv.List)
                    {
                        field = __field;
                        {
                            var n__prev2 = n;

                            foreach (var (_, __n) in field.Names)
                            {
                                n = __n;
                                f.addSpilledParam(f.Pkg.info.Defs[n]);
                            } 
                            // Anonymous receiver?  No need to spill.

                            n = n__prev2;
                        }

                        if (field.Names == null)
                        {
                            f.addParamObj(f.Signature.Recv());
                        }

                    }

                    field = field__prev1;
                }
            } 

            // Parameters.
            if (functype.Params != null)
            {
                var n = len(f.Params); // 1 if has recv, 0 otherwise
                {
                    var field__prev1 = field;

                    foreach (var (_, __field) in functype.Params.List)
                    {
                        field = __field;
                        {
                            var n__prev2 = n;

                            foreach (var (_, __n) in field.Names)
                            {
                                n = __n;
                                f.addSpilledParam(f.Pkg.info.Defs[n]);
                            } 
                            // Anonymous parameter?  No need to spill.

                            n = n__prev2;
                        }

                        if (field.Names == null)
                        {
                            f.addParamObj(f.Signature.Params().At(len(f.Params) - n));
                        }

                    }

                    field = field__prev1;
                }
            } 

            // Named results.
            if (functype.Results != null)
            {
                {
                    var field__prev1 = field;

                    foreach (var (_, __field) in functype.Results.List)
                    {
                        field = __field; 
                        // Implicit "var" decl of locals for named results.
                        {
                            var n__prev2 = n;

                            foreach (var (_, __n) in field.Names)
                            {
                                n = __n;
                                f.namedResults = append(f.namedResults, f.addLocalForIdent(n));
                            }

                            n = n__prev2;
                        }
                    }

                    field = field__prev1;
                }
            }

        }

        private partial interface setNumable
        {
            void setNum(long _p0);
        }

        // numberRegisters assigns numbers to all SSA registers
        // (value-defining Instructions) in f, to aid debugging.
        // (Non-Instruction Values are named at construction.)
        //
        private static void numberRegisters(ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            long v = 0L;
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, instr) in b.Instrs)
                {
                    switch (instr.type())
                    {
                        case Value _:
                            instr._<setNumable>().setNum(v);
                            v++;
                            break;
                    }

                }

            }

        }

        // buildReferrers populates the def/use information in all non-nil
        // Value.Referrers slice.
        // Precondition: all such slices are initially empty.
        private static void buildReferrers(ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            slice<ptr<Value>> rands = default;
            foreach (var (_, b) in f.Blocks)
            {
                foreach (var (_, instr) in b.Instrs)
                {
                    rands = instr.Operands(rands[..0L]); // recycle storage
                    foreach (var (_, rand) in rands)
                    {
                        {
                            var r = rand.val;

                            if (r != null)
                            {
                                {
                                    var @ref = r.Referrers();

                                    if (ref != null)
                                    {
                                        ref.val = append(ref.val, instr);
                                    }

                                }

                            }

                        }

                    }

                }

            }

        }

        // finishBody() finalizes the function after SSA code generation of its body.
        private static void finishBody(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            f.objects = null;
            f.currentBlock = null;
            f.lblocks = null; 

            // Don't pin the AST in memory (except in debug mode).
            {
                var n = f.syntax;

                if (n != null && !f.debugInfo())
                {
                    f.syntax = new extentNode(n.Pos(),n.End());
                } 

                // Remove from f.Locals any Allocs that escape to the heap.

            } 

            // Remove from f.Locals any Allocs that escape to the heap.
            long j = 0L;
            foreach (var (_, l) in f.Locals)
            {
                if (!l.Heap)
                {
                    f.Locals[j] = l;
                    j++;
                }

            } 
            // Nil out f.Locals[j:] to aid GC.
            for (var i = j; i < len(f.Locals); i++)
            {
                f.Locals[i] = null;
            }

            f.Locals = f.Locals[..j];

            optimizeBlocks(f);

            buildReferrers(_addr_f);

            buildDomTree(f);

            if (f.Prog.mode & NaiveForm == 0L)
            { 
                // For debugging pre-state of lifting pass:
                // numberRegisters(f)
                // f.WriteTo(os.Stderr)
                lift(f);

            }

            f.namedResults = null; // (used by lifting)

            numberRegisters(_addr_f);

            if (f.Prog.mode & PrintFunctions != 0L)
            {
                printMu.Lock();
                f.WriteTo(os.Stdout);
                printMu.Unlock();
            }

            if (f.Prog.mode & SanityCheckFunctions != 0L)
            {
                mustSanityCheck(f, null);
            }

        }

        // removeNilBlocks eliminates nils from f.Blocks and updates each
        // BasicBlock.Index.  Use this after any pass that may delete blocks.
        //
        private static void removeNilBlocks(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            long j = 0L;
            foreach (var (_, b) in f.Blocks)
            {
                if (b != null)
                {
                    b.Index = j;
                    f.Blocks[j] = b;
                    j++;
                }

            } 
            // Nil out f.Blocks[j:] to aid GC.
            for (var i = j; i < len(f.Blocks); i++)
            {
                f.Blocks[i] = null;
            }

            f.Blocks = f.Blocks[..j];

        }

        // SetDebugMode sets the debug mode for package pkg.  If true, all its
        // functions will include full debug info.  This greatly increases the
        // size of the instruction stream, and causes Functions to depend upon
        // the ASTs, potentially keeping them live in memory for longer.
        //
        private static void SetDebugMode(this ptr<Package> _addr_pkg, bool debug)
        {
            ref Package pkg = ref _addr_pkg.val;
 
            // TODO(adonovan): do we want ast.File granularity?
            pkg.debug = debug;

        }

        // debugInfo reports whether debug info is wanted for this function.
        private static bool debugInfo(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            return f.Pkg != null && f.Pkg.debug;
        }

        // addNamedLocal creates a local variable, adds it to function f and
        // returns it.  Its name and type are taken from obj.  Subsequent
        // calls to f.lookup(obj) will return the same local.
        //
        private static ptr<Alloc> addNamedLocal(this ptr<Function> _addr_f, types.Object obj)
        {
            ref Function f = ref _addr_f.val;

            var l = f.addLocal(obj.Type(), obj.Pos());
            l.Comment = obj.Name();
            f.objects[obj] = l;
            return _addr_l!;
        }

        private static ptr<Alloc> addLocalForIdent(this ptr<Function> _addr_f, ptr<ast.Ident> _addr_id)
        {
            ref Function f = ref _addr_f.val;
            ref ast.Ident id = ref _addr_id.val;

            return _addr_f.addNamedLocal(f.Pkg.info.Defs[id])!;
        }

        // addLocal creates an anonymous local variable of type typ, adds it
        // to function f and returns it.  pos is the optional source location.
        //
        private static ptr<Alloc> addLocal(this ptr<Function> _addr_f, types.Type typ, token.Pos pos)
        {
            ref Function f = ref _addr_f.val;

            ptr<Alloc> v = addr(new Alloc());
            v.setType(types.NewPointer(typ));
            v.setPos(pos);
            f.Locals = append(f.Locals, v);
            f.emit(v);
            return _addr_v!;
        }

        // lookup returns the address of the named variable identified by obj
        // that is local to function f or one of its enclosing functions.
        // If escaping, the reference comes from a potentially escaping pointer
        // expression and the referent must be heap-allocated.
        //
        private static Value lookup(this ptr<Function> _addr_f, types.Object obj, bool escaping) => func((_, panic, __) =>
        {
            ref Function f = ref _addr_f.val;

            {
                var v__prev1 = v;

                var (v, ok) = f.objects[obj];

                if (ok)
                {
                    {
                        ptr<Alloc> (alloc, ok) = v._<ptr<Alloc>>();

                        if (ok && escaping)
                        {
                            alloc.Heap = true;
                        }

                    }

                    return v; // function-local var (address)
                } 

                // Definition must be in an enclosing function;
                // plumb it through intervening closures.

                v = v__prev1;

            } 

            // Definition must be in an enclosing function;
            // plumb it through intervening closures.
            if (f.parent == null)
            {
                panic("no ssa.Value for " + obj.String());
            }

            var outer = f.parent.lookup(obj, true); // escaping
            ptr<FreeVar> v = addr(new FreeVar(name:obj.Name(),typ:outer.Type(),pos:outer.Pos(),outer:outer,parent:f,));
            f.objects[obj] = v;
            f.FreeVars = append(f.FreeVars, v);
            return v;

        });

        // emit emits the specified instruction to function f.
        private static Value emit(this ptr<Function> _addr_f, Instruction instr)
        {
            ref Function f = ref _addr_f.val;

            return f.currentBlock.emit(instr);
        }

        // RelString returns the full name of this function, qualified by
        // package name, receiver type, etc.
        //
        // The specific formatting rules are not guaranteed and may change.
        //
        // Examples:
        //      "math.IsNaN"                  // a package-level function
        //      "(*bytes.Buffer).Bytes"       // a declared method or a wrapper
        //      "(*bytes.Buffer).Bytes$thunk" // thunk (func wrapping method; receiver is param 0)
        //      "(*bytes.Buffer).Bytes$bound" // bound (func wrapping method; receiver supplied by closure)
        //      "main.main$1"                 // an anonymous function in main
        //      "main.init#1"                 // a declared init function
        //      "main.init"                   // the synthesized package initializer
        //
        // When these functions are referred to from within the same package
        // (i.e. from == f.Pkg.Object), they are rendered without the package path.
        // For example: "IsNaN", "(*Buffer).Bytes", etc.
        //
        // All non-synthetic functions have distinct package-qualified names.
        // (But two methods may have the same name "(T).f" if one is a synthetic
        // wrapper promoting a non-exported method "f" from another package; in
        // that case, the strings are equal but the identifiers "f" are distinct.)
        //
        private static @string RelString(this ptr<Function> _addr_f, ptr<types.Package> _addr_from)
        {
            ref Function f = ref _addr_f.val;
            ref types.Package from = ref _addr_from.val;
 
            // Anonymous?
            if (f.parent != null)
            { 
                // An anonymous function's Name() looks like "parentName$1",
                // but its String() should include the type/package/etc.
                var parent = f.parent.RelString(from);
                foreach (var (i, anon) in f.parent.AnonFuncs)
                {
                    if (anon == f)
                    {
                        return fmt.Sprintf("%s$%d", parent, 1L + i);
                    }

                }
                return f.name; // should never happen
            } 

            // Method (declared or wrapper)?
            {
                var recv = f.Signature.Recv();

                if (recv != null)
                {
                    return f.relMethod(from, recv.Type());
                } 

                // Thunk?

            } 

            // Thunk?
            if (f.method != null)
            {
                return f.relMethod(from, f.method.Recv());
            } 

            // Bound?
            if (len(f.FreeVars) == 1L && strings.HasSuffix(f.name, "$bound"))
            {
                return f.relMethod(from, f.FreeVars[0L].Type());
            } 

            // Package-level function?
            // Prefix with package name for cross-package references only.
            {
                var p = f.pkg();

                if (p != null && p != from)
                {
                    return fmt.Sprintf("%s.%s", p.Path(), f.name);
                } 

                // Unknown.

            } 

            // Unknown.
            return f.name;

        }

        private static @string relMethod(this ptr<Function> _addr_f, ptr<types.Package> _addr_from, types.Type recv)
        {
            ref Function f = ref _addr_f.val;
            ref types.Package from = ref _addr_from.val;

            return fmt.Sprintf("(%s).%s", relType(recv, from), f.name);
        }

        // writeSignature writes to buf the signature sig in declaration syntax.
        private static void writeSignature(ptr<bytes.Buffer> _addr_buf, ptr<types.Package> _addr_from, @string name, ptr<types.Signature> _addr_sig, slice<ptr<Parameter>> @params)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;
            ref types.Package from = ref _addr_from.val;
            ref types.Signature sig = ref _addr_sig.val;

            buf.WriteString("func ");
            {
                var recv = sig.Recv();

                if (recv != null)
                {
                    buf.WriteString("(");
                    {
                        var n = params[0L].Name();

                        if (n != "")
                        {
                            buf.WriteString(n);
                            buf.WriteString(" ");
                        }

                    }

                    types.WriteType(buf, params[0L].Type(), types.RelativeTo(from));
                    buf.WriteString(") ");

                }

            }

            buf.WriteString(name);
            types.WriteSignature(buf, sig, types.RelativeTo(from));

        }

        private static ptr<types.Package> pkg(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            if (f.Pkg != null)
            {
                return _addr_f.Pkg.Pkg!;
            }

            return _addr_null!;

        }

        private static io.WriterTo _ = (Function.val)(null); // *Function implements io.Writer

        private static (long, error) WriteTo(this ptr<Function> _addr_f, io.Writer w)
        {
            long _p0 = default;
            error _p0 = default!;
            ref Function f = ref _addr_f.val;

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            WriteFunction(_addr_buf, _addr_f);
            var (n, err) = w.Write(buf.Bytes());
            return (int64(n), error.As(err)!);
        }

        // WriteFunction writes to buf a human-readable "disassembly" of f.
        public static void WriteFunction(ptr<bytes.Buffer> _addr_buf, ptr<Function> _addr_f)
        {
            ref bytes.Buffer buf = ref _addr_buf.val;
            ref Function f = ref _addr_f.val;

            fmt.Fprintf(buf, "# Name: %s\n", f.String());
            if (f.Pkg != null)
            {
                fmt.Fprintf(buf, "# Package: %s\n", f.Pkg.Pkg.Path());
            }

            {
                var syn = f.Synthetic;

                if (syn != "")
                {
                    fmt.Fprintln(buf, "# Synthetic:", syn);
                }

            }

            {
                var pos = f.Pos();

                if (pos.IsValid())
                {
                    fmt.Fprintf(buf, "# Location: %s\n", f.Prog.Fset.Position(pos));
                }

            }


            if (f.parent != null)
            {
                fmt.Fprintf(buf, "# Parent: %s\n", f.parent.Name());
            }

            if (f.Recover != null)
            {
                fmt.Fprintf(buf, "# Recover: %s\n", f.Recover);
            }

            var from = f.pkg();

            if (f.FreeVars != null)
            {
                buf.WriteString("# Free variables:\n");
                {
                    var i__prev1 = i;

                    foreach (var (__i, __fv) in f.FreeVars)
                    {
                        i = __i;
                        fv = __fv;
                        fmt.Fprintf(buf, "# % 3d:\t%s %s\n", i, fv.Name(), relType(fv.Type(), from));
                    }

                    i = i__prev1;
                }
            }

            if (len(f.Locals) > 0L)
            {
                buf.WriteString("# Locals:\n");
                {
                    var i__prev1 = i;
                    var l__prev1 = l;

                    foreach (var (__i, __l) in f.Locals)
                    {
                        i = __i;
                        l = __l;
                        fmt.Fprintf(buf, "# % 3d:\t%s %s\n", i, l.Name(), relType(deref(l.Type()), from));
                    }

                    i = i__prev1;
                    l = l__prev1;
                }
            }

            writeSignature(_addr_buf, _addr_from, f.Name(), _addr_f.Signature, f.Params);
            buf.WriteString(":\n");

            if (f.Blocks == null)
            {
                buf.WriteString("\t(external)\n");
            } 

            // NB. column calculations are confused by non-ASCII
            // characters and assume 8-space tabs.
            const long punchcard = (long)80L; // for old time's sake.
 // for old time's sake.
            const long tabwidth = (long)8L;

            foreach (var (_, b) in f.Blocks)
            {
                if (b == null)
                { 
                    // Corrupt CFG.
                    fmt.Fprintf(buf, ".nil:\n");
                    continue;

                }

                var (n, _) = fmt.Fprintf(buf, "%d:", b.Index);
                var bmsg = fmt.Sprintf("%s P:%d S:%d", b.Comment, len(b.Preds), len(b.Succs));
                fmt.Fprintf(buf, "%*s%s\n", punchcard - 1L - n - len(bmsg), "", bmsg);

                if (false)
                { // CFG debugging
                    fmt.Fprintf(buf, "\t# CFG: %s --> %s --> %s\n", b.Preds, b, b.Succs);

                }

                foreach (var (_, instr) in b.Instrs)
                {
                    buf.WriteString("\t");
                    switch (instr.type())
                    {
                        case Value v:
                            var l = punchcard - tabwidth; 
                            // Left-align the instruction.
                            {
                                var name = v.Name();

                                if (name != "")
                                {
                                    (n, _) = fmt.Fprintf(buf, "%s = ", name);
                                    l -= n;
                                }

                            }

                            (n, _) = buf.WriteString(instr.String());
                            l -= n; 
                            // Right-align the type if there's space.
                            {
                                var t = v.Type();

                                if (t != null)
                                {
                                    buf.WriteByte(' ');
                                    var ts = relType(t, from);
                                    l -= len(ts) + len("  "); // (spaces before and after type)
                                    if (l > 0L)
                                    {
                                        fmt.Fprintf(buf, "%*s", l, "");
                                    }

                                    buf.WriteString(ts);

                                }

                            }

                            break;
                        case 
                            buf.WriteString("<deleted>");
                            break;
                        default:
                        {
                            var v = instr.type();
                            buf.WriteString(instr.String());
                            break;
                        }
                    }
                    buf.WriteString("\n");

                }

            }
            fmt.Fprintf(buf, "\n");

        }

        // newBasicBlock adds to f a new basic block and returns it.  It does
        // not automatically become the current block for subsequent calls to emit.
        // comment is an optional string for more readable debugging output.
        //
        private static ptr<BasicBlock> newBasicBlock(this ptr<Function> _addr_f, @string comment)
        {
            ref Function f = ref _addr_f.val;

            ptr<BasicBlock> b = addr(new BasicBlock(Index:len(f.Blocks),Comment:comment,parent:f,));
            b.Succs = b.succs2[..0L];
            f.Blocks = append(f.Blocks, b);
            return _addr_b!;
        }

        // NewFunction returns a new synthetic Function instance belonging to
        // prog, with its name and signature fields set as specified.
        //
        // The caller is responsible for initializing the remaining fields of
        // the function object, e.g. Pkg, Params, Blocks.
        //
        // It is practically impossible for clients to construct well-formed
        // SSA functions/packages/programs directly, so we assume this is the
        // job of the Builder alone.  NewFunction exists to provide clients a
        // little flexibility.  For example, analysis tools may wish to
        // construct fake Functions for the root of the callgraph, a fake
        // "reflect" package, etc.
        //
        // TODO(adonovan): think harder about the API here.
        //
        private static ptr<Function> NewFunction(this ptr<Program> _addr_prog, @string name, ptr<types.Signature> _addr_sig, @string provenance)
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Signature sig = ref _addr_sig.val;

            return addr(new Function(Prog:prog,name:name,Signature:sig,Synthetic:provenance));
        }

        private partial struct extentNode // : array<token.Pos>
        {
        }

        private static token.Pos Pos(this extentNode n)
        {
            return n[0L];
        }
        private static token.Pos End(this extentNode n)
        {
            return n[1L];
        }

        // Syntax returns an ast.Node whose Pos/End methods provide the
        // lexical extent of the function if it was defined by Go source code
        // (f.Synthetic==""), or nil otherwise.
        //
        // If f was built with debug information (see Package.SetDebugRef),
        // the result is the *ast.FuncDecl or *ast.FuncLit that declared the
        // function.  Otherwise, it is an opaque Node providing only position
        // information; this avoids pinning the AST in memory.
        //
        private static ast.Node Syntax(this ptr<Function> _addr_f)
        {
            ref Function f = ref _addr_f.val;

            return f.syntax;
        }
    }
}}}}}

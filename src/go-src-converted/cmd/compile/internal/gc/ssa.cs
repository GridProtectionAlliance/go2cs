// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:29:10 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\ssa.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using html = go.html_package;
using os = go.os_package;
using sort = go.sort_package;

using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        private static ref ssa.Config ssaConfig = default;
        private static slice<ssa.Cache> ssaCaches = default;

        private static void initssaconfig()
        {
            ssa.Types types_ = new ssa.Types(Bool:types.Types[TBOOL],Int8:types.Types[TINT8],Int16:types.Types[TINT16],Int32:types.Types[TINT32],Int64:types.Types[TINT64],UInt8:types.Types[TUINT8],UInt16:types.Types[TUINT16],UInt32:types.Types[TUINT32],UInt64:types.Types[TUINT64],Float32:types.Types[TFLOAT32],Float64:types.Types[TFLOAT64],Int:types.Types[TINT],UInt:types.Types[TUINT],Uintptr:types.Types[TUINTPTR],String:types.Types[TSTRING],BytePtr:types.NewPtr(types.Types[TUINT8]),Int32Ptr:types.NewPtr(types.Types[TINT32]),UInt32Ptr:types.NewPtr(types.Types[TUINT32]),IntPtr:types.NewPtr(types.Types[TINT]),UintptrPtr:types.NewPtr(types.Types[TUINTPTR]),Float32Ptr:types.NewPtr(types.Types[TFLOAT32]),Float64Ptr:types.NewPtr(types.Types[TFLOAT64]),BytePtrPtr:types.NewPtr(types.NewPtr(types.Types[TUINT8])),);

            if (thearch.SoftFloat)
            {
                softfloatInit();
            } 

            // Generate a few pointer types that are uncommon in the frontend but common in the backend.
            // Caching is disabled in the backend, so generating these here avoids allocations.
            _ = types.NewPtr(types.Types[TINTER]); // *interface{}
            _ = types.NewPtr(types.NewPtr(types.Types[TSTRING])); // **string
            _ = types.NewPtr(types.NewPtr(types.Idealstring)); // **string
            _ = types.NewPtr(types.NewSlice(types.Types[TINTER])); // *[]interface{}
            _ = types.NewPtr(types.NewPtr(types.Bytetype)); // **byte
            _ = types.NewPtr(types.NewSlice(types.Bytetype)); // *[]byte
            _ = types.NewPtr(types.NewSlice(types.Types[TSTRING])); // *[]string
            _ = types.NewPtr(types.NewSlice(types.Idealstring)); // *[]string
            _ = types.NewPtr(types.NewPtr(types.NewPtr(types.Types[TUINT8]))); // ***uint8
            _ = types.NewPtr(types.Types[TINT16]); // *int16
            _ = types.NewPtr(types.Types[TINT64]); // *int64
            _ = types.NewPtr(types.Errortype); // *error
            types.NewPtrCacheEnabled = false;
            ssaConfig = ssa.NewConfig(thearch.LinkArch.Name, types_, Ctxt, Debug['N'] == 0L);
            if (thearch.LinkArch.Name == "386")
            {
                ssaConfig.Set387(thearch.Use387);
            }
            ssaConfig.SoftFloat = thearch.SoftFloat;
            ssaCaches = make_slice<ssa.Cache>(nBackendWorkers); 

            // Set up some runtime functions we'll need to call.
            Newproc = sysfunc("newproc");
            Deferproc = sysfunc("deferproc");
            Deferreturn = sysfunc("deferreturn");
            Duffcopy = sysfunc("duffcopy");
            Duffzero = sysfunc("duffzero");
            panicindex = sysfunc("panicindex");
            panicslice = sysfunc("panicslice");
            panicdivide = sysfunc("panicdivide");
            growslice = sysfunc("growslice");
            panicdottypeE = sysfunc("panicdottypeE");
            panicdottypeI = sysfunc("panicdottypeI");
            panicnildottype = sysfunc("panicnildottype");
            assertE2I = sysfunc("assertE2I");
            assertE2I2 = sysfunc("assertE2I2");
            assertI2I = sysfunc("assertI2I");
            assertI2I2 = sysfunc("assertI2I2");
            goschedguarded = sysfunc("goschedguarded");
            writeBarrier = sysfunc("writeBarrier");
            writebarrierptr = sysfunc("writebarrierptr");
            gcWriteBarrier = sysfunc("gcWriteBarrier");
            typedmemmove = sysfunc("typedmemmove");
            typedmemclr = sysfunc("typedmemclr");
            Udiv = sysfunc("udiv"); 

            // GO386=387 runtime functions
            ControlWord64trunc = sysfunc("controlWord64trunc");
            ControlWord32 = sysfunc("controlWord32");
        }

        // buildssa builds an SSA function for fn.
        // worker indicates which of the backend workers is doing the processing.
        private static ref ssa.Func buildssa(ref Node _fn, long worker) => func(_fn, (ref Node fn, Defer defer, Panic _, Recover __) =>
        {
            var name = fn.funcname();
            var printssa = name == os.Getenv("GOSSAFUNC");
            if (printssa)
            {
                fmt.Println("generating SSA for", name);
                dumplist("buildssa-enter", fn.Func.Enter);
                dumplist("buildssa-body", fn.Nbody);
                dumplist("buildssa-exit", fn.Func.Exit);
            }
            state s = default;
            s.pushLine(fn.Pos);
            defer(s.popLine());

            s.hasdefer = fn.Func.HasDefer();
            if (fn.Func.Pragma & CgoUnsafeArgs != 0L)
            {
                s.cgoUnsafeArgs = true;
            }
            ssafn fe = new ssafn(curfn:fn,log:printssa,);
            s.curfn = fn;

            s.f = ssa.NewFunc(ref fe);
            s.config = ssaConfig;
            s.f.Config = ssaConfig;
            s.f.Cache = ref ssaCaches[worker];
            s.f.Cache.Reset();
            s.f.DebugTest = s.f.DebugHashMatch("GOSSAHASH", name);
            s.f.Name = name;
            if (fn.Func.Pragma & Nosplit != 0L)
            {
                s.f.NoSplit = true;
            }
            s.exitCode = fn.Func.Exit;
            s.panics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<funcLine, ref ssa.Block>{};
            s.softFloat = s.config.SoftFloat;

            if (name == os.Getenv("GOSSAFUNC"))
            {
                s.f.HTMLWriter = ssa.NewHTMLWriter("ssa.html", s.f.Frontend(), name); 
                // TODO: generate and print a mapping from nodes to values and blocks
            } 

            // Allocate starting block
            s.f.Entry = s.f.NewBlock(ssa.BlockPlain); 

            // Allocate starting values
            s.labels = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, ref ssaLabel>{};
            s.labeledNodes = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref ssaLabel>{};
            s.fwdVars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref ssa.Value>{};
            s.startmem = s.entryNewValue0(ssa.OpInitMem, types.TypeMem);
            s.sp = s.entryNewValue0(ssa.OpSP, types.Types[TUINTPTR]); // TODO: use generic pointer type (unsafe.Pointer?) instead
            s.sb = s.entryNewValue0(ssa.OpSB, types.Types[TUINTPTR]);

            s.startBlock(s.f.Entry);
            s.vars[ref memVar] = s.startmem; 

            // Generate addresses of local declarations
            s.decladdrs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref ssa.Value>{};
            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;

                    if (n.Class() == PPARAM || n.Class() == PPARAMOUT) 
                        s.decladdrs[n] = s.entryNewValue1A(ssa.OpAddr, types.NewPtr(n.Type), n, s.sp);
                        if (n.Class() == PPARAMOUT && s.canSSA(n))
                        { 
                            // Save ssa-able PPARAMOUT variables so we can
                            // store them back to the stack at the end of
                            // the function.
                            s.returns = append(s.returns, n);
                        }
                    else if (n.Class() == PAUTO)                     else if (n.Class() == PAUTOHEAP)                     else if (n.Class() == PFUNC)                     else 
                        s.Fatalf("local variable with class %v unimplemented", n.Class());
                                    } 

                // Populate SSAable arguments.

                n = n__prev1;
            }

            {
                var n__prev1 = n;

                foreach (var (_, __n) in fn.Func.Dcl)
                {
                    n = __n;
                    if (n.Class() == PPARAM && s.canSSA(n))
                    {
                        s.vars[n] = s.newValue0A(ssa.OpArg, n.Type, n);
                    }
                } 

                // Convert the AST-based IR to the SSA-based IR

                n = n__prev1;
            }

            s.stmtList(fn.Func.Enter);
            s.stmtList(fn.Nbody); 

            // fallthrough to exit
            if (s.curBlock != null)
            {
                s.pushLine(fn.Func.Endlineno);
                s.exit();
                s.popLine();
            }
            foreach (var (_, b) in s.f.Blocks)
            {
                if (b.Pos != src.NoXPos)
                {
                    s.updateUnsetPredPos(b);
                }
            }
            s.insertPhis(); 

            // Don't carry reference this around longer than necessary
            s.exitCode = new Nodes(); 

            // Main call to ssa package to compile function
            ssa.Compile(s.f);
            return s.f;
        });

        // updateUnsetPredPos propagates the earliest-value position information for b
        // towards all of b's predecessors that need a position, and recurs on that
        // predecessor if its position is updated. B should have a non-empty position.
        private static void updateUnsetPredPos(this ref state s, ref ssa.Block b)
        {
            if (b.Pos == src.NoXPos)
            {
                s.Fatalf("Block %s should have a position", b);
            }
            var bestPos = src.NoXPos;
            foreach (var (_, e) in b.Preds)
            {
                var p = e.Block();
                if (!p.LackingPos())
                {
                    continue;
                }
                if (bestPos == src.NoXPos)
                {
                    bestPos = b.Pos;
                    foreach (var (_, v) in b.Values)
                    {
                        if (v.LackingPos())
                        {
                            continue;
                        }
                        if (v.Pos != src.NoXPos)
                        { 
                            // Assume values are still in roughly textual order;
                            // TODO: could also seek minimum position?
                            bestPos = v.Pos;
                            break;
                        }
                    }
                }
                p.Pos = bestPos;
                s.updateUnsetPredPos(p); // We do not expect long chains of these, thus recursion is okay.
            }
            return;
        }

        private partial struct state
        {
            public ptr<ssa.Config> config; // function we're building
            public ptr<ssa.Func> f; // Node for function
            public ptr<Node> curfn; // labels and labeled control flow nodes (OFOR, OFORUNTIL, OSWITCH, OSELECT) in f
            public map<@string, ref ssaLabel> labels;
            public map<ref Node, ref ssaLabel> labeledNodes; // Code that must precede any return
// (e.g., copying value of heap-escaped paramout back to true paramout)
            public Nodes exitCode; // unlabeled break and continue statement tracking
            public ptr<ssa.Block> breakTo; // current target for plain break statement
            public ptr<ssa.Block> continueTo; // current target for plain continue statement

// current location where we're interpreting the AST
            public ptr<ssa.Block> curBlock; // variable assignments in the current block (map from variable symbol to ssa value)
// *Node is the unique identifier (an ONAME Node) for the variable.
// TODO: keep a single varnum map, then make all of these maps slices instead?
            public map<ref Node, ref ssa.Value> vars; // fwdVars are variables that are used before they are defined in the current block.
// This map exists just to coalesce multiple references into a single FwdRef op.
// *Node is the unique identifier (an ONAME Node) for the variable.
            public map<ref Node, ref ssa.Value> fwdVars; // all defined variables at the end of each block. Indexed by block ID.
            public slice<map<ref Node, ref ssa.Value>> defvars; // addresses of PPARAM and PPARAMOUT variables.
            public map<ref Node, ref ssa.Value> decladdrs; // starting values. Memory, stack pointer, and globals pointer
            public ptr<ssa.Value> startmem;
            public ptr<ssa.Value> sp;
            public ptr<ssa.Value> sb; // line number stack. The current line number is top of stack
            public slice<src.XPos> line; // the last line number processed; it may have been popped
            public src.XPos lastPos; // list of panic calls by function name and line number.
// Used to deduplicate panic calls.
            public map<funcLine, ref ssa.Block> panics; // list of PPARAMOUT (return) variables.
            public slice<ref Node> returns;
            public bool cgoUnsafeArgs;
            public bool hasdefer; // whether the function contains a defer statement
            public bool softFloat;
        }

        private partial struct funcLine
        {
            public ptr<obj.LSym> f;
            public ptr<src.PosBase> @base;
            public ulong line;
        }

        private partial struct ssaLabel
        {
            public ptr<ssa.Block> target; // block identified by this label
            public ptr<ssa.Block> breakTarget; // block to break to in control flow node identified by this label
            public ptr<ssa.Block> continueTarget; // block to continue to in control flow node identified by this label
        }

        // label returns the label associated with sym, creating it if necessary.
        private static ref ssaLabel label(this ref state s, ref types.Sym sym)
        {
            var lab = s.labels[sym.Name];
            if (lab == null)
            {
                lab = @new<ssaLabel>();
                s.labels[sym.Name] = lab;
            }
            return lab;
        }

        private static void Logf(this ref state s, @string msg, params object[] args)
        {
            s.f.Logf(msg, args);

        }
        private static bool Log(this ref state s)
        {
            return s.f.Log();
        }
        private static void Fatalf(this ref state s, @string msg, params object[] args)
        {
            s.f.Frontend().Fatalf(s.peekPos(), msg, args);
        }
        private static void Warnl(this ref state s, src.XPos pos, @string msg, params object[] args)
        {
            s.f.Warnl(pos, msg, args);

        }
        private static bool Debug_checknil(this ref state s)
        {
            return s.f.Frontend().Debug_checknil();
        }

 
        // dummy node for the memory variable
        private static Node memVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"mem"});        private static Node ptrVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"ptr"});        private static Node lenVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"len"});        private static Node newlenVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"newlen"});        private static Node capVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"cap"});        private static Node typVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"typ"});        private static Node okVar = new Node(Op:ONAME,Sym:&types.Sym{Name:"ok"});

        // startBlock sets the current block we're generating code in to b.
        private static void startBlock(this ref state s, ref ssa.Block b)
        {
            if (s.curBlock != null)
            {
                s.Fatalf("starting block %v when block %v has not ended", b, s.curBlock);
            }
            s.curBlock = b;
            s.vars = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref ssa.Value>{};
            foreach (var (n) in s.fwdVars)
            {
                delete(s.fwdVars, n);
            }
        }

        // endBlock marks the end of generating code for the current block.
        // Returns the (former) current block. Returns nil if there is no current
        // block, i.e. if no code flows to the current execution point.
        private static ref ssa.Block endBlock(this ref state s)
        {
            var b = s.curBlock;
            if (b == null)
            {
                return null;
            }
            while (len(s.defvars) <= int(b.ID))
            {
                s.defvars = append(s.defvars, null);
            }

            s.defvars[b.ID] = s.vars;
            s.curBlock = null;
            s.vars = null;
            if (b.LackingPos())
            { 
                // Empty plain blocks get the line of their successor (handled after all blocks created),
                // except for increment blocks in For statements (handled in ssa conversion of OFOR),
                // and for blocks ending in GOTO/BREAK/CONTINUE.
                b.Pos = src.NoXPos;
            }
            else
            {
                b.Pos = s.lastPos;
            }
            return b;
        }

        // pushLine pushes a line number on the line number stack.
        private static void pushLine(this ref state s, src.XPos line)
        {
            if (!line.IsKnown())
            { 
                // the frontend may emit node with line number missing,
                // use the parent line number in this case.
                line = s.peekPos();
                if (Debug['K'] != 0L)
                {
                    Warn("buildssa: unknown position (line 0)");
                }
            }
            else
            {
                s.lastPos = line;
            }
            s.line = append(s.line, line);
        }

        // popLine pops the top of the line number stack.
        private static void popLine(this ref state s)
        {
            s.line = s.line[..len(s.line) - 1L];
        }

        // peekPos peeks the top of the line number stack.
        private static src.XPos peekPos(this ref state s)
        {
            return s.line[len(s.line) - 1L];
        }

        // newValue0 adds a new value with no arguments to the current block.
        private static ref ssa.Value newValue0(this ref state s, ssa.Op op, ref types.Type t)
        {
            return s.curBlock.NewValue0(s.peekPos(), op, t);
        }

        // newValue0A adds a new value with no arguments and an aux value to the current block.
        private static ref ssa.Value newValue0A(this ref state s, ssa.Op op, ref types.Type t, object aux)
        {
            return s.curBlock.NewValue0A(s.peekPos(), op, t, aux);
        }

        // newValue0I adds a new value with no arguments and an auxint value to the current block.
        private static ref ssa.Value newValue0I(this ref state s, ssa.Op op, ref types.Type t, long auxint)
        {
            return s.curBlock.NewValue0I(s.peekPos(), op, t, auxint);
        }

        // newValue1 adds a new value with one argument to the current block.
        private static ref ssa.Value newValue1(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg)
        {
            return s.curBlock.NewValue1(s.peekPos(), op, t, arg);
        }

        // newValue1A adds a new value with one argument and an aux value to the current block.
        private static ref ssa.Value newValue1A(this ref state s, ssa.Op op, ref types.Type t, object aux, ref ssa.Value arg)
        {
            return s.curBlock.NewValue1A(s.peekPos(), op, t, aux, arg);
        }

        // newValue1I adds a new value with one argument and an auxint value to the current block.
        private static ref ssa.Value newValue1I(this ref state s, ssa.Op op, ref types.Type t, long aux, ref ssa.Value arg)
        {
            return s.curBlock.NewValue1I(s.peekPos(), op, t, aux, arg);
        }

        // newValue2 adds a new value with two arguments to the current block.
        private static ref ssa.Value newValue2(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg0, ref ssa.Value arg1)
        {
            return s.curBlock.NewValue2(s.peekPos(), op, t, arg0, arg1);
        }

        // newValue2I adds a new value with two arguments and an auxint value to the current block.
        private static ref ssa.Value newValue2I(this ref state s, ssa.Op op, ref types.Type t, long aux, ref ssa.Value arg0, ref ssa.Value arg1)
        {
            return s.curBlock.NewValue2I(s.peekPos(), op, t, aux, arg0, arg1);
        }

        // newValue3 adds a new value with three arguments to the current block.
        private static ref ssa.Value newValue3(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg0, ref ssa.Value arg1, ref ssa.Value arg2)
        {
            return s.curBlock.NewValue3(s.peekPos(), op, t, arg0, arg1, arg2);
        }

        // newValue3I adds a new value with three arguments and an auxint value to the current block.
        private static ref ssa.Value newValue3I(this ref state s, ssa.Op op, ref types.Type t, long aux, ref ssa.Value arg0, ref ssa.Value arg1, ref ssa.Value arg2)
        {
            return s.curBlock.NewValue3I(s.peekPos(), op, t, aux, arg0, arg1, arg2);
        }

        // newValue3A adds a new value with three arguments and an aux value to the current block.
        private static ref ssa.Value newValue3A(this ref state s, ssa.Op op, ref types.Type t, object aux, ref ssa.Value arg0, ref ssa.Value arg1, ref ssa.Value arg2)
        {
            return s.curBlock.NewValue3A(s.peekPos(), op, t, aux, arg0, arg1, arg2);
        }

        // newValue4 adds a new value with four arguments to the current block.
        private static ref ssa.Value newValue4(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg0, ref ssa.Value arg1, ref ssa.Value arg2, ref ssa.Value arg3)
        {
            return s.curBlock.NewValue4(s.peekPos(), op, t, arg0, arg1, arg2, arg3);
        }

        // entryNewValue0 adds a new value with no arguments to the entry block.
        private static ref ssa.Value entryNewValue0(this ref state s, ssa.Op op, ref types.Type t)
        {
            return s.f.Entry.NewValue0(src.NoXPos, op, t);
        }

        // entryNewValue0A adds a new value with no arguments and an aux value to the entry block.
        private static ref ssa.Value entryNewValue0A(this ref state s, ssa.Op op, ref types.Type t, object aux)
        {
            return s.f.Entry.NewValue0A(src.NoXPos, op, t, aux);
        }

        // entryNewValue1 adds a new value with one argument to the entry block.
        private static ref ssa.Value entryNewValue1(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg)
        {
            return s.f.Entry.NewValue1(src.NoXPos, op, t, arg);
        }

        // entryNewValue1 adds a new value with one argument and an auxint value to the entry block.
        private static ref ssa.Value entryNewValue1I(this ref state s, ssa.Op op, ref types.Type t, long auxint, ref ssa.Value arg)
        {
            return s.f.Entry.NewValue1I(src.NoXPos, op, t, auxint, arg);
        }

        // entryNewValue1A adds a new value with one argument and an aux value to the entry block.
        private static ref ssa.Value entryNewValue1A(this ref state s, ssa.Op op, ref types.Type t, object aux, ref ssa.Value arg)
        {
            return s.f.Entry.NewValue1A(src.NoXPos, op, t, aux, arg);
        }

        // entryNewValue2 adds a new value with two arguments to the entry block.
        private static ref ssa.Value entryNewValue2(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg0, ref ssa.Value arg1)
        {
            return s.f.Entry.NewValue2(src.NoXPos, op, t, arg0, arg1);
        }

        // const* routines add a new const value to the entry block.
        private static ref ssa.Value constSlice(this ref state s, ref types.Type t)
        {
            return s.f.ConstSlice(s.peekPos(), t);
        }
        private static ref ssa.Value constInterface(this ref state s, ref types.Type t)
        {
            return s.f.ConstInterface(s.peekPos(), t);
        }
        private static ref ssa.Value constNil(this ref state s, ref types.Type t)
        {
            return s.f.ConstNil(s.peekPos(), t);
        }
        private static ref ssa.Value constEmptyString(this ref state s, ref types.Type t)
        {
            return s.f.ConstEmptyString(s.peekPos(), t);
        }
        private static ref ssa.Value constBool(this ref state s, bool c)
        {
            return s.f.ConstBool(s.peekPos(), types.Types[TBOOL], c);
        }
        private static ref ssa.Value constInt8(this ref state s, ref types.Type t, sbyte c)
        {
            return s.f.ConstInt8(s.peekPos(), t, c);
        }
        private static ref ssa.Value constInt16(this ref state s, ref types.Type t, short c)
        {
            return s.f.ConstInt16(s.peekPos(), t, c);
        }
        private static ref ssa.Value constInt32(this ref state s, ref types.Type t, int c)
        {
            return s.f.ConstInt32(s.peekPos(), t, c);
        }
        private static ref ssa.Value constInt64(this ref state s, ref types.Type t, long c)
        {
            return s.f.ConstInt64(s.peekPos(), t, c);
        }
        private static ref ssa.Value constFloat32(this ref state s, ref types.Type t, double c)
        {
            return s.f.ConstFloat32(s.peekPos(), t, c);
        }
        private static ref ssa.Value constFloat64(this ref state s, ref types.Type t, double c)
        {
            return s.f.ConstFloat64(s.peekPos(), t, c);
        }
        private static ref ssa.Value constInt(this ref state s, ref types.Type t, long c)
        {
            if (s.config.PtrSize == 8L)
            {
                return s.constInt64(t, c);
            }
            if (int64(int32(c)) != c)
            {
                s.Fatalf("integer constant too big %d", c);
            }
            return s.constInt32(t, int32(c));
        }
        private static ref ssa.Value constOffPtrSP(this ref state s, ref types.Type t, long c)
        {
            return s.f.ConstOffPtrSP(s.peekPos(), t, c, s.sp);
        }

        // newValueOrSfCall* are wrappers around newValue*, which may create a call to a
        // soft-float runtime function instead (when emitting soft-float code).
        private static ref ssa.Value newValueOrSfCall1(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg)
        {
            if (s.softFloat)
            {
                {
                    var (c, ok) = s.sfcall(op, arg);

                    if (ok)
                    {
                        return c;
                    }

                }
            }
            return s.newValue1(op, t, arg);
        }
        private static ref ssa.Value newValueOrSfCall2(this ref state s, ssa.Op op, ref types.Type t, ref ssa.Value arg0, ref ssa.Value arg1)
        {
            if (s.softFloat)
            {
                {
                    var (c, ok) = s.sfcall(op, arg0, arg1);

                    if (ok)
                    {
                        return c;
                    }

                }
            }
            return s.newValue2(op, t, arg0, arg1);
        }

        // stmtList converts the statement list n to SSA and adds it to s.
        private static void stmtList(this ref state s, Nodes l)
        {
            foreach (var (_, n) in l.Slice())
            {
                s.stmt(n);
            }
        }

        // stmt converts the statement n to SSA and adds it to s.
        private static void stmt(this ref state _s, ref Node _n) => func(_s, _n, (ref state s, ref Node n, Defer defer, Panic _, Recover __) =>
        {
            if (!(n.Op == OVARKILL || n.Op == OVARLIVE))
            { 
                // OVARKILL and OVARLIVE are invisible to the programmer, so we don't use their line numbers to avoid confusion in debugging.
                s.pushLine(n.Pos);
                defer(s.popLine());
            } 

            // If s.curBlock is nil, and n isn't a label (which might have an associated goto somewhere),
            // then this code is dead. Stop here.
            if (s.curBlock == null && n.Op != OLABEL)
            {
                return;
            }
            s.stmtList(n.Ninit);


            if (n.Op == OBLOCK)
            {
                s.stmtList(n.List); 

                // No-ops
                goto __switch_break0;
            }
            if (n.Op == OEMPTY || n.Op == ODCLCONST || n.Op == ODCLTYPE || n.Op == OFALL)
            {
                goto __switch_break0;
            }
            if (n.Op == OCALLFUNC)
            {
                if (isIntrinsicCall(n))
                {
                    s.intrinsicCall(n);
                    return;
                }
                fallthrough = true;

            }
            if (fallthrough || n.Op == OCALLMETH || n.Op == OCALLINTER)
            {
                s.call(n, callNormal);
                if (n.Op == OCALLFUNC && n.Left.Op == ONAME && n.Left.Class() == PFUNC)
                {
                    {
                        var fn = n.Left.Sym.Name;

                        if (compiling_runtime && fn == "throw" || n.Left.Sym.Pkg == Runtimepkg && (fn == "throwinit" || fn == "gopanic" || fn == "panicwrap" || fn == "block"))
                        {
                            var m = s.mem();
                            var b = s.endBlock();
                            b.Kind = ssa.BlockExit;
                            b.SetControl(m); 
                            // TODO: never rewrite OPANIC to OCALLFUNC in the
                            // first place. Need to wait until all backends
                            // go through SSA.
                        }

                    }
                }
                goto __switch_break0;
            }
            if (n.Op == ODEFER)
            {
                s.call(n.Left, callDefer);
                goto __switch_break0;
            }
            if (n.Op == OPROC)
            {
                s.call(n.Left, callGo);
                goto __switch_break0;
            }
            if (n.Op == OAS2DOTTYPE)
            {
                var (res, resok) = s.dottype(n.Rlist.First(), true);
                var deref = false;
                if (!canSSAType(n.Rlist.First().Type))
                {
                    if (res.Op != ssa.OpLoad)
                    {
                        s.Fatalf("dottype of non-load");
                    }
                    var mem = s.mem();
                    if (mem.Op == ssa.OpVarKill)
                    {
                        mem = mem.Args[0L];
                    }
                    if (res.Args[1L] != mem)
                    {
                        s.Fatalf("memory no longer live from 2-result dottype load");
                    }
                    deref = true;
                    res = res.Args[0L];
                }
                s.assign(n.List.First(), res, deref, 0L);
                s.assign(n.List.Second(), resok, false, 0L);
                return;
                goto __switch_break0;
            }
            if (n.Op == OAS2FUNC) 
            {
                // We come here only when it is an intrinsic call returning two values.
                if (!isIntrinsicCall(n.Rlist.First()))
                {
                    s.Fatalf("non-intrinsic AS2FUNC not expanded %v", n.Rlist.First());
                }
                var v = s.intrinsicCall(n.Rlist.First());
                var v1 = s.newValue1(ssa.OpSelect0, n.List.First().Type, v);
                var v2 = s.newValue1(ssa.OpSelect1, n.List.Second().Type, v);
                s.assign(n.List.First(), v1, false, 0L);
                s.assign(n.List.Second(), v2, false, 0L);
                return;
                goto __switch_break0;
            }
            if (n.Op == ODCL)
            {
                if (n.Left.Class() == PAUTOHEAP)
                {
                    Fatalf("DCL %v", n);
                }
                goto __switch_break0;
            }
            if (n.Op == OLABEL)
            {
                var sym = n.Left.Sym;
                var lab = s.label(sym); 

                // Associate label with its control flow node, if any
                {
                    var ctl = n.labeledControl();

                    if (ctl != null)
                    {
                        s.labeledNodes[ctl] = lab;
                    } 

                    // The label might already have a target block via a goto.

                } 

                // The label might already have a target block via a goto.
                if (lab.target == null)
                {
                    lab.target = s.f.NewBlock(ssa.BlockPlain);
                } 

                // Go to that label.
                // (We pretend "label:" is preceded by "goto label", unless the predecessor is unreachable.)
                if (s.curBlock != null)
                {
                    b = s.endBlock();
                    b.AddEdgeTo(lab.target);
                }
                s.startBlock(lab.target);
                goto __switch_break0;
            }
            if (n.Op == OGOTO)
            {
                sym = n.Left.Sym;

                lab = s.label(sym);
                if (lab.target == null)
                {
                    lab.target = s.f.NewBlock(ssa.BlockPlain);
                }
                b = s.endBlock();
                b.Pos = s.lastPos; // Do this even if b is an empty block.
                b.AddEdgeTo(lab.target);
                goto __switch_break0;
            }
            if (n.Op == OAS)
            {
                if (n.Left == n.Right && n.Left.Op == ONAME)
                { 
                    // An x=x assignment. No point in doing anything
                    // here. In addition, skipping this assignment
                    // prevents generating:
                    //   VARDEF x
                    //   COPY x -> x
                    // which is bad because x is incorrectly considered
                    // dead before the vardef. See issue #14904.
                    return;
                } 

                // Evaluate RHS.
                var rhs = n.Right;
                if (rhs != null)
                {

                    if (rhs.Op == OSTRUCTLIT || rhs.Op == OARRAYLIT || rhs.Op == OSLICELIT) 
                        // All literals with nonzero fields have already been
                        // rewritten during walk. Any that remain are just T{}
                        // or equivalents. Use the zero value.
                        if (!iszero(rhs))
                        {
                            Fatalf("literal with nonzero value in SSA: %v", rhs);
                        }
                        rhs = null;
                    else if (rhs.Op == OAPPEND) 
                        // Check whether we're writing the result of an append back to the same slice.
                        // If so, we handle it specially to avoid write barriers on the fast
                        // (non-growth) path.
                        if (!samesafeexpr(n.Left, rhs.List.First()) || Debug['N'] != 0L)
                        {
                            break;
                        } 
                        // If the slice can be SSA'd, it'll be on the stack,
                        // so there will be no write barriers,
                        // so there's no need to attempt to prevent them.
                        if (s.canSSA(n.Left))
                        {
                            if (Debug_append > 0L)
                            { // replicating old diagnostic message
                                Warnl(n.Pos, "append: len-only update (in local slice)");
                            }
                            break;
                        }
                        if (Debug_append > 0L)
                        {
                            Warnl(n.Pos, "append: len-only update");
                        }
                        s.append(rhs, true);
                        return;
                                    }
                if (isblank(n.Left))
                { 
                    // _ = rhs
                    // Just evaluate rhs for side-effects.
                    if (rhs != null)
                    {
                        s.expr(rhs);
                    }
                    return;
                }
                ref types.Type t = default;
                if (n.Right != null)
                {
                    t = n.Right.Type;
                }
                else
                {
                    t = n.Left.Type;
                }
                ref ssa.Value r = default;
                deref = !canSSAType(t);
                if (deref)
                {
                    if (rhs == null)
                    {
                        r = null; // Signal assign to use OpZero.
                    }
                    else
                    {
                        r = s.addr(rhs, false);
                    }
                }
                else
                {
                    if (rhs == null)
                    {
                        r = s.zeroVal(t);
                    }
                    else
                    {
                        r = s.expr(rhs);
                    }
                }
                skipMask skip = default;
                if (rhs != null && (rhs.Op == OSLICE || rhs.Op == OSLICE3 || rhs.Op == OSLICESTR) && samesafeexpr(rhs.Left, n.Left))
                { 
                    // We're assigning a slicing operation back to its source.
                    // Don't write back fields we aren't changing. See issue #14855.
                    var (i, j, k) = rhs.SliceBounds();
                    if (i != null && (i.Op == OLITERAL && i.Val().Ctype() == CTINT && i.Int64() == 0L))
                    { 
                        // [0:...] is the same as [:...]
                        i = null;
                    } 
                    // TODO: detect defaults for len/cap also.
                    // Currently doesn't really work because (*p)[:len(*p)] appears here as:
                    //    tmp = len(*p)
                    //    (*p)[:tmp]
                    //if j != nil && (j.Op == OLEN && samesafeexpr(j.Left, n.Left)) {
                    //      j = nil
                    //}
                    //if k != nil && (k.Op == OCAP && samesafeexpr(k.Left, n.Left)) {
                    //      k = nil
                    //}
                    if (i == null)
                    {
                        skip |= skipPtr;
                        if (j == null)
                        {
                            skip |= skipLen;
                        }
                        if (k == null)
                        {
                            skip |= skipCap;
                        }
                    }
                }
                s.assign(n.Left, r, deref, skip);
                goto __switch_break0;
            }
            if (n.Op == OIF)
            {
                var bThen = s.f.NewBlock(ssa.BlockPlain);
                var bEnd = s.f.NewBlock(ssa.BlockPlain);
                ref ssa.Block bElse = default;
                sbyte likely = default;
                if (n.Likely())
                {
                    likely = 1L;
                }
                if (n.Rlist.Len() != 0L)
                {
                    bElse = s.f.NewBlock(ssa.BlockPlain);
                    s.condBranch(n.Left, bThen, bElse, likely);
                }
                else
                {
                    s.condBranch(n.Left, bThen, bEnd, likely);
                }
                s.startBlock(bThen);
                s.stmtList(n.Nbody);
                {
                    var b__prev1 = b;

                    b = s.endBlock();

                    if (b != null)
                    {
                        b.AddEdgeTo(bEnd);
                    }

                    b = b__prev1;

                }

                if (n.Rlist.Len() != 0L)
                {
                    s.startBlock(bElse);
                    s.stmtList(n.Rlist);
                    {
                        var b__prev2 = b;

                        b = s.endBlock();

                        if (b != null)
                        {
                            b.AddEdgeTo(bEnd);
                        }

                        b = b__prev2;

                    }
                }
                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == ORETURN)
            {
                s.stmtList(n.List);
                b = s.exit();
                b.Pos = s.lastPos;
                goto __switch_break0;
            }
            if (n.Op == ORETJMP)
            {
                s.stmtList(n.List);
                b = s.exit();
                b.Kind = ssa.BlockRetJmp; // override BlockRet
                b.Aux = n.Sym.Linksym();
                goto __switch_break0;
            }
            if (n.Op == OCONTINUE || n.Op == OBREAK)
            {
                ref ssa.Block to = default;
                if (n.Left == null)
                { 
                    // plain break/continue

                    if (n.Op == OCONTINUE) 
                        to = s.continueTo;
                    else if (n.Op == OBREAK) 
                        to = s.breakTo;
                                    }
                else
                { 
                    // labeled break/continue; look up the target
                    sym = n.Left.Sym;
                    lab = s.label(sym);

                    if (n.Op == OCONTINUE) 
                        to = lab.continueTarget;
                    else if (n.Op == OBREAK) 
                        to = lab.breakTarget;
                                    }
                b = s.endBlock();
                b.Pos = s.lastPos; // Do this even if b is an empty block.
                b.AddEdgeTo(to);
                goto __switch_break0;
            }
            if (n.Op == OFOR || n.Op == OFORUNTIL) 
            {
                // OFOR: for Ninit; Left; Right { Nbody }
                // For      = cond; body; incr
                // Foruntil = body; incr; cond
                var bCond = s.f.NewBlock(ssa.BlockPlain);
                var bBody = s.f.NewBlock(ssa.BlockPlain);
                var bIncr = s.f.NewBlock(ssa.BlockPlain);
                bEnd = s.f.NewBlock(ssa.BlockPlain); 

                // first, jump to condition test (OFOR) or body (OFORUNTIL)
                b = s.endBlock();
                if (n.Op == OFOR)
                {
                    b.AddEdgeTo(bCond); 
                    // generate code to test condition
                    s.startBlock(bCond);
                    if (n.Left != null)
                    {
                        s.condBranch(n.Left, bBody, bEnd, 1L);
                    }
                    else
                    {
                        b = s.endBlock();
                        b.Kind = ssa.BlockPlain;
                        b.AddEdgeTo(bBody);
                    }
                }
                else
                {
                    b.AddEdgeTo(bBody);
                } 

                // set up for continue/break in body
                var prevContinue = s.continueTo;
                var prevBreak = s.breakTo;
                s.continueTo = bIncr;
                s.breakTo = bEnd;
                lab = s.labeledNodes[n];
                if (lab != null)
                { 
                    // labeled for loop
                    lab.continueTarget = bIncr;
                    lab.breakTarget = bEnd;
                } 

                // generate body
                s.startBlock(bBody);
                s.stmtList(n.Nbody); 

                // tear down continue/break
                s.continueTo = prevContinue;
                s.breakTo = prevBreak;
                if (lab != null)
                {
                    lab.continueTarget = null;
                    lab.breakTarget = null;
                } 

                // done with body, goto incr
                {
                    var b__prev1 = b;

                    b = s.endBlock();

                    if (b != null)
                    {
                        b.AddEdgeTo(bIncr);
                    } 

                    // generate incr

                    b = b__prev1;

                } 

                // generate incr
                s.startBlock(bIncr);
                if (n.Right != null)
                {
                    s.stmt(n.Right);
                }
                {
                    var b__prev1 = b;

                    b = s.endBlock();

                    if (b != null)
                    {
                        b.AddEdgeTo(bCond); 
                        // It can happen that bIncr ends in a block containing only VARKILL,
                        // and that muddles the debugging experience.
                        if (n.Op != OFORUNTIL && b.Pos == src.NoXPos)
                        {
                            b.Pos = bCond.Pos;
                        }
                    }

                    b = b__prev1;

                }

                if (n.Op == OFORUNTIL)
                { 
                    // generate code to test condition
                    s.startBlock(bCond);
                    if (n.Left != null)
                    {
                        s.condBranch(n.Left, bBody, bEnd, 1L);
                    }
                    else
                    {
                        b = s.endBlock();
                        b.Kind = ssa.BlockPlain;
                        b.AddEdgeTo(bBody);
                    }
                }
                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == OSWITCH || n.Op == OSELECT) 
            {
                // These have been mostly rewritten by the front end into their Nbody fields.
                // Our main task is to correctly hook up any break statements.
                bEnd = s.f.NewBlock(ssa.BlockPlain);

                prevBreak = s.breakTo;
                s.breakTo = bEnd;
                lab = s.labeledNodes[n];
                if (lab != null)
                { 
                    // labeled
                    lab.breakTarget = bEnd;
                } 

                // generate body code
                s.stmtList(n.Nbody);

                s.breakTo = prevBreak;
                if (lab != null)
                {
                    lab.breakTarget = null;
                } 

                // walk adds explicit OBREAK nodes to the end of all reachable code paths.
                // If we still have a current block here, then mark it unreachable.
                if (s.curBlock != null)
                {
                    m = s.mem();
                    b = s.endBlock();
                    b.Kind = ssa.BlockExit;
                    b.SetControl(m);
                }
                s.startBlock(bEnd);
                goto __switch_break0;
            }
            if (n.Op == OVARKILL) 
            {
                // Insert a varkill op to record that a variable is no longer live.
                // We only care about liveness info at call sites, so putting the
                // varkill in the store chain is enough to keep it correctly ordered
                // with respect to call ops.
                if (!s.canSSA(n.Left))
                {
                    s.vars[ref memVar] = s.newValue1A(ssa.OpVarKill, types.TypeMem, n.Left, s.mem());
                }
                goto __switch_break0;
            }
            if (n.Op == OVARLIVE) 
            {
                // Insert a varlive op to record that a variable is still live.
                if (!n.Left.Addrtaken())
                {
                    s.Fatalf("VARLIVE variable %v must have Addrtaken set", n.Left);
                }

                if (n.Left.Class() == PAUTO || n.Left.Class() == PPARAM || n.Left.Class() == PPARAMOUT)                 else 
                    s.Fatalf("VARLIVE variable %v must be Auto or Arg", n.Left);
                                s.vars[ref memVar] = s.newValue1A(ssa.OpVarLive, types.TypeMem, n.Left, s.mem());
                goto __switch_break0;
            }
            if (n.Op == OCHECKNIL)
            {
                var p = s.expr(n.Left);
                s.nilCheck(p);
                goto __switch_break0;
            }
            // default: 
                s.Fatalf("unhandled stmt %v", n.Op);

            __switch_break0:;
        });

        // exit processes any code that needs to be generated just before returning.
        // It returns a BlockRet block that ends the control flow. Its control value
        // will be set to the final memory state.
        private static ref ssa.Block exit(this ref state s)
        {
            if (s.hasdefer)
            {
                s.rtcall(Deferreturn, true, null);
            } 

            // Run exit code. Typically, this code copies heap-allocated PPARAMOUT
            // variables back to the stack.
            s.stmtList(s.exitCode); 

            // Store SSAable PPARAMOUT variables back to stack locations.
            foreach (var (_, n) in s.returns)
            {
                var addr = s.decladdrs[n];
                var val = s.variable(n, n.Type);
                s.vars[ref memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, n, s.mem());
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, n.Type, addr, val, s.mem()); 
                // TODO: if val is ever spilled, we'd like to use the
                // PPARAMOUT slot for spilling it. That won't happen
                // currently.
            } 

            // Do actual return.
            var m = s.mem();
            var b = s.endBlock();
            b.Kind = ssa.BlockRet;
            b.SetControl(m);
            return b;
        }

        private partial struct opAndType
        {
            public Op op;
            public types.EType etype;
        }

        private static map opToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndType, ssa.Op>{opAndType{OADD,TINT8}:ssa.OpAdd8,opAndType{OADD,TUINT8}:ssa.OpAdd8,opAndType{OADD,TINT16}:ssa.OpAdd16,opAndType{OADD,TUINT16}:ssa.OpAdd16,opAndType{OADD,TINT32}:ssa.OpAdd32,opAndType{OADD,TUINT32}:ssa.OpAdd32,opAndType{OADD,TPTR32}:ssa.OpAdd32,opAndType{OADD,TINT64}:ssa.OpAdd64,opAndType{OADD,TUINT64}:ssa.OpAdd64,opAndType{OADD,TPTR64}:ssa.OpAdd64,opAndType{OADD,TFLOAT32}:ssa.OpAdd32F,opAndType{OADD,TFLOAT64}:ssa.OpAdd64F,opAndType{OSUB,TINT8}:ssa.OpSub8,opAndType{OSUB,TUINT8}:ssa.OpSub8,opAndType{OSUB,TINT16}:ssa.OpSub16,opAndType{OSUB,TUINT16}:ssa.OpSub16,opAndType{OSUB,TINT32}:ssa.OpSub32,opAndType{OSUB,TUINT32}:ssa.OpSub32,opAndType{OSUB,TINT64}:ssa.OpSub64,opAndType{OSUB,TUINT64}:ssa.OpSub64,opAndType{OSUB,TFLOAT32}:ssa.OpSub32F,opAndType{OSUB,TFLOAT64}:ssa.OpSub64F,opAndType{ONOT,TBOOL}:ssa.OpNot,opAndType{OMINUS,TINT8}:ssa.OpNeg8,opAndType{OMINUS,TUINT8}:ssa.OpNeg8,opAndType{OMINUS,TINT16}:ssa.OpNeg16,opAndType{OMINUS,TUINT16}:ssa.OpNeg16,opAndType{OMINUS,TINT32}:ssa.OpNeg32,opAndType{OMINUS,TUINT32}:ssa.OpNeg32,opAndType{OMINUS,TINT64}:ssa.OpNeg64,opAndType{OMINUS,TUINT64}:ssa.OpNeg64,opAndType{OMINUS,TFLOAT32}:ssa.OpNeg32F,opAndType{OMINUS,TFLOAT64}:ssa.OpNeg64F,opAndType{OCOM,TINT8}:ssa.OpCom8,opAndType{OCOM,TUINT8}:ssa.OpCom8,opAndType{OCOM,TINT16}:ssa.OpCom16,opAndType{OCOM,TUINT16}:ssa.OpCom16,opAndType{OCOM,TINT32}:ssa.OpCom32,opAndType{OCOM,TUINT32}:ssa.OpCom32,opAndType{OCOM,TINT64}:ssa.OpCom64,opAndType{OCOM,TUINT64}:ssa.OpCom64,opAndType{OIMAG,TCOMPLEX64}:ssa.OpComplexImag,opAndType{OIMAG,TCOMPLEX128}:ssa.OpComplexImag,opAndType{OREAL,TCOMPLEX64}:ssa.OpComplexReal,opAndType{OREAL,TCOMPLEX128}:ssa.OpComplexReal,opAndType{OMUL,TINT8}:ssa.OpMul8,opAndType{OMUL,TUINT8}:ssa.OpMul8,opAndType{OMUL,TINT16}:ssa.OpMul16,opAndType{OMUL,TUINT16}:ssa.OpMul16,opAndType{OMUL,TINT32}:ssa.OpMul32,opAndType{OMUL,TUINT32}:ssa.OpMul32,opAndType{OMUL,TINT64}:ssa.OpMul64,opAndType{OMUL,TUINT64}:ssa.OpMul64,opAndType{OMUL,TFLOAT32}:ssa.OpMul32F,opAndType{OMUL,TFLOAT64}:ssa.OpMul64F,opAndType{ODIV,TFLOAT32}:ssa.OpDiv32F,opAndType{ODIV,TFLOAT64}:ssa.OpDiv64F,opAndType{ODIV,TINT8}:ssa.OpDiv8,opAndType{ODIV,TUINT8}:ssa.OpDiv8u,opAndType{ODIV,TINT16}:ssa.OpDiv16,opAndType{ODIV,TUINT16}:ssa.OpDiv16u,opAndType{ODIV,TINT32}:ssa.OpDiv32,opAndType{ODIV,TUINT32}:ssa.OpDiv32u,opAndType{ODIV,TINT64}:ssa.OpDiv64,opAndType{ODIV,TUINT64}:ssa.OpDiv64u,opAndType{OMOD,TINT8}:ssa.OpMod8,opAndType{OMOD,TUINT8}:ssa.OpMod8u,opAndType{OMOD,TINT16}:ssa.OpMod16,opAndType{OMOD,TUINT16}:ssa.OpMod16u,opAndType{OMOD,TINT32}:ssa.OpMod32,opAndType{OMOD,TUINT32}:ssa.OpMod32u,opAndType{OMOD,TINT64}:ssa.OpMod64,opAndType{OMOD,TUINT64}:ssa.OpMod64u,opAndType{OAND,TINT8}:ssa.OpAnd8,opAndType{OAND,TUINT8}:ssa.OpAnd8,opAndType{OAND,TINT16}:ssa.OpAnd16,opAndType{OAND,TUINT16}:ssa.OpAnd16,opAndType{OAND,TINT32}:ssa.OpAnd32,opAndType{OAND,TUINT32}:ssa.OpAnd32,opAndType{OAND,TINT64}:ssa.OpAnd64,opAndType{OAND,TUINT64}:ssa.OpAnd64,opAndType{OOR,TINT8}:ssa.OpOr8,opAndType{OOR,TUINT8}:ssa.OpOr8,opAndType{OOR,TINT16}:ssa.OpOr16,opAndType{OOR,TUINT16}:ssa.OpOr16,opAndType{OOR,TINT32}:ssa.OpOr32,opAndType{OOR,TUINT32}:ssa.OpOr32,opAndType{OOR,TINT64}:ssa.OpOr64,opAndType{OOR,TUINT64}:ssa.OpOr64,opAndType{OXOR,TINT8}:ssa.OpXor8,opAndType{OXOR,TUINT8}:ssa.OpXor8,opAndType{OXOR,TINT16}:ssa.OpXor16,opAndType{OXOR,TUINT16}:ssa.OpXor16,opAndType{OXOR,TINT32}:ssa.OpXor32,opAndType{OXOR,TUINT32}:ssa.OpXor32,opAndType{OXOR,TINT64}:ssa.OpXor64,opAndType{OXOR,TUINT64}:ssa.OpXor64,opAndType{OEQ,TBOOL}:ssa.OpEqB,opAndType{OEQ,TINT8}:ssa.OpEq8,opAndType{OEQ,TUINT8}:ssa.OpEq8,opAndType{OEQ,TINT16}:ssa.OpEq16,opAndType{OEQ,TUINT16}:ssa.OpEq16,opAndType{OEQ,TINT32}:ssa.OpEq32,opAndType{OEQ,TUINT32}:ssa.OpEq32,opAndType{OEQ,TINT64}:ssa.OpEq64,opAndType{OEQ,TUINT64}:ssa.OpEq64,opAndType{OEQ,TINTER}:ssa.OpEqInter,opAndType{OEQ,TSLICE}:ssa.OpEqSlice,opAndType{OEQ,TFUNC}:ssa.OpEqPtr,opAndType{OEQ,TMAP}:ssa.OpEqPtr,opAndType{OEQ,TCHAN}:ssa.OpEqPtr,opAndType{OEQ,TPTR32}:ssa.OpEqPtr,opAndType{OEQ,TPTR64}:ssa.OpEqPtr,opAndType{OEQ,TUINTPTR}:ssa.OpEqPtr,opAndType{OEQ,TUNSAFEPTR}:ssa.OpEqPtr,opAndType{OEQ,TFLOAT64}:ssa.OpEq64F,opAndType{OEQ,TFLOAT32}:ssa.OpEq32F,opAndType{ONE,TBOOL}:ssa.OpNeqB,opAndType{ONE,TINT8}:ssa.OpNeq8,opAndType{ONE,TUINT8}:ssa.OpNeq8,opAndType{ONE,TINT16}:ssa.OpNeq16,opAndType{ONE,TUINT16}:ssa.OpNeq16,opAndType{ONE,TINT32}:ssa.OpNeq32,opAndType{ONE,TUINT32}:ssa.OpNeq32,opAndType{ONE,TINT64}:ssa.OpNeq64,opAndType{ONE,TUINT64}:ssa.OpNeq64,opAndType{ONE,TINTER}:ssa.OpNeqInter,opAndType{ONE,TSLICE}:ssa.OpNeqSlice,opAndType{ONE,TFUNC}:ssa.OpNeqPtr,opAndType{ONE,TMAP}:ssa.OpNeqPtr,opAndType{ONE,TCHAN}:ssa.OpNeqPtr,opAndType{ONE,TPTR32}:ssa.OpNeqPtr,opAndType{ONE,TPTR64}:ssa.OpNeqPtr,opAndType{ONE,TUINTPTR}:ssa.OpNeqPtr,opAndType{ONE,TUNSAFEPTR}:ssa.OpNeqPtr,opAndType{ONE,TFLOAT64}:ssa.OpNeq64F,opAndType{ONE,TFLOAT32}:ssa.OpNeq32F,opAndType{OLT,TINT8}:ssa.OpLess8,opAndType{OLT,TUINT8}:ssa.OpLess8U,opAndType{OLT,TINT16}:ssa.OpLess16,opAndType{OLT,TUINT16}:ssa.OpLess16U,opAndType{OLT,TINT32}:ssa.OpLess32,opAndType{OLT,TUINT32}:ssa.OpLess32U,opAndType{OLT,TINT64}:ssa.OpLess64,opAndType{OLT,TUINT64}:ssa.OpLess64U,opAndType{OLT,TFLOAT64}:ssa.OpLess64F,opAndType{OLT,TFLOAT32}:ssa.OpLess32F,opAndType{OGT,TINT8}:ssa.OpGreater8,opAndType{OGT,TUINT8}:ssa.OpGreater8U,opAndType{OGT,TINT16}:ssa.OpGreater16,opAndType{OGT,TUINT16}:ssa.OpGreater16U,opAndType{OGT,TINT32}:ssa.OpGreater32,opAndType{OGT,TUINT32}:ssa.OpGreater32U,opAndType{OGT,TINT64}:ssa.OpGreater64,opAndType{OGT,TUINT64}:ssa.OpGreater64U,opAndType{OGT,TFLOAT64}:ssa.OpGreater64F,opAndType{OGT,TFLOAT32}:ssa.OpGreater32F,opAndType{OLE,TINT8}:ssa.OpLeq8,opAndType{OLE,TUINT8}:ssa.OpLeq8U,opAndType{OLE,TINT16}:ssa.OpLeq16,opAndType{OLE,TUINT16}:ssa.OpLeq16U,opAndType{OLE,TINT32}:ssa.OpLeq32,opAndType{OLE,TUINT32}:ssa.OpLeq32U,opAndType{OLE,TINT64}:ssa.OpLeq64,opAndType{OLE,TUINT64}:ssa.OpLeq64U,opAndType{OLE,TFLOAT64}:ssa.OpLeq64F,opAndType{OLE,TFLOAT32}:ssa.OpLeq32F,opAndType{OGE,TINT8}:ssa.OpGeq8,opAndType{OGE,TUINT8}:ssa.OpGeq8U,opAndType{OGE,TINT16}:ssa.OpGeq16,opAndType{OGE,TUINT16}:ssa.OpGeq16U,opAndType{OGE,TINT32}:ssa.OpGeq32,opAndType{OGE,TUINT32}:ssa.OpGeq32U,opAndType{OGE,TINT64}:ssa.OpGeq64,opAndType{OGE,TUINT64}:ssa.OpGeq64U,opAndType{OGE,TFLOAT64}:ssa.OpGeq64F,opAndType{OGE,TFLOAT32}:ssa.OpGeq32F,};

        private static types.EType concreteEtype(this ref state s, ref types.Type t)
        {
            var e = t.Etype;

            if (e == TINT) 
                if (s.config.PtrSize == 8L)
                {
                    return TINT64;
                }
                return TINT32;
            else if (e == TUINT) 
                if (s.config.PtrSize == 8L)
                {
                    return TUINT64;
                }
                return TUINT32;
            else if (e == TUINTPTR) 
                if (s.config.PtrSize == 8L)
                {
                    return TUINT64;
                }
                return TUINT32;
            else 
                return e;
                    }

        private static ssa.Op ssaOp(this ref state s, Op op, ref types.Type t)
        {
            var etype = s.concreteEtype(t);
            var (x, ok) = opToSSA[new opAndType(op,etype)];
            if (!ok)
            {
                s.Fatalf("unhandled binary op %v %s", op, etype);
            }
            return x;
        }

        private static ref types.Type floatForComplex(ref types.Type t)
        {
            if (t.Size() == 8L)
            {
                return types.Types[TFLOAT32];
            }
            else
            {
                return types.Types[TFLOAT64];
            }
        }

        private partial struct opAndTwoTypes
        {
            public Op op;
            public types.EType etype1;
            public types.EType etype2;
        }

        private partial struct twoTypes
        {
            public types.EType etype1;
            public types.EType etype2;
        }

        private partial struct twoOpsAndType
        {
            public ssa.Op op1;
            public ssa.Op op2;
            public types.EType intermediateType;
        }

        private static map fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TINT8,TFLOAT32}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to32F,TINT32},twoTypes{TINT16,TFLOAT32}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to32F,TINT32},twoTypes{TINT32,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to32F,TINT32},twoTypes{TINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to32F,TINT64},twoTypes{TINT8,TFLOAT64}:twoOpsAndType{ssa.OpSignExt8to32,ssa.OpCvt32to64F,TINT32},twoTypes{TINT16,TFLOAT64}:twoOpsAndType{ssa.OpSignExt16to32,ssa.OpCvt32to64F,TINT32},twoTypes{TINT32,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32to64F,TINT32},twoTypes{TINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64to64F,TINT64},twoTypes{TFLOAT32,TINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT32,TINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT32,TINT32}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpCopy,TINT32},twoTypes{TFLOAT32,TINT64}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpCopy,TINT64},twoTypes{TFLOAT64,TINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT64,TINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT64,TINT32}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpCopy,TINT32},twoTypes{TFLOAT64,TINT64}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpCopy,TINT64},twoTypes{TUINT8,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to32F,TINT32},twoTypes{TUINT16,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to32F,TINT32},twoTypes{TUINT32,TFLOAT32}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to32F,TINT64},twoTypes{TUINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,TUINT64},twoTypes{TUINT8,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt8to32,ssa.OpCvt32to64F,TINT32},twoTypes{TUINT16,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt16to32,ssa.OpCvt32to64F,TINT32},twoTypes{TUINT32,TFLOAT64}:twoOpsAndType{ssa.OpZeroExt32to64,ssa.OpCvt64to64F,TINT64},twoTypes{TUINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpInvalid,TUINT64},twoTypes{TFLOAT32,TUINT8}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT32,TUINT16}:twoOpsAndType{ssa.OpCvt32Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT32,TUINT32}:twoOpsAndType{ssa.OpCvt32Fto64,ssa.OpTrunc64to32,TINT64},twoTypes{TFLOAT32,TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TUINT8}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to8,TINT32},twoTypes{TFLOAT64,TUINT16}:twoOpsAndType{ssa.OpCvt64Fto32,ssa.OpTrunc32to16,TINT32},twoTypes{TFLOAT64,TUINT32}:twoOpsAndType{ssa.OpCvt64Fto64,ssa.OpTrunc64to32,TINT64},twoTypes{TFLOAT64,TUINT64}:twoOpsAndType{ssa.OpInvalid,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TFLOAT32}:twoOpsAndType{ssa.OpCvt64Fto32F,ssa.OpCopy,TFLOAT32},twoTypes{TFLOAT64,TFLOAT64}:twoOpsAndType{ssa.OpRound64F,ssa.OpCopy,TFLOAT64},twoTypes{TFLOAT32,TFLOAT32}:twoOpsAndType{ssa.OpRound32F,ssa.OpCopy,TFLOAT32},twoTypes{TFLOAT32,TFLOAT64}:twoOpsAndType{ssa.OpCvt32Fto64F,ssa.OpCopy,TFLOAT64},};

        // this map is used only for 32-bit arch, and only includes the difference
        // on 32-bit arch, don't use int64<->float conversion for uint32
        private static map fpConvOpToSSA32 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TUINT32,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto32F,TUINT32},twoTypes{TUINT32,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt32Uto64F,TUINT32},twoTypes{TFLOAT32,TUINT32}:twoOpsAndType{ssa.OpCvt32Fto32U,ssa.OpCopy,TUINT32},twoTypes{TFLOAT64,TUINT32}:twoOpsAndType{ssa.OpCvt64Fto32U,ssa.OpCopy,TUINT32},};

        // uint64<->float conversions, only on machines that have intructions for that
        private static map uint64fpConvOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<twoTypes, twoOpsAndType>{twoTypes{TUINT64,TFLOAT32}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto32F,TUINT64},twoTypes{TUINT64,TFLOAT64}:twoOpsAndType{ssa.OpCopy,ssa.OpCvt64Uto64F,TUINT64},twoTypes{TFLOAT32,TUINT64}:twoOpsAndType{ssa.OpCvt32Fto64U,ssa.OpCopy,TUINT64},twoTypes{TFLOAT64,TUINT64}:twoOpsAndType{ssa.OpCvt64Fto64U,ssa.OpCopy,TUINT64},};

        private static map shiftOpToSSA = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<opAndTwoTypes, ssa.Op>{opAndTwoTypes{OLSH,TINT8,TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{OLSH,TUINT8,TUINT8}:ssa.OpLsh8x8,opAndTwoTypes{OLSH,TINT8,TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{OLSH,TUINT8,TUINT16}:ssa.OpLsh8x16,opAndTwoTypes{OLSH,TINT8,TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{OLSH,TUINT8,TUINT32}:ssa.OpLsh8x32,opAndTwoTypes{OLSH,TINT8,TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{OLSH,TUINT8,TUINT64}:ssa.OpLsh8x64,opAndTwoTypes{OLSH,TINT16,TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{OLSH,TUINT16,TUINT8}:ssa.OpLsh16x8,opAndTwoTypes{OLSH,TINT16,TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{OLSH,TUINT16,TUINT16}:ssa.OpLsh16x16,opAndTwoTypes{OLSH,TINT16,TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{OLSH,TUINT16,TUINT32}:ssa.OpLsh16x32,opAndTwoTypes{OLSH,TINT16,TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{OLSH,TUINT16,TUINT64}:ssa.OpLsh16x64,opAndTwoTypes{OLSH,TINT32,TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{OLSH,TUINT32,TUINT8}:ssa.OpLsh32x8,opAndTwoTypes{OLSH,TINT32,TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{OLSH,TUINT32,TUINT16}:ssa.OpLsh32x16,opAndTwoTypes{OLSH,TINT32,TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{OLSH,TUINT32,TUINT32}:ssa.OpLsh32x32,opAndTwoTypes{OLSH,TINT32,TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{OLSH,TUINT32,TUINT64}:ssa.OpLsh32x64,opAndTwoTypes{OLSH,TINT64,TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{OLSH,TUINT64,TUINT8}:ssa.OpLsh64x8,opAndTwoTypes{OLSH,TINT64,TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{OLSH,TUINT64,TUINT16}:ssa.OpLsh64x16,opAndTwoTypes{OLSH,TINT64,TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{OLSH,TUINT64,TUINT32}:ssa.OpLsh64x32,opAndTwoTypes{OLSH,TINT64,TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{OLSH,TUINT64,TUINT64}:ssa.OpLsh64x64,opAndTwoTypes{ORSH,TINT8,TUINT8}:ssa.OpRsh8x8,opAndTwoTypes{ORSH,TUINT8,TUINT8}:ssa.OpRsh8Ux8,opAndTwoTypes{ORSH,TINT8,TUINT16}:ssa.OpRsh8x16,opAndTwoTypes{ORSH,TUINT8,TUINT16}:ssa.OpRsh8Ux16,opAndTwoTypes{ORSH,TINT8,TUINT32}:ssa.OpRsh8x32,opAndTwoTypes{ORSH,TUINT8,TUINT32}:ssa.OpRsh8Ux32,opAndTwoTypes{ORSH,TINT8,TUINT64}:ssa.OpRsh8x64,opAndTwoTypes{ORSH,TUINT8,TUINT64}:ssa.OpRsh8Ux64,opAndTwoTypes{ORSH,TINT16,TUINT8}:ssa.OpRsh16x8,opAndTwoTypes{ORSH,TUINT16,TUINT8}:ssa.OpRsh16Ux8,opAndTwoTypes{ORSH,TINT16,TUINT16}:ssa.OpRsh16x16,opAndTwoTypes{ORSH,TUINT16,TUINT16}:ssa.OpRsh16Ux16,opAndTwoTypes{ORSH,TINT16,TUINT32}:ssa.OpRsh16x32,opAndTwoTypes{ORSH,TUINT16,TUINT32}:ssa.OpRsh16Ux32,opAndTwoTypes{ORSH,TINT16,TUINT64}:ssa.OpRsh16x64,opAndTwoTypes{ORSH,TUINT16,TUINT64}:ssa.OpRsh16Ux64,opAndTwoTypes{ORSH,TINT32,TUINT8}:ssa.OpRsh32x8,opAndTwoTypes{ORSH,TUINT32,TUINT8}:ssa.OpRsh32Ux8,opAndTwoTypes{ORSH,TINT32,TUINT16}:ssa.OpRsh32x16,opAndTwoTypes{ORSH,TUINT32,TUINT16}:ssa.OpRsh32Ux16,opAndTwoTypes{ORSH,TINT32,TUINT32}:ssa.OpRsh32x32,opAndTwoTypes{ORSH,TUINT32,TUINT32}:ssa.OpRsh32Ux32,opAndTwoTypes{ORSH,TINT32,TUINT64}:ssa.OpRsh32x64,opAndTwoTypes{ORSH,TUINT32,TUINT64}:ssa.OpRsh32Ux64,opAndTwoTypes{ORSH,TINT64,TUINT8}:ssa.OpRsh64x8,opAndTwoTypes{ORSH,TUINT64,TUINT8}:ssa.OpRsh64Ux8,opAndTwoTypes{ORSH,TINT64,TUINT16}:ssa.OpRsh64x16,opAndTwoTypes{ORSH,TUINT64,TUINT16}:ssa.OpRsh64Ux16,opAndTwoTypes{ORSH,TINT64,TUINT32}:ssa.OpRsh64x32,opAndTwoTypes{ORSH,TUINT64,TUINT32}:ssa.OpRsh64Ux32,opAndTwoTypes{ORSH,TINT64,TUINT64}:ssa.OpRsh64x64,opAndTwoTypes{ORSH,TUINT64,TUINT64}:ssa.OpRsh64Ux64,};

        private static ssa.Op ssaShiftOp(this ref state s, Op op, ref types.Type t, ref types.Type u)
        {
            var etype1 = s.concreteEtype(t);
            var etype2 = s.concreteEtype(u);
            var (x, ok) = shiftOpToSSA[new opAndTwoTypes(op,etype1,etype2)];
            if (!ok)
            {
                s.Fatalf("unhandled shift op %v etype=%s/%s", op, etype1, etype2);
            }
            return x;
        }

        // expr converts the expression n to ssa, adds it to s and returns the ssa result.
        private static ref ssa.Value expr(this ref state _s, ref Node _n) => func(_s, _n, (ref state s, ref Node n, Defer defer, Panic _, Recover __) =>
        {
            if (!(n.Op == ONAME || n.Op == OLITERAL && n.Sym != null))
            { 
                // ONAMEs and named OLITERALs have the line number
                // of the decl, not the use. See issue 14742.
                s.pushLine(n.Pos);
                defer(s.popLine());
            }
            s.stmtList(n.Ninit);

            if (n.Op == OARRAYBYTESTRTMP)
            {
                var slice = s.expr(n.Left);
                var ptr = s.newValue1(ssa.OpSlicePtr, s.f.Config.Types.BytePtr, slice);
                var len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], slice);
                return s.newValue2(ssa.OpStringMake, n.Type, ptr, len);
                goto __switch_break1;
            }
            if (n.Op == OSTRARRAYBYTETMP)
            {
                var str = s.expr(n.Left);
                ptr = s.newValue1(ssa.OpStringPtr, s.f.Config.Types.BytePtr, str);
                len = s.newValue1(ssa.OpStringLen, types.Types[TINT], str);
                return s.newValue3(ssa.OpSliceMake, n.Type, ptr, len, len);
                goto __switch_break1;
            }
            if (n.Op == OCFUNC)
            {
                var aux = n.Left.Sym.Linksym();
                return s.entryNewValue1A(ssa.OpAddr, n.Type, aux, s.sb);
                goto __switch_break1;
            }
            if (n.Op == ONAME)
            {
                if (n.Class() == PFUNC)
                { 
                    // "value" of a function is the address of the function's closure
                    var sym = funcsym(n.Sym).Linksym();
                    return s.entryNewValue1A(ssa.OpAddr, types.NewPtr(n.Type), sym, s.sb);
                }
                if (s.canSSA(n))
                {
                    return s.variable(n, n.Type);
                }
                var addr = s.addr(n, false);
                return s.newValue2(ssa.OpLoad, n.Type, addr, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OCLOSUREVAR)
            {
                addr = s.addr(n, false);
                return s.newValue2(ssa.OpLoad, n.Type, addr, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OLITERAL)
            {
                switch (n.Val().U.type())
                {
                    case ref Mpint u:
                        var i = u.Int64();
                        switch (n.Type.Size())
                        {
                            case 1L: 
                                return s.constInt8(n.Type, int8(i));
                                break;
                            case 2L: 
                                return s.constInt16(n.Type, int16(i));
                                break;
                            case 4L: 
                                return s.constInt32(n.Type, int32(i));
                                break;
                            case 8L: 
                                return s.constInt64(n.Type, i);
                                break;
                            default: 
                                s.Fatalf("bad integer size %d", n.Type.Size());
                                return null;
                                break;
                        }
                        break;
                    case @string u:
                        if (u == "")
                        {
                            return s.constEmptyString(n.Type);
                        }
                        return s.entryNewValue0A(ssa.OpConstString, n.Type, u);
                        break;
                    case bool u:
                        return s.constBool(u);
                        break;
                    case ref NilVal u:
                        var t = n.Type;

                        if (t.IsSlice()) 
                            return s.constSlice(t);
                        else if (t.IsInterface()) 
                            return s.constInterface(t);
                        else 
                            return s.constNil(t);
                                                break;
                    case ref Mpflt u:
                        switch (n.Type.Size())
                        {
                            case 4L: 
                                return s.constFloat32(n.Type, u.Float32());
                                break;
                            case 8L: 
                                return s.constFloat64(n.Type, u.Float64());
                                break;
                            default: 
                                s.Fatalf("bad float size %d", n.Type.Size());
                                return null;
                                break;
                        }
                        break;
                    case ref Mpcplx u:
                        var r = ref u.Real;
                        i = ref u.Imag;
                        switch (n.Type.Size())
                        {
                            case 8L: 
                                var pt = types.Types[TFLOAT32];
                                return s.newValue2(ssa.OpComplexMake, n.Type, s.constFloat32(pt, r.Float32()), s.constFloat32(pt, i.Float32()));
                                break;
                            case 16L: 
                                pt = types.Types[TFLOAT64];
                                return s.newValue2(ssa.OpComplexMake, n.Type, s.constFloat64(pt, r.Float64()), s.constFloat64(pt, i.Float64()));
                                break;
                            default: 
                                s.Fatalf("bad float size %d", n.Type.Size());
                                return null;
                                break;
                        }
                        break;
                    default:
                    {
                        var u = n.Val().U.type();
                        s.Fatalf("unhandled OLITERAL %v", n.Val().Ctype());
                        return null;
                        break;
                    }
                }
                goto __switch_break1;
            }
            if (n.Op == OCONVNOP)
            {
                var to = n.Type;
                var from = n.Left.Type; 

                // Assume everything will work out, so set up our return value.
                // Anything interesting that happens from here is a fatal.
                var x = s.expr(n.Left); 

                // Special case for not confusing GC and liveness.
                // We don't want pointers accidentally classified
                // as not-pointers or vice-versa because of copy
                // elision.
                if (to.IsPtrShaped() != from.IsPtrShaped())
                {
                    return s.newValue2(ssa.OpConvert, to, x, s.mem());
                }
                var v = s.newValue1(ssa.OpCopy, to, x); // ensure that v has the right type

                // CONVNOP closure
                if (to.Etype == TFUNC && from.IsPtrShaped())
                {
                    return v;
                } 

                // named <--> unnamed type or typed <--> untyped const
                if (from.Etype == to.Etype)
                {
                    return v;
                } 

                // unsafe.Pointer <--> *T
                if (to.Etype == TUNSAFEPTR && from.IsPtr() || from.Etype == TUNSAFEPTR && to.IsPtr())
                {
                    return v;
                } 

                // map <--> *hmap
                if (to.Etype == TMAP && from.IsPtr() && to.MapType().Hmap == from.Elem())
                {
                    return v;
                }
                dowidth(from);
                dowidth(to);
                if (from.Width != to.Width)
                {
                    s.Fatalf("CONVNOP width mismatch %v (%d) -> %v (%d)\n", from, from.Width, to, to.Width);
                    return null;
                }
                if (etypesign(from.Etype) != etypesign(to.Etype))
                {
                    s.Fatalf("CONVNOP sign mismatch %v (%s) -> %v (%s)\n", from, from.Etype, to, to.Etype);
                    return null;
                }
                if (instrumenting)
                { 
                    // These appear to be fine, but they fail the
                    // integer constraint below, so okay them here.
                    // Sample non-integer conversion: map[string]string -> *uint8
                    return v;
                }
                if (etypesign(from.Etype) == 0L)
                {
                    s.Fatalf("CONVNOP unrecognized non-integer %v -> %v\n", from, to);
                    return null;
                } 

                // integer, same width, same sign
                return v;
                goto __switch_break1;
            }
            if (n.Op == OCONV)
            {
                x = s.expr(n.Left);
                var ft = n.Left.Type; // from type
                var tt = n.Type; // to type
                if (ft.IsBoolean() && tt.IsKind(TUINT8))
                { 
                    // Bool -> uint8 is generated internally when indexing into runtime.staticbyte.
                    return s.newValue1(ssa.OpCopy, n.Type, x);
                }
                if (ft.IsInteger() && tt.IsInteger())
                {
                    ssa.Op op = default;
                    if (tt.Size() == ft.Size())
                    {
                        op = ssa.OpCopy;
                    }
                    else if (tt.Size() < ft.Size())
                    { 
                        // truncation
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 21L: 
                                op = ssa.OpTrunc16to8;
                                break;
                            case 41L: 
                                op = ssa.OpTrunc32to8;
                                break;
                            case 42L: 
                                op = ssa.OpTrunc32to16;
                                break;
                            case 81L: 
                                op = ssa.OpTrunc64to8;
                                break;
                            case 82L: 
                                op = ssa.OpTrunc64to16;
                                break;
                            case 84L: 
                                op = ssa.OpTrunc64to32;
                                break;
                            default: 
                                s.Fatalf("weird integer truncation %v -> %v", ft, tt);
                                break;
                        }
                    }
                    else if (ft.IsSigned())
                    { 
                        // sign extension
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 12L: 
                                op = ssa.OpSignExt8to16;
                                break;
                            case 14L: 
                                op = ssa.OpSignExt8to32;
                                break;
                            case 18L: 
                                op = ssa.OpSignExt8to64;
                                break;
                            case 24L: 
                                op = ssa.OpSignExt16to32;
                                break;
                            case 28L: 
                                op = ssa.OpSignExt16to64;
                                break;
                            case 48L: 
                                op = ssa.OpSignExt32to64;
                                break;
                            default: 
                                s.Fatalf("bad integer sign extension %v -> %v", ft, tt);
                                break;
                        }
                    }
                    else
                    { 
                        // zero extension
                        switch (10L * ft.Size() + tt.Size())
                        {
                            case 12L: 
                                op = ssa.OpZeroExt8to16;
                                break;
                            case 14L: 
                                op = ssa.OpZeroExt8to32;
                                break;
                            case 18L: 
                                op = ssa.OpZeroExt8to64;
                                break;
                            case 24L: 
                                op = ssa.OpZeroExt16to32;
                                break;
                            case 28L: 
                                op = ssa.OpZeroExt16to64;
                                break;
                            case 48L: 
                                op = ssa.OpZeroExt32to64;
                                break;
                            default: 
                                s.Fatalf("weird integer sign extension %v -> %v", ft, tt);
                                break;
                        }
                    }
                    return s.newValue1(op, n.Type, x);
                }
                if (ft.IsFloat() || tt.IsFloat())
                {
                    var (conv, ok) = fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];
                    if (s.config.RegSize == 4L && thearch.LinkArch.Family != sys.MIPS && !s.softFloat)
                    {
                        {
                            var conv1__prev3 = conv1;

                            var (conv1, ok1) = fpConvOpToSSA32[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                            if (ok1)
                            {
                                conv = conv1;
                            }

                            conv1 = conv1__prev3;

                        }
                    }
                    if (thearch.LinkArch.Family == sys.ARM64 || s.softFloat)
                    {
                        {
                            var conv1__prev3 = conv1;

                            (conv1, ok1) = uint64fpConvOpToSSA[new twoTypes(s.concreteEtype(ft),s.concreteEtype(tt))];

                            if (ok1)
                            {
                                conv = conv1;
                            }

                            conv1 = conv1__prev3;

                        }
                    }
                    if (thearch.LinkArch.Family == sys.MIPS && !s.softFloat)
                    {
                        if (ft.Size() == 4L && ft.IsInteger() && !ft.IsSigned())
                        { 
                            // tt is float32 or float64, and ft is also unsigned
                            if (tt.Size() == 4L)
                            {
                                return s.uint32Tofloat32(n, x, ft, tt);
                            }
                            if (tt.Size() == 8L)
                            {
                                return s.uint32Tofloat64(n, x, ft, tt);
                            }
                        }
                        else if (tt.Size() == 4L && tt.IsInteger() && !tt.IsSigned())
                        { 
                            // ft is float32 or float64, and tt is unsigned integer
                            if (ft.Size() == 4L)
                            {
                                return s.float32ToUint32(n, x, ft, tt);
                            }
                            if (ft.Size() == 8L)
                            {
                                return s.float64ToUint32(n, x, ft, tt);
                            }
                        }
                    }
                    if (!ok)
                    {
                        s.Fatalf("weird float conversion %v -> %v", ft, tt);
                    }
                    var op1 = conv.op1;
                    var op2 = conv.op2;
                    var it = conv.intermediateType;

                    if (op1 != ssa.OpInvalid && op2 != ssa.OpInvalid)
                    { 
                        // normal case, not tripping over unsigned 64
                        if (op1 == ssa.OpCopy)
                        {
                            if (op2 == ssa.OpCopy)
                            {
                                return x;
                            }
                            return s.newValueOrSfCall1(op2, n.Type, x);
                        }
                        if (op2 == ssa.OpCopy)
                        {
                            return s.newValueOrSfCall1(op1, n.Type, x);
                        }
                        return s.newValueOrSfCall1(op2, n.Type, s.newValueOrSfCall1(op1, types.Types[it], x));
                    } 
                    // Tricky 64-bit unsigned cases.
                    if (ft.IsInteger())
                    { 
                        // tt is float32 or float64, and ft is also unsigned
                        if (tt.Size() == 4L)
                        {
                            return s.uint64Tofloat32(n, x, ft, tt);
                        }
                        if (tt.Size() == 8L)
                        {
                            return s.uint64Tofloat64(n, x, ft, tt);
                        }
                        s.Fatalf("weird unsigned integer to float conversion %v -> %v", ft, tt);
                    } 
                    // ft is float32 or float64, and tt is unsigned integer
                    if (ft.Size() == 4L)
                    {
                        return s.float32ToUint64(n, x, ft, tt);
                    }
                    if (ft.Size() == 8L)
                    {
                        return s.float64ToUint64(n, x, ft, tt);
                    }
                    s.Fatalf("weird float to unsigned integer conversion %v -> %v", ft, tt);
                    return null;
                }
                if (ft.IsComplex() && tt.IsComplex())
                {
                    op = default;
                    if (ft.Size() == tt.Size())
                    {
                        switch (ft.Size())
                        {
                            case 8L: 
                                op = ssa.OpRound32F;
                                break;
                            case 16L: 
                                op = ssa.OpRound64F;
                                break;
                            default: 
                                s.Fatalf("weird complex conversion %v -> %v", ft, tt);
                                break;
                        }
                    }
                    else if (ft.Size() == 8L && tt.Size() == 16L)
                    {
                        op = ssa.OpCvt32Fto64F;
                    }
                    else if (ft.Size() == 16L && tt.Size() == 8L)
                    {
                        op = ssa.OpCvt64Fto32F;
                    }
                    else
                    {
                        s.Fatalf("weird complex conversion %v -> %v", ft, tt);
                    }
                    var ftp = floatForComplex(ft);
                    var ttp = floatForComplex(tt);
                    return s.newValue2(ssa.OpComplexMake, tt, s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexReal, ftp, x)), s.newValueOrSfCall1(op, ttp, s.newValue1(ssa.OpComplexImag, ftp, x)));
                }
                s.Fatalf("unhandled OCONV %s -> %s", n.Left.Type.Etype, n.Type.Etype);
                return null;
                goto __switch_break1;
            }
            if (n.Op == ODOTTYPE)
            {
                var (res, _) = s.dottype(n, false);
                return res; 

                // binary ops
                goto __switch_break1;
            }
            if (n.Op == OLT || n.Op == OEQ || n.Op == ONE || n.Op == OLE || n.Op == OGE || n.Op == OGT)
            {
                var a = s.expr(n.Left);
                var b = s.expr(n.Right);
                if (n.Left.Type.IsComplex())
                {
                    pt = floatForComplex(n.Left.Type);
                    op = s.ssaOp(OEQ, pt);
                    r = s.newValueOrSfCall2(op, types.Types[TBOOL], s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b));
                    i = s.newValueOrSfCall2(op, types.Types[TBOOL], s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b));
                    var c = s.newValue2(ssa.OpAndB, types.Types[TBOOL], r, i);

                    if (n.Op == OEQ) 
                        return c;
                    else if (n.Op == ONE) 
                        return s.newValue1(ssa.OpNot, types.Types[TBOOL], c);
                    else 
                        s.Fatalf("ordered complex compare %v", n.Op);
                                    }
                if (n.Left.Type.IsFloat())
                {
                    return s.newValueOrSfCall2(s.ssaOp(n.Op, n.Left.Type), types.Types[TBOOL], a, b);
                }
                return s.newValue2(s.ssaOp(n.Op, n.Left.Type), types.Types[TBOOL], a, b);
                goto __switch_break1;
            }
            if (n.Op == OMUL)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                {
                    var mulop = ssa.OpMul64F;
                    var addop = ssa.OpAdd64F;
                    var subop = ssa.OpSub64F;
                    pt = floatForComplex(n.Type); // Could be Float32 or Float64
                    var wt = types.Types[TFLOAT64]; // Compute in Float64 to minimize cancelation error

                    var areal = s.newValue1(ssa.OpComplexReal, pt, a);
                    var breal = s.newValue1(ssa.OpComplexReal, pt, b);
                    var aimag = s.newValue1(ssa.OpComplexImag, pt, a);
                    var bimag = s.newValue1(ssa.OpComplexImag, pt, b);

                    if (pt != wt)
                    { // Widen for calculation
                        areal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, areal);
                        breal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, breal);
                        aimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, aimag);
                        bimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, bimag);
                    }
                    var xreal = s.newValueOrSfCall2(subop, wt, s.newValueOrSfCall2(mulop, wt, areal, breal), s.newValueOrSfCall2(mulop, wt, aimag, bimag));
                    var ximag = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, areal, bimag), s.newValueOrSfCall2(mulop, wt, aimag, breal));

                    if (pt != wt)
                    { // Narrow to store back
                        xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                        ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);
                    }
                    return s.newValue2(ssa.OpComplexMake, n.Type, xreal, ximag);
                }
                if (n.Type.IsFloat())
                {
                    return s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                }
                return s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                goto __switch_break1;
            }
            if (n.Op == ODIV)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                { 
                    // TODO this is not executed because the front-end substitutes a runtime call.
                    // That probably ought to change; with modest optimization the widen/narrow
                    // conversions could all be elided in larger expression trees.
                    mulop = ssa.OpMul64F;
                    addop = ssa.OpAdd64F;
                    subop = ssa.OpSub64F;
                    var divop = ssa.OpDiv64F;
                    pt = floatForComplex(n.Type); // Could be Float32 or Float64
                    wt = types.Types[TFLOAT64]; // Compute in Float64 to minimize cancelation error

                    areal = s.newValue1(ssa.OpComplexReal, pt, a);
                    breal = s.newValue1(ssa.OpComplexReal, pt, b);
                    aimag = s.newValue1(ssa.OpComplexImag, pt, a);
                    bimag = s.newValue1(ssa.OpComplexImag, pt, b);

                    if (pt != wt)
                    { // Widen for calculation
                        areal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, areal);
                        breal = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, breal);
                        aimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, aimag);
                        bimag = s.newValueOrSfCall1(ssa.OpCvt32Fto64F, wt, bimag);
                    }
                    var denom = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, breal, breal), s.newValueOrSfCall2(mulop, wt, bimag, bimag));
                    xreal = s.newValueOrSfCall2(addop, wt, s.newValueOrSfCall2(mulop, wt, areal, breal), s.newValueOrSfCall2(mulop, wt, aimag, bimag));
                    ximag = s.newValueOrSfCall2(subop, wt, s.newValueOrSfCall2(mulop, wt, aimag, breal), s.newValueOrSfCall2(mulop, wt, areal, bimag)); 

                    // TODO not sure if this is best done in wide precision or narrow
                    // Double-rounding might be an issue.
                    // Note that the pre-SSA implementation does the entire calculation
                    // in wide format, so wide is compatible.
                    xreal = s.newValueOrSfCall2(divop, wt, xreal, denom);
                    ximag = s.newValueOrSfCall2(divop, wt, ximag, denom);

                    if (pt != wt)
                    { // Narrow to store back
                        xreal = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, xreal);
                        ximag = s.newValueOrSfCall1(ssa.OpCvt64Fto32F, pt, ximag);
                    }
                    return s.newValue2(ssa.OpComplexMake, n.Type, xreal, ximag);
                }
                if (n.Type.IsFloat())
                {
                    return s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                }
                return s.intDivide(n, a, b);
                goto __switch_break1;
            }
            if (n.Op == OMOD)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                return s.intDivide(n, a, b);
                goto __switch_break1;
            }
            if (n.Op == OADD || n.Op == OSUB)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                if (n.Type.IsComplex())
                {
                    pt = floatForComplex(n.Type);
                    op = s.ssaOp(n.Op, pt);
                    return s.newValue2(ssa.OpComplexMake, n.Type, s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexReal, pt, a), s.newValue1(ssa.OpComplexReal, pt, b)), s.newValueOrSfCall2(op, pt, s.newValue1(ssa.OpComplexImag, pt, a), s.newValue1(ssa.OpComplexImag, pt, b)));
                }
                if (n.Type.IsFloat())
                {
                    return s.newValueOrSfCall2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                }
                return s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                goto __switch_break1;
            }
            if (n.Op == OAND || n.Op == OOR || n.Op == OXOR)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                return s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
                goto __switch_break1;
            }
            if (n.Op == OLSH || n.Op == ORSH)
            {
                a = s.expr(n.Left);
                b = s.expr(n.Right);
                return s.newValue2(s.ssaShiftOp(n.Op, n.Type, n.Right.Type), a.Type, a, b);
                goto __switch_break1;
            }
            if (n.Op == OANDAND || n.Op == OOROR) 
            {
                // To implement OANDAND (and OOROR), we introduce a
                // new temporary variable to hold the result. The
                // variable is associated with the OANDAND node in the
                // s.vars table (normally variables are only
                // associated with ONAME nodes). We convert
                //     A && B
                // to
                //     var = A
                //     if var {
                //         var = B
                //     }
                // Using var in the subsequent block introduces the
                // necessary phi variable.
                var el = s.expr(n.Left);
                s.vars[n] = el;

                b = s.endBlock();
                b.Kind = ssa.BlockIf;
                b.SetControl(el); 
                // In theory, we should set b.Likely here based on context.
                // However, gc only gives us likeliness hints
                // in a single place, for plain OIF statements,
                // and passing around context is finnicky, so don't bother for now.

                var bRight = s.f.NewBlock(ssa.BlockPlain);
                var bResult = s.f.NewBlock(ssa.BlockPlain);
                if (n.Op == OANDAND)
                {
                    b.AddEdgeTo(bRight);
                    b.AddEdgeTo(bResult);
                }
                else if (n.Op == OOROR)
                {
                    b.AddEdgeTo(bResult);
                    b.AddEdgeTo(bRight);
                }
                s.startBlock(bRight);
                var er = s.expr(n.Right);
                s.vars[n] = er;

                b = s.endBlock();
                b.AddEdgeTo(bResult);

                s.startBlock(bResult);
                return s.variable(n, types.Types[TBOOL]);
                goto __switch_break1;
            }
            if (n.Op == OCOMPLEX)
            {
                r = s.expr(n.Left);
                i = s.expr(n.Right);
                return s.newValue2(ssa.OpComplexMake, n.Type, r, i); 

                // unary ops
                goto __switch_break1;
            }
            if (n.Op == OMINUS)
            {
                a = s.expr(n.Left);
                if (n.Type.IsComplex())
                {
                    var tp = floatForComplex(n.Type);
                    var negop = s.ssaOp(n.Op, tp);
                    return s.newValue2(ssa.OpComplexMake, n.Type, s.newValue1(negop, tp, s.newValue1(ssa.OpComplexReal, tp, a)), s.newValue1(negop, tp, s.newValue1(ssa.OpComplexImag, tp, a)));
                }
                return s.newValue1(s.ssaOp(n.Op, n.Type), a.Type, a);
                goto __switch_break1;
            }
            if (n.Op == ONOT || n.Op == OCOM)
            {
                a = s.expr(n.Left);
                return s.newValue1(s.ssaOp(n.Op, n.Type), a.Type, a);
                goto __switch_break1;
            }
            if (n.Op == OIMAG || n.Op == OREAL)
            {
                a = s.expr(n.Left);
                return s.newValue1(s.ssaOp(n.Op, n.Left.Type), n.Type, a);
                goto __switch_break1;
            }
            if (n.Op == OPLUS)
            {
                return s.expr(n.Left);
                goto __switch_break1;
            }
            if (n.Op == OADDR)
            {
                return s.addr(n.Left, n.Bounded());
                goto __switch_break1;
            }
            if (n.Op == OINDREGSP)
            {
                addr = s.constOffPtrSP(types.NewPtr(n.Type), n.Xoffset);
                return s.newValue2(ssa.OpLoad, n.Type, addr, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OIND)
            {
                var p = s.exprPtr(n.Left, false, n.Pos);
                return s.newValue2(ssa.OpLoad, n.Type, p, s.mem());
                goto __switch_break1;
            }
            if (n.Op == ODOT)
            {
                t = n.Left.Type;
                if (canSSAType(t))
                {
                    v = s.expr(n.Left);
                    return s.newValue1I(ssa.OpStructSelect, n.Type, int64(fieldIdx(n)), v);
                }
                if (n.Left.Op == OSTRUCTLIT)
                { 
                    // All literals with nonzero fields have already been
                    // rewritten during walk. Any that remain are just T{}
                    // or equivalents. Use the zero value.
                    if (!iszero(n.Left))
                    {
                        Fatalf("literal with nonzero value in SSA: %v", n.Left);
                    }
                    return s.zeroVal(n.Type);
                }
                p = s.addr(n, false);
                return s.newValue2(ssa.OpLoad, n.Type, p, s.mem());
                goto __switch_break1;
            }
            if (n.Op == ODOTPTR)
            {
                p = s.exprPtr(n.Left, false, n.Pos);
                p = s.newValue1I(ssa.OpOffPtr, types.NewPtr(n.Type), n.Xoffset, p);
                return s.newValue2(ssa.OpLoad, n.Type, p, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OINDEX)
            {

                if (n.Left.Type.IsString()) 
                    if (n.Bounded() && Isconst(n.Left, CTSTR) && Isconst(n.Right, CTINT))
                    { 
                        // Replace "abc"[1] with 'b'.
                        // Delayed until now because "abc"[1] is not an ideal constant.
                        // See test/fixedbugs/issue11370.go.
                        return s.newValue0I(ssa.OpConst8, types.Types[TUINT8], int64(int8(n.Left.Val().U._<@string>()[n.Right.Int64()])));
                    }
                    a = s.expr(n.Left);
                    i = s.expr(n.Right);
                    i = s.extendIndex(i, panicindex);
                    if (!n.Bounded())
                    {
                        len = s.newValue1(ssa.OpStringLen, types.Types[TINT], a);
                        s.boundsCheck(i, len);
                    }
                    var ptrtyp = s.f.Config.Types.BytePtr;
                    ptr = s.newValue1(ssa.OpStringPtr, ptrtyp, a);
                    if (Isconst(n.Right, CTINT))
                    {
                        ptr = s.newValue1I(ssa.OpOffPtr, ptrtyp, n.Right.Int64(), ptr);
                    }
                    else
                    {
                        ptr = s.newValue2(ssa.OpAddPtr, ptrtyp, ptr, i);
                    }
                    return s.newValue2(ssa.OpLoad, types.Types[TUINT8], ptr, s.mem());
                else if (n.Left.Type.IsSlice()) 
                    p = s.addr(n, false);
                    return s.newValue2(ssa.OpLoad, n.Left.Type.Elem(), p, s.mem());
                else if (n.Left.Type.IsArray()) 
                    {
                        var bound = n.Left.Type.NumElem();

                        if (bound <= 1L)
                        { 
                            // SSA can handle arrays of length at most 1.
                            a = s.expr(n.Left);
                            i = s.expr(n.Right);
                            if (bound == 0L)
                            { 
                                // Bounds check will never succeed.  Might as well
                                // use constants for the bounds check.
                                var z = s.constInt(types.Types[TINT], 0L);
                                s.boundsCheck(z, z); 
                                // The return value won't be live, return junk.
                                return s.newValue0(ssa.OpUnknown, n.Type);
                            }
                            i = s.extendIndex(i, panicindex);
                            if (!n.Bounded())
                            {
                                s.boundsCheck(i, s.constInt(types.Types[TINT], bound));
                            }
                            return s.newValue1I(ssa.OpArraySelect, n.Type, 0L, a);
                        }

                    }
                    p = s.addr(n, false);
                    return s.newValue2(ssa.OpLoad, n.Left.Type.Elem(), p, s.mem());
                else 
                    s.Fatalf("bad type for index %v", n.Left.Type);
                    return null;
                                goto __switch_break1;
            }
            if (n.Op == OLEN || n.Op == OCAP)
            {

                if (n.Left.Type.IsSlice()) 
                    op = ssa.OpSliceLen;
                    if (n.Op == OCAP)
                    {
                        op = ssa.OpSliceCap;
                    }
                    return s.newValue1(op, types.Types[TINT], s.expr(n.Left));
                else if (n.Left.Type.IsString()) // string; not reachable for OCAP
                    return s.newValue1(ssa.OpStringLen, types.Types[TINT], s.expr(n.Left));
                else if (n.Left.Type.IsMap() || n.Left.Type.IsChan()) 
                    return s.referenceTypeBuiltin(n, s.expr(n.Left));
                else // array
                    return s.constInt(types.Types[TINT], n.Left.Type.NumElem());
                                goto __switch_break1;
            }
            if (n.Op == OSPTR)
            {
                a = s.expr(n.Left);
                if (n.Left.Type.IsSlice())
                {
                    return s.newValue1(ssa.OpSlicePtr, n.Type, a);
                }
                else
                {
                    return s.newValue1(ssa.OpStringPtr, n.Type, a);
                }
                goto __switch_break1;
            }
            if (n.Op == OITAB)
            {
                a = s.expr(n.Left);
                return s.newValue1(ssa.OpITab, n.Type, a);
                goto __switch_break1;
            }
            if (n.Op == OIDATA)
            {
                a = s.expr(n.Left);
                return s.newValue1(ssa.OpIData, n.Type, a);
                goto __switch_break1;
            }
            if (n.Op == OEFACE)
            {
                var tab = s.expr(n.Left);
                var data = s.expr(n.Right);
                return s.newValue2(ssa.OpIMake, n.Type, tab, data);
                goto __switch_break1;
            }
            if (n.Op == OSLICE || n.Op == OSLICEARR || n.Op == OSLICE3 || n.Op == OSLICE3ARR)
            {
                v = s.expr(n.Left);
                i = default;                ref ssa.Value j = default;                ref ssa.Value k = default;

                var (low, high, max) = n.SliceBounds();
                if (low != null)
                {
                    i = s.extendIndex(s.expr(low), panicslice);
                }
                if (high != null)
                {
                    j = s.extendIndex(s.expr(high), panicslice);
                }
                if (max != null)
                {
                    k = s.extendIndex(s.expr(max), panicslice);
                }
                var (p, l, c) = s.slice(n.Left.Type, v, i, j, k);
                return s.newValue3(ssa.OpSliceMake, n.Type, p, l, c);
                goto __switch_break1;
            }
            if (n.Op == OSLICESTR)
            {
                v = s.expr(n.Left);
                i = default;                j = default;

                var (low, high, _) = n.SliceBounds();
                if (low != null)
                {
                    i = s.extendIndex(s.expr(low), panicslice);
                }
                if (high != null)
                {
                    j = s.extendIndex(s.expr(high), panicslice);
                }
                var (p, l, _) = s.slice(n.Left.Type, v, i, j, null);
                return s.newValue2(ssa.OpStringMake, n.Type, p, l);
                goto __switch_break1;
            }
            if (n.Op == OCALLFUNC)
            {
                if (isIntrinsicCall(n))
                {
                    return s.intrinsicCall(n);
                }
                fallthrough = true;

            }
            if (fallthrough || n.Op == OCALLINTER || n.Op == OCALLMETH)
            {
                a = s.call(n, callNormal);
                return s.newValue2(ssa.OpLoad, n.Type, a, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OGETG)
            {
                return s.newValue1(ssa.OpGetG, n.Type, s.mem());
                goto __switch_break1;
            }
            if (n.Op == OAPPEND)
            {
                return s.append(n, false);
                goto __switch_break1;
            }
            if (n.Op == OSTRUCTLIT || n.Op == OARRAYLIT) 
            {
                // All literals with nonzero fields have already been
                // rewritten during walk. Any that remain are just T{}
                // or equivalents. Use the zero value.
                if (!iszero(n))
                {
                    Fatalf("literal with nonzero value in SSA: %v", n);
                }
                return s.zeroVal(n.Type);
                goto __switch_break1;
            }
            // default: 
                s.Fatalf("unhandled expr %v", n.Op);
                return null;

            __switch_break1:;
        });

        // append converts an OAPPEND node to SSA.
        // If inplace is false, it converts the OAPPEND expression n to an ssa.Value,
        // adds it to s, and returns the Value.
        // If inplace is true, it writes the result of the OAPPEND expression n
        // back to the slice being appended to, and returns nil.
        // inplace MUST be set to false if the slice can be SSA'd.
        private static ref ssa.Value append(this ref state s, ref Node n, bool inplace)
        { 
            // If inplace is false, process as expression "append(s, e1, e2, e3)":
            //
            // ptr, len, cap := s
            // newlen := len + 3
            // if newlen > cap {
            //     ptr, len, cap = growslice(s, newlen)
            //     newlen = len + 3 // recalculate to avoid a spill
            // }
            // // with write barriers, if needed:
            // *(ptr+len) = e1
            // *(ptr+len+1) = e2
            // *(ptr+len+2) = e3
            // return makeslice(ptr, newlen, cap)
            //
            //
            // If inplace is true, process as statement "s = append(s, e1, e2, e3)":
            //
            // a := &s
            // ptr, len, cap := s
            // newlen := len + 3
            // if newlen > cap {
            //    newptr, len, newcap = growslice(ptr, len, cap, newlen)
            //    vardef(a)       // if necessary, advise liveness we are writing a new a
            //    *a.cap = newcap // write before ptr to avoid a spill
            //    *a.ptr = newptr // with write barrier
            // }
            // newlen = len + 3 // recalculate to avoid a spill
            // *a.len = newlen
            // // with write barriers, if needed:
            // *(ptr+len) = e1
            // *(ptr+len+1) = e2
            // *(ptr+len+2) = e3

            var et = n.Type.Elem();
            var pt = types.NewPtr(et); 

            // Evaluate slice
            var sn = n.List.First(); // the slice node is the first in the list

            ref ssa.Value slice = default;            ref ssa.Value addr = default;

            if (inplace)
            {
                addr = s.addr(sn, false);
                slice = s.newValue2(ssa.OpLoad, n.Type, addr, s.mem());
            }
            else
            {
                slice = s.expr(sn);
            } 

            // Allocate new blocks
            var grow = s.f.NewBlock(ssa.BlockPlain);
            var assign = s.f.NewBlock(ssa.BlockPlain); 

            // Decide if we need to grow
            var nargs = int64(n.List.Len() - 1L);
            var p = s.newValue1(ssa.OpSlicePtr, pt, slice);
            var l = s.newValue1(ssa.OpSliceLen, types.Types[TINT], slice);
            var c = s.newValue1(ssa.OpSliceCap, types.Types[TINT], slice);
            var nl = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], l, s.constInt(types.Types[TINT], nargs));

            var cmp = s.newValue2(s.ssaOp(OGT, types.Types[TINT]), types.Types[TBOOL], nl, c);
            s.vars[ref ptrVar] = p;

            if (!inplace)
            {
                s.vars[ref newlenVar] = nl;
                s.vars[ref capVar] = c;
            }
            else
            {
                s.vars[ref lenVar] = l;
            }
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.Likely = ssa.BranchUnlikely;
            b.SetControl(cmp);
            b.AddEdgeTo(grow);
            b.AddEdgeTo(assign); 

            // Call growslice
            s.startBlock(grow);
            var taddr = s.expr(n.Left);
            var r = s.rtcall(growslice, true, new slice<ref types.Type>(new ref types.Type[] { pt, types.Types[TINT], types.Types[TINT] }), taddr, p, l, c, nl);

            if (inplace)
            {
                if (sn.Op == ONAME && sn.Class() != PEXTERN)
                { 
                    // Tell liveness we're about to build a new slice
                    s.vars[ref memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, sn, s.mem());
                }
                var capaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, int64(array_cap), addr);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TINT], capaddr, r[2L], s.mem());
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, pt, addr, r[0L], s.mem()); 
                // load the value we just stored to avoid having to spill it
                s.vars[ref ptrVar] = s.newValue2(ssa.OpLoad, pt, addr, s.mem());
                s.vars[ref lenVar] = r[1L]; // avoid a spill in the fast path
            }
            else
            {
                s.vars[ref ptrVar] = r[0L];
                s.vars[ref newlenVar] = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], r[1L], s.constInt(types.Types[TINT], nargs));
                s.vars[ref capVar] = r[2L];
            }
            b = s.endBlock();
            b.AddEdgeTo(assign); 

            // assign new elements to slots
            s.startBlock(assign);

            if (inplace)
            {
                l = s.variable(ref lenVar, types.Types[TINT]); // generates phi for len
                nl = s.newValue2(s.ssaOp(OADD, types.Types[TINT]), types.Types[TINT], l, s.constInt(types.Types[TINT], nargs));
                var lenaddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, int64(array_nel), addr);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TINT], lenaddr, nl, s.mem());
            } 

            // Evaluate args
            private partial struct argRec
            {
                public ptr<ssa.Value> v;
                public bool store;
            }
            var args = make_slice<argRec>(0L, nargs);
            foreach (var (_, n) in n.List.Slice()[1L..])
            {
                if (canSSAType(n.Type))
                {
                    args = append(args, new argRec(v:s.expr(n),store:true));
                }
                else
                {
                    var v = s.addr(n, false);
                    args = append(args, new argRec(v:v));
                }
            }
            p = s.variable(ref ptrVar, pt); // generates phi for ptr
            if (!inplace)
            {
                nl = s.variable(ref newlenVar, types.Types[TINT]); // generates phi for nl
                c = s.variable(ref capVar, types.Types[TINT]); // generates phi for cap
            }
            var p2 = s.newValue2(ssa.OpPtrIndex, pt, p, l);
            foreach (var (i, arg) in args)
            {
                addr = s.newValue2(ssa.OpPtrIndex, pt, p2, s.constInt(types.Types[TINT], int64(i)));
                if (arg.store)
                {
                    s.storeType(et, addr, arg.v, 0L);
                }
                else
                {
                    var store = s.newValue3I(ssa.OpMove, types.TypeMem, et.Size(), addr, arg.v, s.mem());
                    store.Aux = et;
                    s.vars[ref memVar] = store;
                }
            }
            delete(s.vars, ref ptrVar);
            if (inplace)
            {
                delete(s.vars, ref lenVar);
                return null;
            }
            delete(s.vars, ref newlenVar);
            delete(s.vars, ref capVar); 
            // make result
            return s.newValue3(ssa.OpSliceMake, n.Type, p, nl, c);
        }

        // condBranch evaluates the boolean expression cond and branches to yes
        // if cond is true and no if cond is false.
        // This function is intended to handle && and || better than just calling
        // s.expr(cond) and branching on the result.
        private static void condBranch(this ref state s, ref Node cond, ref ssa.Block yes, ref ssa.Block no, sbyte likely)
        {

            if (cond.Op == OANDAND) 
                var mid = s.f.NewBlock(ssa.BlockPlain);
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, mid, no, max8(likely, 0L));
                s.startBlock(mid);
                s.condBranch(cond.Right, yes, no, likely);
                return; 
                // Note: if likely==1, then both recursive calls pass 1.
                // If likely==-1, then we don't have enough information to decide
                // whether the first branch is likely or not. So we pass 0 for
                // the likeliness of the first branch.
                // TODO: have the frontend give us branch prediction hints for
                // OANDAND and OOROR nodes (if it ever has such info).
            else if (cond.Op == OOROR) 
                mid = s.f.NewBlock(ssa.BlockPlain);
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, yes, mid, min8(likely, 0L));
                s.startBlock(mid);
                s.condBranch(cond.Right, yes, no, likely);
                return; 
                // Note: if likely==-1, then both recursive calls pass -1.
                // If likely==1, then we don't have enough info to decide
                // the likelihood of the first branch.
            else if (cond.Op == ONOT) 
                s.stmtList(cond.Ninit);
                s.condBranch(cond.Left, no, yes, -likely);
                return;
                        var c = s.expr(cond);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(c);
            b.Likely = ssa.BranchPrediction(likely); // gc and ssa both use -1/0/+1 for likeliness
            b.AddEdgeTo(yes);
            b.AddEdgeTo(no);
        }

        private partial struct skipMask // : byte
        {
        }

        private static readonly skipMask skipPtr = 1L << (int)(iota);
        private static readonly var skipLen = 0;
        private static readonly var skipCap = 1;

        // assign does left = right.
        // Right has already been evaluated to ssa, left has not.
        // If deref is true, then we do left = *right instead (and right has already been nil-checked).
        // If deref is true and right == nil, just do left = 0.
        // skip indicates assignments (at the top level) that can be avoided.
        private static void assign(this ref state s, ref Node left, ref ssa.Value right, bool deref, skipMask skip)
        {
            if (left.Op == ONAME && isblank(left))
            {
                return;
            }
            var t = left.Type;
            dowidth(t);
            if (s.canSSA(left))
            {
                if (deref)
                {
                    s.Fatalf("can SSA LHS %v but not RHS %s", left, right);
                }
                if (left.Op == ODOT)
                { 
                    // We're assigning to a field of an ssa-able value.
                    // We need to build a new structure with the new value for the
                    // field we're assigning and the old values for the other fields.
                    // For instance:
                    //   type T struct {a, b, c int}
                    //   var T x
                    //   x.b = 5
                    // For the x.b = 5 assignment we want to generate x = T{x.a, 5, x.c}

                    // Grab information about the structure type.
                    t = left.Left.Type;
                    var nf = t.NumFields();
                    var idx = fieldIdx(left); 

                    // Grab old value of structure.
                    var old = s.expr(left.Left); 

                    // Make new structure.
                    var @new = s.newValue0(ssa.StructMakeOp(t.NumFields()), t); 

                    // Add fields as args.
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < nf; i++)
                        {
                            if (i == idx)
                            {
                                @new.AddArg(right);
                            }
                            else
                            {
                                @new.AddArg(s.newValue1I(ssa.OpStructSelect, t.FieldType(i), int64(i), old));
                            }
                        } 

                        // Recursively assign the new value we've made to the base of the dot op.


                        i = i__prev1;
                    } 

                    // Recursively assign the new value we've made to the base of the dot op.
                    s.assign(left.Left, new, false, 0L); 
                    // TODO: do we need to update named values here?
                    return;
                }
                if (left.Op == OINDEX && left.Left.Type.IsArray())
                { 
                    // We're assigning to an element of an ssa-able array.
                    // a[i] = v
                    t = left.Left.Type;
                    var n = t.NumElem();

                    i = s.expr(left.Right); // index
                    if (n == 0L)
                    { 
                        // The bounds check must fail.  Might as well
                        // ignore the actual index and just use zeros.
                        var z = s.constInt(types.Types[TINT], 0L);
                        s.boundsCheck(z, z);
                        return;
                    }
                    if (n != 1L)
                    {
                        s.Fatalf("assigning to non-1-length array");
                    } 
                    // Rewrite to a = [1]{v}
                    i = s.extendIndex(i, panicindex);
                    s.boundsCheck(i, s.constInt(types.Types[TINT], 1L));
                    var v = s.newValue1(ssa.OpArrayMake1, t, right);
                    s.assign(left.Left, v, false, 0L);
                    return;
                } 
                // Update variable assignment.
                s.vars[left] = right;
                s.addNamedValue(left, right);
                return;
            } 
            // Left is not ssa-able. Compute its address.
            var addr = s.addr(left, false);
            if (left.Op == ONAME && left.Class() != PEXTERN && skip == 0L)
            {
                s.vars[ref memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, left, s.mem());
            }
            if (isReflectHeaderDataField(left))
            { 
                // Package unsafe's documentation says storing pointers into
                // reflect.SliceHeader and reflect.StringHeader's Data fields
                // is valid, even though they have type uintptr (#19168).
                // Mark it pointer type to signal the writebarrier pass to
                // insert a write barrier.
                t = types.Types[TUNSAFEPTR];
            }
            if (deref)
            { 
                // Treat as a mem->mem move.
                ref ssa.Value store = default;
                if (right == null)
                {
                    store = s.newValue2I(ssa.OpZero, types.TypeMem, t.Size(), addr, s.mem());
                }
                else
                {
                    store = s.newValue3I(ssa.OpMove, types.TypeMem, t.Size(), addr, right, s.mem());
                }
                store.Aux = t;
                s.vars[ref memVar] = store;
                return;
            } 
            // Treat as a store.
            s.storeType(t, addr, right, skip);
        }

        // zeroVal returns the zero value for type t.
        private static ref ssa.Value zeroVal(this ref state s, ref types.Type t)
        {

            if (t.IsInteger()) 
                switch (t.Size())
                {
                    case 1L: 
                        return s.constInt8(t, 0L);
                        break;
                    case 2L: 
                        return s.constInt16(t, 0L);
                        break;
                    case 4L: 
                        return s.constInt32(t, 0L);
                        break;
                    case 8L: 
                        return s.constInt64(t, 0L);
                        break;
                    default: 
                        s.Fatalf("bad sized integer type %v", t);
                        break;
                }
            else if (t.IsFloat()) 
                switch (t.Size())
                {
                    case 4L: 
                        return s.constFloat32(t, 0L);
                        break;
                    case 8L: 
                        return s.constFloat64(t, 0L);
                        break;
                    default: 
                        s.Fatalf("bad sized float type %v", t);
                        break;
                }
            else if (t.IsComplex()) 
                switch (t.Size())
                {
                    case 8L: 
                        var z = s.constFloat32(types.Types[TFLOAT32], 0L);
                        return s.entryNewValue2(ssa.OpComplexMake, t, z, z);
                        break;
                    case 16L: 
                        z = s.constFloat64(types.Types[TFLOAT64], 0L);
                        return s.entryNewValue2(ssa.OpComplexMake, t, z, z);
                        break;
                    default: 
                        s.Fatalf("bad sized complex type %v", t);
                        break;
                }
            else if (t.IsString()) 
                return s.constEmptyString(t);
            else if (t.IsPtrShaped()) 
                return s.constNil(t);
            else if (t.IsBoolean()) 
                return s.constBool(false);
            else if (t.IsInterface()) 
                return s.constInterface(t);
            else if (t.IsSlice()) 
                return s.constSlice(t);
            else if (t.IsStruct()) 
                var n = t.NumFields();
                var v = s.entryNewValue0(ssa.StructMakeOp(t.NumFields()), t);
                for (long i = 0L; i < n; i++)
                {
                    v.AddArg(s.zeroVal(t.FieldType(i)));
                }

                return v;
            else if (t.IsArray()) 
                switch (t.NumElem())
                {
                    case 0L: 
                        return s.entryNewValue0(ssa.OpArrayMake0, t);
                        break;
                    case 1L: 
                        return s.entryNewValue1(ssa.OpArrayMake1, t, s.zeroVal(t.Elem()));
                        break;
                }
                        s.Fatalf("zero for type %v not implemented", t);
            return null;
        }

        private partial struct callKind // : sbyte
        {
        }

        private static readonly callKind callNormal = iota;
        private static readonly var callDefer = 0;
        private static readonly var callGo = 1;

        private partial struct sfRtCallDef
        {
            public ptr<obj.LSym> rtfn;
            public types.EType rtype;
        }

        private static map<ssa.Op, sfRtCallDef> softFloatOps = default;

        private static void softfloatInit()
        { 
            // Some of these operations get transformed by sfcall.
            softFloatOps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ssa.Op, sfRtCallDef>{ssa.OpAdd32F:sfRtCallDef{sysfunc("fadd32"),TFLOAT32},ssa.OpAdd64F:sfRtCallDef{sysfunc("fadd64"),TFLOAT64},ssa.OpSub32F:sfRtCallDef{sysfunc("fadd32"),TFLOAT32},ssa.OpSub64F:sfRtCallDef{sysfunc("fadd64"),TFLOAT64},ssa.OpMul32F:sfRtCallDef{sysfunc("fmul32"),TFLOAT32},ssa.OpMul64F:sfRtCallDef{sysfunc("fmul64"),TFLOAT64},ssa.OpDiv32F:sfRtCallDef{sysfunc("fdiv32"),TFLOAT32},ssa.OpDiv64F:sfRtCallDef{sysfunc("fdiv64"),TFLOAT64},ssa.OpEq64F:sfRtCallDef{sysfunc("feq64"),TBOOL},ssa.OpEq32F:sfRtCallDef{sysfunc("feq32"),TBOOL},ssa.OpNeq64F:sfRtCallDef{sysfunc("feq64"),TBOOL},ssa.OpNeq32F:sfRtCallDef{sysfunc("feq32"),TBOOL},ssa.OpLess64F:sfRtCallDef{sysfunc("fgt64"),TBOOL},ssa.OpLess32F:sfRtCallDef{sysfunc("fgt32"),TBOOL},ssa.OpGreater64F:sfRtCallDef{sysfunc("fgt64"),TBOOL},ssa.OpGreater32F:sfRtCallDef{sysfunc("fgt32"),TBOOL},ssa.OpLeq64F:sfRtCallDef{sysfunc("fge64"),TBOOL},ssa.OpLeq32F:sfRtCallDef{sysfunc("fge32"),TBOOL},ssa.OpGeq64F:sfRtCallDef{sysfunc("fge64"),TBOOL},ssa.OpGeq32F:sfRtCallDef{sysfunc("fge32"),TBOOL},ssa.OpCvt32to32F:sfRtCallDef{sysfunc("fint32to32"),TFLOAT32},ssa.OpCvt32Fto32:sfRtCallDef{sysfunc("f32toint32"),TINT32},ssa.OpCvt64to32F:sfRtCallDef{sysfunc("fint64to32"),TFLOAT32},ssa.OpCvt32Fto64:sfRtCallDef{sysfunc("f32toint64"),TINT64},ssa.OpCvt64Uto32F:sfRtCallDef{sysfunc("fuint64to32"),TFLOAT32},ssa.OpCvt32Fto64U:sfRtCallDef{sysfunc("f32touint64"),TUINT64},ssa.OpCvt32to64F:sfRtCallDef{sysfunc("fint32to64"),TFLOAT64},ssa.OpCvt64Fto32:sfRtCallDef{sysfunc("f64toint32"),TINT32},ssa.OpCvt64to64F:sfRtCallDef{sysfunc("fint64to64"),TFLOAT64},ssa.OpCvt64Fto64:sfRtCallDef{sysfunc("f64toint64"),TINT64},ssa.OpCvt64Uto64F:sfRtCallDef{sysfunc("fuint64to64"),TFLOAT64},ssa.OpCvt64Fto64U:sfRtCallDef{sysfunc("f64touint64"),TUINT64},ssa.OpCvt32Fto64F:sfRtCallDef{sysfunc("f32to64"),TFLOAT64},ssa.OpCvt64Fto32F:sfRtCallDef{sysfunc("f64to32"),TFLOAT32},};
        }

        // TODO: do not emit sfcall if operation can be optimized to constant in later
        // opt phase
        private static (ref ssa.Value, bool) sfcall(this ref state s, ssa.Op op, params ptr<ssa.Value>[] args)
        {
            {
                var (callDef, ok) = softFloatOps[op];

                if (ok)
                {

                    if (op == ssa.OpLess32F || op == ssa.OpLess64F || op == ssa.OpLeq32F || op == ssa.OpLeq64F) 
                        args[0L] = args[1L];
                        args[1L] = args[0L];
                    else if (op == ssa.OpSub32F || op == ssa.OpSub64F) 
                        args[1L] = s.newValue1(s.ssaOp(OMINUS, types.Types[callDef.rtype]), args[1L].Type, args[1L]);
                                        var result = s.rtcall(callDef.rtfn, true, new slice<ref types.Type>(new ref types.Type[] { types.Types[callDef.rtype] }), args)[0L];
                    if (op == ssa.OpNeq32F || op == ssa.OpNeq64F)
                    {
                        result = s.newValue1(ssa.OpNot, result.Type, result);
                    }
                    return (result, true);
                }

            }
            return (null, false);
        }

        private static map<intrinsicKey, intrinsicBuilder> intrinsics = default;

        // An intrinsicBuilder converts a call node n into an ssa value that
        // implements that call as an intrinsic. args is a list of arguments to the func.
        public delegate  ref ssa.Value intrinsicBuilder(ref state,  ref Node,  slice<ref ssa.Value>);

        private partial struct intrinsicKey
        {
            public ptr<sys.Arch> arch;
            public @string pkg;
            public @string fn;
        }

        private static void init() => func((_, panic, __) =>
        {
            intrinsics = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<intrinsicKey, intrinsicBuilder>{};

            slice<ref sys.Arch> all = default;
            slice<ref sys.Arch> p4 = default;
            slice<ref sys.Arch> p8 = default;
            {
                var a__prev1 = a;

                foreach (var (_, __a) in sys.Archs)
                {
                    a = __a;
                    all = append(all, a);
                    if (a.PtrSize == 4L)
                    {
                        p4 = append(p4, a);
                    }
                    else
                    {
                        p8 = append(p8, a);
                    }
                } 

                // add adds the intrinsic b for pkg.fn for the given list of architectures.

                a = a__prev1;
            }

            Action<@string, @string, intrinsicBuilder, ptr<sys.Arch>[]> add = (pkg, fn, b, archs) =>
            {
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in archs)
                    {
                        a = __a;
                        intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                    }

                    a = a__prev1;
                }

            } 
            // addF does the same as add but operates on architecture families.
; 
            // addF does the same as add but operates on architecture families.
            Action<@string, @string, intrinsicBuilder, sys.ArchFamily[]> addF = (pkg, fn, b, archFamilies) =>
            {
                long m = 0L;
                foreach (var (_, f) in archFamilies)
                {
                    if (f >= 32L)
                    {
                        panic("too many architecture families");
                    }
                    m |= 1L << (int)(uint(f));
                }
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in all)
                    {
                        a = __a;
                        if (m >> (int)(uint(a.Family)) & 1L != 0L)
                        {
                            intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                        }
                    }

                    a = a__prev1;
                }

            } 
            // alias defines pkg.fn = pkg2.fn2 for all architectures in archs for which pkg2.fn2 exists.
; 
            // alias defines pkg.fn = pkg2.fn2 for all architectures in archs for which pkg2.fn2 exists.
            Action<@string, @string, @string, @string, ptr<sys.Arch>[]> alias = (pkg, fn, pkg2, fn2, archs) =>
            {
                {
                    var a__prev1 = a;

                    foreach (var (_, __a) in archs)
                    {
                        a = __a;
                        {
                            var b__prev1 = b;

                            var (b, ok) = intrinsics[new intrinsicKey(a,pkg2,fn2)];

                            if (ok)
                            {
                                intrinsics[new intrinsicKey(a,pkg,fn)] = b;
                            }

                            b = b__prev1;

                        }
                    }

                    a = a__prev1;
                }

            } 

            /******** runtime ********/
; 

            /******** runtime ********/
            if (!instrumenting)
            {
                add("runtime", "slicebytetostringtmp", (s, n, args) =>
                { 
                    // Compiler frontend optimizations emit OARRAYBYTESTRTMP nodes
                    // for the backend instead of slicebytetostringtmp calls
                    // when not instrumenting.
                    var slice = args[0L];
                    var ptr = s.newValue1(ssa.OpSlicePtr, s.f.Config.Types.BytePtr, slice);
                    var len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], slice);
                    return s.newValue2(ssa.OpStringMake, n.Type, ptr, len);
                }, all);
            }
            add("runtime", "KeepAlive", (s, n, args) =>
            {
                var data = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, args[0L]);
                s.vars[ref memVar] = s.newValue2(ssa.OpKeepAlive, types.TypeMem, data, s.mem());
                return null;
            }, all);
            add("runtime", "getclosureptr", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetClosurePtr, s.f.Config.Types.Uintptr);
            }, all);

            addF("runtime", "getcallerpc", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetCallerPC, s.f.Config.Types.Uintptr);
            }, sys.AMD64, sys.I386);

            add("runtime", "getcallersp", (s, n, args) =>
            {
                return s.newValue0(ssa.OpGetCallerSP, s.f.Config.Types.Uintptr);
            }, all); 

            /******** runtime/internal/sys ********/
            addF("runtime/internal/sys", "Ctz32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("runtime/internal/sys", "Ctz64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("runtime/internal/sys", "Bswap32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBswap32, types.Types[TUINT32], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X);
            addF("runtime/internal/sys", "Bswap64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBswap64, types.Types[TUINT64], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X); 

            /******** runtime/internal/atomic ********/
            addF("runtime/internal/atomic", "Load", (s, n, args) =>
            {
                var v = s.newValue2(ssa.OpAtomicLoad32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Load64", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoad64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Loadp", (s, n, args) =>
            {
                v = s.newValue2(ssa.OpAtomicLoadPtr, types.NewTuple(s.f.Config.Types.BytePtr, types.TypeMem), args[0L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, s.f.Config.Types.BytePtr, v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);

            addF("runtime/internal/atomic", "Store", (s, n, args) =>
            {
                s.vars[ref memVar] = s.newValue3(ssa.OpAtomicStore32, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Store64", (s, n, args) =>
            {
                s.vars[ref memVar] = s.newValue3(ssa.OpAtomicStore64, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "StorepNoWB", (s, n, args) =>
            {
                s.vars[ref memVar] = s.newValue3(ssa.OpAtomicStorePtrNoWB, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64);

            addF("runtime/internal/atomic", "Xchg", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicExchange32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Xchg64", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicExchange64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS64, sys.PPC64);

            addF("runtime/internal/atomic", "Xadd", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicAdd32, types.NewTuple(types.Types[TUINT32], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT32], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Xadd64", (s, n, args) =>
            {
                v = s.newValue3(ssa.OpAtomicAdd64, types.NewTuple(types.Types[TUINT64], types.TypeMem), args[0L], args[1L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TUINT64], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS64, sys.PPC64);

            addF("runtime/internal/atomic", "Cas", (s, n, args) =>
            {
                v = s.newValue4(ssa.OpAtomicCompareAndSwap32, types.NewTuple(types.Types[TBOOL], types.TypeMem), args[0L], args[1L], args[2L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TBOOL], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS, sys.MIPS64, sys.PPC64);
            addF("runtime/internal/atomic", "Cas64", (s, n, args) =>
            {
                v = s.newValue4(ssa.OpAtomicCompareAndSwap64, types.NewTuple(types.Types[TBOOL], types.TypeMem), args[0L], args[1L], args[2L], s.mem());
                s.vars[ref memVar] = s.newValue1(ssa.OpSelect1, types.TypeMem, v);
                return s.newValue1(ssa.OpSelect0, types.Types[TBOOL], v);
            }, sys.AMD64, sys.ARM64, sys.S390X, sys.MIPS64, sys.PPC64);

            addF("runtime/internal/atomic", "And8", (s, n, args) =>
            {
                s.vars[ref memVar] = s.newValue3(ssa.OpAtomicAnd8, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.PPC64);
            addF("runtime/internal/atomic", "Or8", (s, n, args) =>
            {
                s.vars[ref memVar] = s.newValue3(ssa.OpAtomicOr8, types.TypeMem, args[0L], args[1L], s.mem());
                return null;
            }, sys.AMD64, sys.ARM64, sys.MIPS, sys.PPC64);

            alias("runtime/internal/atomic", "Loadint64", "runtime/internal/atomic", "Load64", all);
            alias("runtime/internal/atomic", "Xaddint64", "runtime/internal/atomic", "Xadd64", all);
            alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load", p4);
            alias("runtime/internal/atomic", "Loaduint", "runtime/internal/atomic", "Load64", p8);
            alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load", p4);
            alias("runtime/internal/atomic", "Loaduintptr", "runtime/internal/atomic", "Load64", p8);
            alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store", p4);
            alias("runtime/internal/atomic", "Storeuintptr", "runtime/internal/atomic", "Store64", p8);
            alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg", p4);
            alias("runtime/internal/atomic", "Xchguintptr", "runtime/internal/atomic", "Xchg64", p8);
            alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd", p4);
            alias("runtime/internal/atomic", "Xadduintptr", "runtime/internal/atomic", "Xadd64", p8);
            alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas", p4);
            alias("runtime/internal/atomic", "Casuintptr", "runtime/internal/atomic", "Cas64", p8);
            alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas", p4);
            alias("runtime/internal/atomic", "Casp1", "runtime/internal/atomic", "Cas64", p8); 

            /******** math ********/
            addF("math", "Sqrt", (s, n, args) =>
            {
                return s.newValue1(ssa.OpSqrt, types.Types[TFLOAT64], args[0L]);
            }, sys.AMD64, sys.ARM, sys.ARM64, sys.MIPS, sys.PPC64, sys.S390X);
            addF("math", "Trunc", (s, n, args) =>
            {
                return s.newValue1(ssa.OpTrunc, types.Types[TFLOAT64], args[0L]);
            }, sys.PPC64, sys.S390X);
            addF("math", "Ceil", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCeil, types.Types[TFLOAT64], args[0L]);
            }, sys.PPC64, sys.S390X);
            addF("math", "Floor", (s, n, args) =>
            {
                return s.newValue1(ssa.OpFloor, types.Types[TFLOAT64], args[0L]);
            }, sys.PPC64, sys.S390X);
            addF("math", "Round", (s, n, args) =>
            {
                return s.newValue1(ssa.OpRound, types.Types[TFLOAT64], args[0L]);
            }, sys.S390X);
            addF("math", "RoundToEven", (s, n, args) =>
            {
                return s.newValue1(ssa.OpRoundToEven, types.Types[TFLOAT64], args[0L]);
            }, sys.S390X);
            addF("math", "Abs", (s, n, args) =>
            {
                return s.newValue1(ssa.OpAbs, types.Types[TFLOAT64], args[0L]);
            }, sys.PPC64);
            addF("math", "Copysign", (s, n, args) =>
            {
                return s.newValue2(ssa.OpCopysign, types.Types[TFLOAT64], args[0L], args[1L]);
            }, sys.PPC64);

            Func<ssa.Op, Func<ref state, ref Node, slice<ref ssa.Value>, ref ssa.Value>> makeRoundAMD64 = op =>
            {
                return (s, n, args) =>
                {
                    var aux = syslook("support_sse41").Sym.Linksym();
                    var addr = s.entryNewValue1A(ssa.OpAddr, types.Types[TBOOL].PtrTo(), aux, s.sb);
                    v = s.newValue2(ssa.OpLoad, types.Types[TBOOL], addr, s.mem());
                    var b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(v);
                    var bTrue = s.f.NewBlock(ssa.BlockPlain);
                    var bFalse = s.f.NewBlock(ssa.BlockPlain);
                    var bEnd = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bTrue);
                    b.AddEdgeTo(bFalse);
                    b.Likely = ssa.BranchLikely; // most machines have sse4.1 nowadays

                    // We have the intrinsic - use it directly.
                    s.startBlock(bTrue);
                    s.vars[n] = s.newValue1(op, types.Types[TFLOAT64], args[0L]);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Call the pure Go version.
                    s.startBlock(bFalse);
                    var a = s.call(n, callNormal);
                    s.vars[n] = s.newValue2(ssa.OpLoad, types.Types[TFLOAT64], a, s.mem());
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Merge results.
                    s.startBlock(bEnd);
                    return s.variable(n, types.Types[TFLOAT64]);
                }
;
            }
;
            addF("math", "RoundToEven", makeRoundAMD64(ssa.OpRoundToEven), sys.AMD64);
            addF("math", "Floor", makeRoundAMD64(ssa.OpFloor), sys.AMD64);
            addF("math", "Ceil", makeRoundAMD64(ssa.OpCeil), sys.AMD64);
            addF("math", "Trunc", makeRoundAMD64(ssa.OpTrunc), sys.AMD64); 

            /******** math/bits ********/
            addF("math/bits", "TrailingZeros64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("math/bits", "TrailingZeros32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("math/bits", "TrailingZeros16", (s, n, args) =>
            {
                var x = s.newValue1(ssa.OpZeroExt16to32, types.Types[TUINT32], args[0L]);
                var c = s.constInt32(types.Types[TUINT32], 1L << (int)(16L));
                var y = s.newValue2(ssa.OpOr32, types.Types[TUINT32], x, c);
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], y);
            }, sys.ARM, sys.MIPS);
            addF("math/bits", "TrailingZeros16", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt16to64, types.Types[TUINT64], args[0L]);
                c = s.constInt64(types.Types[TUINT64], 1L << (int)(16L));
                y = s.newValue2(ssa.OpOr64, types.Types[TUINT64], x, c);
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], y);
            }, sys.AMD64, sys.ARM64, sys.S390X);
            addF("math/bits", "TrailingZeros8", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt8to32, types.Types[TUINT32], args[0L]);
                c = s.constInt32(types.Types[TUINT32], 1L << (int)(8L));
                y = s.newValue2(ssa.OpOr32, types.Types[TUINT32], x, c);
                return s.newValue1(ssa.OpCtz32, types.Types[TINT], y);
            }, sys.ARM, sys.MIPS);
            addF("math/bits", "TrailingZeros8", (s, n, args) =>
            {
                x = s.newValue1(ssa.OpZeroExt8to64, types.Types[TUINT64], args[0L]);
                c = s.constInt64(types.Types[TUINT64], 1L << (int)(8L));
                y = s.newValue2(ssa.OpOr64, types.Types[TUINT64], x, c);
                return s.newValue1(ssa.OpCtz64, types.Types[TINT], y);
            }, sys.AMD64, sys.ARM64, sys.S390X);
            alias("math/bits", "ReverseBytes64", "runtime/internal/sys", "Bswap64", all);
            alias("math/bits", "ReverseBytes32", "runtime/internal/sys", "Bswap32", all); 
            // ReverseBytes inlines correctly, no need to intrinsify it.
            // ReverseBytes16 lowers to a rotate, no need for anything special here.
            addF("math/bits", "Len64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("math/bits", "Len32", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], args[0L]);
                }
                x = s.newValue1(ssa.OpZeroExt32to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);
            addF("math/bits", "Len16", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    x = s.newValue1(ssa.OpZeroExt16to32, types.Types[TUINT32], args[0L]);
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], x);
                }
                x = s.newValue1(ssa.OpZeroExt16to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64); 
            // Note: disabled on AMD64 because the Go code is faster!
            addF("math/bits", "Len8", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    x = s.newValue1(ssa.OpZeroExt8to32, types.Types[TUINT32], args[0L]);
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], x);
                }
                x = s.newValue1(ssa.OpZeroExt8to64, types.Types[TUINT64], args[0L]);
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], x);
            }, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64);

            addF("math/bits", "Len", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitLen32, types.Types[TINT], args[0L]);
                }
                return s.newValue1(ssa.OpBitLen64, types.Types[TINT], args[0L]);
            }, sys.AMD64, sys.ARM64, sys.ARM, sys.S390X, sys.MIPS, sys.PPC64); 
            // LeadingZeros is handled because it trivially calls Len.
            addF("math/bits", "Reverse64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev64, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev32, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse16", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev16, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse8", (s, n, args) =>
            {
                return s.newValue1(ssa.OpBitRev8, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            addF("math/bits", "Reverse", (s, n, args) =>
            {
                if (s.config.PtrSize == 4L)
                {
                    return s.newValue1(ssa.OpBitRev32, types.Types[TINT], args[0L]);
                }
                return s.newValue1(ssa.OpBitRev64, types.Types[TINT], args[0L]);
            }, sys.ARM64);
            Func<ssa.Op, ssa.Op, Func<ref state, ref Node, slice<ref ssa.Value>, ref ssa.Value>> makeOnesCountAMD64 = (op64, op32) =>
            {
                return (s, n, args) =>
                {
                    aux = syslook("support_popcnt").Sym.Linksym();
                    addr = s.entryNewValue1A(ssa.OpAddr, types.Types[TBOOL].PtrTo(), aux, s.sb);
                    v = s.newValue2(ssa.OpLoad, types.Types[TBOOL], addr, s.mem());
                    b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(v);
                    bTrue = s.f.NewBlock(ssa.BlockPlain);
                    bFalse = s.f.NewBlock(ssa.BlockPlain);
                    bEnd = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bTrue);
                    b.AddEdgeTo(bFalse);
                    b.Likely = ssa.BranchLikely; // most machines have popcnt nowadays

                    // We have the intrinsic - use it directly.
                    s.startBlock(bTrue);
                    var op = op64;
                    if (s.config.PtrSize == 4L)
                    {
                        op = op32;
                    }
                    s.vars[n] = s.newValue1(op, types.Types[TINT], args[0L]);
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Call the pure Go version.
                    s.startBlock(bFalse);
                    a = s.call(n, callNormal);
                    s.vars[n] = s.newValue2(ssa.OpLoad, types.Types[TINT], a, s.mem());
                    s.endBlock().AddEdgeTo(bEnd); 

                    // Merge results.
                    s.startBlock(bEnd);
                    return s.variable(n, types.Types[TINT]);
                }
;
            }
;
            addF("math/bits", "OnesCount64", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount64), sys.AMD64);
            addF("math/bits", "OnesCount64", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount64, types.Types[TINT], args[0L]);
            }, sys.PPC64);
            addF("math/bits", "OnesCount32", makeOnesCountAMD64(ssa.OpPopCount32, ssa.OpPopCount32), sys.AMD64);
            addF("math/bits", "OnesCount32", (s, n, args) =>
            {
                return s.newValue1(ssa.OpPopCount32, types.Types[TINT], args[0L]);
            }, sys.PPC64);
            addF("math/bits", "OnesCount16", makeOnesCountAMD64(ssa.OpPopCount16, ssa.OpPopCount16), sys.AMD64); 
            // Note: no OnesCount8, the Go implementation is faster - just a table load.
            addF("math/bits", "OnesCount", makeOnesCountAMD64(ssa.OpPopCount64, ssa.OpPopCount32), sys.AMD64);

            /******** sync/atomic ********/

            // Note: these are disabled by flag_race in findIntrinsic below.
            alias("sync/atomic", "LoadInt32", "runtime/internal/atomic", "Load", all);
            alias("sync/atomic", "LoadInt64", "runtime/internal/atomic", "Load64", all);
            alias("sync/atomic", "LoadPointer", "runtime/internal/atomic", "Loadp", all);
            alias("sync/atomic", "LoadUint32", "runtime/internal/atomic", "Load", all);
            alias("sync/atomic", "LoadUint64", "runtime/internal/atomic", "Load64", all);
            alias("sync/atomic", "LoadUintptr", "runtime/internal/atomic", "Load", p4);
            alias("sync/atomic", "LoadUintptr", "runtime/internal/atomic", "Load64", p8);

            alias("sync/atomic", "StoreInt32", "runtime/internal/atomic", "Store", all);
            alias("sync/atomic", "StoreInt64", "runtime/internal/atomic", "Store64", all); 
            // Note: not StorePointer, that needs a write barrier.  Same below for {CompareAnd}Swap.
            alias("sync/atomic", "StoreUint32", "runtime/internal/atomic", "Store", all);
            alias("sync/atomic", "StoreUint64", "runtime/internal/atomic", "Store64", all);
            alias("sync/atomic", "StoreUintptr", "runtime/internal/atomic", "Store", p4);
            alias("sync/atomic", "StoreUintptr", "runtime/internal/atomic", "Store64", p8);

            alias("sync/atomic", "SwapInt32", "runtime/internal/atomic", "Xchg", all);
            alias("sync/atomic", "SwapInt64", "runtime/internal/atomic", "Xchg64", all);
            alias("sync/atomic", "SwapUint32", "runtime/internal/atomic", "Xchg", all);
            alias("sync/atomic", "SwapUint64", "runtime/internal/atomic", "Xchg64", all);
            alias("sync/atomic", "SwapUintptr", "runtime/internal/atomic", "Xchg", p4);
            alias("sync/atomic", "SwapUintptr", "runtime/internal/atomic", "Xchg64", p8);

            alias("sync/atomic", "CompareAndSwapInt32", "runtime/internal/atomic", "Cas", all);
            alias("sync/atomic", "CompareAndSwapInt64", "runtime/internal/atomic", "Cas64", all);
            alias("sync/atomic", "CompareAndSwapUint32", "runtime/internal/atomic", "Cas", all);
            alias("sync/atomic", "CompareAndSwapUint64", "runtime/internal/atomic", "Cas64", all);
            alias("sync/atomic", "CompareAndSwapUintptr", "runtime/internal/atomic", "Cas", p4);
            alias("sync/atomic", "CompareAndSwapUintptr", "runtime/internal/atomic", "Cas64", p8);

            alias("sync/atomic", "AddInt32", "runtime/internal/atomic", "Xadd", all);
            alias("sync/atomic", "AddInt64", "runtime/internal/atomic", "Xadd64", all);
            alias("sync/atomic", "AddUint32", "runtime/internal/atomic", "Xadd", all);
            alias("sync/atomic", "AddUint64", "runtime/internal/atomic", "Xadd64", all);
            alias("sync/atomic", "AddUintptr", "runtime/internal/atomic", "Xadd", p4);
            alias("sync/atomic", "AddUintptr", "runtime/internal/atomic", "Xadd64", p8); 

            /******** math/big ********/
            add("math/big", "mulWW", (s, n, args) =>
            {
                return s.newValue2(ssa.OpMul64uhilo, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L]);
            }, sys.ArchAMD64);
            add("math/big", "divWW", (s, n, args) =>
            {
                return s.newValue3(ssa.OpDiv128u, types.NewTuple(types.Types[TUINT64], types.Types[TUINT64]), args[0L], args[1L], args[2L]);
            }, sys.ArchAMD64);
        });

        // findIntrinsic returns a function which builds the SSA equivalent of the
        // function identified by the symbol sym.  If sym is not an intrinsic call, returns nil.
        private static intrinsicBuilder findIntrinsic(ref types.Sym sym)
        {
            if (ssa.IntrinsicsDisable)
            {
                return null;
            }
            if (sym == null || sym.Pkg == null)
            {
                return null;
            }
            var pkg = sym.Pkg.Path;
            if (sym.Pkg == localpkg)
            {
                pkg = myimportpath;
            }
            if (flag_race && pkg == "sync/atomic")
            { 
                // The race detector needs to be able to intercept these calls.
                // We can't intrinsify them.
                return null;
            } 
            // Skip intrinsifying math functions (which may contain hard-float
            // instructions) when soft-float
            if (thearch.SoftFloat && pkg == "math")
            {
                return null;
            }
            var fn = sym.Name;
            return intrinsics[new intrinsicKey(thearch.LinkArch.Arch,pkg,fn)];
        }

        private static bool isIntrinsicCall(ref Node n)
        {
            if (n == null || n.Left == null)
            {
                return false;
            }
            return findIntrinsic(n.Left.Sym) != null;
        }

        // intrinsicCall converts a call to a recognized intrinsic function into the intrinsic SSA operation.
        private static ref ssa.Value intrinsicCall(this ref state s, ref Node n)
        {
            var v = findIntrinsic(n.Left.Sym)(s, n, s.intrinsicArgs(n));
            if (ssa.IntrinsicsDebug > 0L)
            {
                var x = v;
                if (x == null)
                {
                    x = s.mem();
                }
                if (x.Op == ssa.OpSelect0 || x.Op == ssa.OpSelect1)
                {
                    x = x.Args[0L];
                }
                Warnl(n.Pos, "intrinsic substitution for %v with %s", n.Left.Sym.Name, x.LongString());
            }
            return v;
        }

        private partial struct callArg
        {
            public long offset;
            public ptr<ssa.Value> v;
        }
        private partial struct byOffset // : slice<callArg>
        {
        }

        private static long Len(this byOffset x)
        {
            return len(x);
        }
        private static void Swap(this byOffset x, long i, long j)
        {
            x[i] = x[j];
            x[j] = x[i];

        }
        private static bool Less(this byOffset x, long i, long j)
        {
            return x[i].offset < x[j].offset;
        }

        // intrinsicArgs extracts args from n, evaluates them to SSA values, and returns them.
        private static slice<ref ssa.Value> intrinsicArgs(this ref state s, ref Node n)
        { 
            // This code is complicated because of how walk transforms calls. For a call node,
            // each entry in n.List is either an assignment to OINDREGSP which actually
            // stores an arg, or an assignment to a temporary which computes an arg
            // which is later assigned.
            // The args can also be out of order.
            // TODO: when walk goes away someday, this code can go away also.
            slice<callArg> args = default;
            map temps = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref Node, ref ssa.Value>{};
            {
                var a__prev1 = a;

                foreach (var (_, __a) in n.List.Slice())
                {
                    a = __a;
                    if (a.Op != OAS)
                    {
                        s.Fatalf("non-assignment as a function argument %v", a.Op);
                    }
                    var l = a.Left;
                    var r = a.Right;

                    if (l.Op == ONAME) 
                        // Evaluate and store to "temporary".
                        // Walk ensures these temporaries are dead outside of n.
                        temps[l] = s.expr(r);
                    else if (l.Op == OINDREGSP) 
                        // Store a value to an argument slot.
                        ref ssa.Value v = default;
                        {
                            var (x, ok) = temps[r];

                            if (ok)
                            { 
                                // This is a previously computed temporary.
                                v = x;
                            }
                            else
                            { 
                                // This is an explicit value; evaluate it.
                                v = s.expr(r);
                            }

                        }
                        args = append(args, new callArg(l.Xoffset,v));
                    else 
                        s.Fatalf("function argument assignment target not allowed: %v", l.Op);
                                    }

                a = a__prev1;
            }

            sort.Sort(byOffset(args));
            var res = make_slice<ref ssa.Value>(len(args));
            {
                var a__prev1 = a;

                foreach (var (__i, __a) in args)
                {
                    i = __i;
                    a = __a;
                    res[i] = a.v;
                }

                a = a__prev1;
            }

            return res;
        }

        // Calls the function n using the specified call type.
        // Returns the address of the return value (or nil if none).
        private static ref ssa.Value call(this ref state s, ref Node n, callKind k)
        {
            ref types.Sym sym = default; // target symbol (if static)
            ref ssa.Value closure = default; // ptr to closure to run (if dynamic)
            ref ssa.Value codeptr = default; // ptr to target code (if dynamic)
            ref ssa.Value rcvr = default; // receiver to set
            var fn = n.Left;

            if (n.Op == OCALLFUNC) 
                if (k == callNormal && fn.Op == ONAME && fn.Class() == PFUNC)
                {
                    sym = fn.Sym;
                    break;
                }
                closure = s.expr(fn);
            else if (n.Op == OCALLMETH) 
                if (fn.Op != ODOTMETH)
                {
                    Fatalf("OCALLMETH: n.Left not an ODOTMETH: %v", fn);
                }
                if (k == callNormal)
                {
                    sym = fn.Sym;
                    break;
                } 
                // Make a name n2 for the function.
                // fn.Sym might be sync.(*Mutex).Unlock.
                // Make a PFUNC node out of that, then evaluate it.
                // We get back an SSA value representing &sync.(*Mutex).Unlock·f.
                // We can then pass that to defer or go.
                var n2 = newnamel(fn.Pos, fn.Sym);
                n2.Name.Curfn = s.curfn;
                n2.SetClass(PFUNC);
                n2.Pos = fn.Pos;
                n2.Type = types.Types[TUINT8]; // dummy type for a static closure. Could use runtime.funcval if we had it.
                closure = s.expr(n2); 
                // Note: receiver is already assigned in n.List, so we don't
                // want to set it here.
            else if (n.Op == OCALLINTER) 
                if (fn.Op != ODOTINTER)
                {
                    Fatalf("OCALLINTER: n.Left not an ODOTINTER: %v", fn.Op);
                }
                var i = s.expr(fn.Left);
                var itab = s.newValue1(ssa.OpITab, types.Types[TUINTPTR], i);
                s.nilCheck(itab);
                var itabidx = fn.Xoffset + 2L * int64(Widthptr) + 8L; // offset of fun field in runtime.itab
                itab = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.UintptrPtr, itabidx, itab);
                if (k == callNormal)
                {
                    codeptr = s.newValue2(ssa.OpLoad, types.Types[TUINTPTR], itab, s.mem());
                }
                else
                {
                    closure = itab;
                }
                rcvr = s.newValue1(ssa.OpIData, types.Types[TUINTPTR], i);
                        dowidth(fn.Type);
            var stksize = fn.Type.ArgWidth(); // includes receiver

            // Run all argument assignments. The arg slots have already
            // been offset by the appropriate amount (+2*widthptr for go/defer,
            // +widthptr for interface calls).
            // For OCALLMETH, the receiver is set in these statements.
            s.stmtList(n.List); 

            // Set receiver (for interface calls)
            if (rcvr != null)
            {
                var argStart = Ctxt.FixedFrameSize();
                if (k != callNormal)
                {
                    argStart += int64(2L * Widthptr);
                }
                var addr = s.constOffPtrSP(s.f.Config.Types.UintptrPtr, argStart);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TUINTPTR], addr, rcvr, s.mem());
            } 

            // Defer/go args
            if (k != callNormal)
            { 
                // Write argsize and closure (args to Newproc/Deferproc).
                argStart = Ctxt.FixedFrameSize();
                var argsize = s.constInt32(types.Types[TUINT32], int32(stksize));
                addr = s.constOffPtrSP(s.f.Config.Types.UInt32Ptr, argStart);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TUINT32], addr, argsize, s.mem());
                addr = s.constOffPtrSP(s.f.Config.Types.UintptrPtr, argStart + int64(Widthptr));
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TUINTPTR], addr, closure, s.mem());
                stksize += 2L * int64(Widthptr);
            } 

            // call target
            ref ssa.Value call = default;

            if (k == callDefer) 
                call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, Deferproc, s.mem());
            else if (k == callGo) 
                call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, Newproc, s.mem());
            else if (closure != null) 
                codeptr = s.newValue2(ssa.OpLoad, types.Types[TUINTPTR], closure, s.mem());
                call = s.newValue3(ssa.OpClosureCall, types.TypeMem, codeptr, closure, s.mem());
            else if (codeptr != null) 
                call = s.newValue2(ssa.OpInterCall, types.TypeMem, codeptr, s.mem());
            else if (sym != null) 
                call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, sym.Linksym(), s.mem());
            else 
                Fatalf("bad call type %v %v", n.Op, n);
                        call.AuxInt = stksize; // Call operations carry the argsize of the callee along with them
            s.vars[ref memVar] = call; 

            // Finish block for defers
            if (k == callDefer)
            {
                var b = s.endBlock();
                b.Kind = ssa.BlockDefer;
                b.SetControl(call);
                var bNext = s.f.NewBlock(ssa.BlockPlain);
                b.AddEdgeTo(bNext); 
                // Add recover edge to exit code.
                var r = s.f.NewBlock(ssa.BlockPlain);
                s.startBlock(r);
                s.exit();
                b.AddEdgeTo(r);
                b.Likely = ssa.BranchLikely;
                s.startBlock(bNext);
            }
            var res = n.Left.Type.Results();
            if (res.NumFields() == 0L || k != callNormal)
            { 
                // call has no return value. Continue with the next statement.
                return null;
            }
            var fp = res.Field(0L);
            return s.constOffPtrSP(types.NewPtr(fp.Type), fp.Offset + Ctxt.FixedFrameSize());
        }

        // etypesign returns the signed-ness of e, for integer/pointer etypes.
        // -1 means signed, +1 means unsigned, 0 means non-integer/non-pointer.
        private static sbyte etypesign(types.EType e)
        {

            if (e == TINT8 || e == TINT16 || e == TINT32 || e == TINT64 || e == TINT) 
                return -1L;
            else if (e == TUINT8 || e == TUINT16 || e == TUINT32 || e == TUINT64 || e == TUINT || e == TUINTPTR || e == TUNSAFEPTR) 
                return +1L;
                        return 0L;
        }

        // addr converts the address of the expression n to SSA, adds it to s and returns the SSA result.
        // The value that the returned Value represents is guaranteed to be non-nil.
        // If bounded is true then this address does not require a nil check for its operand
        // even if that would otherwise be implied.
        private static ref ssa.Value addr(this ref state s, ref Node n, bool bounded)
        {
            var t = types.NewPtr(n.Type);

            if (n.Op == ONAME) 

                if (n.Class() == PEXTERN) 
                    // global variable
                    var v = s.entryNewValue1A(ssa.OpAddr, t, n.Sym.Linksym(), s.sb); 
                    // TODO: Make OpAddr use AuxInt as well as Aux.
                    if (n.Xoffset != 0L)
                    {
                        v = s.entryNewValue1I(ssa.OpOffPtr, v.Type, n.Xoffset, v);
                    }
                    return v;
                else if (n.Class() == PPARAM) 
                    // parameter slot
                    v = s.decladdrs[n];
                    if (v != null)
                    {
                        return v;
                    }
                    if (n == nodfp)
                    { 
                        // Special arg that points to the frame pointer (Used by ORECOVER).
                        return s.entryNewValue1A(ssa.OpAddr, t, n, s.sp);
                    }
                    s.Fatalf("addr of undeclared ONAME %v. declared: %v", n, s.decladdrs);
                    return null;
                else if (n.Class() == PAUTO) 
                    return s.newValue1A(ssa.OpAddr, t, n, s.sp);
                else if (n.Class() == PPARAMOUT) // Same as PAUTO -- cannot generate LEA early.
                    // ensure that we reuse symbols for out parameters so
                    // that cse works on their addresses
                    return s.newValue1A(ssa.OpAddr, t, n, s.sp);
                else 
                    s.Fatalf("variable address class %v not implemented", n.Class());
                    return null;
                            else if (n.Op == OINDREGSP) 
                // indirect off REGSP
                // used for storing/loading arguments/returns to/from callees
                return s.constOffPtrSP(t, n.Xoffset);
            else if (n.Op == OINDEX) 
                if (n.Left.Type.IsSlice())
                {
                    var a = s.expr(n.Left);
                    var i = s.expr(n.Right);
                    i = s.extendIndex(i, panicindex);
                    var len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], a);
                    if (!n.Bounded())
                    {
                        s.boundsCheck(i, len);
                    }
                    var p = s.newValue1(ssa.OpSlicePtr, t, a);
                    return s.newValue2(ssa.OpPtrIndex, t, p, i);
                }
                else
                { // array
                    a = s.addr(n.Left, bounded);
                    i = s.expr(n.Right);
                    i = s.extendIndex(i, panicindex);
                    len = s.constInt(types.Types[TINT], n.Left.Type.NumElem());
                    if (!n.Bounded())
                    {
                        s.boundsCheck(i, len);
                    }
                    return s.newValue2(ssa.OpPtrIndex, types.NewPtr(n.Left.Type.Elem()), a, i);
                }
            else if (n.Op == OIND) 
                return s.exprPtr(n.Left, bounded, n.Pos);
            else if (n.Op == ODOT) 
                p = s.addr(n.Left, bounded);
                return s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, p);
            else if (n.Op == ODOTPTR) 
                p = s.exprPtr(n.Left, bounded, n.Pos);
                return s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, p);
            else if (n.Op == OCLOSUREVAR) 
                return s.newValue1I(ssa.OpOffPtr, t, n.Xoffset, s.entryNewValue0(ssa.OpGetClosurePtr, s.f.Config.Types.BytePtr));
            else if (n.Op == OCONVNOP) 
                var addr = s.addr(n.Left, bounded);
                return s.newValue1(ssa.OpCopy, t, addr); // ensure that addr has the right type
            else if (n.Op == OCALLFUNC || n.Op == OCALLINTER || n.Op == OCALLMETH) 
                return s.call(n, callNormal);
            else if (n.Op == ODOTTYPE) 
                var (v, _) = s.dottype(n, false);
                if (v.Op != ssa.OpLoad)
                {
                    s.Fatalf("dottype of non-load");
                }
                if (v.Args[1L] != s.mem())
                {
                    s.Fatalf("memory no longer live from dottype load");
                }
                return v.Args[0L];
            else 
                s.Fatalf("unhandled addr %v", n.Op);
                return null;
                    }

        // canSSA reports whether n is SSA-able.
        // n must be an ONAME (or an ODOT sequence with an ONAME base).
        private static bool canSSA(this ref state s, ref Node n)
        {
            if (Debug['N'] != 0L)
            {
                return false;
            }
            while (n.Op == ODOT || (n.Op == OINDEX && n.Left.Type.IsArray()))
            {
                n = n.Left;
            }

            if (n.Op != ONAME)
            {
                return false;
            }
            if (n.Addrtaken())
            {
                return false;
            }
            if (n.isParamHeapCopy())
            {
                return false;
            }
            if (n.Class() == PAUTOHEAP)
            {
                Fatalf("canSSA of PAUTOHEAP %v", n);
            }

            if (n.Class() == PEXTERN) 
                return false;
            else if (n.Class() == PPARAMOUT) 
                if (s.hasdefer)
                { 
                    // TODO: handle this case? Named return values must be
                    // in memory so that the deferred function can see them.
                    // Maybe do: if !strings.HasPrefix(n.String(), "~") { return false }
                    // Or maybe not, see issue 18860.  Even unnamed return values
                    // must be written back so if a defer recovers, the caller can see them.
                    return false;
                }
                if (s.cgoUnsafeArgs)
                { 
                    // Cgo effectively takes the address of all result args,
                    // but the compiler can't see that.
                    return false;
                }
                        if (n.Class() == PPARAM && n.Sym != null && n.Sym.Name == ".this")
            { 
                // wrappers generated by genwrapper need to update
                // the .this pointer in place.
                // TODO: treat as a PPARMOUT?
                return false;
            }
            return canSSAType(n.Type); 
            // TODO: try to make more variables SSAable?
        }

        // canSSA reports whether variables of type t are SSA-able.
        private static bool canSSAType(ref types.Type t)
        {
            dowidth(t);
            if (t.Width > int64(4L * Widthptr))
            { 
                // 4*Widthptr is an arbitrary constant. We want it
                // to be at least 3*Widthptr so slices can be registerized.
                // Too big and we'll introduce too much register pressure.
                return false;
            }

            if (t.Etype == TARRAY) 
                // We can't do larger arrays because dynamic indexing is
                // not supported on SSA variables.
                // TODO: allow if all indexes are constant.
                if (t.NumElem() <= 1L)
                {
                    return canSSAType(t.Elem());
                }
                return false;
            else if (t.Etype == TSTRUCT) 
                if (t.NumFields() > ssa.MaxStruct)
                {
                    return false;
                }
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (!canSSAType(t1.Type))
                    {
                        return false;
                    }
                }
                return true;
            else 
                return true;
                    }

        // exprPtr evaluates n to a pointer and nil-checks it.
        private static ref ssa.Value exprPtr(this ref state s, ref Node n, bool bounded, src.XPos lineno)
        {
            var p = s.expr(n);
            if (bounded || n.NonNil())
            {
                if (s.f.Frontend().Debug_checknil() && lineno.Line() > 1L)
                {
                    s.f.Warnl(lineno, "removed nil check");
                }
                return p;
            }
            s.nilCheck(p);
            return p;
        }

        // nilCheck generates nil pointer checking code.
        // Used only for automatically inserted nil checks,
        // not for user code like 'x != nil'.
        private static void nilCheck(this ref state s, ref ssa.Value ptr)
        {
            if (disable_checknil != 0L || s.curfn.Func.NilCheckDisabled())
            {
                return;
            }
            s.newValue2(ssa.OpNilCheck, types.TypeVoid, ptr, s.mem());
        }

        // boundsCheck generates bounds checking code. Checks if 0 <= idx < len, branches to exit if not.
        // Starts a new block on return.
        // idx is already converted to full int width.
        private static void boundsCheck(this ref state s, ref ssa.Value idx, ref ssa.Value len)
        {
            if (Debug['B'] != 0L)
            {
                return;
            } 

            // bounds check
            var cmp = s.newValue2(ssa.OpIsInBounds, types.Types[TBOOL], idx, len);
            s.check(cmp, panicindex);
        }

        // sliceBoundsCheck generates slice bounds checking code. Checks if 0 <= idx <= len, branches to exit if not.
        // Starts a new block on return.
        // idx and len are already converted to full int width.
        private static void sliceBoundsCheck(this ref state s, ref ssa.Value idx, ref ssa.Value len)
        {
            if (Debug['B'] != 0L)
            {
                return;
            } 

            // bounds check
            var cmp = s.newValue2(ssa.OpIsSliceInBounds, types.Types[TBOOL], idx, len);
            s.check(cmp, panicslice);
        }

        // If cmp (a bool) is false, panic using the given function.
        private static void check(this ref state s, ref ssa.Value cmp, ref obj.LSym fn)
        {
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;
            var bNext = s.f.NewBlock(ssa.BlockPlain);
            var line = s.peekPos();
            var pos = Ctxt.PosTable.Pos(line);
            funcLine fl = new funcLine(f:fn,base:pos.Base(),line:pos.Line());
            var bPanic = s.panics[fl];
            if (bPanic == null)
            {
                bPanic = s.f.NewBlock(ssa.BlockPlain);
                s.panics[fl] = bPanic;
                s.startBlock(bPanic); 
                // The panic call takes/returns memory to ensure that the right
                // memory state is observed if the panic happens.
                s.rtcall(fn, false, null);
            }
            b.AddEdgeTo(bNext);
            b.AddEdgeTo(bPanic);
            s.startBlock(bNext);
        }

        private static ref ssa.Value intDivide(this ref state s, ref Node n, ref ssa.Value a, ref ssa.Value b)
        {
            var needcheck = true;

            if (b.Op == ssa.OpConst8 || b.Op == ssa.OpConst16 || b.Op == ssa.OpConst32 || b.Op == ssa.OpConst64) 
                if (b.AuxInt != 0L)
                {
                    needcheck = false;
                }
                        if (needcheck)
            { 
                // do a size-appropriate check for zero
                var cmp = s.newValue2(s.ssaOp(ONE, n.Type), types.Types[TBOOL], b, s.zeroVal(n.Type));
                s.check(cmp, panicdivide);
            }
            return s.newValue2(s.ssaOp(n.Op, n.Type), a.Type, a, b);
        }

        // rtcall issues a call to the given runtime function fn with the listed args.
        // Returns a slice of results of the given result types.
        // The call is added to the end of the current block.
        // If returns is false, the block is marked as an exit block.
        private static slice<ref ssa.Value> rtcall(this ref state s, ref obj.LSym fn, bool returns, slice<ref types.Type> results, params ptr<ssa.Value>[] args)
        { 
            // Write args to the stack
            var off = Ctxt.FixedFrameSize();
            foreach (var (_, arg) in args)
            {
                var t = arg.Type;
                off = Rnd(off, t.Alignment());
                var ptr = s.constOffPtrSP(t.PtrTo(), off);
                var size = t.Size();
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, ptr, arg, s.mem());
                off += size;
            }
            off = Rnd(off, int64(Widthreg)); 

            // Issue call
            var call = s.newValue1A(ssa.OpStaticCall, types.TypeMem, fn, s.mem());
            s.vars[ref memVar] = call;

            if (!returns)
            { 
                // Finish block
                var b = s.endBlock();
                b.Kind = ssa.BlockExit;
                b.SetControl(call);
                call.AuxInt = off - Ctxt.FixedFrameSize();
                if (len(results) > 0L)
                {
                    Fatalf("panic call can't have results");
                }
                return null;
            } 

            // Load results
            var res = make_slice<ref ssa.Value>(len(results));
            {
                var t__prev1 = t;

                foreach (var (__i, __t) in results)
                {
                    i = __i;
                    t = __t;
                    off = Rnd(off, t.Alignment());
                    ptr = s.constOffPtrSP(types.NewPtr(t), off);
                    res[i] = s.newValue2(ssa.OpLoad, t, ptr, s.mem());
                    off += t.Size();
                }

                t = t__prev1;
            }

            off = Rnd(off, int64(Widthptr)); 

            // Remember how much callee stack space we needed.
            call.AuxInt = off;

            return res;
        }

        // do *left = right for type t.
        private static void storeType(this ref state s, ref types.Type t, ref ssa.Value left, ref ssa.Value right, skipMask skip)
        {
            if (skip == 0L && (!types.Haspointers(t) || ssa.IsStackAddr(left)))
            { 
                // Known to not have write barrier. Store the whole type.
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, left, right, s.mem());
                return;
            } 

            // store scalar fields first, so write barrier stores for
            // pointer fields can be grouped together, and scalar values
            // don't need to be live across the write barrier call.
            // TODO: if the writebarrier pass knows how to reorder stores,
            // we can do a single store here as long as skip==0.
            s.storeTypeScalars(t, left, right, skip);
            if (skip & skipPtr == 0L && types.Haspointers(t))
            {
                s.storeTypePtrs(t, left, right);
            }
        }

        // do *left = right for all scalar (non-pointer) parts of t.
        private static void storeTypeScalars(this ref state s, ref types.Type t, ref ssa.Value left, ref ssa.Value right, skipMask skip)
        {

            if (t.IsBoolean() || t.IsInteger() || t.IsFloat() || t.IsComplex()) 
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, left, right, s.mem());
            else if (t.IsPtrShaped())             else if (t.IsString()) 
                if (skip & skipLen != 0L)
                {
                    return;
                }
                var len = s.newValue1(ssa.OpStringLen, types.Types[TINT], right);
                var lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TINT], lenAddr, len, s.mem());
            else if (t.IsSlice()) 
                if (skip & skipLen == 0L)
                {
                    len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], right);
                    lenAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, s.config.PtrSize, left);
                    s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TINT], lenAddr, len, s.mem());
                }
                if (skip & skipCap == 0L)
                {
                    var cap = s.newValue1(ssa.OpSliceCap, types.Types[TINT], right);
                    var capAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.IntPtr, 2L * s.config.PtrSize, left);
                    s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TINT], capAddr, cap, s.mem());
                }
            else if (t.IsInterface()) 
                // itab field doesn't need a write barrier (even though it is a pointer).
                var itab = s.newValue1(ssa.OpITab, s.f.Config.Types.BytePtr, right);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, types.Types[TUINTPTR], left, itab, s.mem());
            else if (t.IsStruct()) 
                var n = t.NumFields();
                for (long i = 0L; i < n; i++)
                {
                    var ft = t.FieldType(i);
                    var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
                    var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
                    s.storeTypeScalars(ft, addr, val, 0L);
                }
            else if (t.IsArray() && t.NumElem() == 0L)             else if (t.IsArray() && t.NumElem() == 1L) 
                s.storeTypeScalars(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0L, right), 0L);
            else 
                s.Fatalf("bad write barrier type %v", t);
                    }

        // do *left = right for all pointer parts of t.
        private static void storeTypePtrs(this ref state s, ref types.Type t, ref ssa.Value left, ref ssa.Value right)
        {

            if (t.IsPtrShaped()) 
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, t, left, right, s.mem());
            else if (t.IsString()) 
                var ptr = s.newValue1(ssa.OpStringPtr, s.f.Config.Types.BytePtr, right);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, s.f.Config.Types.BytePtr, left, ptr, s.mem());
            else if (t.IsSlice()) 
                var elType = types.NewPtr(t.Elem());
                ptr = s.newValue1(ssa.OpSlicePtr, elType, right);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, elType, left, ptr, s.mem());
            else if (t.IsInterface()) 
                // itab field is treated as a scalar.
                var idata = s.newValue1(ssa.OpIData, s.f.Config.Types.BytePtr, right);
                var idataAddr = s.newValue1I(ssa.OpOffPtr, s.f.Config.Types.BytePtrPtr, s.config.PtrSize, left);
                s.vars[ref memVar] = s.newValue3A(ssa.OpStore, types.TypeMem, s.f.Config.Types.BytePtr, idataAddr, idata, s.mem());
            else if (t.IsStruct()) 
                var n = t.NumFields();
                for (long i = 0L; i < n; i++)
                {
                    var ft = t.FieldType(i);
                    if (!types.Haspointers(ft))
                    {
                        continue;
                    }
                    var addr = s.newValue1I(ssa.OpOffPtr, ft.PtrTo(), t.FieldOff(i), left);
                    var val = s.newValue1I(ssa.OpStructSelect, ft, int64(i), right);
                    s.storeTypePtrs(ft, addr, val);
                }
            else if (t.IsArray() && t.NumElem() == 0L)             else if (t.IsArray() && t.NumElem() == 1L) 
                s.storeTypePtrs(t.Elem(), left, s.newValue1I(ssa.OpArraySelect, t.Elem(), 0L, right));
            else 
                s.Fatalf("bad write barrier type %v", t);
                    }

        // slice computes the slice v[i:j:k] and returns ptr, len, and cap of result.
        // i,j,k may be nil, in which case they are set to their default value.
        // t is a slice, ptr to array, or string type.
        private static (ref ssa.Value, ref ssa.Value, ref ssa.Value) slice(this ref state s, ref types.Type t, ref ssa.Value v, ref ssa.Value i, ref ssa.Value j, ref ssa.Value k)
        {
            ref types.Type elemtype = default;
            ref types.Type ptrtype = default;
            ref ssa.Value ptr = default;
            ref ssa.Value len = default;
            ref ssa.Value cap = default;
            var zero = s.constInt(types.Types[TINT], 0L);

            if (t.IsSlice()) 
                elemtype = t.Elem();
                ptrtype = types.NewPtr(elemtype);
                ptr = s.newValue1(ssa.OpSlicePtr, ptrtype, v);
                len = s.newValue1(ssa.OpSliceLen, types.Types[TINT], v);
                cap = s.newValue1(ssa.OpSliceCap, types.Types[TINT], v);
            else if (t.IsString()) 
                elemtype = types.Types[TUINT8];
                ptrtype = types.NewPtr(elemtype);
                ptr = s.newValue1(ssa.OpStringPtr, ptrtype, v);
                len = s.newValue1(ssa.OpStringLen, types.Types[TINT], v);
                cap = len;
            else if (t.IsPtr()) 
                if (!t.Elem().IsArray())
                {
                    s.Fatalf("bad ptr to array in slice %v\n", t);
                }
                elemtype = t.Elem().Elem();
                ptrtype = types.NewPtr(elemtype);
                s.nilCheck(v);
                ptr = v;
                len = s.constInt(types.Types[TINT], t.Elem().NumElem());
                cap = len;
            else 
                s.Fatalf("bad type in slice %v\n", t);
            // Set default values
            if (i == null)
            {
                i = zero;
            }
            if (j == null)
            {
                j = len;
            }
            if (k == null)
            {
                k = cap;
            } 

            // Panic if slice indices are not in bounds.
            s.sliceBoundsCheck(i, j);
            if (j != k)
            {
                s.sliceBoundsCheck(j, k);
            }
            if (k != cap)
            {
                s.sliceBoundsCheck(k, cap);
            } 

            // Generate the following code assuming that indexes are in bounds.
            // The masking is to make sure that we don't generate a slice
            // that points to the next object in memory.
            // rlen = j - i
            // rcap = k - i
            // delta = i * elemsize
            // rptr = p + delta&mask(rcap)
            // result = (SliceMake rptr rlen rcap)
            // where mask(x) is 0 if x==0 and -1 if x>0.
            var subOp = s.ssaOp(OSUB, types.Types[TINT]);
            var mulOp = s.ssaOp(OMUL, types.Types[TINT]);
            var andOp = s.ssaOp(OAND, types.Types[TINT]);
            var rlen = s.newValue2(subOp, types.Types[TINT], j, i);
            ref ssa.Value rcap = default;

            if (t.IsString()) 
                // Capacity of the result is unimportant. However, we use
                // rcap to test if we've generated a zero-length slice.
                // Use length of strings for that.
                rcap = rlen;
            else if (j == k) 
                rcap = rlen;
            else 
                rcap = s.newValue2(subOp, types.Types[TINT], k, i);
                        ref ssa.Value rptr = default;
            if ((i.Op == ssa.OpConst64 || i.Op == ssa.OpConst32) && i.AuxInt == 0L)
            { 
                // No pointer arithmetic necessary.
                rptr = ptr;
            }
            else
            { 
                // delta = # of bytes to offset pointer by.
                var delta = s.newValue2(mulOp, types.Types[TINT], i, s.constInt(types.Types[TINT], elemtype.Width)); 
                // If we're slicing to the point where the capacity is zero,
                // zero out the delta.
                var mask = s.newValue1(ssa.OpSlicemask, types.Types[TINT], rcap);
                delta = s.newValue2(andOp, types.Types[TINT], delta, mask); 
                // Compute rptr = ptr + delta
                rptr = s.newValue2(ssa.OpAddPtr, ptrtype, ptr, delta);
            }
            return (rptr, rlen, rcap);
        }

        private partial struct u642fcvtTab
        {
            public ssa.Op geq;
            public ssa.Op cvt2F;
            public ssa.Op and;
            public ssa.Op rsh;
            public ssa.Op or;
            public ssa.Op add;
            public Func<ref state, ref types.Type, long, ref ssa.Value> one;
        }

        private static u642fcvtTab u64_f64 = new u642fcvtTab(geq:ssa.OpGeq64,cvt2F:ssa.OpCvt64to64F,and:ssa.OpAnd64,rsh:ssa.OpRsh64Ux64,or:ssa.OpOr64,add:ssa.OpAdd64F,one:(*state).constInt64,);

        private static u642fcvtTab u64_f32 = new u642fcvtTab(geq:ssa.OpGeq64,cvt2F:ssa.OpCvt64to32F,and:ssa.OpAnd64,rsh:ssa.OpRsh64Ux64,or:ssa.OpOr64,add:ssa.OpAdd32F,one:(*state).constInt64,);

        private static ref ssa.Value uint64Tofloat64(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.uint64Tofloat(ref u64_f64, n, x, ft, tt);
        }

        private static ref ssa.Value uint64Tofloat32(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.uint64Tofloat(ref u64_f32, n, x, ft, tt);
        }

        private static ref ssa.Value uint64Tofloat(this ref state s, ref u642fcvtTab cvttab, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        { 
            // if x >= 0 {
            //    result = (floatY) x
            // } else {
            //       y = uintX(x) ; y = x & 1
            //       z = uintX(x) ; z = z >> 1
            //       z = z >> 1
            //       z = z | y
            //       result = floatY(z)
            //       result = result + result
            // }
            //
            // Code borrowed from old code generator.
            // What's going on: large 64-bit "unsigned" looks like
            // negative number to hardware's integer-to-float
            // conversion. However, because the mantissa is only
            // 63 bits, we don't need the LSB, so instead we do an
            // unsigned right shift (divide by two), convert, and
            // double. However, before we do that, we need to be
            // sure that we do not lose a "1" if that made the
            // difference in the resulting rounding. Therefore, we
            // preserve it, and OR (not ADD) it back in. The case
            // that matters is when the eleven discarded bits are
            // equal to 10000000001; that rounds up, and the 1 cannot
            // be lost else it would round down if the LSB of the
            // candidate mantissa is 0.
            var cmp = s.newValue2(cvttab.geq, types.Types[TBOOL], x, s.zeroVal(ft));
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvt2F, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var one = cvttab.one(s, ft, 1L);
            var y = s.newValue2(cvttab.and, ft, x, one);
            var z = s.newValue2(cvttab.rsh, ft, x, one);
            z = s.newValue2(cvttab.or, ft, z, y);
            var a = s.newValue1(cvttab.cvt2F, tt, z);
            var a1 = s.newValue2(cvttab.add, tt, a, a);
            s.vars[n] = a1;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return s.variable(n, n.Type);
        }

        private partial struct u322fcvtTab
        {
            public ssa.Op cvtI2F;
            public ssa.Op cvtF2F;
        }

        private static u322fcvtTab u32_f64 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to64F,cvtF2F:ssa.OpCopy,);

        private static u322fcvtTab u32_f32 = new u322fcvtTab(cvtI2F:ssa.OpCvt32to32F,cvtF2F:ssa.OpCvt64Fto32F,);

        private static ref ssa.Value uint32Tofloat64(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.uint32Tofloat(ref u32_f64, n, x, ft, tt);
        }

        private static ref ssa.Value uint32Tofloat32(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.uint32Tofloat(ref u32_f32, n, x, ft, tt);
        }

        private static ref ssa.Value uint32Tofloat(this ref state s, ref u322fcvtTab cvttab, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        { 
            // if x >= 0 {
            //     result = floatY(x)
            // } else {
            //     result = floatY(float64(x) + (1<<32))
            // }
            var cmp = s.newValue2(ssa.OpGeq32, types.Types[TBOOL], x, s.zeroVal(ft));
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvtI2F, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var a1 = s.newValue1(ssa.OpCvt32to64F, types.Types[TFLOAT64], x);
            var twoToThe32 = s.constFloat64(types.Types[TFLOAT64], float64(1L << (int)(32L)));
            var a2 = s.newValue2(ssa.OpAdd64F, types.Types[TFLOAT64], a1, twoToThe32);
            var a3 = s.newValue1(cvttab.cvtF2F, tt, a2);

            s.vars[n] = a3;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return s.variable(n, n.Type);
        }

        // referenceTypeBuiltin generates code for the len/cap builtins for maps and channels.
        private static ref ssa.Value referenceTypeBuiltin(this ref state s, ref Node n, ref ssa.Value x)
        {
            if (!n.Left.Type.IsMap() && !n.Left.Type.IsChan())
            {
                s.Fatalf("node must be a map or a channel");
            } 
            // if n == nil {
            //   return 0
            // } else {
            //   // len
            //   return *((*int)n)
            //   // cap
            //   return *(((*int)n)+1)
            // }
            var lenType = n.Type;
            var nilValue = s.constNil(types.Types[TUINTPTR]);
            var cmp = s.newValue2(ssa.OpEqPtr, types.Types[TBOOL], x, nilValue);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchUnlikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain); 

            // length/capacity of a nil map/chan is zero
            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            s.vars[n] = s.zeroVal(lenType);
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);

            if (n.Op == OLEN) 
                // length is stored in the first word for map/chan
                s.vars[n] = s.newValue2(ssa.OpLoad, lenType, x, s.mem());
            else if (n.Op == OCAP) 
                // capacity is stored in the second word for chan
                var sw = s.newValue1I(ssa.OpOffPtr, lenType.PtrTo(), lenType.Width, x);
                s.vars[n] = s.newValue2(ssa.OpLoad, lenType, sw, s.mem());
            else 
                s.Fatalf("op must be OLEN or OCAP");
                        s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return s.variable(n, lenType);
        }

        private partial struct f2uCvtTab
        {
            public ssa.Op ltf;
            public ssa.Op cvt2U;
            public ssa.Op subf;
            public ssa.Op or;
            public Func<ref state, ref types.Type, double, ref ssa.Value> floatValue;
            public Func<ref state, ref types.Type, long, ref ssa.Value> intValue;
            public ulong cutoff;
        }

        private static f2uCvtTab f32_u64 = new f2uCvtTab(ltf:ssa.OpLess32F,cvt2U:ssa.OpCvt32Fto64,subf:ssa.OpSub32F,or:ssa.OpOr64,floatValue:(*state).constFloat32,intValue:(*state).constInt64,cutoff:9223372036854775808,);

        private static f2uCvtTab f64_u64 = new f2uCvtTab(ltf:ssa.OpLess64F,cvt2U:ssa.OpCvt64Fto64,subf:ssa.OpSub64F,or:ssa.OpOr64,floatValue:(*state).constFloat64,intValue:(*state).constInt64,cutoff:9223372036854775808,);

        private static f2uCvtTab f32_u32 = new f2uCvtTab(ltf:ssa.OpLess32F,cvt2U:ssa.OpCvt32Fto32,subf:ssa.OpSub32F,or:ssa.OpOr32,floatValue:(*state).constFloat32,intValue:func(s*state,t*types.Type,vint64)*ssa.Value{returns.constInt32(t,int32(v))},cutoff:2147483648,);

        private static f2uCvtTab f64_u32 = new f2uCvtTab(ltf:ssa.OpLess64F,cvt2U:ssa.OpCvt64Fto32,subf:ssa.OpSub64F,or:ssa.OpOr32,floatValue:(*state).constFloat64,intValue:func(s*state,t*types.Type,vint64)*ssa.Value{returns.constInt32(t,int32(v))},cutoff:2147483648,);

        private static ref ssa.Value float32ToUint64(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.floatToUint(ref f32_u64, n, x, ft, tt);
        }
        private static ref ssa.Value float64ToUint64(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.floatToUint(ref f64_u64, n, x, ft, tt);
        }

        private static ref ssa.Value float32ToUint32(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.floatToUint(ref f32_u32, n, x, ft, tt);
        }

        private static ref ssa.Value float64ToUint32(this ref state s, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        {
            return s.floatToUint(ref f64_u32, n, x, ft, tt);
        }

        private static ref ssa.Value floatToUint(this ref state s, ref f2uCvtTab cvttab, ref Node n, ref ssa.Value x, ref types.Type ft, ref types.Type tt)
        { 
            // cutoff:=1<<(intY_Size-1)
            // if x < floatX(cutoff) {
            //     result = uintY(x)
            // } else {
            //     y = x - floatX(cutoff)
            //     z = uintY(y)
            //     result = z | -(cutoff)
            // }
            var cutoff = cvttab.floatValue(s, ft, float64(cvttab.cutoff));
            var cmp = s.newValue2(cvttab.ltf, types.Types[TBOOL], x, cutoff);
            var b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cmp);
            b.Likely = ssa.BranchLikely;

            var bThen = s.f.NewBlock(ssa.BlockPlain);
            var bElse = s.f.NewBlock(ssa.BlockPlain);
            var bAfter = s.f.NewBlock(ssa.BlockPlain);

            b.AddEdgeTo(bThen);
            s.startBlock(bThen);
            var a0 = s.newValue1(cvttab.cvt2U, tt, x);
            s.vars[n] = a0;
            s.endBlock();
            bThen.AddEdgeTo(bAfter);

            b.AddEdgeTo(bElse);
            s.startBlock(bElse);
            var y = s.newValue2(cvttab.subf, ft, x, cutoff);
            y = s.newValue1(cvttab.cvt2U, tt, y);
            var z = cvttab.intValue(s, tt, int64(-cvttab.cutoff));
            var a1 = s.newValue2(cvttab.or, tt, y, z);
            s.vars[n] = a1;
            s.endBlock();
            bElse.AddEdgeTo(bAfter);

            s.startBlock(bAfter);
            return s.variable(n, n.Type);
        }

        // dottype generates SSA for a type assertion node.
        // commaok indicates whether to panic or return a bool.
        // If commaok is false, resok will be nil.
        private static (ref ssa.Value, ref ssa.Value) dottype(this ref state s, ref Node n, bool commaok)
        {
            var iface = s.expr(n.Left); // input interface
            var target = s.expr(n.Right); // target type
            var byteptr = s.f.Config.Types.BytePtr;

            if (n.Type.IsInterface())
            {
                if (n.Type.IsEmptyInterface())
                { 
                    // Converting to an empty interface.
                    // Input could be an empty or nonempty interface.
                    if (Debug_typeassert > 0L)
                    {
                        Warnl(n.Pos, "type assertion inlined");
                    } 

                    // Get itab/type field from input.
                    var itab = s.newValue1(ssa.OpITab, byteptr, iface); 
                    // Conversion succeeds iff that field is not nil.
                    var cond = s.newValue2(ssa.OpNeqPtr, types.Types[TBOOL], itab, s.constNil(byteptr));

                    if (n.Left.Type.IsEmptyInterface() && commaok)
                    { 
                        // Converting empty interface to empty interface with ,ok is just a nil check.
                        return (iface, cond);
                    } 

                    // Branch on nilness.
                    var b = s.endBlock();
                    b.Kind = ssa.BlockIf;
                    b.SetControl(cond);
                    b.Likely = ssa.BranchLikely;
                    var bOk = s.f.NewBlock(ssa.BlockPlain);
                    var bFail = s.f.NewBlock(ssa.BlockPlain);
                    b.AddEdgeTo(bOk);
                    b.AddEdgeTo(bFail);

                    if (!commaok)
                    { 
                        // On failure, panic by calling panicnildottype.
                        s.startBlock(bFail);
                        s.rtcall(panicnildottype, false, null, target); 

                        // On success, return (perhaps modified) input interface.
                        s.startBlock(bOk);
                        if (n.Left.Type.IsEmptyInterface())
                        {
                            res = iface; // Use input interface unchanged.
                            return;
                        } 
                        // Load type out of itab, build interface with existing idata.
                        var off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(Widthptr), itab);
                        var typ = s.newValue2(ssa.OpLoad, byteptr, off, s.mem());
                        var idata = s.newValue1(ssa.OpIData, n.Type, iface);
                        res = s.newValue2(ssa.OpIMake, n.Type, typ, idata);
                        return;
                    }
                    s.startBlock(bOk); 
                    // nonempty -> empty
                    // Need to load type from itab
                    off = s.newValue1I(ssa.OpOffPtr, byteptr, int64(Widthptr), itab);
                    s.vars[ref typVar] = s.newValue2(ssa.OpLoad, byteptr, off, s.mem());
                    s.endBlock(); 

                    // itab is nil, might as well use that as the nil result.
                    s.startBlock(bFail);
                    s.vars[ref typVar] = itab;
                    s.endBlock(); 

                    // Merge point.
                    var bEnd = s.f.NewBlock(ssa.BlockPlain);
                    bOk.AddEdgeTo(bEnd);
                    bFail.AddEdgeTo(bEnd);
                    s.startBlock(bEnd);
                    idata = s.newValue1(ssa.OpIData, n.Type, iface);
                    res = s.newValue2(ssa.OpIMake, n.Type, s.variable(ref typVar, byteptr), idata);
                    resok = cond;
                    delete(s.vars, ref typVar);
                    return;
                } 
                // converting to a nonempty interface needs a runtime call.
                if (Debug_typeassert > 0L)
                {
                    Warnl(n.Pos, "type assertion not inlined");
                }
                if (n.Left.Type.IsEmptyInterface())
                {
                    if (commaok)
                    {
                        var call = s.rtcall(assertE2I2, true, new slice<ref types.Type>(new ref types.Type[] { n.Type, types.Types[TBOOL] }), target, iface);
                        return (call[0L], call[1L]);
                    }
                    return (s.rtcall(assertE2I, true, new slice<ref types.Type>(new ref types.Type[] { n.Type }), target, iface)[0L], null);
                }
                if (commaok)
                {
                    call = s.rtcall(assertI2I2, true, new slice<ref types.Type>(new ref types.Type[] { n.Type, types.Types[TBOOL] }), target, iface);
                    return (call[0L], call[1L]);
                }
                return (s.rtcall(assertI2I, true, new slice<ref types.Type>(new ref types.Type[] { n.Type }), target, iface)[0L], null);
            }
            if (Debug_typeassert > 0L)
            {
                Warnl(n.Pos, "type assertion inlined");
            } 

            // Converting to a concrete type.
            var direct = isdirectiface(n.Type);
            itab = s.newValue1(ssa.OpITab, byteptr, iface); // type word of interface
            if (Debug_typeassert > 0L)
            {
                Warnl(n.Pos, "type assertion inlined");
            }
            ref ssa.Value targetITab = default;
            if (n.Left.Type.IsEmptyInterface())
            { 
                // Looking for pointer to target type.
                targetITab = target;
            }
            else
            { 
                // Looking for pointer to itab for target type and source interface.
                targetITab = s.expr(n.List.First());
            }
            ref Node tmp = default; // temporary for use with large types
            ref ssa.Value addr = default; // address of tmp
            if (commaok && !canSSAType(n.Type))
            { 
                // unSSAable type, use temporary.
                // TODO: get rid of some of these temporaries.
                tmp = tempAt(n.Pos, s.curfn, n.Type);
                addr = s.addr(tmp, false);
                s.vars[ref memVar] = s.newValue1A(ssa.OpVarDef, types.TypeMem, tmp, s.mem());
            }
            cond = s.newValue2(ssa.OpEqPtr, types.Types[TBOOL], itab, targetITab);
            b = s.endBlock();
            b.Kind = ssa.BlockIf;
            b.SetControl(cond);
            b.Likely = ssa.BranchLikely;

            bOk = s.f.NewBlock(ssa.BlockPlain);
            bFail = s.f.NewBlock(ssa.BlockPlain);
            b.AddEdgeTo(bOk);
            b.AddEdgeTo(bFail);

            if (!commaok)
            { 
                // on failure, panic by calling panicdottype
                s.startBlock(bFail);
                var taddr = s.expr(n.Right.Right);
                if (n.Left.Type.IsEmptyInterface())
                {
                    s.rtcall(panicdottypeE, false, null, itab, target, taddr);
                }
                else
                {
                    s.rtcall(panicdottypeI, false, null, itab, target, taddr);
                } 

                // on success, return data from interface
                s.startBlock(bOk);
                if (direct)
                {
                    return (s.newValue1(ssa.OpIData, n.Type, iface), null);
                }
                var p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                return (s.newValue2(ssa.OpLoad, n.Type, p, s.mem()), null);
            } 

            // commaok is the more complicated case because we have
            // a control flow merge point.
            bEnd = s.f.NewBlock(ssa.BlockPlain); 
            // Note that we need a new valVar each time (unlike okVar where we can
            // reuse the variable) because it might have a different type every time.
            Node valVar = ref new Node(Op:ONAME,Sym:&types.Sym{Name:"val"}); 

            // type assertion succeeded
            s.startBlock(bOk);
            if (tmp == null)
            {
                if (direct)
                {
                    s.vars[valVar] = s.newValue1(ssa.OpIData, n.Type, iface);
                }
                else
                {
                    p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                    s.vars[valVar] = s.newValue2(ssa.OpLoad, n.Type, p, s.mem());
                }
            }
            else
            {
                p = s.newValue1(ssa.OpIData, types.NewPtr(n.Type), iface);
                var store = s.newValue3I(ssa.OpMove, types.TypeMem, n.Type.Size(), addr, p, s.mem());
                store.Aux = n.Type;
                s.vars[ref memVar] = store;
            }
            s.vars[ref okVar] = s.constBool(true);
            s.endBlock();
            bOk.AddEdgeTo(bEnd); 

            // type assertion failed
            s.startBlock(bFail);
            if (tmp == null)
            {
                s.vars[valVar] = s.zeroVal(n.Type);
            }
            else
            {
                store = s.newValue2I(ssa.OpZero, types.TypeMem, n.Type.Size(), addr, s.mem());
                store.Aux = n.Type;
                s.vars[ref memVar] = store;
            }
            s.vars[ref okVar] = s.constBool(false);
            s.endBlock();
            bFail.AddEdgeTo(bEnd); 

            // merge point
            s.startBlock(bEnd);
            if (tmp == null)
            {
                res = s.variable(valVar, n.Type);
                delete(s.vars, valVar);
            }
            else
            {
                res = s.newValue2(ssa.OpLoad, n.Type, addr, s.mem());
                s.vars[ref memVar] = s.newValue1A(ssa.OpVarKill, types.TypeMem, tmp, s.mem());
            }
            resok = s.variable(ref okVar, types.Types[TBOOL]);
            delete(s.vars, ref okVar);
            return (res, resok);
        }

        // variable returns the value of a variable at the current location.
        private static ref ssa.Value variable(this ref state s, ref Node name, ref types.Type t)
        {
            var v = s.vars[name];
            if (v != null)
            {
                return v;
            }
            v = s.fwdVars[name];
            if (v != null)
            {
                return v;
            }
            if (s.curBlock == s.f.Entry)
            { 
                // No variable should be live at entry.
                s.Fatalf("Value live at entry. It shouldn't be. func %s, node %v, value %v", s.f.Name, name, v);
            } 
            // Make a FwdRef, which records a value that's live on block input.
            // We'll find the matching definition as part of insertPhis.
            v = s.newValue0A(ssa.OpFwdRef, t, name);
            s.fwdVars[name] = v;
            s.addNamedValue(name, v);
            return v;
        }

        private static ref ssa.Value mem(this ref state s)
        {
            return s.variable(ref memVar, types.TypeMem);
        }

        private static void addNamedValue(this ref state s, ref Node n, ref ssa.Value v)
        {
            if (n.Class() == Pxxx)
            { 
                // Don't track our dummy nodes (&memVar etc.).
                return;
            }
            if (n.IsAutoTmp())
            { 
                // Don't track temporary variables.
                return;
            }
            if (n.Class() == PPARAMOUT)
            { 
                // Don't track named output values.  This prevents return values
                // from being assigned too early. See #14591 and #14762. TODO: allow this.
                return;
            }
            if (n.Class() == PAUTO && n.Xoffset != 0L)
            {
                s.Fatalf("AUTO var with offset %v %d", n, n.Xoffset);
            }
            ssa.LocalSlot loc = new ssa.LocalSlot(N:n,Type:n.Type,Off:0);
            var (values, ok) = s.f.NamedValues[loc];
            if (!ok)
            {
                s.f.Names = append(s.f.Names, loc);
            }
            s.f.NamedValues[loc] = append(values, v);
        }

        // Branch is an unresolved branch.
        public partial struct Branch
        {
            public ptr<obj.Prog> P; // branch instruction
            public ptr<ssa.Block> B; // target
        }

        // SSAGenState contains state needed during Prog generation.
        public partial struct SSAGenState
        {
            public ptr<Progs> pp; // Branches remembers all the branch instructions we've seen
// and where they would like to go.
            public slice<Branch> Branches; // bstart remembers where each block starts (indexed by block ID)
            public slice<ref obj.Prog> bstart; // 387 port: maps from SSE registers (REG_X?) to 387 registers (REG_F?)
            public map<short, short> SSEto387; // Some architectures require a 64-bit temporary for FP-related register shuffling. Examples include x86-387, PPC, and Sparc V8.
            public ptr<Node> ScratchFpMem;
            public long maxarg; // largest frame size for arguments to calls made by the function

// Map from GC safe points to stack map index, generated by
// liveness analysis.
            public map<ref ssa.Value, long> stackMapIndex;
        }

        // Prog appends a new Prog.
        private static ref obj.Prog Prog(this ref SSAGenState s, obj.As @as)
        {
            return s.pp.Prog(as);
        }

        // Pc returns the current Prog.
        private static ref obj.Prog Pc(this ref SSAGenState s)
        {
            return s.pp.next;
        }

        // SetPos sets the current source position.
        private static void SetPos(this ref SSAGenState s, src.XPos pos)
        {
            s.pp.pos = pos;
        }

        // DebugFriendlySetPos sets the position subject to heuristics
        // that reduce "jumpy" line number churn when debugging.
        // Spill/fill/copy instructions from the register allocator,
        // phi functions, and instructions with a no-pos position
        // are examples of instructions that can cause churn.
        private static void DebugFriendlySetPosFrom(this ref SSAGenState s, ref ssa.Value v)
        { 
            // The two choices here are either to leave lineno unchanged,
            // or to explicitly set it to src.NoXPos.  Leaving it unchanged
            // (reusing the preceding line number) produces slightly better-
            // looking assembly language output from the compiler, and is
            // expected by some already-existing tests.
            // The debug information appears to be the same in either case

            if (v.Op == ssa.OpPhi || v.Op == ssa.OpCopy || v.Op == ssa.OpLoadReg || v.Op == ssa.OpStoreReg)             else 
                if (v.Pos != src.NoXPos)
                {
                    s.SetPos(v.Pos);
                }
                    }

        // genssa appends entries to pp for each instruction in f.
        private static void genssa(ref ssa.Func f, ref Progs pp)
        {
            SSAGenState s = default;

            ref ssafn e = f.Frontend()._<ref ssafn>();

            s.stackMapIndex = liveness(e, f); 

            // Remember where each block starts.
            s.bstart = make_slice<ref obj.Prog>(f.NumBlocks());
            s.pp = pp;
            map<ref obj.Prog, ref ssa.Value> progToValue = default;
            map<ref obj.Prog, ref ssa.Block> progToBlock = default;
            slice<ref obj.Prog> valueToProgAfter = default; // The first Prog following computation of a value v; v is visible at this point.
            var logProgs = e.log;
            if (logProgs)
            {
                progToValue = make_map<ref obj.Prog, ref ssa.Value>(f.NumValues());
                progToBlock = make_map<ref obj.Prog, ref ssa.Block>(f.NumBlocks());
                f.Logf("genssa %s\n", f.Name);
                progToBlock[s.pp.next] = f.Blocks[0L];
            }
            if (thearch.Use387)
            {
                s.SSEto387 = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<short, short>{};
            }
            s.ScratchFpMem = e.scratchFpMem;

            var logLocationLists = Debug_locationlist != 0L;
            if (Ctxt.Flag_locationlists)
            {
                e.curfn.Func.DebugInfo = ssa.BuildFuncDebug(f, logLocationLists);
                valueToProgAfter = make_slice<ref obj.Prog>(f.NumValues());
            } 

            // Emit basic blocks
            {
                var i__prev1 = i;
                var b__prev1 = b;

                foreach (var (__i, __b) in f.Blocks)
                {
                    i = __i;
                    b = __b;
                    s.bstart[b.ID] = s.pp.next; 
                    // Emit values in block
                    thearch.SSAMarkMoves(ref s, b);
                    {
                        var v__prev2 = v;

                        foreach (var (_, __v) in b.Values)
                        {
                            v = __v;
                            var x = s.pp.next;
                            s.DebugFriendlySetPosFrom(v);

                            if (v.Op == ssa.OpInitMem)                             else if (v.Op == ssa.OpArg)                             else if (v.Op == ssa.OpSP || v.Op == ssa.OpSB)                             else if (v.Op == ssa.OpSelect0 || v.Op == ssa.OpSelect1)                             else if (v.Op == ssa.OpGetG)                             else if (v.Op == ssa.OpVarDef || v.Op == ssa.OpVarLive || v.Op == ssa.OpKeepAlive)                             else if (v.Op == ssa.OpVarKill) 
                                // Zero variable if it is ambiguously live.
                                // After the VARKILL anything this variable references
                                // might be collected. If it were to become live again later,
                                // the GC will see references to already-collected objects.
                                // See issue 20029.
                                ref Node n = v.Aux._<ref Node>();
                                if (n.Name.Needzero())
                                {
                                    if (n.Class() != PAUTO)
                                    {
                                        v.Fatalf("zero of variable which isn't PAUTO %v", n);
                                    }
                                    if (n.Type.Size() % int64(Widthptr) != 0L)
                                    {
                                        v.Fatalf("zero of variable not a multiple of ptr size %v", n);
                                    }
                                    thearch.ZeroAuto(s.pp, n);
                                }
                            else if (v.Op == ssa.OpPhi) 
                                CheckLoweredPhi(v);
                            else if (v.Op == ssa.OpRegKill)                             else 
                                // let the backend handle it
                                thearch.SSAGenValue(ref s, v);
                                                        if (Ctxt.Flag_locationlists)
                            {
                                valueToProgAfter[v.ID] = s.pp.next;
                            }
                            if (logProgs)
                            {
                                while (x != s.pp.next)
                                {
                                    progToValue[x] = v;
                                    x = x.Link;
                                }

                            }
                        } 
                        // Emit control flow instructions for block

                        v = v__prev2;
                    }

                    ref ssa.Block next = default;
                    if (i < len(f.Blocks) - 1L && Debug['N'] == 0L)
                    { 
                        // If -N, leave next==nil so every block with successors
                        // ends in a JMP (except call blocks - plive doesn't like
                        // select{send,recv} followed by a JMP call).  Helps keep
                        // line numbers for otherwise empty blocks.
                        next = f.Blocks[i + 1L];
                    }
                    x = s.pp.next;
                    s.SetPos(b.Pos);
                    thearch.SSAGenBlock(ref s, b, next);
                    if (logProgs)
                    {
                        while (x != s.pp.next)
                        {
                            progToBlock[x] = b;
                            x = x.Link;
                        }

                    }
                }

                i = i__prev1;
                b = b__prev1;
            }

            if (Ctxt.Flag_locationlists)
            {
                {
                    var i__prev1 = i;

                    foreach (var (__i) in f.Blocks)
                    {
                        i = __i;
                        var blockDebug = e.curfn.Func.DebugInfo.Blocks[i];
                        foreach (var (_, locList) in blockDebug.Variables)
                        {
                            foreach (var (_, loc) in locList.Locations)
                            {
                                if (loc.Start == ssa.BlockStart)
                                {
                                    loc.StartProg = s.bstart[f.Blocks[i].ID];
                                }
                                else
                                {
                                    loc.StartProg = valueToProgAfter[loc.Start.ID];
                                }
                                if (loc.End == null)
                                {
                                    Fatalf("empty loc %v compiling %v", loc, f.Name);
                                }
                                if (loc.End == ssa.BlockEnd)
                                { 
                                    // If this variable was live at the end of the block, it should be
                                    // live over the control flow instructions. Extend it up to the
                                    // beginning of the next block.
                                    // If this is the last block, then there's no Prog to use for it, and
                                    // EndProg is unset.
                                    if (i < len(f.Blocks) - 1L)
                                    {
                                        loc.EndProg = s.bstart[f.Blocks[i + 1L].ID];
                                    }
                                }
                                else
                                { 
                                    // Advance the "end" forward by one; the end-of-range doesn't take effect
                                    // until the instruction actually executes.
                                    loc.EndProg = valueToProgAfter[loc.End.ID].Link;
                                    if (loc.EndProg == null)
                                    {
                                        Fatalf("nil loc.EndProg compiling %v, loc=%v", f.Name, loc);
                                    }
                                }
                                if (!logLocationLists)
                                {
                                    loc.Start = null;
                                    loc.End = null;
                                }
                            }
                        }
                    }

                    i = i__prev1;
                }

            } 

            // Resolve branches
            foreach (var (_, br) in s.Branches)
            {
                br.P.To.Val = s.bstart[br.B.ID];
            }
            if (logProgs)
            {
                @string filename = "";
                {
                    var p__prev1 = p;

                    var p = pp.Text;

                    while (p != null)
                    {
                        if (p.Pos.IsKnown() && p.InnermostFilename() != filename)
                        {
                            filename = p.InnermostFilename();
                            f.Logf("# %s\n", filename);
                        p = p.Link;
                        }
                        s = default;
                        {
                            var v__prev2 = v;

                            var (v, ok) = progToValue[p];

                            if (ok)
                            {
                                s = v.String();
                            }                            {
                                var b__prev3 = b;

                                var (b, ok) = progToBlock[p];


                                else if (ok)
                                {
                                    s = b.String();
                                }
                                else
                                {
                                    s = "   "; // most value and branch strings are 2-3 characters long
                                }

                                b = b__prev3;

                            }

                            v = v__prev2;

                        }
                        f.Logf(" %-6s\t%.5d (%s)\t%s\n", s, p.Pc, p.InnermostLineNumber(), p.InstructionString());
                    }


                    p = p__prev1;
                }
                if (f.HTMLWriter != null)
                { 
                    // LineHist is defunct now - this code won't do
                    // anything.
                    // TODO: fix this (ideally without a global variable)
                    // saved := pp.Text.Ctxt.LineHist.PrintFilenameOnly
                    // pp.Text.Ctxt.LineHist.PrintFilenameOnly = true
                    bytes.Buffer buf = default;
                    buf.WriteString("<code>");
                    buf.WriteString("<dl class=\"ssa-gen\">");
                    filename = "";
                    {
                        var p__prev1 = p;

                        p = pp.Text;

                        while (p != null)
                        { 
                            // Don't spam every line with the file name, which is often huge.
                            // Only print changes, and "unknown" is not a change.
                            if (p.Pos.IsKnown() && p.InnermostFilename() != filename)
                            {
                                filename = p.InnermostFilename();
                                buf.WriteString("<dt class=\"ssa-prog-src\"></dt><dd class=\"ssa-prog\">");
                                buf.WriteString(html.EscapeString("# " + filename));
                                buf.WriteString("</dd>");
                            p = p.Link;
                            }
                            buf.WriteString("<dt class=\"ssa-prog-src\">");
                            {
                                var v__prev3 = v;

                                (v, ok) = progToValue[p];

                                if (ok)
                                {
                                    buf.WriteString(v.HTML());
                                }                                {
                                    var b__prev4 = b;

                                    (b, ok) = progToBlock[p];


                                    else if (ok)
                                    {
                                        buf.WriteString("<b>" + b.HTML() + "</b>");
                                    }

                                    b = b__prev4;

                                }

                                v = v__prev3;

                            }
                            buf.WriteString("</dt>");
                            buf.WriteString("<dd class=\"ssa-prog\">");
                            buf.WriteString(fmt.Sprintf("%.5d <span class=\"line-number\">(%s)</span> %s", p.Pc, p.InnermostLineNumber(), html.EscapeString(p.InstructionString())));
                            buf.WriteString("</dd>");
                        }


                        p = p__prev1;
                    }
                    buf.WriteString("</dl>");
                    buf.WriteString("</code>");
                    f.HTMLWriter.WriteColumn("genssa", "ssa-prog", buf.String()); 
                    // pp.Text.Ctxt.LineHist.PrintFilenameOnly = saved
                }
            }
            defframe(ref s, e);
            if (Debug['f'] != 0L)
            {
                frame(0L);
            }
            f.HTMLWriter.Close();
            f.HTMLWriter = null;
        }

        private static void defframe(ref SSAGenState s, ref ssafn e)
        {
            var pp = s.pp;

            var frame = Rnd(s.maxarg + e.stksize, int64(Widthreg));
            if (thearch.PadFrame != null)
            {
                frame = thearch.PadFrame(frame);
            } 

            // Fill in argument and frame size.
            pp.Text.To.Type = obj.TYPE_TEXTSIZE;
            pp.Text.To.Val = int32(Rnd(e.curfn.Type.ArgWidth(), int64(Widthreg)));
            pp.Text.To.Offset = frame; 

            // Insert code to zero ambiguously live variables so that the
            // garbage collector only sees initialized values when it
            // looks for pointers.
            var p = pp.Text;
            long lo = default;            long hi = default; 

            // Opaque state for backend to use. Current backends use it to
            // keep track of which helper registers have been zeroed.
 

            // Opaque state for backend to use. Current backends use it to
            // keep track of which helper registers have been zeroed.
            uint state = default; 

            // Iterate through declarations. They are sorted in decreasing Xoffset order.
            foreach (var (_, n) in e.curfn.Func.Dcl)
            {
                if (!n.Name.Needzero())
                {
                    continue;
                }
                if (n.Class() != PAUTO)
                {
                    Fatalf("needzero class %d", n.Class());
                }
                if (n.Type.Size() % int64(Widthptr) != 0L || n.Xoffset % int64(Widthptr) != 0L || n.Type.Size() == 0L)
                {
                    Fatalf("var %L has size %d offset %d", n, n.Type.Size(), n.Xoffset);
                }
                if (lo != hi && n.Xoffset + n.Type.Size() >= lo - int64(2L * Widthreg))
                { 
                    // Merge with range we already have.
                    lo = n.Xoffset;
                    continue;
                } 

                // Zero old range
                p = thearch.ZeroRange(pp, p, frame + lo, hi - lo, ref state); 

                // Set new range.
                lo = n.Xoffset;
                hi = lo + n.Type.Size();
            } 

            // Zero final range.
            thearch.ZeroRange(pp, p, frame + lo, hi - lo, ref state);
        }

        public partial struct FloatingEQNEJump
        {
            public obj.As Jump;
            public long Index;
        }

        private static void oneFPJump(this ref SSAGenState s, ref ssa.Block b, ref FloatingEQNEJump jumps)
        {
            var p = s.Prog(jumps.Jump);
            p.To.Type = obj.TYPE_BRANCH;
            p.Pos = b.Pos;
            var to = jumps.Index;
            s.Branches = append(s.Branches, new Branch(p,b.Succs[to].Block()));
        }

        private static void FPJump(this ref SSAGenState s, ref ssa.Block b, ref ssa.Block next, ref array<array<FloatingEQNEJump>> jumps)
        {

            if (next == b.Succs[0L].Block()) 
                s.oneFPJump(b, ref jumps[0L][0L]);
                s.oneFPJump(b, ref jumps[0L][1L]);
            else if (next == b.Succs[1L].Block()) 
                s.oneFPJump(b, ref jumps[1L][0L]);
                s.oneFPJump(b, ref jumps[1L][1L]);
            else 
                s.oneFPJump(b, ref jumps[1L][0L]);
                s.oneFPJump(b, ref jumps[1L][1L]);
                var q = s.Prog(obj.AJMP);
                q.Pos = b.Pos;
                q.To.Type = obj.TYPE_BRANCH;
                s.Branches = append(s.Branches, new Branch(q,b.Succs[1].Block()));
                    }

        public static long AuxOffset(ref ssa.Value v)
        {
            if (v.Aux == null)
            {
                return 0L;
            }
            ref Node (n, ok) = v.Aux._<ref Node>();
            if (!ok)
            {
                v.Fatalf("bad aux type in %s\n", v.LongString());
            }
            if (n.Class() == PAUTO)
            {
                return n.Xoffset;
            }
            return 0L;
        }

        // AddAux adds the offset in the aux fields (AuxInt and Aux) of v to a.
        public static void AddAux(ref obj.Addr a, ref ssa.Value v)
        {
            AddAux2(a, v, v.AuxInt);
        }
        public static void AddAux2(ref obj.Addr a, ref ssa.Value v, long offset)
        {
            if (a.Type != obj.TYPE_MEM && a.Type != obj.TYPE_ADDR)
            {
                v.Fatalf("bad AddAux addr %v", a);
            } 
            // add integer offset
            a.Offset += offset; 

            // If no additional symbol offset, we're done.
            if (v.Aux == null)
            {
                return;
            } 
            // Add symbol's offset from its base register.
            switch (v.Aux.type())
            {
                case ref obj.LSym n:
                    a.Name = obj.NAME_EXTERN;
                    a.Sym = n;
                    break;
                case ref Node n:
                    if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
                    {
                        a.Name = obj.NAME_PARAM;
                        a.Sym = n.Orig.Sym.Linksym();
                        a.Offset += n.Xoffset;
                        break;
                    }
                    a.Name = obj.NAME_AUTO;
                    a.Sym = n.Sym.Linksym();
                    a.Offset += n.Xoffset;
                    break;
                default:
                {
                    var n = v.Aux.type();
                    v.Fatalf("aux in %s not implemented %#v", v, v.Aux);
                    break;
                }
            }
        }

        // extendIndex extends v to a full int width.
        // panic using the given function if v does not fit in an int (only on 32-bit archs).
        private static ref ssa.Value extendIndex(this ref state s, ref ssa.Value v, ref obj.LSym panicfn)
        {
            var size = v.Type.Size();
            if (size == s.config.PtrSize)
            {
                return v;
            }
            if (size > s.config.PtrSize)
            { 
                // truncate 64-bit indexes on 32-bit pointer archs. Test the
                // high word and branch to out-of-bounds failure if it is not 0.
                if (Debug['B'] == 0L)
                {
                    var hi = s.newValue1(ssa.OpInt64Hi, types.Types[TUINT32], v);
                    var cmp = s.newValue2(ssa.OpEq32, types.Types[TBOOL], hi, s.constInt32(types.Types[TUINT32], 0L));
                    s.check(cmp, panicfn);
                }
                return s.newValue1(ssa.OpTrunc64to32, types.Types[TINT], v);
            } 

            // Extend value to the required size
            ssa.Op op = default;
            if (v.Type.IsSigned())
            {
                switch (10L * size + s.config.PtrSize)
                {
                    case 14L: 
                        op = ssa.OpSignExt8to32;
                        break;
                    case 18L: 
                        op = ssa.OpSignExt8to64;
                        break;
                    case 24L: 
                        op = ssa.OpSignExt16to32;
                        break;
                    case 28L: 
                        op = ssa.OpSignExt16to64;
                        break;
                    case 48L: 
                        op = ssa.OpSignExt32to64;
                        break;
                    default: 
                        s.Fatalf("bad signed index extension %s", v.Type);
                        break;
                }
            }
            else
            {
                switch (10L * size + s.config.PtrSize)
                {
                    case 14L: 
                        op = ssa.OpZeroExt8to32;
                        break;
                    case 18L: 
                        op = ssa.OpZeroExt8to64;
                        break;
                    case 24L: 
                        op = ssa.OpZeroExt16to32;
                        break;
                    case 28L: 
                        op = ssa.OpZeroExt16to64;
                        break;
                    case 48L: 
                        op = ssa.OpZeroExt32to64;
                        break;
                    default: 
                        s.Fatalf("bad unsigned index extension %s", v.Type);
                        break;
                }
            }
            return s.newValue1(op, types.Types[TINT], v);
        }

        // CheckLoweredPhi checks that regalloc and stackalloc correctly handled phi values.
        // Called during ssaGenValue.
        public static void CheckLoweredPhi(ref ssa.Value v)
        {
            if (v.Op != ssa.OpPhi)
            {
                v.Fatalf("CheckLoweredPhi called with non-phi value: %v", v.LongString());
            }
            if (v.Type.IsMemory())
            {
                return;
            }
            var f = v.Block.Func;
            var loc = f.RegAlloc[v.ID];
            foreach (var (_, a) in v.Args)
            {
                {
                    var aloc = f.RegAlloc[a.ID];

                    if (aloc != loc)
                    { // TODO: .Equal() instead?
                        v.Fatalf("phi arg at different location than phi: %v @ %s, but arg %v @ %s\n%s\n", v, loc, a, aloc, v.Block.Func);
                    }

                }
            }
        }

        // CheckLoweredGetClosurePtr checks that v is the first instruction in the function's entry block.
        // The output of LoweredGetClosurePtr is generally hardwired to the correct register.
        // That register contains the closure pointer on closure entry.
        public static void CheckLoweredGetClosurePtr(ref ssa.Value v)
        {
            var entry = v.Block.Func.Entry;
            if (entry != v.Block || entry.Values[0L] != v)
            {
                Fatalf("in %s, badly placed LoweredGetClosurePtr: %v %v", v.Block.Func.Name, v.Block, v);
            }
        }

        // AutoVar returns a *Node and int64 representing the auto variable and offset within it
        // where v should be spilled.
        public static (ref Node, long) AutoVar(ref ssa.Value v)
        {
            ssa.LocalSlot loc = v.Block.Func.RegAlloc[v.ID]._<ssa.LocalSlot>();
            if (v.Type.Size() > loc.Type.Size())
            {
                v.Fatalf("spill/restore type %s doesn't fit in slot type %s", v.Type, loc.Type);
            }
            return (loc.N._<ref Node>(), loc.Off);
        }

        public static void AddrAuto(ref obj.Addr a, ref ssa.Value v)
        {
            var (n, off) = AutoVar(v);
            a.Type = obj.TYPE_MEM;
            a.Sym = n.Sym.Linksym();
            a.Reg = int16(thearch.REGSP);
            a.Offset = n.Xoffset + off;
            if (n.Class() == PPARAM || n.Class() == PPARAMOUT)
            {
                a.Name = obj.NAME_PARAM;
            }
            else
            {
                a.Name = obj.NAME_AUTO;
            }
        }

        private static void AddrScratch(this ref SSAGenState _s, ref obj.Addr _a) => func(_s, _a, (ref SSAGenState s, ref obj.Addr a, Defer _, Panic panic, Recover __) =>
        {
            if (s.ScratchFpMem == null)
            {
                panic("no scratch memory available; forgot to declare usesScratch for Op?");
            }
            a.Type = obj.TYPE_MEM;
            a.Name = obj.NAME_AUTO;
            a.Sym = s.ScratchFpMem.Sym.Linksym();
            a.Reg = int16(thearch.REGSP);
            a.Offset = s.ScratchFpMem.Xoffset;
        });

        private static ref obj.Prog Call(this ref SSAGenState s, ref ssa.Value v)
        {
            var (idx, ok) = s.stackMapIndex[v];
            if (!ok)
            {
                Fatalf("missing stack map index for %v", v.LongString());
            }
            var p = s.Prog(obj.APCDATA);
            Addrconst(ref p.From, objabi.PCDATA_StackMapIndex);
            Addrconst(ref p.To, int64(idx));

            {
                ref obj.LSym sym__prev1 = sym;

                ref obj.LSym (sym, _) = v.Aux._<ref obj.LSym>();

                if (sym == Deferreturn)
                { 
                    // Deferred calls will appear to be returning to
                    // the CALL deferreturn(SB) that we are about to emit.
                    // However, the stack trace code will show the line
                    // of the instruction byte before the return PC.
                    // To avoid that being an unrelated instruction,
                    // insert an actual hardware NOP that will have the right line number.
                    // This is different from obj.ANOP, which is a virtual no-op
                    // that doesn't make it into the instruction stream.
                    thearch.Ginsnop(s.pp);
                }

                sym = sym__prev1;

            }

            p = s.Prog(obj.ACALL);
            {
                ref obj.LSym sym__prev1 = sym;

                ref obj.LSym (sym, ok) = v.Aux._<ref obj.LSym>();

                if (ok)
                {
                    p.To.Type = obj.TYPE_MEM;
                    p.To.Name = obj.NAME_EXTERN;
                    p.To.Sym = sym; 

                    // Record call graph information for nowritebarrierrec
                    // analysis.
                    if (nowritebarrierrecCheck != null)
                    {
                        nowritebarrierrecCheck.recordCall(s.pp.curfn, sym, v.Pos);
                    }
                }
                else
                { 
                    // TODO(mdempsky): Can these differences be eliminated?

                    if (thearch.LinkArch.Family == sys.AMD64 || thearch.LinkArch.Family == sys.I386 || thearch.LinkArch.Family == sys.PPC64 || thearch.LinkArch.Family == sys.S390X) 
                        p.To.Type = obj.TYPE_REG;
                    else if (thearch.LinkArch.Family == sys.ARM || thearch.LinkArch.Family == sys.ARM64 || thearch.LinkArch.Family == sys.MIPS || thearch.LinkArch.Family == sys.MIPS64) 
                        p.To.Type = obj.TYPE_MEM;
                    else 
                        Fatalf("unknown indirect call family");
                                        p.To.Reg = v.Args[0L].Reg();
                }

                sym = sym__prev1;

            }
            if (s.maxarg < v.AuxInt)
            {
                s.maxarg = v.AuxInt;
            }
            return p;
        }

        // fieldIdx finds the index of the field referred to by the ODOT node n.
        private static long fieldIdx(ref Node _n) => func(_n, (ref Node n, Defer _, Panic panic, Recover __) =>
        {
            var t = n.Left.Type;
            var f = n.Sym;
            if (!t.IsStruct())
            {
                panic("ODOT's LHS is not a struct");
            }
            long i = default;
            foreach (var (_, t1) in t.Fields().Slice())
            {
                if (t1.Sym != f)
                {
                    i++;
                    continue;
                }
                if (t1.Offset != n.Xoffset)
                {
                    panic("field offset doesn't match");
                }
                return i;
            }
            panic(fmt.Sprintf("can't find field in expr %v\n", n)); 

            // TODO: keep the result of this function somewhere in the ODOT Node
            // so we don't have to recompute it each time we need it.
        });

        // ssafn holds frontend information about a function that the backend is processing.
        // It also exports a bunch of compiler services for the ssa backend.
        private partial struct ssafn
        {
            public ptr<Node> curfn;
            public ptr<Node> scratchFpMem; // temp for floating point register / memory moves on some architectures
            public long stksize; // stack size for current frame
            public long stkptrsize; // prefix of stack containing pointers
            public bool log;
        }

        // StringData returns a symbol (a *types.Sym wrapped in an interface) which
        // is the data component of a global string constant containing s.
        private static void StringData(this ref ssafn e, @string s)
        {
            {
                var (aux, ok) = e.strings[s];

                if (ok)
                {
                    return aux;
                }

            }
            if (e.strings == null)
            {
                e.strings = make();
            }
            var data = stringsym(e.curfn.Pos, s);
            e.strings[s] = data;
            return data;
        }

        private static ssa.GCNode Auto(this ref ssafn e, src.XPos pos, ref types.Type t)
        {
            var n = tempAt(pos, e.curfn, t); // Note: adds new auto to e.curfn.Func.Dcl list
            return n;
        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitString(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            var ptrType = types.NewPtr(types.Types[TUINT8]);
            var lenType = types.Types[TINT];
            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Split this string up into two separate variables.
                var p = e.splitSlot(ref name, ".ptr", 0L, ptrType);
                var l = e.splitSlot(ref name, ".len", ptrType.Size(), lenType);
                return (p, l);
            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:ptrType,Off:name.Off), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(Widthptr)));
        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitInterface(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            var t = types.NewPtr(types.Types[TUINT8]);
            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Split this interface up into two separate variables.
                @string f = ".itab";
                if (n.Type.IsEmptyInterface())
                {
                    f = ".type";
                }
                var c = e.splitSlot(ref name, f, 0L, t);
                var d = e.splitSlot(ref name, ".data", t.Size(), t);
                return (c, d);
            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off), new ssa.LocalSlot(N:n,Type:t,Off:name.Off+int64(Widthptr)));
        }

        private static (ssa.LocalSlot, ssa.LocalSlot, ssa.LocalSlot) SplitSlice(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            var ptrType = types.NewPtr(name.Type.ElemType());
            var lenType = types.Types[TINT];
            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Split this slice up into three separate variables.
                var p = e.splitSlot(ref name, ".ptr", 0L, ptrType);
                var l = e.splitSlot(ref name, ".len", ptrType.Size(), lenType);
                var c = e.splitSlot(ref name, ".cap", ptrType.Size() + lenType.Size(), lenType);
                return (p, l, c);
            } 
            // Return the three parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:ptrType,Off:name.Off), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(Widthptr)), new ssa.LocalSlot(N:n,Type:lenType,Off:name.Off+int64(2*Widthptr)));
        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitComplex(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            var s = name.Type.Size() / 2L;
            ref types.Type t = default;
            if (s == 8L)
            {
                t = types.Types[TFLOAT64];
            }
            else
            {
                t = types.Types[TFLOAT32];
            }
            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Split this complex up into two separate variables.
                var r = e.splitSlot(ref name, ".real", 0L, t);
                var i = e.splitSlot(ref name, ".imag", t.Size(), t);
                return (r, i);
            } 
            // Return the two parts of the larger variable.
            return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off), new ssa.LocalSlot(N:n,Type:t,Off:name.Off+s));
        }

        private static (ssa.LocalSlot, ssa.LocalSlot) SplitInt64(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            ref types.Type t = default;
            if (name.Type.IsSigned())
            {
                t = types.Types[TINT32];
            }
            else
            {
                t = types.Types[TUINT32];
            }
            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Split this int64 up into two separate variables.
                if (thearch.LinkArch.ByteOrder == binary.BigEndian)
                {
                    return (e.splitSlot(ref name, ".hi", 0L, t), e.splitSlot(ref name, ".lo", t.Size(), types.Types[TUINT32]));
                }
                return (e.splitSlot(ref name, ".hi", t.Size(), t), e.splitSlot(ref name, ".lo", 0L, types.Types[TUINT32]));
            } 
            // Return the two parts of the larger variable.
            if (thearch.LinkArch.ByteOrder == binary.BigEndian)
            {
                return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off), new ssa.LocalSlot(N:n,Type:types.Types[TUINT32],Off:name.Off+4));
            }
            return (new ssa.LocalSlot(N:n,Type:t,Off:name.Off+4), new ssa.LocalSlot(N:n,Type:types.Types[TUINT32],Off:name.Off));
        }

        private static ssa.LocalSlot SplitStruct(this ref ssafn e, ssa.LocalSlot name, long i)
        {
            ref Node n = name.N._<ref Node>();
            var st = name.Type;
            var ft = st.FieldType(i);
            long offset = default;
            for (long f = 0L; f < i; f++)
            {
                offset += st.FieldType(f).Size();
            }

            if (n.Class() == PAUTO && !n.Addrtaken())
            { 
                // Note: the _ field may appear several times.  But
                // have no fear, identically-named but distinct Autos are
                // ok, albeit maybe confusing for a debugger.
                return e.splitSlot(ref name, "." + st.FieldName(i), offset, ft);
            }
            return new ssa.LocalSlot(N:n,Type:ft,Off:name.Off+st.FieldOff(i));
        }

        private static ssa.LocalSlot SplitArray(this ref ssafn e, ssa.LocalSlot name)
        {
            ref Node n = name.N._<ref Node>();
            var at = name.Type;
            if (at.NumElem() != 1L)
            {
                Fatalf("bad array size");
            }
            var et = at.ElemType();
            if (n.Class() == PAUTO && !n.Addrtaken())
            {
                return e.splitSlot(ref name, "[0]", 0L, et);
            }
            return new ssa.LocalSlot(N:n,Type:et,Off:name.Off);
        }

        private static ref obj.LSym DerefItab(this ref ssafn e, ref obj.LSym it, long offset)
        {
            return itabsym(it, offset);
        }

        // splitSlot returns a slot representing the data of parent starting at offset.
        private static ssa.LocalSlot splitSlot(this ref ssafn e, ref ssa.LocalSlot parent, @string suffix, long offset, ref types.Type t)
        {
            types.Sym s = ref new types.Sym(Name:parent.N.(*Node).Sym.Name+suffix,Pkg:localpkg);

            Node n = ref new Node(Name:new(Name),Op:ONAME,Pos:parent.N.(*Node).Pos,);
            n.Orig = n;

            s.Def = asTypesNode(n);
            asNode(s.Def).Name.SetUsed(true);
            n.Sym = s;
            n.Type = t;
            n.SetClass(PAUTO);
            n.SetAddable(true);
            n.Esc = EscNever;
            n.Name.Curfn = e.curfn;
            e.curfn.Func.Dcl = append(e.curfn.Func.Dcl, n);
            dowidth(t);
            return new ssa.LocalSlot(N:n,Type:t,Off:0,SplitOf:parent,SplitOffset:offset);
        }

        private static bool CanSSA(this ref ssafn e, ref types.Type t)
        {
            return canSSAType(t);
        }

        private static @string Line(this ref ssafn e, src.XPos pos)
        {
            return linestr(pos);
        }

        // Log logs a message from the compiler.
        private static void Logf(this ref ssafn e, @string msg, params object[] args)
        {
            if (e.log)
            {
                fmt.Printf(msg, args);
            }
        }

        private static bool Log(this ref ssafn e)
        {
            return e.log;
        }

        // Fatal reports a compiler error and exits.
        private static void Fatalf(this ref ssafn e, src.XPos pos, @string msg, params object[] args)
        {
            lineno = pos;
            Fatalf(msg, args);
        }

        // Warnl reports a "warning", which is usually flag-triggered
        // logging output for the benefit of tests.
        private static void Warnl(this ref ssafn e, src.XPos pos, @string fmt_, params object[] args)
        {
            Warnl(pos, fmt_, args);
        }

        private static bool Debug_checknil(this ref ssafn e)
        {
            return Debug_checknil != 0L;
        }

        private static bool Debug_eagerwb(this ref ssafn e)
        {
            return Debug_eagerwb != 0L;
        }

        private static bool UseWriteBarrier(this ref ssafn e)
        {
            return use_writebarrier;
        }

        private static ref obj.LSym Syslook(this ref ssafn e, @string name)
        {
            switch (name)
            {
                case "goschedguarded": 
                    return goschedguarded;
                    break;
                case "writeBarrier": 
                    return writeBarrier;
                    break;
                case "writebarrierptr": 
                    return writebarrierptr;
                    break;
                case "gcWriteBarrier": 
                    return gcWriteBarrier;
                    break;
                case "typedmemmove": 
                    return typedmemmove;
                    break;
                case "typedmemclr": 
                    return typedmemclr;
                    break;
            }
            Fatalf("unknown Syslook func %v", name);
            return null;
        }

        private static void SetWBPos(this ref ssafn e, src.XPos pos)
        {
            e.curfn.Func.setWBPos(pos);
        }

        private static ref types.Type Typ(this ref Node n)
        {
            return n.Type;
        }
        private static ssa.StorageClass StorageClass(this ref Node n)
        {

            if (n.Class() == PPARAM) 
                return ssa.ClassParam;
            else if (n.Class() == PPARAMOUT) 
                return ssa.ClassParamOut;
            else if (n.Class() == PAUTO) 
                return ssa.ClassAuto;
            else 
                Fatalf("untranslateable storage class for %v: %s", n, n.Class());
                return 0L;
                    }
    }
}}}}

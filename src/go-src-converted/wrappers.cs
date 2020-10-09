// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:32 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\wrappers.go
// This file defines synthesis of Functions that delegate to declared
// methods; they come in three kinds:
//
// (1) wrappers: methods that wrap declared methods, performing
//     implicit pointer indirections and embedded field selections.
//
// (2) thunks: funcs that wrap declared methods.  Like wrappers,
//     thunks perform indirections and field selections. The thunk's
//     first parameter is used as the receiver for the method call.
//
// (3) bounds: funcs that wrap declared methods.  The bound's sole
//     free variable, supplied by a closure, is used as the receiver
//     for the method call.  No indirections or field selections are
//     performed since they can be done before the call.

using fmt = go.fmt_package;

using types = go.go.types_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // -- wrappers -----------------------------------------------------------

        // makeWrapper returns a synthetic method that delegates to the
        // declared method denoted by meth.Obj(), first performing any
        // necessary pointer indirections or field selections implied by meth.
        //
        // The resulting method's receiver type is meth.Recv().
        //
        // This function is versatile but quite subtle!  Consider the
        // following axes of variation when making changes:
        //   - optional receiver indirection
        //   - optional implicit field selections
        //   - meth.Obj() may denote a concrete or an interface method
        //   - the result may be a thunk or a wrapper.
        //
        // EXCLUSIVE_LOCKS_REQUIRED(prog.methodsMu)
        //
        private static ptr<Function> makeWrapper(ptr<Program> _addr_prog, ptr<types.Selection> _addr_sel) => func((defer, _, __) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Selection sel = ref _addr_sel.val;

            ptr<types.Func> obj = sel.Obj()._<ptr<types.Func>>(); // the declared function
            ptr<types.Signature> sig = sel.Type()._<ptr<types.Signature>>(); // type of this wrapper

            ptr<types.Var> recv; // wrapper's receiver or thunk's params[0]
            var name = obj.Name();
            @string description = default;
            long start = default; // first regular param
            if (sel.Kind() == types.MethodExpr)
            {
                name += "$thunk";
                description = "thunk";
                recv = sig.Params().At(0L);
                start = 1L;
            }
            else
            {
                description = "wrapper";
                recv = sig.Recv();
            }
            description = fmt.Sprintf("%s for %s", description, sel.Obj());
            if (prog.mode & LogSource != 0L)
            {
                defer(logStack("make %s to (%s)", description, recv.Type())());
            }
            ptr<Function> fn = addr(new Function(name:name,method:sel,object:obj,Signature:sig,Synthetic:description,Prog:prog,pos:obj.Pos(),));
            fn.startBody();
            fn.addSpilledParam(recv);
            createParams(fn, start);

            var indices = sel.Index();

            Value v = fn.Locals[0L]; // spilled receiver
            if (isPointer(sel.Recv()))
            {
                v = emitLoad(fn, v); 

                // For simple indirection wrappers, perform an informative nil-check:
                // "value method (T).f called using nil *T pointer"
                if (len(indices) == 1L && !isPointer(recvType(obj)))
                {
                    ref Call c = ref heap(out ptr<Call> _addr_c);
                    c.Call.Value = addr(new Builtin(name:"ssa:wrapnilchk",sig:types.NewSignature(nil,types.NewTuple(anonVar(sel.Recv()),anonVar(tString),anonVar(tString)),types.NewTuple(anonVar(sel.Recv())),false),));
                    c.Call.Args = new slice<Value>(new Value[] { v, stringConst(deref(sel.Recv()).String()), stringConst(sel.Obj().Name()) });
                    c.setType(v.Type());
                    v = fn.emit(_addr_c);
                }
            }
            v = emitImplicitSelections(fn, v, indices[..len(indices) - 1L]); 

            // Invariant: v is a pointer, either
            //   value of implicit *C field, or
            // address of implicit  C field.

            c = default;
            {
                var r = recvType(obj);

                if (!isInterface(r))
                { // concrete method
                    if (!isPointer(r))
                    {
                        v = emitLoad(fn, v);
                    }
                    c.Call.Value = prog.declaredFunc(obj);
                    c.Call.Args = append(c.Call.Args, v);

                }
                else
                {
                    c.Call.Method = obj;
                    c.Call.Value = emitLoad(fn, v);
                }
            }

            foreach (var (_, arg) in fn.Params[1L..])
            {
                c.Call.Args = append(c.Call.Args, arg);
            }            emitTailCall(fn, _addr_c);
            fn.finishBody();
            return _addr_fn!;

        });

        // createParams creates parameters for wrapper method fn based on its
        // Signature.Params, which do not include the receiver.
        // start is the index of the first regular parameter to use.
        //
        private static void createParams(ptr<Function> _addr_fn, long start)
        {
            ref Function fn = ref _addr_fn.val;

            var tparams = fn.Signature.Params();
            for (var i = start;
            var n = tparams.Len(); i < n; i++)
            {
                fn.addParamObj(tparams.At(i));
            }


        }

        // -- bounds -----------------------------------------------------------

        // makeBound returns a bound method wrapper (or "bound"), a synthetic
        // function that delegates to a concrete or interface method denoted
        // by obj.  The resulting function has no receiver, but has one free
        // variable which will be used as the method's receiver in the
        // tail-call.
        //
        // Use MakeClosure with such a wrapper to construct a bound method
        // closure.  e.g.:
        //
        //   type T int          or:  type T interface { meth() }
        //   func (t T) meth()
        //   var t T
        //   f := t.meth
        //   f() // calls t.meth()
        //
        // f is a closure of a synthetic wrapper defined as if by:
        //
        //   f := func() { return t.meth() }
        //
        // Unlike makeWrapper, makeBound need perform no indirection or field
        // selections because that can be done before the closure is
        // constructed.
        //
        // EXCLUSIVE_LOCKS_ACQUIRED(meth.Prog.methodsMu)
        //
        private static ptr<Function> makeBound(ptr<Program> _addr_prog, ptr<types.Func> _addr_obj) => func((defer, _, __) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Func obj = ref _addr_obj.val;

            prog.methodsMu.Lock();
            defer(prog.methodsMu.Unlock());
            var (fn, ok) = prog.bounds[obj];
            if (!ok)
            {
                var description = fmt.Sprintf("bound method wrapper for %s", obj);
                if (prog.mode & LogSource != 0L)
                {
                    defer(logStack("%s", description)());
                }

                fn = addr(new Function(name:obj.Name()+"$bound",object:obj,Signature:changeRecv(obj.Type().(*types.Signature),nil),Synthetic:description,Prog:prog,pos:obj.Pos(),));

                ptr<FreeVar> fv = addr(new FreeVar(name:"recv",typ:recvType(obj),parent:fn));
                fn.FreeVars = new slice<ptr<FreeVar>>(new ptr<FreeVar>[] { fv });
                fn.startBody();
                createParams(_addr_fn, 0L);
                ref Call c = ref heap(out ptr<Call> _addr_c);

                if (!isInterface(recvType(obj)))
                { // concrete
                    c.Call.Value = prog.declaredFunc(obj);
                    c.Call.Args = new slice<Value>(new Value[] { fv });

                }
                else
                {
                    c.Call.Value = fv;
                    c.Call.Method = obj;
                }

                foreach (var (_, arg) in fn.Params)
                {
                    c.Call.Args = append(c.Call.Args, arg);
                }
                emitTailCall(fn, _addr_c);
                fn.finishBody();

                prog.bounds[obj] = fn;

            }

            return _addr_fn!;

        });

        // -- thunks -----------------------------------------------------------

        // makeThunk returns a thunk, a synthetic function that delegates to a
        // concrete or interface method denoted by sel.Obj().  The resulting
        // function has no receiver, but has an additional (first) regular
        // parameter.
        //
        // Precondition: sel.Kind() == types.MethodExpr.
        //
        //   type T int          or:  type T interface { meth() }
        //   func (t T) meth()
        //   f := T.meth
        //   var t T
        //   f(t) // calls t.meth()
        //
        // f is a synthetic wrapper defined as if by:
        //
        //   f := func(t T) { return t.meth() }
        //
        // TODO(adonovan): opt: currently the stub is created even when used
        // directly in a function call: C.f(i, 0).  This is less efficient
        // than inlining the stub.
        //
        // EXCLUSIVE_LOCKS_ACQUIRED(meth.Prog.methodsMu)
        //
        private static ptr<Function> makeThunk(ptr<Program> _addr_prog, ptr<types.Selection> _addr_sel) => func((defer, panic, _) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Selection sel = ref _addr_sel.val;

            if (sel.Kind() != types.MethodExpr)
            {
                panic(sel);
            }

            selectionKey key = new selectionKey(kind:sel.Kind(),recv:sel.Recv(),obj:sel.Obj(),index:fmt.Sprint(sel.Index()),indirect:sel.Indirect(),);

            prog.methodsMu.Lock();
            defer(prog.methodsMu.Unlock()); 

            // Canonicalize key.recv to avoid constructing duplicate thunks.
            types.Type (canonRecv, ok) = prog.canon.At(key.recv)._<types.Type>();
            if (!ok)
            {
                canonRecv = key.recv;
                prog.canon.Set(key.recv, canonRecv);
            }

            key.recv = canonRecv;

            var (fn, ok) = prog.thunks[key];
            if (!ok)
            {
                fn = makeWrapper(_addr_prog, _addr_sel);
                if (fn.Signature.Recv() != null)
                {
                    panic(fn); // unexpected receiver
                }

                prog.thunks[key] = fn;

            }

            return _addr_fn!;

        });

        private static ptr<types.Signature> changeRecv(ptr<types.Signature> _addr_s, ptr<types.Var> _addr_recv)
        {
            ref types.Signature s = ref _addr_s.val;
            ref types.Var recv = ref _addr_recv.val;

            return _addr_types.NewSignature(recv, s.Params(), s.Results(), s.Variadic())!;
        }

        // selectionKey is like types.Selection but a usable map key.
        private partial struct selectionKey
        {
            public types.SelectionKind kind;
            public types.Type recv; // canonicalized via Program.canon
            public types.Object obj;
            public @string index;
            public bool indirect;
        }
    }
}}}}}

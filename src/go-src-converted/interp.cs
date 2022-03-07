// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ssa/interp defines an interpreter for the SSA
// representation of Go programs.
//
// This interpreter is provided as an adjunct for testing the SSA
// construction algorithm.  Its purpose is to provide a minimal
// metacircular implementation of the dynamic semantics of each SSA
// instruction.  It is not, and will never be, a production-quality Go
// interpreter.
//
// The following is a partial list of Go features that are currently
// unsupported or incomplete in the interpreter.
//
// * Unsafe operations, including all uses of unsafe.Pointer, are
// impossible to support given the "boxed" value representation we
// have chosen.
//
// * The reflect package is only partially implemented.
//
// * The "testing" package is no longer supported because it
// depends on low-level details that change too often.
//
// * "sync/atomic" operations are not atomic due to the "boxed" value
// representation: it is not possible to read, modify and write an
// interface value atomically. As a consequence, Mutexes are currently
// broken.
//
// * recover is only partially implemented.  Also, the interpreter
// makes no attempt to distinguish target panics from interpreter
// crashes.
//
// * the sizes of the int, uint and uintptr types in the target
// program are assumed to be the same as those of the interpreter
// itself.
//
// * all values occupy space, even those of types defined by the spec
// to have zero size, e.g. struct{}.  This can cause asymptotic
// performance degradation.
//
// * os.Exit is implemented using panic, causing deferred functions to
// run.
// package interp -- go2cs converted at 2022 March 06 23:33:32 UTC
// import "golang.org/x/tools/go/ssa/interp" ==> using interp = go.golang.org.x.tools.go.ssa.interp_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\interp.go
// import "golang.org/x/tools/go/ssa/interp"

using fmt = go.fmt_package;
using token = go.go.token_package;
using types = go.go.types_package;
using os = go.os_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;

using ssa = go.golang.org.x.tools.go.ssa_package;
using System;
using System.Threading;


namespace go.golang.org.x.tools.go.ssa;

public static partial class interp_package {

private partial struct continuation { // : nint
}

private static readonly continuation kNext = iota;
private static readonly var kReturn = 0;
private static readonly var kJump = 1;


// Mode is a bitmask of options affecting the interpreter.
public partial struct Mode { // : nuint
}

public static readonly Mode DisableRecover = 1 << (int)(iota); // Disable recover() in target programs; show interpreter crash instead.
public static readonly var EnableTracing = 0; // Print a trace of all instructions as they are interpreted.

private partial struct methodSet { // : map<@string, ptr<ssa.Function>>
}

// State shared between all interpreted goroutines.
private partial struct interpreter {
    public slice<value> osArgs; // the value of os.Args
    public ptr<ssa.Program> prog; // the SSA program
    public map<ssa.Value, ptr<value>> globals; // addresses of global variables (immutable)
    public Mode mode; // interpreter options
    public ptr<ssa.Package> reflectPackage; // the fake reflect package
    public methodSet errorMethods; // the method set of reflect.error, which implements the error interface.
    public methodSet rtypeMethods; // the method set of rtype, which implements the reflect.Type interface.
    public types.Type runtimeErrorString; // the runtime.errorString type
    public types.Sizes sizes; // the effective type-sizing function
    public int goroutines; // atomically updated
}

private partial struct deferred {
    public value fn;
    public slice<value> args;
    public ptr<ssa.Defer> instr;
    public ptr<deferred> tail;
}

private partial struct frame {
    public ptr<interpreter> i;
    public ptr<frame> caller;
    public ptr<ssa.Function> fn;
    public ptr<ssa.BasicBlock> block;
    public ptr<ssa.BasicBlock> prevBlock;
    public map<ssa.Value, value> env; // dynamic values of SSA variables
    public slice<value> locals;
    public ptr<deferred> defers;
    public value result;
    public bool panicking;
}

private static value get(this ptr<frame> _addr_fr, ssa.Value key) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;

    switch (key.type()) {
        case 
            return null;
            break;
        case ptr<ssa.Function> key:
            return key;
            break;
        case ptr<ssa.Builtin> key:
            return key;
            break;
        case ptr<ssa.Const> key:
            return constValue(key);
            break;
        case ptr<ssa.Global> key:
            {
                var r__prev1 = r;

                var (r, ok) = fr.i.globals[key];

                if (ok) {
                    return r;
                }

                r = r__prev1;

            }

            break;
    }
    {
        var r__prev1 = r;

        (r, ok) = fr.env[key];

        if (ok) {
            return r;
        }
        r = r__prev1;

    }

    panic(fmt.Sprintf("get: no value for %T: %v", key, key.Name()));

});

// runDefer runs a deferred call d.
// It always returns normally, but may set or clear fr.panic.
//
private static void runDefer(this ptr<frame> _addr_fr, ptr<deferred> _addr_d) => func((defer, _, _) => {
    ref frame fr = ref _addr_fr.val;
    ref deferred d = ref _addr_d.val;

    if (fr.i.mode & EnableTracing != 0) {
        fmt.Fprintf(os.Stderr, "%s: invoking deferred function call\n", fr.i.prog.Fset.Position(d.instr.Pos()));
    }
    bool ok = default;
    defer(() => {
        if (!ok) { 
            // Deferred call created a new state of panic.
            fr.panicking = true;
            fr.panic = recover();

        }
    }());
    call(_addr_fr.i, _addr_fr, d.instr.Pos(), d.fn, d.args);
    ok = true;

});

// runDefers executes fr's deferred function calls in LIFO order.
//
// On entry, fr.panicking indicates a state of panic; if
// true, fr.panic contains the panic value.
//
// On completion, if a deferred call started a panic, or if no
// deferred call recovered from a previous state of panic, then
// runDefers itself panics after the last deferred call has run.
//
// If there was no initial state of panic, or it was recovered from,
// runDefers returns normally.
//
private static void runDefers(this ptr<frame> _addr_fr) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;

    {
        var d = fr.defers;

        while (d != null) {
            fr.runDefer(d);
            d = d.tail;
        }
    }
    fr.defers = null;
    if (fr.panicking) {
        panic(fr.panic); // new panic, or still panicking
    }
});

// lookupMethod returns the method set for type typ, which may be one
// of the interpreter's fake types.
private static ptr<ssa.Function> lookupMethod(ptr<interpreter> _addr_i, types.Type typ, ptr<types.Func> _addr_meth) {
    ref interpreter i = ref _addr_i.val;
    ref types.Func meth = ref _addr_meth.val;


    if (typ == rtypeType) 
        return _addr_i.rtypeMethods[meth.Id()]!;
    else if (typ == errorType) 
        return _addr_i.errorMethods[meth.Id()]!;
        return _addr_i.prog.LookupMethod(typ, meth.Pkg(), meth.Name())!;

}

// visitInstr interprets a single ssa.Instruction within the activation
// record frame.  It returns a continuation value indicating where to
// read the next instruction from.
private static continuation visitInstr(ptr<frame> _addr_fr, ssa.Instruction instr) => func((_, panic, _) => {
    ref frame fr = ref _addr_fr.val;

    switch (instr.type()) {
        case ptr<ssa.DebugRef> instr:
            break;
        case ptr<ssa.UnOp> instr:
            fr.env[instr] = unop(instr, fr.get(instr.X));
            break;
        case ptr<ssa.BinOp> instr:
            fr.env[instr] = binop(instr.Op, instr.X.Type(), fr.get(instr.X), fr.get(instr.Y));
            break;
        case ptr<ssa.Call> instr:
            var (fn, args) = prepareCall(_addr_fr, _addr_instr.Call);
            fr.env[instr] = call(_addr_fr.i, _addr_fr, instr.Pos(), fn, args);
            break;
        case ptr<ssa.ChangeInterface> instr:
            fr.env[instr] = fr.get(instr.X);
            break;
        case ptr<ssa.ChangeType> instr:
            fr.env[instr] = fr.get(instr.X); // (can't fail)
            break;
        case ptr<ssa.Convert> instr:
            fr.env[instr] = conv(instr.Type(), instr.X.Type(), fr.get(instr.X));
            break;
        case ptr<ssa.MakeInterface> instr:
            fr.env[instr] = new iface(t:instr.X.Type(),v:fr.get(instr.X));
            break;
        case ptr<ssa.Extract> instr:
            fr.env[instr] = fr.get(instr.Tuple)._<tuple>()[instr.Index];
            break;
        case ptr<ssa.Slice> instr:
            fr.env[instr] = slice(fr.get(instr.X), fr.get(instr.Low), fr.get(instr.High), fr.get(instr.Max));
            break;
        case ptr<ssa.Return> instr:
            switch (len(instr.Results)) {
                case 0: 

                    break;
                case 1: 
                    fr.result = fr.get(instr.Results[0]);
                    break;
                default: 
                    slice<value> res = default;
                    {
                        var r__prev1 = r;

                        foreach (var (_, __r) in instr.Results) {
                            r = __r;
                            res = append(res, fr.get(r));
                        }

                        r = r__prev1;
                    }

                    fr.result = tuple(res);

                    break;
            }
            fr.block = null;
            return kReturn;
            break;
        case ptr<ssa.RunDefers> instr:
            fr.runDefers();
            break;
        case ptr<ssa.Panic> instr:
            panic(new targetPanic(fr.get(instr.X)));
            break;
        case ptr<ssa.Send> instr:
            fr.get(instr.Chan)._<channel<value>>().Send(fr.get(instr.X));
            break;
        case ptr<ssa.Store> instr:
            store(deref(instr.Addr.Type()), fr.get(instr.Addr)._<ptr<value>>(), fr.get(instr.Val));
            break;
        case ptr<ssa.If> instr:
            nint succ = 1;
            if (fr.get(instr.Cond)._<bool>()) {
                succ = 0;
            }
            (fr.prevBlock, fr.block) = (fr.block, fr.block.Succs[succ]);            return kJump;
            break;
        case ptr<ssa.Jump> instr:
            (fr.prevBlock, fr.block) = (fr.block, fr.block.Succs[0]);            return kJump;
            break;
        case ptr<ssa.Defer> instr:
            (fn, args) = prepareCall(_addr_fr, _addr_instr.Call);
            fr.defers = addr(new deferred(fn:fn,args:args,instr:instr,tail:fr.defers,));
            break;
        case ptr<ssa.Go> instr:
            (fn, args) = prepareCall(_addr_fr, _addr_instr.Call);
            atomic.AddInt32(_addr_fr.i.goroutines, 1);
            go_(() => () => {
                call(_addr_fr.i, _addr_null, instr.Pos(), fn, args);
                atomic.AddInt32(_addr_fr.i.goroutines, -1);
            }());
            break;
        case ptr<ssa.MakeChan> instr:
            fr.env[instr] = make_channel<value>(asInt(fr.get(instr.Size)));
            break;
        case ptr<ssa.Alloc> instr:
            ptr<value> addr;
            if (instr.Heap) { 
                // new
                addr = @new<value>();
                fr.env[instr] = addr;

            }
            else
 { 
                // local
                addr = fr.env[instr]._<ptr<value>>();

            }

            addr.val = zero(deref(instr.Type()));
            break;
        case ptr<ssa.MakeSlice> instr:
            var slice = make_slice<value>(asInt(fr.get(instr.Cap)));
            ptr<types.Slice> tElt = instr.Type().Underlying()._<ptr<types.Slice>>().Elem();
            {
                var i__prev1 = i;

                foreach (var (__i) in slice) {
                    i = __i;
                    slice[i] = zero(tElt);
                }

                i = i__prev1;
            }

            fr.env[instr] = slice[..(int)asInt(fr.get(instr.Len))];
            break;
        case ptr<ssa.MakeMap> instr:
            nint reserve = 0;
            if (instr.Reserve != null) {
                reserve = asInt(fr.get(instr.Reserve));
            }
            fr.env[instr] = makeMap(instr.Type().Underlying()._<ptr<types.Map>>().Key(), reserve);
            break;
        case ptr<ssa.Range> instr:
            fr.env[instr] = rangeIter(fr.get(instr.X), instr.X.Type());
            break;
        case ptr<ssa.Next> instr:
            fr.env[instr] = fr.get(instr.Iter)._<iter>().next();
            break;
        case ptr<ssa.FieldAddr> instr:
            fr.env[instr] = _addr_(new ptr<ptr<fr.get>>(instr.X)._<ptr<value>>())._<structure>()[instr.Field];
            break;
        case ptr<ssa.Field> instr:
            fr.env[instr] = fr.get(instr.X)._<structure>()[instr.Field];
            break;
        case ptr<ssa.IndexAddr> instr:
            var x = fr.get(instr.X);
            var idx = fr.get(instr.Index);
            switch (x.type()) {
                case slice<value> x:
                    fr.env[instr] = _addr_x[asInt(idx)];
                    break;
                case ptr<value> x:
                    fr.env[instr] = _addr_(x.val)._<array>()[asInt(idx)];
                    break;
                default:
                {
                    var x = x.type();
                    panic(fmt.Sprintf("unexpected x type in IndexAddr: %T", x));
                    break;
                }

            }
            break;
        case ptr<ssa.Index> instr:
            fr.env[instr] = fr.get(instr.X)._<array>()[asInt(fr.get(instr.Index))];
            break;
        case ptr<ssa.Lookup> instr:
            fr.env[instr] = lookup(instr, fr.get(instr.X), fr.get(instr.Index));
            break;
        case ptr<ssa.MapUpdate> instr:
            var m = fr.get(instr.Map);
            var key = fr.get(instr.Key);
            var v = fr.get(instr.Value);
            switch (m.type()) {
                case map<value, value> m:
                    m[key] = v;
                    break;
                case ptr<hashmap> m:
                    m.insert(key._<hashable>(), v);
                    break;
                default:
                {
                    var m = m.type();
                    panic(fmt.Sprintf("illegal map type: %T", m));
                    break;
                }

            }
            break;
        case ptr<ssa.TypeAssert> instr:
            fr.env[instr] = typeAssert(fr.i, instr, fr.get(instr.X)._<iface>());
            break;
        case ptr<ssa.MakeClosure> instr:
            slice<value> bindings = default;
            foreach (var (_, binding) in instr.Bindings) {
                bindings = append(bindings, fr.get(binding));
            }
            fr.env[instr] = addr(new closure(instr.Fn.(*ssa.Function),bindings));
            break;
        case ptr<ssa.Phi> instr:
            {
                var i__prev1 = i;

                foreach (var (__i, __pred) in instr.Block().Preds) {
                    i = __i;
                    pred = __pred;
                    if (fr.prevBlock == pred) {
                        fr.env[instr] = fr.get(instr.Edges[i]);
                        break;
                    }
                }

                i = i__prev1;
            }
            break;
        case ptr<ssa.Select> instr:
            slice<reflect.SelectCase> cases = default;
            if (!instr.Blocking) {
                cases = append(cases, new reflect.SelectCase(Dir:reflect.SelectDefault,));
            }
            foreach (var (_, state) in instr.States) {
                reflect.SelectDir dir = default;
                if (state.Dir == types.RecvOnly) {
                    dir = reflect.SelectRecv;
                }
                else
 {
                    dir = reflect.SelectSend;
                }

                reflect.Value send = default;
                if (state.Send != null) {
                    send = reflect.ValueOf(fr.get(state.Send));
                }

                cases = append(cases, new reflect.SelectCase(Dir:dir,Chan:reflect.ValueOf(fr.get(state.Chan)),Send:send,));

            }
            var (chosen, recv, recvOk) = reflect.Select(cases);
            if (!instr.Blocking) {
                chosen--; // default case should have index -1.
            }

            tuple r = new tuple(chosen,recvOk);
            {
                var i__prev1 = i;

                foreach (var (__i, __st) in instr.States) {
                    i = __i;
                    st = __st;
                    if (st.Dir == types.RecvOnly) {
                        v = default;
                        if (i == chosen && recvOk) { 
                            // No need to copy since send makes an unaliased copy.
                            v = recv.Interface()._<value>();

                        }
                        else
 {
                            v = zero(st.Chan.Type().Underlying()._<ptr<types.Chan>>().Elem());
                        }

                        r = append(r, v);

                    }

                }

                i = i__prev1;
            }

            fr.env[instr] = r;
            break;
        default:
        {
            var instr = instr.type();
            panic(fmt.Sprintf("unexpected instruction: %T", instr));
            break;
        } 

        // if val, ok := instr.(ssa.Value); ok {
        //     fmt.Println(toString(fr.env[val])) // debugging
        // }

    } 

    // if val, ok := instr.(ssa.Value); ok {
    //     fmt.Println(toString(fr.env[val])) // debugging
    // }

    return kNext;

});

// prepareCall determines the function value and argument values for a
// function call in a Call, Go or Defer instruction, performing
// interface method lookup if needed.
//
private static (value, slice<value>) prepareCall(ptr<frame> _addr_fr, ptr<ssa.CallCommon> _addr_call) => func((_, panic, _) => {
    value fn = default;
    slice<value> args = default;
    ref frame fr = ref _addr_fr.val;
    ref ssa.CallCommon call = ref _addr_call.val;

    var v = fr.get(call.Value);
    if (call.Method == null) { 
        // Function call.
        fn = v;

    }
    else
 { 
        // Interface method invocation.
        iface recv = v._<iface>();
        if (recv.t == null) {
            panic("method invoked on nil interface");
        }
        {
            var f = lookupMethod(_addr_fr.i, recv.t, _addr_call.Method);

            if (f == null) { 
                // Unreachable in well-typed programs.
                panic(fmt.Sprintf("method set for dynamic type %v does not contain %s", recv.t, call.Method));

            }
            else
 {
                fn = f;
            }

        }

        args = append(args, recv.v);

    }
    foreach (var (_, arg) in call.Args) {
        args = append(args, fr.get(arg));
    }    return ;

});

// call interprets a call to a function (function, builtin or closure)
// fn with arguments args, returning its result.
// callpos is the position of the callsite.
//
private static value call(ptr<interpreter> _addr_i, ptr<frame> _addr_caller, token.Pos callpos, value fn, slice<value> args) => func((_, panic, _) => {
    ref interpreter i = ref _addr_i.val;
    ref frame caller = ref _addr_caller.val;

    switch (fn.type()) {
        case ptr<ssa.Function> fn:
            if (fn == null) {
                panic("call of nil function"); // nil of func type
            }

            return callSSA(_addr_i, _addr_caller, callpos, _addr_fn, args, null);
            break;
        case ptr<closure> fn:
            return callSSA(_addr_i, _addr_caller, callpos, _addr_fn.Fn, args, fn.Env);
            break;
        case ptr<ssa.Builtin> fn:
            return callBuiltin(caller, callpos, fn, args);
            break;
    }
    panic(fmt.Sprintf("cannot call %T", fn));

});

private static @string loc(ptr<token.FileSet> _addr_fset, token.Pos pos) {
    ref token.FileSet fset = ref _addr_fset.val;

    if (pos == token.NoPos) {
        return "";
    }
    return " at " + fset.Position(pos).String();

}

// callSSA interprets a call to function fn with arguments args,
// and lexical environment env, returning its result.
// callpos is the position of the callsite.
//
private static value callSSA(ptr<interpreter> _addr_i, ptr<frame> _addr_caller, token.Pos callpos, ptr<ssa.Function> _addr_fn, slice<value> args, slice<value> env) => func((defer, panic, _) => {
    ref interpreter i = ref _addr_i.val;
    ref frame caller = ref _addr_caller.val;
    ref ssa.Function fn = ref _addr_fn.val;

    if (i.mode & EnableTracing != 0) {
        var fset = fn.Prog.Fset; 
        // TODO(adonovan): fix: loc() lies for external functions.
        fmt.Fprintf(os.Stderr, "Entering %s%s.\n", fn, loc(_addr_fset, fn.Pos()));
        @string suffix = "";
        if (caller != null) {
            suffix = ", resuming " + caller.fn.String() + loc(_addr_fset, callpos);
        }
        defer(fmt.Fprintf(os.Stderr, "Leaving %s%s.\n", fn, suffix));

    }
    ptr<frame> fr = addr(new frame(i:i,caller:caller,fn:fn,));
    if (fn.Parent() == null) {
        var name = fn.String();
        {
            var ext = externals[name];

            if (ext != null) {
                if (i.mode & EnableTracing != 0) {
                    fmt.Fprintln(os.Stderr, "\t(external)");
                }
                return ext(fr, args);
            }

        }

        if (fn.Blocks == null) {
            panic("no code for function: " + name);
        }
    }
    fr.env = make_map<ssa.Value, value>();
    fr.block = fn.Blocks[0];
    fr.locals = make_slice<value>(len(fn.Locals));
    {
        var i__prev1 = i;

        foreach (var (__i, __l) in fn.Locals) {
            i = __i;
            l = __l;
            fr.locals[i] = zero(deref(l.Type()));
            fr.env[l] = _addr_fr.locals[i];
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __p) in fn.Params) {
            i = __i;
            p = __p;
            fr.env[p] = args[i];
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i, __fv) in fn.FreeVars) {
            i = __i;
            fv = __fv;
            fr.env[fv] = env[i];
        }
        i = i__prev1;
    }

    while (fr.block != null) {
        runFrame(fr);
    } 
    // Destroy the locals to avoid accidental use after return.
    {
        var i__prev1 = i;

        foreach (var (__i) in fn.Locals) {
            i = __i;
            fr.locals[i] = new bad();
        }
        i = i__prev1;
    }

    return fr.result;

});

// runFrame executes SSA instructions starting at fr.block and
// continuing until a return, a panic, or a recovered panic.
//
// After a panic, runFrame panics.
//
// After a normal return, fr.result contains the result of the call
// and fr.block is nil.
//
// A recovered panic in a function without named return parameters
// (NRPs) becomes a normal return of the zero value of the function's
// result type.
//
// After a recovered panic in a function with NRPs, fr.result is
// undefined and fr.block contains the block at which to resume
// control.
//
private static void runFrame(ptr<frame> _addr_fr) => func((defer, _, _) => {
    ref frame fr = ref _addr_fr.val;

    defer(() => {
        if (fr.block == null) {
            return ; // normal return
        }
        if (fr.i.mode & DisableRecover != 0) {
            return ; // let interpreter crash
        }
        fr.panicking = true;
        fr.panic = recover();
        if (fr.i.mode & EnableTracing != 0) {
            fmt.Fprintf(os.Stderr, "Panicking: %T %v.\n", fr.panic, fr.panic);
        }
        fr.runDefers();
        fr.block = fr.fn.Recover;

    }());

    while (true) {
        if (fr.i.mode & EnableTracing != 0) {
            fmt.Fprintf(os.Stderr, ".%s:\n", fr.block);
        }
block:
        foreach (var (_, instr) in fr.block.Instrs) {
            if (fr.i.mode & EnableTracing != 0) {
                {
                    ssa.Value (v, ok) = instr._<ssa.Value>();

                    if (ok) {
                        fmt.Fprintln(os.Stderr, "\t", v.Name(), "=", instr);
                    }
                    else
 {
                        fmt.Fprintln(os.Stderr, "\t", instr);
                    }

                }

            }


            if (visitInstr(_addr_fr, instr) == kReturn) 
                return ;
            else if (visitInstr(_addr_fr, instr) == kNext)             else if (visitInstr(_addr_fr, instr) == kJump) 
                _breakblock = true;
                break;
                    }
    }

});

// doRecover implements the recover() built-in.
private static value doRecover(ptr<frame> _addr_caller) => func((_, panic, _) => {
    ref frame caller = ref _addr_caller.val;
 
    // recover() must be exactly one level beneath the deferred
    // function (two levels beneath the panicking function) to
    // have any effect.  Thus we ignore both "defer recover()" and
    // "defer f() -> g() -> recover()".
    if (caller.i.mode & DisableRecover == 0 && caller != null && !caller.panicking && caller.caller != null && caller.caller.panicking) {
        caller.caller.panicking = false;
        var p = caller.caller.panic;
        caller.caller.panic = null; 

        // TODO(adonovan): support runtime.Goexit.
        switch (p.type()) {
            case targetPanic p:
                return p.v;
                break;
            case runtime.Error p:
                return new iface(caller.i.runtimeErrorString,p.Error());
                break;
            case @string p:
                return new iface(caller.i.runtimeErrorString,p);
                break;
            default:
            {
                var p = p.type();
                panic(fmt.Sprintf("unexpected panic type %T in target call to recover()", p));
                break;
            }
        }

    }
    return new iface();

});

// setGlobal sets the value of a system-initialized global variable.
private static void setGlobal(ptr<interpreter> _addr_i, ptr<ssa.Package> _addr_pkg, @string name, value v) => func((_, panic, _) => {
    ref interpreter i = ref _addr_i.val;
    ref ssa.Package pkg = ref _addr_pkg.val;

    {
        var (g, ok) = i.globals[pkg.Var(name)];

        if (ok) {
            g.val = v;
            return ;
        }
    }

    panic("no global variable: " + pkg.Pkg.Path() + "." + name);

});

// Interpret interprets the Go program whose main package is mainpkg.
// mode specifies various interpreter options.  filename and args are
// the initial values of os.Args for the target program.  sizes is the
// effective type-sizing function for this program.
//
// Interpret returns the exit code of the program: 2 for panic (like
// gc does), or the argument to os.Exit for normal termination.
//
// The SSA program must include the "runtime" package.
//
public static nint Interpret(ptr<ssa.Package> _addr_mainpkg, Mode mode, types.Sizes sizes, @string filename, slice<@string> args) => func((defer, panic, _) => {
    nint exitCode = default;
    ref ssa.Package mainpkg = ref _addr_mainpkg.val;

    ptr<interpreter> i = addr(new interpreter(prog:mainpkg.Prog,globals:make(map[ssa.Value]*value),mode:mode,sizes:sizes,goroutines:1,));
    var runtimePkg = i.prog.ImportedPackage("runtime");
    if (runtimePkg == null) {
        panic("ssa.Program doesn't include runtime package");
    }
    i.runtimeErrorString = runtimePkg.Type("errorString").Object().Type();

    initReflect(i);

    i.osArgs = append(i.osArgs, filename);
    foreach (var (_, arg) in args) {
        i.osArgs = append(i.osArgs, arg);
    }    foreach (var (_, pkg) in i.prog.AllPackages()) { 
        // Initialize global storage.
        foreach (var (_, m) in pkg.Members) {
            switch (m.type()) {
                case ptr<ssa.Global> v:
                    ref var cell = ref heap(zero(deref(v.Type())), out ptr<var> _addr_cell);
                    _addr_i.globals[v] = _addr_cell;
                    i.globals[v] = ref _addr_i.globals[v].val;
                    break;
            }

        }
    }    exitCode = 2;
    defer(() => {
        if (exitCode != 2 || i.mode & DisableRecover != 0) {
            return ;
        }
        switch (recover().type()) {
            case exitPanic p:
                exitCode = int(p);
                return ;
                break;
            case targetPanic p:
                fmt.Fprintln(os.Stderr, "panic:", toString(p.v));
                break;
            case runtime.Error p:
                fmt.Fprintln(os.Stderr, "panic:", p.Error());
                break;
            case @string p:
                fmt.Fprintln(os.Stderr, "panic:", p);
                break;
            default:
            {
                var p = recover().type();
                fmt.Fprintf(os.Stderr, "panic: unexpected type: %T: %v\n", p, p);
                break;
            } 

            // TODO(adonovan): dump panicking interpreter goroutine?
            // buf := make([]byte, 0x10000)
            // runtime.Stack(buf, false)
            // fmt.Fprintln(os.Stderr, string(buf))
            // (Or dump panicking target goroutine?)
        } 

        // TODO(adonovan): dump panicking interpreter goroutine?
        // buf := make([]byte, 0x10000)
        // runtime.Stack(buf, false)
        // fmt.Fprintln(os.Stderr, string(buf))
        // (Or dump panicking target goroutine?)
    }()); 

    // Run!
    call(i, _addr_null, token.NoPos, mainpkg.Func("init"), null);
    {
        var mainFn = mainpkg.Func("main");

        if (mainFn != null) {
            call(i, _addr_null, token.NoPos, mainFn, null);
            exitCode = 0;
        }
        else
 {
            fmt.Fprintln(os.Stderr, "No main function.");
            exitCode = 1;
        }
    }

    return ;

});

// deref returns a pointer's element type; otherwise it returns typ.
// TODO(adonovan): Import from ssa?
private static types.Type deref(types.Type typ) {
    {
        ptr<types.Pointer> (p, ok) = typ.Underlying()._<ptr<types.Pointer>>();

        if (ok) {
            return p.Elem();
        }
    }

    return typ;

}

} // end interp_package

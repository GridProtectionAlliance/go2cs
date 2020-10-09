// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package js -- go2cs converted at 2020 October 09 04:53:03 UTC
// import "syscall/js" ==> using js = go.syscall.js_package
// Original source: C:\Go\src\syscall\js\func.go
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace syscall
{
    public static partial class js_package
    {
        private static sync.Mutex funcsMu = default;        private static var funcs = make_map<uint, Action<Value, slice<Value>>>();        private static uint nextFuncID = 1L;

        private static Wrapper _ = new Func(); // Func must implement Wrapper

        // Func is a wrapped Go function to be called by JavaScript.
        public partial struct Func
        {
            public ref Value Value => ref Value_val; // the JavaScript function that invokes the Go function
            public uint id;
        }

        // FuncOf returns a function to be used by JavaScript.
        //
        // The Go function fn is called with the value of JavaScript's "this" keyword and the
        // arguments of the invocation. The return value of the invocation is
        // the result of the Go function mapped back to JavaScript according to ValueOf.
        //
        // Invoking the wrapped Go function from JavaScript will
        // pause the event loop and spawn a new goroutine.
        // Other wrapped functions which are triggered during a call from Go to JavaScript
        // get executed on the same goroutine.
        //
        // As a consequence, if one wrapped function blocks, JavaScript's event loop
        // is blocked until that function returns. Hence, calling any async JavaScript
        // API, which requires the event loop, like fetch (http.Client), will cause an
        // immediate deadlock. Therefore a blocking function should explicitly start a
        // new goroutine.
        //
        // Func.Release must be called to free up resources when the function will not be invoked any more.
        public static Func FuncOf(Action<Value, slice<Value>> fn)
        {
            funcsMu.Lock();
            var id = nextFuncID;
            nextFuncID++;
            funcs[id] = fn;
            funcsMu.Unlock();
            return new Func(id:id,Value:jsGo.Call("_makeFuncWrapper",id),);
        }

        // Release frees up resources allocated for the function.
        // The function must not be invoked after calling Release.
        // It is allowed to call Release while the function is still running.
        public static void Release(this Func c)
        {
            funcsMu.Lock();
            delete(funcs, c.id);
            funcsMu.Unlock();
        }

        // setEventHandler is defined in the runtime package.
        private static void setEventHandler(Action fn)
;

        private static void init()
        {
            setEventHandler(handleEvent);
        }

        private static void handleEvent()
        {
            var cb = jsGo.Get("_pendingEvent");
            if (cb.IsNull())
            {>>MARKER:FUNCTION_setEventHandler_BLOCK_PREFIX<<
                return ;
            }

            jsGo.Set("_pendingEvent", Null());

            var id = uint32(cb.Get("id").Int());
            if (id == 0L)
            { // zero indicates deadlock
            }

            funcsMu.Lock();
            var (f, ok) = funcs[id];
            funcsMu.Unlock();
            if (!ok)
            {
                Global().Get("console").Call("error", "call to released function");
                return ;
            }

            var @this = cb.Get("this");
            var argsObj = cb.Get("args");
            var args = make_slice<Value>(argsObj.Length());
            foreach (var (i) in args)
            {
                args[i] = argsObj.Index(i);
            }
            var result = f(this, args);
            cb.Set("result", result);

        }
    }
}}

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\syscall_windows.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct callbacks
        {
            public mutex @lock;
            public array<ref wincallbackcontext> ctxt;
            public long n;
        }

        private static bool isCleanstack(this ref wincallbackcontext c)
        {
            return c.cleanstack;
        }

        private static void setCleanstack(this ref wincallbackcontext c, bool cleanstack)
        {
            c.cleanstack = cleanstack;
        }

        private static callbacks cbs = default;        private static ptr<ptr<wincallbackcontext>> cbctxts = ref cbs.ctxt[0L];        private static byte callbackasm = default;

        // callbackasmAddr returns address of runtime.callbackasm
        // function adjusted by i.
        // runtime.callbackasm is just a series of CALL instructions
        // (each is 5 bytes long), and we want callback to arrive at
        // correspondent call instruction instead of start of
        // runtime.callbackasm.
        private static System.UIntPtr callbackasmAddr(long i)
        {
            return uintptr(add(@unsafe.Pointer(ref callbackasm), uintptr(i * 5L)));
        }

        //go:linkname compileCallback syscall.compileCallback
        private static System.UIntPtr compileCallback(eface fn, bool cleanstack) => func((defer, panic, _) =>
        {
            if (fn._type == null || (fn._type.kind & kindMask) != kindFunc)
            {
                panic("compileCallback: not a function");
            }
            var ft = (functype.Value)(@unsafe.Pointer(fn._type));
            if (len(ft.@out()) != 1L)
            {
                panic("compileCallback: function must have one output parameter");
            }
            var uintptrSize = @unsafe.Sizeof(uintptr(0L));
            if (ft.@out()[0L].size != uintptrSize)
            {
                panic("compileCallback: output parameter size is wrong");
            }
            var argsize = uintptr(0L);
            foreach (var (_, t) in ft.@in())
            {
                if (t.size > uintptrSize)
                {
                    panic("compileCallback: input parameter size is wrong");
                }
                argsize += uintptrSize;
            }
            lock(ref cbs.@lock);
            defer(unlock(ref cbs.@lock));

            var n = cbs.n;
            for (long i = 0L; i < n; i++)
            {
                if (cbs.ctxt[i].gobody == fn.data && cbs.ctxt[i].isCleanstack() == cleanstack)
                {
                    return callbackasmAddr(i);
                }
            }

            if (n >= cb_max)
            {
                throw("too many callback functions");
            }
            ptr<wincallbackcontext> c = @new<wincallbackcontext>();
            c.gobody = fn.data;
            c.argsize = argsize;
            c.setCleanstack(cleanstack);
            if (cleanstack && argsize != 0L)
            {
                c.restorestack = argsize;
            }
            else
            {
                c.restorestack = 0L;
            }
            cbs.ctxt[n] = c;
            cbs.n++;

            return callbackasmAddr(n);
        });

        private static readonly ulong _LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800UL;

        //go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
        //go:nosplit


        //go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_loadsystemlibrary(ref ushort _filename) => func(_filename, (ref ushort filename, Defer defer, Panic _, Recover __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;

            if (useLoadLibraryEx)
            {
                c.fn = getLoadLibraryEx();
                c.n = 3L;
                struct{lpFileName*uint16hFileuintptrflagsuint32} args = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{lpFileName*uint16hFileuintptrflagsuint32}{filename,0,_LOAD_LIBRARY_SEARCH_SYSTEM32};
                c.args = uintptr(noescape(@unsafe.Pointer(ref args)));
            }
            else
            { 
                // User is on Windows XP or something ancient.
                // The caller wanted to only load the filename DLL
                // from the System32 directory but that facility
                // doesn't exist, so just load it the normal way. This
                // is a potential security risk, but so is Windows XP.
                c.fn = getLoadLibrary();
                c.n = 1L;
                c.args = uintptr(noescape(@unsafe.Pointer(ref filename)));
            }
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            handle = c.r1;
            if (handle == 0L)
            {
                err = c.err;
            }
            return;
        });

        //go:linkname syscall_loadlibrary syscall.loadlibrary
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_loadlibrary(ref ushort _filename) => func(_filename, (ref ushort filename, Defer defer, Panic _, Recover __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = getLoadLibrary();
            c.n = 1L;
            c.args = uintptr(noescape(@unsafe.Pointer(ref filename)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            handle = c.r1;
            if (handle == 0L)
            {
                err = c.err;
            }
            return;
        });

        //go:linkname syscall_getprocaddress syscall.getprocaddress
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_getprocaddress(System.UIntPtr handle, ref byte _procname) => func(_procname, (ref byte procname, Defer defer, Panic _, Recover __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = getGetProcAddress();
            c.n = 2L;
            c.args = uintptr(noescape(@unsafe.Pointer(ref handle)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            outhandle = c.r1;
            if (outhandle == 0L)
            {
                err = c.err;
            }
            return;
        });

        //go:linkname syscall_Syscall syscall.Syscall
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((defer, _, __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall6 syscall.Syscall6
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) => func((defer, _, __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall9 syscall.Syscall9
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall9(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) => func((defer, _, __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall12 syscall.Syscall12
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall12(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12) => func((defer, _, __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall15 syscall.Syscall15
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall15(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15) => func((defer, _, __) =>
        {
            lockOSThread();
            defer(unlockOSThread());
            var c = ref getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });
    }
}

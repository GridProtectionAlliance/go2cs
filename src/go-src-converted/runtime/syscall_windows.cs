// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:52 UTC
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
            public array<ptr<wincallbackcontext>> ctxt;
            public long n;
        }

        private static bool isCleanstack(this ptr<wincallbackcontext> _addr_c)
        {
            ref wincallbackcontext c = ref _addr_c.val;

            return c.cleanstack;
        }

        private static void setCleanstack(this ptr<wincallbackcontext> _addr_c, bool cleanstack)
        {
            ref wincallbackcontext c = ref _addr_c.val;

            c.cleanstack = cleanstack;
        }

        private static callbacks cbs = default;        private static ptr<ptr<wincallbackcontext>> cbctxts_addr_cbs.ctxt[0L];

        private static void callbackasm()
;

        // callbackasmAddr returns address of runtime.callbackasm
        // function adjusted by i.
        // On x86 and amd64, runtime.callbackasm is a series of CALL instructions,
        // and we want callback to arrive at
        // correspondent call instruction instead of start of
        // runtime.callbackasm.
        // On ARM, runtime.callbackasm is a series of mov and branch instructions.
        // R12 is loaded with the callback index. Each entry is two instructions,
        // hence 8 bytes.
        private static System.UIntPtr callbackasmAddr(long i) => func((_, panic, __) =>
        {
            long entrySize = default;
            switch (GOARCH)
            {
                case "386": 

                case "amd64": 
                    entrySize = 5L;
                    break;
                case "arm": 
                    // On ARM, each entry is a MOV instruction
                    // followed by a branch instruction
                    entrySize = 8L;
                    break;
                default: 
                    panic("unsupported architecture");
                    break;
            }
            return funcPC(callbackasm) + uintptr(i * entrySize);

        });

        //go:linkname compileCallback syscall.compileCallback
        private static System.UIntPtr compileCallback(eface fn, bool cleanstack) => func((_, panic, __) =>
        {
            System.UIntPtr code = default;

            if (fn._type == null || (fn._type.kind & kindMask) != kindFunc)
            {>>MARKER:FUNCTION_callbackasm_BLOCK_PREFIX<<
                panic("compileCallback: expected function with one uintptr-sized result");
            }

            var ft = (functype.val)(@unsafe.Pointer(fn._type));
            if (len(ft.@out()) != 1L)
            {
                panic("compileCallback: expected function with one uintptr-sized result");
            }

            var uintptrSize = @unsafe.Sizeof(uintptr(0L));
            if (ft.@out()[0L].size != uintptrSize)
            {
                panic("compileCallback: expected function with one uintptr-sized result");
            }

            var argsize = uintptr(0L);
            foreach (var (_, t) in ft.@in())
            {
                if (t.size > uintptrSize)
                {
                    panic("compileCallback: argument size is larger than uintptr");
                }

                argsize += uintptrSize;

            }
            lock(_addr_cbs.@lock); // We don't unlock this in a defer because this is used from the system stack.

            var n = cbs.n;
            for (long i = 0L; i < n; i++)
            {
                if (cbs.ctxt[i].gobody == fn.data && cbs.ctxt[i].isCleanstack() == cleanstack)
                {
                    var r = callbackasmAddr(i);
                    unlock(_addr_cbs.@lock);
                    return r;
                }

            }

            if (n >= cb_max)
            {
                unlock(_addr_cbs.@lock);
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

            r = callbackasmAddr(n);
            unlock(_addr_cbs.@lock);
            return r;

        });

        private static readonly ulong _LOAD_LIBRARY_SEARCH_SYSTEM32 = (ulong)0x00000800UL;

        // When available, this function will use LoadLibraryEx with the filename
        // parameter and the important SEARCH_SYSTEM32 argument. But on systems that
        // do not have that option, absoluteFilepath should contain a fallback
        // to the full path inside of system32 for use with vanilla LoadLibrary.
        //go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
        //go:nosplit


        // When available, this function will use LoadLibraryEx with the filename
        // parameter and the important SEARCH_SYSTEM32 argument. But on systems that
        // do not have that option, absoluteFilepath should contain a fallback
        // to the full path inside of system32 for use with vanilla LoadLibrary.
        //go:linkname syscall_loadsystemlibrary syscall.loadsystemlibrary
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_loadsystemlibrary(ptr<ushort> _addr_filename, ptr<ushort> _addr_absoluteFilepath)
        {
            System.UIntPtr handle = default;
            System.UIntPtr err = default;
            ref ushort filename = ref _addr_filename.val;
            ref ushort absoluteFilepath = ref _addr_absoluteFilepath.val;

            lockOSThread();
            var c = _addr_getg().m.syscall;

            if (useLoadLibraryEx)
            {
                c.fn = getLoadLibraryEx();
                c.n = 3L;
                ref struct{lpFileName*uint16hFileuintptrflagsuint32} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{lpFileName*uint16hFileuintptrflagsuint32}{filename,0,_LOAD_LIBRARY_SEARCH_SYSTEM32}, out ptr<struct{lpFileName*uint16hFileuintptrflagsuint32}> _addr_args);
                c.args = uintptr(noescape(@unsafe.Pointer(_addr_args)));
            }
            else
            {
                c.fn = getLoadLibrary();
                c.n = 1L;
                c.args = uintptr(noescape(@unsafe.Pointer(_addr_absoluteFilepath)));
            }

            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            handle = c.r1;
            if (handle == 0L)
            {
                err = c.err;
            }

            unlockOSThread(); // not defer'd after the lockOSThread above to save stack frame size.
            return ;

        }

        //go:linkname syscall_loadlibrary syscall.loadlibrary
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_loadlibrary(ptr<ushort> _addr_filename) => func((defer, _, __) =>
        {
            System.UIntPtr handle = default;
            System.UIntPtr err = default;
            ref ushort filename = ref _addr_filename.val;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = getLoadLibrary();
            c.n = 1L;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_filename)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            handle = c.r1;
            if (handle == 0L)
            {
                err = c.err;
            }

            return ;

        });

        //go:linkname syscall_getprocaddress syscall.getprocaddress
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) syscall_getprocaddress(System.UIntPtr handle, ptr<byte> _addr_procname) => func((defer, _, __) =>
        {
            System.UIntPtr outhandle = default;
            System.UIntPtr err = default;
            ref byte procname = ref _addr_procname.val;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = getGetProcAddress();
            c.n = 2L;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_handle)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            outhandle = c.r1;
            if (outhandle == 0L)
            {
                err = c.err;
            }

            return ;

        });

        //go:linkname syscall_Syscall syscall.Syscall
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall6 syscall.Syscall6
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall6(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall9 syscall.Syscall9
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall9(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall12 syscall.Syscall12
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall12(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall15 syscall.Syscall15
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall15(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });

        //go:linkname syscall_Syscall18 syscall.Syscall18
        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_Syscall18(System.UIntPtr fn, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15, System.UIntPtr a16, System.UIntPtr a17, System.UIntPtr a18) => func((defer, _, __) =>
        {
            System.UIntPtr r1 = default;
            System.UIntPtr r2 = default;
            System.UIntPtr err = default;

            lockOSThread();
            defer(unlockOSThread());
            var c = _addr_getg().m.syscall;
            c.fn = fn;
            c.n = nargs;
            c.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
            cgocall(asmstdcallAddr, @unsafe.Pointer(c));
            return (c.r1, c.r2, c.err);
        });
    }
}

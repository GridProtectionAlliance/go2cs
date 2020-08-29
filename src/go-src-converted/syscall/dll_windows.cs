// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:16:21 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\dll_windows.go
using sysdll = go.@internal.syscall.windows.sysdll_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // DLLError describes reasons for DLL load failures.
        public partial struct DLLError
        {
            public error Err;
            public @string ObjName;
            public @string Msg;
        }

        private static @string Error(this ref DLLError e)
        {
            return e.Msg;
        }

        // Implemented in ../runtime/syscall_windows.go.
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall12(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12)
;
        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall15(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15)
;
        private static (System.UIntPtr, Errno) loadlibrary(ref ushort filename)
;
        private static (System.UIntPtr, Errno) loadsystemlibrary(ref ushort filename)
;
        private static (System.UIntPtr, Errno) getprocaddress(System.UIntPtr handle, ref byte procname)
;

        // A DLL implements access to a single DLL.
        public partial struct DLL
        {
            public @string Name;
            public Handle Handle;
        }

        // LoadDLL loads the named DLL file into memory.
        //
        // If name is not an absolute path and is not a known system DLL used by
        // Go, Windows will search for the named DLL in many locations, causing
        // potential DLL preloading attacks.
        //
        // Use LazyDLL in golang.org/x/sys/windows for a secure way to
        // load system DLLs.
        public static (ref DLL, error) LoadDLL(@string name)
        {
            var (namep, err) = UTF16PtrFromString(name);
            if (err != null)
            {>>MARKER:FUNCTION_getprocaddress_BLOCK_PREFIX<<
                return (null, err);
            }
            System.UIntPtr h = default;
            Errno e = default;
            if (sysdll.IsSystemDLL[name])
            {>>MARKER:FUNCTION_loadsystemlibrary_BLOCK_PREFIX<<
                h, e = loadsystemlibrary(namep);
            }
            else
            {>>MARKER:FUNCTION_loadlibrary_BLOCK_PREFIX<<
                h, e = loadlibrary(namep);
            }
            if (e != 0L)
            {>>MARKER:FUNCTION_Syscall15_BLOCK_PREFIX<<
                return (null, ref new DLLError(Err:e,ObjName:name,Msg:"Failed to load "+name+": "+e.Error(),));
            }
            DLL d = ref new DLL(Name:name,Handle:Handle(h),);
            return (d, null);
        }

        // MustLoadDLL is like LoadDLL but panics if load operation fails.
        public static ref DLL MustLoadDLL(@string name) => func((_, panic, __) =>
        {
            var (d, e) = LoadDLL(name);
            if (e != null)
            {>>MARKER:FUNCTION_Syscall12_BLOCK_PREFIX<<
                panic(e);
            }
            return d;
        });

        // FindProc searches DLL d for procedure named name and returns *Proc
        // if found. It returns an error if search fails.
        private static (ref Proc, error) FindProc(this ref DLL d, @string name)
        {>>MARKER:FUNCTION_Syscall9_BLOCK_PREFIX<<
            var (namep, err) = BytePtrFromString(name);
            if (err != null)
            {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
                return (null, err);
            }
            var (a, e) = getprocaddress(uintptr(d.Handle), namep);
            if (e != 0L)
            {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
                return (null, ref new DLLError(Err:e,ObjName:name,Msg:"Failed to find "+name+" procedure in "+d.Name+": "+e.Error(),));
            }
            Proc p = ref new Proc(Dll:d,Name:name,addr:a,);
            return (p, null);
        }

        // MustFindProc is like FindProc but panics if search fails.
        private static ref Proc MustFindProc(this ref DLL _d, @string name) => func(_d, (ref DLL d, Defer _, Panic panic, Recover __) =>
        {
            var (p, e) = d.FindProc(name);
            if (e != null)
            {
                panic(e);
            }
            return p;
        });

        // Release unloads DLL d from memory.
        private static error Release(this ref DLL d)
        {
            return error.As(FreeLibrary(d.Handle));
        }

        // A Proc implements access to a procedure inside a DLL.
        public partial struct Proc
        {
            public ptr<DLL> Dll;
            public @string Name;
            public System.UIntPtr addr;
        }

        // Addr returns the address of the procedure represented by p.
        // The return value can be passed to Syscall to run the procedure.
        private static System.UIntPtr Addr(this ref Proc p)
        {
            return p.addr;
        }

        //go:uintptrescapes

        // Call executes procedure p with arguments a. It will panic, if more than 15 arguments
        // are supplied.
        //
        // The returned error is always non-nil, constructed from the result of GetLastError.
        // Callers must inspect the primary return value to decide whether an error occurred
        // (according to the semantics of the specific function being called) before consulting
        // the error. The error will be guaranteed to contain syscall.Errno.
        private static (System.UIntPtr, System.UIntPtr, error) Call(this ref Proc _p, params System.UIntPtr[] a) => func(_p, (ref Proc p, Defer _, Panic panic, Recover __) =>
        {
            switch (len(a))
            {
                case 0L: 
                    return Syscall(p.Addr(), uintptr(len(a)), 0L, 0L, 0L);
                    break;
                case 1L: 
                    return Syscall(p.Addr(), uintptr(len(a)), a[0L], 0L, 0L);
                    break;
                case 2L: 
                    return Syscall(p.Addr(), uintptr(len(a)), a[0L], a[1L], 0L);
                    break;
                case 3L: 
                    return Syscall(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L]);
                    break;
                case 4L: 
                    return Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], 0L, 0L);
                    break;
                case 5L: 
                    return Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], 0L);
                    break;
                case 6L: 
                    return Syscall6(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L]);
                    break;
                case 7L: 
                    return Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], 0L, 0L);
                    break;
                case 8L: 
                    return Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], 0L);
                    break;
                case 9L: 
                    return Syscall9(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L]);
                    break;
                case 10L: 
                    return Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], 0L, 0L);
                    break;
                case 11L: 
                    return Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], 0L);
                    break;
                case 12L: 
                    return Syscall12(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L]);
                    break;
                case 13L: 
                    return Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], 0L, 0L);
                    break;
                case 14L: 
                    return Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], a[13L], 0L);
                    break;
                case 15L: 
                    return Syscall15(p.Addr(), uintptr(len(a)), a[0L], a[1L], a[2L], a[3L], a[4L], a[5L], a[6L], a[7L], a[8L], a[9L], a[10L], a[11L], a[12L], a[13L], a[14L]);
                    break;
                default: 
                    panic("Call " + p.Name + " with too many arguments " + itoa(len(a)) + ".");
                    break;
            }
        });

        // A LazyDLL implements access to a single DLL.
        // It will delay the load of the DLL until the first
        // call to its Handle method or to one of its
        // LazyProc's Addr method.
        //
        // LazyDLL is subject to the same DLL preloading attacks as documented
        // on LoadDLL.
        //
        // Use LazyDLL in golang.org/x/sys/windows for a secure way to
        // load system DLLs.
        public partial struct LazyDLL
        {
            public sync.Mutex mu;
            public ptr<DLL> dll; // non nil once DLL is loaded
            public @string Name;
        }

        // Load loads DLL file d.Name into memory. It returns an error if fails.
        // Load will not try to load DLL, if it is already loaded into memory.
        private static error Load(this ref LazyDLL _d) => func(_d, (ref LazyDLL d, Defer defer, Panic _, Recover __) =>
        { 
            // Non-racy version of:
            // if d.dll == nil {
            if (atomic.LoadPointer((@unsafe.Pointer.Value)(@unsafe.Pointer(ref d.dll))) == null)
            {
                d.mu.Lock();
                defer(d.mu.Unlock());
                if (d.dll == null)
                {
                    var (dll, e) = LoadDLL(d.Name);
                    if (e != null)
                    {
                        return error.As(e);
                    } 
                    // Non-racy version of:
                    // d.dll = dll
                    atomic.StorePointer((@unsafe.Pointer.Value)(@unsafe.Pointer(ref d.dll)), @unsafe.Pointer(dll));
                }
            }
            return error.As(null);
        });

        // mustLoad is like Load but panics if search fails.
        private static void mustLoad(this ref LazyDLL _d) => func(_d, (ref LazyDLL d, Defer _, Panic panic, Recover __) =>
        {
            var e = d.Load();
            if (e != null)
            {
                panic(e);
            }
        });

        // Handle returns d's module handle.
        private static System.UIntPtr Handle(this ref LazyDLL d)
        {
            d.mustLoad();
            return uintptr(d.dll.Handle);
        }

        // NewProc returns a LazyProc for accessing the named procedure in the DLL d.
        private static ref LazyProc NewProc(this ref LazyDLL d, @string name)
        {
            return ref new LazyProc(l:d,Name:name);
        }

        // NewLazyDLL creates new LazyDLL associated with DLL file.
        public static ref LazyDLL NewLazyDLL(@string name)
        {
            return ref new LazyDLL(Name:name);
        }

        // A LazyProc implements access to a procedure inside a LazyDLL.
        // It delays the lookup until the Addr method is called.
        public partial struct LazyProc
        {
            public sync.Mutex mu;
            public @string Name;
            public ptr<LazyDLL> l;
            public ptr<Proc> proc;
        }

        // Find searches DLL for procedure named p.Name. It returns
        // an error if search fails. Find will not search procedure,
        // if it is already found and loaded into memory.
        private static error Find(this ref LazyProc _p) => func(_p, (ref LazyProc p, Defer defer, Panic _, Recover __) =>
        { 
            // Non-racy version of:
            // if p.proc == nil {
            if (atomic.LoadPointer((@unsafe.Pointer.Value)(@unsafe.Pointer(ref p.proc))) == null)
            {
                p.mu.Lock();
                defer(p.mu.Unlock());
                if (p.proc == null)
                {
                    var e = p.l.Load();
                    if (e != null)
                    {
                        return error.As(e);
                    }
                    var (proc, e) = p.l.dll.FindProc(p.Name);
                    if (e != null)
                    {
                        return error.As(e);
                    } 
                    // Non-racy version of:
                    // p.proc = proc
                    atomic.StorePointer((@unsafe.Pointer.Value)(@unsafe.Pointer(ref p.proc)), @unsafe.Pointer(proc));
                }
            }
            return error.As(null);
        });

        // mustFind is like Find but panics if search fails.
        private static void mustFind(this ref LazyProc _p) => func(_p, (ref LazyProc p, Defer _, Panic panic, Recover __) =>
        {
            var e = p.Find();
            if (e != null)
            {
                panic(e);
            }
        });

        // Addr returns the address of the procedure represented by p.
        // The return value can be passed to Syscall to run the procedure.
        private static System.UIntPtr Addr(this ref LazyProc p)
        {
            p.mustFind();
            return p.proc.Addr();
        }

        //go:uintptrescapes

        // Call executes procedure p with arguments a. It will panic, if more than 15 arguments
        // are supplied.
        //
        // The returned error is always non-nil, constructed from the result of GetLastError.
        // Callers must inspect the primary return value to decide whether an error occurred
        // (according to the semantics of the specific function being called) before consulting
        // the error. The error will be guaranteed to contain syscall.Errno.
        private static (System.UIntPtr, System.UIntPtr, error) Call(this ref LazyProc p, params System.UIntPtr[] a)
        {
            p.mustFind();
            return p.proc.Call(a);
        }
    }
}

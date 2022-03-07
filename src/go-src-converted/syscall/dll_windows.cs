// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:08:12 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\dll_windows.go
using itoa = go.@internal.itoa_package;
using sysdll = go.@internal.syscall.windows.sysdll_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class syscall_package {

    // DLLError describes reasons for DLL load failures.
public partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

private static @string Error(this ptr<DLLError> _addr_e) {
    ref DLLError e = ref _addr_e.val;

    return e.Msg;
}

private static error Unwrap(this ptr<DLLError> _addr_e) {
    ref DLLError e = ref _addr_e.val;

    return error.As(e.Err)!;
}

// Implemented in ../runtime/syscall_windows.go.

public static (System.UIntPtr, System.UIntPtr, Errno) Syscall(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall6(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall12(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall15(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15);
public static (System.UIntPtr, System.UIntPtr, Errno) Syscall18(System.UIntPtr trap, System.UIntPtr nargs, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9, System.UIntPtr a10, System.UIntPtr a11, System.UIntPtr a12, System.UIntPtr a13, System.UIntPtr a14, System.UIntPtr a15, System.UIntPtr a16, System.UIntPtr a17, System.UIntPtr a18);
private static (System.UIntPtr, Errno) loadlibrary(ptr<ushort> filename);
private static (System.UIntPtr, Errno) loadsystemlibrary(ptr<ushort> filename, ptr<ushort> absoluteFilepath);
private static (System.UIntPtr, Errno) getprocaddress(System.UIntPtr handle, ptr<byte> procname);

// A DLL implements access to a single DLL.
public partial struct DLL {
    public @string Name;
    public Handle Handle;
}

// We use this for computing the absolute path for system DLLs on systems
// where SEARCH_SYSTEM32 is not available.
private static @string systemDirectoryPrefix = default;

private static void init() => func((_, panic, _) => {
    var n = uint32(MAX_PATH);
    while (true) {>>MARKER:FUNCTION_getprocaddress_BLOCK_PREFIX<<
        var b = make_slice<ushort>(n);
        var (l, e) = getSystemDirectory(_addr_b[0], n);
        if (e != null) {>>MARKER:FUNCTION_loadsystemlibrary_BLOCK_PREFIX<<
            panic("Unable to determine system directory: " + e.Error());
        }
        if (l <= n) {>>MARKER:FUNCTION_loadlibrary_BLOCK_PREFIX<<
            systemDirectoryPrefix = UTF16ToString(b[..(int)l]) + "\\";
            break;
        }
        n = l;

    }

});

// LoadDLL loads the named DLL file into memory.
//
// If name is not an absolute path and is not a known system DLL used by
// Go, Windows will search for the named DLL in many locations, causing
// potential DLL preloading attacks.
//
// Use LazyDLL in golang.org/x/sys/windows for a secure way to
// load system DLLs.
public static (ptr<DLL>, error) LoadDLL(@string name) {
    ptr<DLL> _p0 = default!;
    error _p0 = default!;

    var (namep, err) = UTF16PtrFromString(name);
    if (err != null) {>>MARKER:FUNCTION_Syscall18_BLOCK_PREFIX<<
        return (_addr_null!, error.As(err)!);
    }
    System.UIntPtr h = default;
    Errno e = default;
    if (sysdll.IsSystemDLL[name]) {>>MARKER:FUNCTION_Syscall15_BLOCK_PREFIX<<
        var (absoluteFilepathp, err) = UTF16PtrFromString(systemDirectoryPrefix + name);
        if (err != null) {>>MARKER:FUNCTION_Syscall12_BLOCK_PREFIX<<
            return (_addr_null!, error.As(err)!);
        }
        h, e = loadsystemlibrary(_addr_namep, _addr_absoluteFilepathp);

    }
    else
 {>>MARKER:FUNCTION_Syscall9_BLOCK_PREFIX<<
        h, e = loadlibrary(_addr_namep);
    }
    if (e != 0) {>>MARKER:FUNCTION_Syscall6_BLOCK_PREFIX<<
        return (_addr_null!, error.As(addr(new DLLError(Err:e,ObjName:name,Msg:"Failed to load "+name+": "+e.Error(),))!)!);
    }
    ptr<DLL> d = addr(new DLL(Name:name,Handle:Handle(h),));
    return (_addr_d!, error.As(null!)!);

}

// MustLoadDLL is like LoadDLL but panics if load operation fails.
public static ptr<DLL> MustLoadDLL(@string name) => func((_, panic, _) => {
    var (d, e) = LoadDLL(name);
    if (e != null) {>>MARKER:FUNCTION_Syscall_BLOCK_PREFIX<<
        panic(e);
    }
    return _addr_d!;

});

// FindProc searches DLL d for procedure named name and returns *Proc
// if found. It returns an error if search fails.
private static (ptr<Proc>, error) FindProc(this ptr<DLL> _addr_d, @string name) {
    ptr<Proc> proc = default!;
    error err = default!;
    ref DLL d = ref _addr_d.val;

    var (namep, err) = BytePtrFromString(name);
    if (err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (a, e) = getprocaddress(uintptr(d.Handle), _addr_namep);
    if (e != 0) {
        return (_addr_null!, error.As(addr(new DLLError(Err:e,ObjName:name,Msg:"Failed to find "+name+" procedure in "+d.Name+": "+e.Error(),))!)!);
    }
    ptr<Proc> p = addr(new Proc(Dll:d,Name:name,addr:a,));
    return (_addr_p!, error.As(null!)!);

}

// MustFindProc is like FindProc but panics if search fails.
private static ptr<Proc> MustFindProc(this ptr<DLL> _addr_d, @string name) => func((_, panic, _) => {
    ref DLL d = ref _addr_d.val;

    var (p, e) = d.FindProc(name);
    if (e != null) {
        panic(e);
    }
    return _addr_p!;

});

// Release unloads DLL d from memory.
private static error Release(this ptr<DLL> _addr_d) {
    error err = default!;
    ref DLL d = ref _addr_d.val;

    return error.As(FreeLibrary(d.Handle))!;
}

// A Proc implements access to a procedure inside a DLL.
public partial struct Proc {
    public ptr<DLL> Dll;
    public @string Name;
    public System.UIntPtr addr;
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
private static System.UIntPtr Addr(this ptr<Proc> _addr_p) {
    ref Proc p = ref _addr_p.val;

    return p.addr;
}

//go:uintptrescapes

// Call executes procedure p with arguments a. It will panic if more than 18 arguments
// are supplied.
//
// The returned error is always non-nil, constructed from the result of GetLastError.
// Callers must inspect the primary return value to decide whether an error occurred
// (according to the semantics of the specific function being called) before consulting
// the error. The error always has type syscall.Errno.
//
// On amd64, Call can pass and return floating-point values. To pass
// an argument x with C type "float", use
// uintptr(math.Float32bits(x)). To pass an argument with C type
// "double", use uintptr(math.Float64bits(x)). Floating-point return
// values are returned in r2. The return value for C type "float" is
// math.Float32frombits(uint32(r2)). For C type "double", it is
// math.Float64frombits(uint64(r2)).
private static (System.UIntPtr, System.UIntPtr, error) Call(this ptr<Proc> _addr_p, params System.UIntPtr[] a) => func((_, panic, _) => {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    error lastErr = default!;
    a = a.Clone();
    ref Proc p = ref _addr_p.val;

    switch (len(a)) {
        case 0: 
            return Syscall(p.Addr(), uintptr(len(a)), 0, 0, 0);
            break;
        case 1: 
            return Syscall(p.Addr(), uintptr(len(a)), a[0], 0, 0);
            break;
        case 2: 
            return Syscall(p.Addr(), uintptr(len(a)), a[0], a[1], 0);
            break;
        case 3: 
            return Syscall(p.Addr(), uintptr(len(a)), a[0], a[1], a[2]);
            break;
        case 4: 
            return Syscall6(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], 0, 0);
            break;
        case 5: 
            return Syscall6(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], 0);
            break;
        case 6: 
            return Syscall6(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5]);
            break;
        case 7: 
            return Syscall9(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], 0, 0);
            break;
        case 8: 
            return Syscall9(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], 0);
            break;
        case 9: 
            return Syscall9(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8]);
            break;
        case 10: 
            return Syscall12(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], 0, 0);
            break;
        case 11: 
            return Syscall12(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], 0);
            break;
        case 12: 
            return Syscall12(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11]);
            break;
        case 13: 
            return Syscall15(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], 0, 0);
            break;
        case 14: 
            return Syscall15(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], 0);
            break;
        case 15: 
            return Syscall15(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14]);
            break;
        case 16: 
            return Syscall18(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], 0, 0);
            break;
        case 17: 
            return Syscall18(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16], 0);
            break;
        case 18: 
            return Syscall18(p.Addr(), uintptr(len(a)), a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15], a[16], a[17]);
            break;
        default: 
            panic("Call " + p.Name + " with too many arguments " + itoa.Itoa(len(a)) + ".");
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
public partial struct LazyDLL {
    public sync.Mutex mu;
    public ptr<DLL> dll; // non nil once DLL is loaded
    public @string Name;
}

// Load loads DLL file d.Name into memory. It returns an error if fails.
// Load will not try to load DLL, if it is already loaded into memory.
private static error Load(this ptr<LazyDLL> _addr_d) => func((defer, _, _) => {
    ref LazyDLL d = ref _addr_d.val;
 
    // Non-racy version of:
    // if d.dll == nil {
    if (atomic.LoadPointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_d.dll))) == null) {
        d.mu.Lock();
        defer(d.mu.Unlock());
        if (d.dll == null) {
            var (dll, e) = LoadDLL(d.Name);
            if (e != null) {
                return error.As(e)!;
            } 
            // Non-racy version of:
            // d.dll = dll
            atomic.StorePointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_d.dll)), @unsafe.Pointer(dll));

        }
    }
    return error.As(null!)!;

});

// mustLoad is like Load but panics if search fails.
private static void mustLoad(this ptr<LazyDLL> _addr_d) => func((_, panic, _) => {
    ref LazyDLL d = ref _addr_d.val;

    var e = d.Load();
    if (e != null) {
        panic(e);
    }
});

// Handle returns d's module handle.
private static System.UIntPtr Handle(this ptr<LazyDLL> _addr_d) {
    ref LazyDLL d = ref _addr_d.val;

    d.mustLoad();
    return uintptr(d.dll.Handle);
}

// NewProc returns a LazyProc for accessing the named procedure in the DLL d.
private static ptr<LazyProc> NewProc(this ptr<LazyDLL> _addr_d, @string name) {
    ref LazyDLL d = ref _addr_d.val;

    return addr(new LazyProc(l:d,Name:name));
}

// NewLazyDLL creates new LazyDLL associated with DLL file.
public static ptr<LazyDLL> NewLazyDLL(@string name) {
    return addr(new LazyDLL(Name:name));
}

// A LazyProc implements access to a procedure inside a LazyDLL.
// It delays the lookup until the Addr, Call, or Find method is called.
public partial struct LazyProc {
    public sync.Mutex mu;
    public @string Name;
    public ptr<LazyDLL> l;
    public ptr<Proc> proc;
}

// Find searches DLL for procedure named p.Name. It returns
// an error if search fails. Find will not search procedure,
// if it is already found and loaded into memory.
private static error Find(this ptr<LazyProc> _addr_p) => func((defer, _, _) => {
    ref LazyProc p = ref _addr_p.val;
 
    // Non-racy version of:
    // if p.proc == nil {
    if (atomic.LoadPointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_p.proc))) == null) {
        p.mu.Lock();
        defer(p.mu.Unlock());
        if (p.proc == null) {
            var e = p.l.Load();
            if (e != null) {
                return error.As(e)!;
            }
            var (proc, e) = p.l.dll.FindProc(p.Name);
            if (e != null) {
                return error.As(e)!;
            } 
            // Non-racy version of:
            // p.proc = proc
            atomic.StorePointer((@unsafe.Pointer.val)(@unsafe.Pointer(_addr_p.proc)), @unsafe.Pointer(proc));

        }
    }
    return error.As(null!)!;

});

// mustFind is like Find but panics if search fails.
private static void mustFind(this ptr<LazyProc> _addr_p) => func((_, panic, _) => {
    ref LazyProc p = ref _addr_p.val;

    var e = p.Find();
    if (e != null) {
        panic(e);
    }
});

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
private static System.UIntPtr Addr(this ptr<LazyProc> _addr_p) {
    ref LazyProc p = ref _addr_p.val;

    p.mustFind();
    return p.proc.Addr();
}

//go:uintptrescapes

// Call executes procedure p with arguments a. See the documentation of
// Proc.Call for more information.
private static (System.UIntPtr, System.UIntPtr, error) Call(this ptr<LazyProc> _addr_p, params System.UIntPtr[] a) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    error lastErr = default!;
    a = a.Clone();
    ref LazyProc p = ref _addr_p.val;

    p.mustFind();
    return p.proc.Call(a);
}

} // end syscall_package

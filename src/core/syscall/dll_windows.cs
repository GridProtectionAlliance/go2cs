// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sysdll = @internal.syscall.windows.sysdll_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using @internal.syscall.windows;
using sync;
using ꓸꓸꓸuintptr = Span<uintptr>;

partial class syscall_package {

// DLLError describes reasons for DLL load failures.
[GoType] partial struct DLLError {
    public error Err;
    public @string ObjName;
    public @string Msg;
}

[GoRecv] public static @string Error(this ref DLLError e) {
    return e.Msg;
}

[GoRecv] public static error Unwrap(this ref DLLError e) {
    return e.Err;
}

// Implemented in ../runtime/syscall_windows.go.

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3);

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall6(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6);

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall9(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9);

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall12(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12);

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall15(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15);

// Deprecated: Use [SyscallN] instead.
public static partial (uintptr r1, uintptr r2, Errno err) Syscall18(uintptr trap, uintptr nargs, uintptr a1, uintptr a2, uintptr a3, uintptr a4, uintptr a5, uintptr a6, uintptr a7, uintptr a8, uintptr a9, uintptr a10, uintptr a11, uintptr a12, uintptr a13, uintptr a14, uintptr a15, uintptr a16, uintptr a17, uintptr a18);

public static partial (uintptr r1, uintptr r2, Errno err) SyscallN(uintptr trap, params ꓸꓸꓸuintptr argsʗp);

internal static partial (uintptr handle, Errno err) loadlibrary(ж<uint16> filename);

internal static partial (uintptr handle, Errno err) loadsystemlibrary(ж<uint16> filename);

internal static partial (uintptr proc, Errno err) getprocaddress(uintptr handle, ж<uint8> procname);

// A DLL implements access to a single DLL.
[GoType] partial struct DLL {
    public @string Name;
    public ΔHandle Handle;
}

// LoadDLL loads the named DLL file into memory.
//
// If name is not an absolute path and is not a known system DLL used by
// Go, Windows will search for the named DLL in many locations, causing
// potential DLL preloading attacks.
//
// Use [LazyDLL] in golang.org/x/sys/windows for a secure way to
// load system DLLs.
public static (ж<DLL>, error) LoadDLL(@string name) {
    (namep, err) = UTF16PtrFromString(name);
    if (err != default!) {
        return (default!, err);
    }
    uintptr h = default!;
    ref var e = ref heap(new Errno(), out var Ꮡe);
    if (sysdll.IsSystemDLL[name]){
        (h, e) = loadsystemlibrary(namep);
    } else {
        (h, e) = loadlibrary(namep);
    }
    if (e != 0) {
        return (default!, new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to load "u8 + name + ": "u8 + e.Error()
        ));
    }
    var d = Ꮡ(new DLL(
        Name: name,
        ΔHandle: ((ΔHandle)h)
    ));
    return (d, default!);
}

// MustLoadDLL is like [LoadDLL] but panics if load operation fails.
public static ж<DLL> MustLoadDLL(@string name) {
    (d, e) = LoadDLL(name);
    if (e != default!) {
        throw panic(e);
    }
    return d;
}

// FindProc searches [DLL] d for procedure named name and returns [*Proc]
// if found. It returns an error if search fails.
[GoRecv] public static (ж<Proc> proc, error err) FindProc(this ref DLL d, @string name) {
    ж<Proc> proc = default!;
    error err = default!;

    (namep, err) = BytePtrFromString(name);
    if (err != default!) {
        return (default!, err);
    }
    (a, e) = getprocaddress(((uintptr)d.Handle), namep);
    if (e != 0) {
        return (default!, new DLLError(
            Err: e,
            ObjName: name,
            Msg: "Failed to find "u8 + name + " procedure in "u8 + d.Name + ": "u8 + e.Error()
        ));
    }
    var p = Ꮡ(new Proc(
        Dll: d,
        Name: name,
        addr: a
    ));
    return (p, default!);
}

// MustFindProc is like [DLL.FindProc] but panics if search fails.
[GoRecv] public static ж<Proc> MustFindProc(this ref DLL d, @string name) {
    (p, e) = d.FindProc(name);
    if (e != default!) {
        throw panic(e);
    }
    return p;
}

// Release unloads [DLL] d from memory.
[GoRecv] public static error /*err*/ Release(this ref DLL d) {
    error err = default!;

    return FreeLibrary(d.Handle);
}

// A Proc implements access to a procedure inside a [DLL].
[GoType] partial struct Proc {
    public ж<DLL> Dll;
    public @string Name;
    internal uintptr addr;
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
[GoRecv] public static uintptr Addr(this ref Proc p) {
    return p.addr;
}

// Call executes procedure p with arguments a.
//
// The returned error is always non-nil, constructed from the result of GetLastError.
// Callers must inspect the primary return value to decide whether an error occurred
// (according to the semantics of the specific function being called) before consulting
// the error. The error always has type [Errno].
//
// On amd64, Call can pass and return floating-point values. To pass
// an argument x with C type "float", use
// uintptr(math.Float32bits(x)). To pass an argument with C type
// "double", use uintptr(math.Float64bits(x)). Floating-point return
// values are returned in r2. The return value for C type "float" is
// [math.Float32frombits](uint32(r2)). For C type "double", it is
// [math.Float64frombits](uint64(r2)).
//
//go:uintptrescapes
[GoRecv] public static (uintptr, uintptr, error) Call(this ref Proc p, params ꓸꓸꓸuintptr aʗp) {
    var a = aʗp.slice();

    return SyscallN(p.Addr(), a.ꓸꓸꓸ);
}

// A LazyDLL implements access to a single [DLL].
// It will delay the load of the DLL until the first
// call to its [LazyDLL.Handle] method or to one of its
// [LazyProc]'s Addr method.
//
// LazyDLL is subject to the same DLL preloading attacks as documented
// on [LoadDLL].
//
// Use LazyDLL in golang.org/x/sys/windows for a secure way to
// load system DLLs.
[GoType] partial struct LazyDLL {
    internal sync_package.Mutex mu;
    internal ж<DLL> dll; // non nil once DLL is loaded
    public @string Name;
}

// Load loads DLL file d.Name into memory. It returns an error if fails.
// Load will not try to load DLL, if it is already loaded into memory.
[GoRecv] public static error Load(this ref LazyDLL d) => func((defer, _) => {
    // Non-racy version of:
    // if d.dll == nil {
    if ((uintptr)atomic.LoadPointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(d.dll))))) == nil) {
        d.mu.Lock();
        defer(d.mu.Unlock);
        if (d.dll == nil) {
            (dll, e) = LoadDLL(d.Name);
            if (e != default!) {
                return e;
            }
            // Non-racy version of:
            // d.dll = dll
            atomic.StorePointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(d.dll)))), new @unsafe.Pointer(dll));
        }
    }
    return default!;
});

// mustLoad is like Load but panics if search fails.
[GoRecv] internal static void mustLoad(this ref LazyDLL d) {
    var e = d.Load();
    if (e != default!) {
        throw panic(e);
    }
}

// Handle returns d's module handle.
[GoRecv] public static uintptr Handle(this ref LazyDLL d) {
    d.mustLoad();
    return ((uintptr)d.dll.Handle);
}

// NewProc returns a [LazyProc] for accessing the named procedure in the [DLL] d.
[GoRecv] public static ж<LazyProc> NewProc(this ref LazyDLL d, @string name) {
    return Ꮡ(new LazyProc(l: d, Name: name));
}

// NewLazyDLL creates new [LazyDLL] associated with [DLL] file.
public static ж<LazyDLL> NewLazyDLL(@string name) {
    return Ꮡ(new LazyDLL(Name: name));
}

// A LazyProc implements access to a procedure inside a [LazyDLL].
// It delays the lookup until the [LazyProc.Addr], [LazyProc.Call], or [LazyProc.Find] method is called.
[GoType] partial struct LazyProc {
    internal sync_package.Mutex mu;
    public @string Name;
    internal ж<LazyDLL> l;
    internal ж<Proc> proc;
}

// Find searches [DLL] for procedure named p.Name. It returns
// an error if search fails. Find will not search procedure,
// if it is already found and loaded into memory.
[GoRecv] public static error Find(this ref LazyProc p) => func((defer, _) => {
    // Non-racy version of:
    // if p.proc == nil {
    if ((uintptr)atomic.LoadPointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(p.proc))))) == nil) {
        p.mu.Lock();
        defer(p.mu.Unlock);
        if (p.proc == nil) {
            var e = p.l.Load();
            if (e != default!) {
                return e;
            }
            (proc, e) = p.l.dll.FindProc(p.Name);
            if (e != default!) {
                return e;
            }
            // Non-racy version of:
            // p.proc = proc
            atomic.StorePointer((ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)(Ꮡ(p.proc)))), new @unsafe.Pointer(proc));
        }
    }
    return default!;
});

// mustFind is like Find but panics if search fails.
[GoRecv] internal static void mustFind(this ref LazyProc p) {
    var e = p.Find();
    if (e != default!) {
        throw panic(e);
    }
}

// Addr returns the address of the procedure represented by p.
// The return value can be passed to Syscall to run the procedure.
[GoRecv] public static uintptr Addr(this ref LazyProc p) {
    p.mustFind();
    return p.proc.Addr();
}

// Call executes procedure p with arguments a. See the documentation of
// Proc.Call for more information.
//
//go:uintptrescapes
[GoRecv] public static (uintptr r1, uintptr r2, error lastErr) Call(this ref LazyProc p, params ꓸꓸꓸuintptr aʗp) {
    uintptr r1 = default!;
    uintptr r2 = default!;
    error lastErr = default!;
    var a = aʗp.slice();

    p.mustFind();
    return p.proc.Call(a.ꓸꓸꓸ);
}

} // end syscall_package

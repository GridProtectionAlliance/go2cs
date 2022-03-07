// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:36 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_solaris.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct mts {
    public long tv_sec;
    public long tv_nsec;
}

private partial struct mscratch {
    public array<System.UIntPtr> v;
}

private partial struct mOS {
    public System.UIntPtr waitsema; // semaphore for parking on locks
    public ptr<int> perrno; // pointer to tls errno
// these are here because they are too large to be on the stack
// of low-level NOSPLIT functions.
//LibCall       libcall;
    public mts ts;
    public mscratch scratch;
}

private partial struct libcFunc { // : System.UIntPtr
}

//go:linkname asmsysvicall6x runtime.asmsysvicall6
private static libcFunc asmsysvicall6x = default; // name to take addr of asmsysvicall6

private static void asmsysvicall6(); // declared for vet; do NOT call

//go:nosplit
private static System.UIntPtr sysvicall0(ptr<libcFunc> _addr_fn) {
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {>>MARKER:FUNCTION_asmsysvicall6_BLOCK_PREFIX<<
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null; // See comment in sys_darwin.go:libcCall
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 0;
    libcall.args = uintptr(@unsafe.Pointer(fn)); // it's unused but must be non-nil, otherwise crashes
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return libcall.r1;

}

//go:nosplit
private static System.UIntPtr sysvicall1(ptr<libcFunc> _addr_fn, System.UIntPtr a1) {
    ref libcFunc fn = ref _addr_fn.val;

    var (r1, _) = sysvicall1Err(_addr_fn, a1);
    return r1;
}

//go:nosplit

// sysvicall1Err returns both the system call result and the errno value.
// This is used by sysvicall1 and pipe.
private static (System.UIntPtr, System.UIntPtr) sysvicall1Err(ptr<libcFunc> _addr_fn, System.UIntPtr a1) {
    System.UIntPtr r1 = default;
    System.UIntPtr err = default;
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 1; 
    // TODO(rsc): Why is noescape necessary here and below?
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return (libcall.r1, libcall.err);

}

//go:nosplit
private static System.UIntPtr sysvicall2(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2) {
    ref libcFunc fn = ref _addr_fn.val;

    var (r1, _) = sysvicall2Err(_addr_fn, a1, a2);
    return r1;
}

//go:nosplit
//go:cgo_unsafe_args

// sysvicall2Err returns both the system call result and the errno value.
// This is used by sysvicall2 and pipe2.
private static (System.UIntPtr, System.UIntPtr) sysvicall2Err(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2) {
    System.UIntPtr _p0 = default;
    System.UIntPtr _p0 = default;
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 2;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return (libcall.r1, libcall.err);

}

//go:nosplit
private static System.UIntPtr sysvicall3(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    ref libcFunc fn = ref _addr_fn.val;

    var (r1, _) = sysvicall3Err(_addr_fn, a1, a2, a3);
    return r1;
}

//go:nosplit
//go:cgo_unsafe_args

// sysvicall3Err returns both the system call result and the errno value.
// This is used by sysicall3 and write1.
private static (System.UIntPtr, System.UIntPtr) sysvicall3Err(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr err = default;
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 3;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return (libcall.r1, libcall.err);

}

//go:nosplit
private static System.UIntPtr sysvicall4(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4) {
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 4;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return libcall.r1;

}

//go:nosplit
private static System.UIntPtr sysvicall5(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5) {
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 5;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return libcall.r1;

}

//go:nosplit
private static System.UIntPtr sysvicall6(ptr<libcFunc> _addr_fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    ref libcFunc fn = ref _addr_fn.val;
 
    // Leave caller's PC/SP around for traceback.
    var gp = getg();
    ptr<m> mp;
    if (gp != null) {
        mp = gp.m;
    }
    if (mp != null && mp.libcallsp == 0) {
        mp.libcallg.set(gp);
        mp.libcallpc = getcallerpc(); 
        // sp must be the last, because once async cpu profiler finds
        // all three values to be non-zero, it will use them
        mp.libcallsp = getcallersp();

    }
    else
 {
        mp = null;
    }
    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(fn));
    libcall.n = 6;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_a1)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    if (mp != null) {
        mp.libcallsp = 0;
    }
    return libcall.r1;

}

} // end runtime_package

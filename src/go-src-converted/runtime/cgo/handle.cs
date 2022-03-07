// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cgo -- go2cs converted at 2022 March 06 22:12:30 UTC
// import "runtime/cgo" ==> using cgo = go.runtime.cgo_package
// Original source: C:\Program Files\Go\src\runtime\cgo\handle.go
using sync = go.sync_package;
using atomic = go.sync.atomic_package;

namespace go.runtime;

public static partial class cgo_package {

    // Handle provides a way to pass values that contain Go pointers
    // (pointers to memory allocated by Go) between Go and C without
    // breaking the cgo pointer passing rules. A Handle is an integer
    // value that can represent any Go value. A Handle can be passed
    // through C and back to Go, and Go code can use the Handle to
    // retrieve the original Go value.
    //
    // The underlying type of Handle is guaranteed to fit in an integer type
    // that is large enough to hold the bit pattern of any pointer. The zero
    // value of a Handle is not valid, and thus is safe to use as a sentinel
    // in C APIs.
    //
    // For instance, on the Go side:
    //
    //    package main
    //
    //    /*
    //    #include <stdint.h> // for uintptr_t
    //
    //    extern void MyGoPrint(uintptr_t handle);
    //    void myprint(uintptr_t handle);
    //    */
    //    import "C"
    //    import "runtime/cgo"
    //
    //    //export MyGoPrint
    //    func MyGoPrint(handle C.uintptr_t) {
    //        h := cgo.Handle(handle)
    //        val := h.Value().(string)
    //        println(val)
    //        h.Delete()
    //    }
    //
    //    func main() {
    //        val := "hello Go"
    //        C.myprint(C.uintptr_t(cgo.NewHandle(val)))
    //        // Output: hello Go
    //    }
    //
    // and on the C side:
    //
    //    #include <stdint.h> // for uintptr_t
    //
    //    // A Go function
    //    extern void MyGoPrint(uintptr_t handle);
    //
    //    // A C function
    //    void myprint(uintptr_t handle) {
    //        MyGoPrint(handle);
    //    }
public partial struct Handle { // : System.UIntPtr
}

// NewHandle returns a handle for a given value.
//
// The handle is valid until the program calls Delete on it. The handle
// uses resources, and this package assumes that C code may hold on to
// the handle, so a program must explicitly call Delete when the handle
// is no longer needed.
//
// The intended use is to pass the returned handle to C code, which
// passes it back to Go, which calls Value.
public static Handle NewHandle(object v) => func((_, panic, _) => {
    var h = atomic.AddUintptr(_addr_handleIdx, 1);
    if (h == 0) {
        panic("runtime/cgo: ran out of handle space");
    }
    handles.Store(h, v);
    return Handle(h);

});

// Value returns the associated Go value for a valid handle.
//
// The method panics if the handle is invalid.
public static void Value(this Handle h) => func((_, panic, _) => {
    var (v, ok) = handles.Load(uintptr(h));
    if (!ok) {
        panic("runtime/cgo: misuse of an invalid Handle");
    }
    return v;

});

// Delete invalidates a handle. This method should only be called once
// the program no longer needs to pass the handle to C and the C code
// no longer has a copy of the handle value.
//
// The method panics if the handle is invalid.
public static void Delete(this Handle h) => func((_, panic, _) => {
    var (_, ok) = handles.LoadAndDelete(uintptr(h));
    if (!ok) {
        panic("runtime/cgo: misuse of an invalid Handle");
    }
});

private static sync.Map handles = new sync.Map();private static System.UIntPtr handleIdx = default;

} // end cgo_package

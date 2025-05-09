// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
The weak package is a package for managing weak pointers.

Weak pointers are pointers that explicitly do not keep a value live and
must be queried for a regular Go pointer.
The result of such a query may be observed as nil at any point after a
weakly-pointed-to object becomes eligible for reclamation by the garbage
collector.
More specifically, weak pointers become nil as soon as the garbage collector
identifies that the object is unreachable, before it is made reachable
again by a finalizer.
In terms of the C# language, these semantics are roughly equivalent to the
the semantics of "short" weak references.
In terms of the Java language, these semantics are roughly equivalent to the
semantics of the WeakReference type.

Using go:linkname to access this package and the functions it references
is explicitly forbidden by the toolchain because the semantics of this
package have not gone through the proposal process. By exposing this
functionality, we risk locking in the existing semantics due to Hyrum's Law.

If you believe you have a good use-case for weak references not already
covered by the standard library, file a proposal issue at
https://github.com/golang/go/issues instead of relying on this package.
*/
namespace go.@internal;

using abi = @internal.abi_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class weak_package {

// Pointer is a weak pointer to a value of type T.
//
// This value is comparable is guaranteed to compare equal if the pointers
// that they were created from compare equal. This property is retained even
// after the object referenced by the pointer used to create a weak reference
// is reclaimed.
//
// If multiple weak pointers are made to different offsets within same object
// (for example, pointers to different fields of the same struct), those pointers
// will not compare equal.
// If a weak pointer is created from an object that becomes reachable again due
// to a finalizer, that weak pointer will not compare equal with weak pointers
// created before it became unreachable.
[GoType] partial struct Pointer<T>
    where T : new()
{
    internal @unsafe.Pointer u;
}

// Make creates a weak pointer from a strong pointer to some value of type T.
public static Pointer<T> Make<T>(ж<T> Ꮡptr)
    where T : new()
{
    ref var ptr = ref Ꮡptr.val;

    // Explicitly force ptr to escape to the heap.
    ptr = abi.Escape(ptr);
    @unsafe.Pointer u = default!;
    if (ptr != nil) {
        u = (uintptr)runtime_registerWeakPointer(new @unsafe.Pointer(Ꮡptr));
    }
    runtime.KeepAlive(ptr);
    return new Pointer<T>(u.val);
}

// Strong creates a strong pointer from the weak pointer.
// Returns nil if the original value for the weak pointer was reclaimed by
// the garbage collector.
// If a weak pointer points to an object with a finalizer, then Strong will
// return nil as soon as the object's finalizer is queued for execution.
public static ж<T> Strong<T>(this Pointer<T> p)
    where T : new()
{
    return (ж<T>)(uintptr)(runtime_makeStrongFromWeak(p.u));
}

// Implemented in runtime.

//go:linkname runtime_registerWeakPointer
internal static partial @unsafe.Pointer runtime_registerWeakPointer(@unsafe.Pointer _);

//go:linkname runtime_makeStrongFromWeak
internal static partial @unsafe.Pointer runtime_makeStrongFromWeak(@unsafe.Pointer _);

} // end weak_package

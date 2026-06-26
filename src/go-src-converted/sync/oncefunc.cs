// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class sync_package {

// OnceFunc returns a function that invokes f only once. The returned function
// may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Action OnceFunc(Action f) => func((defer, recover) => {
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = default!;
    any p = default!;
    // Construct the inner closure just once to reduce costs on the fast path.
    var g = 
    var pʗ1 = p;
    () => {
        var pʗ2 = p;
        defer(() => {
            pʗ2 = recover();
            if (!valid) {
                // Re-panic immediately so on the first call the user gets a
                // complete stack trace into f.
                throw panic(pʗ2);
            }
        });
        f();
        f = default!;
        // Do not keep f alive after invoking it.
        valid = true;
    };
    // Set only if f does not panic.
    var gʗ1 = g;
    var onceʗ1 = once;
    var pʗ3 = p;
    return () => {
        onceʗ1.Do(gʗ1);
        if (!valid) {
            throw panic(pʗ3);
        }
    };
});

// OnceValue returns a function that invokes f only once and returns the value
// returned by f. The returned function may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Func<T> OnceValue<T>(Func<T> f)
    where T : new() => func((defer, recover) =>
{
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = default!;
    any p = default!;
    T result = default!;
    var g = 
    var pʗ1 = p;
    var resultʗ1 = result;
    () => {
        var pʗ2 = p;
        defer(() => {
            pʗ2 = recover();
            if (!valid) {
                throw panic(pʗ2);
            }
        });
        result = f();
        f = default!;
        valid = true;
    };
    var gʗ1 = g;
    var onceʗ1 = once;
    var pʗ3 = p;
    var resultʗ2 = result;
    return () => {
        onceʗ1.Do(gʗ1);
        if (!valid) {
            throw panic(pʗ3);
        }
        return resultʗ2;
    };
});

// OnceValues returns a function that invokes f only once and returns the values
// returned by f. The returned function may be called concurrently.
//
// If f panics, the returned function will panic with the same value on every call.
public static Func<(T1, T2)> OnceValues<T1, T2>(Func<(T1, T2)> f)
    where T1 : new()
    where T2 : new() => func((defer, recover) =>
{
    ref var once = ref heap(new Once(), out var Ꮡonce);
    bool valid = default!;
    any p = default!;
    T1 r1 = default!;
    T2 r2 = default!;
    var g = 
    var pʗ1 = p;
    var r1ʗ1 = r1;
    var r2ʗ1 = r2;
    () => {
        var pʗ2 = p;
        defer(() => {
            pʗ2 = recover();
            if (!valid) {
                throw panic(pʗ2);
            }
        });
        (r1, r2) = f();
        f = default!;
        valid = true;
    };
    var gʗ1 = g;
    var onceʗ1 = once;
    var pʗ3 = p;
    var r1ʗ2 = r1;
    var r2ʗ2 = r2;
    return () => {
        onceʗ1.Do(gʗ1);
        if (!valid) {
            throw panic(pʗ3);
        }
        return (r1ʗ2, r2ʗ2);
    };
});

} // end sync_package

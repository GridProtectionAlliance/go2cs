// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (the reflection bridge — Phase 4).
//
// Go's abi.TypeOf reads an interface's type-word via unsafe.Pointer to reach a Go runtime type
// descriptor that has no managed equivalent: an `any` here is a single System.Object reference, not
// a two-word eface over a descriptor, so reinterpreting the reference reads garbage and NREs (the
// first operational hit is fmt.Print/Sprint → doPrint → reflect.TypeOf(arg).Kind()). Instead,
// synthesize an abi.Type descriptor whose Kind_ is classified from the value's managed System.Type
// (golib GoReflect.KindOf), and record the System.Type on the descriptor box so the hand-owned
// reflect Type/Value methods can recover Go type info from it. The converter skips the auto form of
// TypeOf via the manualConversionFuncs registry (go2cs/manualTypeOperations.go); this module marker
// also makes go2cs skip re-converting this file wholesale, and the overlay restores it over auto
// output on every reconversion. See docs/Phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go.@internal;

partial class abi_package {

// The managed System.Type this synthetic abi.Type stands for — carried directly on the descriptor so
// the reflect Type methods (String/Name/Elem/Field) can recover Go type info from it (the reflect
// rtype wraps an abi.Type by value, so the field rides along the copy). Null for a non-synthesized Type.
partial struct Type {
    public System.Type? sysType;
}

// synthType builds a managed-backed abi.Type from a System.Type: Kind_ classified from it (GoReflect)
// and the System.Type carried on the descriptor. The single builder behind both TypeOf (from a value)
// and reflect's Type.Elem/Field (from a type with no value).
public static ж<Type> synthType(System.Type? st) {
    if (st is null) {
        return default!;
    }
    ref var t = ref heap<Type>(out var Ꮡt);
    t.Kind_ = (ΔKind)((uint8)GoReflect.KindOf(st));
    t.sysType = st;
    // Carry Go comparability on the descriptor: reflect.Type.Comparable and internal/reflectlite's
    // Comparable both report `Equal != nil`, and errors.Is gates its equality match on the latter — so a
    // comparable Go type (e.g. the *errorString behind a sentinel like csv.ErrFieldCount) must have a
    // non-nil Equal or errors.Is(err, sentinel) silently returns false. A synthetic descriptor carries no
    // addressable value memory, so this is a comparability signal, not a real bit-compare; the delegate
    // compares its pointer arguments as a safe, non-throwing fallback should any path invoke it directly.
    if (GoReflect.IsComparable(st)) {
        t.Equal = static (p, q) => AreEqual(p, q);
    }
    return Ꮡt;
}

// TypeOf returns the abi.Type of some value. The descriptor stands for the value's GO dynamic
// type: an interface-carrier wrapper (IжAdapter / IInterfaceAdapter chain) unwraps to the *T box
// / original value it stands for (GoReflect.GoDynamicTypeOf, R10), so adapter-held and raw-box
// values of one Go type share one canonical descriptor.
public static ж<Type> TypeOf(any a) {
    return a == default! ? default! : synthType(GoReflect.GoDynamicTypeOf(a));
}

} // end abi_package

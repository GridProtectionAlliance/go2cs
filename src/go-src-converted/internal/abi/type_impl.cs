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

// TypeOf returns the abi.Type of some value.
public static ж<Type> TypeOf(any a) {
    if (a == default!) {
        return default!;
    }
    ref var t = ref heap<Type>(out var Ꮡt);
    t.Kind_ = (ΔKind)((uint8)GoReflect.KindOf(a.GetType()));
    GoReflect.Register(Ꮡt, a.GetType());
    return Ꮡt;
}

} // end abi_package

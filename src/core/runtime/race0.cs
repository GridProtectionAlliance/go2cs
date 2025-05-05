// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !race
// Dummy race detection API, used when not built with -race.
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal const bool raceenabled = false;

// Because raceenabled is false, none of these functions should be called.
internal static void raceReadObjectPC(ж<_type> Ꮡt, @unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
    ref var t = ref Ꮡt.val;

    @throw("race"u8);
}

internal static void raceWriteObjectPC(ж<_type> Ꮡt, @unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
    ref var t = ref Ꮡt.val;

    @throw("race"u8);
}

internal static (uintptr, uintptr) raceinit() {
    @throw("race"u8);
    return (0, 0);
}

internal static void racefini() {
    @throw("race"u8);
}

internal static uintptr raceproccreate() {
    @throw("race"u8);
    return 0;
}

internal static void raceprocdestroy(uintptr ctx) {
    @throw("race"u8);
}

internal static void racemapshadow(@unsafe.Pointer addr, uintptr size) {
    @throw("race"u8);
}

internal static void racewritepc(@unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
    @throw("race"u8);
}

internal static void racereadpc(@unsafe.Pointer addr, uintptr callerpc, uintptr pc) {
    @throw("race"u8);
}

internal static void racereadrangepc(@unsafe.Pointer addr, uintptr sz, uintptr callerpc, uintptr pc) {
    @throw("race"u8);
}

internal static void racewriterangepc(@unsafe.Pointer addr, uintptr sz, uintptr callerpc, uintptr pc) {
    @throw("race"u8);
}

internal static void raceacquire(@unsafe.Pointer addr) {
    @throw("race"u8);
}

internal static void raceacquireg(ж<g> Ꮡgp, @unsafe.Pointer addr) {
    ref var gp = ref Ꮡgp.val;

    @throw("race"u8);
}

internal static void raceacquirectx(uintptr racectx, @unsafe.Pointer addr) {
    @throw("race"u8);
}

internal static void racerelease(@unsafe.Pointer addr) {
    @throw("race"u8);
}

internal static void racereleaseg(ж<g> Ꮡgp, @unsafe.Pointer addr) {
    ref var gp = ref Ꮡgp.val;

    @throw("race"u8);
}

internal static void racereleaseacquire(@unsafe.Pointer addr) {
    @throw("race"u8);
}

internal static void racereleaseacquireg(ж<g> Ꮡgp, @unsafe.Pointer addr) {
    ref var gp = ref Ꮡgp.val;

    @throw("race"u8);
}

internal static void racereleasemerge(@unsafe.Pointer addr) {
    @throw("race"u8);
}

internal static void racereleasemergeg(ж<g> Ꮡgp, @unsafe.Pointer addr) {
    ref var gp = ref Ꮡgp.val;

    @throw("race"u8);
}

internal static void racefingo() {
    @throw("race"u8);
}

internal static void racemalloc(@unsafe.Pointer Δp, uintptr sz) {
    @throw("race"u8);
}

internal static void racefree(@unsafe.Pointer Δp, uintptr sz) {
    @throw("race"u8);
}

internal static uintptr racegostart(uintptr pc) {
    @throw("race"u8);
    return 0;
}

internal static void racegoend() {
    @throw("race"u8);
}

internal static void racectxend(uintptr racectx) {
    @throw("race"u8);
}

} // end runtime_package

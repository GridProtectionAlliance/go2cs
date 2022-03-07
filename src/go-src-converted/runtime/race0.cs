// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !race
// +build !race

// Dummy race detection API, used when not built with -race.

// package runtime -- go2cs converted at 2022 March 06 22:11:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\race0.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly var raceenabled = false;

// Because raceenabled is false, none of these functions should be called.



// Because raceenabled is false, none of these functions should be called.

private static void raceReadObjectPC(ptr<_type> _addr_t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    ref _type t = ref _addr_t.val;

    throw("race");
}
private static void raceWriteObjectPC(ptr<_type> _addr_t, unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    ref _type t = ref _addr_t.val;

    throw("race");
}
private static (System.UIntPtr, System.UIntPtr) raceinit() {
    System.UIntPtr _p0 = default;
    System.UIntPtr _p0 = default;

    throw("race");

    return (0, 0);
}
private static void racefini() {
    throw("race");
}
private static System.UIntPtr raceproccreate() {
    throw("race");

    return 0;
}
private static void raceprocdestroy(System.UIntPtr ctx) {
    throw("race");
}
private static void racemapshadow(unsafe.Pointer addr, System.UIntPtr size) {
    throw("race");
}
private static void racewritepc(unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    throw("race");
}
private static void racereadpc(unsafe.Pointer addr, System.UIntPtr callerpc, System.UIntPtr pc) {
    throw("race");
}
private static void racereadrangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callerpc, System.UIntPtr pc) {
    throw("race");
}
private static void racewriterangepc(unsafe.Pointer addr, System.UIntPtr sz, System.UIntPtr callerpc, System.UIntPtr pc) {
    throw("race");
}
private static void raceacquire(unsafe.Pointer addr) {
    throw("race");
}
private static void raceacquireg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    throw("race");
}
private static void raceacquirectx(System.UIntPtr racectx, unsafe.Pointer addr) {
    throw("race");
}
private static void racerelease(unsafe.Pointer addr) {
    throw("race");
}
private static void racereleaseg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    throw("race");
}
private static void racereleaseacquire(unsafe.Pointer addr) {
    throw("race");
}
private static void racereleaseacquireg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    throw("race");
}
private static void racereleasemerge(unsafe.Pointer addr) {
    throw("race");
}
private static void racereleasemergeg(ptr<g> _addr_gp, unsafe.Pointer addr) {
    ref g gp = ref _addr_gp.val;

    throw("race");
}
private static void racefingo() {
    throw("race");
}
private static void racemalloc(unsafe.Pointer p, System.UIntPtr sz) {
    throw("race");
}
private static void racefree(unsafe.Pointer p, System.UIntPtr sz) {
    throw("race");
}
private static System.UIntPtr racegostart(System.UIntPtr pc) {
    throw("race");

    return 0;
}
private static void racegoend() {
    throw("race");
}
private static void racectxend(System.UIntPtr racectx) {
    throw("race");
}

} // end runtime_package

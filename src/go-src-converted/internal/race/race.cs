// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build race
// +build race

// package race -- go2cs converted at 2022 March 13 05:24:05 UTC
// import "internal/race" ==> using race = go.@internal.race_package
// Original source: C:\Program Files\Go\src\internal\race\race.go
namespace go.@internal;

using runtime = runtime_package;
using @unsafe = @unsafe_package;

public static partial class race_package {

public static readonly var Enabled = true;



public static void Acquire(unsafe.Pointer addr) {
    runtime.RaceAcquire(addr);
}

public static void Release(unsafe.Pointer addr) {
    runtime.RaceRelease(addr);
}

public static void ReleaseMerge(unsafe.Pointer addr) {
    runtime.RaceReleaseMerge(addr);
}

public static void Disable() {
    runtime.RaceDisable();
}

public static void Enable() {
    runtime.RaceEnable();
}

public static void Read(unsafe.Pointer addr) {
    runtime.RaceRead(addr);
}

public static void Write(unsafe.Pointer addr) {
    runtime.RaceWrite(addr);
}

public static void ReadRange(unsafe.Pointer addr, nint len) {
    runtime.RaceReadRange(addr, len);
}

public static void WriteRange(unsafe.Pointer addr, nint len) {
    runtime.RaceWriteRange(addr, len);
}

public static nint Errors() {
    return runtime.RaceErrors();
}

} // end race_package

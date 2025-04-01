// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

partial class abi_package {

[GoType("num:nint")] partial struct RF_State;

// These constants are shared between the compiler, which uses them for state functions
// and panic indicators, and the runtime, which turns them into more meaningful strings
// For best code generation, RF_DONE and RF_READY should be 0 and 1.
public static readonly RF_State RF_DONE = /* RF_State(iota) */ 0; // body of loop has exited in a non-panic way

public static readonly RF_State RF_READY = 1;           // body of loop has not exited yet, is not running  -- this is not a panic index

public static readonly RF_State RF_PANIC = 2;           // body of loop is either currently running, or has panicked

public static readonly RF_State RF_EXHAUSTED = 3;       // iterator function return, i.e., sequence is "exhausted"

public const nint RF_MISSING_PANIC = 4;       // body of loop panicked but iterator function defer-recovered it away

} // end abi_package

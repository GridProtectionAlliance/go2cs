// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:41 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_plan9.go


namespace go;

public static partial class runtime_package {

private partial struct sigTabT {
    public nint flags;
    public @string name;
}

// Incoming notes are compared against this table using strncmp, so the
// order matters: longer patterns must appear before their prefixes.
// There are _SIG constants in os2_plan9.go for the table index of some
// of these.
//
// If you add entries to this table, you must respect the prefix ordering
// and also update the constant values is os2_plan9.go.
private static array<sigTabT> sigtable = new array<sigTabT>(new sigTabT[] { {_SigThrow,"sys: trap: debug exception"}, {_SigThrow,"sys: trap: invalid opcode"}, {_SigPanic,"sys: trap: fault read"}, {_SigPanic,"sys: trap: fault write"}, {_SigPanic,"sys: trap: divide error"}, {_SigPanic,"sys: fp:"}, {_SigPanic,"sys: trap:"}, {_SigNotify,"sys: write on closed pipe"}, {_SigThrow,"sys:"}, {_SigGoExit,"go: exit "}, {_SigKill,"kill"}, {_SigNotify+_SigKill,"interrupt"}, {_SigNotify+_SigKill,"hangup"}, {_SigNotify,"alarm"}, {_SigNotify+_SigThrow,"abort"} });

} // end runtime_package

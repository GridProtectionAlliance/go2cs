// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build plan9 || windows
// +build plan9 windows

// package @base -- go2cs converted at 2022 March 13 06:32:32 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\base\signal_notunix.go
namespace go.cmd.go.@internal;

using os = os_package;

public static partial class @base_package {

private static os.Signal signalsToIgnore = new slice<os.Signal>(new os.Signal[] { os.Interrupt });

// SignalTrace is the signal to send to make a Go program
// crash with a stack trace (no such signal in this case).
public static os.Signal SignalTrace = null;

} // end @base_package

// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package traceviewer provides definitions of the JSON data structures
// used by the Chrome trace viewer.
//
// The official description of the format is in this file:
// https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview
//
// Note: This can't be part of the parent traceviewer package as that would
// throw. go_bootstrap cannot depend on the cgo version of package net in ./make.bash.
namespace go.@internal.trace.traceviewer;

partial class format_package {

[GoType] partial struct Data {
    [GoTag(@"json:""traceEvents""")]
    public slice<ж<Event>> Events;
    [GoTag(@"json:""stackFrames""")]
    public map<@string, Frame> Frames;
    [GoTag(@"json:""displayTimeUnit""")]
    public @string TimeUnit;
}

[GoType] partial struct Event {
    [GoTag(@"json:""name,omitempty""")]
    public @string Name;
    [GoTag(@"json:""ph""")]
    public @string Phase;
    [GoTag(@"json:""s,omitempty""")]
    public @string Scope;
    [GoTag(@"json:""ts""")]
    public float64 Time;
    [GoTag(@"json:""dur,omitempty""")]
    public float64 Dur;
    [GoTag(@"json:""pid""")]
    public uint64 PID;
    [GoTag(@"json:""tid""")]
    public uint64 TID;
    [GoTag(@"json:""id,omitempty""")]
    public uint64 ID;
    [GoTag(@"json:""bp,omitempty""")]
    public @string BindPoint;
    [GoTag(@"json:""sf,omitempty""")]
    public nint Stack;
    [GoTag(@"json:""esf,omitempty""")]
    public nint EndStack;
    [GoTag(@"json:""args,omitempty""")]
    public any Arg;
    [GoTag(@"json:""cname,omitempty""")]
    public @string Cname;
    [GoTag(@"json:""cat,omitempty""")]
    public @string Category;
}

[GoType] partial struct Frame {
    [GoTag(@"json:""name""")]
    public @string Name;
    [GoTag(@"json:""parent,omitempty""")]
    public nint Parent;
}

[GoType] partial struct NameArg {
    [GoTag(@"json:""name""")]
    public @string Name;
}

[GoType] partial struct BlockedArg {
    [GoTag(@"json:""blocked""")]
    public @string Blocked;
}

[GoType] partial struct SortIndexArg {
    [GoTag(@"json:""sort_index""")]
    public nint Index;
}

[GoType] partial struct HeapCountersArg {
    public uint64 Allocated;
    public uint64 NextGC;
}

public static readonly UntypedInt ProcsSection = 0; // where Goroutines or per-P timelines are presented.
public static readonly UntypedInt StatsSection = 1; // where counters are presented.
public static readonly UntypedInt TasksSection = 2; // where Task hierarchy & timeline is presented.

[GoType] partial struct GoroutineCountersArg {
    public uint64 Running;
    public uint64 Runnable;
    public uint64 GCWaiting;
}

[GoType] partial struct ThreadCountersArg {
    public int64 Running;
    public int64 InSyscall;
}

[GoType] partial struct ThreadIDArg {
    public uint64 ThreadID;
}

} // end format_package

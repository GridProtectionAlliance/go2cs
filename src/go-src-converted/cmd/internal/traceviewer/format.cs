// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package traceviewer provides definitions of the JSON data structures
// used by the Chrome trace viewer.
//
// The official description of the format is in this file:
// https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview

// package traceviewer -- go2cs converted at 2022 March 13 06:30:10 UTC
// import "cmd/internal/traceviewer" ==> using traceviewer = go.cmd.@internal.traceviewer_package
// Original source: C:\Program Files\Go\src\cmd\internal\traceviewer\format.go
namespace go.cmd.@internal;

using System.ComponentModel;
public static partial class traceviewer_package {

public partial struct Data {
    [Description("json:\"traceEvents\"")]
    public slice<ptr<Event>> Events;
    [Description("json:\"stackFrames\"")]
    public map<@string, Frame> Frames;
    [Description("json:\"displayTimeUnit\"")]
    public @string TimeUnit;
}

public partial struct Event {
    [Description("json:\"name,omitempty\"")]
    public @string Name;
    [Description("json:\"ph\"")]
    public @string Phase;
    [Description("json:\"s,omitempty\"")]
    public @string Scope;
    [Description("json:\"ts\"")]
    public double Time;
    [Description("json:\"dur,omitempty\"")]
    public double Dur;
    [Description("json:\"pid\"")]
    public ulong PID;
    [Description("json:\"tid\"")]
    public ulong TID;
    [Description("json:\"id,omitempty\"")]
    public ulong ID;
    [Description("json:\"bp,omitempty\"")]
    public @string BindPoint;
    [Description("json:\"sf,omitempty\"")]
    public nint Stack;
    [Description("json:\"esf,omitempty\"")]
    public nint EndStack;
    [Description("json:\"cname,omitempty\"")]
    public @string Cname;
    [Description("json:\"cat,omitempty\"")]
    public @string Category;
}

public partial struct Frame {
    [Description("json:\"name\"")]
    public @string Name;
    [Description("json:\"parent,omitempty\"")]
    public nint Parent;
}

} // end traceviewer_package

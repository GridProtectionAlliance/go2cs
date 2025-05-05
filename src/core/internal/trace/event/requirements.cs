// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

partial class event_package {

// SchedReqs is a set of constraints on what the scheduling
// context must look like.
[GoType] partial struct SchedReqs {
    public Constraint Thread;
    public Constraint Proc;
    public Constraint Goroutine;
}

[GoType("num:uint8")] partial struct Constraint;

public static readonly Constraint MustNotHave = /* iota */ 0;
public static readonly Constraint MayHave = 1;
public static readonly Constraint MustHave = 2;

// UserGoReqs is a common requirement among events that are running
// or are close to running user code.
public static SchedReqs UserGoReqs = new SchedReqs(Thread: MustHave, Proc: MustHave, Goroutine: MustHave);

} // end event_package

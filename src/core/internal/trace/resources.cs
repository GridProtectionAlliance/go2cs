// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;

partial class trace_package {

[GoType("num:int64")] partial struct ThreadID;

// NoThread indicates that the relevant events don't correspond to any
// thread in particular.
public static readonly ThreadID NoThread = /* ThreadID(-1) */ -1;

[GoType("num:int64")] partial struct ProcID;

// NoProc indicates that the relevant events don't correspond to any
// P in particular.
public static readonly ProcID NoProc = /* ProcID(-1) */ -1;

[GoType("num:int64")] partial struct GoID;

// NoGoroutine indicates that the relevant events don't correspond to any
// goroutine in particular.
public static readonly GoID NoGoroutine = /* GoID(-1) */ -1;

[GoType("num:uint8")] partial struct GoState;

public static readonly GoState GoUndetermined = /* iota */ 0;  // No information is known about the goroutine.
public static readonly GoState GoNotExist = 1;      // Goroutine does not exist.
public static readonly GoState GoRunnable = 2;      // Goroutine is runnable but not running.
public static readonly GoState GoRunning = 3;       // Goroutine is running.
public static readonly GoState GoWaiting = 4;       // Goroutine is waiting on something to happen.
public static readonly GoState GoSyscall = 5;       // Goroutine is in a system call.

// Executing returns true if the state indicates that the goroutine is executing
// and bound to its thread.
public static bool Executing(this GoState s) {
    return s == GoRunning || s == GoSyscall;
}

// String returns a human-readable representation of a GoState.
//
// The format of the returned string is for debugging purposes and is subject to change.
public static @string String(this GoState s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == GoUndetermined) {
        return "Undetermined"u8;
    }
    if (exprᴛ1 == GoNotExist) {
        return "NotExist"u8;
    }
    if (exprᴛ1 == GoRunnable) {
        return "Runnable"u8;
    }
    if (exprᴛ1 == GoRunning) {
        return "Running"u8;
    }
    if (exprᴛ1 == GoWaiting) {
        return "Waiting"u8;
    }
    if (exprᴛ1 == GoSyscall) {
        return "Syscall"u8;
    }

    return "Bad"u8;
}

[GoType("num:uint8")] partial struct ProcState;

public static readonly ProcState ProcUndetermined = /* iota */ 0;  // No information is known about the proc.
public static readonly ProcState ProcNotExist = 1;      // Proc does not exist.
public static readonly ProcState ProcRunning = 2;       // Proc is running.
public static readonly ProcState ProcIdle = 3;          // Proc is idle.

// Executing returns true if the state indicates that the proc is executing
// and bound to its thread.
public static bool Executing(this ProcState s) {
    return s == ProcRunning;
}

// String returns a human-readable representation of a ProcState.
//
// The format of the returned string is for debugging purposes and is subject to change.
public static @string String(this ProcState s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == ProcUndetermined) {
        return "Undetermined"u8;
    }
    if (exprᴛ1 == ProcNotExist) {
        return "NotExist"u8;
    }
    if (exprᴛ1 == ProcRunning) {
        return "Running"u8;
    }
    if (exprᴛ1 == ProcIdle) {
        return "Idle"u8;
    }

    return "Bad"u8;
}

[GoType("num:uint8")] partial struct ResourceKind;

public static readonly ResourceKind ResourceNone = /* iota */ 0;       // No resource.
public static readonly ResourceKind ResourceGoroutine = 1;  // Goroutine.
public static readonly ResourceKind ResourceProc = 2;       // Proc.
public static readonly ResourceKind ResourceThread = 3;     // Thread.

// String returns a human-readable representation of a ResourceKind.
//
// The format of the returned string is for debugging purposes and is subject to change.
public static @string String(this ResourceKind r) {
    var exprᴛ1 = r;
    if (exprᴛ1 == ResourceNone) {
        return "None"u8;
    }
    if (exprᴛ1 == ResourceGoroutine) {
        return "Goroutine"u8;
    }
    if (exprᴛ1 == ResourceProc) {
        return "Proc"u8;
    }
    if (exprᴛ1 == ResourceThread) {
        return "Thread"u8;
    }

    return "Bad"u8;
}

// ResourceID represents a generic resource ID.
[GoType] partial struct ResourceID {
    // Kind is the kind of resource this ID is for.
    public ResourceKind Kind;
    internal int64 id;
}

// MakeResourceID creates a general resource ID from a specific resource's ID.
public static ResourceID MakeResourceID<T>(T id)
    where T : /* interface{internal/trace.GoID | internal/trace.ProcID | internal/trace.ThreadID} */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ResourceID rd = default!;
    any a = id;
    switch (a.type()) {
    case GoID : {
        rd.Kind = ResourceGoroutine;
        break;
    }
    case ProcID : {
        rd.Kind = ResourceProc;
        break;
    }
    case ThreadID : {
        rd.Kind = ResourceThread;
        break;
    }}

    rd.id = ((int64)id);
    return rd;
}

// Goroutine obtains a GoID from the resource ID.
//
// r.Kind must be ResourceGoroutine or this function will panic.
public static GoID Goroutine(this ResourceID r) {
    if (r.Kind != ResourceGoroutine) {
        throw panic(fmt.Sprintf("attempted to get GoID from %s resource ID"u8, r.Kind));
    }
    return ((GoID)r.id);
}

// Proc obtains a ProcID from the resource ID.
//
// r.Kind must be ResourceProc or this function will panic.
public static ProcID Proc(this ResourceID r) {
    if (r.Kind != ResourceProc) {
        throw panic(fmt.Sprintf("attempted to get ProcID from %s resource ID"u8, r.Kind));
    }
    return ((ProcID)r.id);
}

// Thread obtains a ThreadID from the resource ID.
//
// r.Kind must be ResourceThread or this function will panic.
public static ThreadID Thread(this ResourceID r) {
    if (r.Kind != ResourceThread) {
        throw panic(fmt.Sprintf("attempted to get ThreadID from %s resource ID"u8, r.Kind));
    }
    return ((ThreadID)r.id);
}

// String returns a human-readable string representation of the ResourceID.
//
// This representation is subject to change and is intended primarily for debugging.
public static @string String(this ResourceID r) {
    if (r.Kind == ResourceNone) {
        return r.Kind.String();
    }
    return fmt.Sprintf("%s(%d)"u8, r.Kind, r.id);
}

// StateTransition provides details about a StateTransition event.
[GoType] partial struct ΔStateTransition {
    // Resource is the resource this state transition is for.
    public ResourceID Resource;
    // Reason is a human-readable reason for the state transition.
    public @string Reason;
    // Stack is the stack trace of the resource making the state transition.
    //
    // This is distinct from the result (Event).Stack because it pertains to
    // the transitioning resource, not any of the ones executing the event
    // this StateTransition came from.
    //
    // An example of this difference is the NotExist -> Runnable transition for
    // goroutines, which indicates goroutine creation. In this particular case,
    // a Stack here would refer to the starting stack of the new goroutine, and
    // an (Event).Stack would refer to the stack trace of whoever created the
    // goroutine.
    public ΔStack Stack;
    // The actual transition data. Stored in a neutral form so that
    // we don't need fields for every kind of resource.
    internal int64 id;
    internal uint8 oldState;
    internal uint8 newState;
}

internal static ΔStateTransition goStateTransition(GoID id, GoState from, GoState to) {
    return new ΔStateTransition(
        Resource: new ResourceID(Kind: ResourceGoroutine, id: ((int64)id)),
        oldState: ((uint8)from),
        newState: ((uint8)to)
    );
}

internal static ΔStateTransition procStateTransition(ProcID id, ProcState from, ProcState to) {
    return new ΔStateTransition(
        Resource: new ResourceID(Kind: ResourceProc, id: ((int64)id)),
        oldState: ((uint8)from),
        newState: ((uint8)to)
    );
}

// Goroutine returns the state transition for a goroutine.
//
// Transitions to and from states that are Executing are special in that
// they change the future execution context. In other words, future events
// on the same thread will feature the same goroutine until it stops running.
//
// Panics if d.Resource.Kind is not ResourceGoroutine.
public static (GoState from, GoState to) Goroutine(this ΔStateTransition d) {
    GoState from = default!;
    GoState to = default!;

    if (d.Resource.Kind != ResourceGoroutine) {
        throw panic("Goroutine called on non-Goroutine state transition");
    }
    return (((GoState)d.oldState), ((GoState)d.newState));
}

// Proc returns the state transition for a proc.
//
// Transitions to and from states that are Executing are special in that
// they change the future execution context. In other words, future events
// on the same thread will feature the same goroutine until it stops running.
//
// Panics if d.Resource.Kind is not ResourceProc.
public static (ProcState from, ProcState to) Proc(this ΔStateTransition d) {
    ProcState from = default!;
    ProcState to = default!;

    if (d.Resource.Kind != ResourceProc) {
        throw panic("Proc called on non-Proc state transition");
    }
    return (((ProcState)d.oldState), ((ProcState)d.newState));
}

} // end trace_package

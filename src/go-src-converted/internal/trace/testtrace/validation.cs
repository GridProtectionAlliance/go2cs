// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using errors = errors_package;
using fmt = fmt_package;
using trace = @internal.trace_package;
using slices = slices_package;
using strings = strings_package;
using @internal;
using ꓸꓸꓸany = Span<any>;

partial class testtrace_package {

// Validator is a type used for validating a stream of trace.Events.
[GoType] partial struct Validator {
    internal @internal.trace_package.ΔTime lastTs;
    internal trace.GoID>*goState gs;
    internal trace.ProcID>*procState ps;
    internal trace.ThreadID>*schedContext ms;
    internal trace.ResourceID><>string ranges;
    internal trace.TaskID>string tasks;
    internal bool seenSync;
    public bool Go121;
}

[GoType] partial struct schedContext {
    public @internal.trace_package.ThreadID M;
    public @internal.trace_package.ProcID P;
    public @internal.trace_package.GoID G;
}

[GoType] partial struct goState {
    internal @internal.trace_package.GoState state;
    internal ж<schedContext> binding;
}

[GoType] partial struct procState {
    internal @internal.trace_package.ProcState state;
    internal ж<schedContext> binding;
}

// NewValidator creates a new Validator.
public static ж<Validator> NewValidator() {
    return Ꮡ(new Validator(
        gs: new trace.GoID>*goState(),
        ps: new trace.ProcID>*procState(),
        ms: new trace.ThreadID>*schedContext(),
        ranges: new trace.ResourceID><>string(),
        tasks: new trace.TaskID>string()
    ));
}

// Event validates ev as the next event in a stream of trace.Events.
//
// Returns an error if validation fails.
[GoRecv] public static error Event(this ref Validator v, traceꓸEvent ev) {
    var e = @new<errAccumulator>();
    // Validate timestamp order.
    if (v.lastTs != 0){
        if (ev.Time() <= v.lastTs){
            e.Errorf("timestamp out-of-order for %+v"u8, ev);
        } else {
            v.lastTs = ev.Time();
        }
    } else {
        v.lastTs = ev.Time();
    }
    // Validate event stack.
    checkStack(e, ev.Stack());
    var exprᴛ1 = ev.Kind();
    if (exprᴛ1 == trace.EventSync) {
        v.seenSync = true;
    }
    else if (exprᴛ1 == trace.EventMetric) {
        var m = ev.Metric();
        if (!strings.Contains(m.Name, // Just record that we've seen a Sync at some point.
 ":"u8)) {
            // Should have a ":" as per runtime/metrics convention.
            e.Errorf("invalid metric name %q"u8, m.Name);
        }
        if (m.Value.Kind() == trace.ValueBad) {
            // Make sure the value is OK.
            e.Errorf("invalid value"u8);
        }
        var exprᴛ2 = m.Value.Kind();
        if (exprᴛ2 == trace.ValueUint64) {
            _ = m.Value.Uint64();
        }

    }
    else if (exprᴛ1 == trace.EventLabel) {
        var l = ev.Label();
        if (l.Label == ""u8) {
            // Just make sure it doesn't panic.
            // Check label.
            e.Errorf("invalid label %q"u8, l.Label);
        }
        if (l.Resource.Kind == trace.ResourceNone) {
            // Check label resource.
            e.Errorf("label resource none"u8);
        }
        var exprᴛ3 = l.Resource.Kind;
        if (exprᴛ3 == trace.ResourceGoroutine) {
            var id = l.Resource.Goroutine();
            {
                var _ = v.gs[id];
                var ok = v.gs[id]; if (!ok) {
                    e.Errorf("label for invalid goroutine %d"u8, id);
                }
            }
        }
        else if (exprᴛ3 == trace.ResourceProc) {
            var id = l.Resource.Proc();
            {
                var _ = v.ps[id];
                var ok = v.ps[id]; if (!ok) {
                    e.Errorf("label for invalid proc %d"u8, id);
                }
            }
        }
        else if (exprᴛ3 == trace.ResourceThread) {
            var id = l.Resource.Thread();
            {
                var _ = v.ms[id];
                var ok = v.ms[id]; if (!ok) {
                    e.Errorf("label for invalid thread %d"u8, id);
                }
            }
        }

    }
    else if (exprᴛ1 == trace.EventStackSample) {
    }
    else if (exprᴛ1 == trace.EventStateTransition) {
        var tr = ev.StateTransition();
        checkStack(e, // Not much to check here. It's basically a sched context and a stack.
 // The sched context is also not guaranteed to align with other events.
 // We already checked the stack above.
 // Validate state transitions.
 //
 // TODO(mknyszek): A lot of logic is duplicated between goroutines and procs.
 // The two are intentionally handled identically; from the perspective of the
 // API, resources all have the same general properties. Consider making this
 // code generic over resources and implementing validation just once.
 tr.Stack);
        var exprᴛ4 = tr.Resource.Kind;
        if (exprᴛ4 == trace.ResourceGoroutine) {
            var id = tr.Resource.Goroutine();
            var (old, @new) = tr.Goroutine();
            if (@new == trace.GoUndetermined) {
                // Basic state transition validation.
                e.Errorf("transition to undetermined state for goroutine %d"u8, id);
            }
            if (v.seenSync && old == trace.GoUndetermined) {
                e.Errorf("undetermined goroutine %d after first global sync"u8, id);
            }
            if (@new == trace.GoNotExist && v.hasAnyRange(trace.MakeResourceID(id))) {
                e.Errorf("goroutine %d died with active ranges"u8, id);
            }
            var state = v.gs[id];
            var ok = v.gs[id];
            if (ok){
                if (old != (~state).state) {
                    e.Errorf("bad old state for goroutine %d: got %s, want %s"u8, id, old, (~state).state);
                }
                state.val.state = @new;
            } else {
                if (old != trace.GoUndetermined && old != trace.GoNotExist) {
                    e.Errorf("bad old state for unregistered goroutine %d: %s"u8, id, old);
                }
                state = Ꮡ(new goState(state: @new));
                v.gs[id] = state;
            }
            if (@new.Executing()){
                // Validate sched context.
                var ctx = v.getOrCreateThread(e, ev, ev.Thread());
                if (ctx != nil) {
                    if ((~ctx).G != trace.NoGoroutine && (~ctx).G != id) {
                        e.Errorf("tried to run goroutine %d when one was already executing (%d) on thread %d"u8, id, (~ctx).G, ev.Thread());
                    }
                    ctx.val.G = id;
                    state.val.binding = ctx;
                }
            } else 
            if (old.Executing() && !@new.Executing()) {
                if (tr.Stack != ev.Stack()) {
                    // This is a case where the transition is happening to a goroutine that is also executing, so
                    // these two stacks should always match.
                    e.Errorf("StateTransition.Stack doesn't match Event.Stack"u8);
                }
                var ctx = state.val.binding;
                if (ctx != nil){
                    if ((~ctx).G != id) {
                        e.Errorf("tried to stop goroutine %d when it wasn't currently executing (currently executing %d) on thread %d"u8, id, (~ctx).G, ev.Thread());
                    }
                    ctx.val.G = trace.NoGoroutine;
                    state.val.binding = default!;
                } else {
                    e.Errorf("stopping goroutine %d not bound to any active context"u8, id);
                }
            }
        }
        else if (exprᴛ4 == trace.ResourceProc) {
            var id = tr.Resource.Proc();
            var (old, @new) = tr.Proc();
            if (@new == trace.ProcUndetermined) {
                // Basic state transition validation.
                e.Errorf("transition to undetermined state for proc %d"u8, id);
            }
            if (v.seenSync && old == trace.ProcUndetermined) {
                e.Errorf("undetermined proc %d after first global sync"u8, id);
            }
            if (@new == trace.ProcNotExist && v.hasAnyRange(trace.MakeResourceID(id))) {
                e.Errorf("proc %d died with active ranges"u8, id);
            }
            var state = v.ps[id];
            var ok = v.ps[id];
            if (ok){
                if (old != (~state).state) {
                    e.Errorf("bad old state for proc %d: got %s, want %s"u8, id, old, (~state).state);
                }
                state.val.state = @new;
            } else {
                if (old != trace.ProcUndetermined && old != trace.ProcNotExist) {
                    e.Errorf("bad old state for unregistered proc %d: %s"u8, id, old);
                }
                state = Ꮡ(new procState(state: @new));
                v.ps[id] = state;
            }
            if (@new.Executing()){
                // Validate sched context.
                var ctx = v.getOrCreateThread(e, ev, ev.Thread());
                if (ctx != nil) {
                    if ((~ctx).P != trace.NoProc && (~ctx).P != id) {
                        e.Errorf("tried to run proc %d when one was already executing (%d) on thread %d"u8, id, (~ctx).P, ev.Thread());
                    }
                    ctx.val.P = id;
                    state.val.binding = ctx;
                }
            } else 
            if (old.Executing() && !@new.Executing()) {
                var ctx = state.val.binding;
                if (ctx != nil){
                    if ((~ctx).P != id) {
                        e.Errorf("tried to stop proc %d when it wasn't currently executing (currently executing %d) on thread %d"u8, id, (~ctx).P, (~ctx).M);
                    }
                    ctx.val.P = trace.NoProc;
                    state.val.binding = default!;
                } else {
                    e.Errorf("stopping proc %d not bound to any active context"u8, id);
                }
            }
        }

    }
    else if (exprᴛ1 == trace.EventRangeBegin || exprᴛ1 == trace.EventRangeActive || exprᴛ1 == trace.EventRangeEnd) {
        var r = ev.Range();
        var exprᴛ5 = ev.Kind();
        if (exprᴛ5 == trace.EventRangeBegin) {
            if (v.hasRange(r.Scope, // Validate ranges.
 r.Name)) {
                e.Errorf("already active range %q on %v begun again"u8, r.Name, r.Scope);
            }
            v.addRange(r.Scope, r.Name);
        }
        else if (exprᴛ5 == trace.EventRangeActive) {
            if (!v.hasRange(r.Scope, r.Name)) {
                v.addRange(r.Scope, r.Name);
            }
        }
        else if (exprᴛ5 == trace.EventRangeEnd) {
            if (!v.hasRange(r.Scope, r.Name)) {
                e.Errorf("inactive range %q on %v ended"u8, r.Name, r.Scope);
            }
            v.deleteRange(r.Scope, r.Name);
        }

    }
    else if (exprᴛ1 == trace.EventTaskBegin) {
        var t = ev.Task();
        if (t.ID == trace.NoTask || t.ID == trace.BackgroundTask) {
            // Validate task begin.
            // The background task should never have an event emitted for it.
            e.Errorf("found invalid task ID for task of type %s"u8, t.Type);
        }
        if (t.Parent == trace.BackgroundTask) {
            // It's not possible for a task to be a subtask of the background task.
            e.Errorf("found background task as the parent for task of type %s"u8, t.Type);
        }
        v.tasks[t.ID] = t.Type;
    }
    else if (exprᴛ1 == trace.EventTaskEnd) {
        var t = ev.Task();
        {
            @string typ = v.tasks[t.ID];
            var ok = v.tasks[t.ID]; if (ok) {
                // N.B. Don't check the task type. Empty string is a valid task type.
                // Validate task end.
                // We can see a task end without a begin, so ignore a task without information.
                // Instead, if we've seen the task begin, just make sure the task end lines up.
                if (t.Type != typ) {
                    e.Errorf("task end type %q doesn't match task start type %q for task %d"u8, t.Type, typ, t.ID);
                }
                delete(v.tasks, t.ID);
            }
        }
    }
    else if (exprᴛ1 == trace.EventLog) {
        _ = ev.Log();
    }

    // There's really not much here to check, except that we can
    // generate a Log. The category and message are entirely user-created,
    // so we can't make any assumptions as to what they are. We also
    // can't validate the task, because proving the task's existence is very
    // much best-effort.
    return e.Errors();
}

[GoRecv] internal static bool hasRange(this ref Validator v, trace.ResourceID r, @string name) {
    var ranges = v.ranges[r];
    var ok = v.ranges[r];
    return ok && slices.Contains(ranges, name);
}

[GoRecv] internal static void addRange(this ref Validator v, trace.ResourceID r, @string name) {
    var ranges = v.ranges[r];
    var _ = v.ranges[r];
    ranges = append(ranges, name);
    v.ranges[r] = ranges;
}

[GoRecv] internal static bool hasAnyRange(this ref Validator v, trace.ResourceID r) {
    var ranges = v.ranges[r];
    var ok = v.ranges[r];
    return ok && len(ranges) != 0;
}

[GoRecv] internal static void deleteRange(this ref Validator v, trace.ResourceID r, @string name) {
    var ranges = v.ranges[r];
    var ok = v.ranges[r];
    if (!ok) {
        return;
    }
    nint i = slices.Index(ranges, name);
    if (i < 0) {
        return;
    }
    v.ranges[r] = slices.Delete(ranges, i, i + 1);
}

[GoRecv] public static ж<schedContext> getOrCreateThread(this ref Validator v, ж<errAccumulator> Ꮡe, traceꓸEvent ev, trace.ThreadID m) {
    ref var e = ref Ꮡe.val;

    var lenient = 
    var evʗ1 = ev;
    () => {
        // Be lenient about GoUndetermined -> GoSyscall transitions if they
        // originate from an old trace. These transitions lack thread
        // information in trace formats older than 1.22.
        if (!v.Go121) {
            return false;
        }
        if (evʗ1.Kind() != trace.EventStateTransition) {
            return false;
        }
        ref var tr = ref heap<@internal.trace_package.ΔStateTransition>(out var Ꮡtr);
        tr = evʗ1.StateTransition();
        if (tr.Resource.Kind != trace.ResourceGoroutine) {
            return false;
        }
        var (from, to) = tr.Goroutine();
        return from == trace.GoUndetermined && to == trace.GoSyscall;
    };
    if (m == trace.NoThread && !lenient()) {
        e.Errorf("must have thread, but thread ID is none"u8);
        return default!;
    }
    var s = v.ms[m];
    var ok = v.ms[m];
    if (!ok) {
        s = Ꮡ(new schedContext(M: m, P: trace.NoProc, G: trace.NoGoroutine));
        v.ms[m] = s;
        return s;
    }
    return s;
}

internal static void checkStack(ж<errAccumulator> Ꮡe, traceꓸStack stk) {
    ref var e = ref Ꮡe.val;

    // Check for non-empty values, but we also check for crashes due to incorrect validation.
    nint i = 0;
    stk.Frames((trace.StackFrame f) => {
        if (i == 0) {
            return true;
        }
        if (f.Func == ""u8 || f.File == ""u8 || f.PC == 0 || f.Line == 0) {
            e.Errorf("invalid stack frame %#v: missing information"u8, f);
        }
        i++;
        return true;
    });
}

[GoType] partial struct errAccumulator {
    internal slice<error> errs;
}

[GoRecv] internal static void Errorf(this ref errAccumulator e, @string f, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.slice();

    e.errs = append(e.errs, fmt.Errorf(f, args.ꓸꓸꓸ));
}

[GoRecv] internal static error Errors(this ref errAccumulator e) {
    return errors.Join(e.errs.ꓸꓸꓸ);
}

} // end testtrace_package

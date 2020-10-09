// package trace -- go2cs converted at 2020 October 09 04:59:05 UTC
// import "runtime/trace" ==> using trace = go.runtime.trace_package
// Original source: C:\Go\src\runtime\trace\annotation.go
using context = go.context_package;
using fmt = go.fmt_package;
using atomic = go.sync.atomic_package;
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace runtime
{
    public static partial class trace_package
    {
        private partial struct traceContextKey
        {
        }

        // NewTask creates a task instance with the type taskType and returns
        // it along with a Context that carries the task.
        // If the input context contains a task, the new task is its subtask.
        //
        // The taskType is used to classify task instances. Analysis tools
        // like the Go execution tracer may assume there are only a bounded
        // number of unique task types in the system.
        //
        // The returned end function is used to mark the task's end.
        // The trace tool measures task latency as the time between task creation
        // and when the end function is called, and provides the latency
        // distribution per task type.
        // If the end function is called multiple times, only the first
        // call is used in the latency measurement.
        //
        //   ctx, task := trace.NewTask(ctx, "awesomeTask")
        //   trace.WithRegion(ctx, "preparation", prepWork)
        //   // preparation of the task
        //   go func() {  // continue processing the task in a separate goroutine.
        //       defer task.End()
        //       trace.WithRegion(ctx, "remainingWork", remainingWork)
        //   }()
        public static (context.Context, ptr<Task>) NewTask(context.Context pctx, @string taskType)
        {
            context.Context ctx = default;
            ptr<Task> task = default!;

            var pid = fromContext(pctx).id;
            var id = newID();
            userTaskCreate(id, pid, taskType);
            ptr<Task> s = addr(new Task(id:id));
            return (context.WithValue(pctx, new traceContextKey(), s), _addr_s!); 

            // We allocate a new task and the end function even when
            // the tracing is disabled because the context and the detach
            // function can be used across trace enable/disable boundaries,
            // which complicates the problem.
            //
            // For example, consider the following scenario:
            //   - trace is enabled.
            //   - trace.WithRegion is called, so a new context ctx
            //     with a new region is created.
            //   - trace is disabled.
            //   - trace is enabled again.
            //   - trace APIs with the ctx is called. Is the ID in the task
            //   a valid one to use?
            //
            // TODO(hyangah): reduce the overhead at least when
            // tracing is disabled. Maybe the id can embed a tracing
            // round number and ignore ids generated from previous
            // tracing round.
        }

        private static ptr<Task> fromContext(context.Context ctx)
        {
            {
                ptr<Task> (s, ok) = ctx.Value(new traceContextKey())._<ptr<Task>>();

                if (ok)
                {
                    return _addr_s!;
                }

            }

            return _addr__addr_bgTask!;

        }

        // Task is a data type for tracing a user-defined, logical operation.
        public partial struct Task
        {
            public ulong id; // TODO(hyangah): record parent id?
        }

        // End marks the end of the operation represented by the Task.
        private static void End(this ptr<Task> _addr_t)
        {
            ref Task t = ref _addr_t.val;

            userTaskEnd(t.id);
        }

        private static ulong lastTaskID = 0L; // task id issued last time

        private static ulong newID()
        { 
            // TODO(hyangah): use per-P cache
            return atomic.AddUint64(_addr_lastTaskID, 1L);

        }

        private static Task bgTask = new Task(id:uint64(0));

        // Log emits a one-off event with the given category and message.
        // Category can be empty and the API assumes there are only a handful of
        // unique categories in the system.
        public static void Log(context.Context ctx, @string category, @string message)
        {
            var id = fromContext(ctx).id;
            userLog(id, category, message);
        }

        // Logf is like Log, but the value is formatted using the specified format spec.
        public static void Logf(context.Context ctx, @string category, @string format, params object[] args)
        {
            args = args.Clone();

            if (IsEnabled())
            { 
                // Ideally this should be just Log, but that will
                // add one more frame in the stack trace.
                var id = fromContext(ctx).id;
                userLog(id, category, fmt.Sprintf(format, args));

            }

        }

        private static readonly var regionStartCode = uint64(0L);
        private static readonly var regionEndCode = uint64(1L);


        // WithRegion starts a region associated with its calling goroutine, runs fn,
        // and then ends the region. If the context carries a task, the region is
        // associated with the task. Otherwise, the region is attached to the background
        // task.
        //
        // The regionType is used to classify regions, so there should be only a
        // handful of unique region types.
        public static void WithRegion(context.Context ctx, @string regionType, Action fn) => func((defer, _, __) =>
        { 
            // NOTE:
            // WithRegion helps avoiding misuse of the API but in practice,
            // this is very restrictive:
            // - Use of WithRegion makes the stack traces captured from
            //   region start and end are identical.
            // - Refactoring the existing code to use WithRegion is sometimes
            //   hard and makes the code less readable.
            //     e.g. code block nested deep in the loop with various
            //          exit point with return values
            // - Refactoring the code to use this API with closure can
            //   cause different GC behavior such as retaining some parameters
            //   longer.
            // This causes more churns in code than I hoped, and sometimes
            // makes the code less readable.

            var id = fromContext(ctx).id;
            userRegion(id, regionStartCode, regionType);
            defer(userRegion(id, regionEndCode, regionType));
            fn();

        });

        // StartRegion starts a region and returns a function for marking the
        // end of the region. The returned Region's End function must be called
        // from the same goroutine where the region was started.
        // Within each goroutine, regions must nest. That is, regions started
        // after this region must be ended before this region can be ended.
        // Recommended usage is
        //
        //     defer trace.StartRegion(ctx, "myTracedRegion").End()
        //
        public static ptr<Region> StartRegion(context.Context ctx, @string regionType)
        {
            if (!IsEnabled())
            {
                return _addr_noopRegion!;
            }

            var id = fromContext(ctx).id;
            userRegion(id, regionStartCode, regionType);
            return addr(new Region(id,regionType));

        }

        // Region is a region of code whose execution time interval is traced.
        public partial struct Region
        {
            public ulong id;
            public @string regionType;
        }

        private static ptr<Region> noopRegion = addr(new Region());

        // End marks the end of the traced code region.
        private static void End(this ptr<Region> _addr_r)
        {
            ref Region r = ref _addr_r.val;

            if (r == noopRegion)
            {
                return ;
            }

            userRegion(r.id, regionEndCode, r.regionType);

        }

        // IsEnabled reports whether tracing is enabled.
        // The information is advisory only. The tracing status
        // may have changed by the time this function returns.
        public static bool IsEnabled()
        {
            var enabled = atomic.LoadInt32(_addr_tracing.enabled);
            return enabled == 1L;
        }

        //
        // Function bodies are defined in runtime/trace.go
        //

        // emits UserTaskCreate event.
        private static void userTaskCreate(ulong id, ulong parentID, @string taskType)
;

        // emits UserTaskEnd event.
        private static void userTaskEnd(ulong id)
;

        // emits UserRegion event.
        private static void userRegion(ulong id, ulong mode, @string regionType)
;

        // emits UserLog event.
        private static void userLog(ulong id, @string category, @string message)
;
    }
}}

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 October 08 03:26:08 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\label.go
using context = go.context_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go {
namespace runtime
{
    public static partial class pprof_package
    {
        private partial struct label
        {
            public @string key;
            public @string value;
        }

        // LabelSet is a set of labels.
        public partial struct LabelSet
        {
            public slice<label> list;
        }

        // labelContextKey is the type of contextKeys used for profiler labels.
        private partial struct labelContextKey
        {
        }

        private static labelMap labelValue(context.Context ctx)
        {
            ptr<labelMap> (labels, _) = ctx.Value(new labelContextKey())._<ptr<labelMap>>();
            if (labels == null)
            {
                return labelMap(null);
            }

            return labels.val;

        }

        // labelMap is the representation of the label set held in the context type.
        // This is an initial implementation, but it will be replaced with something
        // that admits incremental immutable modification more efficiently.
        private partial struct labelMap // : map<@string, @string>
        {
        }

        // String statisfies Stringer and returns key, value pairs in a consistent
        // order.
        private static @string String(this ptr<labelMap> _addr_l)
        {
            ref labelMap l = ref _addr_l.val;

            if (l == null)
            {
                return "";
            }

            var keyVals = make_slice<@string>(0L, len(l.val));

            foreach (var (k, v) in l.val)
            {
                keyVals = append(keyVals, fmt.Sprintf("%q:%q", k, v));
            }
            sort.Strings(keyVals);

            return "{" + strings.Join(keyVals, ", ") + "}";

        }

        // WithLabels returns a new context.Context with the given labels added.
        // A label overwrites a prior label with the same key.
        public static context.Context WithLabels(context.Context ctx, LabelSet labels)
        {
            ref var childLabels = ref heap(make(labelMap), out ptr<var> _addr_childLabels);
            var parentLabels = labelValue(ctx); 
            // TODO(matloob): replace the map implementation with something
            // more efficient so creating a child context WithLabels doesn't need
            // to clone the map.
            foreach (var (k, v) in parentLabels)
            {
                childLabels[k] = v;
            }
            foreach (var (_, label) in labels.list)
            {
                childLabels[label.key] = label.value;
            }
            return context.WithValue(ctx, new labelContextKey(), _addr_childLabels);

        }

        // Labels takes an even number of strings representing key-value pairs
        // and makes a LabelSet containing them.
        // A label overwrites a prior label with the same key.
        // Currently only the CPU and goroutine profiles utilize any labels
        // information.
        // See https://golang.org/issue/23458 for details.
        public static LabelSet Labels(params @string[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            if (len(args) % 2L != 0L)
            {
                panic("uneven number of arguments to pprof.Labels");
            }

            var list = make_slice<label>(0L, len(args) / 2L);
            {
                long i = 0L;

                while (i + 1L < len(args))
                {
                    list = append(list, new label(key:args[i],value:args[i+1]));
                    i += 2L;
                }

            }
            return new LabelSet(list:list);

        });

        // Label returns the value of the label with the given key on ctx, and a boolean indicating
        // whether that label exists.
        public static (@string, bool) Label(context.Context ctx, @string key)
        {
            @string _p0 = default;
            bool _p0 = default;

            var ctxLabels = labelValue(ctx);
            var (v, ok) = ctxLabels[key];
            return (v, ok);
        }

        // ForLabels invokes f with each label set on the context.
        // The function f should return true to continue iteration or false to stop iteration early.
        public static bool ForLabels(context.Context ctx, Func<@string, @string, bool> f)
        {
            var ctxLabels = labelValue(ctx);
            foreach (var (k, v) in ctxLabels)
            {
                if (!f(k, v))
                {
                    break;
                }

            }

        }
    }
}}

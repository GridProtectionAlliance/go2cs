// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2020 August 29 08:22:43 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Go\src\runtime\pprof\label.go
using context = go.context_package;
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
            ref labelMap (labels, _) = ctx.Value(new labelContextKey())._<ref labelMap>();
            if (labels == null)
            {
                return labelMap(null);
            }
            return labels.Value;
        }

        // labelMap is the representation of the label set held in the context type.
        // This is an initial implementation, but it will be replaced with something
        // that admits incremental immutable modification more efficiently.
        private partial struct labelMap // : map<@string, @string>
        {
        }

        // WithLabels returns a new context.Context with the given labels added.
        // A label overwrites a prior label with the same key.
        public static context.Context WithLabels(context.Context ctx, LabelSet labels)
        {
            var childLabels = make(labelMap);
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
            return context.WithValue(ctx, new labelContextKey(), ref childLabels);
        }

        // Labels takes an even number of strings representing key-value pairs
        // and makes a LabelSet containing them.
        // A label overwrites a prior label with the same key.
        public static LabelSet Labels(params @string[] args) => func((_, panic, __) =>
        {
            args = args.Clone();

            if (len(args) % 2L != 0L)
            {
                panic("uneven number of arguments to pprof.Labels");
            }
            LabelSet labels = new LabelSet();
            {
                long i = 0L;

                while (i + 1L < len(args))
                {
                    labels.list = append(labels.list, new label(key:args[i],value:args[i+1]));
                    i += 2L;
                }

            }
            return labels;
        });

        // Label returns the value of the label with the given key on ctx, and a boolean indicating
        // whether that label exists.
        public static (@string, bool) Label(context.Context ctx, @string key)
        {
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

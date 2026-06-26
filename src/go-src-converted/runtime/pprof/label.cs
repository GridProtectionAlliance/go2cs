// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using context = context_package;
using fmt = fmt_package;
using slices = slices_package;
using strings = strings_package;
using ꓸꓸꓸ@string = Span<@string>;

partial class pprof_package {

[GoType] partial struct label {
    internal @string key;
    internal @string value;
}

// LabelSet is a set of labels.
[GoType] partial struct LabelSet {
    internal slice<label> list;
}

// labelContextKey is the type of contextKeys used for profiler labels.
[GoType] partial struct labelContextKey {
}

internal static labelMap labelValue(context.Context ctx) {
    var (labels, _) = ctx.Value(new labelContextKey(nil))._<labelMap.val>(ᐧ);
    if (labels == nil) {
        return ((labelMap)default!);
    }
    return labels.val;
}
/* visitMapType: map[string]string */

// String satisfies Stringer and returns key, value pairs in a consistent
// order.
[GoRecv] internal static @string String(this ref labelMap l) {
    if (l == nil) {
        return ""u8;
    }
    var keyVals = new slice<@string>(0, len(l));
    foreach (var (k, v) in l) {
        keyVals = append(keyVals, fmt.Sprintf("%q:%q"u8, k, v));
    }
    slices.Sort(keyVals);
    return "{"u8 + strings.Join(keyVals, ", "u8) + "}"u8;
}

// WithLabels returns a new [context.Context] with the given labels added.
// A label overwrites a prior label with the same key.
public static context.Context WithLabels(context.Context ctx, LabelSet labels) {
    var parentLabels = labelValue(ctx);
    var childLabels = new labelMap(len(parentLabels));
    // TODO(matloob): replace the map implementation with something
    // more efficient so creating a child context WithLabels doesn't need
    // to clone the map.
    foreach (var (k, v) in parentLabels) {
        childLabels[k] = v;
    }
    foreach (var (_, label) in labels.list) {
        childLabels[label.key] = label.value;
    }
    return context.WithValue(ctx, new labelContextKey(nil), Ꮡ(childLabels));
}

// Labels takes an even number of strings representing key-value pairs
// and makes a [LabelSet] containing them.
// A label overwrites a prior label with the same key.
// Currently only the CPU and goroutine profiles utilize any labels
// information.
// See https://golang.org/issue/23458 for details.
public static LabelSet Labels(params ꓸꓸꓸ@string argsʗp) {
    var args = argsʗp.slice();

    if (len(args) % 2 != 0) {
        throw panic("uneven number of arguments to pprof.Labels");
    }
    var list = new slice<label>(0, len(args) / 2);
    for (nint i = 0; i + 1 < len(args); i += 2) {
        list = append(list, new label(key: args[i], value: args[i + 1]));
    }
    return new LabelSet(list: list);
}

// Label returns the value of the label with the given key on ctx, and a boolean indicating
// whether that label exists.
public static (@string, bool) Label(context.Context ctx, @string key) {
    var ctxLabels = labelValue(ctx);
    @string v = ctxLabels[key];
    var ok = ctxLabels[key];
    return (v, ok);
}

// ForLabels invokes f with each label set on the context.
// The function f should return true to continue iteration or false to stop iteration early.
public static void ForLabels(context.Context ctx, Func<@string, @string, bool> f) {
    var ctxLabels = labelValue(ctx);
    foreach (var (k, v) in ctxLabels) {
        if (!f(k, v)) {
            break;
        }
    }
}

} // end pprof_package

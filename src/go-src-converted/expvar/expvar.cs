// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package expvar provides a standardized interface to public variables, such
// as operation counters in servers. It exposes these variables via HTTP at
// /debug/vars in JSON format. As of Go 1.22, the /debug/vars request must
// use GET.
//
// Operations to set or modify these public variables are atomic.
//
// In addition to adding the HTTP handler, this package registers the
// following variables:
//
//	cmdline   os.Args
//	memstats  runtime.Memstats
//
// The package is sometimes only imported for the side effect of
// registering its HTTP handler and the above variables. To use it
// this way, link this package into your program:
//
//	import _ "expvar"
namespace go;

using json = encoding.json_package;
using godebug = @internal.godebug_package;
using log = log_package;
using math = math_package;
using http = net.http_package;
using os = os_package;
using runtime = runtime_package;
using slices = slices_package;
using strconv = strconv_package;
using sync = sync_package;
using atomic = sync.atomic_package;
using utf8 = unicode.utf8_package;
using @internal;
using encoding;
using net;
using sync;
using unicode;

partial class expvar_package {

// Var is an abstract type for all exported variables.
[GoType] partial interface Var {
    // String returns a valid JSON value for the variable.
    // Types with String methods that do not return valid JSON
    // (such as time.Time) must not be used as a Var.
    @string String();
}

[GoType] partial interface jsonVar {
    // appendJSON appends the JSON representation of the receiver to b.
    slice<byte> appendJSON(slice<byte> b);
}

// Int is a 64-bit integer variable that satisfies the [Var] interface.
[GoType] partial struct Int {
    internal sync.atomic_package.Int64 i;
}

[GoRecv] public static int64 Value(this ref Int v) {
    return v.i.Load();
}

[GoRecv] public static @string String(this ref Int v) {
    return ((@string)v.appendJSON(default!));
}

[GoRecv] internal static slice<byte> appendJSON(this ref Int v, slice<byte> b) {
    return strconv.AppendInt(b, v.i.Load(), 10);
}

[GoRecv] public static void Add(this ref Int v, int64 delta) {
    v.i.Add(delta);
}

[GoRecv] public static void Set(this ref Int v, int64 value) {
    v.i.Store(value);
}

// Float is a 64-bit float variable that satisfies the [Var] interface.
[GoType] partial struct Float {
    internal sync.atomic_package.Uint64 f;
}

[GoRecv] public static float64 Value(this ref Float v) {
    return math.Float64frombits(v.f.Load());
}

[GoRecv] public static @string String(this ref Float v) {
    return ((@string)v.appendJSON(default!));
}

[GoRecv] internal static slice<byte> appendJSON(this ref Float v, slice<byte> b) {
    return strconv.AppendFloat(b, math.Float64frombits(v.f.Load()), (rune)'g', -1, 64);
}

// Add adds delta to v.
[GoRecv] public static void Add(this ref Float v, float64 delta) {
    while (ᐧ) {
        var cur = v.f.Load();
        var curVal = math.Float64frombits(cur);
        var nxtVal = curVal + delta;
        var nxt = math.Float64bits(nxtVal);
        if (v.f.CompareAndSwap(cur, nxt)) {
            return;
        }
    }
}

// Set sets v to value.
[GoRecv] public static void Set(this ref Float v, float64 value) {
    v.f.Store(math.Float64bits(value));
}

// Map is a string-to-Var map variable that satisfies the [Var] interface.
[GoType] partial struct Map {
    internal sync_package.Map m; // map[string]Var
    internal sync_package.RWMutex keysMu;
    internal slice<@string> keys; // sorted
}

// KeyValue represents a single entry in a [Map].
[GoType] partial struct KeyValue {
    public @string Key;
    public Var Value;
}

[GoRecv] public static @string String(this ref Map v) {
    return ((@string)v.appendJSON(default!));
}

[GoRecv] internal static slice<byte> appendJSON(this ref Map v, slice<byte> b) {
    return v.appendJSONMayExpand(b, false);
}

[GoRecv] internal static slice<byte> appendJSONMayExpand(this ref Map v, slice<byte> b, bool expand) {
    var afterCommaDelim = ((byte)(rune)' ');
    var mayAppendNewline = (slice<byte> b) => bΔ1;
    if (expand) {
        afterCommaDelim = (rune)'\n';
        mayAppendNewline = (slice<byte> b) => append(bΔ2, (rune)'\n');
    }
    b = append(b, (rune)'{');
    b = mayAppendNewline(b);
    var first = true;
    v.Do(
    var bʗ2 = b;
    (KeyValue kv) => {
        if (!first) {
            bʗ2 = append(bʗ2, (rune)',', afterCommaDelim);
        }
        first = false;
        bʗ2 = appendJSONQuote(bʗ2, kv.Key);
        bʗ2 = append(bʗ2, (rune)':', (rune)' ');
        switch (kv.Value.type()) {
        case default! v: {
            bʗ2 = append(bʗ2, "null"u8.ꓸꓸꓸ);
            break;
        }
        case jsonVar v: {
            bʗ2 = v.appendJSON(bʗ2);
            break;
        }
        default: {
            var v = kv.Value.type();
            bʗ2 = append(bʗ2, v.String().ꓸꓸꓸ);
            break;
        }}
    });
    b = mayAppendNewline(b);
    b = append(b, (rune)'}');
    b = mayAppendNewline(b);
    return b;
}

// Init removes all keys from the map.
[GoRecv("capture")] public static ж<Map> Init(this ref Map v) => func((defer, _) => {
    v.keysMu.Lock();
    defer(v.keysMu.Unlock);
    v.keys = v.keys[..0];
    v.m.Clear();
    return InitꓸᏑv;
});

// addKey updates the sorted list of keys in v.keys.
[GoRecv] internal static void addKey(this ref Map v, @string key) => func((defer, _) => {
    v.keysMu.Lock();
    defer(v.keysMu.Unlock);
    // Using insertion sort to place key into the already-sorted v.keys.
    var (i, found) = slices.BinarySearch(v.keys, key);
    if (found) {
        return;
    }
    v.keys = slices.Insert(v.keys, i, key);
});

[GoRecv] public static Var Get(this ref Map v, @string key) {
    var (i, _) = v.m.Load(key);
    var (av, _) = i._<Var>(ᐧ);
    return av;
}

[GoRecv] public static void Set(this ref Map v, @string key, Var av) {
    // Before we store the value, check to see whether the key is new. Try a Load
    // before LoadOrStore: LoadOrStore causes the key interface to escape even on
    // the Load path.
    {
        var (_, ok) = v.m.Load(key); if (!ok) {
            {
                var (_, dup) = v.m.LoadOrStore(key, av); if (!dup) {
                    v.addKey(key);
                    return;
                }
            }
        }
    }
    v.m.Store(key, av);
}

// Add adds delta to the *[Int] value stored under the given map key.
[GoRecv] public static void Add(this ref Map v, @string key, int64 delta) {
    var (i, ok) = v.m.Load(key);
    if (!ok) {
        bool dup = default!;
        (i, dup) = v.m.LoadOrStore(key, @new<Int>());
        if (!dup) {
            v.addKey(key);
        }
    }
    // Add to Int; ignore otherwise.
    {
        var (iv, okΔ1) = i._<Int.val>(ᐧ); if (okΔ1) {
            iv.Add(delta);
        }
    }
}

// AddFloat adds delta to the *[Float] value stored under the given map key.
[GoRecv] public static void AddFloat(this ref Map v, @string key, float64 delta) {
    var (i, ok) = v.m.Load(key);
    if (!ok) {
        bool dup = default!;
        (i, dup) = v.m.LoadOrStore(key, @new<Float>());
        if (!dup) {
            v.addKey(key);
        }
    }
    // Add to Float; ignore otherwise.
    {
        var (iv, okΔ1) = i._<Float.val>(ᐧ); if (okΔ1) {
            iv.Add(delta);
        }
    }
}

// Delete deletes the given key from the map.
[GoRecv] public static void Delete(this ref Map v, @string key) => func((defer, _) => {
    v.keysMu.Lock();
    defer(v.keysMu.Unlock);
    var (i, found) = slices.BinarySearch(v.keys, key);
    if (found) {
        v.keys = slices.Delete(v.keys, i, i + 1);
        v.m.Delete(key);
    }
});

// Do calls f for each entry in the map.
// The map is locked during the iteration,
// but existing entries may be concurrently updated.
[GoRecv] public static void Do(this ref Map v, Action<KeyValue> f) => func((defer, _) => {
    v.keysMu.RLock();
    defer(v.keysMu.RUnlock);
    foreach (var (_, k) in v.keys) {
        var (i, _) = v.m.Load(k);
        var (val, _) = i._<Var>(ᐧ);
        f(new KeyValue(k, val));
    }
});

// String is a string variable, and satisfies the [Var] interface.
[GoType] partial struct ΔString {
    internal sync.atomic_package.Value s; // string
}

[GoRecv] public static @string Value(this ref ΔString v) {
    var (p, _) = v.s.Load()._<@string>(ᐧ);
    return p;
}

// String implements the [Var] interface. To get the unquoted string
// use [String.Value].
[GoRecv] public static @string String(this ref ΔString v) {
    return ((@string)v.appendJSON(default!));
}

[GoRecv] internal static slice<byte> appendJSON(this ref ΔString v, slice<byte> b) {
    return appendJSONQuote(b, v.Value());
}

[GoRecv] public static void Set(this ref ΔString v, @string value) {
    v.s.Store(value);
}

public delegate any Func();

public static any Value(this Func f) {
    return f();
}

public static @string String(this Func f) {
    (v, _) = json.Marshal(f());
    return ((@string)v);
}

// All published variables.
internal static Map vars;

// Publish declares a named exported variable. This should be called from a
// package's init function when it creates its Vars. If the name is already
// registered then this will log.Panic.
public static void Publish(@string name, Var v) => func((defer, _) => {
    {
        var (_, dup) = vars.m.LoadOrStore(name, v); if (dup) {
            log.Panicln("Reuse of exported var name:", name);
        }
    }
    vars.keysMu.Lock();
    var varsʗ1 = vars;
    defer(varsʗ1.keysMu.Unlock);
    vars.keys = append(vars.keys, name);
    slices.Sort(vars.keys);
});

// Get retrieves a named exported variable. It returns nil if the name has
// not been registered.
public static Var Get(@string name) {
    return vars.Get(name);
}

// Convenience functions for creating new exported variables.
public static ж<Int> NewInt(@string name) {
    var v = @new<Int>();
    Publish(name, ~v);
    return v;
}

public static ж<Float> NewFloat(@string name) {
    var v = @new<Float>();
    Publish(name, ~v);
    return v;
}

public static ж<Map> NewMap(@string name) {
    var v = @new<Map>().Init();
    Publish(name, ~v);
    return v;
}

public static ж<ΔString> NewString(@string name) {
    var v = @new<ΔString>();
    Publish(name, ~v);
    return v;
}

// Do calls f for each exported variable.
// The global variable map is locked during the iteration,
// but existing entries may be concurrently updated.
public static void Do(Action<KeyValue> f) {
    vars.Do(f);
}

internal static void expvarHandler(http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var r = ref Ꮡr.val;

    w.Header().Set("Content-Type"u8, "application/json; charset=utf-8"u8);
    w.Write(vars.appendJSONMayExpand(default!, true));
}

// Handler returns the expvar HTTP Handler.
//
// This is only needed to install the handler in a non-standard location.
public static httpꓸHandler Handler() {
    return ((http.HandlerFunc)expvarHandler);
}

internal static any cmdline() {
    return os.Args;
}

internal static any memstats() {
    var stats = @new<runtime.MemStats>();
    runtime.ReadMemStats(stats);
    return stats.val;
}

[GoInit] internal static void init() {
    if (godebug.New("httpmuxgo121"u8).Value() == "1"u8){
        http.HandleFunc("/debug/vars"u8, expvarHandler);
    } else {
        http.HandleFunc("GET /debug/vars"u8, expvarHandler);
    }
    Publish("cmdline"u8, ((Func)cmdline));
    Publish("memstats"u8, ((Func)memstats));
}

// TODO: Use json.appendString instead.
internal static slice<byte> appendJSONQuote(slice<byte> b, @string s) {
    @string hex = "0123456789abcdef"u8;
    b = append(b, (rune)'"');
    foreach (var (_, r) in s) {
        switch (ᐧ) {
        case {} when r < (rune)' ' || r == (rune)'\\' || r == (rune)'"' || r == (rune)'<' || r == (rune)'>' || r == (rune)'&' || r == (rune)'\u2028' || r == (rune)'\u2029': {
            switch (r) {
            case (rune)'\\' or (rune)'"': {
                b = append(b, (rune)'\\', ((byte)r));
                break;
            }
            case (rune)'\n': {
                b = append(b, (rune)'\\', (rune)'n');
                break;
            }
            case (rune)'\r': {
                b = append(b, (rune)'\\', (rune)'r');
                break;
            }
            case (rune)'\t': {
                b = append(b, (rune)'\\', (rune)'t');
                break;
            }
            default: {
                b = append(b, (rune)'\\', (rune)'u', hex[(rune)((r >> (int)(12)) & 15)], hex[(rune)((r >> (int)(8)) & 15)], hex[(rune)((r >> (int)(4)) & 15)], hex[(rune)((r >> (int)(0)) & 15)]);
                break;
            }}

            break;
        }
        case {} when r is < utf8.RuneSelf: {
            b = append(b, ((byte)r));
            break;
        }
        default: {
            b = utf8.AppendRune(b, r);
            break;
        }}

    }
    b = append(b, (rune)'"');
    return b;
}

} // end expvar_package

// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package expvar provides a standardized interface to public variables, such
// as operation counters in servers. It exposes these variables via HTTP at
// /debug/vars in JSON format.
//
// Operations to set or modify these public variables are atomic.
//
// In addition to adding the HTTP handler, this package registers the
// following variables:
//
//    cmdline   os.Args
//    memstats  runtime.Memstats
//
// The package is sometimes only imported for the side effect of
// registering its HTTP handler and the above variables. To use it
// this way, link this package into your program:
//    import _ "expvar"
//

// package expvar -- go2cs converted at 2022 March 13 06:43:41 UTC
// import "expvar" ==> using expvar = go.expvar_package
// Original source: C:\Program Files\Go\src\expvar\expvar.go
namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using log = log_package;
using math = math_package;
using http = net.http_package;
using os = os_package;
using runtime = runtime_package;
using sort = sort_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using atomic = sync.atomic_package;


// Var is an abstract type for all exported variables.

using System;
public static partial class expvar_package {

public partial interface Var {
    @string String();
}

// Int is a 64-bit integer variable that satisfies the Var interface.
public partial struct Int {
    public long i;
}

private static long Value(this ptr<Int> _addr_v) {
    ref Int v = ref _addr_v.val;

    return atomic.LoadInt64(_addr_v.i);
}

private static @string String(this ptr<Int> _addr_v) {
    ref Int v = ref _addr_v.val;

    return strconv.FormatInt(atomic.LoadInt64(_addr_v.i), 10);
}

private static void Add(this ptr<Int> _addr_v, long delta) {
    ref Int v = ref _addr_v.val;

    atomic.AddInt64(_addr_v.i, delta);
}

private static void Set(this ptr<Int> _addr_v, long value) {
    ref Int v = ref _addr_v.val;

    atomic.StoreInt64(_addr_v.i, value);
}

// Float is a 64-bit float variable that satisfies the Var interface.
public partial struct Float {
    public ulong f;
}

private static double Value(this ptr<Float> _addr_v) {
    ref Float v = ref _addr_v.val;

    return math.Float64frombits(atomic.LoadUint64(_addr_v.f));
}

private static @string String(this ptr<Float> _addr_v) {
    ref Float v = ref _addr_v.val;

    return strconv.FormatFloat(math.Float64frombits(atomic.LoadUint64(_addr_v.f)), 'g', -1, 64);
}

// Add adds delta to v.
private static void Add(this ptr<Float> _addr_v, double delta) {
    ref Float v = ref _addr_v.val;

    while (true) {
        var cur = atomic.LoadUint64(_addr_v.f);
        var curVal = math.Float64frombits(cur);
        var nxtVal = curVal + delta;
        var nxt = math.Float64bits(nxtVal);
        if (atomic.CompareAndSwapUint64(_addr_v.f, cur, nxt)) {
            return ;
        }
    }
}

// Set sets v to value.
private static void Set(this ptr<Float> _addr_v, double value) {
    ref Float v = ref _addr_v.val;

    atomic.StoreUint64(_addr_v.f, math.Float64bits(value));
}

// Map is a string-to-Var map variable that satisfies the Var interface.
public partial struct Map {
    public sync.Map m; // map[string]Var
    public sync.RWMutex keysMu;
    public slice<@string> keys; // sorted
}

// KeyValue represents a single entry in a Map.
public partial struct KeyValue {
    public @string Key;
    public Var Value;
}

private static @string String(this ptr<Map> _addr_v) {
    ref Map v = ref _addr_v.val;

    ref strings.Builder b = ref heap(out ptr<strings.Builder> _addr_b);
    fmt.Fprintf(_addr_b, "{");
    var first = true;
    v.Do(kv => {
        if (!first) {
            fmt.Fprintf(_addr_b, ", ");
        }
        fmt.Fprintf(_addr_b, "%q: %v", kv.Key, kv.Value);
        first = false;
    });
    fmt.Fprintf(_addr_b, "}");
    return b.String();
}

// Init removes all keys from the map.
private static ptr<Map> Init(this ptr<Map> _addr_v) => func((defer, _, _) => {
    ref Map v = ref _addr_v.val;

    v.keysMu.Lock();
    defer(v.keysMu.Unlock());
    v.keys = v.keys[..(int)0];
    v.m.Range((k, _) => {
        v.m.Delete(k);
        return _addr_true!;
    });
    return _addr_v!;
});

// addKey updates the sorted list of keys in v.keys.
private static void addKey(this ptr<Map> _addr_v, @string key) => func((defer, _, _) => {
    ref Map v = ref _addr_v.val;

    v.keysMu.Lock();
    defer(v.keysMu.Unlock()); 
    // Using insertion sort to place key into the already-sorted v.keys.
    {
        var i = sort.SearchStrings(v.keys, key);

        if (i >= len(v.keys)) {
            v.keys = append(v.keys, key);
        }
        else if (v.keys[i] != key) {
            v.keys = append(v.keys, "");
            copy(v.keys[(int)i + 1..], v.keys[(int)i..]);
            v.keys[i] = key;
        }

    }
});

private static Var Get(this ptr<Map> _addr_v, @string key) {
    ref Map v = ref _addr_v.val;

    var (i, _) = v.m.Load(key);
    Var (av, _) = Var.As(i._<Var>())!;
    return av;
}

private static void Set(this ptr<Map> _addr_v, @string key, Var av) {
    ref Map v = ref _addr_v.val;
 
    // Before we store the value, check to see whether the key is new. Try a Load
    // before LoadOrStore: LoadOrStore causes the key interface to escape even on
    // the Load path.
    {
        var (_, ok) = v.m.Load(key);

        if (!ok) {
            {
                var (_, dup) = v.m.LoadOrStore(key, av);

                if (!dup) {
                    v.addKey(key);
                    return ;
                }

            }
        }
    }

    v.m.Store(key, av);
}

// Add adds delta to the *Int value stored under the given map key.
private static void Add(this ptr<Map> _addr_v, @string key, long delta) {
    ref Map v = ref _addr_v.val;

    var (i, ok) = v.m.Load(key);
    if (!ok) {
        bool dup = default;
        i, dup = v.m.LoadOrStore(key, @new<Int>());
        if (!dup) {
            v.addKey(key);
        }
    }
    {
        ptr<Int> (iv, ok) = i._<ptr<Int>>();

        if (ok) {
            iv.Add(delta);
        }
    }
}

// AddFloat adds delta to the *Float value stored under the given map key.
private static void AddFloat(this ptr<Map> _addr_v, @string key, double delta) {
    ref Map v = ref _addr_v.val;

    var (i, ok) = v.m.Load(key);
    if (!ok) {
        bool dup = default;
        i, dup = v.m.LoadOrStore(key, @new<Float>());
        if (!dup) {
            v.addKey(key);
        }
    }
    {
        ptr<Float> (iv, ok) = i._<ptr<Float>>();

        if (ok) {
            iv.Add(delta);
        }
    }
}

// Delete deletes the given key from the map.
private static void Delete(this ptr<Map> _addr_v, @string key) => func((defer, _, _) => {
    ref Map v = ref _addr_v.val;

    v.keysMu.Lock();
    defer(v.keysMu.Unlock());
    var i = sort.SearchStrings(v.keys, key);
    if (i < len(v.keys) && key == v.keys[i]) {
        v.keys = append(v.keys[..(int)i], v.keys[(int)i + 1..]);
        v.m.Delete(key);
    }
});

// Do calls f for each entry in the map.
// The map is locked during the iteration,
// but existing entries may be concurrently updated.
private static void Do(this ptr<Map> _addr_v, Action<KeyValue> f) => func((defer, _, _) => {
    ref Map v = ref _addr_v.val;

    v.keysMu.RLock();
    defer(v.keysMu.RUnlock());
    foreach (var (_, k) in v.keys) {
        var (i, _) = v.m.Load(k);
        f(new KeyValue(k,i.(Var)));
    }
});

// String is a string variable, and satisfies the Var interface.
public partial struct String {
    public atomic.Value s; // string
}

private static @string Value(this ptr<String> _addr_v) {
    ref String v = ref _addr_v.val;

    @string (p, _) = v.s.Load()._<@string>();
    return p;
}

// String implements the Var interface. To get the unquoted string
// use Value.
private static @string String(this ptr<String> _addr_v) {
    ref String v = ref _addr_v.val;

    var s = v.Value();
    var (b, _) = json.Marshal(s);
    return string(b);
}

private static void Set(this ptr<String> _addr_v, @string value) {
    ref String v = ref _addr_v.val;

    v.s.Store(value);
}

// Func implements Var by calling the function
// and formatting the returned value using JSON.
public delegate void Func();

public static void Value(this Func f) {
    return f();
}

public static @string String(this Func f) {
    var (v, _) = json.Marshal(f());
    return string(v);
}

// All published variables.
private static sync.Map vars = default;private static sync.RWMutex varKeysMu = default;private static slice<@string> varKeys = default;

// Publish declares a named exported variable. This should be called from a
// package's init function when it creates its Vars. If the name is already
// registered then this will log.Panic.
public static void Publish(@string name, Var v) => func((defer, _, _) => {
    {
        var (_, dup) = vars.LoadOrStore(name, v);

        if (dup) {
            log.Panicln("Reuse of exported var name:", name);
        }
    }
    varKeysMu.Lock();
    defer(varKeysMu.Unlock());
    varKeys = append(varKeys, name);
    sort.Strings(varKeys);
});

// Get retrieves a named exported variable. It returns nil if the name has
// not been registered.
public static Var Get(@string name) {
    var (i, _) = vars.Load(name);
    Var (v, _) = Var.As(i._<Var>())!;
    return v;
}

// Convenience functions for creating new exported variables.

public static ptr<Int> NewInt(@string name) {
    ptr<Int> v = @new<Int>();
    Publish(name, v);
    return _addr_v!;
}

public static ptr<Float> NewFloat(@string name) {
    ptr<Float> v = @new<Float>();
    Publish(name, v);
    return _addr_v!;
}

public static ptr<Map> NewMap(@string name) {
    ptr<Map> v = @new<Map>().Init();
    Publish(name, v);
    return _addr_v!;
}

public static ptr<String> NewString(@string name) {
    ptr<String> v = @new<String>();
    Publish(name, v);
    return _addr_v!;
}

// Do calls f for each exported variable.
// The global variable map is locked during the iteration,
// but existing entries may be concurrently updated.
public static void Do(Action<KeyValue> f) => func((defer, _, _) => {
    varKeysMu.RLock();
    defer(varKeysMu.RUnlock());
    foreach (var (_, k) in varKeys) {
        var (val, _) = vars.Load(k);
        f(new KeyValue(k,val.(Var)));
    }
});

private static void expvarHandler(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    w.Header().Set("Content-Type", "application/json; charset=utf-8");
    fmt.Fprintf(w, "{\n");
    var first = true;
    Do(kv => {
        if (!first) {
            fmt.Fprintf(w, ",\n");
        }
        first = false;
        fmt.Fprintf(w, "%q: %s", kv.Key, kv.Value);
    });
    fmt.Fprintf(w, "\n}\n");
}

// Handler returns the expvar HTTP Handler.
//
// This is only needed to install the handler in a non-standard location.
public static http.Handler Handler() {
    return http.HandlerFunc(expvarHandler);
}

private static void cmdline() {
    return os.Args;
}

private static void memstats() {
    ptr<object> stats = @new<runtime.MemStats>();
    runtime.ReadMemStats(stats);
    return stats.val;
}

private static void init() {
    http.HandleFunc("/debug/vars", expvarHandler);
    Publish("cmdline", Func(cmdline));
    Publish("memstats", Func(memstats));
}

} // end expvar_package

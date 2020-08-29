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
// package expvar -- go2cs converted at 2020 August 29 10:11:03 UTC
// import "expvar" ==> using expvar = go.expvar_package
// Original source: C:\Go\src\expvar\expvar.go
using bytes = go.bytes_package;
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using log = go.log_package;
using math = go.math_package;
using http = go.net.http_package;
using os = go.os_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class expvar_package
    {
        // Var is an abstract type for all exported variables.
        public partial interface Var
        {
            @string String();
        }

        // Int is a 64-bit integer variable that satisfies the Var interface.
        public partial struct Int
        {
            public long i;
        }

        private static long Value(this ref Int v)
        {
            return atomic.LoadInt64(ref v.i);
        }

        private static @string String(this ref Int v)
        {
            return strconv.FormatInt(atomic.LoadInt64(ref v.i), 10L);
        }

        private static void Add(this ref Int v, long delta)
        {
            atomic.AddInt64(ref v.i, delta);
        }

        private static void Set(this ref Int v, long value)
        {
            atomic.StoreInt64(ref v.i, value);
        }

        // Float is a 64-bit float variable that satisfies the Var interface.
        public partial struct Float
        {
            public ulong f;
        }

        private static double Value(this ref Float v)
        {
            return math.Float64frombits(atomic.LoadUint64(ref v.f));
        }

        private static @string String(this ref Float v)
        {
            return strconv.FormatFloat(math.Float64frombits(atomic.LoadUint64(ref v.f)), 'g', -1L, 64L);
        }

        // Add adds delta to v.
        private static void Add(this ref Float v, double delta)
        {
            while (true)
            {
                var cur = atomic.LoadUint64(ref v.f);
                var curVal = math.Float64frombits(cur);
                var nxtVal = curVal + delta;
                var nxt = math.Float64bits(nxtVal);
                if (atomic.CompareAndSwapUint64(ref v.f, cur, nxt))
                {
                    return;
                }
            }

        }

        // Set sets v to value.
        private static void Set(this ref Float v, double value)
        {
            atomic.StoreUint64(ref v.f, math.Float64bits(value));
        }

        // Map is a string-to-Var map variable that satisfies the Var interface.
        public partial struct Map
        {
            public sync.Map m; // map[string]Var
            public sync.RWMutex keysMu;
            public slice<@string> keys; // sorted
        }

        // KeyValue represents a single entry in a Map.
        public partial struct KeyValue
        {
            public @string Key;
            public Var Value;
        }

        private static @string String(this ref Map v)
        {
            bytes.Buffer b = default;
            fmt.Fprintf(ref b, "{");
            var first = true;
            v.Do(kv =>
            {
                if (!first)
                {
                    fmt.Fprintf(ref b, ", ");
                }
                fmt.Fprintf(ref b, "%q: %v", kv.Key, kv.Value);
                first = false;
            });
            fmt.Fprintf(ref b, "}");
            return b.String();
        }

        // Init removes all keys from the map.
        private static ref Map Init(this ref Map _v) => func(_v, (ref Map v, Defer defer, Panic _, Recover __) =>
        {
            v.keysMu.Lock();
            defer(v.keysMu.Unlock());
            v.keys = v.keys[..0L];
            v.m.Range((k, _) =>
            {
                v.m.Delete(k);
                return true;
            });
            return v;
        });

        // updateKeys updates the sorted list of keys in v.keys.
        private static void addKey(this ref Map _v, @string key) => func(_v, (ref Map v, Defer defer, Panic _, Recover __) =>
        {
            v.keysMu.Lock();
            defer(v.keysMu.Unlock());
            v.keys = append(v.keys, key);
            sort.Strings(v.keys);
        });

        private static Var Get(this ref Map v, @string key)
        {
            var (i, _) = v.m.Load(key);
            Var (av, _) = i._<Var>();
            return av;
        }

        private static void Set(this ref Map v, @string key, Var av)
        { 
            // Before we store the value, check to see whether the key is new. Try a Load
            // before LoadOrStore: LoadOrStore causes the key interface to escape even on
            // the Load path.
            {
                var (_, ok) = v.m.Load(key);

                if (!ok)
                {
                    {
                        var (_, dup) = v.m.LoadOrStore(key, av);

                        if (!dup)
                        {
                            v.addKey(key);
                            return;
                        }

                    }
                }

            }

            v.m.Store(key, av);
        }

        // Add adds delta to the *Int value stored under the given map key.
        private static void Add(this ref Map v, @string key, long delta)
        {
            var (i, ok) = v.m.Load(key);
            if (!ok)
            {
                bool dup = default;
                i, dup = v.m.LoadOrStore(key, @new<Int>());
                if (!dup)
                {
                    v.addKey(key);
                }
            } 

            // Add to Int; ignore otherwise.
            {
                ref Int (iv, ok) = i._<ref Int>();

                if (ok)
                {
                    iv.Add(delta);
                }

            }
        }

        // AddFloat adds delta to the *Float value stored under the given map key.
        private static void AddFloat(this ref Map v, @string key, double delta)
        {
            var (i, ok) = v.m.Load(key);
            if (!ok)
            {
                bool dup = default;
                i, dup = v.m.LoadOrStore(key, @new<Float>());
                if (!dup)
                {
                    v.addKey(key);
                }
            } 

            // Add to Float; ignore otherwise.
            {
                ref Float (iv, ok) = i._<ref Float>();

                if (ok)
                {
                    iv.Add(delta);
                }

            }
        }

        // Do calls f for each entry in the map.
        // The map is locked during the iteration,
        // but existing entries may be concurrently updated.
        private static void Do(this ref Map _v, Action<KeyValue> f) => func(_v, (ref Map v, Defer defer, Panic _, Recover __) =>
        {
            v.keysMu.RLock();
            defer(v.keysMu.RUnlock());
            foreach (var (_, k) in v.keys)
            {
                var (i, _) = v.m.Load(k);
                f(new KeyValue(k,i.(Var)));
            }
        });

        // String is a string variable, and satisfies the Var interface.
        public partial struct String
        {
            public atomic.Value s; // string
        }

        private static @string Value(this ref String v)
        {
            @string (p, _) = v.s.Load()._<@string>();
            return p;
        }

        // String implements the Val interface. To get the unquoted string
        // use Value.
        private static @string String(this ref String v)
        {
            var s = v.Value();
            var (b, _) = json.Marshal(s);
            return string(b);
        }

        private static void Set(this ref String v, @string value)
        {
            v.s.Store(value);
        }

        // Func implements Var by calling the function
        // and formatting the returned value using JSON.
        public delegate void Func();

        public static void Value(this Func f)
        {
            return f();
        }

        public static @string String(this Func f)
        {
            var (v, _) = json.Marshal(f());
            return string(v);
        }

        // All published variables.
        private static sync.Map vars = default;        private static sync.RWMutex varKeysMu = default;        private static slice<@string> varKeys = default;

        // Publish declares a named exported variable. This should be called from a
        // package's init function when it creates its Vars. If the name is already
        // registered then this will log.Panic.
        public static void Publish(@string name, Var v) => func((defer, _, __) =>
        {
            {
                var (_, dup) = vars.LoadOrStore(name, v);

                if (dup)
                {
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
        public static Var Get(@string name)
        {
            var (i, _) = vars.Load(name);
            Var (v, _) = i._<Var>();
            return v;
        }

        // Convenience functions for creating new exported variables.

        public static ref Int NewInt(@string name)
        {
            ptr<Int> v = @new<Int>();
            Publish(name, v);
            return v;
        }

        public static ref Float NewFloat(@string name)
        {
            ptr<Float> v = @new<Float>();
            Publish(name, v);
            return v;
        }

        public static ref Map NewMap(@string name)
        {
            ptr<Map> v = @new<Map>().Init();
            Publish(name, v);
            return v;
        }

        public static ref String NewString(@string name)
        {
            ptr<String> v = @new<String>();
            Publish(name, v);
            return v;
        }

        // Do calls f for each exported variable.
        // The global variable map is locked during the iteration,
        // but existing entries may be concurrently updated.
        public static void Do(Action<KeyValue> f) => func((defer, _, __) =>
        {
            varKeysMu.RLock();
            defer(varKeysMu.RUnlock());
            foreach (var (_, k) in varKeys)
            {
                var (val, _) = vars.Load(k);
                f(new KeyValue(k,val.(Var)));
            }
        });

        private static void expvarHandler(http.ResponseWriter w, ref http.Request r)
        {
            w.Header().Set("Content-Type", "application/json; charset=utf-8");
            fmt.Fprintf(w, "{\n");
            var first = true;
            Do(kv =>
            {
                if (!first)
                {
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
        public static http.Handler Handler()
        {
            return http.HandlerFunc(expvarHandler);
        }

        private static void cmdline()
        {
            return os.Args;
        }

        private static void memstats()
        {
            ptr<object> stats = @new<runtime.MemStats>();
            runtime.ReadMemStats(stats);
            return stats.Value;
        }

        private static void init()
        {
            http.HandleFunc("/debug/vars", expvarHandler);
            Publish("cmdline", Func(cmdline));
            Publish("memstats", Func(memstats));
        }
    }
}

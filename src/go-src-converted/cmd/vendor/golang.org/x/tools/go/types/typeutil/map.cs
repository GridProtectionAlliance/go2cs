// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package typeutil defines various utilities for types, such as Map,
// a mapping from types.Type to interface{} values.
// package typeutil -- go2cs converted at 2020 October 08 04:58:32 UTC
// import "cmd/vendor/golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.cmd.vendor.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\types\typeutil\map.go
// import "golang.org/x/tools/go/types/typeutil"

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace types
{
    public static partial class typeutil_package
    {
        // Map is a hash-table-based mapping from types (types.Type) to
        // arbitrary interface{} values.  The concrete types that implement
        // the Type interface are pointers.  Since they are not canonicalized,
        // == cannot be used to check for equivalence, and thus we cannot
        // simply use a Go map.
        //
        // Just as with map[K]V, a nil *Map is a valid empty map.
        //
        // Not thread-safe.
        //
        public partial struct Map
        {
            public Hasher hasher; // shared by many Maps
            public map<uint, slice<entry>> table; // maps hash to bucket; entry.key==nil means unused
            public long length; // number of map entries
        }

        // entry is an entry (key/value association) in a hash bucket.
        private partial struct entry
        {
            public types.Type key;
        }

        // SetHasher sets the hasher used by Map.
        //
        // All Hashers are functionally equivalent but contain internal state
        // used to cache the results of hashing previously seen types.
        //
        // A single Hasher created by MakeHasher() may be shared among many
        // Maps.  This is recommended if the instances have many keys in
        // common, as it will amortize the cost of hash computation.
        //
        // A Hasher may grow without bound as new types are seen.  Even when a
        // type is deleted from the map, the Hasher never shrinks, since other
        // types in the map may reference the deleted type indirectly.
        //
        // Hashers are not thread-safe, and read-only operations such as
        // Map.Lookup require updates to the hasher, so a full Mutex lock (not a
        // read-lock) is require around all Map operations if a shared
        // hasher is accessed from multiple threads.
        //
        // If SetHasher is not called, the Map will create a private hasher at
        // the first call to Insert.
        //
        private static void SetHasher(this ptr<Map> _addr_m, Hasher hasher)
        {
            ref Map m = ref _addr_m.val;

            m.hasher = hasher;
        }

        // Delete removes the entry with the given key, if any.
        // It returns true if the entry was found.
        //
        private static bool Delete(this ptr<Map> _addr_m, types.Type key)
        {
            ref Map m = ref _addr_m.val;

            if (m != null && m.table != null)
            {
                var hash = m.hasher.Hash(key);
                var bucket = m.table[hash];
                foreach (var (i, e) in bucket)
                {
                    if (e.key != null && types.Identical(key, e.key))
                    { 
                        // We can't compact the bucket as it
                        // would disturb iterators.
                        bucket[i] = new entry();
                        m.length--;
                        return true;

                    }

                }

            }

            return false;

        }

        // At returns the map entry for the given key.
        // The result is nil if the entry is not present.
        //
        private static void At(this ptr<Map> _addr_m, types.Type key)
        {
            ref Map m = ref _addr_m.val;

            if (m != null && m.table != null)
            {
                foreach (var (_, e) in m.table[m.hasher.Hash(key)])
                {
                    if (e.key != null && types.Identical(key, e.key))
                    {
                        return e.value;
                    }

                }

            }

            return null;

        }

        // Set sets the map entry for key to val,
        // and returns the previous entry, if any.
        private static object Set(this ptr<Map> _addr_m, types.Type key, object value)
        {
            object prev = default;
            ref Map m = ref _addr_m.val;

            if (m.table != null)
            {
                var hash = m.hasher.Hash(key);
                var bucket = m.table[hash];
                ptr<entry> hole;
                foreach (var (i, e) in bucket)
                {
                    if (e.key == null)
                    {
                        hole = _addr_bucket[i];
                    }
                    else if (types.Identical(key, e.key))
                    {
                        prev = e.value;
                        bucket[i].value = value;
                        return ;
                    }

                }
            else

                if (hole != null)
                {
                    hole.val = new entry(key,value); // overwrite deleted entry
                }
                else
                {
                    m.table[hash] = append(bucket, new entry(key,value));
                }

            }            {
                if (m.hasher.memo == null)
                {
                    m.hasher = MakeHasher();
                }

                hash = m.hasher.Hash(key);
                m.table = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<uint, slice<entry>>{hash:{entry{key,value}}};

            }

            m.length++;
            return ;

        }

        // Len returns the number of map entries.
        private static long Len(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            if (m != null)
            {
                return m.length;
            }

            return 0L;

        }

        // Iterate calls function f on each entry in the map in unspecified order.
        //
        // If f should mutate the map, Iterate provides the same guarantees as
        // Go maps: if f deletes a map entry that Iterate has not yet reached,
        // f will not be invoked for it, but if f inserts a map entry that
        // Iterate has not yet reached, whether or not f will be invoked for
        // it is unspecified.
        //
        private static void Iterate(this ptr<Map> _addr_m, Action<types.Type, object> f)
        {
            ref Map m = ref _addr_m.val;

            if (m != null)
            {
                foreach (var (_, bucket) in m.table)
                {
                    foreach (var (_, e) in bucket)
                    {
                        if (e.key != null)
                        {
                            f(e.key, e.value);
                        }

                    }

                }

            }

        }

        // Keys returns a new slice containing the set of map keys.
        // The order is unspecified.
        private static slice<types.Type> Keys(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            var keys = make_slice<types.Type>(0L, m.Len());
            m.Iterate((key, _) =>
            {
                keys = append(keys, key);
            });
            return keys;

        }

        private static @string toString(this ptr<Map> _addr_m, bool values)
        {
            ref Map m = ref _addr_m.val;

            if (m == null)
            {
                return "{}";
            }

            ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
            fmt.Fprint(_addr_buf, "{");
            @string sep = "";
            m.Iterate((key, value) =>
            {
                fmt.Fprint(_addr_buf, sep);
                sep = ", ";
                fmt.Fprint(_addr_buf, key);
                if (values)
                {
                    fmt.Fprintf(_addr_buf, ": %q", value);
                }

            });
            fmt.Fprint(_addr_buf, "}");
            return buf.String();

        }

        // String returns a string representation of the map's entries.
        // Values are printed using fmt.Sprintf("%v", v).
        // Order is unspecified.
        //
        private static @string String(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            return m.toString(true);
        }

        // KeysString returns a string representation of the map's key set.
        // Order is unspecified.
        //
        private static @string KeysString(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            return m.toString(false);
        }

        ////////////////////////////////////////////////////////////////////////
        // Hasher

        // A Hasher maps each type to its hash value.
        // For efficiency, a hasher uses memoization; thus its memory
        // footprint grows monotonically over time.
        // Hashers are not thread-safe.
        // Hashers have reference semantics.
        // Call MakeHasher to create a Hasher.
        public partial struct Hasher
        {
            public map<types.Type, uint> memo;
        }

        // MakeHasher returns a new Hasher instance.
        public static Hasher MakeHasher()
        {
            return new Hasher(make(map[types.Type]uint32));
        }

        // Hash computes a hash value for the given type t such that
        // Identical(t, t') => Hash(t) == Hash(t').
        public static uint Hash(this Hasher h, types.Type t)
        {
            var (hash, ok) = h.memo[t];
            if (!ok)
            {
                hash = h.hashFor(t);
                h.memo[t] = hash;
            }

            return hash;

        }

        // hashString computes the Fowler–Noll–Vo hash of s.
        private static uint hashString(@string s)
        {
            uint h = default;
            for (long i = 0L; i < len(s); i++)
            {
                h ^= uint32(s[i]);
                h *= 16777619L;
            }

            return h;

        }

        // hashFor computes the hash of t.
        public static uint hashFor(this Hasher h, types.Type t) => func((_, panic, __) =>
        { 
            // See Identical for rationale.
            switch (t.type())
            {
                case ptr<types.Basic> t:
                    return uint32(t.Kind());
                    break;
                case ptr<types.Array> t:
                    return 9043L + 2L * uint32(t.Len()) + 3L * h.Hash(t.Elem());
                    break;
                case ptr<types.Slice> t:
                    return 9049L + 2L * h.Hash(t.Elem());
                    break;
                case ptr<types.Struct> t:
                    uint hash = 9059L;
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (long i = 0L;
                        var n = t.NumFields(); i < n; i++)
                        {
                            var f = t.Field(i);
                            if (f.Anonymous())
                            {
                                hash += 8861L;
                            }

                            hash += hashString(t.Tag(i));
                            hash += hashString(f.Name()); // (ignore f.Pkg)
                            hash += h.Hash(f.Type());

                        }


                        i = i__prev1;
                        n = n__prev1;
                    }
                    return hash;
                    break;
                case ptr<types.Pointer> t:
                    return 9067L + 2L * h.Hash(t.Elem());
                    break;
                case ptr<types.Signature> t:
                    hash = 9091L;
                    if (t.Variadic())
                    {
                        hash *= 8863L;
                    }

                    return hash + 3L * h.hashTuple(t.Params()) + 5L * h.hashTuple(t.Results());
                    break;
                case ptr<types.Interface> t:
                    hash = 9103L;
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (i = 0L;
                        n = t.NumMethods(); i < n; i++)
                        { 
                            // See go/types.identicalMethods for rationale.
                            // Method order is not significant.
                            // Ignore m.Pkg().
                            var m = t.Method(i);
                            hash += 3L * hashString(m.Name()) + 5L * h.Hash(m.Type());

                        }


                        i = i__prev1;
                        n = n__prev1;
                    }
                    return hash;
                    break;
                case ptr<types.Map> t:
                    return 9109L + 2L * h.Hash(t.Key()) + 3L * h.Hash(t.Elem());
                    break;
                case ptr<types.Chan> t:
                    return 9127L + 2L * uint32(t.Dir()) + 3L * h.Hash(t.Elem());
                    break;
                case ptr<types.Named> t:
                    return uint32(reflect.ValueOf(t.Obj()).Pointer());
                    break;
                case ptr<types.Tuple> t:
                    return h.hashTuple(t);
                    break;
            }
            panic(t);

        });

        public static uint hashTuple(this Hasher h, ptr<types.Tuple> _addr_tuple)
        {
            ref types.Tuple tuple = ref _addr_tuple.val;
 
            // See go/types.identicalTypes for rationale.
            var n = tuple.Len();
            uint hash = 9137L + 2L * uint32(n);
            for (long i = 0L; i < n; i++)
            {
                hash += 3L * h.Hash(tuple.At(i).Type());
            }

            return hash;

        }
    }
}}}}}}}}

// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements a cache of method sets.

// package typeutil -- go2cs converted at 2020 October 09 06:05:05 UTC
// import "cmd/vendor/golang.org/x/tools/go/types/typeutil" ==> using typeutil = go.cmd.vendor.golang.org.x.tools.go.types.typeutil_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\types\typeutil\methodsetcache.go
using types = go.go.types_package;
using sync = go.sync_package;
using static go.builtin;

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
        // A MethodSetCache records the method set of each type T for which
        // MethodSet(T) is called so that repeat queries are fast.
        // The zero value is a ready-to-use cache instance.
        public partial struct MethodSetCache
        {
            public sync.Mutex mu;
            public map<types.Type, ptr<types.MethodSet>> others; // all other types
        }

        // MethodSet returns the method set of type T.  It is thread-safe.
        //
        // If cache is nil, this function is equivalent to types.NewMethodSet(T).
        // Utility functions can thus expose an optional *MethodSetCache
        // parameter to clients that care about performance.
        //
        private static ptr<types.MethodSet> MethodSet(this ptr<MethodSetCache> _addr_cache, types.Type T) => func((defer, _, __) =>
        {
            ref MethodSetCache cache = ref _addr_cache.val;

            if (cache == null)
            {
                return _addr_types.NewMethodSet(T)!;
            }

            cache.mu.Lock();
            defer(cache.mu.Unlock());

            switch (T.type())
            {
                case ptr<types.Named> T:
                    return _addr_cache.lookupNamed(T).value!;
                    break;
                case ptr<types.Pointer> T:
                    {
                        ptr<types.Named> (N, ok) = T.Elem()._<ptr<types.Named>>();

                        if (ok)
                        {
                            return _addr_cache.lookupNamed(N).pointer!;
                        }

                    }

                    break; 

                // all other types
                // (The map uses pointer equivalence, not type identity.)
            } 

            // all other types
            // (The map uses pointer equivalence, not type identity.)
            var mset = cache.others[T];
            if (mset == null)
            {
                mset = types.NewMethodSet(T);
                if (cache.others == null)
                {
                    cache.others = make_map<types.Type, ptr<types.MethodSet>>();
                }

                cache.others[T] = mset;

            }

            return _addr_mset!;

        });

        private static void lookupNamed(this ptr<MethodSetCache> _addr_cache, ptr<types.Named> _addr_named)
        {
            ref MethodSetCache cache = ref _addr_cache.val;
            ref types.Named named = ref _addr_named.val;

            if (cache.named == null)
            {
                cache.named = make();
            } 
            // Avoid recomputing mset(*T) for each distinct Pointer
            // instance whose underlying type is a named type.
            var (msets, ok) = cache.named[named];
            if (!ok)
            {
                msets.value = types.NewMethodSet(named);
                msets.pointer = types.NewMethodSet(types.NewPointer(named));
                cache.named[named] = msets;
            }

            return msets;

        }
    }
}}}}}}}}

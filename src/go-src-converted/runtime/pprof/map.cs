// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package pprof -- go2cs converted at 2022 March 06 22:14:40 UTC
// import "runtime/pprof" ==> using pprof = go.runtime.pprof_package
// Original source: C:\Program Files\Go\src\runtime\pprof\map.go
using @unsafe = go.@unsafe_package;

namespace go.runtime;

public static partial class pprof_package {

    // A profMap is a map from (stack, tag) to mapEntry.
    // It grows without bound, but that's assumed to be OK.
private partial struct profMap {
    public map<System.UIntPtr, ptr<profMapEntry>> hash;
    public ptr<profMapEntry> all;
    public ptr<profMapEntry> last;
    public slice<profMapEntry> free;
    public slice<System.UIntPtr> freeStk;
}

// A profMapEntry is a single entry in the profMap.
private partial struct profMapEntry {
    public ptr<profMapEntry> nextHash; // next in hash list
    public ptr<profMapEntry> nextAll; // next in list of all entries
    public slice<System.UIntPtr> stk;
    public unsafe.Pointer tag;
    public long count;
}

private static ptr<profMapEntry> lookup(this ptr<profMap> _addr_m, slice<ulong> stk, unsafe.Pointer tag) {
    ref profMap m = ref _addr_m.val;
 
    // Compute hash of (stk, tag).
    var h = uintptr(0);
    foreach (var (_, x) in stk) {
        h = h << 8 | (h >> (int)((8 * (@unsafe.Sizeof(h) - 1))));
        h += uintptr(x) * 41;
    }    h = h << 8 | (h >> (int)((8 * (@unsafe.Sizeof(h) - 1))));
    h += uintptr(tag) * 41; 

    // Find entry if present.
    ptr<profMapEntry> last;
Search: 

    // Add new entry.
    {
        var e__prev1 = e;

        var e = m.hash[h];

        while (e != null) {
            if (len(e.stk) != len(stk) || e.tag != tag) {
                continue;
            (last, e) = (e, e.nextHash);
            }

            {
                var j__prev2 = j;

                foreach (var (__j) in stk) {
                    j = __j;
                    if (e.stk[j] != uintptr(stk[j])) {
                        _continueSearch = true;
                        break;
                    }

                } 
                // Move to front.

                j = j__prev2;
            }

            if (last != null) {
                last.nextHash = e.nextHash;
                e.nextHash = m.hash[h];
                m.hash[h] = e;
            }

            return _addr_e!;

        }

        e = e__prev1;
    } 

    // Add new entry.
    if (len(m.free) < 1) {
        m.free = make_slice<profMapEntry>(128);
    }
    e = _addr_m.free[0];
    m.free = m.free[(int)1..];
    e.nextHash = m.hash[h];
    e.tag = tag;

    if (len(m.freeStk) < len(stk)) {
        m.freeStk = make_slice<System.UIntPtr>(1024);
    }
    e.stk = m.freeStk.slice(-1, len(stk), len(stk));
    m.freeStk = m.freeStk[(int)len(stk)..];

    {
        var j__prev1 = j;

        foreach (var (__j) in stk) {
            j = __j;
            e.stk[j] = uintptr(stk[j]);
        }
        j = j__prev1;
    }

    if (m.hash == null) {
        m.hash = make_map<System.UIntPtr, ptr<profMapEntry>>();
    }
    m.hash[h] = e;
    if (m.all == null) {
        m.all = e;
        m.last = e;
    }
    else
 {
        m.last.nextAll = e;
        m.last = e;
    }
    return _addr_e!;

}

} // end pprof_package

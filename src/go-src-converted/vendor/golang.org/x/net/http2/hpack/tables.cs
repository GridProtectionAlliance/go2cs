// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net.http2;

using fmt = fmt_package;

partial class hpack_package {

// headerFieldTable implements a list of HeaderFields.
// This is used to implement the static and dynamic tables.
[GoType] partial struct headerFieldTable {
    // For static tables, entries are never evicted.
    //
    // For dynamic tables, entries are evicted from ents[0] and added to the end.
    // Each entry has a unique id that starts at one and increments for each
    // entry that is added. This unique id is stable across evictions, meaning
    // it can be used as a pointer to a specific entry. As in hpack, unique ids
    // are 1-based. The unique id for ents[k] is k + evictCount + 1.
    //
    // Zero is not a valid unique id.
    //
    // evictCount should not overflow in any remotely practical situation. In
    // practice, we will have one dynamic table per HTTP/2 connection. If we
    // assume a very powerful server that handles 1M QPS per connection and each
    // request adds (then evicts) 100 entries from the table, it would still take
    // 2M years for evictCount to overflow.
    internal slice<HeaderField> ents;
    internal uint64 evictCount;
    // byName maps a HeaderField name to the unique id of the newest entry with
    // the same name. See above for a definition of "unique id".
    internal map<@string, uint64> byName;
    // byNameValue maps a HeaderField name/value pair to the unique id of the newest
    // entry with the same name and value. See above for a definition of "unique id".
    internal map<pairNameValue, uint64> byNameValue;
}

[GoType] partial struct pairNameValue {
    internal @string name;
    internal @string value;
}

[GoRecv] internal static void init(this ref headerFieldTable t) {
    t.byName = new map<@string, uint64>();
    t.byNameValue = new map<pairNameValue, uint64>();
}

// len reports the number of entries in the table.
[GoRecv] internal static nint len(this ref headerFieldTable t) {
    return len(t.ents);
}

// addEntry adds a new entry.
[GoRecv] internal static void addEntry(this ref headerFieldTable t, HeaderField f) {
    var id = ((uint64)t.len()) + t.evictCount + 1;
    t.byName[f.Name] = id;
    t.byNameValue[new pairNameValue(f.Name, f.Value)] = id;
    t.ents = append(t.ents, f);
}

// evictOldest evicts the n oldest entries in the table.
[GoRecv] internal static void evictOldest(this ref headerFieldTable t, nint n) {
    if (n > t.len()) {
        throw panic(fmt.Sprintf("evictOldest(%v) on table with %v entries"u8, n, t.len()));
    }
    for (nint k = 0; k < n; k++) {
        var f = t.ents[k];
        var id = t.evictCount + ((uint64)k) + 1;
        if (t.byName[f.Name] == id) {
            delete(t.byName, f.Name);
        }
        {
            var p = (new pairNameValue(f.Name, f.Value)); if (t.byNameValue[p] == id) {
                delete(t.byNameValue, p);
            }
        }
    }
    copy(t.ents, t.ents[(int)(n)..]);
    for (nint k = t.len() - n; k < t.len(); k++) {
        t.ents[k] = new HeaderField(nil);
    }
    // so strings can be garbage collected
    t.ents = t.ents[..(int)(t.len() - n)];
    if (t.evictCount + ((uint64)n) < t.evictCount) {
        throw panic("evictCount overflow");
    }
    t.evictCount += ((uint64)n);
}

// search finds f in the table. If there is no match, i is 0.
// If both name and value match, i is the matched index and nameValueMatch
// becomes true. If only name matches, i points to that index and
// nameValueMatch becomes false.
//
// The returned index is a 1-based HPACK index. For dynamic tables, HPACK says
// that index 1 should be the newest entry, but t.ents[0] is the oldest entry,
// meaning t.ents is reversed for dynamic tables. Hence, when t is a dynamic
// table, the return value i actually refers to the entry t.ents[t.len()-i].
//
// All tables are assumed to be a dynamic tables except for the global staticTable.
//
// See Section 2.3.3.
[GoRecv] internal static (uint64 i, bool nameValueMatch) search(this ref headerFieldTable t, HeaderField f) {
    uint64 i = default!;
    bool nameValueMatch = default!;

    if (!f.Sensitive) {
        {
            var id = t.byNameValue[new pairNameValue(f.Name, f.Value)]; if (id != 0) {
                return (t.idToIndex(id), true);
            }
        }
    }
    {
        var id = t.byName[f.Name]; if (id != 0) {
            return (t.idToIndex(id), false);
        }
    }
    return (0, false);
}

// idToIndex converts a unique id to an HPACK index.
// See Section 2.3.3.
[GoRecv] internal static uint64 idToIndex(this ref headerFieldTable t, uint64 id) {
    if (id <= t.evictCount) {
        throw panic(fmt.Sprintf("id (%v) <= evictCount (%v)"u8, id, t.evictCount));
    }
    var k = id - t.evictCount - 1;
    // convert id to an index t.ents[k]
    if (t != staticTable) {
        return ((uint64)t.len()) - k;
    }
    // dynamic table
    return k + 1;
}

internal static array<uint32> huffmanCodes = new uint32[]{
    8184,
    8388568,
    268435426,
    268435427,
    268435428,
    268435429,
    268435430,
    268435431,
    268435432,
    16777194,
    1073741820,
    268435433,
    268435434,
    1073741821,
    268435435,
    268435436,
    268435437,
    268435438,
    268435439,
    268435440,
    268435441,
    268435442,
    1073741822,
    268435443,
    268435444,
    268435445,
    268435446,
    268435447,
    268435448,
    268435449,
    268435450,
    268435451,
    20,
    1016,
    1017,
    4090,
    8185,
    21,
    248,
    2042,
    1018,
    1019,
    249,
    2043,
    250,
    22,
    23,
    24,
    0,
    1,
    2,
    25,
    26,
    27,
    28,
    29,
    30,
    31,
    92,
    251,
    32764,
    32,
    4091,
    1020,
    8186,
    33,
    93,
    94,
    95,
    96,
    97,
    98,
    99,
    100,
    101,
    102,
    103,
    104,
    105,
    106,
    107,
    108,
    109,
    110,
    111,
    112,
    113,
    114,
    252,
    115,
    253,
    8187,
    524272,
    8188,
    16380,
    34,
    32765,
    3,
    35,
    4,
    36,
    5,
    37,
    38,
    39,
    6,
    116,
    117,
    40,
    41,
    42,
    7,
    43,
    118,
    44,
    8,
    9,
    45,
    119,
    120,
    121,
    122,
    123,
    32766,
    2044,
    16381,
    8189,
    268435452,
    1048550,
    4194258,
    1048551,
    1048552,
    4194259,
    4194260,
    4194261,
    8388569,
    4194262,
    8388570,
    8388571,
    8388572,
    8388573,
    8388574,
    16777195,
    8388575,
    16777196,
    16777197,
    4194263,
    8388576,
    16777198,
    8388577,
    8388578,
    8388579,
    8388580,
    2097116,
    4194264,
    8388581,
    4194265,
    8388582,
    8388583,
    16777199,
    4194266,
    2097117,
    1048553,
    4194267,
    4194268,
    8388584,
    8388585,
    2097118,
    8388586,
    4194269,
    4194270,
    16777200,
    2097119,
    4194271,
    8388587,
    8388588,
    2097120,
    2097121,
    4194272,
    2097122,
    8388589,
    4194273,
    8388590,
    8388591,
    1048554,
    4194274,
    4194275,
    4194276,
    8388592,
    4194277,
    4194278,
    8388593,
    67108832,
    67108833,
    1048555,
    524273,
    4194279,
    8388594,
    4194280,
    33554412,
    67108834,
    67108835,
    67108836,
    134217694,
    134217695,
    67108837,
    16777201,
    33554413,
    524274,
    2097123,
    67108838,
    134217696,
    134217697,
    67108839,
    134217698,
    16777202,
    2097124,
    2097125,
    67108840,
    67108841,
    268435453,
    134217699,
    134217700,
    134217701,
    1048556,
    16777203,
    1048557,
    2097126,
    4194281,
    2097127,
    2097128,
    8388595,
    4194282,
    4194283,
    33554414,
    33554415,
    16777204,
    16777205,
    67108842,
    8388596,
    67108843,
    134217702,
    67108844,
    67108845,
    134217703,
    134217704,
    134217705,
    134217706,
    134217707,
    268435454,
    134217708,
    134217709,
    134217710,
    134217711,
    134217712,
    67108846
}.array();

internal static array<uint8> huffmanCodeLen = new uint8[]{
    13, 23, 28, 28, 28, 28, 28, 28, 28, 24, 30, 28, 28, 30, 28, 28,
    28, 28, 28, 28, 28, 28, 30, 28, 28, 28, 28, 28, 28, 28, 28, 28,
    6, 10, 10, 12, 13, 6, 8, 11, 10, 10, 8, 11, 8, 6, 6, 6,
    5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 7, 8, 15, 6, 12, 10,
    13, 6, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
    7, 7, 7, 7, 7, 7, 7, 7, 8, 7, 8, 13, 19, 13, 14, 6,
    15, 5, 6, 5, 6, 5, 6, 6, 6, 5, 7, 7, 6, 6, 6, 5,
    6, 7, 6, 5, 5, 6, 7, 7, 7, 7, 7, 15, 11, 14, 13, 28,
    20, 22, 20, 20, 22, 22, 22, 23, 22, 23, 23, 23, 23, 23, 24, 23,
    24, 24, 22, 23, 24, 23, 23, 23, 23, 21, 22, 23, 22, 23, 23, 24,
    22, 21, 20, 22, 22, 23, 23, 21, 23, 22, 22, 24, 21, 22, 23, 23,
    21, 21, 22, 21, 23, 22, 23, 23, 20, 22, 22, 22, 23, 22, 22, 23,
    26, 26, 20, 19, 22, 23, 22, 25, 26, 26, 26, 27, 27, 26, 24, 25,
    19, 21, 26, 27, 27, 26, 27, 24, 21, 21, 26, 26, 28, 27, 27, 27,
    20, 24, 20, 21, 22, 21, 21, 23, 22, 22, 25, 25, 24, 24, 26, 23,
    26, 27, 26, 26, 27, 27, 27, 27, 27, 28, 27, 27, 27, 27, 27, 26
}.array();

} // end hpack_package

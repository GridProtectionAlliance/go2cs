// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

partial class http_package {

// A mapping is a collection of key-value pairs where the keys are unique.
// A zero mapping is empty and ready to use.
// A mapping tries to pick a representation that makes [mapping.find] most efficient.
[GoType] partial struct mapping<K, V>
    where K : /* comparable */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, K, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
    where V : new()
{
    internal slice<entry<K, V>> s; // for few pairs
    internal map<K, V> m;     // for many pairs
}

[GoType] partial struct entry<K, V>
    where K : /* comparable */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, K, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
    where V : new()
{
    internal K key;
    internal V value;
}

// maxSlice is the maximum number of pairs for which a slice is used.
// It is a variable for benchmarking.
internal static nint maxSlice = 8;

// add adds a key-value pair to the mapping.
[GoRecv] internal static void add<K, V>(this ref mapping<K, V> h, K k, V v)
    where K : /* comparable */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, K, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
    where V : new()
{
    if (h.m == default! && len(h.s) < maxSlice){
        h.s = append(h.s, new entry<K, V>(k, v));
    } else {
        if (h.m == default!) {
            h.m = new map<K, V>{};
            foreach (var (_, e) in h.s) {
                h.m[e.key] = e.value;
            }
            h.s = default!;
        }
        h.m[k] = v;
    }
}

// find returns the value corresponding to the given key.
// The second return value is false if there is no value
// with that key.
[GoRecv] internal static (V v, bool found) find<K, V>(this ref mapping<K, V> h, K k)
    where K : /* comparable */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, K, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
    where V : new()
{
    V v = default!;
    bool found = default!;

    if (h == nil) {
        return (v, false);
    }
    if (h.m != default!) {
        (v, found) = h.m[k];
        return (v, found);
    }
    foreach (var (_, e) in h.s) {
        if (AreEqual(e.key, k)) {
            return (e.value, true);
        }
    }
    return (v, false);
}

// eachPair calls f for each pair in the mapping.
// If f returns false, pairs returns immediately.
[GoRecv] internal static void eachPair<K, V>(this ref mapping<K, V> h, Func<K, V, bool> f)
    where K : /* comparable */ IAdditionOperators<K, K, K>, ISubtractionOperators<K, K, K>, IMultiplyOperators<K, K, K>, IDivisionOperators<K, K, K>, IModulusOperators<K, K, K>, IBitwiseOperators<K, K, K>, IShiftOperators<K, K, K>, IEqualityOperators<K, K, bool>, IComparisonOperators<K, K, bool>, new()
    where V : new()
{
    if (h == nil) {
        return;
    }
    if (h.m != default!){
        foreach (var (k, v) in h.m) {
            if (!f(k, v)) {
                return;
            }
        }
    } else {
        foreach (var (_, e) in h.s) {
            if (!f(e.key, e.value)) {
                return;
            }
        }
    }
}

} // end http_package

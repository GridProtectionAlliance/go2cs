// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using rand = go.math.rand.rand_package;
using slices = slices_package;
using static go.sort_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.math.rand;
using sort = sort_package;

partial class sort_test_package {

internal static array<nint> ints = new nint[]{74, 59, 238, -784, 9845, 959, 905, 0, 0, 42, 7586, -5467984, 7586}.array();

internal static array<float64> float64s = new float64[]{74.3D, 59.0D, Δmath.Inf(1), 238.2D, -784.0D, 2.3D, Δmath.NaN(), Δmath.NaN(), Δmath.Inf(-1), 9845.768D, -959.7485D, 905, 7.8D, 7.8D}.array();

internal static array<@string> stringsData = new @string[]{"", "Hello", "foo", "bar", "foo", "f00", "%*&^*&^&", "***"}.array();

public static void TestSortIntSlice(ж<testing.T> Ꮡt) {
    var data = ints.Clone();
    var a = ((sort.IntSlice)(data[0..]));
    Sort(a);
    if (!IsSorted(a)) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortFloat64Slice(ж<testing.T> Ꮡt) {
    var data = float64s.Clone();
    var a = ((sort.Float64Slice)(data[0..]));
    Sort(a);
    if (!IsSorted(a)) {
        Ꮡt.Errorf("sorted %v"u8, float64s);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

// Compare Sort with slices.Sort sorting a float64 slice containing NaNs.
public static void TestSortFloat64sCompareSlicesSort(ж<testing.T> Ꮡt) {
    var slice1 = slices.Clone<slice<float64>, float64>(float64s[..]);
    var slice2 = slices.Clone<slice<float64>, float64>(float64s[..]);
    Sort(((sort.Float64Slice)slice1));
    slices.Sort<slice<float64>, float64>(slice2);
    // Compare for equality using cmp.Compare, which considers NaNs equal.
    if (!slices.EqualFunc(slice1, slice2, (float64 a, float64 b) => cmp.Compare(a, b) == 0)) {
        Ꮡt.Errorf("mismatch between Sort and slices.Sort: got %v, want %v"u8, slice1, slice2);
    }
}

public static void TestSortStringSlice(ж<testing.T> Ꮡt) {
    var data = stringsData.Clone();
    var a = ((sort.StringSlice)(data[0..]));
    Sort(a);
    if (!IsSorted(a)) {
        Ꮡt.Errorf("sorted %v"u8, stringsData);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestInts(ж<testing.T> Ꮡt) {
    var data = ints.Clone();
    Ints(data[0..]);
    if (!IntsAreSorted(data[0..])) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestFloat64s(ж<testing.T> Ꮡt) {
    var data = float64s.Clone();
    Float64s(data[0..]);
    if (!Float64sAreSorted(data[0..])) {
        Ꮡt.Errorf("sorted %v"u8, float64s);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestStrings(ж<testing.T> Ꮡt) {
    var data = stringsData.Clone();
    Strings(data[0..]);
    if (!StringsAreSorted(data[0..])) {
        Ꮡt.Errorf("sorted %v"u8, stringsData);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSlice(ж<testing.T> Ꮡt) {
    ref var data = ref heap<array<@string>>(out var Ꮡdata);
    data = stringsData.Clone();
    var dataʗ1 = data;
    Slice(data[..], (nint i, nint j) => dataʗ1[i] < dataʗ1[j]);
    var dataʗ3 = data;
    if (!SliceIsSorted(data[..], (nint i, nint j) => dataʗ3[i] < dataʗ3[j])) {
        Ꮡt.Errorf("sorted %v"u8, stringsData);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortLarge_Random(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    nint n = 1000000;
    if (testing.Short()) {
        n /= 100;
    }
    var data = new slice<nint>(n);
    for (nint i = 0; i < len(data); i++) {
        data[i] = rand.IntN(100);
    }
    if (IntsAreSorted(data)) {
        Ꮡt.Fatalf("terrible rand.rand"u8);
    }
    Ints(data);
    if (!IntsAreSorted(data)) {
        Ꮡt.Errorf("sort didn't sort - 1M ints"u8);
    }
}

public static void TestReverseSortIntSlice(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var data = ints.Clone();
    var data1 = ints.Clone();
    var a = ((sort.IntSlice)(data[0..]));
    Sort(a);
    var r = ((sort.IntSlice)(data1[0..]));
    Sort(Reverse(r));
    for (nint i = 0; i < len(data); i++) {
        if (a[i] != r[len(data) - 1 - i]) {
            Ꮡt.Errorf("reverse sort didn't sort"u8);
        }
        if (i > len(data) / 2) {
            break;
        }
    }
}

public static void TestBreakPatterns(ж<testing.T> Ꮡt) {
    // Special slice used to trigger breakPatterns.
    var data = new slice<nint>(30);
    foreach (var (i, _) in data) {
        data[i] = 10;
    }
    data[(len(data) / 4) * 1] = 0;
    data[(len(data) / 4) * 2] = 1;
    data[(len(data) / 4) * 3] = 2;
    Sort(((sort.IntSlice)data));
}

public static void TestReverseRange(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var data = new nint[]{1, 2, 3, 4, 5, 6, 7}.slice();
    ReverseRange(((sort.IntSlice)data), 0, len(data));
    for (nint i = len(data) - 1; i > 0; i--) {
        if (data[i] > data[i - 1]) {
            Ꮡt.Fatalf("reverseRange didn't work"u8);
        }
    }
    var data1 = new nint[]{1, 2, 3, 4, 5, 6, 7}.slice();
    var data2 = new nint[]{1, 2, 5, 4, 3, 6, 7}.slice();
    ReverseRange(((sort.IntSlice)data1), 2, 5);
    foreach (var (i, v) in data1) {
        if (v != data2[i]) {
            Ꮡt.Fatalf("reverseRange didn't work"u8);
        }
    }
}

[GoType] partial struct nonDeterministicTestingData {
    internal ж<rand.Rand> r;
}

[GoRecv] internal static nint Len(this ref nonDeterministicTestingData t) {
    return 500;
}

[GoRecv] internal static bool Less(this ref nonDeterministicTestingData t, nint i, nint j) {
    if (i < 0 || j < 0 || i >= t.Len() || j >= t.Len()) {
        throw panic("nondeterministic comparison out of bounds");
    }
    return t.r.Float32() < 0.5F;
}

[GoRecv] internal static void Swap(this ref nonDeterministicTestingData t, nint i, nint j) {
    if (i < 0 || j < 0 || i >= t.Len() || j >= t.Len()) {
        throw panic("nondeterministic comparison out of bounds");
    }
}

public static void TestNonDeterministicComparison(ж<testing.T> Ꮡt) => func((defer, recover) => {
    // Ensure that sort.Sort does not panic when Less returns inconsistent results.
    // See https://golang.org/issue/14377.
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                Ꮡt.Error(r);
            }
        }
    });
    var td = Ꮡ(new nonDeterministicTestingData(
        r: rand.New(new rand_PCGжSource(rand.NewPCG(0, 0)))
    ));
    for (nint i = 0; i < 10; i++) {
        Sort(new nonDeterministicTestingDataжInterface(td));
    }
});

public static void BenchmarkSortString1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    var unsorted = new slice<@string>((1 << (int)(10)));
    foreach (var (i, _) in unsorted) {
        unsorted[i] = strconv.Itoa((nint)(i ^ 0x2cc));
    }
    var data = new slice<@string>(len(unsorted));
    for (nint i = 0; i < b.N; i++) {
        copy(data, unsorted);
        b.StartTimer();
        Strings(data);
        b.StopTimer();
    }
}

public static void BenchmarkSortString1K_Slice(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    var unsorted = new slice<@string>((1 << (int)(10)));
    foreach (var (i, _) in unsorted) {
        unsorted[i] = strconv.Itoa((nint)(i ^ 0x2cc));
    }
    var data = new slice<@string>(len(unsorted));
    for (nint i = 0; i < b.N; i++) {
        copy(data, unsorted);
        b.StartTimer();
        var dataʗ1 = data;
        Slice(data, (nint iΔ1, nint j) => dataʗ1[iΔ1] < dataʗ1[j]);
        b.StopTimer();
    }
}

public static void BenchmarkStableString1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    var unsorted = new slice<@string>((1 << (int)(10)));
    foreach (var (i, _) in unsorted) {
        unsorted[i] = strconv.Itoa((nint)(i ^ 0x2cc));
    }
    var data = new slice<@string>(len(unsorted));
    for (nint i = 0; i < b.N; i++) {
        copy(data, unsorted);
        b.StartTimer();
        Stable(((sort.StringSlice)data));
        b.StopTimer();
    }
}

public static void BenchmarkSortInt1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(10)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = (nint)(iΔ1 ^ 0x2cc);
        }
        b.StartTimer();
        Ints(data);
        b.StopTimer();
    }
}

public static void BenchmarkSortInt1K_Sorted(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(10)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = iΔ1;
        }
        b.StartTimer();
        Ints(data);
        b.StopTimer();
    }
}

public static void BenchmarkSortInt1K_Reversed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(10)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = len(data) - iΔ1;
        }
        b.StartTimer();
        Ints(data);
        b.StopTimer();
    }
}

public static void BenchmarkSortInt1K_Mod8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(10)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = iΔ1 % 8;
        }
        b.StartTimer();
        Ints(data);
        b.StopTimer();
    }
}

public static void BenchmarkStableInt1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    var unsorted = new slice<nint>((1 << (int)(10)));
    foreach (var (i, _) in unsorted) {
        unsorted[i] = (nint)(i ^ 0x2cc);
    }
    var data = new slice<nint>(len(unsorted));
    for (nint i = 0; i < b.N; i++) {
        copy(data, unsorted);
        b.StartTimer();
        Stable(((sort.IntSlice)data));
        b.StopTimer();
    }
}

public static void BenchmarkStableInt1K_Slice(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    var unsorted = new slice<nint>((1 << (int)(10)));
    foreach (var (i, _) in unsorted) {
        unsorted[i] = (nint)(i ^ 0x2cc);
    }
    var data = new slice<nint>(len(unsorted));
    for (nint i = 0; i < b.N; i++) {
        copy(data, unsorted);
        b.StartTimer();
        var dataʗ1 = data;
        SliceStable(data, (nint iΔ1, nint j) => dataʗ1[iΔ1] < dataʗ1[j]);
        b.StopTimer();
    }
}

public static void BenchmarkSortInt64K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(16)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = (nint)(iΔ1 ^ 0xcccc);
        }
        b.StartTimer();
        Ints(data);
        b.StopTimer();
    }
}

public static void BenchmarkSortInt64K_Slice(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(16)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = (nint)(iΔ1 ^ 0xcccc);
        }
        b.StartTimer();
        var dataʗ1 = data;
        Slice(data, (nint iΔ2, nint j) => dataʗ1[iΔ2] < dataʗ1[j]);
        b.StopTimer();
    }
}

public static void BenchmarkStableInt64K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    b.StopTimer();
    for (nint i = 0; i < b.N; i++) {
        var data = new slice<nint>((1 << (int)(16)));
        for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
            data[iΔ1] = (nint)(iΔ1 ^ 0xcccc);
        }
        b.StartTimer();
        Stable(((sort.IntSlice)data));
        b.StopTimer();
    }
}

internal static readonly UntypedInt _Sawtooth = iota;
internal static readonly UntypedInt _Rand = 1;
internal static readonly UntypedInt _Stagger = 2;
internal static readonly UntypedInt _Plateau = 3;
internal static readonly UntypedInt _Shuffle = 4;
internal static readonly UntypedInt _NDist = 5;

internal static readonly UntypedInt _Copy = iota;
internal static readonly UntypedInt _Reverse = 1;
internal static readonly UntypedInt _ReverseFirstHalf = 2;
internal static readonly UntypedInt _ReverseSecondHalf = 3;
internal static readonly UntypedInt _Sorted = 4;
internal static readonly UntypedInt _Dither = 5;
internal static readonly UntypedInt _NMode = 6;

[GoType] partial struct testingData {
    internal @string desc;
    internal ж<testing.T> t;
    internal slice<nint> data;
    internal nint maxswap; // number of swaps allowed
    internal nint ncmp, nswap;
}

[GoRecv] internal static nint Len(this ref testingData d) {
    return len(d.data);
}

[GoRecv] internal static bool Less(this ref testingData d, nint i, nint j) {
    d.ncmp++;
    return d.data[i] < d.data[j];
}

[GoRecv] internal static void Swap(this ref testingData d, nint i, nint j) {
    if (d.nswap >= d.maxswap) {
        d.t.Fatalf("%s: used %d swaps sorting slice of %d"u8, d.desc, d.nswap, len(d.data));
    }
    d.nswap++;
    (d.data[i], d.data[j]) = (d.data[j], d.data[i]);
}

internal static nint lg(nint n) {
    nint i = 0;
    while ((1 << (int)((nuint)i)) < n) {
        i++;
    }
    return i;
}

internal static void testBentleyMcIlroy(ж<testing.T> Ꮡt, Action<sort.Interface> sort, Func<nint, nint> maxswap) {
    ref var t = ref Ꮡt.Value;

    var sizes = new nint[]{100, 1023, 1024, 1025}.slice();
    if (testing.Short()) {
        sizes = new nint[]{100, 127, 128, 129}.slice();
    }
    var dists = new @string[]{"sawtooth", "rand", "stagger", "plateau", "shuffle"}.slice();
    var modes = new @string[]{"copy", "reverse", "reverse1", "reverse2", "sort", "dither"}.slice();
    array<nint> tmp1 = new(1025);
    array<nint> tmp2 = new(1025);
    foreach (var (_, n) in sizes) {
        for (nint m = 1; m < 2 * n; m *= 2) {
            for (nint dist = 0; dist < _NDist; dist++) {
                nint j = 0;
                nint k = 1;
                var data = tmp1[0..(int)(n)];
                for (nint i = 0; i < n; i++) {
                    var exprᴛ1 = dist;
                    if (exprᴛ1 == _Sawtooth) {
                        data[i] = i % m;
                    }
                    else if (exprᴛ1 == _Rand) {
                        data[i] = rand.IntN(m);
                    }
                    else if (exprᴛ1 == _Stagger) {
                        data[i] = (i * m + i) % n;
                    }
                    else if (exprᴛ1 == _Plateau) {
                        data[i] = min(i, m);
                    }
                    else if (exprᴛ1 == _Shuffle) {
                        if (rand.IntN(m) != 0){
                            j += 2;
                            data[i] = j;
                        } else {
                            k += 2;
                            data[i] = k;
                        }
                    }

                }
                var mdata = tmp2[0..(int)(n)];
                for (nint mode = 0; mode < _NMode; mode++) {
                    var exprᴛ2 = mode;
                    if (exprᴛ2 == _Copy) {
                        for (nint i = 0; i < n; i++) {
                            mdata[i] = data[i];
                        }
                    }
                    else if (exprᴛ2 == _Reverse) {
                        for (nint i = 0; i < n; i++) {
                            mdata[i] = data[n - i - 1];
                        }
                    }
                    else if (exprᴛ2 == _ReverseFirstHalf) {
                        for (nint i = 0; i < n / 2; i++) {
                            mdata[i] = data[n / 2 - i - 1];
                        }
                        for (nint i = n / 2; i < n; i++) {
                            mdata[i] = data[i];
                        }
                    }
                    else if (exprᴛ2 == _ReverseSecondHalf) {
                        for (nint i = 0; i < n / 2; i++) {
                            mdata[i] = data[i];
                        }
                        for (nint i = n / 2; i < n; i++) {
                            mdata[i] = data[n - (i - n / 2) - 1];
                        }
                    }
                    else if (exprᴛ2 == _Sorted) {
                        for (nint i = 0; i < n; i++) {
                            mdata[i] = data[i];
                        }
                        Ints(mdata);
                    }
                    else if (exprᴛ2 == _Dither) {
                        for (nint i = 0; i < n; i++) {
                            // Ints is known to be correct
                            // because mode Sort runs after mode _Copy.
                            mdata[i] = data[i] + i % 5;
                        }
                    }

                    ref var desc = ref heap<@string>(out var Ꮡdesc);
                    desc = fmt.Sprintf("n=%d m=%d dist=%s mode=%s"u8, n, m, dists[dist], modes[mode]);
                    var d = Ꮡ(new testingData(desc: desc, t: Ꮡt, data: mdata[0..(int)(n)], maxswap: maxswap(n)));
                    sort(new testingDataжInterface(d));
                    // Uncomment if you are trying to improve the number of compares/swaps.
                    //t.Logf("%s: ncmp=%d, nswp=%d", desc, d.ncmp, d.nswap)
                    // If we were testing C qsort, we'd have to make a copy
                    // of the slice and sort it ourselves and then compare
                    // x against it, to ensure that qsort was only permuting
                    // the data, not (for example) overwriting it with zeros.
                    //
                    // In go, we don't have to be so paranoid: since the only
                    // mutating method Sort can call is TestingData.swap,
                    // it suffices here just to check that the final slice is sorted.
                    if (!IntsAreSorted(mdata)) {
                        Ꮡt.Fatalf("%s: ints not sorted\n\t%v"u8, desc, mdata);
                    }
                }
            }
        }
    }
}

public static void TestSortBM(ж<testing.T> Ꮡt) {
    testBentleyMcIlroy(Ꮡt, Sort, (nint n) => n * lg(n) * 12 / 10);
}

public static void TestHeapsortBM(ж<testing.T> Ꮡt) {
    testBentleyMcIlroy(Ꮡt, Heapsort, (nint n) => n * lg(n) * 12 / 10);
}

public static void TestStableBM(ж<testing.T> Ꮡt) {
    testBentleyMcIlroy(Ꮡt, Stable, (nint n) => n * lg(n) * lg(n) / 3);
}

// This is based on the "antiquicksort" implementation by M. Douglas McIlroy.
// See https://www.cs.dartmouth.edu/~doug/mdmspe.pdf for more info.
[GoType] partial struct adversaryTestingData {
    internal ж<testing.T> t;
    internal slice<nint> data; // item values, initialized to special gas value and changed by Less
    internal nint maxcmp;  // number of comparisons allowed
    internal nint ncmp;  // number of comparisons (calls to Less)
    internal nint nsolid;  // number of elements that have been set to non-gas values
    internal nint candidate;  // guess at current pivot
    internal nint gas;  // special value for unset elements, higher than everything else
}

[GoRecv] internal static nint Len(this ref adversaryTestingData d) {
    return len(d.data);
}

[GoRecv] internal static bool Less(this ref adversaryTestingData d, nint i, nint j) {
    if (d.ncmp >= d.maxcmp) {
        d.t.Fatalf("used %d comparisons sorting adversary data with size %d"u8, d.ncmp, len(d.data));
    }
    d.ncmp++;
    if (d.data[i] == d.gas && d.data[j] == d.gas) {
        if (i == d.candidate){
            // freeze i
            d.data[i] = d.nsolid;
            d.nsolid++;
        } else {
            // freeze j
            d.data[j] = d.nsolid;
            d.nsolid++;
        }
    }
    if (d.data[i] == d.gas){
        d.candidate = i;
    } else 
    if (d.data[j] == d.gas) {
        d.candidate = j;
    }
    return d.data[i] < d.data[j];
}

[GoRecv] internal static void Swap(this ref adversaryTestingData d, nint i, nint j) {
    (d.data[i], d.data[j]) = (d.data[j], d.data[i]);
}

internal static ж<adversaryTestingData> newAdversaryTestingData(ж<testing.T> Ꮡt, nint size, nint maxcmp) {
    ref var t = ref Ꮡt.Value;

    ref var gas = ref heap<nint>(out var Ꮡgas);
    gas = size - 1;
    var data = new slice<nint>(size);
    for (nint i = 0; i < size; i++) {
        data[i] = gas;
    }
    return Ꮡ(new adversaryTestingData(t: Ꮡt, data: data, maxcmp: maxcmp, gas: gas));
}

public static void TestAdversary(ж<testing.T> Ꮡt) {
    const nint size = 10000;   // large enough to distinguish between O(n^2) and O(n*log(n))
    nint maxcmp = size * lg(size) * 4;
    // the factor 4 was found by trial and error
    var d = newAdversaryTestingData(Ꮡt, size, maxcmp);
    Sort(new adversaryTestingDataжInterface(d));
    // This should degenerate to heapsort.
    // Check data is fully populated and sorted.
    foreach (var (i, v) in (~d).data) {
        if (v != i) {
            Ꮡt.Fatalf("adversary data not fully sorted"u8);
        }
    }
}

public static void TestStableInts(ж<testing.T> Ꮡt) {
    var data = ints.Clone();
    Stable(((sort.IntSlice)(data[0..])));
    if (!IntsAreSorted(data[0..])) {
        Ꮡt.Errorf("nsorted %v\n   got %v"u8, ints, data);
    }
}

[GoType("dyn")] partial struct intPairsᴛ1 {
    internal nint a, b;
}

[GoType("[]intPairsᴛ1")] partial struct intPairs;

// IntPairs compare on a only.
internal static nint Len(this intPairs d) {
    return len(d);
}

internal static bool Less(this intPairs d, nint i, nint j) {
    return d[i].a < d[j].a;
}

internal static void Swap(this intPairs d, nint i, nint j) {
    (d[i], d[j]) = (d[j], d[i]);
}

// Record initial order in B.
internal static void initB(this intPairs d) {
    foreach (var (i, _) in d) {
        d[i].b = i;
    }
}

// InOrder checks if a-equal elements were not reordered.
internal static bool inOrder(this intPairs d) {
    nint lastA = -1;
    nint lastB = 0;
    for (nint i = 0; i < len(d); i++) {
        if (lastA != d[i].a) {
            lastA = d[i].a;
            lastB = d[i].b;
            continue;
        }
        if (d[i].b <= lastB) {
            return false;
        }
        lastB = d[i].b;
    }
    return true;
}

public static void TestStability(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    nint n = 100000;
    nint m = 1000;
    if (testing.Short()) {
        (n, m) = (1000, 100);
    }
    var data = new intPairs(n);
    // random distribution
    for (nint i = 0; i < len(data); i++) {
        data[i].a = rand.IntN(m);
    }
    if (IsSorted(data)) {
        Ꮡt.Fatalf("terrible rand.rand"u8);
    }
    data.initB();
    Stable(data);
    if (!IsSorted(data)) {
        Ꮡt.Errorf("Stable didn't sort %d ints"u8, n);
    }
    if (!data.inOrder()) {
        Ꮡt.Errorf("Stable wasn't stable on %d ints"u8, n);
    }
    // already sorted
    data.initB();
    Stable(data);
    if (!IsSorted(data)) {
        Ꮡt.Errorf("Stable shuffled sorted %d ints (order)"u8, n);
    }
    if (!data.inOrder()) {
        Ꮡt.Errorf("Stable shuffled sorted %d ints (stability)"u8, n);
    }
    // sorted reversed
    for (nint i = 0; i < len(data); i++) {
        data[i].a = len(data) - i;
    }
    data.initB();
    Stable(data);
    if (!IsSorted(data)) {
        Ꮡt.Errorf("Stable didn't sort %d ints"u8, n);
    }
    if (!data.inOrder()) {
        Ꮡt.Errorf("Stable wasn't stable on %d ints"u8, n);
    }
}

internal static slice<nint> countOpsSizes = new nint[]{100, 300, 1000, 3000, 10000, 30000, 100000, 300000, 1000000}.slice();

internal static void countOps(ж<testing.T> Ꮡt, Action<sort.Interface> algo, @string name) {
    ref var t = ref Ꮡt.Value;

    var sizes = countOpsSizes;
    if (testing.Short()) {
        sizes = sizes[..5];
    }
    if (!testing.Verbose()) {
        Ꮡt.Skip("Counting skipped as non-verbose mode.");
    }
    foreach (var (_, n) in sizes) {
        ref var td = ref heap<testingData>(out var Ꮡtd);
        td = new testingData(
            desc: name,
            t: Ꮡt,
            data: new slice<nint>(n),
            maxswap: (nint)(2147483648L - 1)
        );
        for (nint i = 0; i < n; i++) {
            td.data[i] = rand.IntN(n / 5);
        }
        algo(new testingDataжInterface(Ꮡtd));
        Ꮡt.Logf("%s %8d elements: %11d Swap, %10d Less"u8, name, n, td.nswap, td.ncmp);
    }
}

public static void TestCountStableOps(ж<testing.T> Ꮡt) {
    countOps(Ꮡt, Stable, "Stable"u8);
}

public static void TestCountSortOps(ж<testing.T> Ꮡt) {
    countOps(Ꮡt, Sort, "Sort  "u8);
}

internal static void bench(ж<testing.B> Ꮡb, nint size, Action<sort.Interface> algo, @string name) {
    ref var b = ref Ꮡb.Value;

    if (strings.HasSuffix(testenv.Builder(), "-race"u8) && size > 10000) {
        Ꮡb.Skip("skipping slow benchmark on race builder");
    }
    b.StopTimer();
    var data = new intPairs(size);
    var x = ~(uint32)0;
    for (nint i = 0; i < b.N; i++) {
        for (nint n = size - 3; n <= size + 3; n++) {
            for (nint iΔ1 = 0; iΔ1 < len(data); iΔ1++) {
                x += x;
                x ^= (uint32)(1);
                if ((int32)x < 0) {
                    x ^= (uint32)(0x88888eefU);
                }
                data[iΔ1].a = (nint)(x % (uint32)(n / 5));
            }
            data.initB();
            b.StartTimer();
            algo(data);
            b.StopTimer();
            if (!IsSorted(data)) {
                Ꮡb.Errorf("%s did not sort %d ints"u8, name, n);
            }
            if (name == "Stable"u8 && !data.inOrder()) {
                Ꮡb.Errorf("%s unstable on %d ints"u8, name, n);
            }
        }
    }
}

public static void BenchmarkSort1e2(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 100, Sort, "Sort"u8);
}

public static void BenchmarkStable1e2(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 100, Stable, "Stable"u8);
}

public static void BenchmarkSort1e4(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 10000, Sort, "Sort"u8);
}

public static void BenchmarkStable1e4(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 10000, Stable, "Stable"u8);
}

public static void BenchmarkSort1e6(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 1000000, Sort, "Sort"u8);
}

public static void BenchmarkStable1e6(ж<testing.B> Ꮡb) {
    bench(Ꮡb, 1000000, Stable, "Stable"u8);
}

} // end sort_test_package

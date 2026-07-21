// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using itoa = go.@internal.itoa_package;
using math = math_package;
using testing = testing_package;
using go.@internal;

partial class itoa_test_package {

internal static int64 minInt64 = math.MinInt64;
internal static int64 maxInt64 = math.MaxInt64;
internal static uint64 maxUint64 = math.MaxUint64;

public static void TestItoa(ж<testing.T> Ꮡt) {
    var tests = new nint[]{(nint)minInt64, math.MinInt32, -999, -100, -1, 0, 1, 100, 999, math.MaxInt32, (nint)maxInt64}.slice();
    foreach (var (_, tt) in tests) {
        @string got = itoa.Itoa(tt);
        @string want = fmt.Sprint(tt);
        if (want != got) {
            Ꮡt.Fatalf("Itoa(%d) = %s, want %s"u8, tt, got, want);
        }
    }
}

public static void TestUitoa(ж<testing.T> Ꮡt) {
    var tests = new nuint[]{0, 1, 100, 999, math.MaxUint32, (nuint)maxUint64}.slice();
    foreach (var (_, tt) in tests) {
        @string got = itoa.Uitoa(tt);
        @string want = fmt.Sprint(tt);
        if (want != got) {
            Ꮡt.Fatalf("Uitoa(%d) = %s, want %s"u8, tt, got, want);
        }
    }
}

public static void TestUitox(ж<testing.T> Ꮡt) {
    var tests = new nuint[]{0, 1, 15, 100, 999, math.MaxUint32, (nuint)maxUint64}.slice();
    foreach (var (_, tt) in tests) {
        @string got = itoa.Uitox(tt);
        @string want = fmt.Sprintf("%#x"u8, tt);
        if (want != got) {
            Ꮡt.Fatalf("Uitox(%x) = %s, want %s"u8, tt, got, want);
        }
    }
}

} // end itoa_test_package

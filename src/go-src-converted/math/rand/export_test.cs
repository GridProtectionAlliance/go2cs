// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

partial class rand_package {

public static int32 Int31nForTest(ж<Rand> Ꮡr, int32 n) {
    ref var r = ref Ꮡr.Value;

    return r.int31n(n);
}

public static (float64, array<uint32>, array<float32>, array<float32>) GetNormalDistributionParameters() {
    return (rn, kn.Clone(), wn.Clone(), fn.Clone());
}

public static (float64, array<uint32>, array<float32>, array<float32>) GetExponentialDistributionParameters() {
    return (re, ke.Clone(), we.Clone(), fe.Clone());
}

} // end rand_package

// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using static go.math.rand_package;
using sync = sync_package;
using testing = testing_package;

partial class rand_test_package {

// TestConcurrent exercises the rand API concurrently, triggering situations
// where the race detector is likely to detect issues.
public static void TestConcurrent(ж<testing.T> Ꮡt) => func((defer, recover) => {
    const nint numRoutines = 10;
    const nint numCycles = 10;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    defer(Ꮡwg.Wait);
    Ꮡwg.Add(numRoutines);
    for (nint i = 0; i < numRoutines; i++) {
        goǃ((nint iΔ1) => func((defer, recover) => {
            defer(Ꮡwg.Done);
            var buf = new slice<byte>(997);
            for (nint j = 0; j < numCycles; j++) {
                int64 seed = default!;
                seed += (int64)ExpFloat64();
                seed += (int64)Float32();
                seed += (int64)Float64();
                seed += (int64)Intn(Int());
                seed += (int64)Int31n(Int31());
                seed += (int64)Int63n(Int63());
                seed += (int64)NormFloat64();
                seed += (int64)Uint32();
                seed += (int64)Uint64();
                foreach (var (_, p) in Perm(10)) {
                    seed += (int64)p;
                }
                Read(buf);
                foreach (var (_, b) in buf) {
                    seed += (int64)b;
                }
                Seed((int64)(iΔ1 * j) * seed);
            }
        }), i);
    }
});

} // end rand_test_package

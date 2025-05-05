// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bits = math.bits_package;
using os = os_package;
using strconv = strconv_package;
using strings = strings_package;
using atomic = sync.atomic_package;
using time = time_package;
using math;
using sync;

partial class fuzz_package {

[GoType] partial interface mutatorRand {
    uint32 uint32();
    nint intn(nint _);
    uint32 uint32n(uint32 _);
    nint exp2();
    bool @bool();
    void save(ж<uint64> randState, ж<uint64> randInc);
    void restore(uint64 randState, uint64 randInc);
}

// The functions in pcg implement a 32 bit PRNG with a 64 bit period: pcg xsh rr
// 64 32. See https://www.pcg-random.org/ for more information. This
// implementation is geared specifically towards the needs of fuzzing: Simple
// creation and use, no reproducibility, no concurrency safety, just the
// necessary methods, optimized for speed.
internal static atomic.Uint64 globalInc;     // PCG stream

internal const uint64 multiplier = 6364136223846793005;

// pcgRand is a PRNG. It should not be copied or shared. No Rand methods are
// concurrency safe.
[GoType] partial struct pcgRand {
    internal noCopy noCopy; // help avoid mistakes: ask vet to ensure that we don't make a copy
    internal uint64 state;
    internal uint64 inc;
}

internal static ж<nint> godebugSeed() {
    var debug = strings.Split(os.Getenv("GODEBUG"u8), ","u8);
    foreach (var (_, f) in debug) {
        if (strings.HasPrefix(f, "fuzzseed="u8)) {
            (seed, err) = strconv.Atoi(strings.TrimPrefix(f, "fuzzseed="u8));
            if (err != default!) {
                throw panic("malformed fuzzseed");
            }
            return Ꮡseed;
        }
    }
    return default!;
}

// newPcgRand generates a new, seeded Rand, ready for use.
internal static ж<pcgRand> newPcgRand() {
    var r = @new<pcgRand>();
    var now = ((uint64)time.Now().UnixNano());
    {
        var seed = godebugSeed(); if (seed != nil) {
            now = ((uint64)(seed.val));
        }
    }
    var inc = globalInc.Add(1);
    r.val.state = now;
    r.val.inc = (uint64)((inc << (int)(1)) | 1);
    r.step();
    r.val.state += now;
    r.step();
    return r;
}

[GoRecv] internal static void step(this ref pcgRand r) {
    r.state *= multiplier;
    r.state += r.inc;
}

[GoRecv] internal static void save(this ref pcgRand r, ж<uint64> ᏑrandState, ж<uint64> ᏑrandInc) {
    ref var randState = ref ᏑrandState.val;
    ref var randInc = ref ᏑrandInc.val;

    randState = r.state;
    randInc = r.inc;
}

[GoRecv] internal static void restore(this ref pcgRand r, uint64 randState, uint64 randInc) {
    r.state = randState;
    r.inc = randInc;
}

// uint32 returns a pseudo-random uint32.
[GoRecv] internal static uint32 uint32(this ref pcgRand r) {
    var x = r.state;
    r.step();
    return bits.RotateLeft32(((uint32)(((uint64)((x >> (int)(18)) ^ x)) >> (int)(27))), -((nint)(x >> (int)(59))));
}

// intn returns a pseudo-random number in [0, n).
// n must fit in a uint32.
[GoRecv] internal static nint intn(this ref pcgRand r, nint n) {
    if (((nint)((uint32)n)) != n) {
        throw panic("large Intn");
    }
    return ((nint)r.uint32n(((uint32)n)));
}

// uint32n returns a pseudo-random number in [0, n).
//
// For implementation details, see:
// https://lemire.me/blog/2016/06/27/a-fast-alternative-to-the-modulo-reduction
// https://lemire.me/blog/2016/06/30/fast-random-shuffling
[GoRecv] internal static uint32 uint32n(this ref pcgRand r, uint32 n) {
    var v = r.uint32();
    var prod = ((uint64)v) * ((uint64)n);
    var low = ((uint32)prod);
    if (low < n) {
        var thresh = ((uint32)(-((int32)n))) % n;
        while (low < thresh) {
            v = r.uint32();
            prod = ((uint64)v) * ((uint64)n);
            low = ((uint32)prod);
        }
    }
    return ((uint32)(prod >> (int)(32)));
}

// exp2 generates n with probability 1/2^(n+1).
[GoRecv] internal static nint exp2(this ref pcgRand r) {
    return bits.TrailingZeros32(r.uint32());
}

// bool generates a random bool.
[GoRecv] internal static bool @bool(this ref pcgRand r) {
    return (uint32)(r.uint32() & 1) == 0;
}

// noCopy may be embedded into structs which must not be copied
// after the first use.
//
// See https://golang.org/issues/8005#issuecomment-190753527
// for details.
[GoType] partial struct noCopy {
}

// Lock is a no-op used by -copylocks checker from `go vet`.
[GoRecv] internal static void Lock(this ref noCopy _) {
}

[GoRecv] internal static void Unlock(this ref noCopy _) {
}

} // end fuzz_package

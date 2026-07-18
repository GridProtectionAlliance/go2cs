//******************************************************************************************************
//  rand_impl.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/17/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Hand-written implementation of math/rand/v2's //go:linkname runtime primitive. In Go, runtime.rand
// is the runtime's per-goroutine ChaCha8 generator, cryptographically seeded at startup; go2cs emits
// the linkname as a bodyless `partial`, and without a body here the PartialStubGenerator would fill it
// with a throwing stub — so every top-level v2 convenience function (globalRand wraps runtimeSource)
// would crash at first use. Random.Shared matches the contract: OS-entropy seeded, thread-safe, fast,
// and NON-deterministic run to run — exactly like Go, where code needing a reproducible sequence must
// seed explicitly (rand.New(rand.NewPCG(seed1, seed2))), which routes through the converted generators
// and never reaches this hook.

using System;
using System.Buffers.Binary;

// Hand-owned (no rand_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned companions.
[module: go.GoManualConversion]

namespace go.math.rand;

partial class rand_package
{
    internal static partial uint64 runtime_rand()
    {
        Span<byte> bytes = stackalloc byte[8];
        Random.Shared.NextBytes(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }
}

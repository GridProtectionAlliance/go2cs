//******************************************************************************************************
//  proc_impl.cs - Gbtc
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
//  07/19/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Hand-written implementation of os's //go:linkname-into-runtime hook (proc.go's
// `runtime_beforeExit`, provided by runtime.os_beforeExit). go2cs emits it as a bodyless `partial`
// method, and without a body here the PartialStubGenerator fills it with a throwing stub — so EVERY
// os.Exit call died with "runtime_beforeExit: external (assembly or cgo) function is not
// implemented" instead of exiting. That made any converted program that calls os.Exit unusable, and
// in particular made a spawned CHILD process report a panic and exit 2 rather than its own status
// (surfaced by the os/exec child-process work: the child's output carried the stub's message and
// its exit code never reached the parent).
//
// Go's runtime.os_beforeExit does exactly two things, neither of which applies here:
//
//   1. runExitHooks(code) — runs the registered exit hooks, whose only stdlib producer is coverage
//      instrumentation writing out a coverage data file. go2cs does not implement Go's coverage
//      instrumentation, so no hook is ever registered and the call has nothing to run. (os cannot
//      forward to the real runtime.os_beforeExit in any case: os.csproj does not reference the
//      runtime package, and the implementation there is `internal` to that assembly.)
//   2. racefini() when exiting 0 under -race — go2cs has no race detector.
//
// So a no-op is the faithful behavior, not a shortcut: os.Exit proceeds directly to syscall.Exit,
// which is what actually terminates the process. If go2cs ever gains exit hooks or coverage output,
// this is the single place that has to grow a body.

namespace go;

partial class os_package
{
    internal static partial void runtime_beforeExit(nint exitCode)
    {
    }
}

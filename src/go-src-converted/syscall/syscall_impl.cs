//******************************************************************************************************
//  syscall_impl.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
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

// Hand-written implementations of the syscall primitives Go's RUNTIME provides rather than the
// syscall package itself (syscall.go: "Getpagesize and Exit are provided by the runtime", plus the
// runtimeSetenv/runtimeUnsetenv linknames). go2cs emits each as a bodyless `partial` method, and
// without a body here the PartialStubGenerator fills them with throwing stubs — so `syscall.Exit`
// died with "Exit: external (assembly or cgo) function is not implemented" instead of terminating
// the process. That is the last step of os.Exit, so no converted program could exit deliberately;
// it surfaced in the os/exec child-process work, where a CHILD that called os.Exit(7) reported the
// stub panic instead of its own status.

using System;

// Hand-owned (no syscall_impl.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned operational files in this package.
[module: go.GoManualConversion]

namespace go;

partial class syscall_package
{
    // Go's runtime exits the process immediately with the given status. Environment.Exit is the
    // managed equivalent (it is what golib's unrecovered-panic handler already uses to report
    // exit code 2) and flushes the console writers on the way out, which Go's buffered stdout
    // does as well.
    public static partial void Exit(nint code)
    {
        Environment.Exit((int)code);
    }

    // Windows' page size is 4096 on every architecture Go supports; runtime.getpagesize reports
    // the value the OS gave it at startup, which is this constant in practice.
    public static partial nint Getpagesize()
    {
        return 4096;
    }

    // In Go these keep the RUNTIME's private copy of the environment in sync with the process
    // environment that syscall.Setenv/Unsetenv just changed via SetEnvironmentVariable. go2cs has
    // no such second copy — syscall.Getenv/Environ read the live process environment through
    // GetEnvironmentVariableW/GetEnvironmentStringsW every time — so there is nothing to mirror
    // and the correct behavior is to do nothing.
    internal static partial void runtimeSetenv(@string k, @string v)
    {
    }

    internal static partial void runtimeUnsetenv(@string k)
    {
    }
}

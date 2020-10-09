// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,amd64

// Package macOS provides cgo-less wrappers for Core Foundation and
// Security.framework, similarly to how package syscall provides access to
// libSystem.dylib.
// package macOS -- go2cs converted at 2020 October 09 04:54:54 UTC
// import "crypto/x509/internal.macOS" ==> using macOS = go.crypto.x509.internal.macOS_package
// Original source: C:\Go\src\crypto\x509\internal\macos\corefoundation.go
using errors = go.errors_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace crypto {
namespace x509
{
    public static partial class macOS_package
    {
        // CFRef is an opaque reference to a Core Foundation object. It is a pointer,
        // but to memory not owned by Go, so not an unsafe.Pointer.
        public partial struct CFRef // : System.UIntPtr
        {
        }

        // CFDataToSlice returns a copy of the contents of data as a bytes slice.
        public static slice<byte> CFDataToSlice(CFRef data)
        {
            var length = CFDataGetLength(data);
            var ptr = CFDataGetBytePtr(data);
            ptr<array<byte>> src = new ptr<ptr<array<byte>>>(@unsafe.Pointer(ptr)).slice(-1, length, length);
            var @out = make_slice<byte>(length);
            copy(out, src);
            return out;
        }

        public partial struct CFString // : CFRef
        {
        }

        private static readonly long kCFAllocatorDefault = (long)0L;

        private static readonly ulong kCFStringEncodingUTF8 = (ulong)0x08000100UL;

        //go:linkname x509_CFStringCreateWithBytes x509_CFStringCreateWithBytes
        //go:cgo_import_dynamic x509_CFStringCreateWithBytes CFStringCreateWithBytes "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        // StringToCFString returns a copy of the UTF-8 contents of s as a new CFString.


        //go:linkname x509_CFStringCreateWithBytes x509_CFStringCreateWithBytes
        //go:cgo_import_dynamic x509_CFStringCreateWithBytes CFStringCreateWithBytes "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        // StringToCFString returns a copy of the UTF-8 contents of s as a new CFString.
        public static CFString StringToCFString(@string s)
        {
            var p = @unsafe.Pointer((reflect.StringHeader.val)(@unsafe.Pointer(_addr_s)).Data);
            var ret = syscall(funcPC(x509_CFStringCreateWithBytes_trampoline), kCFAllocatorDefault, uintptr(p), uintptr(len(s)), uintptr(kCFStringEncodingUTF8), 0L, 0L);
            runtime.KeepAlive(p);
            return CFString(ret);
        }
        private static void x509_CFStringCreateWithBytes_trampoline()
;

        //go:linkname x509_CFDictionaryGetValueIfPresent x509_CFDictionaryGetValueIfPresent
        //go:cgo_import_dynamic x509_CFDictionaryGetValueIfPresent CFDictionaryGetValueIfPresent "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static (CFRef, bool) CFDictionaryGetValueIfPresent(CFRef dict, CFString key)
        {
            CFRef value = default;
            bool ok = default;

            var ret = syscall(funcPC(x509_CFDictionaryGetValueIfPresent_trampoline), uintptr(dict), uintptr(key), uintptr(@unsafe.Pointer(_addr_value)), 0L, 0L, 0L);
            if (ret == 0L)
            {>>MARKER:FUNCTION_x509_CFStringCreateWithBytes_trampoline_BLOCK_PREFIX<<
                return (0L, false);
            }

            return (value, true);

        }
        private static void x509_CFDictionaryGetValueIfPresent_trampoline()
;

        private static readonly long kCFNumberSInt32Type = (long)3L;

        //go:linkname x509_CFNumberGetValue x509_CFNumberGetValue
        //go:cgo_import_dynamic x509_CFNumberGetValue CFNumberGetValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"



        //go:linkname x509_CFNumberGetValue x509_CFNumberGetValue
        //go:cgo_import_dynamic x509_CFNumberGetValue CFNumberGetValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static (int, error) CFNumberGetValue(CFRef num)
        {
            int _p0 = default;
            error _p0 = default!;

            ref int value = ref heap(out ptr<int> _addr_value);
            var ret = syscall(funcPC(x509_CFNumberGetValue_trampoline), uintptr(num), uintptr(kCFNumberSInt32Type), uintptr(@unsafe.Pointer(_addr_value)), 0L, 0L, 0L);
            if (ret == 0L)
            {>>MARKER:FUNCTION_x509_CFDictionaryGetValueIfPresent_trampoline_BLOCK_PREFIX<<
                return (0L, error.As(errors.New("CFNumberGetValue call failed"))!);
            }

            return (value, error.As(null!)!);

        }
        private static void x509_CFNumberGetValue_trampoline()
;

        //go:linkname x509_CFDataGetLength x509_CFDataGetLength
        //go:cgo_import_dynamic x509_CFDataGetLength CFDataGetLength "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static long CFDataGetLength(CFRef data)
        {
            var ret = syscall(funcPC(x509_CFDataGetLength_trampoline), uintptr(data), 0L, 0L, 0L, 0L, 0L);
            return int(ret);
        }
        private static void x509_CFDataGetLength_trampoline()
;

        //go:linkname x509_CFDataGetBytePtr x509_CFDataGetBytePtr
        //go:cgo_import_dynamic x509_CFDataGetBytePtr CFDataGetBytePtr "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static System.UIntPtr CFDataGetBytePtr(CFRef data)
        {
            var ret = syscall(funcPC(x509_CFDataGetBytePtr_trampoline), uintptr(data), 0L, 0L, 0L, 0L, 0L);
            return ret;
        }
        private static void x509_CFDataGetBytePtr_trampoline()
;

        //go:linkname x509_CFArrayGetCount x509_CFArrayGetCount
        //go:cgo_import_dynamic x509_CFArrayGetCount CFArrayGetCount "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static long CFArrayGetCount(CFRef array)
        {
            var ret = syscall(funcPC(x509_CFArrayGetCount_trampoline), uintptr(array), 0L, 0L, 0L, 0L, 0L);
            return int(ret);
        }
        private static void x509_CFArrayGetCount_trampoline()
;

        //go:linkname x509_CFArrayGetValueAtIndex x509_CFArrayGetValueAtIndex
        //go:cgo_import_dynamic x509_CFArrayGetValueAtIndex CFArrayGetValueAtIndex "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static CFRef CFArrayGetValueAtIndex(CFRef array, long index)
        {
            var ret = syscall(funcPC(x509_CFArrayGetValueAtIndex_trampoline), uintptr(array), uintptr(index), 0L, 0L, 0L, 0L);
            return CFRef(ret);
        }
        private static void x509_CFArrayGetValueAtIndex_trampoline()
;

        //go:linkname x509_CFEqual x509_CFEqual
        //go:cgo_import_dynamic x509_CFEqual CFEqual "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static bool CFEqual(CFRef a, CFRef b)
        {
            var ret = syscall(funcPC(x509_CFEqual_trampoline), uintptr(a), uintptr(b), 0L, 0L, 0L, 0L);
            return ret == 1L;
        }
        private static void x509_CFEqual_trampoline()
;

        //go:linkname x509_CFRelease x509_CFRelease
        //go:cgo_import_dynamic x509_CFRelease CFRelease "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

        public static void CFRelease(CFRef @ref)
        {
            syscall(funcPC(x509_CFRelease_trampoline), uintptr(ref), 0L, 0L, 0L, 0L, 0L);
        }
        private static void x509_CFRelease_trampoline()
;

        // syscall is implemented in the runtime package (runtime/sys_darwin.go)
        private static System.UIntPtr syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
;

        // funcPC returns the entry point for f. See comments in runtime/proc.go
        // for the function of the same name.
        //go:nosplit
        private static System.UIntPtr funcPC(Action f)
        {
            return new ptr<ptr<ptr<ptr<ptr<System.UIntPtr>>>>>(@unsafe.Pointer(_addr_f));
        }
    }
}}}

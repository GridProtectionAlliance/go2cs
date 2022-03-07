// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin && !ios
// +build darwin,!ios

// Package macOS provides cgo-less wrappers for Core Foundation and
// Security.framework, similarly to how package syscall provides access to
// libSystem.dylib.
// package macOS -- go2cs converted at 2022 March 06 22:19:50 UTC
// import "crypto/x509/internal.macOS" ==> using macOS = go.crypto.x509.internal.macOS_package
// Original source: C:\Program Files\Go\src\crypto\x509\internal\macos\corefoundation.go
using errors = go.errors_package;
using abi = go.@internal.abi_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using @unsafe = go.@unsafe_package;

namespace go.crypto.x509;

public static partial class macOS_package {

    // Core Foundation linker flags for the external linker. See Issue 42459.
    //go:cgo_ldflag "-framework"
    //go:cgo_ldflag "CoreFoundation"

    // CFRef is an opaque reference to a Core Foundation object. It is a pointer,
    // but to memory not owned by Go, so not an unsafe.Pointer.
public partial struct CFRef { // : System.UIntPtr
}

// CFDataToSlice returns a copy of the contents of data as a bytes slice.
public static slice<byte> CFDataToSlice(CFRef data) {
    var length = CFDataGetLength(data);
    var ptr = CFDataGetBytePtr(data);
    ptr<array<byte>> src = new ptr<ptr<array<byte>>>(@unsafe.Pointer(ptr)).slice(-1, length, length);
    var @out = make_slice<byte>(length);
    copy(out, src);
    return out;
}

public partial struct CFString { // : CFRef
}

private static readonly nint kCFAllocatorDefault = 0;

private static readonly nuint kCFStringEncodingUTF8 = 0x08000100;

//go:cgo_import_dynamic x509_CFStringCreateWithBytes CFStringCreateWithBytes "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

// StringToCFString returns a copy of the UTF-8 contents of s as a new CFString.


//go:cgo_import_dynamic x509_CFStringCreateWithBytes CFStringCreateWithBytes "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

// StringToCFString returns a copy of the UTF-8 contents of s as a new CFString.
public static CFString StringToCFString(@string s) {
    var p = @unsafe.Pointer((reflect.StringHeader.val)(@unsafe.Pointer(_addr_s)).Data);
    var ret = syscall(abi.FuncPCABI0(x509_CFStringCreateWithBytes_trampoline), kCFAllocatorDefault, uintptr(p), uintptr(len(s)), uintptr(kCFStringEncodingUTF8), 0, 0);
    runtime.KeepAlive(p);
    return CFString(ret);
}
private static void x509_CFStringCreateWithBytes_trampoline();

//go:cgo_import_dynamic x509_CFDictionaryGetValueIfPresent CFDictionaryGetValueIfPresent "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static (CFRef, bool) CFDictionaryGetValueIfPresent(CFRef dict, CFString key) {
    CFRef value = default;
    bool ok = default;

    var ret = syscall(abi.FuncPCABI0(x509_CFDictionaryGetValueIfPresent_trampoline), uintptr(dict), uintptr(key), uintptr(@unsafe.Pointer(_addr_value)), 0, 0, 0);
    if (ret == 0) {>>MARKER:FUNCTION_x509_CFStringCreateWithBytes_trampoline_BLOCK_PREFIX<<
        return (0, false);
    }
    return (value, true);

}
private static void x509_CFDictionaryGetValueIfPresent_trampoline();

private static readonly nint kCFNumberSInt32Type = 3;

//go:cgo_import_dynamic x509_CFNumberGetValue CFNumberGetValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"



//go:cgo_import_dynamic x509_CFNumberGetValue CFNumberGetValue "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static (int, error) CFNumberGetValue(CFRef num) {
    int _p0 = default;
    error _p0 = default!;

    ref int value = ref heap(out ptr<int> _addr_value);
    var ret = syscall(abi.FuncPCABI0(x509_CFNumberGetValue_trampoline), uintptr(num), uintptr(kCFNumberSInt32Type), uintptr(@unsafe.Pointer(_addr_value)), 0, 0, 0);
    if (ret == 0) {>>MARKER:FUNCTION_x509_CFDictionaryGetValueIfPresent_trampoline_BLOCK_PREFIX<<
        return (0, error.As(errors.New("CFNumberGetValue call failed"))!);
    }
    return (value, error.As(null!)!);

}
private static void x509_CFNumberGetValue_trampoline();

//go:cgo_import_dynamic x509_CFDataGetLength CFDataGetLength "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static nint CFDataGetLength(CFRef data) {
    var ret = syscall(abi.FuncPCABI0(x509_CFDataGetLength_trampoline), uintptr(data), 0, 0, 0, 0, 0);
    return int(ret);
}
private static void x509_CFDataGetLength_trampoline();

//go:cgo_import_dynamic x509_CFDataGetBytePtr CFDataGetBytePtr "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static System.UIntPtr CFDataGetBytePtr(CFRef data) {
    var ret = syscall(abi.FuncPCABI0(x509_CFDataGetBytePtr_trampoline), uintptr(data), 0, 0, 0, 0, 0);
    return ret;
}
private static void x509_CFDataGetBytePtr_trampoline();

//go:cgo_import_dynamic x509_CFArrayGetCount CFArrayGetCount "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static nint CFArrayGetCount(CFRef array) {
    var ret = syscall(abi.FuncPCABI0(x509_CFArrayGetCount_trampoline), uintptr(array), 0, 0, 0, 0, 0);
    return int(ret);
}
private static void x509_CFArrayGetCount_trampoline();

//go:cgo_import_dynamic x509_CFArrayGetValueAtIndex CFArrayGetValueAtIndex "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static CFRef CFArrayGetValueAtIndex(CFRef array, nint index) {
    var ret = syscall(abi.FuncPCABI0(x509_CFArrayGetValueAtIndex_trampoline), uintptr(array), uintptr(index), 0, 0, 0, 0);
    return CFRef(ret);
}
private static void x509_CFArrayGetValueAtIndex_trampoline();

//go:cgo_import_dynamic x509_CFEqual CFEqual "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static bool CFEqual(CFRef a, CFRef b) {
    var ret = syscall(abi.FuncPCABI0(x509_CFEqual_trampoline), uintptr(a), uintptr(b), 0, 0, 0, 0);
    return ret == 1;
}
private static void x509_CFEqual_trampoline();

//go:cgo_import_dynamic x509_CFRelease CFRelease "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation"

public static void CFRelease(CFRef @ref) {
    syscall(abi.FuncPCABI0(x509_CFRelease_trampoline), uintptr(ref), 0, 0, 0, 0, 0);
}
private static void x509_CFRelease_trampoline();

// syscall is implemented in the runtime package (runtime/sys_darwin.go)
private static System.UIntPtr syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6);

} // end macOS_package

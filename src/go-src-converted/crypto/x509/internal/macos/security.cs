// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin && !ios
// +build darwin,!ios

// package macOS -- go2cs converted at 2022 March 06 22:19:51 UTC
// import "crypto/x509/internal.macOS" ==> using macOS = go.crypto.x509.internal.macOS_package
// Original source: C:\Program Files\Go\src\crypto\x509\internal\macos\security.go
using errors = go.errors_package;
using abi = go.@internal.abi_package;
using strconv = go.strconv_package;
using @unsafe = go.@unsafe_package;

namespace go.crypto.x509;

public static partial class macOS_package {

    // Security.framework linker flags for the external linker. See Issue 42459.
    //go:cgo_ldflag "-framework"
    //go:cgo_ldflag "Security"

    // Based on https://opensource.apple.com/source/Security/Security-59306.41.2/base/Security.h
public partial struct SecTrustSettingsResult { // : int
}

public static readonly SecTrustSettingsResult SecTrustSettingsResultInvalid = iota;
public static readonly var SecTrustSettingsResultTrustRoot = 0;
public static readonly var SecTrustSettingsResultTrustAsRoot = 1;
public static readonly var SecTrustSettingsResultDeny = 2;
public static readonly var SecTrustSettingsResultUnspecified = 3;


public partial struct SecTrustSettingsDomain { // : int
}

public static readonly SecTrustSettingsDomain SecTrustSettingsDomainUser = iota;
public static readonly var SecTrustSettingsDomainAdmin = 0;
public static readonly var SecTrustSettingsDomainSystem = 1;


public partial struct OSStatus {
    public @string call;
    public int status;
}

public static @string Error(this OSStatus s) {
    return s.call + " error: " + strconv.Itoa(int(s.status));
}

// Dictionary keys are defined as build-time strings with CFSTR, but the Go
// linker's internal linking mode can't handle CFSTR relocations. Create our
// own dynamic strings instead and just never release them.
//
// Note that this might be the only thing that can break over time if
// these values change, as the ABI arguably requires using the strings
// pointed to by the symbols, not values that happen to be equal to them.

public static var SecTrustSettingsResultKey = StringToCFString("kSecTrustSettingsResult");
public static var SecTrustSettingsPolicy = StringToCFString("kSecTrustSettingsPolicy");
public static var SecTrustSettingsPolicyString = StringToCFString("kSecTrustSettingsPolicyString");
public static var SecPolicyOid = StringToCFString("SecPolicyOid");
public static var SecPolicyAppleSSL = StringToCFString("1.2.840.113635.100.1.3"); // defined by POLICYMACRO

public static var ErrNoTrustSettings = errors.New("no trust settings found");

private static readonly nint errSecNoTrustSettings = -25263;

//go:cgo_import_dynamic x509_SecTrustSettingsCopyCertificates SecTrustSettingsCopyCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"



//go:cgo_import_dynamic x509_SecTrustSettingsCopyCertificates SecTrustSettingsCopyCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"

public static (CFRef, error) SecTrustSettingsCopyCertificates(SecTrustSettingsDomain domain) {
    CFRef certArray = default;
    error err = default!;

    var ret = syscall(abi.FuncPCABI0(x509_SecTrustSettingsCopyCertificates_trampoline), uintptr(domain), uintptr(@unsafe.Pointer(_addr_certArray)), 0, 0, 0, 0);
    if (int32(ret) == errSecNoTrustSettings) {
        return (0, error.As(ErrNoTrustSettings)!);
    }
    else if (ret != 0) {
        return (0, error.As(new OSStatus("SecTrustSettingsCopyCertificates",int32(ret)))!);
    }
    return (certArray, error.As(null!)!);

}
private static void x509_SecTrustSettingsCopyCertificates_trampoline();

private static readonly int kSecFormatX509Cert = 9;

//go:cgo_import_dynamic x509_SecItemExport SecItemExport "/System/Library/Frameworks/Security.framework/Versions/A/Security"



//go:cgo_import_dynamic x509_SecItemExport SecItemExport "/System/Library/Frameworks/Security.framework/Versions/A/Security"

public static (CFRef, error) SecItemExport(CFRef cert) {
    CFRef data = default;
    error err = default!;

    var ret = syscall(abi.FuncPCABI0(x509_SecItemExport_trampoline), uintptr(cert), uintptr(kSecFormatX509Cert), 0, 0, uintptr(@unsafe.Pointer(_addr_data)), 0);
    if (ret != 0) {>>MARKER:FUNCTION_x509_SecTrustSettingsCopyCertificates_trampoline_BLOCK_PREFIX<<
        return (0, error.As(new OSStatus("SecItemExport",int32(ret)))!);
    }
    return (data, error.As(null!)!);

}
private static void x509_SecItemExport_trampoline();

private static readonly nint errSecItemNotFound = -25300;

//go:cgo_import_dynamic x509_SecTrustSettingsCopyTrustSettings SecTrustSettingsCopyTrustSettings "/System/Library/Frameworks/Security.framework/Versions/A/Security"



//go:cgo_import_dynamic x509_SecTrustSettingsCopyTrustSettings SecTrustSettingsCopyTrustSettings "/System/Library/Frameworks/Security.framework/Versions/A/Security"

public static (CFRef, error) SecTrustSettingsCopyTrustSettings(CFRef cert, SecTrustSettingsDomain domain) {
    CFRef trustSettings = default;
    error err = default!;

    var ret = syscall(abi.FuncPCABI0(x509_SecTrustSettingsCopyTrustSettings_trampoline), uintptr(cert), uintptr(domain), uintptr(@unsafe.Pointer(_addr_trustSettings)), 0, 0, 0);
    if (int32(ret) == errSecItemNotFound) {>>MARKER:FUNCTION_x509_SecItemExport_trampoline_BLOCK_PREFIX<<
        return (0, error.As(ErrNoTrustSettings)!);
    }
    else if (ret != 0) {
        return (0, error.As(new OSStatus("SecTrustSettingsCopyTrustSettings",int32(ret)))!);
    }
    return (trustSettings, error.As(null!)!);

}
private static void x509_SecTrustSettingsCopyTrustSettings_trampoline();

//go:cgo_import_dynamic x509_SecPolicyCopyProperties SecPolicyCopyProperties "/System/Library/Frameworks/Security.framework/Versions/A/Security"

public static CFRef SecPolicyCopyProperties(CFRef policy) {
    var ret = syscall(abi.FuncPCABI0(x509_SecPolicyCopyProperties_trampoline), uintptr(policy), 0, 0, 0, 0, 0);
    return CFRef(ret);
}
private static void x509_SecPolicyCopyProperties_trampoline();

} // end macOS_package

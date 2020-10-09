// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin,amd64

// package macOS -- go2cs converted at 2020 October 09 04:54:54 UTC
// import "crypto/x509/internal.macOS" ==> using macOS = go.crypto.x509.internal.macOS_package
// Original source: C:\Go\src\crypto\x509\internal\macos\security.go
using errors = go.errors_package;
using strconv = go.strconv_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace crypto {
namespace x509
{
    public static partial class macOS_package
    {
        // Based on https://opensource.apple.com/source/Security/Security-59306.41.2/base/Security.h
        public partial struct SecTrustSettingsResult // : int
        {
        }

        public static readonly SecTrustSettingsResult SecTrustSettingsResultInvalid = (SecTrustSettingsResult)iota;
        public static readonly var SecTrustSettingsResultTrustRoot = 0;
        public static readonly var SecTrustSettingsResultTrustAsRoot = 1;
        public static readonly var SecTrustSettingsResultDeny = 2;
        public static readonly var SecTrustSettingsResultUnspecified = 3;


        public partial struct SecTrustSettingsDomain // : int
        {
        }

        public static readonly SecTrustSettingsDomain SecTrustSettingsDomainUser = (SecTrustSettingsDomain)iota;
        public static readonly var SecTrustSettingsDomainAdmin = 0;
        public static readonly var SecTrustSettingsDomainSystem = 1;


        public partial struct OSStatus
        {
            public @string call;
            public int status;
        }

        public static @string Error(this OSStatus s)
        {
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

        private static readonly long errSecNoTrustSettings = (long)-25263L;

        //go:linkname x509_SecTrustSettingsCopyCertificates x509_SecTrustSettingsCopyCertificates
        //go:cgo_import_dynamic x509_SecTrustSettingsCopyCertificates SecTrustSettingsCopyCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"



        //go:linkname x509_SecTrustSettingsCopyCertificates x509_SecTrustSettingsCopyCertificates
        //go:cgo_import_dynamic x509_SecTrustSettingsCopyCertificates SecTrustSettingsCopyCertificates "/System/Library/Frameworks/Security.framework/Versions/A/Security"

        public static (CFRef, error) SecTrustSettingsCopyCertificates(SecTrustSettingsDomain domain)
        {
            CFRef certArray = default;
            error err = default!;

            var ret = syscall(funcPC(x509_SecTrustSettingsCopyCertificates_trampoline), uintptr(domain), uintptr(@unsafe.Pointer(_addr_certArray)), 0L, 0L, 0L, 0L);
            if (int32(ret) == errSecNoTrustSettings)
            {
                return (0L, error.As(ErrNoTrustSettings)!);
            }
            else if (ret != 0L)
            {
                return (0L, error.As(new OSStatus("SecTrustSettingsCopyCertificates",int32(ret)))!);
            }

            return (certArray, error.As(null!)!);

        }
        private static void x509_SecTrustSettingsCopyCertificates_trampoline()
;

        private static readonly int kSecFormatX509Cert = (int)9L;

        //go:linkname x509_SecItemExport x509_SecItemExport
        //go:cgo_import_dynamic x509_SecItemExport SecItemExport "/System/Library/Frameworks/Security.framework/Versions/A/Security"



        //go:linkname x509_SecItemExport x509_SecItemExport
        //go:cgo_import_dynamic x509_SecItemExport SecItemExport "/System/Library/Frameworks/Security.framework/Versions/A/Security"

        public static (CFRef, error) SecItemExport(CFRef cert)
        {
            CFRef data = default;
            error err = default!;

            var ret = syscall(funcPC(x509_SecItemExport_trampoline), uintptr(cert), uintptr(kSecFormatX509Cert), 0L, 0L, uintptr(@unsafe.Pointer(_addr_data)), 0L);
            if (ret != 0L)
            {>>MARKER:FUNCTION_x509_SecTrustSettingsCopyCertificates_trampoline_BLOCK_PREFIX<<
                return (0L, error.As(new OSStatus("SecItemExport",int32(ret)))!);
            }

            return (data, error.As(null!)!);

        }
        private static void x509_SecItemExport_trampoline()
;

        private static readonly long errSecItemNotFound = (long)-25300L;

        //go:linkname x509_SecTrustSettingsCopyTrustSettings x509_SecTrustSettingsCopyTrustSettings
        //go:cgo_import_dynamic x509_SecTrustSettingsCopyTrustSettings SecTrustSettingsCopyTrustSettings "/System/Library/Frameworks/Security.framework/Versions/A/Security"



        //go:linkname x509_SecTrustSettingsCopyTrustSettings x509_SecTrustSettingsCopyTrustSettings
        //go:cgo_import_dynamic x509_SecTrustSettingsCopyTrustSettings SecTrustSettingsCopyTrustSettings "/System/Library/Frameworks/Security.framework/Versions/A/Security"

        public static (CFRef, error) SecTrustSettingsCopyTrustSettings(CFRef cert, SecTrustSettingsDomain domain)
        {
            CFRef trustSettings = default;
            error err = default!;

            var ret = syscall(funcPC(x509_SecTrustSettingsCopyTrustSettings_trampoline), uintptr(cert), uintptr(domain), uintptr(@unsafe.Pointer(_addr_trustSettings)), 0L, 0L, 0L);
            if (int32(ret) == errSecItemNotFound)
            {>>MARKER:FUNCTION_x509_SecItemExport_trampoline_BLOCK_PREFIX<<
                return (0L, error.As(ErrNoTrustSettings)!);
            }
            else if (ret != 0L)
            {
                return (0L, error.As(new OSStatus("SecTrustSettingsCopyTrustSettings",int32(ret)))!);
            }

            return (trustSettings, error.As(null!)!);

        }
        private static void x509_SecTrustSettingsCopyTrustSettings_trampoline()
;

        //go:linkname x509_SecPolicyCopyProperties x509_SecPolicyCopyProperties
        //go:cgo_import_dynamic x509_SecPolicyCopyProperties SecPolicyCopyProperties "/System/Library/Frameworks/Security.framework/Versions/A/Security"

        public static CFRef SecPolicyCopyProperties(CFRef policy)
        {
            var ret = syscall(funcPC(x509_SecPolicyCopyProperties_trampoline), uintptr(policy), 0L, 0L, 0L, 0L, 0L);
            return CFRef(ret);
        }
        private static void x509_SecPolicyCopyProperties_trampoline()
;
    }
}}}

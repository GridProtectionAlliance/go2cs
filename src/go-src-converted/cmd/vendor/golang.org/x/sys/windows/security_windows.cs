// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2022 March 06 23:30:36 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\windows\security_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

using unsafeheader = go.golang.org.x.sys.@internal.unsafeheader_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class windows_package {

public static readonly nint NameUnknown = 0;
public static readonly nint NameFullyQualifiedDN = 1;
public static readonly nint NameSamCompatible = 2;
public static readonly nint NameDisplay = 3;
public static readonly nint NameUniqueId = 6;
public static readonly nint NameCanonical = 7;
public static readonly nint NameUserPrincipal = 8;
public static readonly nint NameCanonicalEx = 9;
public static readonly nint NameServicePrincipal = 10;
public static readonly nint NameDnsDomain = 12;


// This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
// http://blogs.msdn.com/b/drnick/archive/2007/12/19/windows-and-upn-format-credentials.aspx
//sys    TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
//sys    GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

// TranslateAccountName converts a directory service
// object name from one format to another.
public static (@string, error) TranslateAccountName(@string username, uint from, uint to, nint initSize) {
    @string _p0 = default;
    error _p0 = default!;

    var (u, e) = UTF16PtrFromString(username);
    if (e != null) {
        return ("", error.As(e)!);
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<ushort>(n);
        e = TranslateName(u, from, to, _addr_b[0], _addr_n);
        if (e == null) {
            return (UTF16ToString(b[..(int)n]), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", error.As(e)!);
        }
    }

}

 
// do not reorder
public static readonly var NetSetupUnknownStatus = iota;
public static readonly var NetSetupUnjoined = 0;
public static readonly var NetSetupWorkgroupName = 1;
public static readonly var NetSetupDomainName = 2;


public partial struct UserInfo10 {
    public ptr<ushort> Name;
    public ptr<ushort> Comment;
    public ptr<ushort> UsrComment;
    public ptr<ushort> FullName;
}

//sys    NetUserGetInfo(serverName *uint16, userName *uint16, level uint32, buf **byte) (neterr error) = netapi32.NetUserGetInfo
//sys    NetGetJoinInformation(server *uint16, name **uint16, bufType *uint32) (neterr error) = netapi32.NetGetJoinInformation
//sys    NetApiBufferFree(buf *byte) (neterr error) = netapi32.NetApiBufferFree

 
// do not reorder
public static readonly nint SidTypeUser = 1 + iota;
public static readonly var SidTypeGroup = 0;
public static readonly var SidTypeDomain = 1;
public static readonly var SidTypeAlias = 2;
public static readonly var SidTypeWellKnownGroup = 3;
public static readonly var SidTypeDeletedAccount = 4;
public static readonly var SidTypeInvalid = 5;
public static readonly var SidTypeUnknown = 6;
public static readonly var SidTypeComputer = 7;
public static readonly var SidTypeLabel = 8;


public partial struct SidIdentifierAuthority {
    public array<byte> Value;
}

public static SidIdentifierAuthority SECURITY_NULL_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,0});public static SidIdentifierAuthority SECURITY_WORLD_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,1});public static SidIdentifierAuthority SECURITY_LOCAL_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,2});public static SidIdentifierAuthority SECURITY_CREATOR_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,3});public static SidIdentifierAuthority SECURITY_NON_UNIQUE_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,4});public static SidIdentifierAuthority SECURITY_NT_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,5});public static SidIdentifierAuthority SECURITY_MANDATORY_LABEL_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,16});

public static readonly nint SECURITY_NULL_RID = 0;
public static readonly nint SECURITY_WORLD_RID = 0;
public static readonly nint SECURITY_LOCAL_RID = 0;
public static readonly nint SECURITY_CREATOR_OWNER_RID = 0;
public static readonly nint SECURITY_CREATOR_GROUP_RID = 1;
public static readonly nint SECURITY_DIALUP_RID = 1;
public static readonly nint SECURITY_NETWORK_RID = 2;
public static readonly nint SECURITY_BATCH_RID = 3;
public static readonly nint SECURITY_INTERACTIVE_RID = 4;
public static readonly nint SECURITY_LOGON_IDS_RID = 5;
public static readonly nint SECURITY_SERVICE_RID = 6;
public static readonly nint SECURITY_LOCAL_SYSTEM_RID = 18;
public static readonly nint SECURITY_BUILTIN_DOMAIN_RID = 32;
public static readonly nint SECURITY_PRINCIPAL_SELF_RID = 10;
public static readonly nuint SECURITY_CREATOR_OWNER_SERVER_RID = 0x2;
public static readonly nuint SECURITY_CREATOR_GROUP_SERVER_RID = 0x3;
public static readonly nuint SECURITY_LOGON_IDS_RID_COUNT = 0x3;
public static readonly nuint SECURITY_ANONYMOUS_LOGON_RID = 0x7;
public static readonly nuint SECURITY_PROXY_RID = 0x8;
public static readonly nuint SECURITY_ENTERPRISE_CONTROLLERS_RID = 0x9;
public static readonly var SECURITY_SERVER_LOGON_RID = SECURITY_ENTERPRISE_CONTROLLERS_RID;
public static readonly nuint SECURITY_AUTHENTICATED_USER_RID = 0xb;
public static readonly nuint SECURITY_RESTRICTED_CODE_RID = 0xc;
public static readonly nuint SECURITY_NT_NON_UNIQUE_RID = 0x15;


// Predefined domain-relative RIDs for local groups.
// See https://msdn.microsoft.com/en-us/library/windows/desktop/aa379649(v=vs.85).aspx
public static readonly nuint DOMAIN_ALIAS_RID_ADMINS = 0x220;
public static readonly nuint DOMAIN_ALIAS_RID_USERS = 0x221;
public static readonly nuint DOMAIN_ALIAS_RID_GUESTS = 0x222;
public static readonly nuint DOMAIN_ALIAS_RID_POWER_USERS = 0x223;
public static readonly nuint DOMAIN_ALIAS_RID_ACCOUNT_OPS = 0x224;
public static readonly nuint DOMAIN_ALIAS_RID_SYSTEM_OPS = 0x225;
public static readonly nuint DOMAIN_ALIAS_RID_PRINT_OPS = 0x226;
public static readonly nuint DOMAIN_ALIAS_RID_BACKUP_OPS = 0x227;
public static readonly nuint DOMAIN_ALIAS_RID_REPLICATOR = 0x228;
public static readonly nuint DOMAIN_ALIAS_RID_RAS_SERVERS = 0x229;
public static readonly nuint DOMAIN_ALIAS_RID_PREW2KCOMPACCESS = 0x22a;
public static readonly nuint DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS = 0x22b;
public static readonly nuint DOMAIN_ALIAS_RID_NETWORK_CONFIGURATION_OPS = 0x22c;
public static readonly nuint DOMAIN_ALIAS_RID_INCOMING_FOREST_TRUST_BUILDERS = 0x22d;
public static readonly nuint DOMAIN_ALIAS_RID_MONITORING_USERS = 0x22e;
public static readonly nuint DOMAIN_ALIAS_RID_LOGGING_USERS = 0x22f;
public static readonly nuint DOMAIN_ALIAS_RID_AUTHORIZATIONACCESS = 0x230;
public static readonly nuint DOMAIN_ALIAS_RID_TS_LICENSE_SERVERS = 0x231;
public static readonly nuint DOMAIN_ALIAS_RID_DCOM_USERS = 0x232;
public static readonly nuint DOMAIN_ALIAS_RID_IUSERS = 0x238;
public static readonly nuint DOMAIN_ALIAS_RID_CRYPTO_OPERATORS = 0x239;
public static readonly nuint DOMAIN_ALIAS_RID_CACHEABLE_PRINCIPALS_GROUP = 0x23b;
public static readonly nuint DOMAIN_ALIAS_RID_NON_CACHEABLE_PRINCIPALS_GROUP = 0x23c;
public static readonly nuint DOMAIN_ALIAS_RID_EVENT_LOG_READERS_GROUP = 0x23d;
public static readonly nuint DOMAIN_ALIAS_RID_CERTSVC_DCOM_ACCESS_GROUP = 0x23e;


//sys    LookupAccountSid(systemName *uint16, sid *SID, name *uint16, nameLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountSidW
//sys    LookupAccountName(systemName *uint16, accountName *uint16, sid *SID, sidLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountNameW
//sys    ConvertSidToStringSid(sid *SID, stringSid **uint16) (err error) = advapi32.ConvertSidToStringSidW
//sys    ConvertStringSidToSid(stringSid *uint16, sid **SID) (err error) = advapi32.ConvertStringSidToSidW
//sys    GetLengthSid(sid *SID) (len uint32) = advapi32.GetLengthSid
//sys    CopySid(destSidLen uint32, destSid *SID, srcSid *SID) (err error) = advapi32.CopySid
//sys    AllocateAndInitializeSid(identAuth *SidIdentifierAuthority, subAuth byte, subAuth0 uint32, subAuth1 uint32, subAuth2 uint32, subAuth3 uint32, subAuth4 uint32, subAuth5 uint32, subAuth6 uint32, subAuth7 uint32, sid **SID) (err error) = advapi32.AllocateAndInitializeSid
//sys    createWellKnownSid(sidType WELL_KNOWN_SID_TYPE, domainSid *SID, sid *SID, sizeSid *uint32) (err error) = advapi32.CreateWellKnownSid
//sys    isWellKnownSid(sid *SID, sidType WELL_KNOWN_SID_TYPE) (isWellKnown bool) = advapi32.IsWellKnownSid
//sys    FreeSid(sid *SID) (err error) [failretval!=0] = advapi32.FreeSid
//sys    EqualSid(sid1 *SID, sid2 *SID) (isEqual bool) = advapi32.EqualSid
//sys    getSidIdentifierAuthority(sid *SID) (authority *SidIdentifierAuthority) = advapi32.GetSidIdentifierAuthority
//sys    getSidSubAuthorityCount(sid *SID) (count *uint8) = advapi32.GetSidSubAuthorityCount
//sys    getSidSubAuthority(sid *SID, index uint32) (subAuthority *uint32) = advapi32.GetSidSubAuthority
//sys    isValidSid(sid *SID) (isValid bool) = advapi32.IsValidSid

// The security identifier (SID) structure is a variable-length
// structure used to uniquely identify users or groups.
public partial struct SID {
}

// StringToSid converts a string-format security identifier
// SID into a valid, functional SID.
public static (ptr<SID>, error) StringToSid(@string s) => func((defer, _, _) => {
    ptr<SID> _p0 = default!;
    error _p0 = default!;

    ptr<SID> sid;
    var (p, e) = UTF16PtrFromString(s);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    e = ConvertStringSidToSid(p, _addr_sid);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    defer(LocalFree((Handle)(@unsafe.Pointer(sid))));
    return _addr_sid.Copy()!;

});

// LookupSID retrieves a security identifier SID for the account
// and the name of the domain on which the account was found.
// System specify target computer to search.
public static (ptr<SID>, @string, uint, error) LookupSID(@string system, @string account) {
    ptr<SID> sid = default!;
    @string domain = default;
    uint accType = default;
    error err = default!;

    if (len(account) == 0) {
        return (_addr_null!, "", 0, error.As(syscall.EINVAL)!);
    }
    var (acc, e) = UTF16PtrFromString(account);
    if (e != null) {
        return (_addr_null!, "", 0, error.As(e)!);
    }
    ptr<ushort> sys;
    if (len(system) > 0) {
        sys, e = UTF16PtrFromString(system);
        if (e != null) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    ref var dn = ref heap(uint32(50), out ptr<var> _addr_dn);
    while (true) {
        var b = make_slice<byte>(n);
        var db = make_slice<ushort>(dn);
        sid = (SID.val)(@unsafe.Pointer(_addr_b[0]));
        e = LookupAccountName(sys, acc, sid, _addr_n, _addr_db[0], _addr_dn, _addr_accType);
        if (e == null) {
            return (_addr_sid!, UTF16ToString(db), accType, error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return (_addr_null!, "", 0, error.As(e)!);
        }
    }

}

// String converts SID to a string format suitable for display, storage, or transmission.
private static @string String(this ptr<SID> _addr_sid) => func((defer, _, _) => {
    ref SID sid = ref _addr_sid.val;

    ptr<ushort> s;
    var e = ConvertSidToStringSid(sid, _addr_s);
    if (e != null) {
        return "";
    }
    defer(LocalFree((Handle)(@unsafe.Pointer(s))));
    return UTF16ToString(new ptr<ptr<array<ushort>>>(@unsafe.Pointer(s))[..]);

});

// Len returns the length, in bytes, of a valid security identifier SID.
private static nint Len(this ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return int(GetLengthSid(sid));
}

// Copy creates a duplicate of security identifier SID.
private static (ptr<SID>, error) Copy(this ptr<SID> _addr_sid) {
    ptr<SID> _p0 = default!;
    error _p0 = default!;
    ref SID sid = ref _addr_sid.val;

    var b = make_slice<byte>(sid.Len());
    var sid2 = (SID.val)(@unsafe.Pointer(_addr_b[0]));
    var e = CopySid(uint32(len(b)), sid2, sid);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_sid2!, error.As(null!)!);

}

// IdentifierAuthority returns the identifier authority of the SID.
private static SidIdentifierAuthority IdentifierAuthority(this ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return new ptr<ptr<getSidIdentifierAuthority>>(sid);
}

// SubAuthorityCount returns the number of sub-authorities in the SID.
private static byte SubAuthorityCount(this ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return new ptr<ptr<getSidSubAuthorityCount>>(sid);
}

// SubAuthority returns the sub-authority of the SID as specified by
// the index, which must be less than sid.SubAuthorityCount().
private static uint SubAuthority(this ptr<SID> _addr_sid, uint idx) => func((_, panic, _) => {
    ref SID sid = ref _addr_sid.val;

    if (idx >= uint32(sid.SubAuthorityCount())) {
        panic("sub-authority index out of range");
    }
    return getSidSubAuthority(sid, idx).val;

});

// IsValid returns whether the SID has a valid revision and length.
private static bool IsValid(this ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return isValidSid(sid);
}

// Equals compares two SIDs for equality.
private static bool Equals(this ptr<SID> _addr_sid, ptr<SID> _addr_sid2) {
    ref SID sid = ref _addr_sid.val;
    ref SID sid2 = ref _addr_sid2.val;

    return EqualSid(sid, sid2);
}

// IsWellKnown determines whether the SID matches the well-known sidType.
private static bool IsWellKnown(this ptr<SID> _addr_sid, WELL_KNOWN_SID_TYPE sidType) {
    ref SID sid = ref _addr_sid.val;

    return isWellKnownSid(sid, sidType);
}

// LookupAccount retrieves the name of the account for this SID
// and the name of the first domain on which this SID is found.
// System specify target computer to search for.
private static (@string, @string, uint, error) LookupAccount(this ptr<SID> _addr_sid, @string system) {
    @string account = default;
    @string domain = default;
    uint accType = default;
    error err = default!;
    ref SID sid = ref _addr_sid.val;

    ptr<ushort> sys;
    if (len(system) > 0) {
        sys, err = UTF16PtrFromString(system);
        if (err != null) {
            return ("", "", 0, error.As(err)!);
        }
    }
    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    ref var dn = ref heap(uint32(50), out ptr<var> _addr_dn);
    while (true) {
        var b = make_slice<ushort>(n);
        var db = make_slice<ushort>(dn);
        var e = LookupAccountSid(sys, sid, _addr_b[0], _addr_n, _addr_db[0], _addr_dn, _addr_accType);
        if (e == null) {
            return (UTF16ToString(b), UTF16ToString(db), accType, error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", "", 0, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", "", 0, error.As(e)!);
        }
    }

}

// Various types of pre-specified SIDs that can be synthesized and compared at runtime.
public partial struct WELL_KNOWN_SID_TYPE { // : uint
}

public static readonly nint WinNullSid = 0;
public static readonly nint WinWorldSid = 1;
public static readonly nint WinLocalSid = 2;
public static readonly nint WinCreatorOwnerSid = 3;
public static readonly nint WinCreatorGroupSid = 4;
public static readonly nint WinCreatorOwnerServerSid = 5;
public static readonly nint WinCreatorGroupServerSid = 6;
public static readonly nint WinNtAuthoritySid = 7;
public static readonly nint WinDialupSid = 8;
public static readonly nint WinNetworkSid = 9;
public static readonly nint WinBatchSid = 10;
public static readonly nint WinInteractiveSid = 11;
public static readonly nint WinServiceSid = 12;
public static readonly nint WinAnonymousSid = 13;
public static readonly nint WinProxySid = 14;
public static readonly nint WinEnterpriseControllersSid = 15;
public static readonly nint WinSelfSid = 16;
public static readonly nint WinAuthenticatedUserSid = 17;
public static readonly nint WinRestrictedCodeSid = 18;
public static readonly nint WinTerminalServerSid = 19;
public static readonly nint WinRemoteLogonIdSid = 20;
public static readonly nint WinLogonIdsSid = 21;
public static readonly nint WinLocalSystemSid = 22;
public static readonly nint WinLocalServiceSid = 23;
public static readonly nint WinNetworkServiceSid = 24;
public static readonly nint WinBuiltinDomainSid = 25;
public static readonly nint WinBuiltinAdministratorsSid = 26;
public static readonly nint WinBuiltinUsersSid = 27;
public static readonly nint WinBuiltinGuestsSid = 28;
public static readonly nint WinBuiltinPowerUsersSid = 29;
public static readonly nint WinBuiltinAccountOperatorsSid = 30;
public static readonly nint WinBuiltinSystemOperatorsSid = 31;
public static readonly nint WinBuiltinPrintOperatorsSid = 32;
public static readonly nint WinBuiltinBackupOperatorsSid = 33;
public static readonly nint WinBuiltinReplicatorSid = 34;
public static readonly nint WinBuiltinPreWindows2000CompatibleAccessSid = 35;
public static readonly nint WinBuiltinRemoteDesktopUsersSid = 36;
public static readonly nint WinBuiltinNetworkConfigurationOperatorsSid = 37;
public static readonly nint WinAccountAdministratorSid = 38;
public static readonly nint WinAccountGuestSid = 39;
public static readonly nint WinAccountKrbtgtSid = 40;
public static readonly nint WinAccountDomainAdminsSid = 41;
public static readonly nint WinAccountDomainUsersSid = 42;
public static readonly nint WinAccountDomainGuestsSid = 43;
public static readonly nint WinAccountComputersSid = 44;
public static readonly nint WinAccountControllersSid = 45;
public static readonly nint WinAccountCertAdminsSid = 46;
public static readonly nint WinAccountSchemaAdminsSid = 47;
public static readonly nint WinAccountEnterpriseAdminsSid = 48;
public static readonly nint WinAccountPolicyAdminsSid = 49;
public static readonly nint WinAccountRasAndIasServersSid = 50;
public static readonly nint WinNTLMAuthenticationSid = 51;
public static readonly nint WinDigestAuthenticationSid = 52;
public static readonly nint WinSChannelAuthenticationSid = 53;
public static readonly nint WinThisOrganizationSid = 54;
public static readonly nint WinOtherOrganizationSid = 55;
public static readonly nint WinBuiltinIncomingForestTrustBuildersSid = 56;
public static readonly nint WinBuiltinPerfMonitoringUsersSid = 57;
public static readonly nint WinBuiltinPerfLoggingUsersSid = 58;
public static readonly nint WinBuiltinAuthorizationAccessSid = 59;
public static readonly nint WinBuiltinTerminalServerLicenseServersSid = 60;
public static readonly nint WinBuiltinDCOMUsersSid = 61;
public static readonly nint WinBuiltinIUsersSid = 62;
public static readonly nint WinIUserSid = 63;
public static readonly nint WinBuiltinCryptoOperatorsSid = 64;
public static readonly nint WinUntrustedLabelSid = 65;
public static readonly nint WinLowLabelSid = 66;
public static readonly nint WinMediumLabelSid = 67;
public static readonly nint WinHighLabelSid = 68;
public static readonly nint WinSystemLabelSid = 69;
public static readonly nint WinWriteRestrictedCodeSid = 70;
public static readonly nint WinCreatorOwnerRightsSid = 71;
public static readonly nint WinCacheablePrincipalsGroupSid = 72;
public static readonly nint WinNonCacheablePrincipalsGroupSid = 73;
public static readonly nint WinEnterpriseReadonlyControllersSid = 74;
public static readonly nint WinAccountReadonlyControllersSid = 75;
public static readonly nint WinBuiltinEventLogReadersGroup = 76;
public static readonly nint WinNewEnterpriseReadonlyControllersSid = 77;
public static readonly nint WinBuiltinCertSvcDComAccessGroup = 78;
public static readonly nint WinMediumPlusLabelSid = 79;
public static readonly nint WinLocalLogonSid = 80;
public static readonly nint WinConsoleLogonSid = 81;
public static readonly nint WinThisOrganizationCertificateSid = 82;
public static readonly nint WinApplicationPackageAuthoritySid = 83;
public static readonly nint WinBuiltinAnyPackageSid = 84;
public static readonly nint WinCapabilityInternetClientSid = 85;
public static readonly nint WinCapabilityInternetClientServerSid = 86;
public static readonly nint WinCapabilityPrivateNetworkClientServerSid = 87;
public static readonly nint WinCapabilityPicturesLibrarySid = 88;
public static readonly nint WinCapabilityVideosLibrarySid = 89;
public static readonly nint WinCapabilityMusicLibrarySid = 90;
public static readonly nint WinCapabilityDocumentsLibrarySid = 91;
public static readonly nint WinCapabilitySharedUserCertificatesSid = 92;
public static readonly nint WinCapabilityEnterpriseAuthenticationSid = 93;
public static readonly nint WinCapabilityRemovableStorageSid = 94;
public static readonly nint WinBuiltinRDSRemoteAccessServersSid = 95;
public static readonly nint WinBuiltinRDSEndpointServersSid = 96;
public static readonly nint WinBuiltinRDSManagementServersSid = 97;
public static readonly nint WinUserModeDriversSid = 98;
public static readonly nint WinBuiltinHyperVAdminsSid = 99;
public static readonly nint WinAccountCloneableControllersSid = 100;
public static readonly nint WinBuiltinAccessControlAssistanceOperatorsSid = 101;
public static readonly nint WinBuiltinRemoteManagementUsersSid = 102;
public static readonly nint WinAuthenticationAuthorityAssertedSid = 103;
public static readonly nint WinAuthenticationServiceAssertedSid = 104;
public static readonly nint WinLocalAccountSid = 105;
public static readonly nint WinLocalAccountAndAdministratorSid = 106;
public static readonly nint WinAccountProtectedUsersSid = 107;
public static readonly nint WinCapabilityAppointmentsSid = 108;
public static readonly nint WinCapabilityContactsSid = 109;
public static readonly nint WinAccountDefaultSystemManagedSid = 110;
public static readonly nint WinBuiltinDefaultSystemManagedGroupSid = 111;
public static readonly nint WinBuiltinStorageReplicaAdminsSid = 112;
public static readonly nint WinAccountKeyAdminsSid = 113;
public static readonly nint WinAccountEnterpriseKeyAdminsSid = 114;
public static readonly nint WinAuthenticationKeyTrustSid = 115;
public static readonly nint WinAuthenticationKeyPropertyMFASid = 116;
public static readonly nint WinAuthenticationKeyPropertyAttestationSid = 117;
public static readonly nint WinAuthenticationFreshKeyAuthSid = 118;
public static readonly nint WinBuiltinDeviceOwnersSid = 119;


// Creates a SID for a well-known predefined alias, generally using the constants of the form
// Win*Sid, for the local machine.
public static (ptr<SID>, error) CreateWellKnownSid(WELL_KNOWN_SID_TYPE sidType) {
    ptr<SID> _p0 = default!;
    error _p0 = default!;

    return _addr_CreateWellKnownDomainSid(sidType, _addr_null)!;
}

// Creates a SID for a well-known predefined alias, generally using the constants of the form
// Win*Sid, for the domain specified by the domainSid parameter.
public static (ptr<SID>, error) CreateWellKnownDomainSid(WELL_KNOWN_SID_TYPE sidType, ptr<SID> _addr_domainSid) {
    ptr<SID> _p0 = default!;
    error _p0 = default!;
    ref SID domainSid = ref _addr_domainSid.val;

    ref var n = ref heap(uint32(50), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<byte>(n);
        var sid = (SID.val)(@unsafe.Pointer(_addr_b[0]));
        var err = createWellKnownSid(sidType, domainSid, sid, _addr_n);
        if (err == null) {
            return (_addr_sid!, error.As(null!)!);
        }
        if (err != ERROR_INSUFFICIENT_BUFFER) {
            return (_addr_null!, error.As(err)!);
        }
        if (n <= uint32(len(b))) {
            return (_addr_null!, error.As(err)!);
        }
    }

}

 
// do not reorder
public static readonly nint TOKEN_ASSIGN_PRIMARY = 1 << (int)(iota);
public static readonly var TOKEN_DUPLICATE = 0;
public static readonly var TOKEN_IMPERSONATE = 1;
public static readonly var TOKEN_QUERY = 2;
public static readonly var TOKEN_QUERY_SOURCE = 3;
public static readonly var TOKEN_ADJUST_PRIVILEGES = 4;
public static readonly var TOKEN_ADJUST_GROUPS = 5;
public static readonly var TOKEN_ADJUST_DEFAULT = 6;
public static readonly TOKEN_ALL_ACCESS TOKEN_ADJUST_SESSIONID = STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;
public static readonly var TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
public static readonly var TOKEN_WRITE = STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
public static readonly var TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;


 
// do not reorder
public static readonly nint TokenUser = 1 + iota;
public static readonly var TokenGroups = 0;
public static readonly var TokenPrivileges = 1;
public static readonly var TokenOwner = 2;
public static readonly var TokenPrimaryGroup = 3;
public static readonly var TokenDefaultDacl = 4;
public static readonly var TokenSource = 5;
public static readonly var TokenType = 6;
public static readonly var TokenImpersonationLevel = 7;
public static readonly var TokenStatistics = 8;
public static readonly var TokenRestrictedSids = 9;
public static readonly var TokenSessionId = 10;
public static readonly var TokenGroupsAndPrivileges = 11;
public static readonly var TokenSessionReference = 12;
public static readonly var TokenSandBoxInert = 13;
public static readonly var TokenAuditPolicy = 14;
public static readonly var TokenOrigin = 15;
public static readonly var TokenElevationType = 16;
public static readonly var TokenLinkedToken = 17;
public static readonly var TokenElevation = 18;
public static readonly var TokenHasRestrictions = 19;
public static readonly var TokenAccessInformation = 20;
public static readonly var TokenVirtualizationAllowed = 21;
public static readonly var TokenVirtualizationEnabled = 22;
public static readonly var TokenIntegrityLevel = 23;
public static readonly var TokenUIAccess = 24;
public static readonly var TokenMandatoryPolicy = 25;
public static readonly var TokenLogonSid = 26;
public static readonly var MaxTokenInfoClass = 27;


// Group attributes inside of Tokengroups.Groups[i].Attributes
public static readonly nuint SE_GROUP_MANDATORY = 0x00000001;
public static readonly nuint SE_GROUP_ENABLED_BY_DEFAULT = 0x00000002;
public static readonly nuint SE_GROUP_ENABLED = 0x00000004;
public static readonly nuint SE_GROUP_OWNER = 0x00000008;
public static readonly nuint SE_GROUP_USE_FOR_DENY_ONLY = 0x00000010;
public static readonly nuint SE_GROUP_INTEGRITY = 0x00000020;
public static readonly nuint SE_GROUP_INTEGRITY_ENABLED = 0x00000040;
public static readonly nuint SE_GROUP_LOGON_ID = 0xC0000000;
public static readonly nuint SE_GROUP_RESOURCE = 0x20000000;
public static readonly var SE_GROUP_VALID_ATTRIBUTES = SE_GROUP_MANDATORY | SE_GROUP_ENABLED_BY_DEFAULT | SE_GROUP_ENABLED | SE_GROUP_OWNER | SE_GROUP_USE_FOR_DENY_ONLY | SE_GROUP_LOGON_ID | SE_GROUP_RESOURCE | SE_GROUP_INTEGRITY | SE_GROUP_INTEGRITY_ENABLED;


// Privilege attributes
public static readonly nuint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
public static readonly nuint SE_PRIVILEGE_ENABLED = 0x00000002;
public static readonly nuint SE_PRIVILEGE_REMOVED = 0x00000004;
public static readonly nuint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
public static readonly var SE_PRIVILEGE_VALID_ATTRIBUTES = SE_PRIVILEGE_ENABLED_BY_DEFAULT | SE_PRIVILEGE_ENABLED | SE_PRIVILEGE_REMOVED | SE_PRIVILEGE_USED_FOR_ACCESS;


// Token types
public static readonly nint TokenPrimary = 1;
public static readonly nint TokenImpersonation = 2;


// Impersonation levels
public static readonly nint SecurityAnonymous = 0;
public static readonly nint SecurityIdentification = 1;
public static readonly nint SecurityImpersonation = 2;
public static readonly nint SecurityDelegation = 3;


public partial struct LUID {
    public uint LowPart;
    public int HighPart;
}

public partial struct LUIDAndAttributes {
    public LUID Luid;
    public uint Attributes;
}

public partial struct SIDAndAttributes {
    public ptr<SID> Sid;
    public uint Attributes;
}

public partial struct Tokenuser {
    public SIDAndAttributes User;
}

public partial struct Tokenprimarygroup {
    public ptr<SID> PrimaryGroup;
}

public partial struct Tokengroups {
    public uint GroupCount;
    public array<SIDAndAttributes> Groups; // Use AllGroups() for iterating.
}

// AllGroups returns a slice that can be used to iterate over the groups in g.
private static slice<SIDAndAttributes> AllGroups(this ptr<Tokengroups> _addr_g) {
    ref Tokengroups g = ref _addr_g.val;

    return new ptr<ptr<array<SIDAndAttributes>>>(@unsafe.Pointer(_addr_g.Groups[0])).slice(-1, g.GroupCount, g.GroupCount);
}

public partial struct Tokenprivileges {
    public uint PrivilegeCount;
    public array<LUIDAndAttributes> Privileges; // Use AllPrivileges() for iterating.
}

// AllPrivileges returns a slice that can be used to iterate over the privileges in p.
private static slice<LUIDAndAttributes> AllPrivileges(this ptr<Tokenprivileges> _addr_p) {
    ref Tokenprivileges p = ref _addr_p.val;

    return new ptr<ptr<array<LUIDAndAttributes>>>(@unsafe.Pointer(_addr_p.Privileges[0])).slice(-1, p.PrivilegeCount, p.PrivilegeCount);
}

public partial struct Tokenmandatorylabel {
    public SIDAndAttributes Label;
}

private static uint Size(this ptr<Tokenmandatorylabel> _addr_tml) {
    ref Tokenmandatorylabel tml = ref _addr_tml.val;

    return uint32(@unsafe.Sizeof(new Tokenmandatorylabel())) + GetLengthSid(tml.Label.Sid);
}

// Authorization Functions
//sys    checkTokenMembership(tokenHandle Token, sidToCheck *SID, isMember *int32) (err error) = advapi32.CheckTokenMembership
//sys    isTokenRestricted(tokenHandle Token) (ret bool, err error) [!failretval] = advapi32.IsTokenRestricted
//sys    OpenProcessToken(process Handle, access uint32, token *Token) (err error) = advapi32.OpenProcessToken
//sys    OpenThreadToken(thread Handle, access uint32, openAsSelf bool, token *Token) (err error) = advapi32.OpenThreadToken
//sys    ImpersonateSelf(impersonationlevel uint32) (err error) = advapi32.ImpersonateSelf
//sys    RevertToSelf() (err error) = advapi32.RevertToSelf
//sys    SetThreadToken(thread *Handle, token Token) (err error) = advapi32.SetThreadToken
//sys    LookupPrivilegeValue(systemname *uint16, name *uint16, luid *LUID) (err error) = advapi32.LookupPrivilegeValueW
//sys    AdjustTokenPrivileges(token Token, disableAllPrivileges bool, newstate *Tokenprivileges, buflen uint32, prevstate *Tokenprivileges, returnlen *uint32) (err error) = advapi32.AdjustTokenPrivileges
//sys    AdjustTokenGroups(token Token, resetToDefault bool, newstate *Tokengroups, buflen uint32, prevstate *Tokengroups, returnlen *uint32) (err error) = advapi32.AdjustTokenGroups
//sys    GetTokenInformation(token Token, infoClass uint32, info *byte, infoLen uint32, returnedLen *uint32) (err error) = advapi32.GetTokenInformation
//sys    SetTokenInformation(token Token, infoClass uint32, info *byte, infoLen uint32) (err error) = advapi32.SetTokenInformation
//sys    DuplicateTokenEx(existingToken Token, desiredAccess uint32, tokenAttributes *SecurityAttributes, impersonationLevel uint32, tokenType uint32, newToken *Token) (err error) = advapi32.DuplicateTokenEx
//sys    GetUserProfileDirectory(t Token, dir *uint16, dirLen *uint32) (err error) = userenv.GetUserProfileDirectoryW
//sys    getSystemDirectory(dir *uint16, dirLen uint32) (len uint32, err error) = kernel32.GetSystemDirectoryW
//sys    getWindowsDirectory(dir *uint16, dirLen uint32) (len uint32, err error) = kernel32.GetWindowsDirectoryW
//sys    getSystemWindowsDirectory(dir *uint16, dirLen uint32) (len uint32, err error) = kernel32.GetSystemWindowsDirectoryW

// An access token contains the security information for a logon session.
// The system creates an access token when a user logs on, and every
// process executed on behalf of the user has a copy of the token.
// The token identifies the user, the user's groups, and the user's
// privileges. The system uses the token to control access to securable
// objects and to control the ability of the user to perform various
// system-related operations on the local computer.
public partial struct Token { // : Handle
}

// OpenCurrentProcessToken opens an access token associated with current
// process with TOKEN_QUERY access. It is a real token that needs to be closed.
//
// Deprecated: Explicitly call OpenProcessToken(CurrentProcess(), ...)
// with the desired access instead, or use GetCurrentProcessToken for a
// TOKEN_QUERY token.
public static (Token, error) OpenCurrentProcessToken() {
    Token _p0 = default;
    error _p0 = default!;

    ref Token token = ref heap(out ptr<Token> _addr_token);
    var err = OpenProcessToken(CurrentProcess(), TOKEN_QUERY, _addr_token);
    return (token, error.As(err)!);
}

// GetCurrentProcessToken returns the access token associated with
// the current process. It is a pseudo token that does not need
// to be closed.
public static Token GetCurrentProcessToken() {
    return Token(~uintptr(4 - 1));
}

// GetCurrentThreadToken return the access token associated with
// the current thread. It is a pseudo token that does not need
// to be closed.
public static Token GetCurrentThreadToken() {
    return Token(~uintptr(5 - 1));
}

// GetCurrentThreadEffectiveToken returns the effective access token
// associated with the current thread. It is a pseudo token that does
// not need to be closed.
public static Token GetCurrentThreadEffectiveToken() {
    return Token(~uintptr(6 - 1));
}

// Close releases access to access token.
public static error Close(this Token t) {
    return error.As(CloseHandle(Handle(t)))!;
}

// getInfo retrieves a specified type of information about an access token.
public static (unsafe.Pointer, error) getInfo(this Token t, uint @class, nint initSize) {
    unsafe.Pointer _p0 = default;
    error _p0 = default!;

    ref var n = ref heap(uint32(initSize), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<byte>(n);
        var e = GetTokenInformation(t, class, _addr_b[0], uint32(len(b)), _addr_n);
        if (e == null) {
            return (@unsafe.Pointer(_addr_b[0]), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return (null, error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return (null, error.As(e)!);
        }
    }

}

// GetTokenUser retrieves access token t user account information.
public static (ptr<Tokenuser>, error) GetTokenUser(this Token t) {
    ptr<Tokenuser> _p0 = default!;
    error _p0 = default!;

    var (i, e) = t.getInfo(TokenUser, 50);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_(Tokenuser.val)(i)!, error.As(null!)!);

}

// GetTokenGroups retrieves group accounts associated with access token t.
public static (ptr<Tokengroups>, error) GetTokenGroups(this Token t) {
    ptr<Tokengroups> _p0 = default!;
    error _p0 = default!;

    var (i, e) = t.getInfo(TokenGroups, 50);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_(Tokengroups.val)(i)!, error.As(null!)!);

}

// GetTokenPrimaryGroup retrieves access token t primary group information.
// A pointer to a SID structure representing a group that will become
// the primary group of any objects created by a process using this access token.
public static (ptr<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t) {
    ptr<Tokenprimarygroup> _p0 = default!;
    error _p0 = default!;

    var (i, e) = t.getInfo(TokenPrimaryGroup, 50);
    if (e != null) {
        return (_addr_null!, error.As(e)!);
    }
    return (_addr_(Tokenprimarygroup.val)(i)!, error.As(null!)!);

}

// GetUserProfileDirectory retrieves path to the
// root directory of the access token t user's profile.
public static (@string, error) GetUserProfileDirectory(this Token t) {
    @string _p0 = default;
    error _p0 = default!;

    ref var n = ref heap(uint32(100), out ptr<var> _addr_n);
    while (true) {
        var b = make_slice<ushort>(n);
        var e = GetUserProfileDirectory(t, _addr_b[0], _addr_n);
        if (e == null) {
            return (UTF16ToString(b), error.As(null!)!);
        }
        if (e != ERROR_INSUFFICIENT_BUFFER) {
            return ("", error.As(e)!);
        }
        if (n <= uint32(len(b))) {
            return ("", error.As(e)!);
        }
    }

}

// IsElevated returns whether the current token is elevated from a UAC perspective.
public static bool IsElevated(this Token token) {
    ref uint isElevated = ref heap(out ptr<uint> _addr_isElevated);
    ref uint outLen = ref heap(out ptr<uint> _addr_outLen);
    var err = GetTokenInformation(token, TokenElevation, (byte.val)(@unsafe.Pointer(_addr_isElevated)), uint32(@unsafe.Sizeof(isElevated)), _addr_outLen);
    if (err != null) {
        return false;
    }
    return outLen == uint32(@unsafe.Sizeof(isElevated)) && isElevated != 0;

}

// GetLinkedToken returns the linked token, which may be an elevated UAC token.
public static (Token, error) GetLinkedToken(this Token token) {
    Token _p0 = default;
    error _p0 = default!;

    ref Token linkedToken = ref heap(out ptr<Token> _addr_linkedToken);
    ref uint outLen = ref heap(out ptr<uint> _addr_outLen);
    var err = GetTokenInformation(token, TokenLinkedToken, (byte.val)(@unsafe.Pointer(_addr_linkedToken)), uint32(@unsafe.Sizeof(linkedToken)), _addr_outLen);
    if (err != null) {
        return (Token(0), error.As(err)!);
    }
    return (linkedToken, error.As(null!)!);

}

// GetSystemDirectory retrieves the path to current location of the system
// directory, which is typically, though not always, `C:\Windows\System32`.
public static (@string, error) GetSystemDirectory() {
    @string _p0 = default;
    error _p0 = default!;

    var n = uint32(MAX_PATH);
    while (true) {
        var b = make_slice<ushort>(n);
        var (l, e) = getSystemDirectory(_addr_b[0], n);
        if (e != null) {
            return ("", error.As(e)!);
        }
        if (l <= n) {
            return (UTF16ToString(b[..(int)l]), error.As(null!)!);
        }
        n = l;

    }

}

// GetWindowsDirectory retrieves the path to current location of the Windows
// directory, which is typically, though not always, `C:\Windows`. This may
// be a private user directory in the case that the application is running
// under a terminal server.
public static (@string, error) GetWindowsDirectory() {
    @string _p0 = default;
    error _p0 = default!;

    var n = uint32(MAX_PATH);
    while (true) {
        var b = make_slice<ushort>(n);
        var (l, e) = getWindowsDirectory(_addr_b[0], n);
        if (e != null) {
            return ("", error.As(e)!);
        }
        if (l <= n) {
            return (UTF16ToString(b[..(int)l]), error.As(null!)!);
        }
        n = l;

    }

}

// GetSystemWindowsDirectory retrieves the path to current location of the
// Windows directory, which is typically, though not always, `C:\Windows`.
public static (@string, error) GetSystemWindowsDirectory() {
    @string _p0 = default;
    error _p0 = default!;

    var n = uint32(MAX_PATH);
    while (true) {
        var b = make_slice<ushort>(n);
        var (l, e) = getSystemWindowsDirectory(_addr_b[0], n);
        if (e != null) {
            return ("", error.As(e)!);
        }
        if (l <= n) {
            return (UTF16ToString(b[..(int)l]), error.As(null!)!);
        }
        n = l;

    }

}

// IsMember reports whether the access token t is a member of the provided SID.
public static (bool, error) IsMember(this Token t, ptr<SID> _addr_sid) {
    bool _p0 = default;
    error _p0 = default!;
    ref SID sid = ref _addr_sid.val;

    ref int b = ref heap(out ptr<int> _addr_b);
    {
        var e = checkTokenMembership(t, sid, _addr_b);

        if (e != null) {
            return (false, error.As(e)!);
        }
    }

    return (b != 0, error.As(null!)!);

}

// IsRestricted reports whether the access token t is a restricted token.
public static (bool, error) IsRestricted(this Token t) {
    bool isRestricted = default;
    error err = default!;

    isRestricted, err = isTokenRestricted(t);
    if (!isRestricted && err == syscall.EINVAL) { 
        // If err is EINVAL, this returned ERROR_SUCCESS indicating a non-restricted token.
        err = null;

    }
    return ;

}

public static readonly nuint WTS_CONSOLE_CONNECT = 0x1;
public static readonly nuint WTS_CONSOLE_DISCONNECT = 0x2;
public static readonly nuint WTS_REMOTE_CONNECT = 0x3;
public static readonly nuint WTS_REMOTE_DISCONNECT = 0x4;
public static readonly nuint WTS_SESSION_LOGON = 0x5;
public static readonly nuint WTS_SESSION_LOGOFF = 0x6;
public static readonly nuint WTS_SESSION_LOCK = 0x7;
public static readonly nuint WTS_SESSION_UNLOCK = 0x8;
public static readonly nuint WTS_SESSION_REMOTE_CONTROL = 0x9;
public static readonly nuint WTS_SESSION_CREATE = 0xa;
public static readonly nuint WTS_SESSION_TERMINATE = 0xb;


public static readonly nint WTSActive = 0;
public static readonly nint WTSConnected = 1;
public static readonly nint WTSConnectQuery = 2;
public static readonly nint WTSShadow = 3;
public static readonly nint WTSDisconnected = 4;
public static readonly nint WTSIdle = 5;
public static readonly nint WTSListen = 6;
public static readonly nint WTSReset = 7;
public static readonly nint WTSDown = 8;
public static readonly nint WTSInit = 9;


public partial struct WTSSESSION_NOTIFICATION {
    public uint Size;
    public uint SessionID;
}

public partial struct WTS_SESSION_INFO {
    public uint SessionID;
    public ptr<ushort> WindowStationName;
    public uint State;
}

//sys WTSQueryUserToken(session uint32, token *Token) (err error) = wtsapi32.WTSQueryUserToken
//sys WTSEnumerateSessions(handle Handle, reserved uint32, version uint32, sessions **WTS_SESSION_INFO, count *uint32) (err error) = wtsapi32.WTSEnumerateSessionsW
//sys WTSFreeMemory(ptr uintptr) = wtsapi32.WTSFreeMemory

public partial struct ACL {
    public byte aclRevision;
    public byte sbz1;
    public ushort aclSize;
    public ushort aceCount;
    public ushort sbz2;
}

public partial struct SECURITY_DESCRIPTOR {
    public byte revision;
    public byte sbz1;
    public SECURITY_DESCRIPTOR_CONTROL control;
    public ptr<SID> owner;
    public ptr<SID> group;
    public ptr<ACL> sacl;
    public ptr<ACL> dacl;
}

public partial struct SECURITY_QUALITY_OF_SERVICE {
    public uint Length;
    public uint ImpersonationLevel;
    public byte ContextTrackingMode;
    public byte EffectiveOnly;
}

// Constants for the ContextTrackingMode field of SECURITY_QUALITY_OF_SERVICE.
public static readonly nint SECURITY_STATIC_TRACKING = 0;
public static readonly nint SECURITY_DYNAMIC_TRACKING = 1;


public partial struct SecurityAttributes {
    public uint Length;
    public ptr<SECURITY_DESCRIPTOR> SecurityDescriptor;
    public uint InheritHandle;
}

public partial struct SE_OBJECT_TYPE { // : uint
}

// Constants for type SE_OBJECT_TYPE
public static readonly nint SE_UNKNOWN_OBJECT_TYPE = 0;
public static readonly nint SE_FILE_OBJECT = 1;
public static readonly nint SE_SERVICE = 2;
public static readonly nint SE_PRINTER = 3;
public static readonly nint SE_REGISTRY_KEY = 4;
public static readonly nint SE_LMSHARE = 5;
public static readonly nint SE_KERNEL_OBJECT = 6;
public static readonly nint SE_WINDOW_OBJECT = 7;
public static readonly nint SE_DS_OBJECT = 8;
public static readonly nint SE_DS_OBJECT_ALL = 9;
public static readonly nint SE_PROVIDER_DEFINED_OBJECT = 10;
public static readonly nint SE_WMIGUID_OBJECT = 11;
public static readonly nint SE_REGISTRY_WOW64_32KEY = 12;
public static readonly nint SE_REGISTRY_WOW64_64KEY = 13;


public partial struct SECURITY_INFORMATION { // : uint
}

// Constants for type SECURITY_INFORMATION
public static readonly nuint OWNER_SECURITY_INFORMATION = 0x00000001;
public static readonly nuint GROUP_SECURITY_INFORMATION = 0x00000002;
public static readonly nuint DACL_SECURITY_INFORMATION = 0x00000004;
public static readonly nuint SACL_SECURITY_INFORMATION = 0x00000008;
public static readonly nuint LABEL_SECURITY_INFORMATION = 0x00000010;
public static readonly nuint ATTRIBUTE_SECURITY_INFORMATION = 0x00000020;
public static readonly nuint SCOPE_SECURITY_INFORMATION = 0x00000040;
public static readonly nuint BACKUP_SECURITY_INFORMATION = 0x00010000;
public static readonly nuint PROTECTED_DACL_SECURITY_INFORMATION = 0x80000000;
public static readonly nuint PROTECTED_SACL_SECURITY_INFORMATION = 0x40000000;
public static readonly nuint UNPROTECTED_DACL_SECURITY_INFORMATION = 0x20000000;
public static readonly nuint UNPROTECTED_SACL_SECURITY_INFORMATION = 0x10000000;


public partial struct SECURITY_DESCRIPTOR_CONTROL { // : ushort
}

// Constants for type SECURITY_DESCRIPTOR_CONTROL
public static readonly nuint SE_OWNER_DEFAULTED = 0x0001;
public static readonly nuint SE_GROUP_DEFAULTED = 0x0002;
public static readonly nuint SE_DACL_PRESENT = 0x0004;
public static readonly nuint SE_DACL_DEFAULTED = 0x0008;
public static readonly nuint SE_SACL_PRESENT = 0x0010;
public static readonly nuint SE_SACL_DEFAULTED = 0x0020;
public static readonly nuint SE_DACL_AUTO_INHERIT_REQ = 0x0100;
public static readonly nuint SE_SACL_AUTO_INHERIT_REQ = 0x0200;
public static readonly nuint SE_DACL_AUTO_INHERITED = 0x0400;
public static readonly nuint SE_SACL_AUTO_INHERITED = 0x0800;
public static readonly nuint SE_DACL_PROTECTED = 0x1000;
public static readonly nuint SE_SACL_PROTECTED = 0x2000;
public static readonly nuint SE_RM_CONTROL_VALID = 0x4000;
public static readonly nuint SE_SELF_RELATIVE = 0x8000;


public partial struct ACCESS_MASK { // : uint
}

// Constants for type ACCESS_MASK
public static readonly nuint DELETE = 0x00010000;
public static readonly nuint READ_CONTROL = 0x00020000;
public static readonly nuint WRITE_DAC = 0x00040000;
public static readonly nuint WRITE_OWNER = 0x00080000;
public static readonly nuint SYNCHRONIZE = 0x00100000;
public static readonly nuint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
public static readonly var STANDARD_RIGHTS_READ = READ_CONTROL;
public static readonly var STANDARD_RIGHTS_WRITE = READ_CONTROL;
public static readonly var STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
public static readonly nuint STANDARD_RIGHTS_ALL = 0x001F0000;
public static readonly nuint SPECIFIC_RIGHTS_ALL = 0x0000FFFF;
public static readonly nuint ACCESS_SYSTEM_SECURITY = 0x01000000;
public static readonly nuint MAXIMUM_ALLOWED = 0x02000000;
public static readonly nuint GENERIC_READ = 0x80000000;
public static readonly nuint GENERIC_WRITE = 0x40000000;
public static readonly nuint GENERIC_EXECUTE = 0x20000000;
public static readonly nuint GENERIC_ALL = 0x10000000;


public partial struct ACCESS_MODE { // : uint
}

// Constants for type ACCESS_MODE
public static readonly nint NOT_USED_ACCESS = 0;
public static readonly nint GRANT_ACCESS = 1;
public static readonly nint SET_ACCESS = 2;
public static readonly nint DENY_ACCESS = 3;
public static readonly nint REVOKE_ACCESS = 4;
public static readonly nint SET_AUDIT_SUCCESS = 5;
public static readonly nint SET_AUDIT_FAILURE = 6;


// Constants for AceFlags and Inheritance fields
public static readonly nuint NO_INHERITANCE = 0x0;
public static readonly nuint SUB_OBJECTS_ONLY_INHERIT = 0x1;
public static readonly nuint SUB_CONTAINERS_ONLY_INHERIT = 0x2;
public static readonly nuint SUB_CONTAINERS_AND_OBJECTS_INHERIT = 0x3;
public static readonly nuint INHERIT_NO_PROPAGATE = 0x4;
public static readonly nuint INHERIT_ONLY = 0x8;
public static readonly nuint INHERITED_ACCESS_ENTRY = 0x10;
public static readonly nuint INHERITED_PARENT = 0x10000000;
public static readonly nuint INHERITED_GRANDPARENT = 0x20000000;
public static readonly nuint OBJECT_INHERIT_ACE = 0x1;
public static readonly nuint CONTAINER_INHERIT_ACE = 0x2;
public static readonly nuint NO_PROPAGATE_INHERIT_ACE = 0x4;
public static readonly nuint INHERIT_ONLY_ACE = 0x8;
public static readonly nuint INHERITED_ACE = 0x10;
public static readonly nuint VALID_INHERIT_FLAGS = 0x1F;


public partial struct MULTIPLE_TRUSTEE_OPERATION { // : uint
}

// Constants for MULTIPLE_TRUSTEE_OPERATION
public static readonly nint NO_MULTIPLE_TRUSTEE = 0;
public static readonly nint TRUSTEE_IS_IMPERSONATE = 1;


public partial struct TRUSTEE_FORM { // : uint
}

// Constants for TRUSTEE_FORM
public static readonly nint TRUSTEE_IS_SID = 0;
public static readonly nint TRUSTEE_IS_NAME = 1;
public static readonly nint TRUSTEE_BAD_FORM = 2;
public static readonly nint TRUSTEE_IS_OBJECTS_AND_SID = 3;
public static readonly nint TRUSTEE_IS_OBJECTS_AND_NAME = 4;


public partial struct TRUSTEE_TYPE { // : uint
}

// Constants for TRUSTEE_TYPE
public static readonly nint TRUSTEE_IS_UNKNOWN = 0;
public static readonly nint TRUSTEE_IS_USER = 1;
public static readonly nint TRUSTEE_IS_GROUP = 2;
public static readonly nint TRUSTEE_IS_DOMAIN = 3;
public static readonly nint TRUSTEE_IS_ALIAS = 4;
public static readonly nint TRUSTEE_IS_WELL_KNOWN_GROUP = 5;
public static readonly nint TRUSTEE_IS_DELETED = 6;
public static readonly nint TRUSTEE_IS_INVALID = 7;
public static readonly nint TRUSTEE_IS_COMPUTER = 8;


// Constants for ObjectsPresent field
public static readonly nuint ACE_OBJECT_TYPE_PRESENT = 0x1;
public static readonly nuint ACE_INHERITED_OBJECT_TYPE_PRESENT = 0x2;


public partial struct EXPLICIT_ACCESS {
    public ACCESS_MASK AccessPermissions;
    public ACCESS_MODE AccessMode;
    public uint Inheritance;
    public TRUSTEE Trustee;
}

// This type is the union inside of TRUSTEE and must be created using one of the TrusteeValueFrom* functions.
public partial struct TrusteeValue { // : System.UIntPtr
}

public static TrusteeValue TrusteeValueFromString(@string str) {
    return TrusteeValue(@unsafe.Pointer(StringToUTF16Ptr(str)));
}
public static TrusteeValue TrusteeValueFromSID(ptr<SID> _addr_sid) {
    ref SID sid = ref _addr_sid.val;

    return TrusteeValue(@unsafe.Pointer(sid));
}
public static TrusteeValue TrusteeValueFromObjectsAndSid(ptr<OBJECTS_AND_SID> _addr_objectsAndSid) {
    ref OBJECTS_AND_SID objectsAndSid = ref _addr_objectsAndSid.val;

    return TrusteeValue(@unsafe.Pointer(objectsAndSid));
}
public static TrusteeValue TrusteeValueFromObjectsAndName(ptr<OBJECTS_AND_NAME> _addr_objectsAndName) {
    ref OBJECTS_AND_NAME objectsAndName = ref _addr_objectsAndName.val;

    return TrusteeValue(@unsafe.Pointer(objectsAndName));
}

public partial struct TRUSTEE {
    public ptr<TRUSTEE> MultipleTrustee;
    public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
    public TRUSTEE_FORM TrusteeForm;
    public TRUSTEE_TYPE TrusteeType;
    public TrusteeValue TrusteeValue;
}

public partial struct OBJECTS_AND_SID {
    public uint ObjectsPresent;
    public GUID ObjectTypeGuid;
    public GUID InheritedObjectTypeGuid;
    public ptr<SID> Sid;
}

public partial struct OBJECTS_AND_NAME {
    public uint ObjectsPresent;
    public SE_OBJECT_TYPE ObjectType;
    public ptr<ushort> ObjectTypeName;
    public ptr<ushort> InheritedObjectTypeName;
    public ptr<ushort> Name;
}

//sys    getSecurityInfo(handle Handle, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner **SID, group **SID, dacl **ACL, sacl **ACL, sd **SECURITY_DESCRIPTOR) (ret error) = advapi32.GetSecurityInfo
//sys    SetSecurityInfo(handle Handle, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner *SID, group *SID, dacl *ACL, sacl *ACL) (ret error) = advapi32.SetSecurityInfo
//sys    getNamedSecurityInfo(objectName string, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner **SID, group **SID, dacl **ACL, sacl **ACL, sd **SECURITY_DESCRIPTOR) (ret error) = advapi32.GetNamedSecurityInfoW
//sys    SetNamedSecurityInfo(objectName string, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner *SID, group *SID, dacl *ACL, sacl *ACL) (ret error) = advapi32.SetNamedSecurityInfoW
//sys    SetKernelObjectSecurity(handle Handle, securityInformation SECURITY_INFORMATION, securityDescriptor *SECURITY_DESCRIPTOR) (err error) = advapi32.SetKernelObjectSecurity

//sys    buildSecurityDescriptor(owner *TRUSTEE, group *TRUSTEE, countAccessEntries uint32, accessEntries *EXPLICIT_ACCESS, countAuditEntries uint32, auditEntries *EXPLICIT_ACCESS, oldSecurityDescriptor *SECURITY_DESCRIPTOR, sizeNewSecurityDescriptor *uint32, newSecurityDescriptor **SECURITY_DESCRIPTOR) (ret error) = advapi32.BuildSecurityDescriptorW
//sys    initializeSecurityDescriptor(absoluteSD *SECURITY_DESCRIPTOR, revision uint32) (err error) = advapi32.InitializeSecurityDescriptor

//sys    getSecurityDescriptorControl(sd *SECURITY_DESCRIPTOR, control *SECURITY_DESCRIPTOR_CONTROL, revision *uint32) (err error) = advapi32.GetSecurityDescriptorControl
//sys    getSecurityDescriptorDacl(sd *SECURITY_DESCRIPTOR, daclPresent *bool, dacl **ACL, daclDefaulted *bool) (err error) = advapi32.GetSecurityDescriptorDacl
//sys    getSecurityDescriptorSacl(sd *SECURITY_DESCRIPTOR, saclPresent *bool, sacl **ACL, saclDefaulted *bool) (err error) = advapi32.GetSecurityDescriptorSacl
//sys    getSecurityDescriptorOwner(sd *SECURITY_DESCRIPTOR, owner **SID, ownerDefaulted *bool) (err error) = advapi32.GetSecurityDescriptorOwner
//sys    getSecurityDescriptorGroup(sd *SECURITY_DESCRIPTOR, group **SID, groupDefaulted *bool) (err error) = advapi32.GetSecurityDescriptorGroup
//sys    getSecurityDescriptorLength(sd *SECURITY_DESCRIPTOR) (len uint32) = advapi32.GetSecurityDescriptorLength
//sys    getSecurityDescriptorRMControl(sd *SECURITY_DESCRIPTOR, rmControl *uint8) (ret error) [failretval!=0] = advapi32.GetSecurityDescriptorRMControl
//sys    isValidSecurityDescriptor(sd *SECURITY_DESCRIPTOR) (isValid bool) = advapi32.IsValidSecurityDescriptor

//sys    setSecurityDescriptorControl(sd *SECURITY_DESCRIPTOR, controlBitsOfInterest SECURITY_DESCRIPTOR_CONTROL, controlBitsToSet SECURITY_DESCRIPTOR_CONTROL) (err error) = advapi32.SetSecurityDescriptorControl
//sys    setSecurityDescriptorDacl(sd *SECURITY_DESCRIPTOR, daclPresent bool, dacl *ACL, daclDefaulted bool) (err error) = advapi32.SetSecurityDescriptorDacl
//sys    setSecurityDescriptorSacl(sd *SECURITY_DESCRIPTOR, saclPresent bool, sacl *ACL, saclDefaulted bool) (err error) = advapi32.SetSecurityDescriptorSacl
//sys    setSecurityDescriptorOwner(sd *SECURITY_DESCRIPTOR, owner *SID, ownerDefaulted bool) (err error) = advapi32.SetSecurityDescriptorOwner
//sys    setSecurityDescriptorGroup(sd *SECURITY_DESCRIPTOR, group *SID, groupDefaulted bool) (err error) = advapi32.SetSecurityDescriptorGroup
//sys    setSecurityDescriptorRMControl(sd *SECURITY_DESCRIPTOR, rmControl *uint8) = advapi32.SetSecurityDescriptorRMControl

//sys    convertStringSecurityDescriptorToSecurityDescriptor(str string, revision uint32, sd **SECURITY_DESCRIPTOR, size *uint32) (err error) = advapi32.ConvertStringSecurityDescriptorToSecurityDescriptorW
//sys    convertSecurityDescriptorToStringSecurityDescriptor(sd *SECURITY_DESCRIPTOR, revision uint32, securityInformation SECURITY_INFORMATION, str **uint16, strLen *uint32) (err error) = advapi32.ConvertSecurityDescriptorToStringSecurityDescriptorW

//sys    makeAbsoluteSD(selfRelativeSD *SECURITY_DESCRIPTOR, absoluteSD *SECURITY_DESCRIPTOR, absoluteSDSize *uint32, dacl *ACL, daclSize *uint32, sacl *ACL, saclSize *uint32, owner *SID, ownerSize *uint32, group *SID, groupSize *uint32) (err error) = advapi32.MakeAbsoluteSD
//sys    makeSelfRelativeSD(absoluteSD *SECURITY_DESCRIPTOR, selfRelativeSD *SECURITY_DESCRIPTOR, selfRelativeSDSize *uint32) (err error) = advapi32.MakeSelfRelativeSD

//sys    setEntriesInAcl(countExplicitEntries uint32, explicitEntries *EXPLICIT_ACCESS, oldACL *ACL, newACL **ACL) (ret error) = advapi32.SetEntriesInAclW

// Control returns the security descriptor control bits.
private static (SECURITY_DESCRIPTOR_CONTROL, uint, error) Control(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    SECURITY_DESCRIPTOR_CONTROL control = default;
    uint revision = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    err = getSecurityDescriptorControl(sd, _addr_control, _addr_revision);
    return ;
}

// SetControl sets the security descriptor control bits.
private static error SetControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd, SECURITY_DESCRIPTOR_CONTROL controlBitsOfInterest, SECURITY_DESCRIPTOR_CONTROL controlBitsToSet) {
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    return error.As(setSecurityDescriptorControl(sd, controlBitsOfInterest, controlBitsToSet))!;
}

// RMControl returns the security descriptor resource manager control bits.
private static (byte, error) RMControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    byte control = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    err = getSecurityDescriptorRMControl(sd, _addr_control);
    return ;
}

// SetRMControl sets the security descriptor resource manager control bits.
private static void SetRMControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd, byte rmControl) {
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    setSecurityDescriptorRMControl(sd, _addr_rmControl);
}

// DACL returns the security descriptor DACL and whether it was defaulted. The dacl return value may be nil
// if a DACL exists but is an "empty DACL", meaning fully permissive. If the DACL does not exist, err returns
// ERROR_OBJECT_NOT_FOUND.
private static (ptr<ACL>, bool, error) DACL(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ptr<ACL> dacl = default!;
    bool defaulted = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    ref bool present = ref heap(out ptr<bool> _addr_present);
    err = getSecurityDescriptorDacl(sd, _addr_present, _addr_dacl, _addr_defaulted);
    if (!present) {
        err = ERROR_OBJECT_NOT_FOUND;
    }
    return ;

}

// SetDACL sets the absolute security descriptor DACL.
private static error SetDACL(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<ACL> _addr_dacl, bool present, bool defaulted) {
    ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
    ref ACL dacl = ref _addr_dacl.val;

    return error.As(setSecurityDescriptorDacl(absoluteSD, present, dacl, defaulted))!;
}

// SACL returns the security descriptor SACL and whether it was defaulted. The sacl return value may be nil
// if a SACL exists but is an "empty SACL", meaning fully permissive. If the SACL does not exist, err returns
// ERROR_OBJECT_NOT_FOUND.
private static (ptr<ACL>, bool, error) SACL(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ptr<ACL> sacl = default!;
    bool defaulted = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    ref bool present = ref heap(out ptr<bool> _addr_present);
    err = getSecurityDescriptorSacl(sd, _addr_present, _addr_sacl, _addr_defaulted);
    if (!present) {
        err = ERROR_OBJECT_NOT_FOUND;
    }
    return ;

}

// SetSACL sets the absolute security descriptor SACL.
private static error SetSACL(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<ACL> _addr_sacl, bool present, bool defaulted) {
    ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
    ref ACL sacl = ref _addr_sacl.val;

    return error.As(setSecurityDescriptorSacl(absoluteSD, present, sacl, defaulted))!;
}

// Owner returns the security descriptor owner and whether it was defaulted.
private static (ptr<SID>, bool, error) Owner(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ptr<SID> owner = default!;
    bool defaulted = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    err = getSecurityDescriptorOwner(sd, _addr_owner, _addr_defaulted);
    return ;
}

// SetOwner sets the absolute security descriptor owner.
private static error SetOwner(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<SID> _addr_owner, bool defaulted) {
    ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
    ref SID owner = ref _addr_owner.val;

    return error.As(setSecurityDescriptorOwner(absoluteSD, owner, defaulted))!;
}

// Group returns the security descriptor group and whether it was defaulted.
private static (ptr<SID>, bool, error) Group(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ptr<SID> group = default!;
    bool defaulted = default;
    error err = default!;
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    err = getSecurityDescriptorGroup(sd, _addr_group, _addr_defaulted);
    return ;
}

// SetGroup sets the absolute security descriptor owner.
private static error SetGroup(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<SID> _addr_group, bool defaulted) {
    ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
    ref SID group = ref _addr_group.val;

    return error.As(setSecurityDescriptorGroup(absoluteSD, group, defaulted))!;
}

// Length returns the length of the security descriptor.
private static uint Length(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    return getSecurityDescriptorLength(sd);
}

// IsValid returns whether the security descriptor is valid.
private static bool IsValid(this ptr<SECURITY_DESCRIPTOR> _addr_sd) {
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    return isValidSecurityDescriptor(sd);
}

// String returns the SDDL form of the security descriptor, with a function signature that can be
// used with %v formatting directives.
private static @string String(this ptr<SECURITY_DESCRIPTOR> _addr_sd) => func((defer, _, _) => {
    ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

    ptr<ushort> sddl;
    var err = convertSecurityDescriptorToStringSecurityDescriptor(sd, 1, 0xff, _addr_sddl, null);
    if (err != null) {
        return "";
    }
    defer(LocalFree(Handle(@unsafe.Pointer(sddl))));
    return UTF16PtrToString(sddl);

});

// ToAbsolute converts a self-relative security descriptor into an absolute one.
private static (ptr<SECURITY_DESCRIPTOR>, error) ToAbsolute(this ptr<SECURITY_DESCRIPTOR> _addr_selfRelativeSD) {
    ptr<SECURITY_DESCRIPTOR> absoluteSD = default!;
    error err = default!;
    ref SECURITY_DESCRIPTOR selfRelativeSD = ref _addr_selfRelativeSD.val;

    var (control, _, err) = selfRelativeSD.Control();
    if (err != null) {
        return ;
    }
    if (control & SE_SELF_RELATIVE == 0) {
        err = ERROR_INVALID_PARAMETER;
        return ;
    }
    ref uint absoluteSDSize = ref heap(out ptr<uint> _addr_absoluteSDSize);    ref uint daclSize = ref heap(out ptr<uint> _addr_daclSize);    ref uint saclSize = ref heap(out ptr<uint> _addr_saclSize);    ref uint ownerSize = ref heap(out ptr<uint> _addr_ownerSize);    ref uint groupSize = ref heap(out ptr<uint> _addr_groupSize);

    err = makeAbsoluteSD(selfRelativeSD, null, _addr_absoluteSDSize, null, _addr_daclSize, null, _addr_saclSize, null, _addr_ownerSize, null, _addr_groupSize);

    if (err == ERROR_INSUFFICIENT_BUFFER)     else if (err == null) 
        // makeAbsoluteSD is expected to fail, but it succeeds.
        return (_addr_null!, error.As(ERROR_INTERNAL_ERROR)!);
    else 
        return (_addr_null!, error.As(err)!);
        if (absoluteSDSize > 0) {
        absoluteSD = (SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_make_slice<byte>(absoluteSDSize)[0]));
    }
    ptr<ACL> dacl;    ptr<ACL> sacl;    ptr<SID> owner;    ptr<SID> group;
    if (daclSize > 0) {
        dacl = (ACL.val)(@unsafe.Pointer(_addr_make_slice<byte>(daclSize)[0]));
    }
    if (saclSize > 0) {
        sacl = (ACL.val)(@unsafe.Pointer(_addr_make_slice<byte>(saclSize)[0]));
    }
    if (ownerSize > 0) {
        owner = (SID.val)(@unsafe.Pointer(_addr_make_slice<byte>(ownerSize)[0]));
    }
    if (groupSize > 0) {
        group = (SID.val)(@unsafe.Pointer(_addr_make_slice<byte>(groupSize)[0]));
    }
    err = makeAbsoluteSD(selfRelativeSD, absoluteSD, _addr_absoluteSDSize, dacl, _addr_daclSize, sacl, _addr_saclSize, owner, _addr_ownerSize, group, _addr_groupSize);
    return ;

}

// ToSelfRelative converts an absolute security descriptor into a self-relative one.
private static (ptr<SECURITY_DESCRIPTOR>, error) ToSelfRelative(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD) {
    ptr<SECURITY_DESCRIPTOR> selfRelativeSD = default!;
    error err = default!;
    ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;

    var (control, _, err) = absoluteSD.Control();
    if (err != null) {
        return ;
    }
    if (control & SE_SELF_RELATIVE != 0) {
        err = ERROR_INVALID_PARAMETER;
        return ;
    }
    ref uint selfRelativeSDSize = ref heap(out ptr<uint> _addr_selfRelativeSDSize);
    err = makeSelfRelativeSD(absoluteSD, null, _addr_selfRelativeSDSize);

    if (err == ERROR_INSUFFICIENT_BUFFER)     else if (err == null) 
        // makeSelfRelativeSD is expected to fail, but it succeeds.
        return (_addr_null!, error.As(ERROR_INTERNAL_ERROR)!);
    else 
        return (_addr_null!, error.As(err)!);
        if (selfRelativeSDSize > 0) {
        selfRelativeSD = (SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_make_slice<byte>(selfRelativeSDSize)[0]));
    }
    err = makeSelfRelativeSD(absoluteSD, selfRelativeSD, _addr_selfRelativeSDSize);
    return ;

}

private static ptr<SECURITY_DESCRIPTOR> copySelfRelativeSecurityDescriptor(this ptr<SECURITY_DESCRIPTOR> _addr_selfRelativeSD) {
    ref SECURITY_DESCRIPTOR selfRelativeSD = ref _addr_selfRelativeSD.val;

    var sdLen = int(selfRelativeSD.Length());
    const var min = int(@unsafe.Sizeof(new SECURITY_DESCRIPTOR()));

    if (sdLen < min) {
        sdLen = min;
    }
    ref slice<byte> src = ref heap(out ptr<slice<byte>> _addr_src);
    var h = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_src));
    h.Data = @unsafe.Pointer(selfRelativeSD);
    h.Len = sdLen;
    h.Cap = sdLen;

    const var psize = int(@unsafe.Sizeof(uintptr(0)));



    ref slice<byte> dst = ref heap(out ptr<slice<byte>> _addr_dst);
    h = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_dst));
    ref var alloc = ref heap(make_slice<System.UIntPtr>((sdLen + psize - 1) / psize), out ptr<var> _addr_alloc);
    h.Data = (unsafeheader.Slice.val)(@unsafe.Pointer(_addr_alloc)).Data;
    h.Len = sdLen;
    h.Cap = sdLen;

    copy(dst, src);
    return _addr_(SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_dst[0]))!;

}

// SecurityDescriptorFromString converts an SDDL string describing a security descriptor into a
// self-relative security descriptor object allocated on the Go heap.
public static (ptr<SECURITY_DESCRIPTOR>, error) SecurityDescriptorFromString(@string sddl) => func((defer, _, _) => {
    ptr<SECURITY_DESCRIPTOR> sd = default!;
    error err = default!;

    ptr<SECURITY_DESCRIPTOR> winHeapSD;
    err = convertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, _addr_winHeapSD, null);
    if (err != null) {
        return ;
    }
    defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
    return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

});

// GetSecurityInfo queries the security information for a given handle and returns the self-relative security
// descriptor result on the Go heap.
public static (ptr<SECURITY_DESCRIPTOR>, error) GetSecurityInfo(Handle handle, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) => func((defer, _, _) => {
    ptr<SECURITY_DESCRIPTOR> sd = default!;
    error err = default!;

    ptr<SECURITY_DESCRIPTOR> winHeapSD;
    err = getSecurityInfo(handle, objectType, securityInformation, null, null, null, null, _addr_winHeapSD);
    if (err != null) {
        return ;
    }
    defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
    return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

});

// GetNamedSecurityInfo queries the security information for a given named object and returns the self-relative security
// descriptor result on the Go heap.
public static (ptr<SECURITY_DESCRIPTOR>, error) GetNamedSecurityInfo(@string objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) => func((defer, _, _) => {
    ptr<SECURITY_DESCRIPTOR> sd = default!;
    error err = default!;

    ptr<SECURITY_DESCRIPTOR> winHeapSD;
    err = getNamedSecurityInfo(objectName, objectType, securityInformation, null, null, null, null, _addr_winHeapSD);
    if (err != null) {
        return ;
    }
    defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
    return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

});

// BuildSecurityDescriptor makes a new security descriptor using the input trustees, explicit access lists, and
// prior security descriptor to be merged, any of which can be nil, returning the self-relative security descriptor
// result on the Go heap.
public static (ptr<SECURITY_DESCRIPTOR>, error) BuildSecurityDescriptor(ptr<TRUSTEE> _addr_owner, ptr<TRUSTEE> _addr_group, slice<EXPLICIT_ACCESS> accessEntries, slice<EXPLICIT_ACCESS> auditEntries, ptr<SECURITY_DESCRIPTOR> _addr_mergedSecurityDescriptor) => func((defer, _, _) => {
    ptr<SECURITY_DESCRIPTOR> sd = default!;
    error err = default!;
    ref TRUSTEE owner = ref _addr_owner.val;
    ref TRUSTEE group = ref _addr_group.val;
    ref SECURITY_DESCRIPTOR mergedSecurityDescriptor = ref _addr_mergedSecurityDescriptor.val;

    ptr<SECURITY_DESCRIPTOR> winHeapSD;
    ref uint winHeapSDSize = ref heap(out ptr<uint> _addr_winHeapSDSize);
    ptr<EXPLICIT_ACCESS> firstAccessEntry;
    if (len(accessEntries) > 0) {
        firstAccessEntry = _addr_accessEntries[0];
    }
    ptr<EXPLICIT_ACCESS> firstAuditEntry;
    if (len(auditEntries) > 0) {
        firstAuditEntry = _addr_auditEntries[0];
    }
    err = buildSecurityDescriptor(owner, group, uint32(len(accessEntries)), firstAccessEntry, uint32(len(auditEntries)), firstAuditEntry, mergedSecurityDescriptor, _addr_winHeapSDSize, _addr_winHeapSD);
    if (err != null) {
        return ;
    }
    defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
    return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

});

// NewSecurityDescriptor creates and initializes a new absolute security descriptor.
public static (ptr<SECURITY_DESCRIPTOR>, error) NewSecurityDescriptor() {
    ptr<SECURITY_DESCRIPTOR> absoluteSD = default!;
    error err = default!;

    absoluteSD = addr(new SECURITY_DESCRIPTOR());
    err = initializeSecurityDescriptor(absoluteSD, 1);
    return ;
}

// ACLFromEntries returns a new ACL on the Go heap containing a list of explicit entries as well as those of another ACL.
// Both explicitEntries and mergedACL are optional and can be nil.
public static (ptr<ACL>, error) ACLFromEntries(slice<EXPLICIT_ACCESS> explicitEntries, ptr<ACL> _addr_mergedACL) => func((defer, _, _) => {
    ptr<ACL> acl = default!;
    error err = default!;
    ref ACL mergedACL = ref _addr_mergedACL.val;

    ptr<EXPLICIT_ACCESS> firstExplicitEntry;
    if (len(explicitEntries) > 0) {
        firstExplicitEntry = _addr_explicitEntries[0];
    }
    ptr<ACL> winHeapACL;
    err = setEntriesInAcl(uint32(len(explicitEntries)), firstExplicitEntry, mergedACL, _addr_winHeapACL);
    if (err != null) {
        return ;
    }
    defer(LocalFree(Handle(@unsafe.Pointer(winHeapACL))));
    var aclBytes = make_slice<byte>(winHeapACL.aclSize);
    copy(aclBytes, new ptr<ptr<array<byte>>>(@unsafe.Pointer(winHeapACL)).slice(-1, len(aclBytes), len(aclBytes)));
    return (_addr_(ACL.val)(@unsafe.Pointer(_addr_aclBytes[0]))!, error.As(null!)!);

});

} // end windows_package

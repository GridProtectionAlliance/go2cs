// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 09 06:00:52 UTC
// import "cmd/vendor/golang.org/x/sys/windows" ==> using windows = go.cmd.vendor.golang.org.x.sys.windows_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\windows\security_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        public static readonly long NameUnknown = (long)0L;
        public static readonly long NameFullyQualifiedDN = (long)1L;
        public static readonly long NameSamCompatible = (long)2L;
        public static readonly long NameDisplay = (long)3L;
        public static readonly long NameUniqueId = (long)6L;
        public static readonly long NameCanonical = (long)7L;
        public static readonly long NameUserPrincipal = (long)8L;
        public static readonly long NameCanonicalEx = (long)9L;
        public static readonly long NameServicePrincipal = (long)10L;
        public static readonly long NameDnsDomain = (long)12L;


        // This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
        // http://blogs.msdn.com/b/drnick/archive/2007/12/19/windows-and-upn-format-credentials.aspx
        //sys    TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
        //sys    GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

        // TranslateAccountName converts a directory service
        // object name from one format to another.
        public static (@string, error) TranslateAccountName(@string username, uint from, uint to, long initSize)
        {
            @string _p0 = default;
            error _p0 = default!;

            var (u, e) = UTF16PtrFromString(username);
            if (e != null)
            {
                return ("", error.As(e)!);
            }

            ref var n = ref heap(uint32(50L), out ptr<var> _addr_n);
            while (true)
            {
                var b = make_slice<ushort>(n);
                e = TranslateName(u, from, to, _addr_b[0L], _addr_n);
                if (e == null)
                {
                    return (UTF16ToString(b[..n]), error.As(null!)!);
                }

                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", error.As(e)!);
                }

                if (n <= uint32(len(b)))
                {
                    return ("", error.As(e)!);
                }

            }


        }

 
        // do not reorder
        public static readonly var NetSetupUnknownStatus = iota;
        public static readonly var NetSetupUnjoined = 0;
        public static readonly var NetSetupWorkgroupName = 1;
        public static readonly var NetSetupDomainName = 2;


        public partial struct UserInfo10
        {
            public ptr<ushort> Name;
            public ptr<ushort> Comment;
            public ptr<ushort> UsrComment;
            public ptr<ushort> FullName;
        }

        //sys    NetUserGetInfo(serverName *uint16, userName *uint16, level uint32, buf **byte) (neterr error) = netapi32.NetUserGetInfo
        //sys    NetGetJoinInformation(server *uint16, name **uint16, bufType *uint32) (neterr error) = netapi32.NetGetJoinInformation
        //sys    NetApiBufferFree(buf *byte) (neterr error) = netapi32.NetApiBufferFree

 
        // do not reorder
        public static readonly long SidTypeUser = (long)1L + iota;
        public static readonly var SidTypeGroup = 0;
        public static readonly var SidTypeDomain = 1;
        public static readonly var SidTypeAlias = 2;
        public static readonly var SidTypeWellKnownGroup = 3;
        public static readonly var SidTypeDeletedAccount = 4;
        public static readonly var SidTypeInvalid = 5;
        public static readonly var SidTypeUnknown = 6;
        public static readonly var SidTypeComputer = 7;
        public static readonly var SidTypeLabel = 8;


        public partial struct SidIdentifierAuthority
        {
            public array<byte> Value;
        }

        public static SidIdentifierAuthority SECURITY_NULL_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,0});        public static SidIdentifierAuthority SECURITY_WORLD_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,1});        public static SidIdentifierAuthority SECURITY_LOCAL_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,2});        public static SidIdentifierAuthority SECURITY_CREATOR_SID_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,3});        public static SidIdentifierAuthority SECURITY_NON_UNIQUE_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,4});        public static SidIdentifierAuthority SECURITY_NT_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,5});        public static SidIdentifierAuthority SECURITY_MANDATORY_LABEL_AUTHORITY = new SidIdentifierAuthority([6]byte{0,0,0,0,0,16});

        public static readonly long SECURITY_NULL_RID = (long)0L;
        public static readonly long SECURITY_WORLD_RID = (long)0L;
        public static readonly long SECURITY_LOCAL_RID = (long)0L;
        public static readonly long SECURITY_CREATOR_OWNER_RID = (long)0L;
        public static readonly long SECURITY_CREATOR_GROUP_RID = (long)1L;
        public static readonly long SECURITY_DIALUP_RID = (long)1L;
        public static readonly long SECURITY_NETWORK_RID = (long)2L;
        public static readonly long SECURITY_BATCH_RID = (long)3L;
        public static readonly long SECURITY_INTERACTIVE_RID = (long)4L;
        public static readonly long SECURITY_LOGON_IDS_RID = (long)5L;
        public static readonly long SECURITY_SERVICE_RID = (long)6L;
        public static readonly long SECURITY_LOCAL_SYSTEM_RID = (long)18L;
        public static readonly long SECURITY_BUILTIN_DOMAIN_RID = (long)32L;
        public static readonly long SECURITY_PRINCIPAL_SELF_RID = (long)10L;
        public static readonly ulong SECURITY_CREATOR_OWNER_SERVER_RID = (ulong)0x2UL;
        public static readonly ulong SECURITY_CREATOR_GROUP_SERVER_RID = (ulong)0x3UL;
        public static readonly ulong SECURITY_LOGON_IDS_RID_COUNT = (ulong)0x3UL;
        public static readonly ulong SECURITY_ANONYMOUS_LOGON_RID = (ulong)0x7UL;
        public static readonly ulong SECURITY_PROXY_RID = (ulong)0x8UL;
        public static readonly ulong SECURITY_ENTERPRISE_CONTROLLERS_RID = (ulong)0x9UL;
        public static readonly var SECURITY_SERVER_LOGON_RID = SECURITY_ENTERPRISE_CONTROLLERS_RID;
        public static readonly ulong SECURITY_AUTHENTICATED_USER_RID = (ulong)0xbUL;
        public static readonly ulong SECURITY_RESTRICTED_CODE_RID = (ulong)0xcUL;
        public static readonly ulong SECURITY_NT_NON_UNIQUE_RID = (ulong)0x15UL;


        // Predefined domain-relative RIDs for local groups.
        // See https://msdn.microsoft.com/en-us/library/windows/desktop/aa379649(v=vs.85).aspx
        public static readonly ulong DOMAIN_ALIAS_RID_ADMINS = (ulong)0x220UL;
        public static readonly ulong DOMAIN_ALIAS_RID_USERS = (ulong)0x221UL;
        public static readonly ulong DOMAIN_ALIAS_RID_GUESTS = (ulong)0x222UL;
        public static readonly ulong DOMAIN_ALIAS_RID_POWER_USERS = (ulong)0x223UL;
        public static readonly ulong DOMAIN_ALIAS_RID_ACCOUNT_OPS = (ulong)0x224UL;
        public static readonly ulong DOMAIN_ALIAS_RID_SYSTEM_OPS = (ulong)0x225UL;
        public static readonly ulong DOMAIN_ALIAS_RID_PRINT_OPS = (ulong)0x226UL;
        public static readonly ulong DOMAIN_ALIAS_RID_BACKUP_OPS = (ulong)0x227UL;
        public static readonly ulong DOMAIN_ALIAS_RID_REPLICATOR = (ulong)0x228UL;
        public static readonly ulong DOMAIN_ALIAS_RID_RAS_SERVERS = (ulong)0x229UL;
        public static readonly ulong DOMAIN_ALIAS_RID_PREW2KCOMPACCESS = (ulong)0x22aUL;
        public static readonly ulong DOMAIN_ALIAS_RID_REMOTE_DESKTOP_USERS = (ulong)0x22bUL;
        public static readonly ulong DOMAIN_ALIAS_RID_NETWORK_CONFIGURATION_OPS = (ulong)0x22cUL;
        public static readonly ulong DOMAIN_ALIAS_RID_INCOMING_FOREST_TRUST_BUILDERS = (ulong)0x22dUL;
        public static readonly ulong DOMAIN_ALIAS_RID_MONITORING_USERS = (ulong)0x22eUL;
        public static readonly ulong DOMAIN_ALIAS_RID_LOGGING_USERS = (ulong)0x22fUL;
        public static readonly ulong DOMAIN_ALIAS_RID_AUTHORIZATIONACCESS = (ulong)0x230UL;
        public static readonly ulong DOMAIN_ALIAS_RID_TS_LICENSE_SERVERS = (ulong)0x231UL;
        public static readonly ulong DOMAIN_ALIAS_RID_DCOM_USERS = (ulong)0x232UL;
        public static readonly ulong DOMAIN_ALIAS_RID_IUSERS = (ulong)0x238UL;
        public static readonly ulong DOMAIN_ALIAS_RID_CRYPTO_OPERATORS = (ulong)0x239UL;
        public static readonly ulong DOMAIN_ALIAS_RID_CACHEABLE_PRINCIPALS_GROUP = (ulong)0x23bUL;
        public static readonly ulong DOMAIN_ALIAS_RID_NON_CACHEABLE_PRINCIPALS_GROUP = (ulong)0x23cUL;
        public static readonly ulong DOMAIN_ALIAS_RID_EVENT_LOG_READERS_GROUP = (ulong)0x23dUL;
        public static readonly ulong DOMAIN_ALIAS_RID_CERTSVC_DCOM_ACCESS_GROUP = (ulong)0x23eUL;


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
        public partial struct SID
        {
        }

        // StringToSid converts a string-format security identifier
        // SID into a valid, functional SID.
        public static (ptr<SID>, error) StringToSid(@string s) => func((defer, _, __) =>
        {
            ptr<SID> _p0 = default!;
            error _p0 = default!;

            ptr<SID> sid;
            var (p, e) = UTF16PtrFromString(s);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            e = ConvertStringSidToSid(p, _addr_sid);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            defer(LocalFree((Handle)(@unsafe.Pointer(sid))));
            return _addr_sid.Copy()!;

        });

        // LookupSID retrieves a security identifier SID for the account
        // and the name of the domain on which the account was found.
        // System specify target computer to search.
        public static (ptr<SID>, @string, uint, error) LookupSID(@string system, @string account)
        {
            ptr<SID> sid = default!;
            @string domain = default;
            uint accType = default;
            error err = default!;

            if (len(account) == 0L)
            {
                return (_addr_null!, "", 0L, error.As(syscall.EINVAL)!);
            }

            var (acc, e) = UTF16PtrFromString(account);
            if (e != null)
            {
                return (_addr_null!, "", 0L, error.As(e)!);
            }

            ptr<ushort> sys;
            if (len(system) > 0L)
            {
                sys, e = UTF16PtrFromString(system);
                if (e != null)
                {
                    return (_addr_null!, "", 0L, error.As(e)!);
                }

            }

            ref var n = ref heap(uint32(50L), out ptr<var> _addr_n);
            ref var dn = ref heap(uint32(50L), out ptr<var> _addr_dn);
            while (true)
            {
                var b = make_slice<byte>(n);
                var db = make_slice<ushort>(dn);
                sid = (SID.val)(@unsafe.Pointer(_addr_b[0L]));
                e = LookupAccountName(sys, acc, sid, _addr_n, _addr_db[0L], _addr_dn, _addr_accType);
                if (e == null)
                {
                    return (_addr_sid!, UTF16ToString(db), accType, error.As(null!)!);
                }

                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return (_addr_null!, "", 0L, error.As(e)!);
                }

                if (n <= uint32(len(b)))
                {
                    return (_addr_null!, "", 0L, error.As(e)!);
                }

            }


        }

        // String converts SID to a string format suitable for display, storage, or transmission.
        private static @string String(this ptr<SID> _addr_sid) => func((defer, _, __) =>
        {
            ref SID sid = ref _addr_sid.val;

            ptr<ushort> s;
            var e = ConvertSidToStringSid(sid, _addr_s);
            if (e != null)
            {
                return "";
            }

            defer(LocalFree((Handle)(@unsafe.Pointer(s))));
            return UTF16ToString(new ptr<ptr<array<ushort>>>(@unsafe.Pointer(s))[..]);

        });

        // Len returns the length, in bytes, of a valid security identifier SID.
        private static long Len(this ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return int(GetLengthSid(sid));
        }

        // Copy creates a duplicate of security identifier SID.
        private static (ptr<SID>, error) Copy(this ptr<SID> _addr_sid)
        {
            ptr<SID> _p0 = default!;
            error _p0 = default!;
            ref SID sid = ref _addr_sid.val;

            var b = make_slice<byte>(sid.Len());
            var sid2 = (SID.val)(@unsafe.Pointer(_addr_b[0L]));
            var e = CopySid(uint32(len(b)), sid2, sid);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return (_addr_sid2!, error.As(null!)!);

        }

        // IdentifierAuthority returns the identifier authority of the SID.
        private static SidIdentifierAuthority IdentifierAuthority(this ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return new ptr<ptr<getSidIdentifierAuthority>>(sid);
        }

        // SubAuthorityCount returns the number of sub-authorities in the SID.
        private static byte SubAuthorityCount(this ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return new ptr<ptr<getSidSubAuthorityCount>>(sid);
        }

        // SubAuthority returns the sub-authority of the SID as specified by
        // the index, which must be less than sid.SubAuthorityCount().
        private static uint SubAuthority(this ptr<SID> _addr_sid, uint idx) => func((_, panic, __) =>
        {
            ref SID sid = ref _addr_sid.val;

            if (idx >= uint32(sid.SubAuthorityCount()))
            {
                panic("sub-authority index out of range");
            }

            return getSidSubAuthority(sid, idx).val;

        });

        // IsValid returns whether the SID has a valid revision and length.
        private static bool IsValid(this ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return isValidSid(sid);
        }

        // Equals compares two SIDs for equality.
        private static bool Equals(this ptr<SID> _addr_sid, ptr<SID> _addr_sid2)
        {
            ref SID sid = ref _addr_sid.val;
            ref SID sid2 = ref _addr_sid2.val;

            return EqualSid(sid, sid2);
        }

        // IsWellKnown determines whether the SID matches the well-known sidType.
        private static bool IsWellKnown(this ptr<SID> _addr_sid, WELL_KNOWN_SID_TYPE sidType)
        {
            ref SID sid = ref _addr_sid.val;

            return isWellKnownSid(sid, sidType);
        }

        // LookupAccount retrieves the name of the account for this SID
        // and the name of the first domain on which this SID is found.
        // System specify target computer to search for.
        private static (@string, @string, uint, error) LookupAccount(this ptr<SID> _addr_sid, @string system)
        {
            @string account = default;
            @string domain = default;
            uint accType = default;
            error err = default!;
            ref SID sid = ref _addr_sid.val;

            ptr<ushort> sys;
            if (len(system) > 0L)
            {
                sys, err = UTF16PtrFromString(system);
                if (err != null)
                {
                    return ("", "", 0L, error.As(err)!);
                }

            }

            ref var n = ref heap(uint32(50L), out ptr<var> _addr_n);
            ref var dn = ref heap(uint32(50L), out ptr<var> _addr_dn);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var db = make_slice<ushort>(dn);
                var e = LookupAccountSid(sys, sid, _addr_b[0L], _addr_n, _addr_db[0L], _addr_dn, _addr_accType);
                if (e == null)
                {
                    return (UTF16ToString(b), UTF16ToString(db), accType, error.As(null!)!);
                }

                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", "", 0L, error.As(e)!);
                }

                if (n <= uint32(len(b)))
                {
                    return ("", "", 0L, error.As(e)!);
                }

            }


        }

        // Various types of pre-specified SIDs that can be synthesized and compared at runtime.
        public partial struct WELL_KNOWN_SID_TYPE // : uint
        {
        }

        public static readonly long WinNullSid = (long)0L;
        public static readonly long WinWorldSid = (long)1L;
        public static readonly long WinLocalSid = (long)2L;
        public static readonly long WinCreatorOwnerSid = (long)3L;
        public static readonly long WinCreatorGroupSid = (long)4L;
        public static readonly long WinCreatorOwnerServerSid = (long)5L;
        public static readonly long WinCreatorGroupServerSid = (long)6L;
        public static readonly long WinNtAuthoritySid = (long)7L;
        public static readonly long WinDialupSid = (long)8L;
        public static readonly long WinNetworkSid = (long)9L;
        public static readonly long WinBatchSid = (long)10L;
        public static readonly long WinInteractiveSid = (long)11L;
        public static readonly long WinServiceSid = (long)12L;
        public static readonly long WinAnonymousSid = (long)13L;
        public static readonly long WinProxySid = (long)14L;
        public static readonly long WinEnterpriseControllersSid = (long)15L;
        public static readonly long WinSelfSid = (long)16L;
        public static readonly long WinAuthenticatedUserSid = (long)17L;
        public static readonly long WinRestrictedCodeSid = (long)18L;
        public static readonly long WinTerminalServerSid = (long)19L;
        public static readonly long WinRemoteLogonIdSid = (long)20L;
        public static readonly long WinLogonIdsSid = (long)21L;
        public static readonly long WinLocalSystemSid = (long)22L;
        public static readonly long WinLocalServiceSid = (long)23L;
        public static readonly long WinNetworkServiceSid = (long)24L;
        public static readonly long WinBuiltinDomainSid = (long)25L;
        public static readonly long WinBuiltinAdministratorsSid = (long)26L;
        public static readonly long WinBuiltinUsersSid = (long)27L;
        public static readonly long WinBuiltinGuestsSid = (long)28L;
        public static readonly long WinBuiltinPowerUsersSid = (long)29L;
        public static readonly long WinBuiltinAccountOperatorsSid = (long)30L;
        public static readonly long WinBuiltinSystemOperatorsSid = (long)31L;
        public static readonly long WinBuiltinPrintOperatorsSid = (long)32L;
        public static readonly long WinBuiltinBackupOperatorsSid = (long)33L;
        public static readonly long WinBuiltinReplicatorSid = (long)34L;
        public static readonly long WinBuiltinPreWindows2000CompatibleAccessSid = (long)35L;
        public static readonly long WinBuiltinRemoteDesktopUsersSid = (long)36L;
        public static readonly long WinBuiltinNetworkConfigurationOperatorsSid = (long)37L;
        public static readonly long WinAccountAdministratorSid = (long)38L;
        public static readonly long WinAccountGuestSid = (long)39L;
        public static readonly long WinAccountKrbtgtSid = (long)40L;
        public static readonly long WinAccountDomainAdminsSid = (long)41L;
        public static readonly long WinAccountDomainUsersSid = (long)42L;
        public static readonly long WinAccountDomainGuestsSid = (long)43L;
        public static readonly long WinAccountComputersSid = (long)44L;
        public static readonly long WinAccountControllersSid = (long)45L;
        public static readonly long WinAccountCertAdminsSid = (long)46L;
        public static readonly long WinAccountSchemaAdminsSid = (long)47L;
        public static readonly long WinAccountEnterpriseAdminsSid = (long)48L;
        public static readonly long WinAccountPolicyAdminsSid = (long)49L;
        public static readonly long WinAccountRasAndIasServersSid = (long)50L;
        public static readonly long WinNTLMAuthenticationSid = (long)51L;
        public static readonly long WinDigestAuthenticationSid = (long)52L;
        public static readonly long WinSChannelAuthenticationSid = (long)53L;
        public static readonly long WinThisOrganizationSid = (long)54L;
        public static readonly long WinOtherOrganizationSid = (long)55L;
        public static readonly long WinBuiltinIncomingForestTrustBuildersSid = (long)56L;
        public static readonly long WinBuiltinPerfMonitoringUsersSid = (long)57L;
        public static readonly long WinBuiltinPerfLoggingUsersSid = (long)58L;
        public static readonly long WinBuiltinAuthorizationAccessSid = (long)59L;
        public static readonly long WinBuiltinTerminalServerLicenseServersSid = (long)60L;
        public static readonly long WinBuiltinDCOMUsersSid = (long)61L;
        public static readonly long WinBuiltinIUsersSid = (long)62L;
        public static readonly long WinIUserSid = (long)63L;
        public static readonly long WinBuiltinCryptoOperatorsSid = (long)64L;
        public static readonly long WinUntrustedLabelSid = (long)65L;
        public static readonly long WinLowLabelSid = (long)66L;
        public static readonly long WinMediumLabelSid = (long)67L;
        public static readonly long WinHighLabelSid = (long)68L;
        public static readonly long WinSystemLabelSid = (long)69L;
        public static readonly long WinWriteRestrictedCodeSid = (long)70L;
        public static readonly long WinCreatorOwnerRightsSid = (long)71L;
        public static readonly long WinCacheablePrincipalsGroupSid = (long)72L;
        public static readonly long WinNonCacheablePrincipalsGroupSid = (long)73L;
        public static readonly long WinEnterpriseReadonlyControllersSid = (long)74L;
        public static readonly long WinAccountReadonlyControllersSid = (long)75L;
        public static readonly long WinBuiltinEventLogReadersGroup = (long)76L;
        public static readonly long WinNewEnterpriseReadonlyControllersSid = (long)77L;
        public static readonly long WinBuiltinCertSvcDComAccessGroup = (long)78L;
        public static readonly long WinMediumPlusLabelSid = (long)79L;
        public static readonly long WinLocalLogonSid = (long)80L;
        public static readonly long WinConsoleLogonSid = (long)81L;
        public static readonly long WinThisOrganizationCertificateSid = (long)82L;
        public static readonly long WinApplicationPackageAuthoritySid = (long)83L;
        public static readonly long WinBuiltinAnyPackageSid = (long)84L;
        public static readonly long WinCapabilityInternetClientSid = (long)85L;
        public static readonly long WinCapabilityInternetClientServerSid = (long)86L;
        public static readonly long WinCapabilityPrivateNetworkClientServerSid = (long)87L;
        public static readonly long WinCapabilityPicturesLibrarySid = (long)88L;
        public static readonly long WinCapabilityVideosLibrarySid = (long)89L;
        public static readonly long WinCapabilityMusicLibrarySid = (long)90L;
        public static readonly long WinCapabilityDocumentsLibrarySid = (long)91L;
        public static readonly long WinCapabilitySharedUserCertificatesSid = (long)92L;
        public static readonly long WinCapabilityEnterpriseAuthenticationSid = (long)93L;
        public static readonly long WinCapabilityRemovableStorageSid = (long)94L;
        public static readonly long WinBuiltinRDSRemoteAccessServersSid = (long)95L;
        public static readonly long WinBuiltinRDSEndpointServersSid = (long)96L;
        public static readonly long WinBuiltinRDSManagementServersSid = (long)97L;
        public static readonly long WinUserModeDriversSid = (long)98L;
        public static readonly long WinBuiltinHyperVAdminsSid = (long)99L;
        public static readonly long WinAccountCloneableControllersSid = (long)100L;
        public static readonly long WinBuiltinAccessControlAssistanceOperatorsSid = (long)101L;
        public static readonly long WinBuiltinRemoteManagementUsersSid = (long)102L;
        public static readonly long WinAuthenticationAuthorityAssertedSid = (long)103L;
        public static readonly long WinAuthenticationServiceAssertedSid = (long)104L;
        public static readonly long WinLocalAccountSid = (long)105L;
        public static readonly long WinLocalAccountAndAdministratorSid = (long)106L;
        public static readonly long WinAccountProtectedUsersSid = (long)107L;
        public static readonly long WinCapabilityAppointmentsSid = (long)108L;
        public static readonly long WinCapabilityContactsSid = (long)109L;
        public static readonly long WinAccountDefaultSystemManagedSid = (long)110L;
        public static readonly long WinBuiltinDefaultSystemManagedGroupSid = (long)111L;
        public static readonly long WinBuiltinStorageReplicaAdminsSid = (long)112L;
        public static readonly long WinAccountKeyAdminsSid = (long)113L;
        public static readonly long WinAccountEnterpriseKeyAdminsSid = (long)114L;
        public static readonly long WinAuthenticationKeyTrustSid = (long)115L;
        public static readonly long WinAuthenticationKeyPropertyMFASid = (long)116L;
        public static readonly long WinAuthenticationKeyPropertyAttestationSid = (long)117L;
        public static readonly long WinAuthenticationFreshKeyAuthSid = (long)118L;
        public static readonly long WinBuiltinDeviceOwnersSid = (long)119L;


        // Creates a SID for a well-known predefined alias, generally using the constants of the form
        // Win*Sid, for the local machine.
        public static (ptr<SID>, error) CreateWellKnownSid(WELL_KNOWN_SID_TYPE sidType)
        {
            ptr<SID> _p0 = default!;
            error _p0 = default!;

            return _addr_CreateWellKnownDomainSid(sidType, _addr_null)!;
        }

        // Creates a SID for a well-known predefined alias, generally using the constants of the form
        // Win*Sid, for the domain specified by the domainSid parameter.
        public static (ptr<SID>, error) CreateWellKnownDomainSid(WELL_KNOWN_SID_TYPE sidType, ptr<SID> _addr_domainSid)
        {
            ptr<SID> _p0 = default!;
            error _p0 = default!;
            ref SID domainSid = ref _addr_domainSid.val;

            ref var n = ref heap(uint32(50L), out ptr<var> _addr_n);
            while (true)
            {
                var b = make_slice<byte>(n);
                var sid = (SID.val)(@unsafe.Pointer(_addr_b[0L]));
                var err = createWellKnownSid(sidType, domainSid, sid, _addr_n);
                if (err == null)
                {
                    return (_addr_sid!, error.As(null!)!);
                }

                if (err != ERROR_INSUFFICIENT_BUFFER)
                {
                    return (_addr_null!, error.As(err)!);
                }

                if (n <= uint32(len(b)))
                {
                    return (_addr_null!, error.As(err)!);
                }

            }


        }

 
        // do not reorder
        public static readonly long TOKEN_ASSIGN_PRIMARY = (long)1L << (int)(iota);
        public static readonly var TOKEN_DUPLICATE = 0;
        public static readonly var TOKEN_IMPERSONATE = 1;
        public static readonly var TOKEN_QUERY = 2;
        public static readonly var TOKEN_QUERY_SOURCE = 3;
        public static readonly var TOKEN_ADJUST_PRIVILEGES = 4;
        public static readonly var TOKEN_ADJUST_GROUPS = 5;
        public static readonly var TOKEN_ADJUST_DEFAULT = 6;
        public static readonly TOKEN_ALL_ACCESS TOKEN_ADJUST_SESSIONID = (TOKEN_ALL_ACCESS)STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;
        public static readonly var TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
        public static readonly var TOKEN_WRITE = STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
        public static readonly var TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;


 
        // do not reorder
        public static readonly long TokenUser = (long)1L + iota;
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
        public static readonly ulong SE_GROUP_MANDATORY = (ulong)0x00000001UL;
        public static readonly ulong SE_GROUP_ENABLED_BY_DEFAULT = (ulong)0x00000002UL;
        public static readonly ulong SE_GROUP_ENABLED = (ulong)0x00000004UL;
        public static readonly ulong SE_GROUP_OWNER = (ulong)0x00000008UL;
        public static readonly ulong SE_GROUP_USE_FOR_DENY_ONLY = (ulong)0x00000010UL;
        public static readonly ulong SE_GROUP_INTEGRITY = (ulong)0x00000020UL;
        public static readonly ulong SE_GROUP_INTEGRITY_ENABLED = (ulong)0x00000040UL;
        public static readonly ulong SE_GROUP_LOGON_ID = (ulong)0xC0000000UL;
        public static readonly ulong SE_GROUP_RESOURCE = (ulong)0x20000000UL;
        public static readonly var SE_GROUP_VALID_ATTRIBUTES = SE_GROUP_MANDATORY | SE_GROUP_ENABLED_BY_DEFAULT | SE_GROUP_ENABLED | SE_GROUP_OWNER | SE_GROUP_USE_FOR_DENY_ONLY | SE_GROUP_LOGON_ID | SE_GROUP_RESOURCE | SE_GROUP_INTEGRITY | SE_GROUP_INTEGRITY_ENABLED;


        // Privilege attributes
        public static readonly ulong SE_PRIVILEGE_ENABLED_BY_DEFAULT = (ulong)0x00000001UL;
        public static readonly ulong SE_PRIVILEGE_ENABLED = (ulong)0x00000002UL;
        public static readonly ulong SE_PRIVILEGE_REMOVED = (ulong)0x00000004UL;
        public static readonly ulong SE_PRIVILEGE_USED_FOR_ACCESS = (ulong)0x80000000UL;
        public static readonly var SE_PRIVILEGE_VALID_ATTRIBUTES = SE_PRIVILEGE_ENABLED_BY_DEFAULT | SE_PRIVILEGE_ENABLED | SE_PRIVILEGE_REMOVED | SE_PRIVILEGE_USED_FOR_ACCESS;


        // Token types
        public static readonly long TokenPrimary = (long)1L;
        public static readonly long TokenImpersonation = (long)2L;


        // Impersonation levels
        public static readonly long SecurityAnonymous = (long)0L;
        public static readonly long SecurityIdentification = (long)1L;
        public static readonly long SecurityImpersonation = (long)2L;
        public static readonly long SecurityDelegation = (long)3L;


        public partial struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        public partial struct LUIDAndAttributes
        {
            public LUID Luid;
            public uint Attributes;
        }

        public partial struct SIDAndAttributes
        {
            public ptr<SID> Sid;
            public uint Attributes;
        }

        public partial struct Tokenuser
        {
            public SIDAndAttributes User;
        }

        public partial struct Tokenprimarygroup
        {
            public ptr<SID> PrimaryGroup;
        }

        public partial struct Tokengroups
        {
            public uint GroupCount;
            public array<SIDAndAttributes> Groups; // Use AllGroups() for iterating.
        }

        // AllGroups returns a slice that can be used to iterate over the groups in g.
        private static slice<SIDAndAttributes> AllGroups(this ptr<Tokengroups> _addr_g)
        {
            ref Tokengroups g = ref _addr_g.val;

            return new ptr<ptr<array<SIDAndAttributes>>>(@unsafe.Pointer(_addr_g.Groups[0L])).slice(-1, g.GroupCount, g.GroupCount);
        }

        public partial struct Tokenprivileges
        {
            public uint PrivilegeCount;
            public array<LUIDAndAttributes> Privileges; // Use AllPrivileges() for iterating.
        }

        // AllPrivileges returns a slice that can be used to iterate over the privileges in p.
        private static slice<LUIDAndAttributes> AllPrivileges(this ptr<Tokenprivileges> _addr_p)
        {
            ref Tokenprivileges p = ref _addr_p.val;

            return new ptr<ptr<array<LUIDAndAttributes>>>(@unsafe.Pointer(_addr_p.Privileges[0L])).slice(-1, p.PrivilegeCount, p.PrivilegeCount);
        }

        public partial struct Tokenmandatorylabel
        {
            public SIDAndAttributes Label;
        }

        private static uint Size(this ptr<Tokenmandatorylabel> _addr_tml)
        {
            ref Tokenmandatorylabel tml = ref _addr_tml.val;

            return uint32(@unsafe.Sizeof(new Tokenmandatorylabel())) + GetLengthSid(tml.Label.Sid);
        }

        // Authorization Functions
        //sys    checkTokenMembership(tokenHandle Token, sidToCheck *SID, isMember *int32) (err error) = advapi32.CheckTokenMembership
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
        public partial struct Token // : Handle
        {
        }

        // OpenCurrentProcessToken opens an access token associated with current
        // process with TOKEN_QUERY access. It is a real token that needs to be closed.
        //
        // Deprecated: Explicitly call OpenProcessToken(CurrentProcess(), ...)
        // with the desired access instead, or use GetCurrentProcessToken for a
        // TOKEN_QUERY token.
        public static (Token, error) OpenCurrentProcessToken()
        {
            Token _p0 = default;
            error _p0 = default!;

            ref Token token = ref heap(out ptr<Token> _addr_token);
            var err = OpenProcessToken(CurrentProcess(), TOKEN_QUERY, _addr_token);
            return (token, error.As(err)!);
        }

        // GetCurrentProcessToken returns the access token associated with
        // the current process. It is a pseudo token that does not need
        // to be closed.
        public static Token GetCurrentProcessToken()
        {
            return Token(~uintptr(4L - 1L));
        }

        // GetCurrentThreadToken return the access token associated with
        // the current thread. It is a pseudo token that does not need
        // to be closed.
        public static Token GetCurrentThreadToken()
        {
            return Token(~uintptr(5L - 1L));
        }

        // GetCurrentThreadEffectiveToken returns the effective access token
        // associated with the current thread. It is a pseudo token that does
        // not need to be closed.
        public static Token GetCurrentThreadEffectiveToken()
        {
            return Token(~uintptr(6L - 1L));
        }

        // Close releases access to access token.
        public static error Close(this Token t)
        {
            return error.As(CloseHandle(Handle(t)))!;
        }

        // getInfo retrieves a specified type of information about an access token.
        public static (unsafe.Pointer, error) getInfo(this Token t, uint @class, long initSize)
        {
            unsafe.Pointer _p0 = default;
            error _p0 = default!;

            ref var n = ref heap(uint32(initSize), out ptr<var> _addr_n);
            while (true)
            {
                var b = make_slice<byte>(n);
                var e = GetTokenInformation(t, class, _addr_b[0L], uint32(len(b)), _addr_n);
                if (e == null)
                {
                    return (@unsafe.Pointer(_addr_b[0L]), error.As(null!)!);
                }

                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return (null, error.As(e)!);
                }

                if (n <= uint32(len(b)))
                {
                    return (null, error.As(e)!);
                }

            }


        }

        // GetTokenUser retrieves access token t user account information.
        public static (ptr<Tokenuser>, error) GetTokenUser(this Token t)
        {
            ptr<Tokenuser> _p0 = default!;
            error _p0 = default!;

            var (i, e) = t.getInfo(TokenUser, 50L);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return (_addr_(Tokenuser.val)(i)!, error.As(null!)!);

        }

        // GetTokenGroups retrieves group accounts associated with access token t.
        public static (ptr<Tokengroups>, error) GetTokenGroups(this Token t)
        {
            ptr<Tokengroups> _p0 = default!;
            error _p0 = default!;

            var (i, e) = t.getInfo(TokenGroups, 50L);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return (_addr_(Tokengroups.val)(i)!, error.As(null!)!);

        }

        // GetTokenPrimaryGroup retrieves access token t primary group information.
        // A pointer to a SID structure representing a group that will become
        // the primary group of any objects created by a process using this access token.
        public static (ptr<Tokenprimarygroup>, error) GetTokenPrimaryGroup(this Token t)
        {
            ptr<Tokenprimarygroup> _p0 = default!;
            error _p0 = default!;

            var (i, e) = t.getInfo(TokenPrimaryGroup, 50L);
            if (e != null)
            {
                return (_addr_null!, error.As(e)!);
            }

            return (_addr_(Tokenprimarygroup.val)(i)!, error.As(null!)!);

        }

        // GetUserProfileDirectory retrieves path to the
        // root directory of the access token t user's profile.
        public static (@string, error) GetUserProfileDirectory(this Token t)
        {
            @string _p0 = default;
            error _p0 = default!;

            ref var n = ref heap(uint32(100L), out ptr<var> _addr_n);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var e = GetUserProfileDirectory(t, _addr_b[0L], _addr_n);
                if (e == null)
                {
                    return (UTF16ToString(b), error.As(null!)!);
                }

                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", error.As(e)!);
                }

                if (n <= uint32(len(b)))
                {
                    return ("", error.As(e)!);
                }

            }


        }

        // IsElevated returns whether the current token is elevated from a UAC perspective.
        public static bool IsElevated(this Token token)
        {
            ref uint isElevated = ref heap(out ptr<uint> _addr_isElevated);
            ref uint outLen = ref heap(out ptr<uint> _addr_outLen);
            var err = GetTokenInformation(token, TokenElevation, (byte.val)(@unsafe.Pointer(_addr_isElevated)), uint32(@unsafe.Sizeof(isElevated)), _addr_outLen);
            if (err != null)
            {
                return false;
            }

            return outLen == uint32(@unsafe.Sizeof(isElevated)) && isElevated != 0L;

        }

        // GetLinkedToken returns the linked token, which may be an elevated UAC token.
        public static (Token, error) GetLinkedToken(this Token token)
        {
            Token _p0 = default;
            error _p0 = default!;

            ref Token linkedToken = ref heap(out ptr<Token> _addr_linkedToken);
            ref uint outLen = ref heap(out ptr<uint> _addr_outLen);
            var err = GetTokenInformation(token, TokenLinkedToken, (byte.val)(@unsafe.Pointer(_addr_linkedToken)), uint32(@unsafe.Sizeof(linkedToken)), _addr_outLen);
            if (err != null)
            {
                return (Token(0L), error.As(err)!);
            }

            return (linkedToken, error.As(null!)!);

        }

        // GetSystemDirectory retrieves the path to current location of the system
        // directory, which is typically, though not always, `C:\Windows\System32`.
        public static (@string, error) GetSystemDirectory()
        {
            @string _p0 = default;
            error _p0 = default!;

            var n = uint32(MAX_PATH);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var (l, e) = getSystemDirectory(_addr_b[0L], n);
                if (e != null)
                {
                    return ("", error.As(e)!);
                }

                if (l <= n)
                {
                    return (UTF16ToString(b[..l]), error.As(null!)!);
                }

                n = l;

            }


        }

        // GetWindowsDirectory retrieves the path to current location of the Windows
        // directory, which is typically, though not always, `C:\Windows`. This may
        // be a private user directory in the case that the application is running
        // under a terminal server.
        public static (@string, error) GetWindowsDirectory()
        {
            @string _p0 = default;
            error _p0 = default!;

            var n = uint32(MAX_PATH);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var (l, e) = getWindowsDirectory(_addr_b[0L], n);
                if (e != null)
                {
                    return ("", error.As(e)!);
                }

                if (l <= n)
                {
                    return (UTF16ToString(b[..l]), error.As(null!)!);
                }

                n = l;

            }


        }

        // GetSystemWindowsDirectory retrieves the path to current location of the
        // Windows directory, which is typically, though not always, `C:\Windows`.
        public static (@string, error) GetSystemWindowsDirectory()
        {
            @string _p0 = default;
            error _p0 = default!;

            var n = uint32(MAX_PATH);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var (l, e) = getSystemWindowsDirectory(_addr_b[0L], n);
                if (e != null)
                {
                    return ("", error.As(e)!);
                }

                if (l <= n)
                {
                    return (UTF16ToString(b[..l]), error.As(null!)!);
                }

                n = l;

            }


        }

        // IsMember reports whether the access token t is a member of the provided SID.
        public static (bool, error) IsMember(this Token t, ptr<SID> _addr_sid)
        {
            bool _p0 = default;
            error _p0 = default!;
            ref SID sid = ref _addr_sid.val;

            ref int b = ref heap(out ptr<int> _addr_b);
            {
                var e = checkTokenMembership(t, sid, _addr_b);

                if (e != null)
                {
                    return (false, error.As(e)!);
                }

            }

            return (b != 0L, error.As(null!)!);

        }

        public static readonly ulong WTS_CONSOLE_CONNECT = (ulong)0x1UL;
        public static readonly ulong WTS_CONSOLE_DISCONNECT = (ulong)0x2UL;
        public static readonly ulong WTS_REMOTE_CONNECT = (ulong)0x3UL;
        public static readonly ulong WTS_REMOTE_DISCONNECT = (ulong)0x4UL;
        public static readonly ulong WTS_SESSION_LOGON = (ulong)0x5UL;
        public static readonly ulong WTS_SESSION_LOGOFF = (ulong)0x6UL;
        public static readonly ulong WTS_SESSION_LOCK = (ulong)0x7UL;
        public static readonly ulong WTS_SESSION_UNLOCK = (ulong)0x8UL;
        public static readonly ulong WTS_SESSION_REMOTE_CONTROL = (ulong)0x9UL;
        public static readonly ulong WTS_SESSION_CREATE = (ulong)0xaUL;
        public static readonly ulong WTS_SESSION_TERMINATE = (ulong)0xbUL;


        public static readonly long WTSActive = (long)0L;
        public static readonly long WTSConnected = (long)1L;
        public static readonly long WTSConnectQuery = (long)2L;
        public static readonly long WTSShadow = (long)3L;
        public static readonly long WTSDisconnected = (long)4L;
        public static readonly long WTSIdle = (long)5L;
        public static readonly long WTSListen = (long)6L;
        public static readonly long WTSReset = (long)7L;
        public static readonly long WTSDown = (long)8L;
        public static readonly long WTSInit = (long)9L;


        public partial struct WTSSESSION_NOTIFICATION
        {
            public uint Size;
            public uint SessionID;
        }

        public partial struct WTS_SESSION_INFO
        {
            public uint SessionID;
            public ptr<ushort> WindowStationName;
            public uint State;
        }

        //sys WTSQueryUserToken(session uint32, token *Token) (err error) = wtsapi32.WTSQueryUserToken
        //sys WTSEnumerateSessions(handle Handle, reserved uint32, version uint32, sessions **WTS_SESSION_INFO, count *uint32) (err error) = wtsapi32.WTSEnumerateSessionsW
        //sys WTSFreeMemory(ptr uintptr) = wtsapi32.WTSFreeMemory

        public partial struct ACL
        {
            public byte aclRevision;
            public byte sbz1;
            public ushort aclSize;
            public ushort aceCount;
            public ushort sbz2;
        }

        public partial struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte sbz1;
            public SECURITY_DESCRIPTOR_CONTROL control;
            public ptr<SID> owner;
            public ptr<SID> group;
            public ptr<ACL> sacl;
            public ptr<ACL> dacl;
        }

        public partial struct SecurityAttributes
        {
            public uint Length;
            public ptr<SECURITY_DESCRIPTOR> SecurityDescriptor;
            public uint InheritHandle;
        }

        public partial struct SE_OBJECT_TYPE // : uint
        {
        }

        // Constants for type SE_OBJECT_TYPE
        public static readonly long SE_UNKNOWN_OBJECT_TYPE = (long)0L;
        public static readonly long SE_FILE_OBJECT = (long)1L;
        public static readonly long SE_SERVICE = (long)2L;
        public static readonly long SE_PRINTER = (long)3L;
        public static readonly long SE_REGISTRY_KEY = (long)4L;
        public static readonly long SE_LMSHARE = (long)5L;
        public static readonly long SE_KERNEL_OBJECT = (long)6L;
        public static readonly long SE_WINDOW_OBJECT = (long)7L;
        public static readonly long SE_DS_OBJECT = (long)8L;
        public static readonly long SE_DS_OBJECT_ALL = (long)9L;
        public static readonly long SE_PROVIDER_DEFINED_OBJECT = (long)10L;
        public static readonly long SE_WMIGUID_OBJECT = (long)11L;
        public static readonly long SE_REGISTRY_WOW64_32KEY = (long)12L;
        public static readonly long SE_REGISTRY_WOW64_64KEY = (long)13L;


        public partial struct SECURITY_INFORMATION // : uint
        {
        }

        // Constants for type SECURITY_INFORMATION
        public static readonly ulong OWNER_SECURITY_INFORMATION = (ulong)0x00000001UL;
        public static readonly ulong GROUP_SECURITY_INFORMATION = (ulong)0x00000002UL;
        public static readonly ulong DACL_SECURITY_INFORMATION = (ulong)0x00000004UL;
        public static readonly ulong SACL_SECURITY_INFORMATION = (ulong)0x00000008UL;
        public static readonly ulong LABEL_SECURITY_INFORMATION = (ulong)0x00000010UL;
        public static readonly ulong ATTRIBUTE_SECURITY_INFORMATION = (ulong)0x00000020UL;
        public static readonly ulong SCOPE_SECURITY_INFORMATION = (ulong)0x00000040UL;
        public static readonly ulong BACKUP_SECURITY_INFORMATION = (ulong)0x00010000UL;
        public static readonly ulong PROTECTED_DACL_SECURITY_INFORMATION = (ulong)0x80000000UL;
        public static readonly ulong PROTECTED_SACL_SECURITY_INFORMATION = (ulong)0x40000000UL;
        public static readonly ulong UNPROTECTED_DACL_SECURITY_INFORMATION = (ulong)0x20000000UL;
        public static readonly ulong UNPROTECTED_SACL_SECURITY_INFORMATION = (ulong)0x10000000UL;


        public partial struct SECURITY_DESCRIPTOR_CONTROL // : ushort
        {
        }

        // Constants for type SECURITY_DESCRIPTOR_CONTROL
        public static readonly ulong SE_OWNER_DEFAULTED = (ulong)0x0001UL;
        public static readonly ulong SE_GROUP_DEFAULTED = (ulong)0x0002UL;
        public static readonly ulong SE_DACL_PRESENT = (ulong)0x0004UL;
        public static readonly ulong SE_DACL_DEFAULTED = (ulong)0x0008UL;
        public static readonly ulong SE_SACL_PRESENT = (ulong)0x0010UL;
        public static readonly ulong SE_SACL_DEFAULTED = (ulong)0x0020UL;
        public static readonly ulong SE_DACL_AUTO_INHERIT_REQ = (ulong)0x0100UL;
        public static readonly ulong SE_SACL_AUTO_INHERIT_REQ = (ulong)0x0200UL;
        public static readonly ulong SE_DACL_AUTO_INHERITED = (ulong)0x0400UL;
        public static readonly ulong SE_SACL_AUTO_INHERITED = (ulong)0x0800UL;
        public static readonly ulong SE_DACL_PROTECTED = (ulong)0x1000UL;
        public static readonly ulong SE_SACL_PROTECTED = (ulong)0x2000UL;
        public static readonly ulong SE_RM_CONTROL_VALID = (ulong)0x4000UL;
        public static readonly ulong SE_SELF_RELATIVE = (ulong)0x8000UL;


        public partial struct ACCESS_MASK // : uint
        {
        }

        // Constants for type ACCESS_MASK
        public static readonly ulong DELETE = (ulong)0x00010000UL;
        public static readonly ulong READ_CONTROL = (ulong)0x00020000UL;
        public static readonly ulong WRITE_DAC = (ulong)0x00040000UL;
        public static readonly ulong WRITE_OWNER = (ulong)0x00080000UL;
        public static readonly ulong SYNCHRONIZE = (ulong)0x00100000UL;
        public static readonly ulong STANDARD_RIGHTS_REQUIRED = (ulong)0x000F0000UL;
        public static readonly var STANDARD_RIGHTS_READ = READ_CONTROL;
        public static readonly var STANDARD_RIGHTS_WRITE = READ_CONTROL;
        public static readonly var STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
        public static readonly ulong STANDARD_RIGHTS_ALL = (ulong)0x001F0000UL;
        public static readonly ulong SPECIFIC_RIGHTS_ALL = (ulong)0x0000FFFFUL;
        public static readonly ulong ACCESS_SYSTEM_SECURITY = (ulong)0x01000000UL;
        public static readonly ulong MAXIMUM_ALLOWED = (ulong)0x02000000UL;
        public static readonly ulong GENERIC_READ = (ulong)0x80000000UL;
        public static readonly ulong GENERIC_WRITE = (ulong)0x40000000UL;
        public static readonly ulong GENERIC_EXECUTE = (ulong)0x20000000UL;
        public static readonly ulong GENERIC_ALL = (ulong)0x10000000UL;


        public partial struct ACCESS_MODE // : uint
        {
        }

        // Constants for type ACCESS_MODE
        public static readonly long NOT_USED_ACCESS = (long)0L;
        public static readonly long GRANT_ACCESS = (long)1L;
        public static readonly long SET_ACCESS = (long)2L;
        public static readonly long DENY_ACCESS = (long)3L;
        public static readonly long REVOKE_ACCESS = (long)4L;
        public static readonly long SET_AUDIT_SUCCESS = (long)5L;
        public static readonly long SET_AUDIT_FAILURE = (long)6L;


        // Constants for AceFlags and Inheritance fields
        public static readonly ulong NO_INHERITANCE = (ulong)0x0UL;
        public static readonly ulong SUB_OBJECTS_ONLY_INHERIT = (ulong)0x1UL;
        public static readonly ulong SUB_CONTAINERS_ONLY_INHERIT = (ulong)0x2UL;
        public static readonly ulong SUB_CONTAINERS_AND_OBJECTS_INHERIT = (ulong)0x3UL;
        public static readonly ulong INHERIT_NO_PROPAGATE = (ulong)0x4UL;
        public static readonly ulong INHERIT_ONLY = (ulong)0x8UL;
        public static readonly ulong INHERITED_ACCESS_ENTRY = (ulong)0x10UL;
        public static readonly ulong INHERITED_PARENT = (ulong)0x10000000UL;
        public static readonly ulong INHERITED_GRANDPARENT = (ulong)0x20000000UL;
        public static readonly ulong OBJECT_INHERIT_ACE = (ulong)0x1UL;
        public static readonly ulong CONTAINER_INHERIT_ACE = (ulong)0x2UL;
        public static readonly ulong NO_PROPAGATE_INHERIT_ACE = (ulong)0x4UL;
        public static readonly ulong INHERIT_ONLY_ACE = (ulong)0x8UL;
        public static readonly ulong INHERITED_ACE = (ulong)0x10UL;
        public static readonly ulong VALID_INHERIT_FLAGS = (ulong)0x1FUL;


        public partial struct MULTIPLE_TRUSTEE_OPERATION // : uint
        {
        }

        // Constants for MULTIPLE_TRUSTEE_OPERATION
        public static readonly long NO_MULTIPLE_TRUSTEE = (long)0L;
        public static readonly long TRUSTEE_IS_IMPERSONATE = (long)1L;


        public partial struct TRUSTEE_FORM // : uint
        {
        }

        // Constants for TRUSTEE_FORM
        public static readonly long TRUSTEE_IS_SID = (long)0L;
        public static readonly long TRUSTEE_IS_NAME = (long)1L;
        public static readonly long TRUSTEE_BAD_FORM = (long)2L;
        public static readonly long TRUSTEE_IS_OBJECTS_AND_SID = (long)3L;
        public static readonly long TRUSTEE_IS_OBJECTS_AND_NAME = (long)4L;


        public partial struct TRUSTEE_TYPE // : uint
        {
        }

        // Constants for TRUSTEE_TYPE
        public static readonly long TRUSTEE_IS_UNKNOWN = (long)0L;
        public static readonly long TRUSTEE_IS_USER = (long)1L;
        public static readonly long TRUSTEE_IS_GROUP = (long)2L;
        public static readonly long TRUSTEE_IS_DOMAIN = (long)3L;
        public static readonly long TRUSTEE_IS_ALIAS = (long)4L;
        public static readonly long TRUSTEE_IS_WELL_KNOWN_GROUP = (long)5L;
        public static readonly long TRUSTEE_IS_DELETED = (long)6L;
        public static readonly long TRUSTEE_IS_INVALID = (long)7L;
        public static readonly long TRUSTEE_IS_COMPUTER = (long)8L;


        // Constants for ObjectsPresent field
        public static readonly ulong ACE_OBJECT_TYPE_PRESENT = (ulong)0x1UL;
        public static readonly ulong ACE_INHERITED_OBJECT_TYPE_PRESENT = (ulong)0x2UL;


        public partial struct EXPLICIT_ACCESS
        {
            public ACCESS_MASK AccessPermissions;
            public ACCESS_MODE AccessMode;
            public uint Inheritance;
            public TRUSTEE Trustee;
        }

        // This type is the union inside of TRUSTEE and must be created using one of the TrusteeValueFrom* functions.
        public partial struct TrusteeValue // : System.UIntPtr
        {
        }

        public static TrusteeValue TrusteeValueFromString(@string str)
        {
            return TrusteeValue(@unsafe.Pointer(StringToUTF16Ptr(str)));
        }
        public static TrusteeValue TrusteeValueFromSID(ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return TrusteeValue(@unsafe.Pointer(sid));
        }
        public static TrusteeValue TrusteeValueFromObjectsAndSid(ptr<OBJECTS_AND_SID> _addr_objectsAndSid)
        {
            ref OBJECTS_AND_SID objectsAndSid = ref _addr_objectsAndSid.val;

            return TrusteeValue(@unsafe.Pointer(objectsAndSid));
        }
        public static TrusteeValue TrusteeValueFromObjectsAndName(ptr<OBJECTS_AND_NAME> _addr_objectsAndName)
        {
            ref OBJECTS_AND_NAME objectsAndName = ref _addr_objectsAndName.val;

            return TrusteeValue(@unsafe.Pointer(objectsAndName));
        }

        public partial struct TRUSTEE
        {
            public ptr<TRUSTEE> MultipleTrustee;
            public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            public TrusteeValue TrusteeValue;
        }

        public partial struct OBJECTS_AND_SID
        {
            public uint ObjectsPresent;
            public GUID ObjectTypeGuid;
            public GUID InheritedObjectTypeGuid;
            public ptr<SID> Sid;
        }

        public partial struct OBJECTS_AND_NAME
        {
            public uint ObjectsPresent;
            public SE_OBJECT_TYPE ObjectType;
            public ptr<ushort> ObjectTypeName;
            public ptr<ushort> InheritedObjectTypeName;
            public ptr<ushort> Name;
        }

        //sys    getSecurityInfo(handle Handle, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner **SID, group **SID, dacl **ACL, sacl **ACL, sd **SECURITY_DESCRIPTOR) (ret error) = advapi32.GetSecurityInfo
        //sys    SetSecurityInfo(handle Handle, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner *SID, group *SID, dacl *ACL, sacl *ACL) = advapi32.SetSecurityInfo
        //sys    getNamedSecurityInfo(objectName string, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner **SID, group **SID, dacl **ACL, sacl **ACL, sd **SECURITY_DESCRIPTOR) (ret error) = advapi32.GetNamedSecurityInfoW
        //sys    SetNamedSecurityInfo(objectName string, objectType SE_OBJECT_TYPE, securityInformation SECURITY_INFORMATION, owner *SID, group *SID, dacl *ACL, sacl *ACL) (ret error) = advapi32.SetNamedSecurityInfoW

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
        private static (SECURITY_DESCRIPTOR_CONTROL, uint, error) Control(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            SECURITY_DESCRIPTOR_CONTROL control = default;
            uint revision = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            err = getSecurityDescriptorControl(sd, _addr_control, _addr_revision);
            return ;
        }

        // SetControl sets the security descriptor control bits.
        private static error SetControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd, SECURITY_DESCRIPTOR_CONTROL controlBitsOfInterest, SECURITY_DESCRIPTOR_CONTROL controlBitsToSet)
        {
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            return error.As(setSecurityDescriptorControl(sd, controlBitsOfInterest, controlBitsToSet))!;
        }

        // RMControl returns the security descriptor resource manager control bits.
        private static (byte, error) RMControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            byte control = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            err = getSecurityDescriptorRMControl(sd, _addr_control);
            return ;
        }

        // SetRMControl sets the security descriptor resource manager control bits.
        private static void SetRMControl(this ptr<SECURITY_DESCRIPTOR> _addr_sd, byte rmControl)
        {
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            setSecurityDescriptorRMControl(sd, _addr_rmControl);
        }

        // DACL returns the security descriptor DACL and whether it was defaulted. The dacl return value may be nil
        // if a DACL exists but is an "empty DACL", meaning fully permissive. If the DACL does not exist, err returns
        // ERROR_OBJECT_NOT_FOUND.
        private static (ptr<ACL>, bool, error) DACL(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ptr<ACL> dacl = default!;
            bool defaulted = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            ref bool present = ref heap(out ptr<bool> _addr_present);
            err = getSecurityDescriptorDacl(sd, _addr_present, _addr_dacl, _addr_defaulted);
            if (!present)
            {
                err = ERROR_OBJECT_NOT_FOUND;
            }

            return ;

        }

        // SetDACL sets the absolute security descriptor DACL.
        private static error SetDACL(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<ACL> _addr_dacl, bool present, bool defaulted)
        {
            ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
            ref ACL dacl = ref _addr_dacl.val;

            return error.As(setSecurityDescriptorDacl(absoluteSD, present, dacl, defaulted))!;
        }

        // SACL returns the security descriptor SACL and whether it was defaulted. The sacl return value may be nil
        // if a SACL exists but is an "empty SACL", meaning fully permissive. If the SACL does not exist, err returns
        // ERROR_OBJECT_NOT_FOUND.
        private static (ptr<ACL>, bool, error) SACL(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ptr<ACL> sacl = default!;
            bool defaulted = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            ref bool present = ref heap(out ptr<bool> _addr_present);
            err = getSecurityDescriptorSacl(sd, _addr_present, _addr_sacl, _addr_defaulted);
            if (!present)
            {
                err = ERROR_OBJECT_NOT_FOUND;
            }

            return ;

        }

        // SetSACL sets the absolute security descriptor SACL.
        private static error SetSACL(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<ACL> _addr_sacl, bool present, bool defaulted)
        {
            ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
            ref ACL sacl = ref _addr_sacl.val;

            return error.As(setSecurityDescriptorSacl(absoluteSD, present, sacl, defaulted))!;
        }

        // Owner returns the security descriptor owner and whether it was defaulted.
        private static (ptr<SID>, bool, error) Owner(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ptr<SID> owner = default!;
            bool defaulted = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            err = getSecurityDescriptorOwner(sd, _addr_owner, _addr_defaulted);
            return ;
        }

        // SetOwner sets the absolute security descriptor owner.
        private static error SetOwner(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<SID> _addr_owner, bool defaulted)
        {
            ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
            ref SID owner = ref _addr_owner.val;

            return error.As(setSecurityDescriptorOwner(absoluteSD, owner, defaulted))!;
        }

        // Group returns the security descriptor group and whether it was defaulted.
        private static (ptr<SID>, bool, error) Group(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ptr<SID> group = default!;
            bool defaulted = default;
            error err = default!;
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            err = getSecurityDescriptorGroup(sd, _addr_group, _addr_defaulted);
            return ;
        }

        // SetGroup sets the absolute security descriptor owner.
        private static error SetGroup(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD, ptr<SID> _addr_group, bool defaulted)
        {
            ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;
            ref SID group = ref _addr_group.val;

            return error.As(setSecurityDescriptorGroup(absoluteSD, group, defaulted))!;
        }

        // Length returns the length of the security descriptor.
        private static uint Length(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            return getSecurityDescriptorLength(sd);
        }

        // IsValid returns whether the security descriptor is valid.
        private static bool IsValid(this ptr<SECURITY_DESCRIPTOR> _addr_sd)
        {
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            return isValidSecurityDescriptor(sd);
        }

        // String returns the SDDL form of the security descriptor, with a function signature that can be
        // used with %v formatting directives.
        private static @string String(this ptr<SECURITY_DESCRIPTOR> _addr_sd) => func((defer, _, __) =>
        {
            ref SECURITY_DESCRIPTOR sd = ref _addr_sd.val;

            ptr<ushort> sddl;
            var err = convertSecurityDescriptorToStringSecurityDescriptor(sd, 1L, 0xffUL, _addr_sddl, null);
            if (err != null)
            {
                return "";
            }

            defer(LocalFree(Handle(@unsafe.Pointer(sddl))));
            return UTF16ToString(new ptr<ptr<array<ushort>>>(@unsafe.Pointer(sddl))[..]);

        });

        // ToAbsolute converts a self-relative security descriptor into an absolute one.
        private static (ptr<SECURITY_DESCRIPTOR>, error) ToAbsolute(this ptr<SECURITY_DESCRIPTOR> _addr_selfRelativeSD)
        {
            ptr<SECURITY_DESCRIPTOR> absoluteSD = default!;
            error err = default!;
            ref SECURITY_DESCRIPTOR selfRelativeSD = ref _addr_selfRelativeSD.val;

            var (control, _, err) = selfRelativeSD.Control();
            if (err != null)
            {
                return ;
            }

            if (control & SE_SELF_RELATIVE == 0L)
            {
                err = ERROR_INVALID_PARAMETER;
                return ;
            }

            ref uint absoluteSDSize = ref heap(out ptr<uint> _addr_absoluteSDSize);            ref uint daclSize = ref heap(out ptr<uint> _addr_daclSize);            ref uint saclSize = ref heap(out ptr<uint> _addr_saclSize);            ref uint ownerSize = ref heap(out ptr<uint> _addr_ownerSize);            ref uint groupSize = ref heap(out ptr<uint> _addr_groupSize);

            err = makeAbsoluteSD(selfRelativeSD, null, _addr_absoluteSDSize, null, _addr_daclSize, null, _addr_saclSize, null, _addr_ownerSize, null, _addr_groupSize);

            if (err == ERROR_INSUFFICIENT_BUFFER)             else if (err == null) 
                // makeAbsoluteSD is expected to fail, but it succeeds.
                return (_addr_null!, error.As(ERROR_INTERNAL_ERROR)!);
            else 
                return (_addr_null!, error.As(err)!);
                        if (absoluteSDSize > 0L)
            {
                absoluteSD = (SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_make_slice<byte>(absoluteSDSize)[0L]));
            }

            ptr<ACL> dacl;            ptr<ACL> sacl;            ptr<SID> owner;            ptr<SID> group;
            if (daclSize > 0L)
            {
                dacl = (ACL.val)(@unsafe.Pointer(_addr_make_slice<byte>(daclSize)[0L]));
            }

            if (saclSize > 0L)
            {
                sacl = (ACL.val)(@unsafe.Pointer(_addr_make_slice<byte>(saclSize)[0L]));
            }

            if (ownerSize > 0L)
            {
                owner = (SID.val)(@unsafe.Pointer(_addr_make_slice<byte>(ownerSize)[0L]));
            }

            if (groupSize > 0L)
            {
                group = (SID.val)(@unsafe.Pointer(_addr_make_slice<byte>(groupSize)[0L]));
            }

            err = makeAbsoluteSD(selfRelativeSD, absoluteSD, _addr_absoluteSDSize, dacl, _addr_daclSize, sacl, _addr_saclSize, owner, _addr_ownerSize, group, _addr_groupSize);
            return ;

        }

        // ToSelfRelative converts an absolute security descriptor into a self-relative one.
        private static (ptr<SECURITY_DESCRIPTOR>, error) ToSelfRelative(this ptr<SECURITY_DESCRIPTOR> _addr_absoluteSD)
        {
            ptr<SECURITY_DESCRIPTOR> selfRelativeSD = default!;
            error err = default!;
            ref SECURITY_DESCRIPTOR absoluteSD = ref _addr_absoluteSD.val;

            var (control, _, err) = absoluteSD.Control();
            if (err != null)
            {
                return ;
            }

            if (control & SE_SELF_RELATIVE != 0L)
            {
                err = ERROR_INVALID_PARAMETER;
                return ;
            }

            ref uint selfRelativeSDSize = ref heap(out ptr<uint> _addr_selfRelativeSDSize);
            err = makeSelfRelativeSD(absoluteSD, null, _addr_selfRelativeSDSize);

            if (err == ERROR_INSUFFICIENT_BUFFER)             else if (err == null) 
                // makeSelfRelativeSD is expected to fail, but it succeeds.
                return (_addr_null!, error.As(ERROR_INTERNAL_ERROR)!);
            else 
                return (_addr_null!, error.As(err)!);
                        if (selfRelativeSDSize > 0L)
            {
                selfRelativeSD = (SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_make_slice<byte>(selfRelativeSDSize)[0L]));
            }

            err = makeSelfRelativeSD(absoluteSD, selfRelativeSD, _addr_selfRelativeSDSize);
            return ;

        }

        private static ptr<SECURITY_DESCRIPTOR> copySelfRelativeSecurityDescriptor(this ptr<SECURITY_DESCRIPTOR> _addr_selfRelativeSD)
        {
            ref SECURITY_DESCRIPTOR selfRelativeSD = ref _addr_selfRelativeSD.val;

            var sdBytes = make_slice<byte>(selfRelativeSD.Length());
            copy(sdBytes, new ptr<ptr<array<byte>>>(@unsafe.Pointer(selfRelativeSD))[..len(sdBytes)]);
            return _addr_(SECURITY_DESCRIPTOR.val)(@unsafe.Pointer(_addr_sdBytes[0L]))!;
        }

        // SecurityDescriptorFromString converts an SDDL string describing a security descriptor into a
        // self-relative security descriptor object allocated on the Go heap.
        public static (ptr<SECURITY_DESCRIPTOR>, error) SecurityDescriptorFromString(@string sddl) => func((defer, _, __) =>
        {
            ptr<SECURITY_DESCRIPTOR> sd = default!;
            error err = default!;

            ptr<SECURITY_DESCRIPTOR> winHeapSD;
            err = convertStringSecurityDescriptorToSecurityDescriptor(sddl, 1L, _addr_winHeapSD, null);
            if (err != null)
            {
                return ;
            }

            defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
            return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

        });

        // GetSecurityInfo queries the security information for a given handle and returns the self-relative security
        // descriptor result on the Go heap.
        public static (ptr<SECURITY_DESCRIPTOR>, error) GetSecurityInfo(Handle handle, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) => func((defer, _, __) =>
        {
            ptr<SECURITY_DESCRIPTOR> sd = default!;
            error err = default!;

            ptr<SECURITY_DESCRIPTOR> winHeapSD;
            err = getSecurityInfo(handle, objectType, securityInformation, null, null, null, null, _addr_winHeapSD);
            if (err != null)
            {
                return ;
            }

            defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
            return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

        });

        // GetNamedSecurityInfo queries the security information for a given named object and returns the self-relative security
        // descriptor result on the Go heap.
        public static (ptr<SECURITY_DESCRIPTOR>, error) GetNamedSecurityInfo(@string objectName, SE_OBJECT_TYPE objectType, SECURITY_INFORMATION securityInformation) => func((defer, _, __) =>
        {
            ptr<SECURITY_DESCRIPTOR> sd = default!;
            error err = default!;

            ptr<SECURITY_DESCRIPTOR> winHeapSD;
            err = getNamedSecurityInfo(objectName, objectType, securityInformation, null, null, null, null, _addr_winHeapSD);
            if (err != null)
            {
                return ;
            }

            defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
            return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

        });

        // BuildSecurityDescriptor makes a new security descriptor using the input trustees, explicit access lists, and
        // prior security descriptor to be merged, any of which can be nil, returning the self-relative security descriptor
        // result on the Go heap.
        public static (ptr<SECURITY_DESCRIPTOR>, error) BuildSecurityDescriptor(ptr<TRUSTEE> _addr_owner, ptr<TRUSTEE> _addr_group, slice<EXPLICIT_ACCESS> accessEntries, slice<EXPLICIT_ACCESS> auditEntries, ptr<SECURITY_DESCRIPTOR> _addr_mergedSecurityDescriptor) => func((defer, _, __) =>
        {
            ptr<SECURITY_DESCRIPTOR> sd = default!;
            error err = default!;
            ref TRUSTEE owner = ref _addr_owner.val;
            ref TRUSTEE group = ref _addr_group.val;
            ref SECURITY_DESCRIPTOR mergedSecurityDescriptor = ref _addr_mergedSecurityDescriptor.val;

            ptr<SECURITY_DESCRIPTOR> winHeapSD;
            ref uint winHeapSDSize = ref heap(out ptr<uint> _addr_winHeapSDSize);
            ptr<EXPLICIT_ACCESS> firstAccessEntry;
            if (len(accessEntries) > 0L)
            {
                firstAccessEntry = _addr_accessEntries[0L];
            }

            ptr<EXPLICIT_ACCESS> firstAuditEntry;
            if (len(auditEntries) > 0L)
            {
                firstAuditEntry = _addr_auditEntries[0L];
            }

            err = buildSecurityDescriptor(owner, group, uint32(len(accessEntries)), firstAccessEntry, uint32(len(auditEntries)), firstAuditEntry, mergedSecurityDescriptor, _addr_winHeapSDSize, _addr_winHeapSD);
            if (err != null)
            {
                return ;
            }

            defer(LocalFree(Handle(@unsafe.Pointer(winHeapSD))));
            return (_addr_winHeapSD.copySelfRelativeSecurityDescriptor()!, error.As(null!)!);

        });

        // NewSecurityDescriptor creates and initializes a new absolute security descriptor.
        public static (ptr<SECURITY_DESCRIPTOR>, error) NewSecurityDescriptor()
        {
            ptr<SECURITY_DESCRIPTOR> absoluteSD = default!;
            error err = default!;

            absoluteSD = addr(new SECURITY_DESCRIPTOR());
            err = initializeSecurityDescriptor(absoluteSD, 1L);
            return ;
        }

        // ACLFromEntries returns a new ACL on the Go heap containing a list of explicit entries as well as those of another ACL.
        // Both explicitEntries and mergedACL are optional and can be nil.
        public static (ptr<ACL>, error) ACLFromEntries(slice<EXPLICIT_ACCESS> explicitEntries, ptr<ACL> _addr_mergedACL) => func((defer, _, __) =>
        {
            ptr<ACL> acl = default!;
            error err = default!;
            ref ACL mergedACL = ref _addr_mergedACL.val;

            ptr<EXPLICIT_ACCESS> firstExplicitEntry;
            if (len(explicitEntries) > 0L)
            {
                firstExplicitEntry = _addr_explicitEntries[0L];
            }

            ptr<ACL> winHeapACL;
            err = setEntriesInAcl(uint32(len(explicitEntries)), firstExplicitEntry, mergedACL, _addr_winHeapACL);
            if (err != null)
            {
                return ;
            }

            defer(LocalFree(Handle(@unsafe.Pointer(winHeapACL))));
            var aclBytes = make_slice<byte>(winHeapACL.aclSize);
            copy(aclBytes, new ptr<ptr<array<byte>>>(@unsafe.Pointer(winHeapACL))[..len(aclBytes)]);
            return (_addr_(ACL.val)(@unsafe.Pointer(_addr_aclBytes[0L]))!, error.As(null!)!);

        });
    }
}}}}}}

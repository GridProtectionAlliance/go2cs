// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 08 03:27:01 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\security_windows.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly ulong STANDARD_RIGHTS_REQUIRED = (ulong)0xf0000UL;
        public static readonly ulong STANDARD_RIGHTS_READ = (ulong)0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_WRITE = (ulong)0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_EXECUTE = (ulong)0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_ALL = (ulong)0x1F0000UL;


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
        // https://blogs.msdn.com/b/drnick/archive/2007/12/19/windows-and-upn-format-credentials.aspx
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
        public static readonly var NetSetupUnknownStatus = (var)iota;
        public static readonly var NetSetupUnjoined = (var)0;
        public static readonly var NetSetupWorkgroupName = (var)1;
        public static readonly var NetSetupDomainName = (var)2;


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
        public static readonly var SidTypeGroup = (var)0;
        public static readonly var SidTypeDomain = (var)1;
        public static readonly var SidTypeAlias = (var)2;
        public static readonly var SidTypeWellKnownGroup = (var)3;
        public static readonly var SidTypeDeletedAccount = (var)4;
        public static readonly var SidTypeInvalid = (var)5;
        public static readonly var SidTypeUnknown = (var)6;
        public static readonly var SidTypeComputer = (var)7;
        public static readonly var SidTypeLabel = (var)8;


        //sys    LookupAccountSid(systemName *uint16, sid *SID, name *uint16, nameLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountSidW
        //sys    LookupAccountName(systemName *uint16, accountName *uint16, sid *SID, sidLen *uint32, refdDomainName *uint16, refdDomainNameLen *uint32, use *uint32) (err error) = advapi32.LookupAccountNameW
        //sys    ConvertSidToStringSid(sid *SID, stringSid **uint16) (err error) = advapi32.ConvertSidToStringSidW
        //sys    ConvertStringSidToSid(stringSid *uint16, sid **SID) (err error) = advapi32.ConvertStringSidToSidW
        //sys    GetLengthSid(sid *SID) (len uint32) = advapi32.GetLengthSid
        //sys    CopySid(destSidLen uint32, destSid *SID, srcSid *SID) (err error) = advapi32.CopySid

        // The security identifier (SID) structure is a variable-length
        // structure used to uniquely identify users or groups.
        public partial struct SID
        {
        }

        // StringToSid converts a string-format security identifier
        // sid into a valid, functional sid.
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

        // LookupSID retrieves a security identifier sid for the account
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
                return (_addr_null!, "", 0L, error.As(EINVAL)!);
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

        // String converts sid to a string format
        // suitable for display, storage, or transmission.
        private static (@string, error) String(this ptr<SID> _addr_sid) => func((defer, _, __) =>
        {
            @string _p0 = default;
            error _p0 = default!;
            ref SID sid = ref _addr_sid.val;

            ptr<ushort> s;
            var e = ConvertSidToStringSid(sid, _addr_s);
            if (e != null)
            {
                return ("", error.As(e)!);
            }

            defer(LocalFree((Handle)(@unsafe.Pointer(s))));
            return (utf16PtrToString(s), error.As(null!)!);

        });

        // Len returns the length, in bytes, of a valid security identifier sid.
        private static long Len(this ptr<SID> _addr_sid)
        {
            ref SID sid = ref _addr_sid.val;

            return int(GetLengthSid(sid));
        }

        // Copy creates a duplicate of security identifier sid.
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

        // LookupAccount retrieves the name of the account for this sid
        // and the name of the first domain on which this sid is found.
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

 
        // do not reorder
        public static readonly long TOKEN_ASSIGN_PRIMARY = (long)1L << (int)(iota);
        public static readonly var TOKEN_DUPLICATE = (var)0;
        public static readonly var TOKEN_IMPERSONATE = (var)1;
        public static readonly var TOKEN_QUERY = (var)2;
        public static readonly var TOKEN_QUERY_SOURCE = (var)3;
        public static readonly var TOKEN_ADJUST_PRIVILEGES = (var)4;
        public static readonly var TOKEN_ADJUST_GROUPS = (var)5;
        public static readonly var TOKEN_ADJUST_DEFAULT = (var)6;
        public static readonly TOKEN_ALL_ACCESS TOKEN_ADJUST_SESSIONID = (TOKEN_ALL_ACCESS)STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;
        public static readonly var TOKEN_READ = (var)STANDARD_RIGHTS_READ | TOKEN_QUERY;
        public static readonly var TOKEN_WRITE = (var)STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
        public static readonly var TOKEN_EXECUTE = (var)STANDARD_RIGHTS_EXECUTE;


 
        // do not reorder
        public static readonly long TokenUser = (long)1L + iota;
        public static readonly var TokenGroups = (var)0;
        public static readonly var TokenPrivileges = (var)1;
        public static readonly var TokenOwner = (var)2;
        public static readonly var TokenPrimaryGroup = (var)3;
        public static readonly var TokenDefaultDacl = (var)4;
        public static readonly var TokenSource = (var)5;
        public static readonly var TokenType = (var)6;
        public static readonly var TokenImpersonationLevel = (var)7;
        public static readonly var TokenStatistics = (var)8;
        public static readonly var TokenRestrictedSids = (var)9;
        public static readonly var TokenSessionId = (var)10;
        public static readonly var TokenGroupsAndPrivileges = (var)11;
        public static readonly var TokenSessionReference = (var)12;
        public static readonly var TokenSandBoxInert = (var)13;
        public static readonly var TokenAuditPolicy = (var)14;
        public static readonly var TokenOrigin = (var)15;
        public static readonly var TokenElevationType = (var)16;
        public static readonly var TokenLinkedToken = (var)17;
        public static readonly var TokenElevation = (var)18;
        public static readonly var TokenHasRestrictions = (var)19;
        public static readonly var TokenAccessInformation = (var)20;
        public static readonly var TokenVirtualizationAllowed = (var)21;
        public static readonly var TokenVirtualizationEnabled = (var)22;
        public static readonly var TokenIntegrityLevel = (var)23;
        public static readonly var TokenUIAccess = (var)24;
        public static readonly var TokenMandatoryPolicy = (var)25;
        public static readonly var TokenLogonSid = (var)26;
        public static readonly var MaxTokenInfoClass = (var)27;


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

        //sys    OpenProcessToken(h Handle, access uint32, token *Token) (err error) = advapi32.OpenProcessToken
        //sys    GetTokenInformation(t Token, infoClass uint32, info *byte, infoLen uint32, returnedLen *uint32) (err error) = advapi32.GetTokenInformation
        //sys    GetUserProfileDirectory(t Token, dir *uint16, dirLen *uint32) (err error) = userenv.GetUserProfileDirectoryW
        //sys    getSystemDirectory(dir *uint16, dirLen uint32) (len uint32, err error) = kernel32.GetSystemDirectoryW

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

        // OpenCurrentProcessToken opens the access token
        // associated with current process.
        public static (Token, error) OpenCurrentProcessToken()
        {
            Token _p0 = default;
            error _p0 = default!;

            var (p, e) = GetCurrentProcess();
            if (e != null)
            {
                return (0L, error.As(e)!);
            }

            ref Token t = ref heap(out ptr<Token> _addr_t);
            e = OpenProcessToken(p, TOKEN_QUERY, _addr_t);
            if (e != null)
            {
                return (0L, error.As(e)!);
            }

            return (t, error.As(null!)!);

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
    }
}

// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 August 29 08:37:37 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\security_windows.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        public static readonly ulong STANDARD_RIGHTS_REQUIRED = 0xf0000UL;
        public static readonly ulong STANDARD_RIGHTS_READ = 0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_WRITE = 0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_EXECUTE = 0x20000UL;
        public static readonly ulong STANDARD_RIGHTS_ALL = 0x1F0000UL;

        public static readonly long NameUnknown = 0L;
        public static readonly long NameFullyQualifiedDN = 1L;
        public static readonly long NameSamCompatible = 2L;
        public static readonly long NameDisplay = 3L;
        public static readonly long NameUniqueId = 6L;
        public static readonly long NameCanonical = 7L;
        public static readonly long NameUserPrincipal = 8L;
        public static readonly long NameCanonicalEx = 9L;
        public static readonly long NameServicePrincipal = 10L;
        public static readonly long NameDnsDomain = 12L;

        // This function returns 1 byte BOOLEAN rather than the 4 byte BOOL.
        // http://blogs.msdn.com/b/drnick/archive/2007/12/19/windows-and-upn-format-credentials.aspx
        //sys    TranslateName(accName *uint16, accNameFormat uint32, desiredNameFormat uint32, translatedName *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.TranslateNameW
        //sys    GetUserNameEx(nameFormat uint32, nameBuffre *uint16, nSize *uint32) (err error) [failretval&0xff==0] = secur32.GetUserNameExW

        // TranslateAccountName converts a directory service
        // object name from one format to another.
        public static (@string, error) TranslateAccountName(@string username, uint from, uint to, long initSize)
        {
            var (u, e) = UTF16PtrFromString(username);
            if (e != null)
            {
                return ("", e);
            }
            var n = uint32(50L);
            while (true)
            {
                var b = make_slice<ushort>(n);
                e = TranslateName(u, from, to, ref b[0L], ref n);
                if (e == null)
                {
                    return (UTF16ToString(b[..n]), null);
                }
                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", e);
                }
                if (n <= uint32(len(b)))
                {
                    return ("", e);
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
        public static readonly long SidTypeUser = 1L + iota;
        public static readonly var SidTypeGroup = 0;
        public static readonly var SidTypeDomain = 1;
        public static readonly var SidTypeAlias = 2;
        public static readonly var SidTypeWellKnownGroup = 3;
        public static readonly var SidTypeDeletedAccount = 4;
        public static readonly var SidTypeInvalid = 5;
        public static readonly var SidTypeUnknown = 6;
        public static readonly var SidTypeComputer = 7;
        public static readonly var SidTypeLabel = 8;

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
        public static (ref SID, error) StringToSid(@string s) => func((defer, _, __) =>
        {
            ref SID sid = default;
            var (p, e) = UTF16PtrFromString(s);
            if (e != null)
            {
                return (null, e);
            }
            e = ConvertStringSidToSid(p, ref sid);
            if (e != null)
            {
                return (null, e);
            }
            defer(LocalFree((Handle)(@unsafe.Pointer(sid))));
            return sid.Copy();
        });

        // LookupSID retrieves a security identifier sid for the account
        // and the name of the domain on which the account was found.
        // System specify target computer to search.
        public static (ref SID, @string, uint, error) LookupSID(@string system, @string account)
        {
            if (len(account) == 0L)
            {
                return (null, "", 0L, EINVAL);
            }
            var (acc, e) = UTF16PtrFromString(account);
            if (e != null)
            {
                return (null, "", 0L, e);
            }
            ref ushort sys = default;
            if (len(system) > 0L)
            {
                sys, e = UTF16PtrFromString(system);
                if (e != null)
                {
                    return (null, "", 0L, e);
                }
            }
            var n = uint32(50L);
            var dn = uint32(50L);
            while (true)
            {
                var b = make_slice<byte>(n);
                var db = make_slice<ushort>(dn);
                sid = (SID.Value)(@unsafe.Pointer(ref b[0L]));
                e = LookupAccountName(sys, acc, sid, ref n, ref db[0L], ref dn, ref accType);
                if (e == null)
                {
                    return (sid, UTF16ToString(db), accType, null);
                }
                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return (null, "", 0L, e);
                }
                if (n <= uint32(len(b)))
                {
                    return (null, "", 0L, e);
                }
            }

        }

        // String converts sid to a string format
        // suitable for display, storage, or transmission.
        private static (@string, error) String(this ref SID _sid) => func(_sid, (ref SID sid, Defer defer, Panic _, Recover __) =>
        {
            ref ushort s = default;
            var e = ConvertSidToStringSid(sid, ref s);
            if (e != null)
            {
                return ("", e);
            }
            defer(LocalFree((Handle)(@unsafe.Pointer(s))));
            return (UTF16ToString(new ptr<ref array<ushort>>(@unsafe.Pointer(s))[..]), null);
        });

        // Len returns the length, in bytes, of a valid security identifier sid.
        private static long Len(this ref SID sid)
        {
            return int(GetLengthSid(sid));
        }

        // Copy creates a duplicate of security identifier sid.
        private static (ref SID, error) Copy(this ref SID sid)
        {
            var b = make_slice<byte>(sid.Len());
            var sid2 = (SID.Value)(@unsafe.Pointer(ref b[0L]));
            var e = CopySid(uint32(len(b)), sid2, sid);
            if (e != null)
            {
                return (null, e);
            }
            return (sid2, null);
        }

        // LookupAccount retrieves the name of the account for this sid
        // and the name of the first domain on which this sid is found.
        // System specify target computer to search for.
        private static (@string, @string, uint, error) LookupAccount(this ref SID sid, @string system)
        {
            ref ushort sys = default;
            if (len(system) > 0L)
            {
                sys, err = UTF16PtrFromString(system);
                if (err != null)
                {
                    return ("", "", 0L, err);
                }
            }
            var n = uint32(50L);
            var dn = uint32(50L);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var db = make_slice<ushort>(dn);
                var e = LookupAccountSid(sys, sid, ref b[0L], ref n, ref db[0L], ref dn, ref accType);
                if (e == null)
                {
                    return (UTF16ToString(b), UTF16ToString(db), accType, null);
                }
                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", "", 0L, e);
                }
                if (n <= uint32(len(b)))
                {
                    return ("", "", 0L, e);
                }
            }

        }

 
        // do not reorder
        public static readonly long TOKEN_ASSIGN_PRIMARY = 1L << (int)(iota);
        public static readonly var TOKEN_DUPLICATE = 0;
        public static readonly var TOKEN_IMPERSONATE = 1;
        public static readonly var TOKEN_QUERY = 2;
        public static readonly var TOKEN_QUERY_SOURCE = 3;
        public static readonly var TOKEN_ADJUST_PRIVILEGES = 4;
        public static readonly var TOKEN_ADJUST_GROUPS = 5;
        public static readonly TOKEN_ALL_ACCESS TOKEN_ADJUST_DEFAULT = STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
        public static readonly var TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;
        public static readonly var TOKEN_WRITE = STANDARD_RIGHTS_WRITE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT;
        public static readonly var TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;

 
        // do not reorder
        public static readonly long TokenUser = 1L + iota;
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
            var (p, e) = GetCurrentProcess();
            if (e != null)
            {
                return (0L, e);
            }
            Token t = default;
            e = OpenProcessToken(p, TOKEN_QUERY, ref t);
            if (e != null)
            {
                return (0L, e);
            }
            return (t, null);
        }

        // Close releases access to access token.
        public static error Close(this Token t)
        {
            return error.As(CloseHandle(Handle(t)));
        }

        // getInfo retrieves a specified type of information about an access token.
        public static (unsafe.Pointer, error) getInfo(this Token t, uint @class, long initSize)
        {
            var n = uint32(initSize);
            while (true)
            {
                var b = make_slice<byte>(n);
                var e = GetTokenInformation(t, class, ref b[0L], uint32(len(b)), ref n);
                if (e == null)
                {
                    return (@unsafe.Pointer(ref b[0L]), null);
                }
                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return (null, e);
                }
                if (n <= uint32(len(b)))
                {
                    return (null, e);
                }
            }

        }

        // GetTokenUser retrieves access token t user account information.
        public static (ref Tokenuser, error) GetTokenUser(this Token t)
        {
            var (i, e) = t.getInfo(TokenUser, 50L);
            if (e != null)
            {
                return (null, e);
            }
            return ((Tokenuser.Value)(i), null);
        }

        // GetTokenPrimaryGroup retrieves access token t primary group information.
        // A pointer to a SID structure representing a group that will become
        // the primary group of any objects created by a process using this access token.
        public static (ref Tokenprimarygroup, error) GetTokenPrimaryGroup(this Token t)
        {
            var (i, e) = t.getInfo(TokenPrimaryGroup, 50L);
            if (e != null)
            {
                return (null, e);
            }
            return ((Tokenprimarygroup.Value)(i), null);
        }

        // GetUserProfileDirectory retrieves path to the
        // root directory of the access token t user's profile.
        public static (@string, error) GetUserProfileDirectory(this Token t)
        {
            var n = uint32(100L);
            while (true)
            {
                var b = make_slice<ushort>(n);
                var e = GetUserProfileDirectory(t, ref b[0L], ref n);
                if (e == null)
                {
                    return (UTF16ToString(b), null);
                }
                if (e != ERROR_INSUFFICIENT_BUFFER)
                {
                    return ("", e);
                }
                if (n <= uint32(len(b)))
                {
                    return ("", e);
                }
            }

        }
    }
}

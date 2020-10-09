// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 October 09 04:51:13 UTC
// import "internal/syscall/windows" ==> using windows = go.@internal.syscall.windows_package
// Original source: C:\Go\src\internal\syscall\windows\security_windows.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal {
namespace syscall
{
    public static partial class windows_package
    {
        public static readonly long SecurityAnonymous = (long)0L;
        public static readonly long SecurityIdentification = (long)1L;
        public static readonly long SecurityImpersonation = (long)2L;
        public static readonly long SecurityDelegation = (long)3L;


        //sys    ImpersonateSelf(impersonationlevel uint32) (err error) = advapi32.ImpersonateSelf
        //sys    RevertToSelf() (err error) = advapi32.RevertToSelf

        public static readonly ulong TOKEN_ADJUST_PRIVILEGES = (ulong)0x0020UL;
        public static readonly ulong SE_PRIVILEGE_ENABLED = (ulong)0x00000002UL;


        public partial struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        public partial struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        public partial struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public array<LUID_AND_ATTRIBUTES> Privileges;
        }

        //sys    OpenThreadToken(h syscall.Handle, access uint32, openasself bool, token *syscall.Token) (err error) = advapi32.OpenThreadToken
        //sys    LookupPrivilegeValue(systemname *uint16, name *uint16, luid *LUID) (err error) = advapi32.LookupPrivilegeValueW
        //sys    adjustTokenPrivileges(token syscall.Token, disableAllPrivileges bool, newstate *TOKEN_PRIVILEGES, buflen uint32, prevstate *TOKEN_PRIVILEGES, returnlen *uint32) (ret uint32, err error) [true] = advapi32.AdjustTokenPrivileges

        public static error AdjustTokenPrivileges(syscall.Token token, bool disableAllPrivileges, ptr<TOKEN_PRIVILEGES> _addr_newstate, uint buflen, ptr<TOKEN_PRIVILEGES> _addr_prevstate, ptr<uint> _addr_returnlen)
        {
            ref TOKEN_PRIVILEGES newstate = ref _addr_newstate.val;
            ref TOKEN_PRIVILEGES prevstate = ref _addr_prevstate.val;
            ref uint returnlen = ref _addr_returnlen.val;

            var (ret, err) = adjustTokenPrivileges(token, disableAllPrivileges, newstate, buflen, prevstate, returnlen);
            if (ret == 0L)
            { 
                // AdjustTokenPrivileges call failed
                return error.As(err)!;

            } 
            // AdjustTokenPrivileges call succeeded
            if (err == syscall.EINVAL)
            { 
                // GetLastError returned ERROR_SUCCESS
                return error.As(null!)!;

            }

            return error.As(err)!;

        }

        //sys DuplicateTokenEx(hExistingToken syscall.Token, dwDesiredAccess uint32, lpTokenAttributes *syscall.SecurityAttributes, impersonationLevel uint32, tokenType TokenType, phNewToken *syscall.Token) (err error) = advapi32.DuplicateTokenEx
        //sys SetTokenInformation(tokenHandle syscall.Token, tokenInformationClass uint32, tokenInformation uintptr, tokenInformationLength uint32) (err error) = advapi32.SetTokenInformation

        public partial struct SID_AND_ATTRIBUTES
        {
            public ptr<syscall.SID> Sid;
            public uint Attributes;
        }

        public partial struct TOKEN_MANDATORY_LABEL
        {
            public SID_AND_ATTRIBUTES Label;
        }

        private static uint Size(this ptr<TOKEN_MANDATORY_LABEL> _addr_tml)
        {
            ref TOKEN_MANDATORY_LABEL tml = ref _addr_tml.val;

            return uint32(@unsafe.Sizeof(new TOKEN_MANDATORY_LABEL())) + syscall.GetLengthSid(tml.Label.Sid);
        }

        public static readonly ulong SE_GROUP_INTEGRITY = (ulong)0x00000020UL;



        public partial struct TokenType // : uint
        {
        }

        public static readonly TokenType TokenPrimary = (TokenType)1L;
        public static readonly TokenType TokenImpersonation = (TokenType)2L;


        //sys    GetProfilesDirectory(dir *uint16, dirLen *uint32) (err error) = userenv.GetProfilesDirectoryW

        public static readonly ulong LG_INCLUDE_INDIRECT = (ulong)0x1UL;
        public static readonly ulong MAX_PREFERRED_LENGTH = (ulong)0xFFFFFFFFUL;


        public partial struct LocalGroupUserInfo0
        {
            public ptr<ushort> Name;
        }

        public partial struct UserInfo4
        {
            public ptr<ushort> Name;
            public ptr<ushort> Password;
            public uint PasswordAge;
            public uint Priv;
            public ptr<ushort> HomeDir;
            public ptr<ushort> Comment;
            public uint Flags;
            public ptr<ushort> ScriptPath;
            public uint AuthFlags;
            public ptr<ushort> FullName;
            public ptr<ushort> UsrComment;
            public ptr<ushort> Parms;
            public ptr<ushort> Workstations;
            public uint LastLogon;
            public uint LastLogoff;
            public uint AcctExpires;
            public uint MaxStorage;
            public uint UnitsPerWeek;
            public ptr<byte> LogonHours;
            public uint BadPwCount;
            public uint NumLogons;
            public ptr<ushort> LogonServer;
            public uint CountryCode;
            public uint CodePage;
            public ptr<syscall.SID> UserSid;
            public uint PrimaryGroupID;
            public ptr<ushort> Profile;
            public ptr<ushort> HomeDirDrive;
            public uint PasswordExpired;
        }

        //sys    NetUserGetLocalGroups(serverName *uint16, userName *uint16, level uint32, flags uint32, buf **byte, prefMaxLen uint32, entriesRead *uint32, totalEntries *uint32) (neterr error) = netapi32.NetUserGetLocalGroups
    }
}}}

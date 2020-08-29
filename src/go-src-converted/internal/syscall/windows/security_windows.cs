// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package windows -- go2cs converted at 2020 August 29 08:22:31 UTC
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
        public static readonly long SecurityAnonymous = 0L;
        public static readonly long SecurityIdentification = 1L;
        public static readonly long SecurityImpersonation = 2L;
        public static readonly long SecurityDelegation = 3L;

        //sys    ImpersonateSelf(impersonationlevel uint32) (err error) = advapi32.ImpersonateSelf
        //sys    RevertToSelf() (err error) = advapi32.RevertToSelf

        public static readonly ulong TOKEN_ADJUST_PRIVILEGES = 0x0020UL;
        public static readonly ulong SE_PRIVILEGE_ENABLED = 0x00000002UL;

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

        public static error AdjustTokenPrivileges(syscall.Token token, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newstate, uint buflen, ref TOKEN_PRIVILEGES prevstate, ref uint returnlen)
        {
            var (ret, err) = adjustTokenPrivileges(token, disableAllPrivileges, newstate, buflen, prevstate, returnlen);
            if (ret == 0L)
            { 
                // AdjustTokenPrivileges call failed
                return error.As(err);
            } 
            // AdjustTokenPrivileges call succeeded
            if (err == syscall.EINVAL)
            { 
                // GetLastError returned ERROR_SUCCESS
                return error.As(null);
            }
            return error.As(err);
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

        private static uint Size(this ref TOKEN_MANDATORY_LABEL tml)
        {
            return uint32(@unsafe.Sizeof(new TOKEN_MANDATORY_LABEL())) + syscall.GetLengthSid(tml.Label.Sid);
        }

        public static readonly ulong SE_GROUP_INTEGRITY = 0x00000020UL;



        public partial struct TokenType // : uint
        {
        }

        public static readonly TokenType TokenPrimary = 1L;
        public static readonly TokenType TokenImpersonation = 2L;
    }
}}}

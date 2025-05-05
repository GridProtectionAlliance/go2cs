// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
using @unsafe = unsafe_package;

partial class windows_package {

public static readonly UntypedInt SecurityAnonymous = 0;
public static readonly UntypedInt SecurityIdentification = 1;
public static readonly UntypedInt SecurityImpersonation = 2;
public static readonly UntypedInt SecurityDelegation = 3;

//sys	ImpersonateSelf(impersonationlevel uint32) (err error) = advapi32.ImpersonateSelf
//sys	RevertToSelf() (err error) = advapi32.RevertToSelf
public static readonly UntypedInt TOKEN_ADJUST_PRIVILEGES = /* 0x0020 */ 32;
public static readonly UntypedInt SE_PRIVILEGE_ENABLED = /* 0x00000002 */ 2;

[GoType] partial struct LUID {
    public uint32 LowPart;
    public int32 HighPart;
}

[GoType] partial struct LUID_AND_ATTRIBUTES {
    public LUID Luid;
    public uint32 Attributes;
}

[GoType] partial struct TOKEN_PRIVILEGES {
    public uint32 PrivilegeCount;
    public array<LUID_AND_ATTRIBUTES> Privileges = new(1);
}

//sys	OpenThreadToken(h syscall.Handle, access uint32, openasself bool, token *syscall.Token) (err error) = advapi32.OpenThreadToken
//sys	LookupPrivilegeValue(systemname *uint16, name *uint16, luid *LUID) (err error) = advapi32.LookupPrivilegeValueW
//sys	adjustTokenPrivileges(token syscall.Token, disableAllPrivileges bool, newstate *TOKEN_PRIVILEGES, buflen uint32, prevstate *TOKEN_PRIVILEGES, returnlen *uint32) (ret uint32, err error) [true] = advapi32.AdjustTokenPrivileges
public static error AdjustTokenPrivileges(syscall.Token token, bool disableAllPrivileges, ж<TOKEN_PRIVILEGES> Ꮡnewstate, uint32 buflen, ж<TOKEN_PRIVILEGES> Ꮡprevstate, ж<uint32> Ꮡreturnlen) {
    ref var newstate = ref Ꮡnewstate.val;
    ref var prevstate = ref Ꮡprevstate.val;
    ref var returnlen = ref Ꮡreturnlen.val;

    var (ret, err) = adjustTokenPrivileges(token, disableAllPrivileges, Ꮡnewstate, buflen, Ꮡprevstate, Ꮡreturnlen);
    if (ret == 0) {
        // AdjustTokenPrivileges call failed
        return err;
    }
    // AdjustTokenPrivileges call succeeded
    if (err == syscall.EINVAL) {
        // GetLastError returned ERROR_SUCCESS
        return default!;
    }
    return err;
}

//sys DuplicateTokenEx(hExistingToken syscall.Token, dwDesiredAccess uint32, lpTokenAttributes *syscall.SecurityAttributes, impersonationLevel uint32, tokenType TokenType, phNewToken *syscall.Token) (err error) = advapi32.DuplicateTokenEx
//sys SetTokenInformation(tokenHandle syscall.Token, tokenInformationClass uint32, tokenInformation uintptr, tokenInformationLength uint32) (err error) = advapi32.SetTokenInformation
[GoType] partial struct SID_AND_ATTRIBUTES {
    public ж<syscall_package.SID> Sid;
    public uint32 Attributes;
}

[GoType] partial struct TOKEN_MANDATORY_LABEL {
    public SID_AND_ATTRIBUTES Label;
}

[GoRecv] public static uint32 Size(this ref TOKEN_MANDATORY_LABEL tml) {
    return ((uint32)@unsafe.Sizeof(new TOKEN_MANDATORY_LABEL(nil))) + syscall.GetLengthSid(tml.Label.Sid);
}

public static readonly UntypedInt SE_GROUP_INTEGRITY = /* 0x00000020 */ 32;

[GoType("num:uint32")] partial struct TokenType;

public static readonly TokenType TokenPrimary = 1;
public static readonly TokenType TokenImpersonation = 2;

//sys	GetProfilesDirectory(dir *uint16, dirLen *uint32) (err error) = userenv.GetProfilesDirectoryW
public static readonly UntypedInt LG_INCLUDE_INDIRECT = /* 0x1 */ 1;
public static readonly UntypedInt MAX_PREFERRED_LENGTH = /* 0xFFFFFFFF */ 4294967295;

[GoType] partial struct LocalGroupUserInfo0 {
    public ж<uint16> Name;
}

[GoType] partial struct UserInfo4 {
    public ж<uint16> Name;
    public ж<uint16> Password;
    public uint32 PasswordAge;
    public uint32 Priv;
    public ж<uint16> HomeDir;
    public ж<uint16> Comment;
    public uint32 Flags;
    public ж<uint16> ScriptPath;
    public uint32 AuthFlags;
    public ж<uint16> FullName;
    public ж<uint16> UsrComment;
    public ж<uint16> Parms;
    public ж<uint16> Workstations;
    public uint32 LastLogon;
    public uint32 LastLogoff;
    public uint32 AcctExpires;
    public uint32 MaxStorage;
    public uint32 UnitsPerWeek;
    public ж<byte> LogonHours;
    public uint32 BadPwCount;
    public uint32 NumLogons;
    public ж<uint16> LogonServer;
    public uint32 CountryCode;
    public uint32 CodePage;
    public ж<syscall_package.SID> UserSid;
    public uint32 PrimaryGroupID;
    public ж<uint16> Profile;
    public ж<uint16> HomeDirDrive;
    public uint32 PasswordExpired;
}

//sys	NetUserGetLocalGroups(serverName *uint16, userName *uint16, level uint32, flags uint32, buf **byte, prefMaxLen uint32, entriesRead *uint32, totalEntries *uint32) (neterr error) = netapi32.NetUserGetLocalGroups

// GetSystemDirectory retrieves the path to current location of the system
// directory, which is typically, though not always, `C:\Windows\System32`.
//
//go:linkname GetSystemDirectory
public static partial @string GetSystemDirectory();

// Implemented in runtime package.

} // end windows_package

// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using fmt = fmt_package;
using windows = @internal.syscall.windows_package;
using registry = @internal.syscall.windows.registry_package;
using syscall = syscall_package;
using @unsafe = unsafe_package;
using @internal.syscall;
using @internal.syscall.windows;

partial class user_package {

internal static (bool, error) isDomainJoined() {
    ref var domain = ref heap<ж<uint16>>(out var Ꮡdomain);
    ref var status = ref heap(new uint32(), out var Ꮡstatus);
    var err = syscall.NetGetJoinInformation(nil, Ꮡdomain, Ꮡstatus);
    if (err != default!) {
        return (false, err);
    }
    syscall.NetApiBufferFree((ж<byte>)(uintptr)(new @unsafe.Pointer(domain)));
    return (status == syscall.NetSetupDomainName, default!);
}

internal static (@string, error) lookupFullNameDomain(@string domainAndUser) {
    return syscall.TranslateAccountName(domainAndUser,
        syscall.NameSamCompatible, syscall.NameDisplay, 50);
}

internal static (@string, error) lookupFullNameServer(@string servername, @string username) => func<(@string, error)>((defer, recover) => {
    var (s, e) = syscall.UTF16PtrFromString(servername);
    if (e != default!) {
        return ("", e);
    }
    (var u, e) = syscall.UTF16PtrFromString(username);
    if (e != default!) {
        return ("", e);
    }
    ref var p = ref heap<ж<byte>>(out var Ꮡp);
    e = syscall.NetUserGetInfo(s, u, 10, Ꮡp);
    if (e != default!) {
        return ("", e);
    }
    deferǃ(syscall.NetApiBufferFree, p, defer);
    var i = (ж<syscall.UserInfo10>)(uintptr)(new @unsafe.Pointer(p));
    return (windows.UTF16PtrToString((~i).FullName), default!);
});

internal static (@string, error) lookupFullName(@string domain, @string username, @string domainAndUser) {
    var (joined, err) = isDomainJoined();
    if (err == default! && joined) {
        var (nameΔ1, errΔ1) = lookupFullNameDomain(domainAndUser);
        if (errΔ1 == default!) {
            return (nameΔ1, default!);
        }
    }
    (var name, err) = lookupFullNameServer(domain, username);
    if (err == default!) {
        return (name, default!);
    }
    // domain worked neither as a domain nor as a server
    // could be domain server unavailable
    // pretend username is fullname
    return (username, default!);
}

// getProfilesDirectory retrieves the path to the root directory
// where user profiles are stored.
internal static (@string, error) getProfilesDirectory() {
    ref var n = ref heap<uint32>(out var Ꮡn);
    n = (uint32)100;
    while (ᐧ) {
        var b = new slice<uint16>((nint)(n));
        var e = windows.GetProfilesDirectory(Ꮡ(b, 0), Ꮡn);
        if (e == default!) {
            return (syscall.UTF16ToString(b), default!);
        }
        if (!AreEqual(e, syscall.ERROR_INSUFFICIENT_BUFFER)) {
            return ("", e);
        }
        if (n <= (uint32)len(b)) {
            return ("", e);
        }
    }
}

// lookupUsernameAndDomain obtains the username and domain for usid.
internal static (@string username, @string domain, error e) lookupUsernameAndDomain(ж<syscall.SID> Ꮡusid) {
    @string username = default!;
    @string domain = default!;
    error e = default!;

    (username, domain, var t, e) = Ꮡusid.LookupAccount(""u8);
    if (e != default!) {
        return ("", "", e);
    }
    if (t != syscall.SidTypeUser) {
        return ("", "", fmt.Errorf("user: should be user account type, not %d"u8, t));
    }
    return (username, domain, default!);
}

// findHomeDirInRegistry finds the user home path based on the uid.
internal static (@string dir, error e) findHomeDirInRegistry(@string uid) {
    @string dir = default!;
    error e = default!;
    func((defer, recover) => {
        (var k, e) = registry.OpenKey(registry.LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\"u8 + uid, registry.QUERY_VALUE);
        if (e != default!) {
            (dir, e) = ("", e); return;
        }
        defer(() => k.Close());
        (dir, _, e) = k.GetStringValue("ProfileImagePath"u8);
        if (e != default!) {
            (dir, e) = ("", e); return;
        }
        (dir, e) = (dir, default!);
    });
    return (dir, e);
}

// lookupGroupName accepts the name of a group and retrieves the group SID.
internal static (@string, error) lookupGroupName(@string groupname) {
    var (sid, _, t, e) = syscall.LookupSID(""u8, groupname);
    if (e != default!) {
        return ("", e);
    }
    // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-samr/7b2aeb27-92fc-41f6-8437-deb65d950921#gt_0387e636-5654-4910-9519-1f8326cf5ec0
    // SidTypeAlias should also be treated as a group type next to SidTypeGroup
    // and SidTypeWellKnownGroup:
    // "alias object -> resource group: A group object..."
    //
    // Tests show that "Administrators" can be considered of type SidTypeAlias.
    if (t != syscall.SidTypeGroup && t != syscall.SidTypeWellKnownGroup && t != syscall.SidTypeAlias) {
        return ("", fmt.Errorf("lookupGroupName: should be group account type, not %d"u8, t));
    }
    return sid.String();
}

// listGroupsForUsernameAndDomain accepts username and domain and retrieves
// a SID list of the local groups where this user is a member.
internal static unsafe (slice<@string>, error) listGroupsForUsernameAndDomain(@string username, @string domain) => func<(slice<@string>, error)>((defer, recover) => {
    // Check if both the domain name and user should be used.
    @string query = default!;
    var (joined, err) = isDomainJoined();
    if (err == default! && joined && len(domain) != 0){
        query = domain + @"\"u8 + username;
    } else {
        query = username;
    }
    (var q, err) = syscall.UTF16PtrFromString(query);
    if (err != default!) {
        return (default!, err);
    }
    ref var p0 = ref heap<ж<byte>>(out var Ꮡp0);
    ref var entriesRead = ref heap(new uint32(), out var ᏑentriesRead);
    ref var totalEntries = ref heap(new uint32(), out var ᏑtotalEntries);
    // https://learn.microsoft.com/en-us/windows/win32/api/lmaccess/nf-lmaccess-netusergetlocalgroups
    // NetUserGetLocalGroups() would return a list of LocalGroupUserInfo0
    // elements which hold the names of local groups where the user participates.
    // The list does not follow any sorting order.
    //
    // If no groups can be found for this user, NetUserGetLocalGroups() should
    // always return the SID of a single group called "None", which
    // also happens to be the primary group for the local user.
    err = windows.NetUserGetLocalGroups(nil, q, 0, windows.LG_INCLUDE_INDIRECT, Ꮡp0, windows.MAX_PREFERRED_LENGTH, ᏑentriesRead, ᏑtotalEntries);
    if (err != default!) {
        return (default!, err);
    }
    deferǃ(syscall.NetApiBufferFree, p0, defer);
    if (entriesRead == 0) {
        return (default!, fmt.Errorf("listGroupsForUsernameAndDomain: NetUserGetLocalGroups() returned an empty list for domain: %s, username: %s"u8, domain, username));
    }
    var entries = new slice<windows.LocalGroupUserInfo0>(new ReadOnlySpan<windows.LocalGroupUserInfo0>((windows.LocalGroupUserInfo0*)(uintptr)(new @unsafe.Pointer(p0)), (int)(entriesRead)));
    slice<@string> sids = default!;
    foreach (var (_, entry) in entries) {
        if (entry.Name == nil) {
            continue;
        }
        var (sid, errΔ1) = lookupGroupName(windows.UTF16PtrToString(entry.Name));
        if (errΔ1 != default!) {
            return (default!, errΔ1);
        }
        sids = append(sids, sid);
    }
    return (sids, default!);
});

internal static (ж<User>, error) newUser(@string uid, @string gid, @string dir, @string username, @string domain) {
    ref var domainAndUser = ref heap<@string>(out var ᏑdomainAndUser);
    domainAndUser = domain + @"\"u8 + username;
    ref var name = ref heap<@string>(out var Ꮡname);
    (name, var e) = lookupFullName(domain, username, domainAndUser);
    if (e != default!) {
        return (default!, e);
    }
    var u = Ꮡ(new User(
        Uid: uid,
        Gid: gid,
        Username: domainAndUser,
        Name: name,
        HomeDir: dir
    ));
    return (u, default!);
}

internal static nint userBuffer = 0;
internal static nint groupBuffer = 0;

internal static (ж<User>, error) current() => func<(ж<User>, error)>((defer, recover) => {
    var (t, e) = syscall.OpenCurrentProcessToken();
    if (e != default!) {
        return (default!, e);
    }
    defer(() => t.Close());
    (var u, e) = t.GetTokenUser();
    if (e != default!) {
        return (default!, e);
    }
    (var pg, e) = t.GetTokenPrimaryGroup();
    if (e != default!) {
        return (default!, e);
    }
    (var uid, e) = (~u).User.Sid.String();
    if (e != default!) {
        return (default!, e);
    }
    (var gid, e) = (~pg).PrimaryGroup.String();
    if (e != default!) {
        return (default!, e);
    }
    (var dir, e) = t.GetUserProfileDirectory();
    if (e != default!) {
        return (default!, e);
    }
    (var username, var domain, e) = lookupUsernameAndDomain((~u).User.Sid);
    if (e != default!) {
        return (default!, e);
    }
    return newUser(uid, gid, dir, username, domain);
});

// lookupUserPrimaryGroup obtains the primary group SID for a user using this method:
// https://support.microsoft.com/en-us/help/297951/how-to-use-the-primarygroupid-attribute-to-find-the-primary-group-for
// The method follows this formula: domainRID + "-" + primaryGroupRID
internal static (@string, error) lookupUserPrimaryGroup(@string username, @string domain) => func<(@string, error)>((defer, recover) => {
    // get the domain RID
    var (sid, _, t, e) = syscall.LookupSID(""u8, domain);
    if (e != default!) {
        return ("", e);
    }
    if (t != syscall.SidTypeDomain) {
        return ("", fmt.Errorf("lookupUserPrimaryGroup: should be domain account type, not %d"u8, t));
    }
    (var domainRID, e) = sid.String();
    if (e != default!) {
        return ("", e);
    }
    // If the user has joined a domain use the RID of the default primary group
    // called "Domain Users":
    // https://support.microsoft.com/en-us/help/243330/well-known-security-identifiers-in-windows-operating-systems
    // SID: S-1-5-21domain-513
    //
    // The correct way to obtain the primary group of a domain user is
    // probing the user primaryGroupID attribute in the server Active Directory:
    // https://learn.microsoft.com/en-us/windows/win32/adschema/a-primarygroupid
    //
    // Note that the primary group of domain users should not be modified
    // on Windows for performance reasons, even if it's possible to do that.
    // The .NET Developer's Guide to Directory Services Programming - Page 409
    // https://books.google.bg/books?id=kGApqjobEfsC&lpg=PA410&ots=p7oo-eOQL7&dq=primary%20group%20RID&hl=bg&pg=PA409#v=onepage&q&f=false
    var (joined, err) = isDomainJoined();
    if (err == default! && joined) {
        return (domainRID + "-513", default!);
    }
    // For non-domain users call NetUserGetInfo() with level 4, which
    // in this case would not have any network overhead.
    // The primary group should not change from RID 513 here either
    // but the group will be called "None" instead:
    // https://www.adampalmer.me/iodigitalsec/2013/08/10/windows-null-session-enumeration/
    // "Group 'None' (RID: 513)"
    (var u, e) = syscall.UTF16PtrFromString(username);
    if (e != default!) {
        return ("", e);
    }
    (var d, e) = syscall.UTF16PtrFromString(domain);
    if (e != default!) {
        return ("", e);
    }
    ref var p = ref heap<ж<byte>>(out var Ꮡp);
    e = syscall.NetUserGetInfo(d, u, 4, Ꮡp);
    if (e != default!) {
        return ("", e);
    }
    deferǃ(syscall.NetApiBufferFree, p, defer);
    var i = (ж<windows.UserInfo4>)(uintptr)(new @unsafe.Pointer(p));
    return (fmt.Sprintf("%s-%d"u8, domainRID, (~i).PrimaryGroupID), default!);
});

internal static (ж<User>, error) newUserFromSid(ж<syscall.SID> Ꮡusid) {
    var (username, domain, e) = lookupUsernameAndDomain(Ꮡusid);
    if (e != default!) {
        return (default!, e);
    }
    (var gid, e) = lookupUserPrimaryGroup(username, domain);
    if (e != default!) {
        return (default!, e);
    }
    (var uid, e) = Ꮡusid.String();
    if (e != default!) {
        return (default!, e);
    }
    // If this user has logged in at least once their home path should be stored
    // in the registry under the specified SID. References:
    // https://social.technet.microsoft.com/wiki/contents/articles/13895.how-to-remove-a-corrupted-user-profile-from-the-registry.aspx
    // https://support.asperasoft.com/hc/en-us/articles/216127438-How-to-delete-Windows-user-profiles
    //
    // The registry is the most reliable way to find the home path as the user
    // might have decided to move it outside of the default location,
    // (e.g. C:\users). Reference:
    // https://answers.microsoft.com/en-us/windows/forum/windows_7-security/how-do-i-set-a-home-directory-outside-cusers-for-a/aed68262-1bf4-4a4d-93dc-7495193a440f
    (var dir, e) = findHomeDirInRegistry(uid);
    if (e != default!) {
        // If the home path does not exist in the registry, the user might
        // have not logged in yet; fall back to using getProfilesDirectory().
        // Find the username based on a SID and append that to the result of
        // getProfilesDirectory(). The domain is not relevant here.
        (dir, e) = getProfilesDirectory();
        if (e != default!) {
            return (default!, e);
        }
        dir += @"\"u8 + username;
    }
    return newUser(uid, gid, dir, username, domain);
}

internal static (ж<User>, error) lookupUser(@string username) {
    var (sid, _, t, e) = syscall.LookupSID(""u8, username);
    if (e != default!) {
        return (default!, e);
    }
    if (t != syscall.SidTypeUser) {
        return (default!, fmt.Errorf("user: should be user account type, not %d"u8, t));
    }
    return newUserFromSid(sid);
}

internal static (ж<User>, error) lookupUserId(@string uid) {
    var (sid, e) = syscall.StringToSid(uid);
    if (e != default!) {
        return (default!, e);
    }
    return newUserFromSid(sid);
}

internal static (ж<Group>, error) lookupGroup(@string groupname) {
    ref var sid = ref heap<@string>(out var Ꮡsid);
    (sid, var err) = lookupGroupName(groupname);
    if (err != default!) {
        return (default!, err);
    }
    return (Ꮡ(new Group(Name: groupname, Gid: sid)), default!);
}

internal static (ж<Group>, error) lookupGroupId(@string gid) {
    var (sid, err) = syscall.StringToSid(gid);
    if (err != default!) {
        return (default!, err);
    }
    ref var groupname = ref heap<@string>(out var Ꮡgroupname);
    (groupname, _, var t, err) = sid.LookupAccount(""u8);
    if (err != default!) {
        return (default!, err);
    }
    if (t != syscall.SidTypeGroup && t != syscall.SidTypeWellKnownGroup && t != syscall.SidTypeAlias) {
        return (default!, fmt.Errorf("lookupGroupId: should be group account type, not %d"u8, t));
    }
    return (Ꮡ(new Group(Name: groupname, Gid: gid)), default!);
}

internal static (slice<@string>, error) listGroups(ж<User> Ꮡuser) {
    ref var user = ref Ꮡuser.Value;

    var (sid, err) = syscall.StringToSid(user.Uid);
    if (err != default!) {
        return (default!, err);
    }
    (var username, var domain, err) = lookupUsernameAndDomain(sid);
    if (err != default!) {
        return (default!, err);
    }
    (var sids, err) = listGroupsForUsernameAndDomain(username, domain);
    if (err != default!) {
        return (default!, err);
    }
    // Add the primary group of the user to the list if it is not already there.
    // This is done only to comply with the POSIX concept of a primary group.
    foreach (var (_, sidΔ1) in sids) {
        if (sidΔ1 == user.Gid) {
            return (sids, default!);
        }
    }
    return (append(sids, user.Gid), default!);
}

} // end user_package

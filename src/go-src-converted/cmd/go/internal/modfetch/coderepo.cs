// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modfetch -- go2cs converted at 2020 October 08 04:36:01 UTC
// import "cmd/go/internal/modfetch" ==> using modfetch = go.cmd.go.@internal.modfetch_package
// Original source: C:\Go\src\cmd\go\internal\modfetch\coderepo.go
using zip = go.archive.zip_package;
using bytes = go.bytes_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using path = go.path_package;
using sort = go.sort_package;
using strings = go.strings_package;
using time = go.time_package;

using codehost = go.cmd.go.@internal.modfetch.codehost_package;

using modfile = go.golang.org.x.mod.modfile_package;
using module = go.golang.org.x.mod.module_package;
using semver = go.golang.org.x.mod.semver_package;
using modzip = go.golang.org.x.mod.zip_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modfetch_package
    {
        // A codeRepo implements modfetch.Repo using an underlying codehost.Repo.
        private partial struct codeRepo
        {
            public @string modPath; // code is the repository containing this module.
            public codehost.Repo code; // codeRoot is the import path at the root of code.
            public @string codeRoot; // codeDir is the directory (relative to root) at which we expect to find the module.
// If pathMajor is non-empty and codeRoot is not the full modPath,
// then we look in both codeDir and codeDir/pathMajor[1:].
            public @string codeDir; // pathMajor is the suffix of modPath that indicates its major version,
// or the empty string if modPath is at major version 0 or 1.
//
// pathMajor is typically of the form "/vN", but possibly ".vN", or
// ".vN-unstable" for modules resolved using gopkg.in.
            public @string pathMajor; // pathPrefix is the prefix of modPath that excludes pathMajor.
// It is used only for logging.
            public @string pathPrefix; // pseudoMajor is the major version prefix to require when generating
// pseudo-versions for this module, derived from the module path. pseudoMajor
// is empty if the module path does not include a version suffix (that is,
// accepts either v0 or v1).
            public @string pseudoMajor;
        }

        // newCodeRepo returns a Repo that reads the source code for the module with the
        // given path, from the repo stored in code, with the root of the repo
        // containing the path given by codeRoot.
        private static (Repo, error) newCodeRepo(codehost.Repo code, @string codeRoot, @string path)
        {
            Repo _p0 = default;
            error _p0 = default!;

            if (!hasPathPrefix(path, codeRoot))
            {
                return (null, error.As(fmt.Errorf("mismatched repo: found %s for %s", codeRoot, path))!);
            }

            var (pathPrefix, pathMajor, ok) = module.SplitPathVersion(path);
            if (!ok)
            {
                return (null, error.As(fmt.Errorf("invalid module path %q", path))!);
            }

            if (codeRoot == path)
            {
                pathPrefix = path;
            }

            var pseudoMajor = module.PathMajorPrefix(pathMajor); 

            // Compute codeDir = bar, the subdirectory within the repo
            // corresponding to the module root.
            //
            // At this point we might have:
            //    path = github.com/rsc/foo/bar/v2
            //    codeRoot = github.com/rsc/foo
            //    pathPrefix = github.com/rsc/foo/bar
            //    pathMajor = /v2
            //    pseudoMajor = v2
            //
            // which gives
            //    codeDir = bar
            //
            // We know that pathPrefix is a prefix of path, and codeRoot is a prefix of
            // path, but codeRoot may or may not be a prefix of pathPrefix, because
            // codeRoot may be the entire path (in which case codeDir should be empty).
            // That occurs in two situations.
            //
            // One is when a go-import meta tag resolves the complete module path,
            // including the pathMajor suffix:
            //    path = nanomsg.org/go/mangos/v2
            //    codeRoot = nanomsg.org/go/mangos/v2
            //    pathPrefix = nanomsg.org/go/mangos
            //    pathMajor = /v2
            //    pseudoMajor = v2
            //
            // The other is similar: for gopkg.in only, the major version is encoded
            // with a dot rather than a slash, and thus can't be in a subdirectory.
            //    path = gopkg.in/yaml.v2
            //    codeRoot = gopkg.in/yaml.v2
            //    pathPrefix = gopkg.in/yaml
            //    pathMajor = .v2
            //    pseudoMajor = v2
            //
            @string codeDir = "";
            if (codeRoot != path)
            {
                if (!hasPathPrefix(pathPrefix, codeRoot))
                {
                    return (null, error.As(fmt.Errorf("repository rooted at %s cannot contain module %s", codeRoot, path))!);
                }

                codeDir = strings.Trim(pathPrefix[len(codeRoot)..], "/");

            }

            ptr<codeRepo> r = addr(new codeRepo(modPath:path,code:code,codeRoot:codeRoot,codeDir:codeDir,pathPrefix:pathPrefix,pathMajor:pathMajor,pseudoMajor:pseudoMajor,));

            return (r, error.As(null!)!);

        }

        private static @string ModulePath(this ptr<codeRepo> _addr_r)
        {
            ref codeRepo r = ref _addr_r.val;

            return r.modPath;
        }

        private static (slice<@string>, error) Versions(this ptr<codeRepo> _addr_r, @string prefix)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref codeRepo r = ref _addr_r.val;
 
            // Special case: gopkg.in/macaroon-bakery.v2-unstable
            // does not use the v2 tags (those are for macaroon-bakery.v2).
            // It has no possible tags at all.
            if (strings.HasPrefix(r.modPath, "gopkg.in/") && strings.HasSuffix(r.modPath, "-unstable"))
            {
                return (null, error.As(null!)!);
            }

            var p = prefix;
            if (r.codeDir != "")
            {
                p = r.codeDir + "/" + p;
            }

            var (tags, err) = r.code.Tags(p);
            if (err != null)
            {
                return (null, error.As(addr(new module.ModuleError(Path:r.modPath,Err:err,))!)!);
            }

            slice<@string> list = default;            slice<@string> incompatible = default;

            foreach (var (_, tag) in tags)
            {
                if (!strings.HasPrefix(tag, p))
                {
                    continue;
                }

                var v = tag;
                if (r.codeDir != "")
                {
                    v = v[len(r.codeDir) + 1L..];
                }

                if (v == "" || v != module.CanonicalVersion(v) || IsPseudoVersion(v))
                {
                    continue;
                }

                {
                    var err = module.CheckPathMajor(v, r.pathMajor);

                    if (err != null)
                    {
                        if (r.codeDir == "" && r.pathMajor == "" && semver.Major(v) > "v1")
                        {
                            incompatible = append(incompatible, v);
                        }

                        continue;

                    }

                }


                list = append(list, v);

            }
            SortVersions(list);
            SortVersions(incompatible);

            return r.appendIncompatibleVersions(list, incompatible);

        }

        // appendIncompatibleVersions appends "+incompatible" versions to list if
        // appropriate, returning the final list.
        //
        // The incompatible list contains candidate versions without the '+incompatible'
        // prefix.
        //
        // Both list and incompatible must be sorted in semantic order.
        private static (slice<@string>, error) appendIncompatibleVersions(this ptr<codeRepo> _addr_r, slice<@string> list, slice<@string> incompatible)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref codeRepo r = ref _addr_r.val;

            if (len(incompatible) == 0L || r.pathMajor != "")
            { 
                // No +incompatible versions are possible, so no need to check them.
                return (list, error.As(null!)!);

            }

            Func<@string, (bool, error)> versionHasGoMod = v =>
            {
                var (_, err) = r.code.ReadFile(v, "go.mod", codehost.MaxGoMod);
                if (err == null)
                {
                    return (true, error.As(null!)!);
                }

                if (!os.IsNotExist(err))
                {
                    return (false, error.As(addr(new module.ModuleError(Path:r.modPath,Err:err,))!)!);
                }

                return (false, error.As(null!)!);

            }
;

            if (len(list) > 0L)
            {
                var (ok, err) = versionHasGoMod(list[len(list) - 1L]);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

                if (ok)
                { 
                    // The latest compatible version has a go.mod file, so assume that all
                    // subsequent versions do as well, and do not include any +incompatible
                    // versions. Even if we are wrong, the author clearly intends module
                    // consumers to be on the v0/v1 line instead of a higher +incompatible
                    // version. (See https://golang.org/issue/34189.)
                    //
                    // We know of at least two examples where this behavior is desired
                    // (github.com/russross/blackfriday@v2.0.0 and
                    // github.com/libp2p/go-libp2p@v6.0.23), and (as of 2019-10-29) have no
                    // concrete examples for which it is undesired.
                    return (list, error.As(null!)!);

                }

            }

            @string lastMajor = default;            bool lastMajorHasGoMod = default;
            foreach (var (i, v) in incompatible)
            {
                var major = semver.Major(v);

                if (major != lastMajor)
                {
                    var rem = incompatible[i..];
                    var j = sort.Search(len(rem), j =>
                    {
                        return semver.Major(rem[j]) != major;
                    });
                    var latestAtMajor = rem[j - 1L];

                    error err = default!;
                    lastMajor = major;
                    lastMajorHasGoMod, err = versionHasGoMod(latestAtMajor);
                    if (err != null)
                    {
                        return (null, error.As(err)!);
                    }

                }

                if (lastMajorHasGoMod)
                { 
                    // The latest release of this major version has a go.mod file, so it is
                    // not allowed as +incompatible. It would be confusing to include some
                    // minor versions of this major version as +incompatible but require
                    // semantic import versioning for others, so drop all +incompatible
                    // versions for this major version.
                    //
                    // If we're wrong about a minor version in the middle, users will still be
                    // able to 'go get' specific tags for that version explicitly — they just
                    // won't appear in 'go list' or as the results for queries with inequality
                    // bounds.
                    continue;

                }

                list = append(list, v + "+incompatible");

            }
            return (list, error.As(null!)!);

        }

        private static (ptr<RevInfo>, error) Stat(this ptr<codeRepo> _addr_r, @string rev)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref codeRepo r = ref _addr_r.val;

            if (rev == "latest")
            {
                return _addr_r.Latest()!;
            }

            var codeRev = r.revToRev(rev);
            var (info, err) = r.code.Stat(codeRev);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new module.ModuleError(Path:r.modPath,Err:&module.InvalidVersionError{Version:rev,Err:err,},))!)!);
            }

            return _addr_r.convert(info, rev)!;

        }

        private static (ptr<RevInfo>, error) Latest(this ptr<codeRepo> _addr_r)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref codeRepo r = ref _addr_r.val;

            var (info, err) = r.code.Latest();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return _addr_r.convert(info, "")!;

        }

        // convert converts a version as reported by the code host to a version as
        // interpreted by the module system.
        //
        // If statVers is a valid module version, it is used for the Version field.
        // Otherwise, the Version is derived from the passed-in info and recent tags.
        private static (ptr<RevInfo>, error) convert(this ptr<codeRepo> _addr_r, ptr<codehost.RevInfo> _addr_info, @string statVers)
        {
            ptr<RevInfo> _p0 = default!;
            error _p0 = default!;
            ref codeRepo r = ref _addr_r.val;
            ref codehost.RevInfo info = ref _addr_info.val;

            ptr<RevInfo> info2 = addr(new RevInfo(Name:info.Name,Short:info.Short,Time:info.Time,)); 

            // If this is a plain tag (no dir/ prefix)
            // and the module path is unversioned,
            // and if the underlying file tree has no go.mod,
            // then allow using the tag with a +incompatible suffix.
            Func<bool> canUseIncompatible = default;
            canUseIncompatible = () =>
            {
                bool ok = default;
                if (r.codeDir == "" && r.pathMajor == "")
                {
                    var (_, errGoMod) = r.code.ReadFile(info.Name, "go.mod", codehost.MaxGoMod);
                    if (errGoMod != null)
                    {
                        ok = true;
                    }

                }

                canUseIncompatible = () => _addr_ok!;
                return _addr_ok!;

            }
;

            Func<@string, object[], error> invalidf = (format, args) =>
            {
                return addr(new module.ModuleError(Path:r.modPath,Err:&module.InvalidVersionError{Version:info2.Version,Err:fmt.Errorf(format,args...),},));
            } 

            // checkGoMod verifies that the go.mod file for the module exists or does not
            // exist as required by info2.Version and the module path represented by r.
; 

            // checkGoMod verifies that the go.mod file for the module exists or does not
            // exist as required by info2.Version and the module path represented by r.
            Func<(ptr<RevInfo>, error)> checkGoMod = () =>
            { 
                // If r.codeDir is non-empty, then the go.mod file must exist: the module
                // author — not the module consumer, — gets to decide how to carve up the repo
                // into modules.
                //
                // Conversely, if the go.mod file exists, the module author — not the module
                // consumer — gets to determine the module's path
                //
                // r.findDir verifies both of these conditions. Execute it now so that
                // r.Stat will correctly return a notExistError if the go.mod location or
                // declared module path doesn't match.
                var (_, _, _, err) = r.findDir(info2.Version);
                if (err != null)
                { 
                    // TODO: It would be nice to return an error like "not a module".
                    // Right now we return "missing go.mod", which is a little confusing.
                    return (_addr_null!, error.As(addr(new module.ModuleError(Path:r.modPath,Err:&module.InvalidVersionError{Version:info2.Version,Err:notExistError{err:err},},))!)!);

                } 

                // If the version is +incompatible, then the go.mod file must not exist:
                // +incompatible is not an ongoing opt-out from semantic import versioning.
                if (strings.HasSuffix(info2.Version, "+incompatible"))
                {
                    if (!canUseIncompatible())
                    {
                        if (r.pathMajor != "")
                        {
                            return (_addr_null!, error.As(invalidf("+incompatible suffix not allowed: module path includes a major version suffix, so major version must match"))!);
                        }
                        else
                        {
                            return (_addr_null!, error.As(invalidf("+incompatible suffix not allowed: module contains a go.mod file, so semantic import versioning is required"))!);
                        }

                    }

                    {
                        var err__prev2 = err;

                        var err = module.CheckPathMajor(strings.TrimSuffix(info2.Version, "+incompatible"), r.pathMajor);

                        if (err == null)
                        {
                            return (_addr_null!, error.As(invalidf("+incompatible suffix not allowed: major version %s is compatible", semver.Major(info2.Version)))!);
                        }

                        err = err__prev2;

                    }

                }

                return (_addr_info2!, error.As(null!)!);

            } 

            // Determine version.
            //
            // If statVers is canonical, then the original call was repo.Stat(statVers).
            // Since the version is canonical, we must not resolve it to anything but
            // itself, possibly with a '+incompatible' annotation: we do not need to do
            // the work required to look for an arbitrary pseudo-version.
; 

            // Determine version.
            //
            // If statVers is canonical, then the original call was repo.Stat(statVers).
            // Since the version is canonical, we must not resolve it to anything but
            // itself, possibly with a '+incompatible' annotation: we do not need to do
            // the work required to look for an arbitrary pseudo-version.
            if (statVers != "" && statVers == module.CanonicalVersion(statVers))
            {
                info2.Version = statVers;

                if (IsPseudoVersion(info2.Version))
                {
                    {
                        var err__prev3 = err;

                        err = r.validatePseudoVersion(info, info2.Version);

                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        err = err__prev3;

                    }

                    return _addr_checkGoMod()!;

                }

                {
                    var err__prev2 = err;

                    err = module.CheckPathMajor(info2.Version, r.pathMajor);

                    if (err != null)
                    {
                        if (canUseIncompatible())
                        {
                            info2.Version += "+incompatible";
                            return _addr_checkGoMod()!;
                        }
                        else
                        {
                            {
                                ptr<module.InvalidVersionError> (vErr, ok) = err._<ptr<module.InvalidVersionError>>();

                                if (ok)
                                { 
                                    // We're going to describe why the version is invalid in more detail,
                                    // so strip out the existing “invalid version” wrapper.
                                    err = vErr.Err;

                                }

                            }

                            return (_addr_null!, error.As(invalidf("module contains a go.mod file, so major version must be compatible: %v", err))!);

                        }

                    }

                    err = err__prev2;

                }


                return _addr_checkGoMod()!;

            } 

            // statVers is empty or non-canonical, so we need to resolve it to a canonical
            // version or pseudo-version.

            // Derive or verify a version from a code repo tag.
            // Tag must have a prefix matching codeDir.
            @string tagPrefix = "";
            if (r.codeDir != "")
            {
                tagPrefix = r.codeDir + "/";
            } 

            // tagToVersion returns the version obtained by trimming tagPrefix from tag.
            // If the tag is invalid or a pseudo-version, tagToVersion returns an empty
            // version.
            Func<@string, (@string, bool)> tagToVersion = tag =>
            {
                if (!strings.HasPrefix(tag, tagPrefix))
                {
                    return (_addr_""!, error.As(false)!);
                }

                var trimmed = tag[len(tagPrefix)..]; 
                // Tags that look like pseudo-versions would be confusing. Ignore them.
                if (IsPseudoVersion(tag))
                {
                    return (_addr_""!, error.As(false)!);
                }

                v = semver.Canonical(trimmed); // Not module.Canonical: we don't want to pick up an explicit "+incompatible" suffix from the tag.
                if (v == "" || !strings.HasPrefix(trimmed, v))
                {
                    return (_addr_""!, error.As(false)!); // Invalid or incomplete version (just vX or vX.Y).
                }

                if (v == trimmed)
                {
                    tagIsCanonical = true;
                }

                {
                    var err__prev1 = err;

                    err = module.CheckPathMajor(v, r.pathMajor);

                    if (err != null)
                    {
                        if (canUseIncompatible())
                        {
                            return (_addr_v + "+incompatible"!, error.As(tagIsCanonical)!);
                        }

                        return (_addr_""!, error.As(false)!);

                    }

                    err = err__prev1;

                }


                return (_addr_v!, error.As(tagIsCanonical)!);

            } 

            // If the VCS gave us a valid version, use that.
; 

            // If the VCS gave us a valid version, use that.
            {
                var v__prev1 = v;

                var (v, tagIsCanonical) = tagToVersion(info.Version);

                if (tagIsCanonical)
                {
                    info2.Version = v;
                    return _addr_checkGoMod()!;
                } 

                // Look through the tags on the revision for either a usable canonical version
                // or an appropriate base for a pseudo-version.

                v = v__prev1;

            } 

            // Look through the tags on the revision for either a usable canonical version
            // or an appropriate base for a pseudo-version.
            @string pseudoBase = default;
            foreach (var (_, pathTag) in info.Tags)
            {
                (v, tagIsCanonical) = tagToVersion(pathTag);
                if (tagIsCanonical)
                {
                    if (statVers != "" && semver.Compare(v, statVers) == 0L)
                    { 
                        // The user requested a non-canonical version, but the tag for the
                        // canonical equivalent refers to the same revision. Use it.
                        info2.Version = v;
                        return _addr_checkGoMod()!;

                    }
                    else
                    { 
                        // Save the highest canonical tag for the revision. If we don't find a
                        // better match, we'll use it as the canonical version.
                        //
                        // NOTE: Do not replace this with semver.Max. Despite the name,
                        // semver.Max *also* canonicalizes its arguments, which uses
                        // semver.Canonical instead of module.CanonicalVersion and thereby
                        // strips our "+incompatible" suffix.
                        if (semver.Compare(info2.Version, v) < 0L)
                        {
                            info2.Version = v;
                        }

                    }

                }
                else if (v != "" && semver.Compare(v, statVers) == 0L)
                { 
                    // The user explicitly requested something equivalent to this tag. We
                    // can't use the version from the tag directly: since the tag is not
                    // canonical, it could be ambiguous. For example, tags v0.0.1+a and
                    // v0.0.1+b might both exist and refer to different revisions.
                    //
                    // The tag is otherwise valid for the module, so we can at least use it as
                    // the base of an unambiguous pseudo-version.
                    //
                    // If multiple tags match, tagToVersion will canonicalize them to the same
                    // base version.
                    pseudoBase = v;

                }

            } 

            // If we found any canonical tag for the revision, return it.
            // Even if we found a good pseudo-version base, a canonical version is better.
            if (info2.Version != "")
            {
                return _addr_checkGoMod()!;
            }

            if (pseudoBase == "")
            {
                @string tag = default;
                if (r.pseudoMajor != "" || canUseIncompatible())
                {
                    tag, _ = r.code.RecentTag(info.Name, tagPrefix, r.pseudoMajor);
                }
                else
                { 
                    // Allow either v1 or v0, but not incompatible higher versions.
                    tag, _ = r.code.RecentTag(info.Name, tagPrefix, "v1");
                    if (tag == "")
                    {
                        tag, _ = r.code.RecentTag(info.Name, tagPrefix, "v0");
                    }

                }

                pseudoBase, _ = tagToVersion(tag); // empty if the tag is invalid
            }

            info2.Version = PseudoVersion(r.pseudoMajor, pseudoBase, info.Time, info.Short);
            return _addr_checkGoMod()!;

        }

        // validatePseudoVersion checks that version has a major version compatible with
        // r.modPath and encodes a base version and commit metadata that agrees with
        // info.
        //
        // Note that verifying a nontrivial base version in particular may be somewhat
        // expensive: in order to do so, r.code.DescendsFrom will need to fetch at least
        // enough of the commit history to find a path between version and its base.
        // Fortunately, many pseudo-versions — such as those for untagged repositories —
        // have trivial bases!
        private static error validatePseudoVersion(this ptr<codeRepo> _addr_r, ptr<codehost.RevInfo> _addr_info, @string version) => func((defer, _, __) =>
        {
            error err = default!;
            ref codeRepo r = ref _addr_r.val;
            ref codehost.RevInfo info = ref _addr_info.val;

            defer(() =>
            {
                if (err != null)
                {
                    {
                        ptr<module.ModuleError> (_, ok) = err._<ptr<module.ModuleError>>();

                        if (!ok)
                        {
                            {
                                (_, ok) = err._<ptr<module.InvalidVersionError>>();

                                if (!ok)
                                {
                                    err = addr(new module.InvalidVersionError(Version:version,Pseudo:true,Err:err));
                                }

                            }

                            err = addr(new module.ModuleError(Path:r.modPath,Err:err));

                        }

                    }

                }

            }());

            {
                var err = module.CheckPathMajor(version, r.pathMajor);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            var (rev, err) = PseudoVersionRev(version);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (rev != info.Short)
            {

                if (strings.HasPrefix(rev, info.Short)) 
                    return error.As(fmt.Errorf("revision is longer than canonical (%s)", info.Short))!;
                else if (strings.HasPrefix(info.Short, rev)) 
                    return error.As(fmt.Errorf("revision is shorter than canonical (%s)", info.Short))!;
                else 
                    return error.As(fmt.Errorf("does not match short name of revision (%s)", info.Short))!;
                
            }

            var (t, err) = PseudoVersionTime(version);
            if (err != null)
            {
                return error.As(err)!;
            }

            if (!t.Equal(info.Time.Truncate(time.Second)))
            {
                return error.As(fmt.Errorf("does not match version-control timestamp (expected %s)", info.Time.UTC().Format(pseudoVersionTimestampFormat)))!;
            }

            @string tagPrefix = "";
            if (r.codeDir != "")
            {
                tagPrefix = r.codeDir + "/";
            } 

            // A pseudo-version should have a precedence just above its parent revisions,
            // and no higher. Otherwise, it would be possible for library authors to "pin"
            // dependency versions (and bypass the usual minimum version selection) by
            // naming an extremely high pseudo-version rather than an accurate one.
            //
            // Moreover, if we allow a pseudo-version to use any arbitrary pre-release
            // tag, we end up with infinitely many possible names for each commit. Each
            // name consumes resources in the module cache and proxies, so we want to
            // restrict them to a finite set under control of the module author.
            //
            // We address both of these issues by requiring the tag upon which the
            // pseudo-version is based to refer to some ancestor of the revision. We
            // prefer the highest such tag when constructing a new pseudo-version, but do
            // not enforce that property when resolving existing pseudo-versions: we don't
            // know when the parent tags were added, and the highest-tagged parent may not
            // have existed when the pseudo-version was first resolved.
            var (base, err) = PseudoVersionBase(strings.TrimSuffix(version, "+incompatible"));
            if (err != null)
            {
                return error.As(err)!;
            }

            if (base == "")
            {
                if (r.pseudoMajor == "" && semver.Major(version) == "v1")
                {
                    return error.As(fmt.Errorf("major version without preceding tag must be v0, not v1"))!;
                }

                return error.As(null!)!;

            }
            else
            {
                {
                    var tag__prev1 = tag;

                    foreach (var (_, __tag) in info.Tags)
                    {
                        tag = __tag;
                        var versionOnly = strings.TrimPrefix(tag, tagPrefix);
                        if (versionOnly == base)
                        { 
                            // The base version is canonical, so if the version from the tag is
                            // literally equal (not just equivalent), then the tag is canonical too.
                            //
                            // We allow pseudo-versions to be derived from non-canonical tags on the
                            // same commit, so that tags like "v1.1.0+some-metadata" resolve as
                            // close as possible to the canonical version ("v1.1.0") while still
                            // enforcing a total ordering ("v1.1.1-0.[…]" with a unique suffix).
                            //
                            // However, canonical tags already have a total ordering, so there is no
                            // reason not to use the canonical tag directly, and we know that the
                            // canonical tag must already exist because the pseudo-version is
                            // derived from it. In that case, referring to the revision by a
                            // pseudo-version derived from its own canonical tag is just confusing.
                            return error.As(fmt.Errorf("tag (%s) found on revision %s is already canonical, so should not be replaced with a pseudo-version derived from that tag", tag, rev))!;

                        }

                    }

                    tag = tag__prev1;
                }
            }

            var (tags, err) = r.code.Tags(tagPrefix + base);
            if (err != null)
            {
                return error.As(err)!;
            }

            @string lastTag = default; // Prefer to log some real tag rather than a canonically-equivalent base.
            var ancestorFound = false;
            {
                var tag__prev1 = tag;

                foreach (var (_, __tag) in tags)
                {
                    tag = __tag;
                    versionOnly = strings.TrimPrefix(tag, tagPrefix);
                    if (semver.Compare(versionOnly, base) == 0L)
                    {
                        lastTag = tag;
                        ancestorFound, err = r.code.DescendsFrom(info.Name, tag);
                        if (ancestorFound)
                        {
                            break;
                        }

                    }

                }

                tag = tag__prev1;
            }

            if (lastTag == "")
            {
                return error.As(fmt.Errorf("preceding tag (%s) not found", base))!;
            }

            if (!ancestorFound)
            {
                if (err != null)
                {
                    return error.As(err)!;
                }

                (rev, err) = PseudoVersionRev(version);
                if (err != null)
                {
                    return error.As(fmt.Errorf("not a descendent of preceding tag (%s)", lastTag))!;
                }

                return error.As(fmt.Errorf("revision %s is not a descendent of preceding tag (%s)", rev, lastTag))!;

            }

            return error.As(null!)!;

        });

        private static @string revToRev(this ptr<codeRepo> _addr_r, @string rev)
        {
            ref codeRepo r = ref _addr_r.val;

            if (semver.IsValid(rev))
            {
                if (IsPseudoVersion(rev))
                {
                    var (r, _) = PseudoVersionRev(rev);
                    return r;
                }

                if (semver.Build(rev) == "+incompatible")
                {
                    rev = rev[..len(rev) - len("+incompatible")];
                }

                if (r.codeDir == "")
                {
                    return rev;
                }

                return r.codeDir + "/" + rev;

            }

            return rev;

        }

        private static (@string, error) versionToRev(this ptr<codeRepo> _addr_r, @string version)
        {
            @string rev = default;
            error err = default!;
            ref codeRepo r = ref _addr_r.val;

            if (!semver.IsValid(version))
            {
                return ("", error.As(addr(new module.ModuleError(Path:r.modPath,Err:&module.InvalidVersionError{Version:version,Err:errors.New("syntax error"),},))!)!);
            }

            return (r.revToRev(version), error.As(null!)!);

        }

        // findDir locates the directory within the repo containing the module.
        //
        // If r.pathMajor is non-empty, this can be either r.codeDir or — if a go.mod
        // file exists — r.codeDir/r.pathMajor[1:].
        private static (@string, @string, slice<byte>, error) findDir(this ptr<codeRepo> _addr_r, @string version)
        {
            @string rev = default;
            @string dir = default;
            slice<byte> gomod = default;
            error err = default!;
            ref codeRepo r = ref _addr_r.val;

            rev, err = r.versionToRev(version);
            if (err != null)
            {
                return ("", "", null, error.As(err)!);
            } 

            // Load info about go.mod but delay consideration
            // (except I/O error) until we rule out v2/go.mod.
            var file1 = path.Join(r.codeDir, "go.mod");
            var (gomod1, err1) = r.code.ReadFile(rev, file1, codehost.MaxGoMod);
            if (err1 != null && !os.IsNotExist(err1))
            {
                return ("", "", null, error.As(fmt.Errorf("reading %s/%s at revision %s: %v", r.pathPrefix, file1, rev, err1))!);
            }

            var mpath1 = modfile.ModulePath(gomod1);
            var found1 = err1 == null && (isMajor(mpath1, r.pathMajor) || r.canReplaceMismatchedVersionDueToBug(mpath1));

            @string file2 = default;
            if (r.pathMajor != "" && r.codeRoot != r.modPath && !strings.HasPrefix(r.pathMajor, "."))
            { 
                // Suppose pathMajor is "/v2".
                // Either go.mod should claim v2 and v2/go.mod should not exist,
                // or v2/go.mod should exist and claim v2. Not both.
                // Note that we don't check the full path, just the major suffix,
                // because of replacement modules. This might be a fork of
                // the real module, found at a different path, usable only in
                // a replace directive.
                var dir2 = path.Join(r.codeDir, r.pathMajor[1L..]);
                file2 = path.Join(dir2, "go.mod");
                var (gomod2, err2) = r.code.ReadFile(rev, file2, codehost.MaxGoMod);
                if (err2 != null && !os.IsNotExist(err2))
                {
                    return ("", "", null, error.As(fmt.Errorf("reading %s/%s at revision %s: %v", r.pathPrefix, file2, rev, err2))!);
                }

                var mpath2 = modfile.ModulePath(gomod2);
                var found2 = err2 == null && isMajor(mpath2, r.pathMajor);

                if (found1 && found2)
                {
                    return ("", "", null, error.As(fmt.Errorf("%s/%s and ...%s/go.mod both have ...%s module paths at revision %s", r.pathPrefix, file1, r.pathMajor, r.pathMajor, rev))!);
                }

                if (found2)
                {
                    return (rev, dir2, gomod2, error.As(null!)!);
                }

                if (err2 == null)
                {
                    if (mpath2 == "")
                    {
                        return ("", "", null, error.As(fmt.Errorf("%s/%s is missing module path at revision %s", r.pathPrefix, file2, rev))!);
                    }

                    return ("", "", null, error.As(fmt.Errorf("%s/%s has non-...%s module path %q at revision %s", r.pathPrefix, file2, r.pathMajor, mpath2, rev))!);

                }

            } 

            // Not v2/go.mod, so it's either go.mod or nothing. Which is it?
            if (found1)
            { 
                // Explicit go.mod with matching major version ok.
                return (rev, r.codeDir, gomod1, error.As(null!)!);

            }

            if (err1 == null)
            { 
                // Explicit go.mod with non-matching major version disallowed.
                @string suffix = "";
                if (file2 != "")
                {
                    suffix = fmt.Sprintf(" (and ...%s/go.mod does not exist)", r.pathMajor);
                }

                if (mpath1 == "")
                {
                    return ("", "", null, error.As(fmt.Errorf("%s is missing module path%s at revision %s", file1, suffix, rev))!);
                }

                if (r.pathMajor != "")
                { // ".v1", ".v2" for gopkg.in
                    return ("", "", null, error.As(fmt.Errorf("%s has non-...%s module path %q%s at revision %s", file1, r.pathMajor, mpath1, suffix, rev))!);

                }

                {
                    var (_, _, ok) = module.SplitPathVersion(mpath1);

                    if (!ok)
                    {
                        return ("", "", null, error.As(fmt.Errorf("%s has malformed module path %q%s at revision %s", file1, mpath1, suffix, rev))!);
                    }

                }

                return ("", "", null, error.As(fmt.Errorf("%s has post-%s module path %q%s at revision %s", file1, semver.Major(version), mpath1, suffix, rev))!);

            }

            if (r.codeDir == "" && (r.pathMajor == "" || strings.HasPrefix(r.pathMajor, ".")))
            { 
                // Implicit go.mod at root of repo OK for v0/v1 and for gopkg.in.
                return (rev, "", null, error.As(null!)!);

            } 

            // Implicit go.mod below root of repo or at v2+ disallowed.
            // Be clear about possibility of using either location for v2+.
            if (file2 != "")
            {
                return ("", "", null, error.As(fmt.Errorf("missing %s/go.mod and ...%s/go.mod at revision %s", r.pathPrefix, r.pathMajor, rev))!);
            }

            return ("", "", null, error.As(fmt.Errorf("missing %s/go.mod at revision %s", r.pathPrefix, rev))!);

        }

        // isMajor reports whether the versions allowed for mpath are compatible with
        // the major version(s) implied by pathMajor, or false if mpath has an invalid
        // version suffix.
        private static bool isMajor(@string mpath, @string pathMajor)
        {
            if (mpath == "")
            { 
                // If we don't have a path, we don't know what version(s) it is compatible with.
                return false;

            }

            var (_, mpathMajor, ok) = module.SplitPathVersion(mpath);
            if (!ok)
            { 
                // An invalid module path is not compatible with any version.
                return false;

            }

            if (pathMajor == "")
            { 
                // All of the valid versions for a gopkg.in module that requires major
                // version v0 or v1 are compatible with the "v0 or v1" implied by an empty
                // pathMajor.
                switch (module.PathMajorPrefix(mpathMajor))
                {
                    case "": 

                    case "v0": 

                    case "v1": 
                        return true;
                        break;
                    default: 
                        return false;
                        break;
                }

            }

            if (mpathMajor == "")
            { 
                // Even if pathMajor is ".v0" or ".v1", we can't be sure that a module
                // without a suffix is tagged appropriately. Besides, we don't expect clones
                // of non-gopkg.in modules to have gopkg.in paths, so a non-empty,
                // non-gopkg.in mpath is probably the wrong module for any such pathMajor
                // anyway.
                return false;

            } 
            // If both pathMajor and mpathMajor are non-empty, then we only care that they
            // have the same major-version validation rules. A clone fetched via a /v2
            // path might replace a module with path gopkg.in/foo.v2-unstable, and that's
            // ok.
            return pathMajor[1L..] == mpathMajor[1L..];

        }

        // canReplaceMismatchedVersionDueToBug reports whether versions of r
        // could replace versions of mpath with otherwise-mismatched major versions
        // due to a historical bug in the Go command (golang.org/issue/34254).
        private static bool canReplaceMismatchedVersionDueToBug(this ptr<codeRepo> _addr_r, @string mpath)
        {
            ref codeRepo r = ref _addr_r.val;
 
            // The bug caused us to erroneously accept unversioned paths as replacements
            // for versioned gopkg.in paths.
            var unversioned = r.pathMajor == "";
            var replacingGopkgIn = strings.HasPrefix(mpath, "gopkg.in/");
            return unversioned && replacingGopkgIn;

        }

        private static (slice<byte>, error) GoMod(this ptr<codeRepo> _addr_r, @string version)
        {
            slice<byte> data = default;
            error err = default!;
            ref codeRepo r = ref _addr_r.val;

            if (version != module.CanonicalVersion(version))
            {
                return (null, error.As(fmt.Errorf("version %s is not canonical", version))!);
            }

            if (IsPseudoVersion(version))
            { 
                // findDir ignores the metadata encoded in a pseudo-version,
                // only using the revision at the end.
                // Invoke Stat to verify the metadata explicitly so we don't return
                // a bogus file for an invalid version.
                var (_, err) = r.Stat(version);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }

            }

            var (rev, dir, gomod, err) = r.findDir(version);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (gomod != null)
            {
                return (gomod, error.As(null!)!);
            }

            data, err = r.code.ReadFile(rev, path.Join(dir, "go.mod"), codehost.MaxGoMod);
            if (err != null)
            {
                if (os.IsNotExist(err))
                {
                    return (r.legacyGoMod(rev, dir), error.As(null!)!);
                }

                return (null, error.As(err)!);

            }

            return (data, error.As(null!)!);

        }

        private static slice<byte> legacyGoMod(this ptr<codeRepo> _addr_r, @string rev, @string dir)
        {
            ref codeRepo r = ref _addr_r.val;
 
            // We used to try to build a go.mod reflecting pre-existing
            // package management metadata files, but the conversion
            // was inherently imperfect (because those files don't have
            // exactly the same semantics as go.mod) and, when done
            // for dependencies in the middle of a build, impossible to
            // correct. So we stopped.
            // Return a fake go.mod that simply declares the module path.
            return (slice<byte>)fmt.Sprintf("module %s\n", modfile.AutoQuote(r.modPath));

        }

        private static @string modPrefix(this ptr<codeRepo> _addr_r, @string rev)
        {
            ref codeRepo r = ref _addr_r.val;

            return r.modPath + "@" + rev;
        }

        private static error Zip(this ptr<codeRepo> _addr_r, io.Writer dst, @string version) => func((defer, _, __) =>
        {
            ref codeRepo r = ref _addr_r.val;

            if (version != module.CanonicalVersion(version))
            {
                return error.As(fmt.Errorf("version %s is not canonical", version))!;
            }

            if (IsPseudoVersion(version))
            { 
                // findDir ignores the metadata encoded in a pseudo-version,
                // only using the revision at the end.
                // Invoke Stat to verify the metadata explicitly so we don't return
                // a bogus file for an invalid version.
                var (_, err) = r.Stat(version);
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            var (rev, subdir, _, err) = r.findDir(version);
            if (err != null)
            {
                return error.As(err)!;
            }

            var (dl, err) = r.code.ReadZip(rev, subdir, codehost.MaxZipFile);
            if (err != null)
            {
                return error.As(err)!;
            }

            defer(dl.Close());
            subdir = strings.Trim(subdir, "/"); 

            // Spool to local file.
            var (f, err) = ioutil.TempFile("", "go-codehost-");
            if (err != null)
            {
                dl.Close();
                return error.As(err)!;
            }

            defer(os.Remove(f.Name()));
            defer(f.Close());
            var maxSize = int64(codehost.MaxZipFile);
            ptr<io.LimitedReader> lr = addr(new io.LimitedReader(R:dl,N:maxSize+1));
            {
                (_, err) = io.Copy(f, lr);

                if (err != null)
                {
                    dl.Close();
                    return error.As(err)!;
                }

            }

            dl.Close();
            if (lr.N <= 0L)
            {
                return error.As(fmt.Errorf("downloaded zip file too large"))!;
            }

            var size = (maxSize + 1L) - lr.N;
            {
                (_, err) = f.Seek(0L, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // Translate from zip file we have to zip file we want.

            } 

            // Translate from zip file we have to zip file we want.
            var (zr, err) = zip.NewReader(f, size);
            if (err != null)
            {
                return error.As(err)!;
            }

            slice<modzip.File> files = default;
            if (subdir != "")
            {
                subdir += "/";
            }

            var haveLICENSE = false;
            @string topPrefix = "";
            foreach (var (_, zf) in zr.File)
            {
                if (topPrefix == "")
                {
                    var i = strings.Index(zf.Name, "/");
                    if (i < 0L)
                    {
                        return error.As(fmt.Errorf("missing top-level directory prefix"))!;
                    }

                    topPrefix = zf.Name[..i + 1L];

                }

                if (!strings.HasPrefix(zf.Name, topPrefix))
                {
                    return error.As(fmt.Errorf("zip file contains more than one top-level directory"))!;
                }

                var name = strings.TrimPrefix(zf.Name, topPrefix);
                if (!strings.HasPrefix(name, subdir))
                {
                    continue;
                }

                name = strings.TrimPrefix(name, subdir);
                if (name == "" || strings.HasSuffix(name, "/"))
                {
                    continue;
                }

                files = append(files, new zipFile(name:name,f:zf));
                if (name == "LICENSE")
                {
                    haveLICENSE = true;
                }

            }
            if (!haveLICENSE && subdir != "")
            {
                var (data, err) = r.code.ReadFile(rev, "LICENSE", codehost.MaxLICENSE);
                if (err == null)
                {
                    files = append(files, new dataFile(name:"LICENSE",data:data));
                }

            }

            return error.As(modzip.Create(dst, new module.Version(Path:r.modPath,Version:version), files))!;

        });

        private partial struct zipFile
        {
            public @string name;
            public ptr<zip.File> f;
        }

        private static @string Path(this zipFile f)
        {
            return f.name;
        }
        private static (os.FileInfo, error) Lstat(this zipFile f)
        {
            os.FileInfo _p0 = default;
            error _p0 = default!;

            return (f.f.FileInfo(), error.As(null!)!);
        }
        private static (io.ReadCloser, error) Open(this zipFile f)
        {
            io.ReadCloser _p0 = default;
            error _p0 = default!;

            return f.f.Open();
        }

        private partial struct dataFile
        {
            public @string name;
            public slice<byte> data;
        }

        private static @string Path(this dataFile f)
        {
            return f.name;
        }
        private static (os.FileInfo, error) Lstat(this dataFile f)
        {
            os.FileInfo _p0 = default;
            error _p0 = default!;

            return (new dataFileInfo(f), error.As(null!)!);
        }
        private static (io.ReadCloser, error) Open(this dataFile f)
        {
            io.ReadCloser _p0 = default;
            error _p0 = default!;

            return (ioutil.NopCloser(bytes.NewReader(f.data)), error.As(null!)!);
        }

        private partial struct dataFileInfo
        {
            public dataFile f;
        }

        private static @string Name(this dataFileInfo fi)
        {
            return path.Base(fi.f.name);
        }
        private static long Size(this dataFileInfo fi)
        {
            return int64(len(fi.f.data));
        }
        private static os.FileMode Mode(this dataFileInfo fi)
        {
            return 0644L;
        }
        private static time.Time ModTime(this dataFileInfo fi)
        {
            return new time.Time();
        }
        private static bool IsDir(this dataFileInfo fi)
        {
            return false;
        }
        private static void Sys(this dataFileInfo fi)
        {
            return null;
        }

        // hasPathPrefix reports whether the path s begins with the
        // elements in prefix.
        private static bool hasPathPrefix(@string s, @string prefix)
        {

            if (len(s) == len(prefix)) 
                return s == prefix;
            else if (len(s) > len(prefix)) 
                if (prefix != "" && prefix[len(prefix) - 1L] == '/')
                {
                    return strings.HasPrefix(s, prefix);
                }

                return s[len(prefix)] == '/' && s[..len(prefix)] == prefix;
            else 
                return false;
            
        }
    }
}}}}

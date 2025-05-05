// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using cmp = cmp_package;
using fmt = fmt_package;
using coverage = @internal.coverage_package;
using os = os_package;
using filepath = path.filepath_package;
using regexp = regexp_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using @internal;
using path;
using ꓸꓸꓸany = Span<any>;

partial class pods_package {

// Pod encapsulates a set of files emitted during the executions of a
// coverage-instrumented binary. Each pod contains a single meta-data
// file, and then 0 or more counter data files that refer to that
// meta-data file. Pods are intended to simplify processing of
// coverage output files in the case where we have several coverage
// output directories containing output files derived from more
// than one instrumented executable. In the case where the files that
// make up a pod are spread out across multiple directories, each
// element of the "Origins" field below will be populated with the
// index of the originating directory for the corresponding counter
// data file (within the slice of input dirs handed to CollectPods).
// The ProcessIDs field will be populated with the process ID of each
// data file in the CounterDataFiles slice.
[GoType] partial struct Pod {
    public @string MetaFile;
    public slice<@string> CounterDataFiles;
    public slice<nint> Origins;
    public slice<nint> ProcessIDs;
}

// CollectPods visits the files contained within the directories in
// the list 'dirs', collects any coverage-related files, partitions
// them into pods, and returns a list of the pods to the caller, along
// with an error if something went wrong during directory/file
// reading.
//
// CollectPods skips over any file that is not related to coverage
// (e.g. avoids looking at things that are not meta-data files or
// counter-data files). CollectPods also skips over 'orphaned' counter
// data files (e.g. counter data files for which we can't find the
// corresponding meta-data file). If "warn" is true, CollectPods will
// issue warnings to stderr when it encounters non-fatal problems (for
// orphans or a directory with no meta-data files).
public static (slice<Pod>, error) CollectPods(slice<@string> dirs, bool warn) {
    var files = new @string[]{}.slice();
    var dirIndices = new nint[]{}.slice();
    foreach (var (k, dir) in dirs) {
        (dents, err) = os.ReadDir(dir);
        if (err != default!) {
            return (default!, err);
        }
        foreach (var (_, e) in dents) {
            if (e.IsDir()) {
                continue;
            }
            files = append(files, filepath.Join(dir, e.Name()));
            dirIndices = append(dirIndices, k);
        }
    }
    return (collectPodsImpl(files, dirIndices, warn), default!);
}

// CollectPodsFromFiles functions the same as "CollectPods" but
// operates on an explicit list of files instead of a directory.
public static slice<Pod> CollectPodsFromFiles(slice<@string> files, bool warn) {
    return collectPodsImpl(files, default!, warn);
}

[GoType] partial struct fileWithAnnotations {
    internal @string file;
    internal nint origin;
    internal nint pid;
}

[GoType] partial struct protoPod {
    internal @string mf;
    internal slice<fileWithAnnotations> elements;
}

// collectPodsImpl examines the specified list of files and picks out
// subsets that correspond to coverage pods. The first stage in this
// process is collecting a set { M1, M2, ... MN } where each M_k is a
// distinct coverage meta-data file. We then create a single pod for
// each meta-data file M_k, then find all of the counter data files
// that refer to that meta-data file (recall that the counter data
// file name incorporates the meta-data hash), and add the counter
// data file to the appropriate pod.
//
// This process is complicated by the fact that we need to keep track
// of directory indices for counter data files. Here is an example to
// motivate:
//
//	directory 1:
//
// M1   covmeta.9bbf1777f47b3fcacb05c38b035512d6
// C1   covcounters.9bbf1777f47b3fcacb05c38b035512d6.1677673.1662138360208416486
// C2   covcounters.9bbf1777f47b3fcacb05c38b035512d6.1677637.1662138359974441782
//
//	directory 2:
//
// M2   covmeta.9bbf1777f47b3fcacb05c38b035512d6
// C3   covcounters.9bbf1777f47b3fcacb05c38b035512d6.1677445.1662138360208416480
// C4   covcounters.9bbf1777f47b3fcacb05c38b035512d6.1677677.1662138359974441781
// M3   covmeta.a723844208cea2ae80c63482c78b2245
// C5   covcounters.a723844208cea2ae80c63482c78b2245.3677445.1662138360208416480
// C6   covcounters.a723844208cea2ae80c63482c78b2245.1877677.1662138359974441781
//
// In these two directories we have three meta-data files, but only
// two are distinct, meaning that we'll wind up with two pods. The
// first pod (with meta-file M1) will have four counter data files
// (C1, C2, C3, C4) and the second pod will have two counter data files
// (C5, C6).
internal static slice<Pod> collectPodsImpl(slice<@string> files, slice<nint> dirIndices, bool warn) {
    var metaRE = regexp.MustCompile(fmt.Sprintf(@"^%s\.(\S+)$"u8, coverage.MetaFilePref));
    var mm = new map<@string, protoPod>();
    foreach (var (_, f) in files) {
        @string @base = filepath.Base(f);
        {
            var m = metaRE.FindStringSubmatch(@base); if (m != default!) {
                @string tag = m[1];
                // We need to allow for the possibility of duplicate
                // meta-data files. If we hit this case, use the
                // first encountered as the canonical version.
                {
                    var (_, ok) = mm[tag]; if (!ok) {
                        mm[tag] = new protoPod(mf: f);
                    }
                }
            }
        }
    }
    // FIXME: should probably check file length and hash here for
    // the duplicate.
    var counterRE = regexp.MustCompile(fmt.Sprintf(coverage.CounterFileRegexp, coverage.CounterFilePref));
    foreach (var (k, f) in files) {
        @string @base = filepath.Base(f);
        {
            var m = counterRE.FindStringSubmatch(@base); if (m != default!) {
                @string tag = m[1];
                // meta hash
                var (pid, err) = strconv.Atoi(m[2]);
                if (err != default!) {
                    continue;
                }
                {
                    var (v, ok) = mm[tag]; if (ok){
                        nint idx = -1;
                        if (dirIndices != default!) {
                            idx = dirIndices[k];
                        }
                        var fo = new fileWithAnnotations(file: f, origin: idx, pid: pid);
                        v.elements = append(v.elements, fo);
                        mm[tag] = v;
                    } else {
                        if (warn) {
                            warning("skipping orphaned counter file: %s"u8, f);
                        }
                    }
                }
            }
        }
    }
    if (len(mm) == 0) {
        if (warn) {
            warning("no coverage data files found"u8);
        }
        return default!;
    }
    var pods = new slice<Pod>(0, len(mm));
    foreach (var (_, p) in mm) {
        slices.SortFunc(p.elements, (fileWithAnnotations a, fileWithAnnotations b) => {
            {
                nint r = cmp.Compare(a.origin, b.origin); if (r != 0) {
                    return r;
                }
            }
            return strings.Compare(a.file, b.file);
        });
        var pod = new Pod(
            MetaFile: p.mf,
            CounterDataFiles: new slice<@string>(0, len(p.elements)),
            Origins: new slice<nint>(0, len(p.elements)),
            ProcessIDs: new slice<nint>(0, len(p.elements))
        );
        foreach (var (_, e) in p.elements) {
            pod.CounterDataFiles = append(pod.CounterDataFiles, e.file);
            pod.Origins = append(pod.Origins, e.origin);
            pod.ProcessIDs = append(pod.ProcessIDs, e.pid);
        }
        pods = append(pods, pod);
    }
    slices.SortFunc(pods, (Pod a, Pod b) => strings.Compare(a.MetaFile, b.MetaFile));
    return pods;
}

internal static void warning(@string s, params ꓸꓸꓸany aʗp) {
    var a = aʗp.slice();

    fmt.Fprintf(~os.Stderr, "warning: "u8);
    fmt.Fprintf(~os.Stderr, s, a.ꓸꓸꓸ);
    fmt.Fprintf(~os.Stderr, "\n"u8);
}

} // end pods_package

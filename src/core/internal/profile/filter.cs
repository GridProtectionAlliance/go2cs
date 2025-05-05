// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implements methods to filter samples from profiles.
namespace go.@internal;

partial class profile_package {

public delegate bool TagMatch(@string key, @string val, int64 nval);

// FilterSamplesByTag removes all samples from the profile, except
// those that match focus and do not match the ignore regular
// expression.
[GoRecv] public static (bool fm, bool im) FilterSamplesByTag(this ref Profile p, TagMatch focus, TagMatch ignore) {
    bool fm = default!;
    bool im = default!;

    var samples = new slice<ж<Sample>>(0, len(p.Sample));
    foreach (var (_, s) in p.Sample) {
        var (focused, ignored) = focusedSample(s, focus, ignore);
        fm = fm || focused;
        im = im || ignored;
        if (focused && !ignored) {
            samples = append(samples, s);
        }
    }
    p.Sample = samples;
    return (fm, im);
}

// focusedSample checks a sample against focus and ignore regexps.
// Returns whether the focus/ignore regexps match any tags.
internal static (bool fm, bool im) focusedSample(ж<Sample> Ꮡs, TagMatch focus, TagMatch ignore) {
    bool fm = default!;
    bool im = default!;

    ref var s = ref Ꮡs.val;
    fm = focus == default!;
    foreach (var (key, vals) in s.Label) {
        foreach (var (_, val) in vals) {
            if (ignore != default! && ignore(key, val, 0)) {
                im = true;
            }
            if (!fm && focus(key, val, 0)) {
                fm = true;
            }
        }
    }
    foreach (var (key, vals) in s.NumLabel) {
        foreach (var (_, val) in vals) {
            if (ignore != default! && ignore(key, ""u8, val)) {
                im = true;
            }
            if (!fm && focus(key, ""u8, val)) {
                fm = true;
            }
        }
    }
    return (fm, im);
}

} // end profile_package

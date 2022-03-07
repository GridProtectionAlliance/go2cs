// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package profile -- go2cs converted at 2022 March 06 23:23:55 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\profile\filter.go
// Implements methods to filter samples from profiles.

using regexp = go.regexp_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof;

public static partial class profile_package {

    // FilterSamplesByName filters the samples in a profile and only keeps
    // samples where at least one frame matches focus but none match ignore.
    // Returns true is the corresponding regexp matched at least one sample.
private static (bool, bool, bool, bool) FilterSamplesByName(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_focus, ptr<regexp.Regexp> _addr_ignore, ptr<regexp.Regexp> _addr_hide, ptr<regexp.Regexp> _addr_show) {
    bool fm = default;
    bool im = default;
    bool hm = default;
    bool hnm = default;
    ref Profile p = ref _addr_p.val;
    ref regexp.Regexp focus = ref _addr_focus.val;
    ref regexp.Regexp ignore = ref _addr_ignore.val;
    ref regexp.Regexp hide = ref _addr_hide.val;
    ref regexp.Regexp show = ref _addr_show.val;

    var focusOrIgnore = make_map<ulong, bool>();
    var hidden = make_map<ulong, bool>();
    foreach (var (_, l) in p.Location) {
        if (ignore != null && l.matchesName(ignore)) {
            im = true;
            focusOrIgnore[l.ID] = false;
        }
        else if (focus == null || l.matchesName(focus)) {
            fm = true;
            focusOrIgnore[l.ID] = true;
        }
        if (hide != null && l.matchesName(hide)) {
            hm = true;
            l.Line = l.unmatchedLines(hide);
            if (len(l.Line) == 0) {
                hidden[l.ID] = true;
            }
        }
        if (show != null) {
            l.Line = l.matchedLines(show);
            if (len(l.Line) == 0) {
                hidden[l.ID] = true;
            }
            else
 {
                hnm = true;
            }
        }
    }    var s = make_slice<ptr<Sample>>(0, len(p.Sample));
    foreach (var (_, sample) in p.Sample) {
        if (focusedAndNotIgnored(sample.Location, focusOrIgnore)) {
            if (len(hidden) > 0) {
                slice<ptr<Location>> locs = default;
                foreach (var (_, loc) in sample.Location) {
                    if (!hidden[loc.ID]) {
                        locs = append(locs, loc);
                    }
                }                if (len(locs) == 0) { 
                    // Remove sample with no locations (by not adding it to s).
                    continue;

                }
                sample.Location = locs;

            }
            s = append(s, sample);

        }
    }    p.Sample = s;

    return ;

}

// ShowFrom drops all stack frames above the highest matching frame and returns
// whether a match was found. If showFrom is nil it returns false and does not
// modify the profile.
//
// Example: consider a sample with frames [A, B, C, B], where A is the root.
// ShowFrom(nil) returns false and has frames [A, B, C, B].
// ShowFrom(A) returns true and has frames [A, B, C, B].
// ShowFrom(B) returns true and has frames [B, C, B].
// ShowFrom(C) returns true and has frames [C, B].
// ShowFrom(D) returns false and drops the sample because no frames remain.
private static bool ShowFrom(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_showFrom) {
    bool matched = default;
    ref Profile p = ref _addr_p.val;
    ref regexp.Regexp showFrom = ref _addr_showFrom.val;

    if (showFrom == null) {
        return false;
    }
    var showFromLocs = make_map<ulong, bool>(); 
    // Apply to locations.
    foreach (var (_, loc) in p.Location) {
        if (filterShowFromLocation(_addr_loc, _addr_showFrom)) {
            showFromLocs[loc.ID] = true;
            matched = true;
        }
    }    var s = make_slice<ptr<Sample>>(0, len(p.Sample));
    foreach (var (_, sample) in p.Sample) {
        for (var i = len(sample.Location) - 1; i >= 0; i--) {
            if (showFromLocs[sample.Location[i].ID]) {
                sample.Location = sample.Location[..(int)i + 1];
                s = append(s, sample);
                break;
            }
        }
    }    p.Sample = s;
    return matched;

}

// filterShowFromLocation tests a showFrom regex against a location, removes
// lines after the last match and returns whether a match was found. If the
// mapping is matched, then all lines are kept.
private static bool filterShowFromLocation(ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_showFrom) {
    ref Location loc = ref _addr_loc.val;
    ref regexp.Regexp showFrom = ref _addr_showFrom.val;

    {
        var m = loc.Mapping;

        if (m != null && showFrom.MatchString(m.File)) {
            return true;
        }
    }

    {
        var i = loc.lastMatchedLineIndex(showFrom);

        if (i >= 0) {
            loc.Line = loc.Line[..(int)i + 1];
            return true;
        }
    }

    return false;

}

// lastMatchedLineIndex returns the index of the last line that matches a regex,
// or -1 if no match is found.
private static nint lastMatchedLineIndex(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re) {
    ref Location loc = ref _addr_loc.val;
    ref regexp.Regexp re = ref _addr_re.val;

    for (var i = len(loc.Line) - 1; i >= 0; i--) {
        {
            var fn = loc.Line[i].Function;

            if (fn != null) {
                if (re.MatchString(fn.Name) || re.MatchString(fn.Filename)) {
                    return i;
                }
            }

        }

    }
    return -1;

}

// FilterTagsByName filters the tags in a profile and only keeps
// tags that match show and not hide.
private static (bool, bool) FilterTagsByName(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_show, ptr<regexp.Regexp> _addr_hide) {
    bool sm = default;
    bool hm = default;
    ref Profile p = ref _addr_p.val;
    ref regexp.Regexp show = ref _addr_show.val;
    ref regexp.Regexp hide = ref _addr_hide.val;

    Func<@string, bool> matchRemove = name => {
        var matchShow = show == null || show.MatchString(name);
        var matchHide = hide != null && hide.MatchString(name);

        if (matchShow) {
            sm = true;
        }
        if (matchHide) {
            hm = true;
        }
        return !matchShow || matchHide;

    };
    foreach (var (_, s) in p.Sample) {
        {
            var lab__prev2 = lab;

            foreach (var (__lab) in s.Label) {
                lab = __lab;
                if (matchRemove(lab)) {
                    delete(s.Label, lab);
                }
            }

            lab = lab__prev2;
        }

        {
            var lab__prev2 = lab;

            foreach (var (__lab) in s.NumLabel) {
                lab = __lab;
                if (matchRemove(lab)) {
                    delete(s.NumLabel, lab);
                }
            }

            lab = lab__prev2;
        }
    }    return ;

}

// matchesName returns whether the location matches the regular
// expression. It checks any available function names, file names, and
// mapping object filename.
private static bool matchesName(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re) {
    ref Location loc = ref _addr_loc.val;
    ref regexp.Regexp re = ref _addr_re.val;

    foreach (var (_, ln) in loc.Line) {
        {
            var fn = ln.Function;

            if (fn != null) {
                if (re.MatchString(fn.Name) || re.MatchString(fn.Filename)) {
                    return true;
                }
            }

        }

    }    {
        var m = loc.Mapping;

        if (m != null && re.MatchString(m.File)) {
            return true;
        }
    }

    return false;

}

// unmatchedLines returns the lines in the location that do not match
// the regular expression.
private static slice<Line> unmatchedLines(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re) {
    ref Location loc = ref _addr_loc.val;
    ref regexp.Regexp re = ref _addr_re.val;

    {
        var m = loc.Mapping;

        if (m != null && re.MatchString(m.File)) {
            return null;
        }
    }

    slice<Line> lines = default;
    foreach (var (_, ln) in loc.Line) {
        {
            var fn = ln.Function;

            if (fn != null) {
                if (re.MatchString(fn.Name) || re.MatchString(fn.Filename)) {
                    continue;
                }
            }

        }

        lines = append(lines, ln);

    }    return lines;

}

// matchedLines returns the lines in the location that match
// the regular expression.
private static slice<Line> matchedLines(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re) {
    ref Location loc = ref _addr_loc.val;
    ref regexp.Regexp re = ref _addr_re.val;

    {
        var m = loc.Mapping;

        if (m != null && re.MatchString(m.File)) {
            return loc.Line;
        }
    }

    slice<Line> lines = default;
    foreach (var (_, ln) in loc.Line) {
        {
            var fn = ln.Function;

            if (fn != null) {
                if (!re.MatchString(fn.Name) && !re.MatchString(fn.Filename)) {
                    continue;
                }
            }

        }

        lines = append(lines, ln);

    }    return lines;

}

// focusedAndNotIgnored looks up a slice of ids against a map of
// focused/ignored locations. The map only contains locations that are
// explicitly focused or ignored. Returns whether there is at least
// one focused location but no ignored locations.
private static bool focusedAndNotIgnored(slice<ptr<Location>> locs, map<ulong, bool> m) {
    bool f = default;
    foreach (var (_, loc) in locs) {
        {
            var (focus, focusOrIgnore) = m[loc.ID];

            if (focusOrIgnore) {
                if (focus) { 
                    // Found focused location. Must keep searching in case there
                    // is an ignored one as well.
                    f = true;

                }
                else
 { 
                    // Found ignored location. Can return false right away.
                    return false;

                }

            }

        }

    }    return f;

}

// TagMatch selects tags for filtering
public delegate  bool TagMatch(ptr<Sample>);

// FilterSamplesByTag removes all samples from the profile, except
// those that match focus and do not match the ignore regular
// expression.
private static (bool, bool) FilterSamplesByTag(this ptr<Profile> _addr_p, TagMatch focus, TagMatch ignore) {
    bool fm = default;
    bool im = default;
    ref Profile p = ref _addr_p.val;

    var samples = make_slice<ptr<Sample>>(0, len(p.Sample));
    foreach (var (_, s) in p.Sample) {
        var focused = true;
        var ignored = false;
        if (focus != null) {
            focused = focus(s);
        }
        if (ignore != null) {
            ignored = ignore(s);
        }
        fm = fm || focused;
        im = im || ignored;
        if (focused && !ignored) {
            samples = append(samples, s);
        }
    }    p.Sample = samples;
    return ;

}

} // end profile_package

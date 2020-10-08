// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Implements methods to filter samples from profiles.

// package profile -- go2cs converted at 2020 October 08 03:41:43 UTC
// import "internal/profile" ==> using profile = go.@internal.profile_package
// Original source: C:\Go\src\internal\profile\filter.go
using regexp = go.regexp_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class profile_package
    {
        // FilterSamplesByName filters the samples in a profile and only keeps
        // samples where at least one frame matches focus but none match ignore.
        // Returns true is the corresponding regexp matched at least one sample.
        private static (bool, bool, bool) FilterSamplesByName(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_focus, ptr<regexp.Regexp> _addr_ignore, ptr<regexp.Regexp> _addr_hide)
        {
            bool fm = default;
            bool im = default;
            bool hm = default;
            ref Profile p = ref _addr_p.val;
            ref regexp.Regexp focus = ref _addr_focus.val;
            ref regexp.Regexp ignore = ref _addr_ignore.val;
            ref regexp.Regexp hide = ref _addr_hide.val;

            var focusOrIgnore = make_map<ulong, bool>();
            var hidden = make_map<ulong, bool>();
            foreach (var (_, l) in p.Location)
            {
                if (ignore != null && l.matchesName(ignore))
                {
                    im = true;
                    focusOrIgnore[l.ID] = false;
                }
                else if (focus == null || l.matchesName(focus))
                {
                    fm = true;
                    focusOrIgnore[l.ID] = true;
                }
                if (hide != null && l.matchesName(hide))
                {
                    hm = true;
                    l.Line = l.unmatchedLines(hide);
                    if (len(l.Line) == 0L)
                    {
                        hidden[l.ID] = true;
                    }
                }
            }            var s = make_slice<ptr<Sample>>(0L, len(p.Sample));
            foreach (var (_, sample) in p.Sample)
            {
                if (focusedAndNotIgnored(sample.Location, focusOrIgnore))
                {
                    if (len(hidden) > 0L)
                    {
                        slice<ptr<Location>> locs = default;
                        foreach (var (_, loc) in sample.Location)
                        {
                            if (!hidden[loc.ID])
                            {
                                locs = append(locs, loc);
                            }
                        }                        if (len(locs) == 0L)
                        { 
                            // Remove sample with no locations (by not adding it to s).
                            continue;

                        }
                        sample.Location = locs;

                    }
                    s = append(s, sample);

                }
            }            p.Sample = s;

            return ;

        }

        // matchesName reports whether the function name or file in the
        // location matches the regular expression.
        private static bool matchesName(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re)
        {
            ref Location loc = ref _addr_loc.val;
            ref regexp.Regexp re = ref _addr_re.val;

            foreach (var (_, ln) in loc.Line)
            {
                {
                    var fn = ln.Function;

                    if (fn != null)
                    {
                        if (re.MatchString(fn.Name))
                        {
                            return true;
                        }

                        if (re.MatchString(fn.Filename))
                        {
                            return true;
                        }

                    }

                }

            }
            return false;

        }

        // unmatchedLines returns the lines in the location that do not match
        // the regular expression.
        private static slice<Line> unmatchedLines(this ptr<Location> _addr_loc, ptr<regexp.Regexp> _addr_re)
        {
            ref Location loc = ref _addr_loc.val;
            ref regexp.Regexp re = ref _addr_re.val;

            slice<Line> lines = default;
            foreach (var (_, ln) in loc.Line)
            {
                {
                    var fn = ln.Function;

                    if (fn != null)
                    {
                        if (re.MatchString(fn.Name))
                        {
                            continue;
                        }

                        if (re.MatchString(fn.Filename))
                        {
                            continue;
                        }

                    }

                }

                lines = append(lines, ln);

            }
            return lines;

        }

        // focusedAndNotIgnored looks up a slice of ids against a map of
        // focused/ignored locations. The map only contains locations that are
        // explicitly focused or ignored. Returns whether there is at least
        // one focused location but no ignored locations.
        private static bool focusedAndNotIgnored(slice<ptr<Location>> locs, map<ulong, bool> m)
        {
            bool f = default;
            foreach (var (_, loc) in locs)
            {
                {
                    var (focus, focusOrIgnore) = m[loc.ID];

                    if (focusOrIgnore)
                    {
                        if (focus)
                        { 
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

            }
            return f;

        }

        // TagMatch selects tags for filtering
        public delegate  bool TagMatch(@string,  @string,  long);

        // FilterSamplesByTag removes all samples from the profile, except
        // those that match focus and do not match the ignore regular
        // expression.
        private static (bool, bool) FilterSamplesByTag(this ptr<Profile> _addr_p, TagMatch focus, TagMatch ignore)
        {
            bool fm = default;
            bool im = default;
            ref Profile p = ref _addr_p.val;

            var samples = make_slice<ptr<Sample>>(0L, len(p.Sample));
            foreach (var (_, s) in p.Sample)
            {
                var (focused, ignored) = focusedSample(_addr_s, focus, ignore);
                fm = fm || focused;
                im = im || ignored;
                if (focused && !ignored)
                {
                    samples = append(samples, s);
                }

            }
            p.Sample = samples;
            return ;

        }

        // focusedTag checks a sample against focus and ignore regexps.
        // Returns whether the focus/ignore regexps match any tags
        private static (bool, bool) focusedSample(ptr<Sample> _addr_s, TagMatch focus, TagMatch ignore)
        {
            bool fm = default;
            bool im = default;
            ref Sample s = ref _addr_s.val;

            fm = focus == null;
            {
                var key__prev1 = key;
                var vals__prev1 = vals;

                foreach (var (__key, __vals) in s.Label)
                {
                    key = __key;
                    vals = __vals;
                    {
                        var val__prev2 = val;

                        foreach (var (_, __val) in vals)
                        {
                            val = __val;
                            if (ignore != null && ignore(key, val, 0L))
                            {
                                im = true;
                            }

                            if (!fm && focus(key, val, 0L))
                            {
                                fm = true;
                            }

                        }

                        val = val__prev2;
                    }
                }

                key = key__prev1;
                vals = vals__prev1;
            }

            {
                var key__prev1 = key;
                var vals__prev1 = vals;

                foreach (var (__key, __vals) in s.NumLabel)
                {
                    key = __key;
                    vals = __vals;
                    {
                        var val__prev2 = val;

                        foreach (var (_, __val) in vals)
                        {
                            val = __val;
                            if (ignore != null && ignore(key, "", val))
                            {
                                im = true;
                            }

                            if (!fm && focus(key, "", val))
                            {
                                fm = true;
                            }

                        }

                        val = val__prev2;
                    }
                }

                key = key__prev1;
                vals = vals__prev1;
            }

            return (fm, im);

        }
    }
}}

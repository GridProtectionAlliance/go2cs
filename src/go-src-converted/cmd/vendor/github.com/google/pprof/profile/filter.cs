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

// package profile -- go2cs converted at 2020 August 29 10:06:20 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\filter.go
// Implements methods to filter samples from profiles.

using regexp = go.regexp_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
{
    public static partial class profile_package
    {
        // FilterSamplesByName filters the samples in a profile and only keeps
        // samples where at least one frame matches focus but none match ignore.
        // Returns true is the corresponding regexp matched at least one sample.
        private static (bool, bool, bool, bool) FilterSamplesByName(this ref Profile p, ref regexp.Regexp focus, ref regexp.Regexp ignore, ref regexp.Regexp hide, ref regexp.Regexp show)
        {
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
                if (show != null)
                {
                    l.Line = l.matchedLines(show);
                    if (len(l.Line) == 0L)
                    {
                        hidden[l.ID] = true;
                    }
                    else
                    {
                        hnm = true;
                    }
                }
            }            var s = make_slice<ref Sample>(0L, len(p.Sample));
            foreach (var (_, sample) in p.Sample)
            {
                if (focusedAndNotIgnored(sample.Location, focusOrIgnore))
                {
                    if (len(hidden) > 0L)
                    {
                        slice<ref Location> locs = default;
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

            return;
        }

        // FilterTagsByName filters the tags in a profile and only keeps
        // tags that match show and not hide.
        private static (bool, bool) FilterTagsByName(this ref Profile p, ref regexp.Regexp show, ref regexp.Regexp hide)
        {
            Func<@string, bool> matchRemove = name =>
            {
                var matchShow = show == null || show.MatchString(name);
                var matchHide = hide != null && hide.MatchString(name);

                if (matchShow)
                {
                    sm = true;
                }
                if (matchHide)
                {
                    hm = true;
                }
                return !matchShow || matchHide;
            }
;
            foreach (var (_, s) in p.Sample)
            {
                {
                    var lab__prev2 = lab;

                    foreach (var (__lab) in s.Label)
                    {
                        lab = __lab;
                        if (matchRemove(lab))
                        {
                            delete(s.Label, lab);
                        }
                    }

                    lab = lab__prev2;
                }

                {
                    var lab__prev2 = lab;

                    foreach (var (__lab) in s.NumLabel)
                    {
                        lab = __lab;
                        if (matchRemove(lab))
                        {
                            delete(s.NumLabel, lab);
                        }
                    }

                    lab = lab__prev2;
                }

            }
            return;
        }

        // matchesName returns whether the location matches the regular
        // expression. It checks any available function names, file names, and
        // mapping object filename.
        private static bool matchesName(this ref Location loc, ref regexp.Regexp re)
        {
            foreach (var (_, ln) in loc.Line)
            {
                {
                    var fn = ln.Function;

                    if (fn != null)
                    {
                        if (re.MatchString(fn.Name) || re.MatchString(fn.Filename))
                        {
                            return true;
                        }
                    }

                }
            }
            {
                var m = loc.Mapping;

                if (m != null && re.MatchString(m.File))
                {
                    return true;
                }

            }
            return false;
        }

        // unmatchedLines returns the lines in the location that do not match
        // the regular expression.
        private static slice<Line> unmatchedLines(this ref Location loc, ref regexp.Regexp re)
        {
            {
                var m = loc.Mapping;

                if (m != null && re.MatchString(m.File))
                {
                    return null;
                }

            }
            slice<Line> lines = default;
            foreach (var (_, ln) in loc.Line)
            {
                {
                    var fn = ln.Function;

                    if (fn != null)
                    {
                        if (re.MatchString(fn.Name) || re.MatchString(fn.Filename))
                        {
                            continue;
                        }
                    }

                }
                lines = append(lines, ln);
            }
            return lines;
        }

        // matchedLines returns the lines in the location that match
        // the regular expression.
        private static slice<Line> matchedLines(this ref Location loc, ref regexp.Regexp re)
        {
            slice<Line> lines = default;
            foreach (var (_, ln) in loc.Line)
            {
                {
                    var fn = ln.Function;

                    if (fn != null)
                    {
                        if (!re.MatchString(fn.Name) && !re.MatchString(fn.Filename))
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
        private static bool focusedAndNotIgnored(slice<ref Location> locs, map<ulong, bool> m)
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
        public delegate  bool TagMatch(ref Sample);

        // FilterSamplesByTag removes all samples from the profile, except
        // those that match focus and do not match the ignore regular
        // expression.
        private static (bool, bool) FilterSamplesByTag(this ref Profile p, TagMatch focus, TagMatch ignore)
        {
            var samples = make_slice<ref Sample>(0L, len(p.Sample));
            foreach (var (_, s) in p.Sample)
            {
                var focused = true;
                var ignored = false;
                if (focus != null)
                {
                    focused = focus(s);
                }
                if (ignore != null)
                {
                    ignored = ignore(s);
                }
                fm = fm || focused;
                im = im || ignored;
                if (focused && !ignored)
                {
                    samples = append(samples, s);
                }
            }
            p.Sample = samples;
            return;
        }
    }
}}}}}}

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

// package driver -- go2cs converted at 2022 March 13 06:36:29 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\driver_focus.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using fmt = fmt_package;
using regexp = regexp_package;
using strconv = strconv_package;
using strings = strings_package;

using measurement = github.com.google.pprof.@internal.measurement_package;
using plugin = github.com.google.pprof.@internal.plugin_package;
using profile = github.com.google.pprof.profile_package;
using System;

public static partial class driver_package {

private static var tagFilterRangeRx = regexp.MustCompile("([+-]?[[:digit:]]+)([[:alpha:]]+)?");

// applyFocus filters samples based on the focus/ignore options
private static error applyFocus(ptr<profile.Profile> _addr_prof, map<@string, @string> numLabelUnits, config cfg, plugin.UI ui) {
    ref profile.Profile prof = ref _addr_prof.val;

    var (focus, err) = compileRegexOption("focus", cfg.Focus, null);
    var (ignore, err) = compileRegexOption("ignore", cfg.Ignore, err);
    var (hide, err) = compileRegexOption("hide", cfg.Hide, err);
    var (show, err) = compileRegexOption("show", cfg.Show, err);
    var (showfrom, err) = compileRegexOption("show_from", cfg.ShowFrom, err);
    var (tagfocus, err) = compileTagFilter("tagfocus", cfg.TagFocus, numLabelUnits, ui, err);
    var (tagignore, err) = compileTagFilter("tagignore", cfg.TagIgnore, numLabelUnits, ui, err);
    var (prunefrom, err) = compileRegexOption("prune_from", cfg.PruneFrom, err);
    if (err != null) {
        return error.As(err)!;
    }
    var (fm, im, hm, hnm) = prof.FilterSamplesByName(focus, ignore, hide, show);
    warnNoMatches(focus == null || fm, "Focus", ui);
    warnNoMatches(ignore == null || im, "Ignore", ui);
    warnNoMatches(hide == null || hm, "Hide", ui);
    warnNoMatches(show == null || hnm, "Show", ui);

    var sfm = prof.ShowFrom(showfrom);
    warnNoMatches(showfrom == null || sfm, "ShowFrom", ui);

    var (tfm, tim) = prof.FilterSamplesByTag(tagfocus, tagignore);
    warnNoMatches(tagfocus == null || tfm, "TagFocus", ui);
    warnNoMatches(tagignore == null || tim, "TagIgnore", ui);

    var (tagshow, err) = compileRegexOption("tagshow", cfg.TagShow, err);
    var (taghide, err) = compileRegexOption("taghide", cfg.TagHide, err);
    var (tns, tnh) = prof.FilterTagsByName(tagshow, taghide);
    warnNoMatches(tagshow == null || tns, "TagShow", ui);
    warnNoMatches(taghide == null || tnh, "TagHide", ui);

    if (prunefrom != null) {
        prof.PruneFrom(prunefrom);
    }
    return error.As(err)!;
}

private static (ptr<regexp.Regexp>, error) compileRegexOption(@string name, @string value, error err) {
    ptr<regexp.Regexp> _p0 = default!;
    error _p0 = default!;

    if (value == "" || err != null) {
        return (_addr_null!, error.As(err)!);
    }
    var (rx, err) = regexp.Compile(value);
    if (err != null) {
        return (_addr_null!, error.As(fmt.Errorf("parsing %s regexp: %v", name, err))!);
    }
    return (_addr_rx!, error.As(null!)!);
}

private static (Func<ptr<profile.Sample>, bool>, error) compileTagFilter(@string name, @string value, map<@string, @string> numLabelUnits, plugin.UI ui, error err) {
    Func<ptr<profile.Sample>, bool> _p0 = default;
    error _p0 = default!;

    if (value == "" || err != null) {
        return (null, error.As(err)!);
    }
    var tagValuePair = strings.SplitN(value, "=", 2);
    @string wantKey = default;
    if (len(tagValuePair) == 2) {
        wantKey = tagValuePair[0];
        value = tagValuePair[1];
    }
    {
        var numFilter = parseTagFilterRange(value);

        if (numFilter != null) {
            ui.PrintErr(name, ":Interpreted '", value, "' as range, not regexp");
            Func<slice<long>, @string, bool> labelFilter = (vals, unit) => {
                {
                    var val__prev1 = val;

                    foreach (var (_, __val) in vals) {
                        val = __val;
                        if (numFilter(val, unit)) {
                            return true;
                        }
                    }

                    val = val__prev1;
                }

                return false;
            }
;
            Func<@string, @string> numLabelUnit = key => numLabelUnits[key];
            if (wantKey == "") {
                return (s => {
                    {
                        var key__prev1 = key;
                        var vals__prev1 = vals;

                        foreach (var (__key, __vals) in s.NumLabel) {
                            key = __key;
                            vals = __vals;
                            if (labelFilter(vals, numLabelUnit(key))) {
                                return true;
                            }
                        }

                        key = key__prev1;
                        vals = vals__prev1;
                    }

                    return false;
                }, error.As(null!)!);
            }
            return (s => {
                {
                    var vals__prev2 = vals;

                    var (vals, ok) = s.NumLabel[wantKey];

                    if (ok) {
                        return labelFilter(vals, numLabelUnit(wantKey));
                    }

                    vals = vals__prev2;

                }
                return false;
            }, error.As(null!)!);
        }
    }

    slice<ptr<regexp.Regexp>> rfx = default;
    foreach (var (_, tagf) in strings.Split(value, ",")) {
        var (fx, err) = regexp.Compile(tagf);
        if (err != null) {
            return (null, error.As(fmt.Errorf("parsing %s regexp: %v", name, err))!);
        }
        rfx = append(rfx, fx);
    }    if (wantKey == "") {
        return (s => {
matchedrx:
            {
                var rx__prev1 = rx;

                foreach (var (_, __rx) in rfx) {
                    rx = __rx;
                    {
                        var key__prev2 = key;
                        var vals__prev2 = vals;

                        foreach (var (__key, __vals) in s.Label) {
                            key = __key;
                            vals = __vals;
                            {
                                var val__prev3 = val;

                                foreach (var (_, __val) in vals) {
                                    val = __val; 
                                    // TODO: Match against val, not key:val in future
                                    if (rx.MatchString(key + ":" + val)) {
                                        _continuematchedrx = true;
                                        break;
                                    }
                                }

                                val = val__prev3;
                            }
                        }

                        key = key__prev2;
                        vals = vals__prev2;
                    }

                    return false;
                }

                rx = rx__prev1;
            }
            return true;
        }, error.As(null!)!);
    }
    return (s => {
        {
            var vals__prev1 = vals;

            (vals, ok) = s.Label[wantKey];

            if (ok) {
                {
                    var rx__prev1 = rx;

                    foreach (var (_, __rx) in rfx) {
                        rx = __rx;
                        {
                            var val__prev2 = val;

                            foreach (var (_, __val) in vals) {
                                val = __val;
                                if (rx.MatchString(val)) {
                                    return true;
                                }
                            }

                            val = val__prev2;
                        }
                    }

                    rx = rx__prev1;
                }
            }

            vals = vals__prev1;

        }
        return false;
    }, error.As(null!)!);
}

// parseTagFilterRange returns a function to checks if a value is
// contained on the range described by a string. It can recognize
// strings of the form:
// "32kb" -- matches values == 32kb
// ":64kb" -- matches values <= 64kb
// "4mb:" -- matches values >= 4mb
// "12kb:64mb" -- matches values between 12kb and 64mb (both included).
private static Func<long, @string, bool> parseTagFilterRange(@string filter) => func((_, panic, _) => {
    var ranges = tagFilterRangeRx.FindAllStringSubmatch(filter, 2);
    if (len(ranges) == 0) {
        return null; // No ranges were identified
    }
    var (v, err) = strconv.ParseInt(ranges[0][1], 10, 64);
    if (err != null) {
        panic(fmt.Errorf("failed to parse int %s: %v", ranges[0][1], err));
    }
    var (scaledValue, unit) = measurement.Scale(v, ranges[0][2], ranges[0][2]);
    if (len(ranges) == 1) {
        {
            var match = ranges[0][0];


            if (filter == match) 
                return (v, u) => {
                    var (sv, su) = measurement.Scale(v, u, unit);
                    return su == unit && sv == scaledValue;
                };
            else if (filter == match + ":") 
                return (v, u) => {
                    (sv, su) = measurement.Scale(v, u, unit);
                    return su == unit && sv >= scaledValue;
                };
            else if (filter == ":" + match) 
                return (v, u) => {
                    (sv, su) = measurement.Scale(v, u, unit);
                    return su == unit && sv <= scaledValue;
                };

        }
        return null;
    }
    if (filter != ranges[0][0] + ":" + ranges[1][0]) {
        return null;
    }
    v, err = strconv.ParseInt(ranges[1][1], 10, 64);

    if (err != null) {
        panic(fmt.Errorf("failed to parse int %s: %v", ranges[1][1], err));
    }
    var (scaledValue2, unit2) = measurement.Scale(v, ranges[1][2], unit);
    if (unit != unit2) {
        return null;
    }
    return (v, u) => {
        (sv, su) = measurement.Scale(v, u, unit);
        return su == unit && sv >= scaledValue && sv <= scaledValue2;
    };
});

private static void warnNoMatches(bool match, @string option, plugin.UI ui) {
    if (!match) {
        ui.PrintErr(option + " expression matched no samples");
    }
}

} // end driver_package

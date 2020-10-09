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

// Implements methods to remove frames from profiles.

// package profile -- go2cs converted at 2020 October 09 05:54:02 UTC
// import "cmd/vendor/github.com/google/pprof/profile" ==> using profile = go.cmd.vendor.github.com.google.pprof.profile_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\profile\prune.go
using fmt = go.fmt_package;
using regexp = go.regexp_package;
using strings = go.strings_package;
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
        private static @string reservedNames = new slice<@string>(new @string[] { "(anonymous namespace)", "operator()" });        private static Func<ptr<regexp.Regexp>> bracketRx = () =>
        {
            private static slice<@string> quotedNames = default;
            foreach (var (_, name) in append(reservedNames, "("))
            {
                quotedNames = append(quotedNames, regexp.QuoteMeta(name));
            }            return regexp.MustCompile(strings.Join(quotedNames, "|"));

        }();

        // simplifyFunc does some primitive simplification of function names.
        private static @string simplifyFunc(@string f)
        { 
            // Account for leading '.' on the PPC ELF v1 ABI.
            var funcName = strings.TrimPrefix(f, "."); 
            // Account for unsimplified names -- try  to remove the argument list by trimming
            // starting from the first '(', but skipping reserved names that have '('.
            foreach (var (_, ind) in bracketRx.FindAllStringSubmatchIndex(funcName, -1L))
            {
                var foundReserved = false;
                foreach (var (_, res) in reservedNames)
                {
                    if (funcName[ind[0L]..ind[1L]] == res)
                    {
                        foundReserved = true;
                        break;
                    }

                }
                if (!foundReserved)
                {
                    funcName = funcName[..ind[0L]];
                    break;
                }

            }
            return funcName;

        }

        // Prune removes all nodes beneath a node matching dropRx, and not
        // matching keepRx. If the root node of a Sample matches, the sample
        // will have an empty stack.
        private static void Prune(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_dropRx, ptr<regexp.Regexp> _addr_keepRx)
        {
            ref Profile p = ref _addr_p.val;
            ref regexp.Regexp dropRx = ref _addr_dropRx.val;
            ref regexp.Regexp keepRx = ref _addr_keepRx.val;

            var prune = make_map<ulong, bool>();
            var pruneBeneath = make_map<ulong, bool>();

            foreach (var (_, loc) in p.Location)
            {
                long i = default;
                for (i = len(loc.Line) - 1L; i >= 0L; i--)
                {
                    {
                        var fn = loc.Line[i].Function;

                        if (fn != null && fn.Name != "")
                        {
                            var funcName = simplifyFunc(fn.Name);
                            if (dropRx.MatchString(funcName))
                            {
                                if (keepRx == null || !keepRx.MatchString(funcName))
                                {
                                    break;
                                }

                            }

                        }

                    }

                }


                if (i >= 0L)
                { 
                    // Found matching entry to prune.
                    pruneBeneath[loc.ID] = true; 

                    // Remove the matching location.
                    if (i == len(loc.Line) - 1L)
                    { 
                        // Matched the top entry: prune the whole location.
                        prune[loc.ID] = true;

                    }
                    else
                    {
                        loc.Line = loc.Line[i + 1L..];
                    }

                }

            } 

            // Prune locs from each Sample
            foreach (var (_, sample) in p.Sample)
            { 
                // Scan from the root to the leaves to find the prune location.
                // Do not prune frames before the first user frame, to avoid
                // pruning everything.
                var foundUser = false;
                {
                    long i__prev2 = i;

                    for (i = len(sample.Location) - 1L; i >= 0L; i--)
                    {
                        var id = sample.Location[i].ID;
                        if (!prune[id] && !pruneBeneath[id])
                        {
                            foundUser = true;
                            continue;
                        }

                        if (!foundUser)
                        {
                            continue;
                        }

                        if (prune[id])
                        {
                            sample.Location = sample.Location[i + 1L..];
                            break;
                        }

                        if (pruneBeneath[id])
                        {
                            sample.Location = sample.Location[i..];
                            break;
                        }

                    }


                    i = i__prev2;
                }

            }

        }

        // RemoveUninteresting prunes and elides profiles using built-in
        // tables of uninteresting function names.
        private static error RemoveUninteresting(this ptr<Profile> _addr_p)
        {
            ref Profile p = ref _addr_p.val;

            ptr<regexp.Regexp> keep;            ptr<regexp.Regexp> drop;

            error err = default!;

            if (p.DropFrames != "")
            {
                drop, err = regexp.Compile("^(" + p.DropFrames + ")$");

                if (err != null)
                {
                    return error.As(fmt.Errorf("failed to compile regexp %s: %v", p.DropFrames, err))!;
                }

                if (p.KeepFrames != "")
                {
                    keep, err = regexp.Compile("^(" + p.KeepFrames + ")$");

                    if (err != null)
                    {
                        return error.As(fmt.Errorf("failed to compile regexp %s: %v", p.KeepFrames, err))!;
                    }

                }

                p.Prune(drop, keep);

            }

            return error.As(null!)!;

        }

        // PruneFrom removes all nodes beneath the lowest node matching dropRx, not including itself.
        //
        // Please see the example below to understand this method as well as
        // the difference from Prune method.
        //
        // A sample contains Location of [A,B,C,B,D] where D is the top frame and there's no inline.
        //
        // PruneFrom(A) returns [A,B,C,B,D] because there's no node beneath A.
        // Prune(A, nil) returns [B,C,B,D] by removing A itself.
        //
        // PruneFrom(B) returns [B,C,B,D] by removing all nodes beneath the first B when scanning from the bottom.
        // Prune(B, nil) returns [D] because a matching node is found by scanning from the root.
        private static void PruneFrom(this ptr<Profile> _addr_p, ptr<regexp.Regexp> _addr_dropRx)
        {
            ref Profile p = ref _addr_p.val;
            ref regexp.Regexp dropRx = ref _addr_dropRx.val;

            var pruneBeneath = make_map<ulong, bool>();

            {
                var loc__prev1 = loc;

                foreach (var (_, __loc) in p.Location)
                {
                    loc = __loc;
                    {
                        long i__prev2 = i;

                        for (long i = 0L; i < len(loc.Line); i++)
                        {
                            {
                                var fn = loc.Line[i].Function;

                                if (fn != null && fn.Name != "")
                                {
                                    var funcName = simplifyFunc(fn.Name);
                                    if (dropRx.MatchString(funcName))
                                    { 
                                        // Found matching entry to prune.
                                        pruneBeneath[loc.ID] = true;
                                        loc.Line = loc.Line[i..];
                                        break;

                                    }

                                }

                            }

                        }


                        i = i__prev2;
                    }

                } 

                // Prune locs from each Sample

                loc = loc__prev1;
            }

            foreach (var (_, sample) in p.Sample)
            { 
                // Scan from the bottom leaf to the root to find the prune location.
                {
                    long i__prev2 = i;
                    var loc__prev2 = loc;

                    foreach (var (__i, __loc) in sample.Location)
                    {
                        i = __i;
                        loc = __loc;
                        if (pruneBeneath[loc.ID])
                        {
                            sample.Location = sample.Location[i..];
                            break;
                        }

                    }

                    i = i__prev2;
                    loc = loc__prev2;
                }
            }

        }
    }
}}}}}}

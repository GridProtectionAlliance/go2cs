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

// Package proftest provides some utility routines to test other
// packages related to profiles.
// package proftest -- go2cs converted at 2020 August 29 10:05:43 UTC
// import "cmd/vendor/github.com/google/pprof/internal/proftest" ==> using proftest = go.cmd.vendor.github.com.google.pprof.@internal.proftest_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\proftest\proftest.go
using json = go.encoding.json_package;
using fmt = go.fmt_package;
using ioutil = go.io.ioutil_package;
using os = go.os_package;
using exec = go.os.exec_package;
using regexp = go.regexp_package;
using testing = go.testing_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class proftest_package
    {
        // Diff compares two byte arrays using the diff tool to highlight the
        // differences. It is meant for testing purposes to display the
        // differences between expected and actual output.
        public static (slice<byte>, error) Diff(slice<byte> b1, slice<byte> b2) => func((defer, _, __) =>
        {
            var (f1, err) = ioutil.TempFile("", "proto_test");
            if (err != null)
            {
                return (null, err);
            }
            defer(os.Remove(f1.Name()));
            defer(f1.Close());

            var (f2, err) = ioutil.TempFile("", "proto_test");
            if (err != null)
            {
                return (null, err);
            }
            defer(os.Remove(f2.Name()));
            defer(f2.Close());

            f1.Write(b1);
            f2.Write(b2);

            data, err = exec.Command("diff", "-u", f1.Name(), f2.Name()).CombinedOutput();
            if (len(data) > 0L)
            { 
                // diff exits with a non-zero status when the files don't match.
                // Ignore that failure as long as we get output.
                err = null;
            }
            if (err != null)
            {
                data = (slice<byte>)fmt.Sprintf("diff failed: %v\nb1: %q\nb2: %q\n", err, b1, b2);
                err = null;
            }
            return;
        });

        // EncodeJSON encodes a value into a byte array. This is intended for
        // testing purposes.
        public static slice<byte> EncodeJSON(object x) => func((_, panic, __) =>
        {
            var (data, err) = json.MarshalIndent(x, "", "    ");
            if (err != null)
            {
                panic(err);
            }
            data = append(data, '\n');
            return data;
        });

        // TestUI implements the plugin.UI interface, triggering test failures
        // if more than Ignore errors not matching AllowRx are printed.
        // Also tracks the number of times the error matches AllowRx in
        // NumAllowRxMatches.
        public partial struct TestUI
        {
            public ptr<testing.T> T;
            public long Ignore;
            public @string AllowRx;
            public long NumAllowRxMatches;
        }

        // ReadLine returns no input, as no input is expected during testing.
        private static (@string, error) ReadLine(this ref TestUI ui, @string _)
        {
            return ("", fmt.Errorf("no input"));
        }

        // Print messages are discarded by the test UI.
        private static void Print(this ref TestUI ui, params object[] args)
        {
        }

        // PrintErr messages may trigger an error failure. A fixed number of
        // error messages are permitted when appropriate.
        private static void PrintErr(this ref TestUI ui, params object[] args)
        {
            if (ui.AllowRx != "")
            {
                {
                    var (matched, err) = regexp.MatchString(ui.AllowRx, fmt.Sprint(args));

                    if (matched || err != null)
                    {
                        if (err != null)
                        {
                            ui.T.Errorf("failed to match against regex %q: %v", ui.AllowRx, err);
                        }
                        ui.NumAllowRxMatches++;
                        return;
                    }

                }
            }
            if (ui.Ignore > 0L)
            {
                ui.Ignore--;
                return;
            } 
            // Stringify arguments with fmt.Sprint() to match what default UI
            // implementation does. Without this Error() calls fmt.Sprintln() which
            // _always_ adds spaces between arguments, unlike fmt.Sprint() which only
            // adds them between arguments if neither is string.
            ui.T.Error(fmt.Sprint(args));
        }

        // IsTerminal indicates if the UI is an interactive terminal.
        private static bool IsTerminal(this ref TestUI ui)
        {
            return false;
        }

        // SetAutoComplete is not supported by the test UI.
        private static @string SetAutoComplete(this ref TestUI ui, Func<@string, @string> _)
        {
        }
    }
}}}}}}}

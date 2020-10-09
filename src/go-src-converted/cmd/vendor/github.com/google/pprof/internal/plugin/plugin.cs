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

// Package plugin defines the plugin implementations that the main pprof driver requires.
// package plugin -- go2cs converted at 2020 October 09 05:53:41 UTC
// import "cmd/vendor/github.com/google/pprof/internal/plugin" ==> using plugin = go.cmd.vendor.github.com.google.pprof.@internal.plugin_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\plugin\plugin.go
using io = go.io_package;
using http = go.net.http_package;
using regexp = go.regexp_package;
using time = go.time_package;

using profile = go.github.com.google.pprof.profile_package;
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
    public static partial class plugin_package
    {
        // Options groups all the optional plugins into pprof.
        public partial struct Options
        {
            public Writer Writer;
            public FlagSet Flagset;
            public Fetcher Fetch;
            public Symbolizer Sym;
            public ObjTool Obj;
            public UI UI; // HTTPServer is a function that should block serving http requests,
// including the handlers specified in args.  If non-nil, pprof will
// invoke this function if necessary to provide a web interface.
//
// If HTTPServer is nil, pprof will use its own internal HTTP server.
//
// A common use for a custom HTTPServer is to provide custom
// authentication checks.
            public Func<ptr<HTTPServerArgs>, error> HTTPServer;
            public http.RoundTripper HTTPTransport;
        }

        // Writer provides a mechanism to write data under a certain name,
        // typically a filename.
        public partial interface Writer
        {
            (io.WriteCloser, error) Open(@string name);
        }

        // A FlagSet creates and parses command-line flags.
        // It is similar to the standard flag.FlagSet.
        public partial interface FlagSet
        {
            slice<@string> Bool(@string name, bool def, @string usage);
            slice<@string> Int(@string name, long def, @string usage);
            slice<@string> Float64(@string name, double def, @string usage);
            slice<@string> String(@string name, @string def, @string usage); // StringList is similar to String but allows multiple values for a
// single flag
            slice<@string> StringList(@string name, @string def, @string usage); // ExtraUsage returns any additional text that should be printed after the
// standard usage message. The extra usage message returned includes all text
// added with AddExtraUsage().
// The typical use of ExtraUsage is to show any custom flags defined by the
// specific pprof plugins being used.
            slice<@string> ExtraUsage(); // AddExtraUsage appends additional text to the end of the extra usage message.
            slice<@string> AddExtraUsage(@string eu); // Parse initializes the flags with their values for this run
// and returns the non-flag command line arguments.
// If an unknown flag is encountered or there are no arguments,
// Parse should call usage and return nil.
            slice<@string> Parse(Action usage);
        }

        // A Fetcher reads and returns the profile named by src. src can be a
        // local file path or a URL. duration and timeout are units specified
        // by the end user, or 0 by default. duration refers to the length of
        // the profile collection, if applicable, and timeout is the amount of
        // time to wait for a profile before returning an error. Returns the
        // fetched profile, the URL of the actual source of the profile, or an
        // error.
        public partial interface Fetcher
        {
            (ptr<profile.Profile>, @string, error) Fetch(@string src, time.Duration duration, time.Duration timeout);
        }

        // A Symbolizer introduces symbol information into a profile.
        public partial interface Symbolizer
        {
            error Symbolize(@string mode, MappingSources srcs, ptr<profile.Profile> prof);
        }

        // MappingSources map each profile.Mapping to the source of the profile.
        // The key is either Mapping.File or Mapping.BuildId.
        public partial struct MappingSources // : map<@string, slice<object>>
        {
        }

        // An ObjTool inspects shared libraries and executable files.
        public partial interface ObjTool
        {
            (slice<Inst>, error) Open(@string file, ulong start, ulong limit, ulong offset); // Disasm disassembles the named object file, starting at
// the start address and stopping at (before) the end address.
            (slice<Inst>, error) Disasm(@string file, ulong start, ulong end);
        }

        // An Inst is a single instruction in an assembly listing.
        public partial struct Inst
        {
            public ulong Addr; // virtual address of instruction
            public @string Text; // instruction text
            public @string Function; // function name
            public @string File; // source file
            public long Line; // source line
        }

        // An ObjFile is a single object file: a shared library or executable.
        public partial interface ObjFile
        {
            error Name(); // Base returns the base address to use when looking up symbols in the file.
            error Base(); // BuildID returns the GNU build ID of the file, or an empty string.
            error BuildID(); // SourceLine reports the source line information for a given
// address in the file. Due to inlining, the source line information
// is in general a list of positions representing a call stack,
// with the leaf function first.
            error SourceLine(ulong addr); // Symbols returns a list of symbols in the object file.
// If r is not nil, Symbols restricts the list to symbols
// with names matching the regular expression.
// If addr is not zero, Symbols restricts the list to symbols
// containing that address.
            error Symbols(ptr<regexp.Regexp> r, ulong addr); // Close closes the file, releasing associated resources.
            error Close();
        }

        // A Frame describes a single line in a source file.
        public partial struct Frame
        {
            public @string Func; // name of function
            public @string File; // source file name
            public long Line; // line in file
        }

        // A Sym describes a single symbol in an object file.
        public partial struct Sym
        {
            public slice<@string> Name; // names of symbol (many if symbol was dedup'ed)
            public @string File; // object file containing symbol
            public ulong Start; // start virtual address
            public ulong End; // virtual address of last byte in sym (Start+size-1)
        }

        // A UI manages user interactions.
        public partial interface UI
        {
            @string ReadLine(@string prompt); // Print shows a message to the user.
// It formats the text as fmt.Print would and adds a final \n if not already present.
// For line-based UI, Print writes to standard error.
// (Standard output is reserved for report data.)
            @string Print(params object _p0); // PrintErr shows an error message to the user.
// It formats the text as fmt.Print would and adds a final \n if not already present.
// For line-based UI, PrintErr writes to standard error.
            @string PrintErr(params object _p0); // IsTerminal returns whether the UI is known to be tied to an
// interactive terminal (as opposed to being redirected to a file).
            @string IsTerminal(); // WantBrowser indicates whether a browser should be opened with the -http option.
            @string WantBrowser(); // SetAutoComplete instructs the UI to call complete(cmd) to obtain
// the auto-completion of cmd, if the UI supports auto-completion at all.
            @string SetAutoComplete(Func<@string, @string> complete);
        }

        // HTTPServerArgs contains arguments needed by an HTTP server that
        // is exporting a pprof web interface.
        public partial struct HTTPServerArgs
        {
            public @string Hostport;
            public @string Host; // Host portion of Hostport
            public long Port; // Port portion of Hostport

// Handlers maps from URL paths to the handler to invoke to
// serve that path.
            public map<@string, http.Handler> Handlers;
        }
    }
}}}}}}}

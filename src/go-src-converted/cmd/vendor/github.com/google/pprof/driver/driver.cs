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

// Package driver provides an external entry point to the pprof driver.
// package driver -- go2cs converted at 2020 August 29 10:05:09 UTC
// import "cmd/vendor/github.com/google/pprof/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\driver\driver.go
using io = go.io_package;
using regexp = go.regexp_package;
using time = go.time_package;

using internaldriver = go.github.com.google.pprof.@internal.driver_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using profile = go.github.com.google.pprof.profile_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof
{
    public static partial class driver_package
    {
        // PProf acquires a profile, and symbolizes it using a profile
        // manager. Then it generates a report formatted according to the
        // options selected through the flags package.
        public static error PProf(ref Options o)
        {
            return error.As(internaldriver.PProf(o.internalOptions()));
        }

        private static ref plugin.Options internalOptions(this ref Options o)
        {
            plugin.ObjTool obj = default;
            if (o.Obj != null)
            {
                obj = ref new internalObjTool(o.Obj);
            }
            plugin.Symbolizer sym = default;
            if (o.Sym != null)
            {
                sym = ref new internalSymbolizer(o.Sym);
            }
            return ref new plugin.Options(Writer:o.Writer,Flagset:o.Flagset,Fetch:o.Fetch,Sym:sym,Obj:obj,UI:o.UI,);
        }

        // Options groups all the optional plugins into pprof.
        public partial struct Options
        {
            public Writer Writer;
            public FlagSet Flagset;
            public Fetcher Fetch;
            public Symbolizer Sym;
            public ObjTool Obj;
            public UI UI;
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
            slice<@string> String(@string name, @string def, @string usage); // BoolVar, IntVar, Float64Var, and StringVar define new flags referencing
// a given pointer, like the functions of the same name in package flag.
            slice<@string> BoolVar(ref bool pointer, @string name, bool def, @string usage);
            slice<@string> IntVar(ref long pointer, @string name, long def, @string usage);
            slice<@string> Float64Var(ref double pointer, @string name, double def, @string usage);
            slice<@string> StringVar(ref @string pointer, @string name, @string def, @string usage); // StringList is similar to String but allows multiple values for a
// single flag
            slice<@string> StringList(@string name, @string def, @string usage); // ExtraUsage returns any additional text that should be
// printed after the standard usage message.
// The typical use of ExtraUsage is to show any custom flags
// defined by the specific pprof plugins being used.
            slice<@string> ExtraUsage(); // Parse initializes the flags with their values for this run
// and returns the non-flag command line arguments.
// If an unknown flag is encountered or there are no arguments,
// Parse should call usage and return nil.
            slice<@string> Parse(Action usage);
        }

        // A Fetcher reads and returns the profile named by src, using
        // the specified duration and timeout. It returns the fetched
        // profile and a string indicating a URL from where the profile
        // was fetched, which may be different than src.
        public partial interface Fetcher
        {
            (ref profile.Profile, @string, error) Fetch(@string src, time.Duration duration, time.Duration timeout);
        }

        // A Symbolizer introduces symbol information into a profile.
        public partial interface Symbolizer
        {
            error Symbolize(@string mode, MappingSources srcs, ref profile.Profile prof);
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
            error Symbols(ref regexp.Regexp r, ulong addr); // Close closes the file, releasing associated resources.
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
            @string IsTerminal(); // SetAutoComplete instructs the UI to call complete(cmd) to obtain
// the auto-completion of cmd, if the UI supports auto-completion at all.
            @string SetAutoComplete(Func<@string, @string> complete);
        }

        // internalObjTool is a wrapper to map from the pprof external
        // interface to the internal interface.
        private partial struct internalObjTool : ObjTool
        {
            public ObjTool ObjTool;
        }

        private static (plugin.ObjFile, error) Open(this ref internalObjTool o, @string file, ulong start, ulong limit, ulong offset)
        {
            var (f, err) = o.ObjTool.Open(file, start, limit, offset);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new internalObjFile(f), err);
        }

        private partial struct internalObjFile : ObjFile
        {
            public ObjFile ObjFile;
        }

        private static (slice<plugin.Frame>, error) SourceLine(this ref internalObjFile f, ulong frame)
        {
            var (frames, err) = f.ObjFile.SourceLine(frame);
            if (err != null)
            {
                return (null, err);
            }
            slice<plugin.Frame> pluginFrames = default;
            foreach (var (_, f) in frames)
            {
                pluginFrames = append(pluginFrames, plugin.Frame(f));
            }
            return (pluginFrames, null);
        }

        private static (slice<ref plugin.Sym>, error) Symbols(this ref internalObjFile f, ref regexp.Regexp r, ulong addr)
        {
            var (syms, err) = f.ObjFile.Symbols(r, addr);
            if (err != null)
            {
                return (null, err);
            }
            slice<ref plugin.Sym> pluginSyms = default;
            foreach (var (_, s) in syms)
            {
                var ps = plugin.Sym(s.Value);
                pluginSyms = append(pluginSyms, ref ps);
            }
            return (pluginSyms, null);
        }

        private static (slice<plugin.Inst>, error) Disasm(this ref internalObjTool o, @string file, ulong start, ulong end)
        {
            var (insts, err) = o.ObjTool.Disasm(file, start, end);
            if (err != null)
            {
                return (null, err);
            }
            slice<plugin.Inst> pluginInst = default;
            foreach (var (_, inst) in insts)
            {
                pluginInst = append(pluginInst, plugin.Inst(inst));
            }
            return (pluginInst, null);
        }

        // internalSymbolizer is a wrapper to map from the pprof external
        // interface to the internal interface.
        private partial struct internalSymbolizer : Symbolizer
        {
            public Symbolizer Symbolizer;
        }

        private static error Symbolize(this ref internalSymbolizer s, @string mode, plugin.MappingSources srcs, ref profile.Profile prof)
        {
            MappingSources isrcs = new MappingSources();
            foreach (var (m, s) in srcs)
            {
                isrcs[m] = s;
            }
            return error.As(s.Symbolizer.Symbolize(mode, isrcs, prof));
        }
    }
}}}}}}

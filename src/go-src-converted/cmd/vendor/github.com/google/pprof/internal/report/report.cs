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

// Package report summarizes a performance profile into a
// human-readable report.
// package report -- go2cs converted at 2022 March 06 23:23:45 UTC
// import "cmd/vendor/github.com/google/pprof/internal/report" ==> using report = go.cmd.vendor.github.com.google.pprof.@internal.report_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\report\report.go
using fmt = go.fmt_package;
using io = go.io_package;
using filepath = go.path.filepath_package;
using regexp = go.regexp_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using tabwriter = go.text.tabwriter_package;
using time = go.time_package;

using graph = go.github.com.google.pprof.@internal.graph_package;
using measurement = go.github.com.google.pprof.@internal.measurement_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using profile = go.github.com.google.pprof.profile_package;
using System;


namespace go.cmd.vendor.github.com.google.pprof.@internal;

public static partial class report_package {

    // Output formats.
public static readonly var Callgrind = iota;
public static readonly var Comments = 0;
public static readonly var Dis = 1;
public static readonly var Dot = 2;
public static readonly var List = 3;
public static readonly var Proto = 4;
public static readonly var Raw = 5;
public static readonly var Tags = 6;
public static readonly var Text = 7;
public static readonly var TopProto = 8;
public static readonly var Traces = 9;
public static readonly var Tree = 10;
public static readonly var WebList = 11;


// Options are the formatting and filtering options used to generate a
// profile.
public partial struct Options {
    public nint OutputFormat;
    public bool CumSort;
    public bool CallTree;
    public bool DropNegative;
    public bool CompactLabels;
    public double Ratio;
    public @string Title;
    public slice<@string> ProfileLabels;
    public slice<@string> ActiveFilters;
    public map<@string, @string> NumLabelUnits;
    public nint NodeCount;
    public double NodeFraction;
    public double EdgeFraction;
    public Func<slice<long>, long> SampleValue;
    public Func<slice<long>, long> SampleMeanDivisor;
    public @string SampleType;
    public @string SampleUnit; // Unit for the sample data from the profile.

    public @string OutputUnit; // Units for data formatting in report.

    public ptr<regexp.Regexp> Symbol; // Symbols to include on disassembly report.
    public @string SourcePath; // Search path for source files.
    public @string TrimPath; // Paths to trim from source file paths.

    public bool IntelSyntax; // Whether or not to print assembly in Intel syntax.
}

// Generate generates a report as directed by the Report.
public static error Generate(io.Writer w, ptr<Report> _addr_rpt, plugin.ObjTool obj) {
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options;


    if (o.OutputFormat == Comments) 
        return error.As(printComments(w, _addr_rpt))!;
    else if (o.OutputFormat == Dot) 
        return error.As(printDOT(w, _addr_rpt))!;
    else if (o.OutputFormat == Tree) 
        return error.As(printTree(w, _addr_rpt))!;
    else if (o.OutputFormat == Text) 
        return error.As(printText(w, _addr_rpt))!;
    else if (o.OutputFormat == Traces) 
        return error.As(printTraces(w, _addr_rpt))!;
    else if (o.OutputFormat == Raw) 
        fmt.Fprint(w, rpt.prof.String());
        return error.As(null!)!;
    else if (o.OutputFormat == Tags) 
        return error.As(printTags(w, _addr_rpt))!;
    else if (o.OutputFormat == Proto) 
        return error.As(printProto(w, _addr_rpt))!;
    else if (o.OutputFormat == TopProto) 
        return error.As(printTopProto(w, _addr_rpt))!;
    else if (o.OutputFormat == Dis) 
        return error.As(printAssembly(w, _addr_rpt, obj))!;
    else if (o.OutputFormat == List) 
        return error.As(printSource(w, rpt))!;
    else if (o.OutputFormat == WebList) 
        return error.As(printWebSource(w, rpt, obj))!;
    else if (o.OutputFormat == Callgrind) 
        return error.As(printCallgrind(w, _addr_rpt))!;
        return error.As(fmt.Errorf("unexpected output format"))!;

}

// newTrimmedGraph creates a graph for this report, trimmed according
// to the report options.
private static (ptr<graph.Graph>, nint, nint, nint) newTrimmedGraph(this ptr<Report> _addr_rpt) {
    ptr<graph.Graph> g = default!;
    nint origCount = default;
    nint droppedNodes = default;
    nint droppedEdges = default;
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options; 

    // Build a graph and refine it. On each refinement step we must rebuild the graph from the samples,
    // as the graph itself doesn't contain enough information to preserve full precision.
    var visualMode = o.OutputFormat == Dot;
    var cumSort = o.CumSort; 

    // The call_tree option is only honored when generating visual representations of the callgraph.
    var callTree = o.CallTree && (o.OutputFormat == Dot || o.OutputFormat == Callgrind); 

    // First step: Build complete graph to identify low frequency nodes, based on their cum weight.
    g = rpt.newGraph(null);
    var (totalValue, _) = g.Nodes.Sum();
    var nodeCutoff = abs64(int64(float64(totalValue) * o.NodeFraction));
    var edgeCutoff = abs64(int64(float64(totalValue) * o.EdgeFraction)); 

    // Filter out nodes with cum value below nodeCutoff.
    if (nodeCutoff > 0) {
        if (callTree) {
            {
                var nodesKept__prev3 = nodesKept;

                var nodesKept = g.DiscardLowFrequencyNodePtrs(nodeCutoff);

                if (len(g.Nodes) != len(nodesKept)) {
                    droppedNodes = len(g.Nodes) - len(nodesKept);
                    g.TrimTree(nodesKept);
                }

                nodesKept = nodesKept__prev3;

            }

        }
        else
 {
            {
                var nodesKept__prev3 = nodesKept;

                nodesKept = g.DiscardLowFrequencyNodes(nodeCutoff);

                if (len(g.Nodes) != len(nodesKept)) {
                    droppedNodes = len(g.Nodes) - len(nodesKept);
                    g = rpt.newGraph(nodesKept);
                }

                nodesKept = nodesKept__prev3;

            }

        }
    }
    origCount = len(g.Nodes); 

    // Second step: Limit the total number of nodes. Apply specialized heuristics to improve
    // visualization when generating dot output.
    g.SortNodes(cumSort, visualMode);
    {
        var nodeCount = o.NodeCount;

        if (nodeCount > 0) { 
            // Remove low frequency tags and edges as they affect selection.
            g.TrimLowFrequencyTags(nodeCutoff);
            g.TrimLowFrequencyEdges(edgeCutoff);
            if (callTree) {
                {
                    var nodesKept__prev3 = nodesKept;

                    nodesKept = g.SelectTopNodePtrs(nodeCount, visualMode);

                    if (len(g.Nodes) != len(nodesKept)) {
                        g.TrimTree(nodesKept);
                        g.SortNodes(cumSort, visualMode);
                    }

                    nodesKept = nodesKept__prev3;

                }

            }
            else
 {
                {
                    var nodesKept__prev3 = nodesKept;

                    nodesKept = g.SelectTopNodes(nodeCount, visualMode);

                    if (len(g.Nodes) != len(nodesKept)) {
                        g = rpt.newGraph(nodesKept);
                        g.SortNodes(cumSort, visualMode);
                    }

                    nodesKept = nodesKept__prev3;

                }

            }

        }
    } 

    // Final step: Filter out low frequency tags and edges, and remove redundant edges that clutter
    // the graph.
    g.TrimLowFrequencyTags(nodeCutoff);
    droppedEdges = g.TrimLowFrequencyEdges(edgeCutoff);
    if (visualMode) {
        g.RemoveRedundantEdges();
    }
    return ;

}

private static void selectOutputUnit(this ptr<Report> _addr_rpt, ptr<graph.Graph> _addr_g) {
    ref Report rpt = ref _addr_rpt.val;
    ref graph.Graph g = ref _addr_g.val;

    var o = rpt.options; 

    // Select best unit for profile output.
    // Find the appropriate units for the smallest non-zero sample
    if (o.OutputUnit != "minimum" || len(g.Nodes) == 0) {
        return ;
    }
    long minValue = default;

    foreach (var (_, n) in g.Nodes) {
        var nodeMin = abs64(n.FlatValue());
        if (nodeMin == 0) {
            nodeMin = abs64(n.CumValue());
        }
        if (nodeMin > 0 && (minValue == 0 || nodeMin < minValue)) {
            minValue = nodeMin;
        }
    }    var maxValue = rpt.total;
    if (minValue == 0) {
        minValue = maxValue;
    }
    {
        var r = o.Ratio;

        if (r > 0 && r != 1) {
            minValue = int64(float64(minValue) * r);
            maxValue = int64(float64(maxValue) * r);
        }
    }


    var (_, minUnit) = measurement.Scale(minValue, o.SampleUnit, "minimum");
    var (_, maxUnit) = measurement.Scale(maxValue, o.SampleUnit, "minimum");

    var unit = minUnit;
    if (minUnit != maxUnit && minValue * 100 < maxValue && o.OutputFormat != Callgrind) { 
        // Minimum and maximum values have different units. Scale
        // minimum by 100 to use larger units, allowing minimum value to
        // be scaled down to 0.01, except for callgrind reports since
        // they can only represent integer values.
        _, unit = measurement.Scale(100 * minValue, o.SampleUnit, "minimum");

    }
    if (unit != "") {
        o.OutputUnit = unit;
    }
    else
 {
        o.OutputUnit = o.SampleUnit;
    }
}

// newGraph creates a new graph for this report. If nodes is non-nil,
// only nodes whose info matches are included. Otherwise, all nodes
// are included, without trimming.
private static ptr<graph.Graph> newGraph(this ptr<Report> _addr_rpt, graph.NodeSet nodes) {
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options; 

    // Clean up file paths using heuristics.
    var prof = rpt.prof;
    foreach (var (_, f) in prof.Function) {
        f.Filename = trimPath(f.Filename, o.TrimPath, o.SourcePath);
    }    foreach (var (_, s) in prof.Sample) {
        var numLabels = make_map<@string, slice<long>>(len(s.NumLabel));
        var numUnits = make_map<@string, slice<@string>>(len(s.NumLabel));
        foreach (var (k, vs) in s.NumLabel) {
            if (k == "bytes") {
                var unit = o.NumLabelUnits[k];
                var numValues = make_slice<long>(len(vs));
                var numUnit = make_slice<@string>(len(vs));
                foreach (var (i, v) in vs) {
                    numValues[i] = v;
                    numUnit[i] = unit;
                }
                numLabels[k] = append(numLabels[k], numValues);
                numUnits[k] = append(numUnits[k], numUnit);
            }
        }        s.NumLabel = numLabels;
        s.NumUnit = numUnits;
    }    prof.RemoveLabel("pprof::base");

    Func<long, @string, @string> formatTag = (v, key) => {
        return _addr_measurement.ScaledLabel(v, key, o.OutputUnit)!;
    };

    ptr<graph.Options> gopt = addr(new graph.Options(SampleValue:o.SampleValue,SampleMeanDivisor:o.SampleMeanDivisor,FormatTag:formatTag,CallTree:o.CallTree&&(o.OutputFormat==Dot||o.OutputFormat==Callgrind),DropNegative:o.DropNegative,KeptNodes:nodes,)); 

    // Only keep binary names for disassembly-based reports, otherwise
    // remove it to allow merging of functions across binaries.

    if (o.OutputFormat == Raw || o.OutputFormat == List || o.OutputFormat == WebList || o.OutputFormat == Dis || o.OutputFormat == Callgrind) 
        gopt.ObjNames = true;
        return _addr_graph.New(rpt.prof, gopt)!;

}

// printProto writes the incoming proto via thw writer w.
// If the divide_by option has been specified, samples are scaled appropriately.
private static error printProto(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var p = rpt.prof;
    var o = rpt.options; 

    // Apply the sample ratio to all samples before saving the profile.
    {
        var r = o.Ratio;

        if (r > 0 && r != 1) {
            foreach (var (_, sample) in p.Sample) {
                foreach (var (i, v) in sample.Value) {
                    sample.Value[i] = int64(float64(v) * r);
                }
            }
        }
    }

    return error.As(p.Write(w))!;

}

// printTopProto writes a list of the hottest routines in a profile as a profile.proto.
private static error printTopProto(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var p = rpt.prof;
    var o = rpt.options;
    var (g, _, _, _) = rpt.newTrimmedGraph();
    rpt.selectOutputUnit(g);

    profile.Profile @out = new profile.Profile(SampleType:[]*profile.ValueType{{Type:"cum",Unit:o.OutputUnit},{Type:"flat",Unit:o.OutputUnit},},TimeNanos:p.TimeNanos,DurationNanos:p.DurationNanos,PeriodType:p.PeriodType,Period:p.Period,);
    var functionMap = make(functionMap);
    foreach (var (i, n) in g.Nodes) {
        var (f, added) = functionMap.findOrAdd(n.Info);
        if (added) {
            @out.Function = append(@out.Function, f);
        }
        var flat = n.FlatValue();
        var cum = n.CumValue();
        ptr<profile.Location> l = addr(new profile.Location(ID:uint64(i+1),Address:n.Info.Address,Line:[]profile.Line{{Line:int64(n.Info.Lineno),Function:f,},},));

        var (fv, _) = measurement.Scale(flat, o.SampleUnit, o.OutputUnit);
        var (cv, _) = measurement.Scale(cum, o.SampleUnit, o.OutputUnit);
        ptr<profile.Sample> s = addr(new profile.Sample(Location:[]*profile.Location{l},Value:[]int64{int64(cv),int64(fv)},));
        @out.Location = append(@out.Location, l);
        @out.Sample = append(@out.Sample, s);

    }    return error.As(@out.Write(w))!;

}

private partial struct functionMap { // : map<@string, ptr<profile.Function>>
}

// findOrAdd takes a node representing a function, adds the function
// represented by the node to the map if the function is not already present,
// and returns the function the node represents. This also returns a boolean,
// which is true if the function was added and false otherwise.
private static (ptr<profile.Function>, bool) findOrAdd(this functionMap fm, graph.NodeInfo ni) {
    ptr<profile.Function> _p0 = default!;
    bool _p0 = default;

    var fName = fmt.Sprintf("%q%q%q%d", ni.Name, ni.OrigName, ni.File, ni.StartLine);

    {
        var f__prev1 = f;

        var f = fm[fName];

        if (f != null) {
            return (_addr_f!, false);
        }
        f = f__prev1;

    }


    f = addr(new profile.Function(ID:uint64(len(fm)+1),Name:ni.Name,SystemName:ni.OrigName,Filename:ni.File,StartLine:int64(ni.StartLine),));
    fm[fName] = f;
    return (_addr_f!, true);

}

// printAssembly prints an annotated assembly listing.
private static error printAssembly(io.Writer w, ptr<Report> _addr_rpt, plugin.ObjTool obj) {
    ref Report rpt = ref _addr_rpt.val;

    return error.As(PrintAssembly(w, _addr_rpt, obj, -1))!;
}

// PrintAssembly prints annotated disassembly of rpt to w.
public static error PrintAssembly(io.Writer w, ptr<Report> _addr_rpt, plugin.ObjTool obj, nint maxFuncs) {
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options;
    var prof = rpt.prof;

    var g = rpt.newGraph(null); 

    // If the regexp source can be parsed as an address, also match
    // functions that land on that address.
    ptr<ulong> address;
    {
        var (hex, err) = strconv.ParseUint(o.Symbol.String(), 0, 64);

        if (err == null) {
            address = _addr_hex;
        }
    }


    fmt.Fprintln(w, "Total:", rpt.formatValue(rpt.total));
    var symbols = symbolsFromBinaries(_addr_prof, _addr_g, _addr_o.Symbol, address, obj);
    var symNodes = nodesPerSymbol(g.Nodes, symbols); 

    // Sort for printing.
    slice<ptr<objSymbol>> syms = default;
    {
        var s__prev1 = s;

        foreach (var (__s) in symNodes) {
            s = __s;
            syms = append(syms, s);
        }
        s = s__prev1;
    }

    Func<ptr<objSymbol>, ptr<objSymbol>, bool> byName = (a, b) => {
        {
            var na = a.sym.Name[0];
            var nb = b.sym.Name[0];

            if (na != nb) {
                return error.As(na < nb)!;
            }

        }

        return error.As(a.sym.Start < b.sym.Start)!;

    };
    if (maxFuncs < 0) {
        sort.Sort(new orderSyms(syms,byName));
    }
    else
 {
        Func<ptr<objSymbol>, ptr<objSymbol>, bool> byFlatSum = (a, b) => {
            var (suma, _) = symNodes[a].Sum();
            var (sumb, _) = symNodes[b].Sum();
            if (suma != sumb) {
                return error.As(suma > sumb)!;
            }
            return error.As(byName(a, b))!;
        };
        sort.Sort(new orderSyms(syms,byFlatSum));
        if (len(syms) > maxFuncs) {
            syms = syms[..(int)maxFuncs];
        }
    }
    {
        var s__prev1 = s;

        foreach (var (_, __s) in syms) {
            s = __s;
            var sns = symNodes[s]; 

            // Gather samples for this symbol.
            var (flatSum, cumSum) = sns.Sum(); 

            // Get the function assembly.
            var (insts, err) = obj.Disasm(s.sym.File, s.sym.Start, s.sym.End, o.IntelSyntax);
            if (err != null) {
                return error.As(err)!;
            }

            var ns = annotateAssembly(insts, sns, s.file);

            fmt.Fprintf(w, "ROUTINE ======================== %s\n", s.sym.Name[0]);
            foreach (var (_, name) in s.sym.Name[(int)1..]) {
                fmt.Fprintf(w, "    AKA ======================== %s\n", name);
            }
            fmt.Fprintf(w, "%10s %10s (flat, cum) %s of Total\n", rpt.formatValue(flatSum), rpt.formatValue(cumSum), measurement.Percentage(cumSum, rpt.total));

            @string function = "";
            @string file = "";
            nint line = 0;
            foreach (var (_, n) in ns) {
                @string locStr = ""; 
                // Skip loc information if it hasn't changed from previous instruction.
                if (n.function != function || n.file != file || n.line != line) {
                    (function, file, line) = (n.function, n.file, n.line);                    if (n.function != "") {
                        locStr = n.function + " ";
                    }
                    if (n.file != "") {
                        locStr += n.file;
                        if (n.line != 0) {
                            locStr += fmt.Sprintf(":%d", n.line);
                        }
                    }
                }


                if (locStr == "") 
                    // No location info, just print the instruction.
                    fmt.Fprintf(w, "%10s %10s %10x: %s\n", valueOrDot(n.flatValue(), _addr_rpt), valueOrDot(n.cumValue(), _addr_rpt), n.address, n.instruction);
                else if (len(n.instruction) < 40) 
                    // Short instruction, print loc on the same line.
                    fmt.Fprintf(w, "%10s %10s %10x: %-40s;%s\n", valueOrDot(n.flatValue(), _addr_rpt), valueOrDot(n.cumValue(), _addr_rpt), n.address, n.instruction, locStr);
                else 
                    // Long instruction, print loc on a separate line.
                    fmt.Fprintf(w, "%74s;%s\n", "", locStr);
                    fmt.Fprintf(w, "%10s %10s %10x: %s\n", valueOrDot(n.flatValue(), _addr_rpt), valueOrDot(n.cumValue(), _addr_rpt), n.address, n.instruction);
                
            }

        }
        s = s__prev1;
    }

    return error.As(null!)!;

}

// symbolsFromBinaries examines the binaries listed on the profile
// that have associated samples, and identifies symbols matching rx.
private static slice<ptr<objSymbol>> symbolsFromBinaries(ptr<profile.Profile> _addr_prof, ptr<graph.Graph> _addr_g, ptr<regexp.Regexp> _addr_rx, ptr<ulong> _addr_address, plugin.ObjTool obj) {
    ref profile.Profile prof = ref _addr_prof.val;
    ref graph.Graph g = ref _addr_g.val;
    ref regexp.Regexp rx = ref _addr_rx.val;
    ref ulong address = ref _addr_address.val;

    var hasSamples = make_map<@string, bool>(); 
    // Only examine mappings that have samples that match the
    // regexp. This is an optimization to speed up pprof.
    foreach (var (_, n) in g.Nodes) {
        {
            var name = n.Info.PrintableName();

            if (rx.MatchString(name) && n.Info.Objfile != "") {
                hasSamples[n.Info.Objfile] = true;
            }

        }

    }    slice<ptr<objSymbol>> objSyms = default;
    foreach (var (_, m) in prof.Mapping) {
        if (!hasSamples[m.File]) {
            if (address == null || !(m.Start <= address && address <= m.Limit.val)) {
                continue;
            }
        }
        var (f, err) = obj.Open(m.File, m.Start, m.Limit, m.Offset);
        if (err != null) {
            fmt.Printf("%v\n", err);
            continue;
        }
        ulong addr = default;
        if (address != null) {
            addr = address;
        }
        var (msyms, err) = f.Symbols(rx, addr);
        f.Close();
        if (err != null) {
            continue;
        }
        foreach (var (_, ms) in msyms) {
            objSyms = append(objSyms, addr(new objSymbol(sym:ms,file:f,)));
        }
    }    return objSyms;

}

// objSym represents a symbol identified from a binary. It includes
// the SymbolInfo from the disasm package and the base that must be
// added to correspond to sample addresses
private partial struct objSymbol {
    public ptr<plugin.Sym> sym;
    public plugin.ObjFile file;
}

// orderSyms is a wrapper type to sort []*objSymbol by a supplied comparator.
private partial struct orderSyms {
    public slice<ptr<objSymbol>> v;
    public Func<ptr<objSymbol>, ptr<objSymbol>, bool> less;
}

private static nint Len(this orderSyms o) {
    return len(o.v);
}
private static bool Less(this orderSyms o, nint i, nint j) {
    return o.less(o.v[i], o.v[j]);
}
private static void Swap(this orderSyms o, nint i, nint j) {
    (o.v[i], o.v[j]) = (o.v[j], o.v[i]);
}

// nodesPerSymbol classifies nodes into a group of symbols.
private static map<ptr<objSymbol>, graph.Nodes> nodesPerSymbol(graph.Nodes ns, slice<ptr<objSymbol>> symbols) {
    var symNodes = make_map<ptr<objSymbol>, graph.Nodes>();
    foreach (var (_, s) in symbols) { 
        // Gather samples for this symbol.
        foreach (var (_, n) in ns) {
            {
                var (address, err) = s.file.ObjAddr(n.Info.Address);

                if (err == null && address >= s.sym.Start && address < s.sym.End) {
                    symNodes[s] = append(symNodes[s], n);
                }

            }

        }
    }    return symNodes;

}

private partial struct assemblyInstruction {
    public ulong address;
    public @string instruction;
    public @string function;
    public @string file;
    public nint line;
    public long flat;
    public long cum;
    public long flatDiv;
    public long cumDiv;
    public bool startsBlock;
    public slice<callID> inlineCalls;
}

private partial struct callID {
    public @string file;
    public nint line;
}

private static long flatValue(this ptr<assemblyInstruction> _addr_a) {
    ref assemblyInstruction a = ref _addr_a.val;

    if (a.flatDiv != 0) {
        return a.flat / a.flatDiv;
    }
    return a.flat;

}

private static long cumValue(this ptr<assemblyInstruction> _addr_a) {
    ref assemblyInstruction a = ref _addr_a.val;

    if (a.cumDiv != 0) {
        return a.cum / a.cumDiv;
    }
    return a.cum;

}

// annotateAssembly annotates a set of assembly instructions with a
// set of samples. It returns a set of nodes to display. base is an
// offset to adjust the sample addresses.
private static slice<assemblyInstruction> annotateAssembly(slice<plugin.Inst> insts, graph.Nodes samples, plugin.ObjFile file) { 
    // Add end marker to simplify printing loop.
    insts = append(insts, new plugin.Inst(Addr:^uint64(0),)); 

    // Ensure samples are sorted by address.
    samples.Sort(graph.AddressOrder);

    nint s = 0;
    var asm = make_slice<assemblyInstruction>(0, len(insts));
    foreach (var (ix, in) in insts[..(int)len(insts) - 1]) {
        assemblyInstruction n = new assemblyInstruction(address:in.Addr,instruction:in.Text,function:in.Function,line:in.Line,);
        if (@in.File != "") {
            n.file = filepath.Base(@in.File);
        }
        for (var next = insts[ix + 1].Addr; s < len(samples); s++) {
            {
                var (addr, err) = file.ObjAddr(samples[s].Info.Address);

                if (err != null || addr >= next) {
                    break;
                }

            }

            var sample = samples[s];
            n.flatDiv += sample.FlatDiv;
            n.flat += sample.Flat;
            n.cumDiv += sample.CumDiv;
            n.cum += sample.Cum;
            {
                var f__prev1 = f;

                var f = sample.Info.File;

                if (f != "" && n.file == "") {
                    n.file = filepath.Base(f);
                }

                f = f__prev1;

            }

            {
                var ln = sample.Info.Lineno;

                if (ln != 0 && n.line == 0) {
                    n.line = ln;
                }

            }

            {
                var f__prev1 = f;

                f = sample.Info.Name;

                if (f != "" && n.function == "") {
                    n.function = f;
                }

                f = f__prev1;

            }

        }
        asm = append(asm, n);

    }    return asm;

}

// valueOrDot formats a value according to a report, intercepting zero
// values.
private static @string valueOrDot(long value, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    if (value == 0) {
        return ".";
    }
    return rpt.formatValue(value);

}

// printTags collects all tags referenced in the profile and prints
// them in a sorted table.
private static error printTags(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var p = rpt.prof;

    var o = rpt.options;
    Func<long, @string, @string> formatTag = (v, key) => {
        return error.As(measurement.ScaledLabel(v, key, o.OutputUnit))!;
    }; 

    // Hashtable to keep accumulate tags as key,value,count.
    var tagMap = make_map<@string, map<@string, long>>();
    foreach (var (_, s) in p.Sample) {
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
                        var (valueMap, ok) = tagMap[key];
                        if (!ok) {
                            valueMap = make_map<@string, long>();
                            tagMap[key] = valueMap;
                        }
                        valueMap[val] += o.SampleValue(s.Value);
                    }

                    val = val__prev3;
                }
            }

            key = key__prev2;
            vals = vals__prev2;
        }

        {
            var key__prev2 = key;
            var vals__prev2 = vals;

            foreach (var (__key, __vals) in s.NumLabel) {
                key = __key;
                vals = __vals;
                var unit = o.NumLabelUnits[key];
                foreach (var (_, nval) in vals) {
                    var val = formatTag(nval, unit);
                    (valueMap, ok) = tagMap[key];
                    if (!ok) {
                        valueMap = make_map<@string, long>();
                        tagMap[key] = valueMap;
                    }
                    valueMap[val] += o.SampleValue(s.Value);
                }
            }

            key = key__prev2;
            vals = vals__prev2;
        }
    }    var tagKeys = make_slice<ptr<graph.Tag>>(0, len(tagMap));
    {
        var key__prev1 = key;

        foreach (var (__key) in tagMap) {
            key = __key;
            tagKeys = append(tagKeys, addr(new graph.Tag(Name:key)));
        }
        key = key__prev1;
    }

    var tabw = tabwriter.NewWriter(w, 0, 0, 1, ' ', tabwriter.AlignRight);
    foreach (var (_, tagKey) in graph.SortTags(tagKeys, true)) {
        long total = default;
        var key = tagKey.Name;
        var tags = make_slice<ptr<graph.Tag>>(0, len(tagMap[key]));
        {
            var t__prev2 = t;

            foreach (var (__t, __c) in tagMap[key]) {
                t = __t;
                c = __c;
                total += c;
                tags = append(tags, addr(new graph.Tag(Name:t,Flat:c)));
            }

            t = t__prev2;
        }

        var (f, u) = measurement.Scale(total, o.SampleUnit, o.OutputUnit);
        fmt.Fprintf(tabw, "%s:\t Total %.1f%s\n", key, f, u);
        {
            var t__prev2 = t;

            foreach (var (_, __t) in graph.SortTags(tags, true)) {
                t = __t;
                (f, u) = measurement.Scale(t.FlatValue(), o.SampleUnit, o.OutputUnit);
                if (total > 0) {
                    fmt.Fprintf(tabw, " \t%.1f%s (%s):\t %s\n", f, u, measurement.Percentage(t.FlatValue(), total), t.Name);
                }
                else
 {
                    fmt.Fprintf(tabw, " \t%.1f%s:\t %s\n", f, u, t.Name);
                }

            }

            t = t__prev2;
        }

        fmt.Fprintln(tabw);

    }    return error.As(tabw.Flush())!;

}

// printComments prints all freeform comments in the profile.
private static error printComments(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var p = rpt.prof;

    foreach (var (_, c) in p.Comments) {
        fmt.Fprintln(w, c);
    }    return error.As(null!)!;
}

// TextItem holds a single text report entry.
public partial struct TextItem {
    public @string Name;
    public @string InlineLabel; // Not empty if inlined
    public long Flat; // Raw values
    public long Cum; // Raw values
    public @string FlatFormat; // Formatted values
    public @string CumFormat; // Formatted values
}

// TextItems returns a list of text items from the report and a list
// of labels that describe the report.
public static (slice<TextItem>, slice<@string>) TextItems(ptr<Report> _addr_rpt) {
    slice<TextItem> _p0 = default;
    slice<@string> _p0 = default;
    ref Report rpt = ref _addr_rpt.val;

    var (g, origCount, droppedNodes, _) = rpt.newTrimmedGraph();
    rpt.selectOutputUnit(g);
    var labels = reportLabels(_addr_rpt, _addr_g, origCount, droppedNodes, 0, false);

    slice<TextItem> items = default;
    long flatSum = default;
    foreach (var (_, n) in g.Nodes) {
        var name = n.Info.PrintableName();
        var flat = n.FlatValue();
        var cum = n.CumValue();

        bool inline = default;        bool noinline = default;

        foreach (var (_, e) in n.In) {
            if (e.Inline) {
                inline = true;
            }
            else
 {
                noinline = true;
            }

        }        @string inl = default;
        if (inline) {
            if (noinline) {
                inl = "(partial-inline)";
            }
            else
 {
                inl = "(inline)";
            }

        }
        flatSum += flat;
        items = append(items, new TextItem(Name:name,InlineLabel:inl,Flat:flat,Cum:cum,FlatFormat:rpt.formatValue(flat),CumFormat:rpt.formatValue(cum),));

    }    return (items, labels);

}

// printText prints a flat text report for a profile.
private static error printText(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var (items, labels) = TextItems(_addr_rpt);
    fmt.Fprintln(w, strings.Join(labels, "\n"));
    fmt.Fprintf(w, "%10s %5s%% %5s%% %10s %5s%%\n", "flat", "flat", "sum", "cum", "cum");
    long flatSum = default;
    foreach (var (_, item) in items) {
        var inl = item.InlineLabel;
        if (inl != "") {
            inl = " " + inl;
        }
        flatSum += item.Flat;
        fmt.Fprintf(w, "%10s %s %s %10s %s  %s%s\n", item.FlatFormat, measurement.Percentage(item.Flat, rpt.total), measurement.Percentage(flatSum, rpt.total), item.CumFormat, measurement.Percentage(item.Cum, rpt.total), item.Name, inl);

    }    return error.As(null!)!;

}

// printTraces prints all traces from a profile.
private static error printTraces(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    fmt.Fprintln(w, strings.Join(ProfileLabels(_addr_rpt), "\n"));

    var prof = rpt.prof;
    var o = rpt.options;

    const @string separator = "-----------+-------------------------------------------------------";



    var (_, locations) = graph.CreateNodes(prof, addr(new graph.Options()));
    foreach (var (_, sample) in prof.Sample) {
        private partial struct stk {
            public ref ptr<graph.NodeInfo> NodeInfo> => ref NodeInfo>_ptr;
            public bool inline;
        }
        slice<stk> stack = default;
        foreach (var (_, loc) in sample.Location) {
            var nodes = locations[loc.ID];
            {
                var i__prev3 = i;

                foreach (var (__i, __n) in nodes) {
                    i = __i;
                    n = __n; 
                    // The inline flag may be inaccurate if 'show' or 'hide' filter is
                    // used. See https://github.com/google/pprof/issues/511.
                    var inline = i != len(nodes) - 1;
                    stack = append(stack, new stk(&n.Info,inline));

                }

                i = i__prev3;
            }
        }        if (len(stack) == 0) {
            continue;
        }
        fmt.Fprintln(w, separator); 
        // Print any text labels for the sample.
        slice<@string> labels = default;
        {
            var s__prev2 = s;
            var vs__prev2 = vs;

            foreach (var (__s, __vs) in sample.Label) {
                s = __s;
                vs = __vs;
                labels = append(labels, fmt.Sprintf("%10s:  %s\n", s, strings.Join(vs, " ")));
            }

            s = s__prev2;
            vs = vs__prev2;
        }

        sort.Strings(labels);
        fmt.Fprint(w, strings.Join(labels, "")); 

        // Print any numeric labels for the sample
        slice<@string> numLabels = default;
        foreach (var (key, vals) in sample.NumLabel) {
            var unit = o.NumLabelUnits[key];
            var numValues = make_slice<@string>(len(vals));
            {
                var i__prev3 = i;

                foreach (var (__i, __vv) in vals) {
                    i = __i;
                    vv = __vv;
                    numValues[i] = measurement.Label(vv, unit);
                }

                i = i__prev3;
            }

            numLabels = append(numLabels, fmt.Sprintf("%10s:  %s\n", key, strings.Join(numValues, " ")));

        }        sort.Strings(numLabels);
        fmt.Fprint(w, strings.Join(numLabels, ""));

        long d = default;        long v = default;

        v = o.SampleValue(sample.Value);
        if (o.SampleMeanDivisor != null) {
            d = o.SampleMeanDivisor(sample.Value);
        }
        if (d != 0) {
            v = v / d;
        }
        {
            var i__prev2 = i;
            var s__prev2 = s;

            foreach (var (__i, __s) in stack) {
                i = __i;
                s = __s;
                @string vs = default;                inline = default;

                if (i == 0) {
                    vs = rpt.formatValue(v);
                }
                if (s.inline) {
                    inline = " (inline)";
                }
                fmt.Fprintf(w, "%10s   %s%s\n", vs, s.PrintableName(), inline);
            }

            i = i__prev2;
            s = s__prev2;
        }
    }    fmt.Fprintln(w, separator);
    return error.As(null!)!;

}

// printCallgrind prints a graph for a profile on callgrind format.
private static error printCallgrind(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var o = rpt.options;
    rpt.options.NodeFraction = 0;
    rpt.options.EdgeFraction = 0;
    rpt.options.NodeCount = 0;

    var (g, _, _, _) = rpt.newTrimmedGraph();
    rpt.selectOutputUnit(g);

    var nodeNames = getDisambiguatedNames(_addr_g);

    fmt.Fprintln(w, "positions: instr line");
    fmt.Fprintln(w, "events:", o.SampleType + "(" + o.OutputUnit + ")");

    var objfiles = make_map<@string, nint>();
    var files = make_map<@string, nint>();
    var names = make_map<@string, nint>(); 

    // prevInfo points to the previous NodeInfo.
    // It is used to group cost lines together as much as possible.
    ptr<graph.NodeInfo> prevInfo;
    foreach (var (_, n) in g.Nodes) {
        if (prevInfo == null || n.Info.Objfile != prevInfo.Objfile || n.Info.File != prevInfo.File || n.Info.Name != prevInfo.Name) {
            fmt.Fprintln(w);
            fmt.Fprintln(w, "ob=" + callgrindName(objfiles, n.Info.Objfile));
            fmt.Fprintln(w, "fl=" + callgrindName(files, n.Info.File));
            fmt.Fprintln(w, "fn=" + callgrindName(names, n.Info.Name));
        }
        var addr = callgrindAddress(prevInfo, n.Info.Address);
        var (sv, _) = measurement.Scale(n.FlatValue(), o.SampleUnit, o.OutputUnit);
        fmt.Fprintf(w, "%s %d %d\n", addr, n.Info.Lineno, int64(sv)); 

        // Print outgoing edges.
        foreach (var (_, out) in n.Out.Sort()) {
            var (c, _) = measurement.Scale(@out.Weight, o.SampleUnit, o.OutputUnit);
            var callee = @out.Dest;
            fmt.Fprintln(w, "cfl=" + callgrindName(files, callee.Info.File));
            fmt.Fprintln(w, "cfn=" + callgrindName(names, nodeNames[callee])); 
            // pprof doesn't have a flat weight for a call, leave as 0.
            fmt.Fprintf(w, "calls=0 %s %d\n", callgrindAddress(prevInfo, callee.Info.Address), callee.Info.Lineno); 
            // TODO: This address may be in the middle of a call
            // instruction. It would be best to find the beginning
            // of the instruction, but the tools seem to handle
            // this OK.
            fmt.Fprintf(w, "* * %d\n", int64(c));

        }        prevInfo = _addr_n.Info;

    }    return error.As(null!)!;

}

// getDisambiguatedNames returns a map from each node in the graph to
// the name to use in the callgrind output. Callgrind merges all
// functions with the same [file name, function name]. Add a [%d/n]
// suffix to disambiguate nodes with different values of
// node.Function, which we want to keep separate. In particular, this
// affects graphs created with --call_tree, where nodes from different
// contexts are associated to different Functions.
private static map<ptr<graph.Node>, @string> getDisambiguatedNames(ptr<graph.Graph> _addr_g) {
    ref graph.Graph g = ref _addr_g.val;

    var nodeName = make_map<ptr<graph.Node>, @string>(len(g.Nodes));

    private partial struct names {
        public @string file;
        public @string function;
    } 

    // nameFunctionIndex maps the callgrind names (filename, function)
    // to the node.Function values found for that name, and each
    // node.Function value to a sequential index to be used on the
    // disambiguated name.
    var nameFunctionIndex = make_map<names, map<ptr<graph.Node>, nint>>();
    {
        var n__prev1 = n;

        foreach (var (_, __n) in g.Nodes) {
            n = __n;
            names nm = new names(n.Info.File,n.Info.Name);
            var (p, ok) = nameFunctionIndex[nm];
            if (!ok) {
                p = make_map<ptr<graph.Node>, nint>();
                nameFunctionIndex[nm] = p;
            }
            {
                var (_, ok) = p[n.Function];

                if (!ok) {
                    p[n.Function] = len(p);
                }

            }

        }
        n = n__prev1;
    }

    {
        var n__prev1 = n;

        foreach (var (_, __n) in g.Nodes) {
            n = __n;
            nm = new names(n.Info.File,n.Info.Name);
            nodeName[n] = n.Info.Name;
            {
                var p__prev1 = p;

                var p = nameFunctionIndex[nm];

                if (len(p) > 1) { 
                    // If there is more than one function, add suffix to disambiguate.
                    nodeName[n] += fmt.Sprintf(" [%d/%d]", p[n.Function] + 1, len(p));

                }

                p = p__prev1;

            }

        }
        n = n__prev1;
    }

    return nodeName;

}

// callgrindName implements the callgrind naming compression scheme.
// For names not previously seen returns "(N) name", where N is a
// unique index. For names previously seen returns "(N)" where N is
// the index returned the first time.
private static @string callgrindName(map<@string, nint> names, @string name) {
    if (name == "") {
        return "";
    }
    {
        var id__prev1 = id;

        var (id, ok) = names[name];

        if (ok) {
            return fmt.Sprintf("(%d)", id);
        }
        id = id__prev1;

    }

    var id = len(names) + 1;
    names[name] = id;
    return fmt.Sprintf("(%d) %s", id, name);

}

// callgrindAddress implements the callgrind subposition compression scheme if
// possible. If prevInfo != nil, it contains the previous address. The current
// address can be given relative to the previous address, with an explicit +/-
// to indicate it is relative, or * for the same address.
private static @string callgrindAddress(ptr<graph.NodeInfo> _addr_prevInfo, ulong curr) {
    ref graph.NodeInfo prevInfo = ref _addr_prevInfo.val;

    var abs = fmt.Sprintf("%#x", curr);
    if (prevInfo == null) {
        return abs;
    }
    var prev = prevInfo.Address;
    if (prev == curr) {
        return "*";
    }
    var diff = int64(curr - prev);
    var relative = fmt.Sprintf("%+d", diff); 

    // Only bother to use the relative address if it is actually shorter.
    if (len(relative) < len(abs)) {
        return relative;
    }
    return abs;

}

// printTree prints a tree-based report in text form.
private static error printTree(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    const @string separator = "----------------------------------------------------------+-------------";

    const @string legend = "      flat  flat%   sum%        cum   cum%   calls calls% + context 	 	 ";



    var (g, origCount, droppedNodes, _) = rpt.newTrimmedGraph();
    rpt.selectOutputUnit(g);

    fmt.Fprintln(w, strings.Join(reportLabels(_addr_rpt, _addr_g, origCount, droppedNodes, 0, false), "\n"));

    fmt.Fprintln(w, separator);
    fmt.Fprintln(w, legend);
    long flatSum = default;

    var rx = rpt.options.Symbol;
    foreach (var (_, n) in g.Nodes) {
        var name = n.Info.PrintableName();
        var flat = n.FlatValue();
        var cum = n.CumValue(); 

        // Skip any entries that do not match the regexp (for the "peek" command).
        if (rx != null && !rx.MatchString(name)) {
            continue;
        }
        fmt.Fprintln(w, separator); 
        // Print incoming edges.
        var inEdges = n.In.Sort();
        foreach (var (_, in) in inEdges) {
            @string inline = default;
            if (@in.Inline) {
                inline = " (inline)";
            }
            fmt.Fprintf(w, "%50s %s |   %s%s\n", rpt.formatValue(@in.Weight), measurement.Percentage(@in.Weight, cum), @in.Src.Info.PrintableName(), inline);
        }        flatSum += flat;
        fmt.Fprintf(w, "%10s %s %s %10s %s                | %s\n", rpt.formatValue(flat), measurement.Percentage(flat, rpt.total), measurement.Percentage(flatSum, rpt.total), rpt.formatValue(cum), measurement.Percentage(cum, rpt.total), name); 

        // Print outgoing edges.
        var outEdges = n.Out.Sort();
        foreach (var (_, out) in outEdges) {
            inline = default;
            if (@out.Inline) {
                inline = " (inline)";
            }
            fmt.Fprintf(w, "%50s %s |   %s%s\n", rpt.formatValue(@out.Weight), measurement.Percentage(@out.Weight, cum), @out.Dest.Info.PrintableName(), inline);
        }
    }    if (len(g.Nodes) > 0) {
        fmt.Fprintln(w, separator);
    }
    return error.As(null!)!;

}

// GetDOT returns a graph suitable for dot processing along with some
// configuration information.
public static (ptr<graph.Graph>, ptr<graph.DotConfig>) GetDOT(ptr<Report> _addr_rpt) {
    ptr<graph.Graph> _p0 = default!;
    ptr<graph.DotConfig> _p0 = default!;
    ref Report rpt = ref _addr_rpt.val;

    var (g, origCount, droppedNodes, droppedEdges) = rpt.newTrimmedGraph();
    rpt.selectOutputUnit(g);
    var labels = reportLabels(_addr_rpt, _addr_g, origCount, droppedNodes, droppedEdges, true);

    ptr<graph.DotConfig> c = addr(new graph.DotConfig(Title:rpt.options.Title,Labels:labels,FormatValue:rpt.formatValue,Total:rpt.total,));
    return (_addr_g!, _addr_c!);
}

// printDOT prints an annotated callgraph in DOT format.
private static error printDOT(io.Writer w, ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    var (g, c) = GetDOT(_addr_rpt);
    graph.ComposeDot(w, g, addr(new graph.DotAttributes()), c);
    return error.As(null!)!;
}

// ProfileLabels returns printable labels for a profile.
public static slice<@string> ProfileLabels(ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    @string label = new slice<@string>(new @string[] {  });
    var prof = rpt.prof;
    var o = rpt.options;
    if (len(prof.Mapping) > 0) {
        if (prof.Mapping[0].File != "") {
            label = append(label, "File: " + filepath.Base(prof.Mapping[0].File));
        }
        if (prof.Mapping[0].BuildID != "") {
            label = append(label, "Build ID: " + prof.Mapping[0].BuildID);
        }
    }
    foreach (var (_, c) in prof.Comments) {
        if (!strings.HasPrefix(c, "#")) {
            label = append(label, c);
        }
    }    if (o.SampleType != "") {
        label = append(label, "Type: " + o.SampleType);
    }
    if (prof.TimeNanos != 0) {
        const @string layout = "Jan 2, 2006 at 3:04pm (MST)";

        label = append(label, "Time: " + time.Unix(0, prof.TimeNanos).Format(layout));
    }
    if (prof.DurationNanos != 0) {
        var duration = measurement.Label(prof.DurationNanos, "nanoseconds");
        var (totalNanos, totalUnit) = measurement.Scale(rpt.total, o.SampleUnit, "nanoseconds");
        @string ratio = default;
        if (totalUnit == "ns" && totalNanos != 0) {
            ratio = "(" + measurement.Percentage(int64(totalNanos), prof.DurationNanos) + ")";
        }
        label = append(label, fmt.Sprintf("Duration: %s, Total samples = %s %s", duration, rpt.formatValue(rpt.total), ratio));

    }
    return label;

}

// reportLabels returns printable labels for a report. Includes
// profileLabels.
private static slice<@string> reportLabels(ptr<Report> _addr_rpt, ptr<graph.Graph> _addr_g, nint origCount, nint droppedNodes, nint droppedEdges, bool fullHeaders) {
    ref Report rpt = ref _addr_rpt.val;
    ref graph.Graph g = ref _addr_g.val;

    var nodeFraction = rpt.options.NodeFraction;
    var edgeFraction = rpt.options.EdgeFraction;
    var nodeCount = len(g.Nodes);

    slice<@string> label = default;
    if (len(rpt.options.ProfileLabels) > 0) {
        label = append(label, rpt.options.ProfileLabels);
    }
    else if (fullHeaders || !rpt.options.CompactLabels) {
        label = ProfileLabels(_addr_rpt);
    }
    long flatSum = default;
    foreach (var (_, n) in g.Nodes) {
        flatSum = flatSum + n.FlatValue();
    }    if (len(rpt.options.ActiveFilters) > 0) {
        var activeFilters = legendActiveFilters(rpt.options.ActiveFilters);
        label = append(label, activeFilters);
    }
    label = append(label, fmt.Sprintf("Showing nodes accounting for %s, %s of %s total", rpt.formatValue(flatSum), strings.TrimSpace(measurement.Percentage(flatSum, rpt.total)), rpt.formatValue(rpt.total)));

    if (rpt.total != 0) {
        if (droppedNodes > 0) {
            label = append(label, genLabel(droppedNodes, "node", "cum", rpt.formatValue(abs64(int64(float64(rpt.total) * nodeFraction)))));
        }
        if (droppedEdges > 0) {
            label = append(label, genLabel(droppedEdges, "edge", "freq", rpt.formatValue(abs64(int64(float64(rpt.total) * edgeFraction)))));
        }
        if (nodeCount > 0 && nodeCount < origCount) {
            label = append(label, fmt.Sprintf("Showing top %d nodes out of %d", nodeCount, origCount));
        }
    }
    if (fullHeaders) {
        label = append(label, "\nSee https://git.io/JfYMW for how to read the graph");
    }
    return label;

}

private static slice<@string> legendActiveFilters(slice<@string> activeFilters) {
    var legendActiveFilters = make_slice<@string>(len(activeFilters) + 1);
    legendActiveFilters[0] = "Active filters:";
    foreach (var (i, s) in activeFilters) {
        if (len(s) > 80) {
            s = s[..(int)80] + "…";
        }
        legendActiveFilters[i + 1] = "   " + s;

    }    return legendActiveFilters;

}

private static @string genLabel(nint d, @string n, @string l, @string f) {
    if (d > 1) {
        n = n + "s";
    }
    return fmt.Sprintf("Dropped %d %s (%s <= %s)", d, n, l, f);

}

// New builds a new report indexing the sample values interpreting the
// samples with the provided function.
public static ptr<Report> New(ptr<profile.Profile> _addr_prof, ptr<Options> _addr_o) {
    ref profile.Profile prof = ref _addr_prof.val;
    ref Options o = ref _addr_o.val;

    Func<long, @string> format = v => {
        {
            var r = o.Ratio;

            if (r > 0 && r != 1) {
                var fv = float64(v) * r;
                v = int64(fv);
            }

        }

        return _addr_measurement.ScaledLabel(v, o.SampleUnit, o.OutputUnit)!;

    };
    return addr(new Report(prof,computeTotal(prof,o.SampleValue,o.SampleMeanDivisor),o,format));

}

// NewDefault builds a new report indexing the last sample value
// available.
public static ptr<Report> NewDefault(ptr<profile.Profile> _addr_prof, Options options) {
    ref profile.Profile prof = ref _addr_prof.val;

    var index = len(prof.SampleType) - 1;
    var o = _addr_options;
    if (o.Title == "" && len(prof.Mapping) > 0 && prof.Mapping[0].File != "") {
        o.Title = filepath.Base(prof.Mapping[0].File);
    }
    o.SampleType = prof.SampleType[index].Type;
    o.SampleUnit = strings.ToLower(prof.SampleType[index].Unit);
    o.SampleValue = v => {
        return _addr_v[index]!;
    };
    return _addr_New(_addr_prof, _addr_o)!;

}

// computeTotal computes the sum of the absolute value of all sample values.
// If any samples have label indicating they belong to the diff base, then the
// total will only include samples with that label.
private static long computeTotal(ptr<profile.Profile> _addr_prof, Func<slice<long>, long> value, Func<slice<long>, long> meanDiv) {
    ref profile.Profile prof = ref _addr_prof.val;

    long div = default;    long total = default;    long diffDiv = default;    long diffTotal = default;

    foreach (var (_, sample) in prof.Sample) {
        long d = default;        long v = default;

        v = value(sample.Value);
        if (meanDiv != null) {
            d = meanDiv(sample.Value);
        }
        if (v < 0) {
            v = -v;
        }
        total += v;
        div += d;
        if (sample.DiffBaseSample()) {
            diffTotal += v;
            diffDiv += d;
        }
    }    if (diffTotal > 0) {
        total = diffTotal;
        div = diffDiv;
    }
    if (div != 0) {
        return total / div;
    }
    return total;

}

// Report contains the data and associated routines to extract a
// report from a profile.
public partial struct Report {
    public ptr<profile.Profile> prof;
    public long total;
    public ptr<Options> options;
    public Func<long, @string> formatValue;
}

// Total returns the total number of samples in a report.
private static long Total(this ptr<Report> _addr_rpt) {
    ref Report rpt = ref _addr_rpt.val;

    return rpt.total;
}

private static long abs64(long i) {
    if (i < 0) {
        return -i;
    }
    return i;

}

} // end report_package

// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Minimum mutator utilization (MMU) graphing.

// TODO:
//
// In worst window list, show break-down of GC utilization sources
// (STW, assist, etc). Probably requires a different MutatorUtil
// representation.
//
// When a window size is selected, show a second plot of the mutator
// utilization distribution for that window size.
//
// Render plot progressively so rough outline is visible quickly even
// for very complex MUTs. Start by computing just a few window sizes
// and then add more window sizes.
//
// Consider using sampling to compute an approximate MUT. This would
// work by sampling the mutator utilization at randomly selected
// points in time in the trace to build an empirical distribution. We
// could potentially put confidence intervals on these estimates and
// render this progressively as we refine the distributions.

// package main -- go2cs converted at 2022 March 13 06:36:09 UTC
// Original source: C:\Program Files\Go\src\cmd\trace\mmu.go
namespace go;

using json = encoding.json_package;
using fmt = fmt_package;
using trace = @internal.trace_package;
using log = log_package;
using math = math_package;
using http = net.http_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;
using time = time_package;
using System;

public static partial class main_package {

private static void init() {
    http.HandleFunc("/mmu", httpMMU);
    http.HandleFunc("/mmuPlot", httpMMUPlot);
    http.HandleFunc("/mmuDetails", httpMMUDetails);
}

private static map utilFlagNames = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, trace.UtilFlags>{"perProc":trace.UtilPerProc,"stw":trace.UtilSTW,"background":trace.UtilBackground,"assist":trace.UtilAssist,"sweep":trace.UtilSweep,};

private partial struct mmuCacheEntry {
    public sync.Once init;
    public slice<slice<trace.MutatorUtil>> util;
    public ptr<trace.MMUCurve> mmuCurve;
    public error err;
}

private static var mmuCache = default;

private static void init() {
    mmuCache.m = make_map<trace.UtilFlags, ptr<mmuCacheEntry>>();
}

private static (slice<slice<trace.MutatorUtil>>, ptr<trace.MMUCurve>, error) getMMUCurve(ptr<http.Request> _addr_r) {
    slice<slice<trace.MutatorUtil>> _p0 = default;
    ptr<trace.MMUCurve> _p0 = default!;
    error _p0 = default!;
    ref http.Request r = ref _addr_r.val;

    trace.UtilFlags flags = default;
    foreach (var (_, flagStr) in strings.Split(r.FormValue("flags"), "|")) {
        flags |= utilFlagNames[flagStr];
    }    mmuCache.@lock.Lock();
    var c = mmuCache.m[flags];
    if (c == null) {
        c = @new<mmuCacheEntry>();
        mmuCache.m[flags] = c;
    }
    mmuCache.@lock.Unlock();

    c.init.Do(() => {
        var (events, err) = parseEvents();
        if (err != null) {
            c.err = err;
        }
        else
 {
            c.util = trace.MutatorUtilization(events, flags);
            c.mmuCurve = trace.NewMMUCurve(c.util);
        }
    });
    return (c.util, _addr_c.mmuCurve!, error.As(c.err)!);
}

// httpMMU serves the MMU plot page.
private static void httpMMU(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    http.ServeContent(w, r, "", new time.Time(), strings.NewReader(templMMU));
}

// httpMMUPlot serves the JSON data for the MMU plot.
private static void httpMMUPlot(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    var (mu, mmuCurve, err) = getMMUCurve(_addr_r);
    if (err != null) {
        http.Error(w, fmt.Sprintf("failed to parse events: %v", err), http.StatusInternalServerError);
        return ;
    }
    slice<double> quantiles = default;
    foreach (var (_, flagStr) in strings.Split(r.FormValue("flags"), "|")) {
        if (flagStr == "mut") {
            quantiles = new slice<double>(new double[] { 0, 1-.999, 1-.99, 1-.95 });
            break;
        }
    }    var xMin = time.Second;
    while (xMin > 1) {
        {
            var mmu = mmuCurve.MMU(xMin);

            if (mmu < 0.0001F) {
                break;
            }

        }
        xMin /= 1000;
    } 
    // Cover six orders of magnitude.
    var xMax = xMin * 1e6F; 
    // But no more than the length of the trace.
    var minEvent = mu[0][0].Time;
    var maxEvent = mu[0][len(mu[0]) - 1].Time;
    foreach (var (_, mu1) in mu[(int)1..]) {
        if (mu1[0].Time < minEvent) {
            minEvent = mu1[0].Time;
        }
        if (mu1[len(mu1) - 1].Time > maxEvent) {
            maxEvent = mu1[len(mu1) - 1].Time;
        }
    }    {
        var maxMax = time.Duration(maxEvent - minEvent);

        if (xMax > maxMax) {
            xMax = maxMax;
        }
    } 
    // Compute MMU curve.
    var logMin = math.Log(float64(xMin));
    var logMax = math.Log(float64(xMax));
    const nint samples = 100;

    var plot = make_slice<slice<double>>(samples);
    for (nint i = 0; i < samples; i++) {
        var window = time.Duration(math.Exp(float64(i) / (samples - 1) * (logMax - logMin) + logMin));
        if (quantiles == null) {
            plot[i] = make_slice<double>(2);
            plot[i][1] = mmuCurve.MMU(window);
        }
        else
 {
            plot[i] = make_slice<double>(1 + len(quantiles));
            copy(plot[i][(int)1..], mmuCurve.MUD(window, quantiles));
        }
        plot[i][0] = float64(window);
    } 

    // Create JSON response.
    err = json.NewEncoder(w).Encode();
    if (err != null) {
        log.Printf("failed to serialize response: %v", err);
        return ;
    }
}

private static @string templMMU = "<!doctype html>\n<html>\n  <head>\n    <meta charset=\"utf-8\">\n    <script type=\"text" +
    "/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>\n    <scrip" +
    "t type=\"text/javascript\" src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.2.1" +
    "/jquery.min.js\"></script>\n    <script type=\"text/javascript\">\n      google.chart" +
    "s.load(\'current\', {\'packages\':[\'corechart\']});\n      var chartsReady = false;\n  " +
    "    google.charts.setOnLoadCallback(function() { chartsReady = true; refreshChar" +
    "t(); });\n\n      var chart;\n      var curve;\n\n      function niceDuration(ns) {\n " +
    "         if (ns < 1e3) { return ns + \'ns\'; }\n          else if (ns < 1e6) { retu" +
    "rn ns / 1e3 + \'µs\'; }\n          else if (ns < 1e9) { return ns / 1e6 + \'ms\'; }\n " +
    "         else { return ns / 1e9 + \'s\'; }\n      }\n\n      function niceQuantile(q)" +
    " {\n        return \'p\' + q*100;\n      }\n\n      function mmuFlags() {\n        var " +
    "flags = \"\";\n        $(\"#options input\").each(function(i, elt) {\n          if (el" +
    "t.checked)\n            flags += \"|\" + elt.id;\n        });\n        return flags.s" +
    "ubstr(1);\n      }\n\n      function refreshChart() {\n        if (!chartsReady) ret" +
    "urn;\n        var container = $(\'#mmu_chart\');\n        container.css(\'opacity\', \'" +
    ".5\');\n        refreshChart.count++;\n        var seq = refreshChart.count;\n      " +
    "  $.getJSON(\'/mmuPlot?flags=\' + mmuFlags())\n         .fail(function(xhr, status," +
    " error) {\n           alert(\'failed to load plot: \' + status);\n         })\n      " +
    "   .done(function(result) {\n           if (refreshChart.count === seq)\n         " +
    "    drawChart(result);\n         });\n      }\n      refreshChart.count = 0;\n\n     " +
    " function drawChart(plotData) {\n        curve = plotData.curve;\n        var data" +
    " = new google.visualization.DataTable();\n        data.addColumn(\'number\', \'Windo" +
    "w duration\');\n        data.addColumn(\'number\', \'Minimum mutator utilization\');\n " +
    "       if (plotData.quantiles) {\n          for (var i = 1; i < plotData.quantile" +
    "s.length; i++) {\n            data.addColumn(\'number\', niceQuantile(1 - plotData." +
    "quantiles[i]) + \' MU\');\n          }\n        }\n        data.addRows(curve);\n     " +
    "   for (var i = 0; i < curve.length; i++) {\n          data.setFormattedValue(i, " +
    "0, niceDuration(curve[i][0]));\n        }\n\n        var options = {\n          char" +
    "t: {\n            title: \'Minimum mutator utilization\',\n          },\n          hA" +
    "xis: {\n            title: \'Window duration\',\n            scaleType: \'log\',\n     " +
    "       ticks: [],\n          },\n          vAxis: {\n            title: \'Minimum mu" +
    "tator utilization\',\n            minValue: 0.0,\n            maxValue: 1.0,\n      " +
    "    },\n          legend: { position: \'none\' },\n          focusTarget: \'category\'" +
    ",\n          width: 900,\n          height: 500,\n          chartArea: { width: \'80" +
    "%\', height: \'80%\' },\n        };\n        for (var v = plotData.xMin; v <= plotDat" +
    "a.xMax; v *= 10) {\n          options.hAxis.ticks.push({v:v, f:niceDuration(v)});" +
    "\n        }\n        if (plotData.quantiles) {\n          options.vAxis.title = \'Mu" +
    "tator utilization\';\n          options.legend.position = \'in\';\n        }\n\n       " +
    " var container = $(\'#mmu_chart\');\n        container.empty();\n        container.c" +
    "ss(\'opacity\', \'\');\n        chart = new google.visualization.LineChart(container[" +
    "0]);\n        chart = new google.visualization.LineChart(document.getElementById(" +
    "\'mmu_chart\'));\n        chart.draw(data, options);\n\n        google.visualization." +
    "events.addListener(chart, \'select\', selectHandler);\n        $(\'#details\').empty(" +
    ");\n      }\n\n      function selectHandler() {\n        var items = chart.getSelect" +
    "ion();\n        if (items.length === 0) {\n          return;\n        }\n        var" +
    " details = $(\'#details\');\n        details.empty();\n        var windowNS = curve[" +
    "items[0].row][0];\n        var url = \'/mmuDetails?window=\' + windowNS + \'&flags=\'" +
    " + mmuFlags();\n        $.getJSON(url)\n         .fail(function(xhr, status, error" +
    ") {\n            details.text(status + \': \' + url + \' could not be loaded\');\n    " +
    "     })\n         .done(function(worst) {\n            details.text(\'Lowest mutato" +
    "r utilization in \' + niceDuration(windowNS) + \' windows:\');\n            for (var" +
    " i = 0; i < worst.length; i++) {\n              details.append($(\'<br>\'));\n      " +
    "        var text = worst[i].MutatorUtil.toFixed(3) + \' at time \' + niceDuration(" +
    "worst[i].Time);\n              details.append($(\'<a/>\').text(text).attr(\'href\', w" +
    "orst[i].URL));\n            }\n         });\n      }\n\n      $.when($.ready).then(fu" +
    "nction() {\n        $(\"#options input\").click(refreshChart);\n      });\n    </scri" +
    "pt>\n    <style>\n      .help {\n        display: inline-block;\n        position: r" +
    "elative;\n        width: 1em;\n        height: 1em;\n        border-radius: 50%;\n  " +
    "      color: #fff;\n        background: #555;\n        text-align: center;\n       " +
    " cursor: help;\n      }\n      .help > span {\n        display: none;\n      }\n     " +
    " .help:hover > span {\n        display: block;\n        position: absolute;\n      " +
    "  left: 1.1em;\n        top: 1.1em;\n        background: #555;\n        text-align:" +
    " left;\n        width: 20em;\n        padding: 0.5em;\n        border-radius: 0.5em" +
    ";\n        z-index: 5;\n      }\n    </style>\n  </head>\n  <body>\n    <div style=\"po" +
    "sition: relative\">\n      <div id=\"mmu_chart\" style=\"width: 900px; height: 500px;" +
    " display: inline-block; vertical-align: top\">Loading plot...</div>\n      <div id" +
    "=\"options\" style=\"display: inline-block; vertical-align: top\">\n        <p>\n     " +
    "     <b>View</b><br>\n          <input type=\"radio\" name=\"view\" id=\"system\" check" +
    "ed><label for=\"system\">System</label>\n          <span class=\"help\">?<span>Consid" +
    "er whole system utilization. For example, if one of four procs is available to t" +
    "he mutator, mutator utilization will be 0.25. This is the standard definition of" +
    " an MMU.</span></span><br>\n          <input type=\"radio\" name=\"view\" id=\"perProc" +
    "\"><label for=\"perProc\">Per-goroutine</label>\n          <span class=\"help\">?<span" +
    ">Consider per-goroutine utilization. When even one goroutine is interrupted by G" +
    "C, mutator utilization is 0.</span></span><br>\n        </p>\n        <p>\n        " +
    "  <b>Include</b><br>\n          <input type=\"checkbox\" id=\"stw\" checked><label fo" +
    "r=\"stw\">STW</label>\n          <span class=\"help\">?<span>Stop-the-world stops all" +
    " goroutines simultaneously.</span></span><br>\n          <input type=\"checkbox\" i" +
    "d=\"background\" checked><label for=\"background\">Background workers</label>\n      " +
    "    <span class=\"help\">?<span>Background workers are GC-specific goroutines. 25%" +
    " of the CPU is dedicated to background workers during GC.</span></span><br>\n    " +
    "      <input type=\"checkbox\" id=\"assist\" checked><label for=\"assist\">Mark assist" +
    "</label>\n          <span class=\"help\">?<span>Mark assists are performed by alloc" +
    "ation to prevent the mutator from outpacing GC.</span></span><br>\n          <inp" +
    "ut type=\"checkbox\" id=\"sweep\"><label for=\"sweep\">Sweep</label>\n          <span c" +
    "lass=\"help\">?<span>Sweep reclaims unused memory between GCs. (Enabling this may " +
    "be very slow.).</span></span><br>\n        </p>\n        <p>\n          <b>Display<" +
    "/b><br>\n          <input type=\"checkbox\" id=\"mut\"><label for=\"mut\">Show percenti" +
    "les</label>\n          <span class=\"help\">?<span>Display percentile mutator utili" +
    "zation in addition to minimum. E.g., p99 MU drops the worst 1% of windows.</span" +
    "></span><br>\n        </p>\n      </div>\n    </div>\n    <div id=\"details\">Select a" +
    " point for details.</div>\n  </body>\n</html>\n";

// httpMMUDetails serves details of an MMU graph at a particular window.
private static void httpMMUDetails(http.ResponseWriter w, ptr<http.Request> _addr_r) {
    ref http.Request r = ref _addr_r.val;

    var (_, mmuCurve, err) = getMMUCurve(_addr_r);
    if (err != null) {
        http.Error(w, fmt.Sprintf("failed to parse events: %v", err), http.StatusInternalServerError);
        return ;
    }
    var windowStr = r.FormValue("window");
    var (window, err) = strconv.ParseUint(windowStr, 10, 64);
    if (err != null) {
        http.Error(w, fmt.Sprintf("failed to parse window parameter %q: %v", windowStr, err), http.StatusBadRequest);
        return ;
    }
    var worst = mmuCurve.Examples(time.Duration(window), 10); 

    // Construct a link for each window.
    slice<linkedUtilWindow> links = default;
    foreach (var (_, ui) in worst) {
        links = append(links, newLinkedUtilWindow(ui, time.Duration(window)));
    }    err = json.NewEncoder(w).Encode(links);
    if (err != null) {
        log.Printf("failed to serialize trace: %v", err);
        return ;
    }
}

private partial struct linkedUtilWindow {
    public ref trace.UtilWindow UtilWindow => ref UtilWindow_val;
    public @string URL;
}

private static linkedUtilWindow newLinkedUtilWindow(trace.UtilWindow ui, time.Duration window) { 
    // Find the range containing this window.
    Range r = default;
    foreach (var (_, __r) in ranges) {
        r = __r;
        if (r.EndTime > ui.Time) {
            break;
        }
    }
    return new linkedUtilWindow(ui,fmt.Sprintf("%s#%v:%v",r.URL(),float64(ui.Time)/1e6,float64(ui.Time+int64(window))/1e6));
}

} // end main_package

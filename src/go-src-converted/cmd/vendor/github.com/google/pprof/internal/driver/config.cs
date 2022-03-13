// package driver -- go2cs converted at 2022 March 13 06:36:27 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\config.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using fmt = fmt_package;
using url = net.url_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = sync_package;


// config holds settings for a single named config.
// The JSON tag name for a field is used both for JSON encoding and as
// a named variable.

using System.ComponentModel;
public static partial class driver_package {

private partial struct config {
    [Description("json:\"-\"")]
    public @string Output; // Display options.
    [Description("json:\"call_tree,omitempty\"")]
    public bool CallTree;
    [Description("json:\"relative_percentages,omitempty\"")]
    public bool RelativePercentages;
    [Description("json:\"unit,omitempty\"")]
    public @string Unit;
    [Description("json:\"compact_labels,omitempty\"")]
    public bool CompactLabels;
    [Description("json:\"-\"")]
    public @string SourcePath;
    [Description("json:\"-\"")]
    public @string TrimPath;
    [Description("json:\"intel_syntax,omitempty\"")]
    public bool IntelSyntax;
    [Description("json:\"mean,omitempty\"")]
    public bool Mean;
    [Description("json:\"-\"")]
    public @string SampleIndex;
    [Description("json:\"-\"")]
    public double DivideBy;
    [Description("json:\"normalize,omitempty\"")]
    public bool Normalize;
    [Description("json:\"sort,omitempty\"")]
    public @string Sort; // Filtering options
    [Description("json:\"drop_negative,omitempty\"")]
    public bool DropNegative;
    [Description("json:\"nodecount,omitempty\"")]
    public nint NodeCount;
    [Description("json:\"nodefraction,omitempty\"")]
    public double NodeFraction;
    [Description("json:\"edgefraction,omitempty\"")]
    public double EdgeFraction;
    [Description("json:\"trim,omitempty\"")]
    public bool Trim;
    [Description("json:\"focus,omitempty\"")]
    public @string Focus;
    [Description("json:\"ignore,omitempty\"")]
    public @string Ignore;
    [Description("json:\"prune_from,omitempty\"")]
    public @string PruneFrom;
    [Description("json:\"hide,omitempty\"")]
    public @string Hide;
    [Description("json:\"show,omitempty\"")]
    public @string Show;
    [Description("json:\"show_from,omitempty\"")]
    public @string ShowFrom;
    [Description("json:\"tagfocus,omitempty\"")]
    public @string TagFocus;
    [Description("json:\"tagignore,omitempty\"")]
    public @string TagIgnore;
    [Description("json:\"tagshow,omitempty\"")]
    public @string TagShow;
    [Description("json:\"taghide,omitempty\"")]
    public @string TagHide;
    [Description("json:\"noinlines,omitempty\"")]
    public bool NoInlines; // Output granularity
    [Description("json:\"granularity,omitempty\"")]
    public @string Granularity;
}

// defaultConfig returns the default configuration values; it is unaffected by
// flags and interactive assignments.
private static config defaultConfig() {
    return new config(Unit:"minimum",NodeCount:-1,NodeFraction:0.005,EdgeFraction:0.001,Trim:true,DivideBy:1.0,Sort:"flat",Granularity:"functions",);
}

// currentConfig holds the current configuration values; it is affected by
// flags and interactive assignments.
private static var currentCfg = defaultConfig();
private static sync.Mutex currentMu = default;

private static config currentConfig() => func((defer, _, _) => {
    currentMu.Lock();
    defer(currentMu.Unlock());
    return currentCfg;
});

private static void setCurrentConfig(config cfg) => func((defer, _, _) => {
    currentMu.Lock();
    defer(currentMu.Unlock());
    currentCfg = cfg;
});

// configField contains metadata for a single configuration field.
private partial struct configField {
    public @string name; // JSON field name/key in variables
    public @string urlparam; // URL parameter name
    public bool saved; // Is field saved in settings?
    public reflect.StructField field; // Field in config
    public slice<@string> choices; // Name Of variables in group
    public @string defaultValue; // Default value for this field.
}

private static slice<configField> configFields = default;private static map<@string, configField> configFieldMap = default;

private static void init() { 
    // Config names for fields that are not saved in settings and therefore
    // do not have a JSON name.
    map notSaved = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"SampleIndex":"sample_index","Output":"output","SourcePath":"source_path","TrimPath":"trim_path","DivideBy":"divide_by",}; 

    // choices holds the list of allowed values for config fields that can
    // take on one of a bounded set of values.
    map choices = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, slice<@string>>{"sort":{"cum","flat"},"granularity":{"functions","filefunctions","files","lines","addresses"},}; 

    // urlparam holds the mapping from a config field name to the URL
    // parameter used to hold that config field. If no entry is present for
    // a name, the corresponding field is not saved in URLs.
    map urlparam = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{"drop_negative":"dropneg","call_tree":"calltree","relative_percentages":"rel","unit":"unit","compact_labels":"compact","intel_syntax":"intel","nodecount":"n","nodefraction":"nf","edgefraction":"ef","trim":"trim","focus":"f","ignore":"i","prune_from":"prunefrom","hide":"h","show":"s","show_from":"sf","tagfocus":"tf","tagignore":"ti","tagshow":"ts","taghide":"th","mean":"mean","sample_index":"si","normalize":"norm","sort":"sort","granularity":"g","noinlines":"noinlines",};

    var def = defaultConfig();
    configFieldMap = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, configField>{};
    var t = reflect.TypeOf(new config());
    for (nint i = 0;
    var n = t.NumField(); i < n; i++) {
        var field = t.Field(i);
        var js = strings.Split(field.Tag.Get("json"), ",");
        if (len(js) == 0) {
            continue;
        }
        var name = js[0];
        if (name == "-") {
            name = notSaved[field.Name];
            if (name == "") { 
                // Not a configurable field.
                continue;
            }
        }
        configField f = new configField(name:name,urlparam:urlparam[name],saved:(name==js[0]),field:field,choices:choices[name],);
        f.defaultValue = def.get(f);
        configFields = append(configFields, f);
        configFieldMap[f.name] = f;
        foreach (var (_, choice) in f.choices) {
            configFieldMap[choice] = f;
        }
    }
}

// fieldPtr returns a pointer to the field identified by f in *cfg.
private static void fieldPtr(this ptr<config> _addr_cfg, configField f) {
    ref config cfg = ref _addr_cfg.val;
 
    // reflect.ValueOf: converts to reflect.Value
    // Elem: dereferences cfg to make *cfg
    // FieldByIndex: fetches the field
    // Addr: takes address of field
    // Interface: converts back from reflect.Value to a regular value
    return reflect.ValueOf(cfg).Elem().FieldByIndex(f.field.Index).Addr().Interface();
}

// get returns the value of field f in cfg.
private static @string get(this ptr<config> _addr_cfg, configField f) => func((_, panic, _) => {
    ref config cfg = ref _addr_cfg.val;

    switch (cfg.fieldPtr(f).type()) {
        case ptr<@string> ptr:
            return ptr.val;
            break;
        case ptr<nint> ptr:
            return fmt.Sprint(ptr.val);
            break;
        case ptr<double> ptr:
            return fmt.Sprint(ptr.val);
            break;
        case ptr<bool> ptr:
            return fmt.Sprint(ptr.val);
            break;
    }
    panic(fmt.Sprintf("unsupported config field type %v", f.field.Type));
});

// set sets the value of field f in cfg to value.
private static error set(this ptr<config> _addr_cfg, configField f, @string value) => func((_, panic, _) => {
    ref config cfg = ref _addr_cfg.val;

    switch (cfg.fieldPtr(f).type()) {
        case ptr<@string> ptr:
            if (len(f.choices) > 0) { 
                // Verify that value is one of the allowed choices.
                foreach (var (_, choice) in f.choices) {
                    if (choice == value) {
                        ptr.val = value;
                        return error.As(null!)!;
                    }
                }
                return error.As(fmt.Errorf("invalid %q value %q", f.name, value))!;
            }
            ptr.val = value;
            break;
        case ptr<nint> ptr:
            var (v, err) = strconv.Atoi(value);
            if (err != null) {
                return error.As(err)!;
            }
            ptr.val = v;
            break;
        case ptr<double> ptr:
            (v, err) = strconv.ParseFloat(value, 64);
            if (err != null) {
                return error.As(err)!;
            }
            ptr.val = v;
            break;
        case ptr<bool> ptr:
            (v, err) = stringToBool(value);
            if (err != null) {
                return error.As(err)!;
            }
            ptr.val = v;
            break;
        default:
        {
            var ptr = cfg.fieldPtr(f).type();
            panic(fmt.Sprintf("unsupported config field type %v", f.field.Type));
            break;
        }
    }
    return error.As(null!)!;
});

// isConfigurable returns true if name is either the name of a config field, or
// a valid value for a multi-choice config field.
private static bool isConfigurable(@string name) {
    var (_, ok) = configFieldMap[name];
    return ok;
}

// isBoolConfig returns true if name is either name of a boolean config field,
// or a valid value for a multi-choice config field.
private static bool isBoolConfig(@string name) {
    var (f, ok) = configFieldMap[name];
    if (!ok) {
        return false;
    }
    if (name != f.name) {
        return true; // name must be one possible value for the field
    }
    config cfg = default;
    _, ok = cfg.fieldPtr(f)._<ptr<bool>>();
    return ok;
}

// completeConfig returns the list of configurable names starting with prefix.
private static slice<@string> completeConfig(@string prefix) {
    slice<@string> result = default;
    foreach (var (v) in configFieldMap) {
        if (strings.HasPrefix(v, prefix)) {
            result = append(result, v);
        }
    }    return result;
}

// configure stores the name=value mapping into the current config, correctly
// handling the case when name identifies a particular choice in a field.
private static error configure(@string name, @string value) => func((defer, _, _) => {
    currentMu.Lock();
    defer(currentMu.Unlock());
    var (f, ok) = configFieldMap[name];
    if (!ok) {
        return error.As(fmt.Errorf("unknown config field %q", name))!;
    }
    if (f.name == name) {
        return error.As(currentCfg.set(f, value))!;
    }
    {
        var (v, err) = strconv.ParseBool(value);

        if (v && err == null) {
            return error.As(currentCfg.set(f, name))!;
        }
    }
    return error.As(fmt.Errorf("unknown config field %q", name))!;
});

// resetTransient sets all transient fields in *cfg to their currently
// configured values.
private static void resetTransient(this ptr<config> _addr_cfg) {
    ref config cfg = ref _addr_cfg.val;

    var current = currentConfig();
    cfg.Output = current.Output;
    cfg.SourcePath = current.SourcePath;
    cfg.TrimPath = current.TrimPath;
    cfg.DivideBy = current.DivideBy;
    cfg.SampleIndex = current.SampleIndex;
}

// applyURL updates *cfg based on params.
private static error applyURL(this ptr<config> _addr_cfg, url.Values @params) {
    ref config cfg = ref _addr_cfg.val;

    foreach (var (_, f) in configFields) {
        @string value = default;
        if (f.urlparam != "") {
            value = @params.Get(f.urlparam);
        }
        if (value == "") {
            continue;
        }
        {
            var err = cfg.set(f, value);

            if (err != null) {
                return error.As(fmt.Errorf("error setting config field %s: %v", f.name, err))!;
            }

        }
    }    return error.As(null!)!;
}

// makeURL returns a URL based on initialURL that contains the config contents
// as parameters.  The second result is true iff a parameter value was changed.
private static (url.URL, bool) makeURL(this ptr<config> _addr_cfg, url.URL initialURL) {
    url.URL _p0 = default;
    bool _p0 = default;
    ref config cfg = ref _addr_cfg.val;

    var q = initialURL.Query();
    var changed = false;
    foreach (var (_, f) in configFields) {
        if (f.urlparam == "" || !f.saved) {
            continue;
        }
        var v = cfg.get(f);
        if (v == f.defaultValue) {
            v = ""; // URL for of default value is the empty string.
        }
        else if (f.field.Type.Kind() == reflect.Bool) { 
            // Shorten bool values to "f" or "t"
            v = v[..(int)1];
        }
        if (q.Get(f.urlparam) == v) {
            continue;
        }
        changed = true;
        if (v == "") {
            q.Del(f.urlparam);
        }
        else
 {
            q.Set(f.urlparam, v);
        }
    }    if (changed) {
        initialURL.RawQuery = q.Encode();
    }
    return (initialURL, changed);
}

} // end driver_package

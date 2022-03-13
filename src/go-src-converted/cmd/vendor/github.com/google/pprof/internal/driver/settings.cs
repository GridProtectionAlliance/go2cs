// package driver -- go2cs converted at 2022 March 13 06:36:35 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Program Files\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\settings.go
namespace go.cmd.vendor.github.com.google.pprof.@internal;

using json = encoding.json_package;
using fmt = fmt_package;
using ioutil = io.ioutil_package;
using url = net.url_package;
using os = os_package;
using filepath = path.filepath_package;


// settings holds pprof settings.

using System.ComponentModel;
using System;
public static partial class driver_package {

private partial struct settings {
    [Description("json:\"configs\"")]
    public slice<namedConfig> Configs;
}

// namedConfig associates a name with a config.
private partial struct namedConfig {
    [Description("json:\"name\"")]
    public @string Name;
    public ref config config => ref config_val;
}

// settingsFileName returns the name of the file where settings should be saved.
private static (@string, error) settingsFileName() {
    @string _p0 = default;
    error _p0 = default!;
 
    // Return "pprof/settings.json" under os.UserConfigDir().
    var (dir, err) = os.UserConfigDir();
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (filepath.Join(dir, "pprof", "settings.json"), error.As(null!)!);
}

// readSettings reads settings from fname.
private static (ptr<settings>, error) readSettings(@string fname) {
    ptr<settings> _p0 = default!;
    error _p0 = default!;

    var (data, err) = ioutil.ReadFile(fname);
    if (err != null) {
        if (os.IsNotExist(err)) {
            return (addr(new settings()), error.As(null!)!);
        }
        return (_addr_null!, error.As(fmt.Errorf("could not read settings: %w", err))!);
    }
    ptr<settings> settings = addr(new settings());
    {
        var err = json.Unmarshal(data, settings);

        if (err != null) {
            return (_addr_null!, error.As(fmt.Errorf("could not parse settings: %w", err))!);
        }
    }
    foreach (var (i) in settings.Configs) {
        settings.Configs[i].resetTransient();
    }    return (_addr_settings!, error.As(null!)!);
}

// writeSettings saves settings to fname.
private static error writeSettings(@string fname, ptr<settings> _addr_settings) {
    ref settings settings = ref _addr_settings.val;

    var (data, err) = json.MarshalIndent(settings, "", "  ");
    if (err != null) {
        return error.As(fmt.Errorf("could not encode settings: %w", err))!;
    }
    {
        var err__prev1 = err;

        var err = os.MkdirAll(filepath.Dir(fname), 0700);

        if (err != null) {
            return error.As(fmt.Errorf("failed to create settings directory: %w", err))!;
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = ioutil.WriteFile(fname, data, 0644);

        if (err != null) {
            return error.As(fmt.Errorf("failed to write settings: %w", err))!;
        }
        err = err__prev1;

    }
    return error.As(null!)!;
}

// configMenuEntry holds information for a single config menu entry.
private partial struct configMenuEntry {
    public @string Name;
    public @string URL;
    public bool Current; // Is this the currently selected config?
    public bool UserConfig; // Is this a user-provided config?
}

// configMenu returns a list of items to add to a menu in the web UI.
private static slice<configMenuEntry> configMenu(@string fname, url.URL url) { 
    // Start with system configs.
    namedConfig configs = new slice<namedConfig>(new namedConfig[] { {Name:"Default",config:defaultConfig()} });
    {
        var (settings, err) = readSettings(fname);

        if (err == null) { 
            // Add user configs.
            configs = append(configs, settings.Configs);
        }
    } 

    // Convert to menu entries.
    var result = make_slice<configMenuEntry>(len(configs));
    nint lastMatch = -1;
    foreach (var (i, cfg) in configs) {
        var (dst, changed) = cfg.config.makeURL(url);
        if (!changed) {
            lastMatch = i;
        }
        result[i] = new configMenuEntry(Name:cfg.Name,URL:dst.String(),UserConfig:(i!=0),);
    }    if (lastMatch >= 0) {
        result[lastMatch].Current = true;
    }
    return result;
}

// editSettings edits settings by applying fn to them.
private static error editSettings(@string fname, Func<ptr<settings>, error> fn) {
    var (settings, err) = readSettings(fname);
    if (err != null) {
        return error.As(err)!;
    }
    {
        var err = fn(settings);

        if (err != null) {
            return error.As(err)!;
        }
    }
    return error.As(writeSettings(fname, _addr_settings))!;
}

// setConfig saves the config specified in request to fname.
private static error setConfig(@string fname, url.URL request) {
    var q = request.Query();
    var name = q.Get("config");
    if (name == "") {
        return error.As(fmt.Errorf("invalid config name"))!;
    }
    var cfg = currentConfig();
    {
        var err = cfg.applyURL(q);

        if (err != null) {
            return error.As(err)!;
        }
    }
    return error.As(editSettings(fname, s => {
        foreach (var (i, c) in s.Configs) {
            if (c.Name == name) {
                s.Configs[i].config = cfg;
                return error.As(null!)!;
            }
        }        s.Configs = append(s.Configs, new namedConfig(Name:name,config:cfg));
        return error.As(null!)!;
    }))!;
}

// removeConfig removes config from fname.
private static error removeConfig(@string fname, @string config) {
    return error.As(editSettings(fname, s => {
        foreach (var (i, c) in s.Configs) {
            if (c.Name == config) {
                s.Configs = append(s.Configs[..(int)i], s.Configs[(int)i + 1..]);
                return error.As(null!)!;
            }
        }        return error.As(fmt.Errorf("config %s not found", config))!;
    }))!;
}

} // end driver_package

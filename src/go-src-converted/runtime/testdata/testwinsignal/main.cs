// package main -- go2cs converted at 2022 March 06 22:26:22 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testwinsignal\main.go
using fmt = go.fmt_package;
using os = go.os_package;
using signal = go.os.signal_package;
using time = go.time_package;

namespace go;

public static partial class main_package {

private static void Main() {
    var c = make_channel<os.Signal>(1);
    signal.Notify(c);

    fmt.Println("ready");
    var sig = c.Receive();

    time.Sleep(time.Second);
    fmt.Println(sig);
}

} // end main_package

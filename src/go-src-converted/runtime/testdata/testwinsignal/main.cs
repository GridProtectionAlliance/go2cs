// package main -- go2cs converted at 2022 March 13 05:40:28 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testwinsignal\main.go
namespace go;

using fmt = fmt_package;
using os = os_package;
using signal = os.signal_package;
using time = time_package;

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

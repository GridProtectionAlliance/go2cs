/*
package main

import "fmt"

type IPAddr [4]byte

func (ip IPAddr) String() string {
    return fmt.Sprintf("%v.%v.%v.%v", ip[0], ip[1], ip[2], ip[3])
}

func main() {
    hosts := map[string]IPAddr{
        "loopback":  {127, 0, 0, 1},
        "googleDNS": {8, 8, 8, 8},
    }
    for name, ip := range hosts {
        fmt.Printf("%v: %v\n", name, ip)
    }
}
*/
#region source
using go;
using fmt = go.fmt_package;
using IPAddr = go.array<byte>;

static class main_package
{
    static @string String(this in IPAddr ip) {
        return fmt.Sprintf("{0}.{1}.{2}.{3}", ip[0], ip[1], ip[2], ip[3]);
    }

    static void Main() {
        var hosts  = new map<@string, IPAddr> {
            ["loopback"] = new[] { (byte)127, (byte)0, (byte)0, (byte)1 },
            ["googleDNS"] = new[] { (byte)8, (byte)8, (byte)8, (byte)8 }
        };

        foreach (var (name, ip) in hosts) {
            fmt.Printf("{0}: {1}\n", name, ip);
        }
    }
}
#endregion
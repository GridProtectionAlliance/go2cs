/*
package main

import (
    "fmt"
    "sync"
    "time"
)

// SafeCounter is safe to use concurrently.
type SafeCounter struct {
    mu sync.Mutex
    v  map[string]int
}

// Inc increments the counter for the given key.
func (c *SafeCounter) Inc(key string) {
    c.mu.Lock()
    // Lock so only one goroutine at a time can access the map c.v.
    c.v[key]++
    c.mu.Unlock()
}

// Value returns the current value of the counter for the given key.
func (c *SafeCounter) Value(key string) int {
    c.mu.Lock()
    // Lock so only one goroutine at a time can access the map c.v.
    defer c.mu.Unlock()
    return c.v[key]
}

func main() {
    c := SafeCounter{v: make(map[string]int)}
    for i := 0; i < 1000; i++ {
        go c.Inc("somekey")
    }

    time.Sleep(time.Second)
    fmt.Println(c.Value("somekey"))
}

*/
#region source
using go;
using fmt = go.fmt_package;
using time = go.time_package;
using sync = go.sync_package;
using static go.builtin;

static partial class main_package
{
    // SafeCounter is safe to use concurrently.
    public partial struct SafeCounter {
        public sync.Mutex mu;
        public map<@string, int> v;
    }

    // Inc increments the counter for the given key.
    public static void Inc(this ref SafeCounter c, @string key) {
        c.mu.Lock();
        // Lock so only one goroutine at a time can access the map c.v.
        c.v[key]++;
        c.mu.Unlock();
    }

    // Value returns the current value of the counter for the given key.
    public static int Value(this ref SafeCounter _this, @string key) => func(ref _this, (ref SafeCounter c, Defer defer, Panic panic, Recover recover) => {
        // 'c' escapes stack in defer below, so we need a pointer
        var c__ptr = ptr(c);

        c.mu.Lock();
        // Lock so only one goroutine at a time can access the map c.v.
        defer(() => {
            ref var c = ref c__ptr.Value;
            c.mu.Unlock();
        });
        return c.v[key];
    });

    static void Main() {
        // 'c' escapes stack in goroutine below, so we need a pointer
        ref var c = ref heap(new SafeCounter(v: make_map<@string, int>()), out var c__ptr).Value;

        for (var i = 0; i < 1000; i++) {
            go_(() => {
                ref var c = ref c__ptr.Value;
                c.Inc("somekey");
            });
        }

        time.Sleep(time.Second);
        fmt.Println(c.Value("somekey"));
    }
}
#endregion
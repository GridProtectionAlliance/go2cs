// Synthesized-delegate CHILD-package type-arg qualification guard (the go/importer gccgo
// importer field + net/http TLSNextProto shapes). A struct-field func type naming types from a
// slash-bearing child package used to stringify with the raw import path
// (`func(...*SynthesizedDelegateChildPkg/inner.Record...)`), and the string-path slash
// heuristics then mis-qualified the delegate's type-args (`ж<go.types.Package>`-style — no
// package class, wrong namespace binding — CS0234). The structural signature render must keep
// the file's short import alias (`inner.Record`), exactly like the sibling map field's
// type-args. Both the named-func-type field (via the methodless collapse) and the map-of-func
// field are exercised at runtime.
package main

import (
	"fmt"

	"SynthesizedDelegateChildPkg/inner"
)

// registry mirrors go/importer's gccgoimports struct (load) and net/http's Server.TLSNextProto
// (notify): synthesized Func<…>/Action<…> delegates whose type-args are child-package types.
type registry struct {
	cache  map[string]*inner.Record
	load   inner.Loader
	notify map[string]func(*inner.Record, string)
}

func main() {
	r := registry{
		cache: map[string]*inner.Record{},
		load: func(cache map[string]*inner.Record, name string, lookup func(string) (string, error)) (*inner.Record, error) {
			suffix, err := lookup(name)
			if err != nil {
				return nil, err
			}
			rec := &inner.Record{Name: name + suffix, Hits: 1}
			cache[name] = rec
			return rec, nil
		},
		notify: map[string]func(*inner.Record, string){
			"hit": func(rec *inner.Record, tag string) {
				rec.Hits++
				fmt.Println("notify:", tag, rec.Name, rec.Hits)
			},
		},
	}

	rec, err := r.load(r.cache, "alpha", func(s string) (string, error) {
		return "-" + s, nil
	})
	fmt.Println(rec.Name, rec.Hits, err)

	r.notify["hit"](rec, "t1")
	fmt.Println(len(r.cache), r.cache["alpha"].Hits)
}

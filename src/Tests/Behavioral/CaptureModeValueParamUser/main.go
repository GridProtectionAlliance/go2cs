// CaptureModeValueParamUser is the caller half of the cross-package guard for entry-time heap
// boxing of a value parameter — the go/format side of the format→printer shape. render() takes
// lib.Config BY VALUE and calls the transitively-direct-ж Fprint on it, exactly go/format's
// format(…, cfg printer.Config) calling cfg.Fprint(&buf, fset, file): Go auto-addresses the
// parameter, so the converter must box it at entry (cfgʗp → Ꮡcfg) for the FOREIGN ж<Config>
// extension to bind (CS1929 ×2 before the fix) and for the callee's writes through the
// receiver pointer to stay visible in the rest of the body. The trace accumulating across TWO
// calls is the write-visibility proof a call-site copy-box cannot pass; the caller's copy
// staying unchanged proves by-value parameter semantics were preserved.
package main

import (
	"fmt"

	"CaptureModeValueParamLib"
)

// render mirrors go/format's format(): a pre-call write the callee must observe (format's
// cfg.Indent = indent + indentAdj), two capture-mode calls on the value parameter, then reads
// that must see the callee's writes.
func render(cfg CaptureModeValueParamLib.Config, label string) (string, string) {
	cfg.Indent = cfg.Indent + 1
	s1, _ := cfg.Fprint(label)
	s2, _ := cfg.Fprint(label + "2")
	return s1 + "," + s2, cfg.Trace()
}

func main() {
	cfg := CaptureModeValueParamLib.Config{Indent: 3}
	out, trace := render(cfg, "go")
	fmt.Println("rendered:", out)
	fmt.Println("trace:", trace)
	fmt.Println("caller Indent unchanged:", cfg.Indent)
}

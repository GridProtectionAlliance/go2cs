// CaptureModeValueParamLib is the library half of the cross-package guard for entry-time
// heap boxing of a value parameter — the go/printer side of the go/format shape. Config
// mirrors printer.Config: a small value-semantics struct whose pointer-receiver Fprint is
// capture-mode (direct-ж). fprint is the defer/recover-wrapped primary (bodyWrappedInDeferContext
// promotes it to direct-ж), and Fprint inherits direct-ж transitively by calling fprint on its
// own receiver — exactly (*printer.Config).Fprint → (*printer.Config).fprint. A user package
// calling Fprint on a Config VALUE parameter must entry-time box it (cfgʗp → Ꮡcfg) for the
// cross-assembly ж<Config> extension to bind (CS1929 before the fix).
package CaptureModeValueParamLib

import "fmt"

// Config mirrors go/printer's Config: exported tuning field, unexported state the
// capture-mode methods mutate through the receiver pointer.
type Config struct {
	Indent int
	trace  string
}

// fprint mirrors (*printer.Config).fprint: wrapped in a defer/recover execution context that
// references the receiver — the capture-mode (direct-ж) primary.
func (cfg *Config) fprint(label string) (out string, err error) {
	defer func() {
		if e := recover(); e != nil {
			err = fmt.Errorf("panic: %v", e)
		}
	}()
	cfg.trace = fmt.Sprintf("%s|%s", cfg.trace, label)
	return fmt.Sprintf("%s@%d", label, cfg.Indent), nil
}

// Fprint mirrors (*printer.Config).Fprint: transitively direct-ж (calls fprint on its receiver).
func (cfg *Config) Fprint(label string) (string, error) {
	return cfg.fprint(label)
}

// Trace exposes the accumulated trace so the user package can PROVE the callee's writes
// through the receiver pointer were observed (write-visibility).
func (cfg *Config) Trace() string {
	return cfg.trace
}

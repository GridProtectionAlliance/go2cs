module SynthesizedDelegateCrossPkg

go 1.23

require CrossPkgFuncLib v0.0.0

require CrossPkgLib v0.0.0 // indirect

replace CrossPkgFuncLib => ../CrossPkgFuncLib

replace CrossPkgLib => ../CrossPkgLib

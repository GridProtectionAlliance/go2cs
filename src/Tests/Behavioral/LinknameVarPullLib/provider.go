// A provider package for the //go:linkname variable-pull behavioral test. `secret` is unexported but
// carries a definition-side one-argument //go:linkname handle (Go 1.23's opt-in that authorizes
// cross-package linkname pulls), so the converter emits it PUBLIC — letting the consumer package (a
// separate C# assembly) reach it through the forwarding property its two-argument pull generates.
package LinknameVarPullLib

import _ "unsafe" // for go:linkname

//go:linkname secret
var secret = "pulled across the linkname"

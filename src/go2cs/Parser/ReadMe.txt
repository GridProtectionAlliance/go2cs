The Golang.g4 Antlr4 grammar file, from https://github.com/antlr/grammars-v4/tree/master/golang, has been
modified from the original posted grammar.

Grammar synchronized, with some minor exceptions, up to December 6, 2021 - ongoing changes posted beyond this point by @nicoberline are "very" breaking to existing go2cs behavior. Although posted grammar _may_ be more accurate, it currently would require a major refactoring of go2cs conversion code to accommodate.

Grammar compared to posted version per Dec 2021 is slightly customized to match existing terminal visitors and existing conversion code behavior.

Deviations are marked as comments in grammar with `\\JRC:`.

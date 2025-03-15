package main

import (
	"bufio"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"io"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

// DirectiveType represents the type of Go directive
type DirectiveType string

const (
	// Common Go directives
	BuildDirective    DirectiveType = "build"
	GenerateDirective DirectiveType = "generate"
	LinkDirective     DirectiveType = "link"
	EmbedDirective    DirectiveType = "embed"
	// Add other directives as needed
)

// Directive represents a parsed Go directive
type Directive struct {
	Type    DirectiveType
	Content string
	Line    int
}

// DirectiveParser parses Go directives from source code
type DirectiveParser struct {
	directiveRegex *regexp.Regexp
}

// NewDirectiveParser creates a new directive parser
func NewDirectiveParser() *DirectiveParser {
	// Matches //go: followed by any text
	regex := regexp.MustCompile(`^\s*//go:(\w+)(.*)$`)
	return &DirectiveParser{
		directiveRegex: regex,
	}
}

// ParseReader parses directives from a reader
func (p *DirectiveParser) ParseReader(r io.Reader) ([]Directive, error) {
	scanner := bufio.NewScanner(r)
	lineNum := 0
	var directives []Directive

	for scanner.Scan() {
		lineNum++
		line := scanner.Text()

		matches := p.directiveRegex.FindStringSubmatch(line)
		if len(matches) == 3 {
			directiveType := DirectiveType(strings.ToLower(matches[1]))
			content := strings.TrimSpace(matches[2])

			directives = append(directives, Directive{
				Type:    directiveType,
				Content: content,
				Line:    lineNum,
			})
		}
	}

	if err := scanner.Err(); err != nil {
		return nil, fmt.Errorf("error scanning source: %w", err)
	}

	return directives, nil
}

// ParseString parses directives from a string
func (p *DirectiveParser) ParseString(src string) ([]Directive, error) {
	return p.ParseReader(strings.NewReader(src))
}

// FilterByType returns directives of a specific type
func FilterByType(directives []Directive, directiveType DirectiveType) []Directive {
	var filtered []Directive
	for _, d := range directives {
		if d.Type == directiveType {
			filtered = append(filtered, d)
		}
	}
	return filtered
}

func ParseDirectives(filename string) ([]Directive, error) {
	parser := NewDirectiveParser()

	file, err := os.Open(filename)

	if err != nil {
		return nil, fmt.Errorf("failed to open file %s: %w", filename, err)
	}

	defer file.Close()

	source, err := io.ReadAll(file)

	if err != nil {
		return nil, fmt.Errorf("failed to read file %s: %w", filename, err)
	}

	directives, err := parser.ParseString(string(source))

	if err != nil {
		return nil, err
	}

	return directives, nil
}

// BuildConstraintEvaluator evaluates build constraints against a set of allowed platforms
type BuildConstraintEvaluator struct {
	allowedPlatforms map[string]bool
}

// NewBuildConstraintEvaluator creates a new build constraint evaluator
func NewBuildConstraintEvaluator(platforms []string) *BuildConstraintEvaluator {
	allowedMap := make(map[string]bool)

	// Track Unix-like systems to handle the "unix" tag
	hasUnixSystem := false

	unixSystems := map[string]bool{
		"linux":     true,
		"freebsd":   true,
		"netbsd":    true,
		"openbsd":   true,
		"dragonfly": true,
		"solaris":   true,
		"illumos":   true,
		"darwin":    true,
		"ios":       true,
	}

	// Handle all platforms
	for _, p := range platforms {
		allowedMap[p] = true

		// Split into OS and arch parts
		parts := strings.Split(p, "/")

		if len(parts) == 2 {
			os := parts[0]
			arch := parts[1]

			allowedMap[os] = true   // OS
			allowedMap[arch] = true // Architecture

			// Check if this is a Unix-like system
			if unixSystems[os] {
				hasUnixSystem = true
			}

			// Handle special case for js
			if os == "js" && arch == "wasm" {
				allowedMap["js"] = true
			}
		}
	}

	// Set special categories based on included platforms
	if hasUnixSystem {
		allowedMap["unix"] = true
	}

	// Automatically set "posix" if we have any Unix system (simplified - true POSIX compliance is more complex)
	if hasUnixSystem {
		allowedMap["posix"] = true
	}

	// For cgo, we defer to explicit settings rather than auto-detecting

	return &BuildConstraintEvaluator{
		allowedPlatforms: allowedMap,
	}
}

// SetTag manually sets a build tag to a specific value
func (e *BuildConstraintEvaluator) SetTag(tag string, value bool) {
	e.allowedPlatforms[tag] = value
}

// SetCgo sets whether cgo is available
func (e *BuildConstraintEvaluator) SetCgo(enabled bool) {
	e.allowedPlatforms["cgo"] = enabled
}

// EvaluateConstraint evaluates if a build constraint matches allowed platforms
func (e *BuildConstraintEvaluator) EvaluateConstraint(constraint string) (bool, error) {
	// Convert build constraint to Go expression syntax
	expr := convertToGoSyntax(constraint)

	// Parse the expression
	node, err := parser.ParseExpr(expr)
	if err != nil {
		return false, fmt.Errorf("failed to parse build constraint: %w", err)
	}

	// Evaluate the expression
	return e.evaluateExpr(node), nil
}

// evaluateExpr recursively evaluates the AST expression
func (e *BuildConstraintEvaluator) evaluateExpr(expr ast.Expr) bool {
	switch node := expr.(type) {
	case *ast.BinaryExpr:
		// Handle binary operations (&&, ||)
		left := e.evaluateExpr(node.X)
		right := e.evaluateExpr(node.Y)

		switch node.Op {
		case token.LAND: // &&
			return left && right
		case token.LOR: // ||
			return left || right
		default:
			return false
		}

	case *ast.UnaryExpr:
		// Handle unary operations (!)
		if node.Op == token.NOT {
			return !e.evaluateExpr(node.X)
		}
		return false

	case *ast.Ident:
		// Check if the identifier (e.g., "linux", "darwin", "amd64") is allowed
		identifier := node.Name
		return e.allowedPlatforms[identifier]

	case *ast.ParenExpr:
		// Handle parenthesized expressions
		return e.evaluateExpr(node.X)

	default:
		return false
	}
}

// convertToGoSyntax converts a build constraint to Go expression syntax
func convertToGoSyntax(constraint string) string {
	// Trim any whitespace
	expr := strings.TrimSpace(constraint)

	// Normalize the expression to lowercase
	expr = strings.ToLower(expr)

	return expr
}

// IsBuildAllowed determines if a build directive allows building for the configured platforms
func (e *BuildConstraintEvaluator) IsBuildAllowed(buildDirective Directive) (bool, error) {
	if buildDirective.Type != BuildDirective {
		return false, fmt.Errorf("directive is not a build directive")
	}

	return e.EvaluateConstraint(buildDirective.Content)
}

func (e *BuildConstraintEvaluator) CheckDirectives(directives []Directive) (bool, error) {
	buildDirectives := FilterByType(directives, BuildDirective)

	// If no build directives, assume it's allowed
	if len(buildDirectives) == 0 {
		return true, nil
	}

	// Check each build directive
	for _, directive := range buildDirectives {
		allowed, err := e.IsBuildAllowed(directive)
		if err != nil {
			return false, err
		}

		if allowed {
			return true, nil
		}
	}

	// No matching build directive found
	return false, nil
}

// isFileNameCompatible checks if a Go source file should be included in a build
// based on its filename suffixes (e.g., _linux.go, _windows_amd64.go)
func isFileNameCompatible(filename string, evaluator *BuildConstraintEvaluator) bool {
	// Ignore non-Go files
	if !strings.HasSuffix(filename, ".go") {
		return false
	}

	// Extract the base name without extension
	base := strings.TrimSuffix(filepath.Base(filename), ".go")

	// If the file doesn't have an underscore, it's included in all builds
	if !strings.Contains(base, "_") {
		return true
	}

	// Check for test files which have different build rules
	isTest := strings.HasSuffix(base, "_test")
	if isTest {
		base = strings.TrimSuffix(base, "_test")
	}

	// Get the parts after the last non-empty element before any _test suffix
	parts := strings.Split(base, "_")

	// If we only have a single part or just a _test suffix, there's no build constraint
	if len(parts) <= 1 || (len(parts) == 2 && isTest && parts[1] == "") {
		return true
	}

	// Handle operating system constraints
	osConstraints := []string{}
	archConstraints := []string{}

	// Parse the file name parts to extract OS and arch constraints
	// Skip the first part as it's the base name
	for i := 1; i < len(parts); i++ {
		part := parts[i]
		if part == "" {
			continue
		}

		// Handle common OS and architecture identifiers
		// First check if it's an architecture
		switch part {
		case "386", "amd64", "arm", "arm64", "ppc64", "ppc64le", "mips", "mipsle",
			"mips64", "mips64le", "s390x", "wasm":
			archConstraints = append(archConstraints, part)
		// Then check if it's an OS
		case "linux", "darwin", "windows", "freebsd", "netbsd", "openbsd",
			"android", "ios", "js", "solaris", "plan9", "aix":
			osConstraints = append(osConstraints, part)
		// Handle less common constraints directly
		default:
			// Check if this constraint is allowed
			if !evaluator.allowedPlatforms[part] {
				return false
			}
		}
	}

	// If we have OS constraints, at least one must match
	if len(osConstraints) > 0 {
		osMatch := false
		for _, os := range osConstraints {
			if evaluator.allowedPlatforms[os] {
				osMatch = true
				break
			}
		}
		if !osMatch {
			return false
		}
	}

	// If we have architecture constraints, at least one must match
	if len(archConstraints) > 0 {
		archMatch := false
		for _, arch := range archConstraints {
			if evaluator.allowedPlatforms[arch] {
				archMatch = true
				break
			}
		}
		if !archMatch {
			return false
		}
	}

	return true
}

// Modified CheckBuildConstraints that handles both directives and filename constraints
func CheckBuildConstraints(filename string, targetPlatform string) (bool, error) {
	// Parse the source code to extract directives
	directives, err := ParseDirectives(filename)

	if err != nil {
		return false, fmt.Errorf("failed to parse directives: %w", err)
	}

	// Create a new build constraint evaluator
	evaluator := NewBuildConstraintEvaluator([]string{targetPlatform})

	// First, check if the filename itself indicates build constraints
	if !isFileNameCompatible(filename, evaluator) {
		return false, nil
	}

	// If no build directives exist, we already passed the filename check
	buildDirectives := FilterByType(directives, BuildDirective)
	if len(buildDirectives) == 0 {
		return true, nil
	}

	// Otherwise, check explicit build directives
	return evaluator.CheckDirectives(directives)
}

// containsManualConversionMarker checks if a file contains the GoManualConversion module marker
// that is not commented out. It supports both the standard form "[module: go.GoManualConversion]"
// and the C# attribute form "[module: go.GoManualConversionAttribute]", with or without
// the "go." namespace prefix. Function will exit early if a class definition is detected, as the
// marker should appear before class definitions.
func containsManualConversionMarker(filename string) (bool, error) {
	// Check if file exists
	if _, err := os.Stat(filename); os.IsNotExist(err) {
		return false, nil
	}

	file, err := os.Open(filename)

	if err != nil {
		return false, fmt.Errorf("error opening file: %w", err)
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	// Regex to check if a line starts with a comment
	commentStartRE := regexp.MustCompile(`^\s*//|^\s*/\*`)

	// Regex to match the module pattern with or without "go." namespace prefix
	// and with or without the "Attribute" suffix (for C# compatibility)
	modulePatternRE := regexp.MustCompile(`\[\s*module\s*:\s*(?:go\.)?\s*GoManualConversion(?:Attribute)?\s*\]`)

	// Regex to detect class definition
	// This looks for the word "class" surrounded by spaces/boundaries
	// followed by an identifier (which is a sequence of letters, digits, or underscores, starting with a letter)
	classDefinitionRE := regexp.MustCompile(`\bclass\s+\w+`)

	// Keep track of if we're in a multiline comment
	inMultilineComment := false

	for scanner.Scan() {
		line := scanner.Text()

		// Check for multiline comment boundaries
		if !inMultilineComment && strings.Contains(line, "/*") {
			inMultilineComment = true
		}

		if inMultilineComment {
			if strings.Contains(line, "*/") {
				inMultilineComment = false
				// The rest of the line after */ could contain relevant code
				// Extract the part after */
				parts := strings.Split(line, "*/")
				if len(parts) > 1 {
					line = parts[1]
				} else {
					continue
				}
			} else {
				continue
			}
		}

		// Skip single-line comments
		if commentStartRE.MatchString(line) {
			continue
		}

		// Check if the line contains the marker
		if modulePatternRE.MatchString(line) {
			return true, nil
		}

		// Check if the line contains a class definition - exit early if found
		// This is fine because valid module markers will come before class definitions
		if classDefinitionRE.MatchString(line) {
			return false, nil
		}
	}

	if err := scanner.Err(); err != nil {
		return false, fmt.Errorf("error reading file: %w", err)
	}

	return false, nil
}

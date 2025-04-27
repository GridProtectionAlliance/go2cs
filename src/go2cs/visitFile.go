package main

import (
	"fmt"
	"go/ast"
	"log"
	"sort"
	"strings"
)

const TypeAliasMarker = ">>MARKER:TYPEALIASES<<"
const UsingsMarker = ">>MARKER:USINGS<<"

func (v *Visitor) Visit(node ast.Node) ast.Visitor {
	if node != nil {
		if commentGroup, ok := node.(*ast.CommentGroup); ok {
			for _, comment := range commentGroup.List {
				delete(v.standAloneComments, comment.Slash)
			}
		}
	}

	return v
}

func (v *Visitor) visitFile(file *ast.File) {
	if v.options.includeComments {
		// Create standalone comments map
		for _, commentGroup := range file.Comments {
			for _, comment := range commentGroup.List {
				v.standAloneComments[comment.Slash] = comment.Text
			}
		}

		// Remove CommentGroup instances that exist as AST nodes from standalone comments map
		ast.Walk(v, file)

		for pos := range v.standAloneComments {
			v.sortedCommentPos = append(v.sortedCommentPos, pos)
		}

		sort.Slice(v.sortedCommentPos, func(i, j int) bool {
			return v.sortedCommentPos[i] < v.sortedCommentPos[j]
		})

		v.writeDoc(file.Doc, file.Package)
	}

	// Derive package name
	packageLock.Lock()

	if len(packageName) == 0 {
		packageName = file.Name.Name
	} else {
		if packageName != file.Name.Name {
			log.Fatalf("Multiple package names encountered: %s and %s", packageName, file.Name.Name)
		}
	}

	packageLock.Unlock()

	v.writeOutput(TypeAliasMarker)
	v.writeOutputLn("namespace %s;", packageNamespace)
	v.targetFile.WriteString(v.newline)

	packageClassName := getSanitizedImport(fmt.Sprintf("%s%s", packageName, PackageSuffix))

	v.writeOutput(UsingsMarker)
	v.writeOutputLn("partial class %s {", packageClassName)

	for _, decl := range file.Decls {
		v.visitDecl(decl)
	}

	if v.options.includeComments {
		// Add any remaining standalone comments
		postCodeComments := strings.Builder{}
		v.writeDocString(&postCodeComments, nil, file.FileEnd)
		v.targetFile.WriteString(v.newline)

		if postCodeComments.Len() > 0 {
			v.writeOutputLn(postCodeComments.String())
		} else {
			if v.needsNewLine(v.targetFile.String()) {
				v.targetFile.WriteString(v.newline)
			}
		}
	} else {
		if v.needsNewLine(v.targetFile.String()) {
			v.targetFile.WriteString(v.newline)
		}
	}

	v.writeOutputLn("} // end %s", packageClassName)

	targetFile := v.targetFile.String()

	// Ensure using ortder is consistent
	requiredUsings := v.requiredUsings.Keys()
	sort.Strings(requiredUsings)

	for _, requiredUsing := range requiredUsings {
		v.packageImports.WriteString(fmt.Sprintf("using %s;%s", requiredUsing, v.newline))
	}

	if v.packageImports.Len() > 0 {
		v.packageImports.WriteString(v.newline)
	}

	targetFile = strings.ReplaceAll(targetFile, UsingsMarker, v.packageImports.String())

	if v.typeAliasDeclarations.Len() > 0 {
		v.typeAliasDeclarations.WriteString(v.newline)
		targetFile = strings.ReplaceAll(targetFile, TypeAliasMarker, v.typeAliasDeclarations.String())
	} else {
		targetFile = strings.ReplaceAll(targetFile, TypeAliasMarker, "")
	}

	v.targetFile.Reset()
	v.targetFile.WriteString(targetFile)
}

func (v *Visitor) needsNewLine(text string) bool {
	newLineLen := len(v.newline)
	lastChars := text[len(text)-(newLineLen*2):]

	return lastChars != v.newline+v.newline
}

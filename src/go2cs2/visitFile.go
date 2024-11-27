package main

import (
	"fmt"
	"go/ast"
	"sort"
	"strings"
)

const UsingsMarker = ">>MARKER:USINGS<<"
const UnsafeMarker = ">>MARKER:UNSAFE<<"

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

	v.writeOutputLn("namespace %s;", RootNamespace)
	v.targetFile.WriteString(v.newline)

	v.writeOutputLn(UsingsMarker)
	v.writeOutputLn("public static %spartial class %s%s {", UnsafeMarker, file.Name.Name, PackageSuffix)

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
		}
	} else {
		v.targetFile.WriteString(v.newline)
	}

	v.writeOutputLn("} // end %s%s", file.Name.Name, PackageSuffix)

	targetFile := v.targetFile.String()

	for requiredUsing := range v.requiredUsings {
		v.packageImports.WriteString(fmt.Sprintf("using %s;%s", requiredUsing, v.newline))
	}

	targetFile = strings.ReplaceAll(targetFile, UsingsMarker, v.packageImports.String())

	if v.usesUnsafeCode {
		targetFile = strings.ReplaceAll(targetFile, UnsafeMarker, "unsafe ")
	} else {
		targetFile = strings.ReplaceAll(targetFile, UnsafeMarker, "")
	}

	v.targetFile.Reset()
	v.targetFile.WriteString(targetFile)
}

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

func (v *Visitor) enterFile(x *ast.File) {
	// Create standalone comments map
	for _, commentGroup := range x.Comments {
		for _, comment := range commentGroup.List {
			v.standAloneComments[comment.Slash] = comment.Text
		}
	}

	// Remove CommentGroup instances that exist as AST nodes from standalone comments map
	ast.Walk(v, x)

	for pos, _ := range v.standAloneComments {
		v.sortedCommentPos = append(v.sortedCommentPos, pos)
	}

	sort.Slice(v.sortedCommentPos, func(i, j int) bool {
		return v.sortedCommentPos[i] < v.sortedCommentPos[j]
	})

	v.writeDoc(x.Doc, x.Package)
	v.writeOutputLn("namespace %s;", RootNamespace)
	v.targetFile.WriteString(v.newline)

	v.writeOutputLn(UsingsMarker)
	v.writeOutputLn("public static %spartial class %s%s {", UnsafeMarker, x.Name.Name, ClassSuffix)

	for _, decl := range x.Decls {
		v.visitDecl(decl)
	}
}

func (v *Visitor) exitFile(x *ast.File) {
	// Add any remaining standalone comments
	postCodeComments := strings.Builder{}
	v.writeDocString(&postCodeComments, nil, x.FileEnd)
	v.targetFile.WriteString(v.newline)

	if postCodeComments.Len() > 0 {
		v.writeOutputLn(postCodeComments.String())
	}

	v.writeOutputLn("} // end %s%s", x.Name.Name, ClassSuffix)

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

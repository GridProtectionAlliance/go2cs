package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"strings"
)

func (v *Visitor) indent(indentLevel int) string {
	return strings.Repeat(" ", v.options.indentSpaces*indentLevel)
}

func (v *Visitor) isLineFeedBetween(prevEndPos, currPos token.Pos) bool {
	prevLine := v.fset.Position(prevEndPos).Line
	currLine := v.fset.Position(currPos).Line
	return currLine > prevLine
}

func (v *Visitor) writeString(builder *strings.Builder, format string, a ...interface{}) {
	if v.indentLevel > 0 {
		builder.WriteString(v.indent(v.indentLevel))
	}

	builder.WriteString(fmt.Sprintf(format, a...))
}

func (v *Visitor) writeStringLn(builder *strings.Builder, format string, a ...interface{}) {
	v.writeString(builder, format, a...)
	builder.WriteString(v.newline)
}

// TODO: Come back to this later, you could spend a lot of time here - there is an ongoing effort to improve AST for
// parsing free floating comments: https://github.com/golang/go/issues/20744 - plus a DST package as a fallback
func (v *Visitor) writeStandAloneCommentString(builder *strings.Builder, targetPos token.Pos, doc *ast.CommentGroup, prefix string) (bool, int) {
	wroteStandAloneComment := false
	lines := 0

	if !v.options.includeComments {
		return wroteStandAloneComment, lines
	}

	// Handle standalone comments that may precede the target position
	if targetPos != token.NoPos {
		if v.file == nil {
			v.file = v.fset.File(targetPos)
		}

		handledPos := []token.Pos{}

		for _, pos := range v.sortedCommentPos {
			if pos > targetPos {
				break
			}

			comment, found := v.standAloneComments[pos]

			if !found {
				continue
			}

			comment = strings.ReplaceAll(comment, "\n", v.newline)

			if !strings.HasSuffix(comment, v.newline) {
				comment += v.newline
			}

			lines += strings.Count(comment, "\n")

			builder.WriteString(prefix)
			builder.WriteString(comment)

			delete(v.standAloneComments, pos)
			handledPos = append(handledPos, pos)
			wroteStandAloneComment = true
		}

		if len(handledPos) > 0 {
			lastCommentPos := handledPos[len(handledPos)-1]

			// Add line breaks if there is a gap between the last comment and the target position
			if lastCommentPos > token.NoPos && doc != nil {
				docPos := doc.Pos()
				targetLine := v.file.Line(lastCommentPos)
				nodeLine := v.file.Line(docPos)

				if int(nodeLine-targetLine)-1 > 0 {
					builder.WriteString(v.newline)
				}
			}

			removePos := func(slice []token.Pos, pos token.Pos) []token.Pos {
				for i, v := range slice {
					if v == pos {
						return append(slice[:i], slice[i+1:]...)
					}
				}
				return slice
			}

			// Remove handled positions from sorted list
			for _, pos := range handledPos {
				v.sortedCommentPos = removePos(v.sortedCommentPos, pos)
			}
		}
	}

	return wroteStandAloneComment, lines
}

func (v *Visitor) writeDocString(builder *strings.Builder, doc *ast.CommentGroup, targetPos token.Pos) {
	if !v.options.includeComments {
		return
	}

	/*wroteStandAloneComment_, _ :=*/
	v.writeStandAloneCommentString(builder, targetPos, doc, "")

	if doc == nil {
		return
	}

	// Handle doc comments
	v.writeString(builder, "") // Write indent
	v.writeCommentString(builder, doc, token.NoPos)
	builder.WriteString(v.newline)
}

func (v *Visitor) writeCommentString(builder *strings.Builder, comment *ast.CommentGroup, targetPos token.Pos) {
	if comment == nil || !v.options.includeComments {
		return
	}

	if !v.processedComments.Add(comment.Pos()) {
		return
	}

	for index, comment := range comment.List {
		if index > 0 {
			builder.WriteString(v.newline)
		}

		if targetPos > token.NoPos {
			padding := int(comment.Slash - targetPos)

			if padding < 1 {
				padding = 1
			}

			builder.WriteString(strings.Repeat(" ", padding))
		}

		builder.WriteString(comment.Text)
	}
}

func (v *Visitor) replaceMarkerString(builder *strings.Builder, marker string, replacement string) {
	builderString := strings.ReplaceAll(builder.String(), marker, replacement)
	builder.Reset()
	builder.WriteString(builderString)
}

func (v *Visitor) writeOutput(format string, a ...interface{}) {
	v.writeString(v.targetFile, format, a...)
}

func (v *Visitor) writeOutputLn(format string, a ...interface{}) {
	v.writeStringLn(v.targetFile, format, a...)
}

func (v *Visitor) writeDoc(doc *ast.CommentGroup, targetPos token.Pos) {
	v.writeDocString(v.targetFile, doc, targetPos)
}

func (v *Visitor) writeComment(comment *ast.CommentGroup, targetPos token.Pos) {
	v.writeCommentString(v.targetFile, comment, targetPos)
}

func (v *Visitor) replaceMarker(marker string, replacement string) {
	v.replaceMarkerString(v.targetFile, marker, replacement)
}

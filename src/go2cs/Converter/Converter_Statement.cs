//******************************************************************************************************
//  Converter_Statement.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/04/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Antlr4.Runtime.Misc;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string IfElseMarker = ">>MARKER:IFELSE_LEVEL_{0}<<";
        public const string IfElseBreakMarker = ">>MARKER:IFELSEBREAK_LEVEL_{0}<<";
        public const string IfExpressionMarker = ">>MARKER:IFEXPR_LEVEL_{0}<<";
        public const string IfStatementMarker = ">>MARKER:IFSTATEMENT_LEVEL_{0}<<";
        public const string ExprSwitchExpressionMarker = ">>MARKER:EXPRSWITCH_LEVEL_{0}<<";
        public const string ExprSwitchCaseTypeMarker = ">>MARKER:EXPRSWITCHCASE_LEVEL_{0}<<";
        public const string ExprSwitchStatementMarker = ">>MARKER:EXPRSWITCHSTATEMENT_LEVEL_{0}<<";
        public const string TypeSwitchExpressionMarker = ">>MARKER:TYPESWITCH_LEVEL_{0}<<";
        public const string TypeSwitchCaseTypeMarker = ">>MARKER:TYPESWITCHCASE_LEVEL_{0}<<";
        public const string TypeSwitchStatementMarker = ">>MARKER:TYPESWITCHSTATEMENT_LEVEL_{0}<<";
        public const string ForExpressionMarker = ">>MARKER:FOREXPRESSION_LEVEL_{0}<<";
        public const string ForInitStatementMarker = ">>MARKER:FORINITSTATEMENT_LEVEL_{0}<<";
        public const string ForPostStatementMarker = ">>MARKER:FORPOSTSTATEMENT_LEVEL_{0}<<";
        public const string ForRangeExpressionsMarker = ">>MARKER:FORRANGEEXPRESSIONS_LEVEL_{0}<<";

        private readonly ParseTreeValues<string> m_simpleStatements = new ParseTreeValues<string>();
        private readonly ParseTreeValues<TypeInfo> m_elementTypes = new ParseTreeValues<TypeInfo>();
        private readonly Dictionary<string, bool> m_labels = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Stack<HashSet<string>> m_blockLabeledContinues = new Stack<HashSet<string>>();
        private readonly Stack<HashSet<string>> m_blockLabeledBreaks = new Stack<HashSet<string>>();
        private readonly Stack<StringBuilder> m_exprSwitchDefaultCase = new Stack<StringBuilder>();
        private readonly Stack<StringBuilder> m_typeSwitchDefaultCase = new Stack<StringBuilder>();
        private int m_ifExpressionLevel;
        private int m_exprSwitchExpressionLevel;
        private int m_typeSwitchExpressionLevel;
        private int m_forExpressionLevel;
        private bool m_fallThrough;

        public override void ExitStatement(GoParser.StatementContext context)
        {
            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Append($"{statement}{(LineTerminatorAhead(context.simpleStmt()) ? "" : Environment.NewLine)}");
                else
                    AddWarning(context, $"Failed to find simple statement for: {context.simpleStmt().GetText()}");
            }
        }

        public override void EnterLabeledStmt(GoParser.LabeledStmtContext context)
        {
            // labeledStmt
            //     : IDENTIFIER ':' statement

            PushBlock();
            m_labels.Add(SanitizedIdentifier(context.IDENTIFIER().GetText()), false);

            // Check labeled continue in for loop
            // Check labeled break in for loop, select and switch
        }

        public override void ExitLabeledStmt(GoParser.LabeledStmtContext context)
        {
            // labeledStmt
            //     : IDENTIFIER ':' statement

            string label = SanitizedIdentifier(context.IDENTIFIER().GetText());
            string statement = PopBlock(false);

            m_targetFile.Append($"{label}:{CheckForCommentsRight(context)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();

            m_targetFile.Append(statement);
        }

        public override void ExitSendStmt(GoParser.SendStmtContext context)
        {
            // sendStmt
            //     : expression '<-' expression

            if (Expressions.TryGetValue(context.expression(0), out ExpressionInfo channel) && Expressions.TryGetValue(context.expression(1), out ExpressionInfo value))
            {
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{channel}.Send({value});{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find both channel and value expression for send statement: {context.GetText()}");
            }
        }

        public override void ExitExpressionStmt(GoParser.ExpressionStmtContext context)
        {
            // expressionStmt
            //     : expression

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{expression};{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for expression statement: {context.GetText()}");
            }
        }

        public override void ExitIncDecStmt(GoParser.IncDecStmtContext context)
        {
            // incDecStmt
            //     : expression('++' | '--')

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                StringBuilder statement = new StringBuilder();

                statement.Append($"{Spacing()}{expression}{context.children[1].GetText()};{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    statement.AppendLine();

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for inc/dec statement: {context.GetText()}");
            }
        }

        public override void ExitAssignment(GoParser.AssignmentContext context)
        {
            // assignment
            //     : expressionList assign_op expressionList

            // TODO: Properly handle assignment order of operations, see https://gridprotectionalliance.github.io/go2cs/ConversionStrategies.html#inline-assignment-order-of-operations
            if (ExpressionLists.TryGetValue(context.expressionList(0), out ExpressionInfo[] leftOperands) && ExpressionLists.TryGetValue(context.expressionList(1), out ExpressionInfo[] rightOperands))
            {
                StringBuilder statement = new StringBuilder();

                if (leftOperands.Length != rightOperands.Length)
                {
                    if (leftOperands.Length > rightOperands.Length && rightOperands.Length == 1)
                    {
                        leftOperands = new[]
                        {
                            new ExpressionInfo 
                            { 
                                Text = string.Join(", ", leftOperands.Select(expr => expr.Text)),
                                Type = leftOperands[0].Type
                            }
                        };
                    }
                    else
                    {
                        AddWarning(context, $"Encountered count mismatch for left and right operand expressions in assignment statement: {context.GetText()}");
                    }
                }

                int length = Math.Min(leftOperands.Length, rightOperands.Length);
               
                for (int i = 0; i < length; i++)
                {
                    string assignOP = context.assign_op().GetText();

                    if (assignOP.Equals("<<=") || assignOP.Equals(">>="))
                    {
                        // TODO: Need expression evaluation - cast not needed for int expressions
                        if (!int.TryParse(rightOperands[i].Text, out int _))
                            rightOperands[i].Text = $"(int)({rightOperands[i]})";
                    }

                    if (assignOP.Equals("&^="))
                    {
                        assignOP = " &= ";
                        rightOperands[i].Text = $"~({rightOperands[i].Text})";
                    }
                    else
                    {
                        assignOP = $" {assignOP} ";
                    }

                    statement.Append($"{Spacing()}{leftOperands[i]}{assignOP}{rightOperands[i]};");

                    // Since multiple assignments can be on one line, only check for comments after last assignment
                    if (i < length - 1)
                    {
                        statement.AppendLine();
                    }
                    else
                    {
                        statement.Append(CheckForCommentsRight(context));

                        if (!WroteLineFeed)
                            statement.AppendLine();
                    }

                    m_simpleStatements[context.Parent] = statement.ToString();
                }
            }
            else
            {
                AddWarning(context, $"Failed to find both left and right operand expressions for assignment statement: {context.GetText()}");
            }
        }

        public override void ExitElementType(GoParser.ElementTypeContext context)
        {
            if (Types.TryGetValue(context.type_(), out TypeInfo typeInfo))
                m_elementTypes[context] = typeInfo;
            else
                AddWarning(context, $"Failed to find type info for: {context.GetText()}");
        }

        public override void ExitArrayType(GoParser.ArrayTypeContext context)
        {
            base.ExitArrayType(context);
        }

        public override void ExitLiteralType(GoParser.LiteralTypeContext context)
        {
            base.ExitLiteralType(context);
        }

        public override void ExitLiteralValue(GoParser.LiteralValueContext context)
        {
            base.ExitLiteralValue(context);
        }

        public override void ExitShortVarDecl(GoParser.ShortVarDeclContext context)
        {
            // shortVarDecl
            //     : identifierList ':=' expressionList

            if (Identifiers.TryGetValue(context.identifierList(), out string[] identifiers) && ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions))
            {
                StringBuilder statement = new StringBuilder();

                if (identifiers.Length != expressions.Length)
                {
                    if (identifiers.Length > expressions.Length && expressions.Length == 1)
                        identifiers = new[] { $"({string.Join(", ", identifiers)})" };
                    else
                        AddWarning(context, $"Encountered count mismatch for identifiers and expressions in short var declaration statement: {context.GetText()}");
                }

                int length = Math.Min(identifiers.Length, expressions.Length);

                for (int i = 0; i < length; i++)
                {
                    statement.Append($"{Spacing()}{expressions[i].Type?.TypeName ?? "var"} {identifiers[i]} = {expressions[i]};");

                    // Since multiple declarations can be on one line, only check for comments after last declaration
                    if (i < length - 1)
                    {
                        statement.AppendLine();
                    }
                    else
                    {
                        statement.Append(CheckForCommentsRight(context));

                        if (!WroteLineFeed)
                            statement.AppendLine();
                    }
                }

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find both identifier and expression lists for short var declaration statement: {context.GetText()}");
            }
        }

        public override void ExitGoStmt(GoParser.GoStmtContext context)
        {
            // goStmt
            //     : 'go' expression

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                RequiredUsings.Add("System.Threading");
                m_targetFile.Append($"{Spacing()}go_(() => {expression});{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for go statement: {context.GetText()}");
            }
        }

        public override void ExitReturnStmt(GoParser.ReturnStmtContext context)
        {
            // returnStmt
            //     : 'return' expressionList?

            m_targetFile.Append($"{Spacing()}return");

            if (ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions))
            {
                if (expressions.Length > 1)
                    m_targetFile.Append($" ({string.Join(", ", expressions.Select(expr => expr.Text))})");
                else if (expressions.Length > 0)
                    m_targetFile.Append($" {expressions[0]}");
            }

            m_targetFile.Append($";{CheckForCommentsRight(context)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitBreakStmt(GoParser.BreakStmtContext context)
        {
            // breakStmt
            //     : 'break' IDENTIFIER ?

            bool breakHandled = false;

            if (!(context.IDENTIFIER() is null))
            {
                string label = SanitizedIdentifier(context.IDENTIFIER().GetText());

                if (m_labels.ContainsKey(label))
                {
                    breakHandled = true;

                    foreach (HashSet<string> blockBreaks in m_blockLabeledBreaks)
                        blockBreaks.Add(label);

                    m_targetFile.Append($"{Spacing()}_break{label} = true;{CheckForCommentsRight(context)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine($"{Spacing()}break;");
                }
            }

            if (!breakHandled)
            {
                m_targetFile.Append($"{Spacing()}break;{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
        }

        public override void ExitContinueStmt(GoParser.ContinueStmtContext context)
        {
            // continueStmt
            //     : 'continue' IDENTIFIER ?

            bool continueHandled = false;

            if (!(context.IDENTIFIER() is null))
            {
                string label = SanitizedIdentifier(context.IDENTIFIER().GetText());

                if (m_labels.ContainsKey(label))
                {
                    continueHandled = true;

                    foreach (HashSet<string> blockContinues in m_blockLabeledContinues)
                        blockContinues.Add(label);

                    m_targetFile.Append($"{Spacing()}_continue{label} = true;{CheckForCommentsRight(context)}");

                    if (!WroteLineFeed)
                        m_targetFile.AppendLine();

                    m_targetFile.AppendLine($"{Spacing()}break;");
                }
            }

            if (!continueHandled)
            {
                m_targetFile.Append($"{Spacing()}continue;{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
        }

        public override void ExitGotoStmt(GoParser.GotoStmtContext context)
        {
            // gotoStmt
            //     : 'goto' IDENTIFIER

            m_targetFile.Append($"{Spacing()}goto {SanitizedIdentifier(context.IDENTIFIER().GetText())};{CheckForCommentsRight(context)}");

            if (!WroteLineFeed)
                m_targetFile.AppendLine();
        }

        public override void ExitFallthroughStmt(GoParser.FallthroughStmtContext context)
        {
            // fallthroughStmt
            //     : 'fallthrough'

            m_fallThrough = true;
        }

        public override void EnterIfStmt(GoParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if '(simpleStmt ';') ? expression block ( 'else' ( ifStmt | block ) ) ?

            m_ifExpressionLevel++;

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                // Any declared variable will be scoped to if statement, so create a sub-block for it
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(IfStatementMarker, m_ifExpressionLevel));
            }

            m_targetFile.AppendLine($"{string.Format(IfElseBreakMarker, m_ifExpressionLevel)}{Spacing()}{string.Format(IfElseMarker, m_ifExpressionLevel)}if ({string.Format(IfExpressionMarker, m_ifExpressionLevel)})");

            if (context.block().Length == 2)
            {
                PushOuterBlockSuffix(null);  // For current block
                PushOuterBlockSuffix($"{Environment.NewLine}{Spacing()}else{(LineTerminatorAhead(context.block(0)) ? "" : Environment.NewLine)}");
            }
            else
            {
                PushOuterBlockSuffix(null);  // For current block
            }
        }

        public override void ExitIfStmt(GoParser.IfStmtContext context)
        {
            // ifStmt
            //     : 'if '(simpleStmt ';') ? expression block ( 'else' ( ifStmt | block ) ) ?

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                bool isElseIf = context.Parent is GoParser.IfStmtContext;

                // Replace if markers
                m_targetFile.Replace(string.Format(IfExpressionMarker, m_ifExpressionLevel), expression.Text);
                m_targetFile.Replace(string.Format(IfElseBreakMarker, m_ifExpressionLevel), isElseIf ? Environment.NewLine : "");
                m_targetFile.Replace(string.Format(IfElseMarker, m_ifExpressionLevel), isElseIf ? "else " : "");
            }
            else
            {
                AddWarning(context, $"Failed to find expression for if statement: {context.GetText()}");
            }

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(IfStatementMarker, m_ifExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for if statement: {context.simpleStmt().GetText()}");
                
                // Close any locally scoped declared variable sub-block
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            if (!EndsWithLineFeed(m_targetFile.ToString()))
                m_targetFile.AppendLine();

            m_ifExpressionLevel--;
        }

        public override void EnterExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            m_exprSwitchExpressionLevel++;

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel));
            }

            if (context.expression() is null)
                m_targetFile.Append($"{Spacing()}Switch()");
            else
                m_targetFile.Append($"{Spacing()}Switch({string.Format(ExprSwitchExpressionMarker, m_exprSwitchExpressionLevel)})");

            m_exprSwitchDefaultCase.Push(new StringBuilder());
        }

        public override void EnterExprCaseClause(GoParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            if (context.exprSwitchCase().expressionList() is null)
                m_exprSwitchDefaultCase.Peek().Append($"{Environment.NewLine}{Spacing()}.Default(() =>{Environment.NewLine}{Spacing()}{{{CheckForCommentsLeft(context.statementList(), 1)}");
            else
                m_targetFile.Append($"{Environment.NewLine}{Spacing()}.Case({string.Format(ExprSwitchCaseTypeMarker, m_exprSwitchExpressionLevel)})(() =>{Environment.NewLine}{Spacing()}{{{CheckForCommentsLeft(context.statementList(), 1)}");

            IndentLevel++;

            PushBlock();
        }

        public override void ExitExprCaseClause(GoParser.ExprCaseClauseContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // exprCaseClause
            //     : exprSwitchCase ':' statementList

            // exprSwitchCase
            //     : 'case' expressionList | 'default'

            IndentLevel--;

            GoParser.ExpressionListContext expressionList = context.exprSwitchCase().expressionList();

            if (expressionList is null)
            {
                m_exprSwitchDefaultCase.Peek().Append($"{PopBlock(false)}{Spacing()}}})");
            }
            else
            {
                PopBlock();
                m_targetFile.Append($"{Spacing()}}}{(m_fallThrough ? ", fallthrough" : "")})");

                if (!ExpressionLists.TryGetValue(expressionList, out ExpressionInfo[] expressions))
                    AddWarning(expressionList, $"Failed to find expression list for switch case statement: {context.exprSwitchCase().GetText()}");

                // Replace switch expression case type marker
                m_targetFile.Replace(string.Format(ExprSwitchCaseTypeMarker, m_exprSwitchExpressionLevel), string.Join(", ", expressions.Select(expr => expr.Text)));
            }

            // Reset fallthrough flag at the end of each case clause
            m_fallThrough = false;
        }

        public override void ExitExprSwitchStmt(GoParser.ExprSwitchStmtContext context)
        {
            // exprSwitchStmt
            //     : 'switch'(simpleStmt ';') ? expression ? '{' exprCaseClause * '}'

            // Default case always needs to be last case clause in SwitchExpression - Go allows its declaration anywhere
            m_targetFile.Append($"{m_exprSwitchDefaultCase.Pop()};{CheckForCommentsRight(context)}");

            if (!(context.expression() is null))
            {
                if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
                {
                    // Replace switch expression marker
                    m_targetFile.Replace(string.Format(ExprSwitchExpressionMarker, m_exprSwitchExpressionLevel), expression.Text);
                }
                else
                {
                    AddWarning(context, $"Failed to find expression for switch statement: {context.expression().GetText()}");
                }
            }

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(ExprSwitchStatementMarker, m_exprSwitchExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for expression based switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            m_exprSwitchExpressionLevel--;
        }

        public override void EnterTypeSwitchStmt(GoParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch' (simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            m_typeSwitchExpressionLevel++;

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    // Any declared variable will be scoped to switch statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                m_targetFile.Append(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel));
            }

            if (!(context.typeSwitchGuard().IDENTIFIER() is null))
            {
                string identifier = SanitizedIdentifier(context.typeSwitchGuard().IDENTIFIER().GetText());

                m_targetFile.AppendLine($"{Spacing()}var {identifier} = {string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)};");
                m_targetFile.AppendLine();
                m_targetFile.Append($"{Spacing()}Switch({identifier})");
            }
            else
            {
                m_targetFile.Append($"{Spacing()}Switch({string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel)})");
            }

            m_typeSwitchDefaultCase.Push(new StringBuilder());
        }

        public override void EnterTypeCaseClause(GoParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            if (context.typeSwitchCase().typeList() is null)
                m_typeSwitchDefaultCase.Peek().Append($"{Environment.NewLine}{Spacing()}.Default(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            else
                m_targetFile.Append($"{Environment.NewLine}{Spacing()}.Case({string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel)})(() =>{Environment.NewLine}{Spacing()}{{{Environment.NewLine}");
            
            IndentLevel++;

            PushBlock();
        }

        public override void ExitTypeCaseClause(GoParser.TypeCaseClauseContext context)
        {
            // typeCaseClause
            //     : typeSwitchCase ':' statementList

            // typeSwitchCase
            //     : 'case' typeList | 'default'

            // typeList
            //     : type ( ',' type )*

            IndentLevel--;

            if (context.typeSwitchCase().typeList() is null)
            {
                m_typeSwitchDefaultCase.Peek().Append($"{PopBlock(false)}{Spacing()}}})");
            }
            else
            {
                PopBlock();
                m_targetFile.Append($"{Spacing()}}}{(m_fallThrough ? ", fallthrough" : "")})");

                GoParser.TypeListContext typeList = context.typeSwitchCase().typeList();
                List<string> types = new List<string>();

                for (int i = 0; i < typeList.type_().Length; i++)
                {
                    if (Types.TryGetValue(typeList.type_(i), out TypeInfo typeInfo))
                    {
                        string typeName = typeInfo.TypeName;

                        if (typeName.Equals("nil", StringComparison.Ordinal))
                            typeName = "NilType";

                        types.Add($"typeof({typeName})");
                    }
                    else
                    {
                        AddWarning(typeList, $"Failed to find type info for type switch case statement: {context.typeSwitchCase().GetText()}");
                    }
                }

                // Replace type switch expression marker
                m_targetFile.Replace(string.Format(TypeSwitchCaseTypeMarker, m_typeSwitchExpressionLevel), string.Join(", ", types));
            }

            // Reset fallthrough flag at the end of each case clause
            m_fallThrough = false;
        }

        public override void ExitTypeSwitchStmt(GoParser.TypeSwitchStmtContext context)
        {
            // typeSwitchStmt
            //     : 'switch'(simpleStmt ';') ? typeSwitchGuard '{' typeCaseClause * '}'

            // typeSwitchGuard
            //     : ( IDENTIFIER ':=' )? primaryExpr '.' '(' 'type' ')'

            // Default case always needs to be last case clause in SwitchExpression - Go allows its declaration anywhere
            m_targetFile.Append($"{m_typeSwitchDefaultCase.Pop()};{CheckForCommentsRight(context)}");

            if (PrimaryExpressions.TryGetValue(context.typeSwitchGuard().primaryExpr(), out ExpressionInfo expression))
            {
                // Replace type switch expression marker
                m_targetFile.Replace(string.Format(TypeSwitchExpressionMarker, m_typeSwitchExpressionLevel), expression.Text);
            }
            else
            {
                AddWarning(context, $"Failed to find primary expression for type switch statement: {context.typeSwitchGuard().GetText()}");
            }

            if (!(context.simpleStmt() is null) && context.simpleStmt().emptyStmt() is null)
            {
                // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                if (m_simpleStatements.TryGetValue(context.simpleStmt(), out string statement))
                    m_targetFile.Replace(string.Format(TypeSwitchStatementMarker, m_typeSwitchExpressionLevel), statement + Environment.NewLine);
                else
                    AddWarning(context, $"Failed to find simple statement for type switch statement: {context.simpleStmt().GetText()}");

                // Close any locally scoped declared variable sub-block
                if (!(context.simpleStmt().shortVarDecl() is null))
                {
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            m_typeSwitchExpressionLevel--;
        }

        public override void ExitSelectStmt(GoParser.SelectStmtContext context)
        {
            // selectStmt
            //     : 'select' '{' commClause * '}'
        }

        public override void EnterForStmt(GoParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause)? block

            m_forExpressionLevel++;

            GoParser.ForClauseContext forClause = context.forClause();

            if (!(context.expression() is null) || !(forClause is null) && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
            {
                // Handle while-style statement
                m_targetFile.AppendLine($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
            }
            else if (!(forClause is null))
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                bool hasInitStatement = ForHasInitStatement(forClause, out GoParser.SimpleStmtContext simpleInitStatement);
                bool hasPostStatement = ForHasPostStatement(forClause, out GoParser.SimpleStmtContext simplePostStatement);
                bool useForStyleStatement =
                    hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null));

                if (hasInitStatement)
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    if (!useForStyleStatement && !(simpleInitStatement.shortVarDecl() is null))
                    {
                        m_targetFile.AppendLine($"{Spacing()}{{");
                        IndentLevel++;
                    }
                }

                if (hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null)))
                {
                    // Use standard for-style statement for simple constructs
                    m_targetFile.AppendLine($"{Spacing()}for ({string.Format(ForInitStatementMarker, m_forExpressionLevel)}; {string.Format(ForExpressionMarker, m_forExpressionLevel)}; {string.Format(ForPostStatementMarker, m_forExpressionLevel)})");
                    PushInnerBlockSuffix(null);
                }
                else
                {
                    if (hasInitStatement)
                        m_targetFile.Append(string.Format(ForInitStatementMarker, m_forExpressionLevel));

                    if (hasPostStatement)
                        PushInnerBlockSuffix($"{Spacing(indentLevel: 1)}{string.Format(ForPostStatementMarker, m_forExpressionLevel)}");
                    else
                        PushInnerBlockSuffix(null);

                    // Use loop-style statement for more free-form for constructs
                    m_targetFile.AppendLine($"{Spacing()}while ({string.Format(ForExpressionMarker, m_forExpressionLevel)})");
                }
            }
            else
            {
                // rangeClause
                //     : ( expressionList '=' | identifierList ':=' )? 'range' expression

                if (!(context.rangeClause().identifierList() is null))
                {
                    // Any declared variable will be scoped to for statement, so create a sub-block for it
                    m_targetFile.AppendLine($"{Spacing()}{{");
                    IndentLevel++;
                }

                // Handle item iteration style statement - since Go's iteration variables are mutable and
                // can be pre-declared, a standard for loop is used instead of a foreach. The exception
                // is for an index and rune iteration over a string which in C# a foreach works fine because
                // the inferred tuple instance can be readonly since it would not be referenced by Go code.
            }
        }

        public override void ExitForStmt(GoParser.ForStmtContext context)
        {
            // forStmt
            //     : 'for' (expression | forClause | rangeClause)? block

            GoParser.ForClauseContext forClause = context.forClause();

            if (!(context.expression() is null) || !(forClause is null) && forClause.simpleStmt()?.Length == 0 || context.children.Count < 3)
            {
                Expressions.TryGetValue(context.expression() ?? forClause?.expression(), out ExpressionInfo expression);
                m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression?.Text ?? "true");
            }
            else if (!(forClause is null))
            {
                // forClause
                //     : simpleStmt? ';' expression? ';' simpleStmt?

                if (Expressions.TryGetValue(forClause.expression(), out ExpressionInfo expression))
                    m_targetFile.Replace(string.Format(ForExpressionMarker, m_forExpressionLevel), expression.Text);
                else
                    AddWarning(context, $"Failed to find expression in for statement: {context.GetText()}");

                bool hasInitStatement = ForHasInitStatement(forClause, out GoParser.SimpleStmtContext simpleInitStatement);
                bool hasPostStatement = ForHasPostStatement(forClause, out GoParser.SimpleStmtContext simplePostStatement);
                bool useForStyleStatement =
                    hasInitStatement && (!(simpleInitStatement.shortVarDecl() is null) || !(simpleInitStatement.assignment() is null)) &&
                    hasPostStatement && (!(simplePostStatement.incDecStmt() is null) || !(simplePostStatement.expressionStmt() is null));

                if (hasInitStatement)
                {
                    // TODO: Handle case where short val declaration re-declares a variable already defined in current scope - C# does not allow this. One option: cache current variable value and restore below
                    if (m_simpleStatements.TryGetValue(simpleInitStatement, out string statement))
                    {
                        if (useForStyleStatement)
                        {
                            statement = statement.Trim();

                            if (statement.EndsWith(";", StringComparison.Ordinal))
                                statement = statement.Substring(0, statement.Length - 1);
                        }
                        else
                        {
                            statement = statement + Environment.NewLine;
                        }

                        m_targetFile.Replace(string.Format(ForInitStatementMarker, m_forExpressionLevel), statement);
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find simple initial statement in for clause: {simpleInitStatement.GetText()}");
                    }

                    // Close any locally scoped declared variable sub-block
                    if (!useForStyleStatement && !(simpleInitStatement.shortVarDecl() is null))
                    {
                        IndentLevel--;
                        m_targetFile.AppendLine();
                        m_targetFile.Append($"{Spacing()}}}");
                    }
                }

                if (hasPostStatement)
                {
                    if (m_simpleStatements.TryGetValue(simplePostStatement, out string statement))
                    {
                        if (useForStyleStatement)
                        {
                            statement = statement.Trim();

                            if (statement.EndsWith(";", StringComparison.Ordinal))
                                statement = statement.Substring(0, statement.Length - 1);
                        }
                        else
                        {
                            statement = statement.TrimEnd();
                        }

                        m_targetFile.Replace(string.Format(ForPostStatementMarker, m_forExpressionLevel), statement);
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find simple post statement in for clause: {simplePostStatement.GetText()}");
                    }
                }
            }
            else
            {
                // rangeClause
                //     : (expressionList '=' | identifierList ':=' )? 'range' expression

                if (!(context.rangeClause().identifierList() is null))
                {
                    // Close any locally scoped declared variable sub-block
                    IndentLevel--;
                    m_targetFile.AppendLine();
                    m_targetFile.Append($"{Spacing()}}}");
                }
            }

            m_targetFile.Append(CheckForCommentsRight(context));

            m_forExpressionLevel--;
        }

        private bool ForHasInitStatement(GoParser.ForClauseContext forClause, out GoParser.SimpleStmtContext simpleStatement)
        {
            simpleStatement = null;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            return
                !(simpleStatement is null) &&
                simpleStatement.emptyStmt() is null &&
                forClause.children[^1] != simpleStatement;
        }

        private bool ForHasPostStatement(GoParser.ForClauseContext forClause, out GoParser.SimpleStmtContext simpleStatement)
        {
            simpleStatement = null;

            if (forClause.simpleStmt()?.Length == 0)
                return false;

            simpleStatement = forClause.simpleStmt(0);

            if (forClause.simpleStmt().Length == 1)
                return
                    !(simpleStatement is null) &&
                    simpleStatement.emptyStmt() is null &&
                    forClause.children[^1] == simpleStatement;

            simpleStatement = forClause.simpleStmt(1);

            return
                !(simpleStatement is null) &&
                simpleStatement.emptyStmt() is null &&
                forClause.children[^1] == simpleStatement;
        }

        public override void ExitDeferStmt(GoParser.DeferStmtContext context)
        {
            // deferStmt
            //     : 'defer' expression

            if (Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
            {
                m_targetFile.Append($"{Spacing()}defer({expression});{CheckForCommentsRight(context)}");

                if (!WroteLineFeed)
                    m_targetFile.AppendLine();
            }
            else
            {
                AddWarning(context, $"Failed to find expression for defer statement: {context.GetText()}");
            }
        }
    }
}

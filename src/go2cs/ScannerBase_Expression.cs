//******************************************************************************************************
//  ScannerBase_Expression.cs - Gbtc
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
//  05/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using Antlr4.Runtime.Misc;
using go2cs.Metadata;
using System;
using System.Collections.Generic;
using static go2cs.Common;

namespace go2cs
{
    public partial class ScannerBase
    {
        // Stack handlers:
        //  expressionList (required)
        //  expressionStmt (required)
        //  sendStmt (required)
        //  incDecStmt (required)
        //  defer (required)
        //  ifStmt (required)
        //  exprSwitchStmt (optional)
        //  recvStmt (required)
        //  forClause (optional)
        //  rangeClause (required)
        //  goStmt (required)
        //  arrayLength (required)
        //  operand (optional)
        //  key (optional)
        //  element (optional)
        //  index (required)
        //  slice (optional)
        //  expression (optional)
        //  conversion (required)
        protected readonly ParseTreeValues<string> Expressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> UnaryExpressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> PrimaryExpressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> Operands = new ParseTreeValues<string>();

        public override void ExitExpression(GoParser.ExpressionContext context)
        {
            // expression
            //     : primaryExpr
            //     | unaryExpr
            //     | expression('*' | '/' | '%' | '<<' | '>>' | '&' | '&^') expression
            //     | expression('+' | '-' | '|' | '^') expression
            //     | expression('==' | '!=' | '<' | '<=' | '>' | '>=') expression
            //     | expression '&&' expression
            //     | expression '||' expression

            if (context.expression()?.Length == 2)
            {
                string leftOperand = Expressions[context.expression(0)];
                string rightOperand = Expressions[context.expression(1)];
                string binaryOP = context.children[1].GetText();

                if (binaryOP.Equals("<<") || binaryOP.Equals(">>"))
                {
                    // TODO: Need expression evaluations - no cast needed if expressions is int type
                    if (!int.TryParse(rightOperand, out int _))
                        rightOperand = $"(int)({rightOperand})";
                }

                binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";

                Expressions[context] = $"{leftOperand}{binaryOP}{rightOperand}";
            }
            else
            {
                if (context.primaryExpr() != null)
                {
                    if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression))
                        Expressions[context] = primaryExpression;
                    else
                        AddWarning(context, $"Failed to find primary expression \"{context.unaryExpr().GetText()}\" in the expression \"{context.GetText()}\"");
                }
                else if (context.unaryExpr() != null)
                {
                    if (UnaryExpressions.TryGetValue(context.unaryExpr(), out string unaryExpression))
                        Expressions[context] = unaryExpression;
                    else
                        AddWarning(context, $"Failed to find unary expression \"{context.unaryExpr().GetText()}\" in the expression \"{context.GetText()}\"");
                }
                else
                {
                    AddWarning(context, $"Unexpected expression \"{context.GetText()}\"");
                }
            }
        }

        public override void ExitUnaryExpr(GoParser.UnaryExprContext context)
        {
            // unaryExpr
            //     : primaryExpr
            //     | ('+' | '-' | '!' | '^' | '*' | '&' | '<-') expression

            if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression))
            {
                UnaryExpressions[context] = primaryExpression;
            }
            else if (context.expression() != null)
            {
                string unaryOP = context.children[0].GetText();

                if (unaryOP.Equals("^", StringComparison.Ordinal))
                {
                    unaryOP = "~";
                }
                else if (unaryOP.Equals("<-", StringComparison.Ordinal))
                {
                    // TODO: Handle channel value access (update when channel class is created):
                    unaryOP = null;
                    UnaryExpressions[context] = $"{Expressions[context.expression()]}.Receive()";
                }
                else if (unaryOP.Equals("&", StringComparison.Ordinal))
                {
                    // TODO: Handle pointer acquisition context - may need to augment pre-scan metadata for heap allocation notation
                    unaryOP = null;
                    UnaryExpressions[context] = $"ref {Expressions[context.expression()]}";
                }
                else if (unaryOP.Equals("*", StringComparison.Ordinal))
                {
                    // TODO: Handle pointer dereference context - if this is a ref variable, Deref call is unnecessary
                    unaryOP = null;
                    UnaryExpressions[context] = $"{Expressions[context.expression()]}.Deref";
                }

                if ((object)unaryOP != null)
                    UnaryExpressions[context] = $"{unaryOP}{Expressions[context.expression()]}";
            }
            else if (!UnaryExpressions.ContainsKey(context))
            {
                AddWarning(context, $"Unexpected unary expression \"{context.GetText()}\"");
            }
        }

        public override void ExitPrimaryExpr(GoParser.PrimaryExprContext context)
        {
            // primaryExpr
            //     : operand
            //     | conversion
            //     | primaryExpr selector
            //     | primaryExpr index
            //     | primaryExpr slice
            //     | primaryExpr typeAssertion
            //     | primaryExpr arguments

            PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression);

            if (!string.IsNullOrEmpty(primaryExpression))
                primaryExpression = SanitizedIdentifier(primaryExpression);

            if (Operands.TryGetValue(context.operand(), out string operand))
            {
                PrimaryExpressions[context] = SanitizedIdentifier(operand);
            }
            else if (context.conversion() != null)
            {
                // conversion
                //     : type '(' expression ',' ? ')'

                if (Types.TryGetValue(context.conversion().type_(), out TypeInfo typeInfo) && Expressions.TryGetValue(context.conversion().expression(), out string expression))
                {
                    if (typeInfo.TypeName.StartsWith("*(*"))
                    {
                        // TODO: Complex pointer expression needs special handling consideration - could opt for unsafe implementation
                        PrimaryExpressions[context] = $"{typeInfo.TypeName}{expression}";
                    }
                    else
                    {
                        if (typeInfo.IsPointer)
                            PrimaryExpressions[context] = $"new Ptr<{typeInfo.TypeName}>({expression})";
                        else if (typeInfo.TypeClass == TypeClass.Struct)
                            PrimaryExpressions[context] = $"{typeInfo.TypeName}_cast({expression})";
                        else if (typeInfo.TypeClass == TypeClass.Simple)
                            PrimaryExpressions[context] = $"{typeInfo.TypeName}({expression})";
                        else
                            PrimaryExpressions[context] = $"({typeInfo.TypeName}){expression}";
                    }
                }
                else
                {
                    AddWarning(context, $"Failed to find type or sub-expression for the conversion expression in \"{context.GetText()}\"");
                }
            }
            else if (context.DOT() != null)
            {
                // selector
                //     : '.' IDENTIFIER

                PrimaryExpressions[context] = $"{primaryExpression}.{SanitizedIdentifier(context.IDENTIFIER().GetText())}";
            }
            else if (context.index() != null)
            {
                // index
                //     : '[' expression ']'

                if (Expressions.TryGetValue(context.index().expression(), out string expression))
                    PrimaryExpressions[context] = $"{primaryExpression}[{expression}]";
                else
                    AddWarning(context, $"Failed to find index expression for \"{context.GetText()}\"");
            }
            else if (context.slice() != null)
            {
                // slice
                //     : '['((expression ? ':' expression ? ) | (expression ? ':' expression ':' expression)) ']'

                GoParser.SliceContext sliceContext = context.slice();

                if (sliceContext.children.Count == 3)
                {
                    // primaryExpr[:]
                    PrimaryExpressions[context] = $"{primaryExpression}.slice()";
                }
                else if (sliceContext.children.Count == 4)
                {
                    bool expressionIsLeft = sliceContext.children[1] is GoParser.ExpressionContext;

                    // primaryExpr[low:] or primaryExpr[:high]
                    if (Expressions.TryGetValue(sliceContext.expression(0), out string expression))
                        PrimaryExpressions[context] = $"{primaryExpression}.slice({(expressionIsLeft ? expression : $"high:{expression}")})";
                    else
                        AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                }
                else if (sliceContext.children.Count == 5)
                {
                    if (sliceContext.children[1] is GoParser.ExpressionContext && sliceContext.children[3] is GoParser.ExpressionContext)
                    {
                        // primaryExpr[low:high]
                        if (Expressions.TryGetValue(sliceContext.expression(0), out string lowExpression) && Expressions.TryGetValue(sliceContext.expression(1), out string highExpression))
                            PrimaryExpressions[context] = $"{primaryExpression}.slice({lowExpression}, {highExpression})";
                        else
                            AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                    }
                }
                else if (sliceContext.children.Count == 6)
                {
                    // primaryExpr[:high:max]
                    if (Expressions.TryGetValue(sliceContext.expression(0), out string highExpression) && Expressions.TryGetValue(sliceContext.expression(1), out string maxExpression))
                        PrimaryExpressions[context] = $"{primaryExpression}.slice(-1, {highExpression}, {maxExpression})";
                    else
                        AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
                else if (sliceContext.children.Count == 7)
                {
                    // primaryExpr[low:high:max]
                    if (Expressions.TryGetValue(sliceContext.expression(0), out string lowExpression) && Expressions.TryGetValue(sliceContext.expression(1), out string highExpression) && Expressions.TryGetValue(sliceContext.expression(2), out string maxExpression))
                        PrimaryExpressions[context] = $"{primaryExpression}.slice({lowExpression}, {highExpression}, {maxExpression})";
                    else
                        AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
            }
            else if (context.typeAssertion() != null)
            {
                // typeAssertion
                //     : '.' '(' type ')'

                if (Types.TryGetValue(context.typeAssertion().type_(), out TypeInfo typeInfo))
                    PrimaryExpressions[context] = $"{primaryExpression}.TypeAssert<{typeInfo.TypeName}>()";
                else
                    AddWarning(context, $"Failed to find type for the type assertion expression in \"{context.GetText()}\"");
            }
            else if (context.arguments() != null)
            {
                // arguments
                //     : '('((expressionList | type(',' expressionList) ? ) '...' ? ',' ? ) ? ')'

                GoParser.ArgumentsContext argumentsContext = context.arguments();
                List<string> arguments = new List<string>();

                if (Types.TryGetValue(argumentsContext.type_(), out TypeInfo typeInfo))
                    arguments.Add($"typeof({typeInfo.TypeName})");

                if (ExpressionLists.TryGetValue(argumentsContext.expressionList(), out string[] expressions))
                    arguments.AddRange(expressions);

                PrimaryExpressions[context] = $"{primaryExpression}({string.Join(", ", arguments)})";
            }
            else
            {
                AddWarning(context, $"Unexpected primary expression \"{context.GetText()}\"");
            }
        }

        public override void ExitOperand( GoParser.OperandContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            if (Expressions.TryGetValue(context.expression(), out string expression))
                Operands[context] = $"({expression})";

            // Remaining operands contexts handled below...
        }

        public override void ExitBasicLit(GoParser.BasicLitContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            // literal
            //     : basicLit
            //     | compositeLit
            //     | functionLit

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from basic literal: \"{context.GetText()}\"");
                return;
            }

            string basicLiteral;

            // basicLit
            //     : INT_LIT
            //     | FLOAT_LIT
            //     | IMAGINARY_LIT
            //     | RUNE_LIT
            //     | STRING_LIT

            if (context.IMAGINARY_LIT() != null)
            {
                string value = context.IMAGINARY_LIT().GetText();
                basicLiteral = value.EndsWith("i") ? $"i({value.Substring(0, value.Length - 1)})" : value;
            }
            else if (context.RUNE_LIT() != null)
            {
                basicLiteral = ReplaceOctalBytes(context.RUNE_LIT().GetText());
            }
            else if (context.string_() != null)
            {
                basicLiteral = ToStringLiteral(ReplaceOctalBytes(context.string_().GetText()));
            }
            else
            {
                basicLiteral = context.GetText();
            }

            Operands[operandContext] = basicLiteral;
        }

        public override void ExitCompositeLit(GoParser.CompositeLitContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            // literal
            //     : basicLit
            //     | compositeLit
            //     | functionLit

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from composite literal: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update to handle in-line type constructions
            Operands[operandContext] = SanitizedIdentifier(context.GetText());
        }

        public override void ExitFunctionLit(GoParser.FunctionLitContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            // literal
            //     : basicLit
            //     | compositeLit
            //     | functionLit

            if (!(context.Parent.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from function literal: \"{context.GetText()}\"");
                return;
            }

            // functionLit
            //     : 'func' function

            // This is a place-holder for base class - derived classes, e.g., Converter, have to properly handle function content
            Operands[operandContext] = SanitizedIdentifier(context.GetText());
        }

        public override void ExitOperandName(GoParser.OperandNameContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            if (!(context.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from operand name: \"{context.GetText()}\"");
                return;
            }

            // operandName
            //     : IDENTIFIER
            //     | qualifiedIdent

            Operands[operandContext] = context.GetText();
        }

        public override void ExitMethodExpr([NotNull] GoParser.MethodExprContext context)
        {
            // operand
            //     : literal
            //     | operandName
            //     | methodExpr
            //     | '(' expression ')'

            if (!(context.Parent is GoParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from method expression: \"{context.GetText()}\"");
                return;
            }

            // methodExpr
            //     : receiverType '.' IDENTIFIER

            // receiverType
            //     : typeName
            //     | '(' '*' typeName ')'
            //     | '(' receiverType ')'

            GoParser.ReceiverTypeContext receiverType = context.receiverType();

            // TODO: Handle type name pointer dereference context - if this is a ref variable, Deref call is unnecessary
            if (receiverType?.children.Count == 4)
                Operands[operandContext] = $"{receiverType.typeName().GetText()}.Deref";
            else
                Operands[operandContext] = context.GetText();
        }
    }
}
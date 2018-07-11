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
        protected readonly ParseTreeValues<string> Arguments = new ParseTreeValues<string>();

        public override void ExitPrimaryExpr(GolangParser.PrimaryExprContext context)
        {
            PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression);

            if (Operands.TryGetValue(context.operand(), out string operand))
            {
                PrimaryExpressions[context] = operand;
            }
            else if (context.conversion() != null)
            {
                if (Types.TryGetValue(context.conversion().type(), out TypeInfo typeInfo) && Expressions.TryGetValue(context.index().expression(), out string expression))
                    PrimaryExpressions[context] = $"({typeInfo.PrimitiveName})({expression})";
                else
                    AddWarning(context, $"Failed to find type or sub-expression for the conversion expression in \"{context.GetText()}\"");
            }
            else if (context.selector() != null)
            {
                PrimaryExpressions[context] = $"{primaryExpression}.{SanitizedIdentifier(context.selector().IDENTIFIER().GetText())}";
            }
            else if (context.index() != null)
            {
                if (Expressions.TryGetValue(context.index().expression(), out string expression))
                    PrimaryExpressions[context] = $"{primaryExpression}[{expression}]";
                else
                    AddWarning(context, $"Failed to find index expression for \"{context.GetText()}\"");
            }
            else if (context.slice() != null)
            {
                GolangParser.SliceContext slice = context.slice();

                if (slice.children.Count == 3)
                {
                    // exp[:]
                    PrimaryExpressions[context] = $"{primaryExpression}.Slice()";
                }
                else if (slice.children.Count == 4)
                {
                    bool expressionIsLeft = slice.children[1] is GolangParser.ExpressionContext;

                    // exp[low:] or exp[:high]
                    if (Expressions.TryGetValue(slice.expression(0), out string expression))
                        PrimaryExpressions[context] = $"{primaryExpression}.Slice({(expressionIsLeft ? expression : $"high:{expression}")})";
                    else
                        AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                }
                else if (slice.children.Count == 5)
                {
                    if (slice.children[1] is GolangParser.ExpressionContext && slice.children[3] is GolangParser.ExpressionContext)
                    {
                        // exp[low:high]
                        if (Expressions.TryGetValue(slice.expression(0), out string lowExpression) && Expressions.TryGetValue(slice.expression(1), out string highExpression))
                            PrimaryExpressions[context] = $"{primaryExpression}.Slice({lowExpression}, {highExpression})";
                        else
                            AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                    }
                    else
                    {
                        AddWarning(context, $"Failed to find slice expression for \"{context.GetText()}\"");
                    }
                }
                else if (slice.children.Count == 6)
                {
                    // exp[:high:max]
                    if (Expressions.TryGetValue(slice.expression(0), out string highExpression) && Expressions.TryGetValue(slice.expression(1), out string maxExpression))
                        PrimaryExpressions[context] = $"{primaryExpression}.Slice(-1, {highExpression}, {maxExpression})";
                    else
                        AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
                else if (slice.children.Count == 7)
                {
                    // exp[low:high:max]
                    if (Expressions.TryGetValue(slice.expression(0), out string lowExpression) && Expressions.TryGetValue(slice.expression(1), out string highExpression) && Expressions.TryGetValue(slice.expression(2), out string maxExpression))
                        PrimaryExpressions[context] = $"{primaryExpression}.Slice({lowExpression}, {highExpression}, {maxExpression})";
                    else
                        AddWarning(context, $"Failed to find one of the slice expressions for \"{context.GetText()}\"");
                }
            }
            else if (context.typeAssertion() != null)
            {
                if (Types.TryGetValue(context.typeAssertion().type(), out TypeInfo typeInfo))
                    PrimaryExpressions[context] = $"{primaryExpression}.TypeAssert<{typeInfo.PrimitiveName}>()";
                else
                    AddWarning(context, $"Failed to find type for the type assertion expression in \"{context.GetText()}\"");
            }
            else if (Arguments.TryGetValue(context.arguments(), out string arguments))
            {
                PrimaryExpressions[context] = $"{primaryExpression}{arguments}";
            }
            else
            {
                AddWarning(context, $"Unexpected primary expression \"{context.GetText()}\"");
            }
        }

        public override void ExitExpression(GolangParser.ExpressionContext context)
        {
            if (context.expression()?.Length == 2)
            {                
                string binaryOP = context.children[1].GetText();
                binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";
                Expressions[context] = $"{Expressions[context.expression(0)]}{binaryOP}{Expressions[context.expression(1)]}";
            }
            else
            {
                Expressions[context] = UnaryExpressions[context.unaryExpr()];
            }
        }

        public override void ExitUnaryExpr(GolangParser.UnaryExprContext context)
        {
            if (PrimaryExpressions.TryGetValue(context.primaryExpr(), out string primaryExpression))
            {
                UnaryExpressions[context] = primaryExpression;
            }
            else if (context.unaryExpr() != null)
            {
                string unaryOP = context.children[0].GetText();

                if (unaryOP.Equals("^"))
                {
                    unaryOP = "~";
                }
                else if (unaryOP.Equals("<-"))
                {
                    // TODO: Handle channel value access (update when channel class is created):
                    unaryOP = null;
                    UnaryExpressions[context] = $"{UnaryExpressions[context.unaryExpr()]}.Receive()";
                }
                else if (unaryOP.Equals("&"))
                {
                    // TODO: Handle pointer acquisition context - may need to augment pre-scan metadata for heap allocation notation
                    unaryOP = null;
                    UnaryExpressions[context] = $"ref {UnaryExpressions[context.unaryExpr()]}";
                }
                else if (unaryOP.Equals("*"))
                {
                    // TODO: Handle pointer dereference context
                    unaryOP = null;
                    UnaryExpressions[context] = $"{UnaryExpressions[context.unaryExpr()]}.Deref";
                }

                if ((object)unaryOP != null)
                    UnaryExpressions[context] = $"{unaryOP}{UnaryExpressions[context.unaryExpr()]}";
            }
            else if (!UnaryExpressions.ContainsKey(context))
            {
                AddWarning(context, $"Unexpected unary expression \"{context.GetText()}\"");
            }
        }

        public override void ExitArguments(GolangParser.ArgumentsContext context)
        {
            List<string> arguments = new List<string>();

            if (Types.TryGetValue(context.type(), out TypeInfo typeInfo))
                arguments.Add($"typeof({typeInfo.PrimitiveName})");

            if (ExpressionLists.TryGetValue(context.expressionList(), out string[] expressions))
                arguments.AddRange(expressions);

            Arguments[context] = $"({string.Join(", ", arguments)})";
        }

        public override void ExitOperand( GolangParser.OperandContext context)
        {
            if (Expressions.TryGetValue(context.expression(), out string expression))
                Operands[context] = $"({expression})"; ;
        }

        public override void ExitBasicLit(GolangParser.BasicLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from basic literal: \"{context.GetText()}\"");
                return;
            }

            string basicLiteral;

            //basicLit
            //    : INT_LIT
            //    | FLOAT_LIT
            //    | IMAGINARY_LIT
            //    | RUNE_LIT
            //    | STRING_LIT
            //    ;

            if (context.IMAGINARY_LIT() != null)
            {
                string value = context.IMAGINARY_LIT().GetText();
                basicLiteral = value.EndsWith("i") ? $"i({value.Substring(0, value.Length - 1)})" : value;
            }
            else if (context.RUNE_LIT() != null)
            {
                basicLiteral = ReplaceOctalBytes(context.RUNE_LIT().GetText());
            }
            else if (context.STRING_LIT() != null)
            {
                basicLiteral = ToStringLiteral(ReplaceOctalBytes(context.STRING_LIT().GetText()));
            }
            else
            {
                basicLiteral = context.GetText();
            }

            Operands[operandContext] = basicLiteral;
        }

        public override void ExitCompositeLit(GolangParser.CompositeLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from composite literal: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update to handle in-line type constructions
            Operands[operandContext] = context.GetText();
        }

        public override void ExitFunctionLit(GolangParser.FunctionLitContext context)
        {
            if (!(context.Parent.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from function literal: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update to handle in-line lambda declarations
            Operands[operandContext] = context.GetText();
        }

        public override void ExitOperandName(GolangParser.OperandNameContext context)
        {
            if (!(context.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from operand name: \"{context.GetText()}\"");
                return;
            }

            Operands[operandContext] = context.GetText();
        }

        public override void ExitMethodExpr([NotNull] GolangParser.MethodExprContext context)
        {
            if (!(context.Parent is GolangParser.OperandContext operandContext))
            {
                AddWarning(context, $"Could not derive parent operand context from method expression: \"{context.GetText()}\"");
                return;
            }

            // TODO: Update type name pointer dereferences, e.g., (*typeName)
            Operands[operandContext] = context.GetText();
        }
    }
}
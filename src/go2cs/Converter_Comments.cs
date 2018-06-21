//******************************************************************************************************
//  Converter_Comments.cs - Gbtc
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

using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace go2cs
{
    public partial class Converter
    {
        public const int BaseSpacing = 4;

        private int m_indentLevel;
        private bool m_wroteCommentWithLineFeed;

        private string Spacing(int offsetLevel = 0, int indentLevel = -1)
        {
            if (indentLevel < 0)
                indentLevel = m_indentLevel;

            indentLevel += offsetLevel;

            if (indentLevel < 1)
                return "";

            return new string(' ', BaseSpacing * indentLevel);
        }

        private string FixForwardSpacing(string source, int offsetLevel = 0, int indentLevel = -1, bool autoTrim = true)
        {
            if (indentLevel < 0)
                indentLevel = m_indentLevel;

            indentLevel += offsetLevel;

            string forwardSpacing = Spacing(0, indentLevel);
            string[] lines = source.Split(NewLineDelimeters, StringSplitOptions.None);
            return string.Join(Environment.NewLine, lines.Select(line => string.IsNullOrWhiteSpace(line) ? "" : $"{forwardSpacing}{(autoTrim? line.Trim() : line)}"));
        }

        private string CheckForCommentsLeft(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1)
        {
            if (indentLevel < 0)
                indentLevel = m_indentLevel;

            indentLevel += offsetLevel;

            return CheckForComments(indentLevel, context.Start.TokenIndex, TokenStream.GetHiddenTokensToLeft);
        }

        private string CheckForCommentsRight(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1)
        {
            if (indentLevel < 0)
                indentLevel = m_indentLevel;

            indentLevel += offsetLevel;

            return CheckForComments(indentLevel, context.Stop.TokenIndex, TokenStream.GetHiddenTokensToRight);
        }

        private string CheckForEndOfLineComment(ParserRuleContext context)
        {
            StringBuilder comments = new StringBuilder();
            IList<IToken> lineCommentChannel = TokenStream.GetHiddenTokensToRight(context.Stop.TokenIndex, GolangLexer.LineCommentChannel);

            if (lineCommentChannel?.Count > 0)
            {
                foreach (IToken token in lineCommentChannel)
                {
                    string commentText = token.Text;

                    if (commentText.Trim().StartsWith("//"))
                    {
                        if (!CommentOnNewLine(lineCommentChannel, token))
                        {
                            string[] lines = commentText.Split(NewLineDelimeters, StringSplitOptions.RemoveEmptyEntries);

                            if (lines.Length > 0)
                                comments.Append(lines[0]);
                        }
                    }
                }
            }

            return comments.ToString();
        }

        private string CheckForComments(int indentLevel, int tokenIndex, Func<int, int, IList<IToken>> getHiddenTokens)
        {
            StringBuilder comments = new StringBuilder();
            IList<IToken> hiddenChannel = getHiddenTokens(tokenIndex, TokenConstants.HiddenChannel);
            IList<IToken> lineCommentChannel = getHiddenTokens(tokenIndex, GolangLexer.LineCommentChannel);

            m_wroteCommentWithLineFeed = false;

            if (hiddenChannel?.Count > 0)
            {
                foreach (IToken token in hiddenChannel)
                {
                    string hiddenText = token.Text;

                    if (hiddenText.Trim().StartsWith("/*"))
                    {
                        if (CommentOnNewLine(hiddenChannel, token))
                            comments.Append(FixForwardSpacing(hiddenText, 0, indentLevel, false));
                        else
                            comments.Append(hiddenText);

                        m_wroteCommentWithLineFeed = hiddenText.EndsWith("\r") || hiddenText.EndsWith("\n");
                    }
                }
            }

            if (lineCommentChannel?.Count > 0)
            {
                foreach (IToken token in lineCommentChannel)
                {
                    string commentText = token.Text;

                    if (commentText.Trim().StartsWith("//"))
                    {
                        if (CommentOnNewLine(lineCommentChannel, token))
                            comments.Append(FixForwardSpacing(commentText, 0, indentLevel, false));
                        else
                            comments.Append(commentText);

                        m_wroteCommentWithLineFeed = true;
                    }
                }
            }

            return comments.ToString();
        }

        private bool CommentOnNewLine(IList<IToken> hiddenChannel, IToken testToken)
        {
            IToken priorToken = hiddenChannel?.FirstOrDefault(token => token.StopIndex == testToken.StartIndex - 1);

            if (priorToken != null)
                return priorToken.Text.EndsWith("\r") || priorToken.Text.EndsWith("\n");

            return false;
        }
    }
}
//******************************************************************************************************
//  ScannerBase_Comments.cs - Gbtc
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
    public partial class ScannerBase
    {
        public const int BaseSpacing = 4;

        protected int IndentLevel;
        protected bool WroteLineFeed;

        protected string Spacing(int offsetLevel = 0, int indentLevel = -1)
        {
            if (indentLevel < 0)
                indentLevel = IndentLevel;

            indentLevel += offsetLevel;

            if (indentLevel < 1)
                return "";

            return new string(' ', BaseSpacing * indentLevel);
        }

        protected string FixForwardSpacing(string source, int offsetLevel = 0, int indentLevel = -1, bool autoTrim = true, bool firstIsEOLComment = false)
        {
            if (indentLevel < 0)
                indentLevel = IndentLevel;

            indentLevel += offsetLevel;

            string forwardSpacing = Spacing(0, indentLevel);
            string[] lines = source.Split(NewLineDelimeters, StringSplitOptions.None);
           
            return string.Join(Environment.NewLine, lines.Select((line, index) => string.IsNullOrWhiteSpace(line) ? "" : $"{(index > 0 || !firstIsEOLComment ? forwardSpacing : "")}{(autoTrim ? index == 0 && firstIsEOLComment ? line.Replace("\t", $"{Spacing(indentLevel: 1)}") : line.Trim() : line)}"));
        }

        protected bool LineTerminatorAhead(ParserRuleContext context, int tokenOffset = 0)
        {
            int tokenIndex = context.Stop.TokenIndex + tokenOffset;
            IList<IToken> hiddenChannel = TokenStream.GetHiddenTokensToRight(tokenIndex, TokenConstants.HiddenChannel);

            if (hiddenChannel?.Count > 0)
            {
                foreach (IToken token in hiddenChannel)
                {
                    if (token.Text.IndexOf('\n') > -1)
                        return true;
                }
            }

            IList<IToken> lineCommentChannel = TokenStream.GetHiddenTokensToRight(tokenIndex, GolangLexer.LineCommentChannel);

            if (lineCommentChannel?.Count > 0)
            {
                foreach (IToken token in lineCommentChannel)
                {
                    if (token.Text.IndexOf('\n') > -1)
                        return true;
                }
            }

            return false;
        }

        protected string CheckForBodyCommentsLeft(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1, bool preserveLineFeeds = true)
        {
            return FixForwardSpacing(CheckForCommentsLeft(context, offsetLevel, indentLevel, preserveLineFeeds), offsetLevel, indentLevel, firstIsEOLComment: IsEndOfLineCommentLeft(context));
        }

        protected string CheckForBodyCommentsRight(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1, bool preserveLineFeeds = true)
        {
            return FixForwardSpacing(CheckForCommentsRight(context, offsetLevel, indentLevel, preserveLineFeeds), offsetLevel, indentLevel, firstIsEOLComment: IsEndOfLineCommentRight(context));
        }

        protected string CheckForCommentsLeft(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1, bool preserveLineFeeds = false)
        {
            if (indentLevel < 0)
                indentLevel = IndentLevel;

            indentLevel += offsetLevel;

            return CheckForComments(indentLevel, context.Start.TokenIndex, TokenStream.GetHiddenTokensToLeft, preserveLineFeeds);
        }

        protected string CheckForCommentsRight(ParserRuleContext context, int offsetLevel = 0, int indentLevel = -1, bool preserveLineFeeds = false)
        {
            if (indentLevel < 0)
                indentLevel = IndentLevel;

            indentLevel += offsetLevel;

            return CheckForComments(indentLevel, context.Stop.TokenIndex, TokenStream.GetHiddenTokensToRight, preserveLineFeeds);
        }

        protected string CheckForEndOfLineComment(ParserRuleContext context)
        {
            StringBuilder comments = new StringBuilder();
            IList<IToken> lineCommentChannel = TokenStream.GetHiddenTokensToRight(context.Stop.TokenIndex, GolangLexer.LineCommentChannel);

            if (lineCommentChannel?.Count > 0)
            {
                IToken token = lineCommentChannel[0];
                string commentText = token.Text;

                if (commentText.Trim().StartsWith("//"))
                {
                    if (!CommentOnNewLine(TokenStream.GetHiddenTokensToLeft(token.TokenIndex), token))
                    {
                        string[] lines = commentText.Split(NewLineDelimeters, StringSplitOptions.RemoveEmptyEntries);

                        if (lines.Length > 0)
                            comments.Append(lines[0]);
                    }
                }
            }

            return comments.ToString();
        }

        protected bool IsEndOfLineCommentLeft(ParserRuleContext context)
        {
            return IsEndOfLineComment(context.Start.TokenIndex, TokenStream.GetHiddenTokensToLeft);
        }

        protected bool IsEndOfLineCommentRight(ParserRuleContext context)
        {
            return IsEndOfLineComment(context.Stop.TokenIndex, TokenStream.GetHiddenTokensToRight);
        }

        private bool IsEndOfLineComment(int tokenIndex, Func<int, int, IList<IToken>> getHiddenTokens)
        {
            IList<IToken> lineCommentChannel = getHiddenTokens(tokenIndex, GolangLexer.LineCommentChannel);

            if (lineCommentChannel?.Count > 0)
            {
                IToken token = lineCommentChannel[0];
                string commentText = token.Text;

                if (commentText.Trim().StartsWith("//"))
                    return !CommentOnNewLine(TokenStream.GetHiddenTokensToLeft(token.TokenIndex), token);
            }

            return false;
        }

        private string CheckForComments(int indentLevel, int tokenIndex, Func<int, int, IList<IToken>> getHiddenTokens, bool preserveLineFeeds)
        {
            StringBuilder comments = new StringBuilder();
            StringBuilder lineFeeds = new StringBuilder();
            IList<IToken> hiddenChannel = getHiddenTokens(tokenIndex, TokenConstants.HiddenChannel);
            IList<IToken> lineCommentChannel = getHiddenTokens(tokenIndex, GolangLexer.LineCommentChannel);

            WroteLineFeed = false;

            // TODO: Sort both channels together by token index

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

                        if (!WroteLineFeed)
                            WroteLineFeed = EndsWithLineFeed(hiddenText);
                    }
                    else if (preserveLineFeeds)
                    {
                        hiddenText = PreserveOnlyLineFeeds(hiddenText);

                        if (hiddenText.Length > 0)
                        {
                            lineFeeds.Append(hiddenText);

                            if (!WroteLineFeed)
                                WroteLineFeed = EndsWithLineFeed(hiddenText);
                        }
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
                            comments.Append(FixForwardSpacing(commentText, 0, indentLevel, false, true));
                        else
                            comments.Append(commentText);

                        WroteLineFeed = true;
                    }
                    else if (preserveLineFeeds && lineFeeds.Length == 0)
                    {
                        commentText = PreserveOnlyLineFeeds(commentText);

                        if (commentText.Length > 0)
                        {
                            comments.Append(commentText);

                            if (!WroteLineFeed)
                                WroteLineFeed = EndsWithLineFeed(commentText);
                        }
                    }
                }
            }

            comments.Insert(0, lineFeeds);

            return comments.ToString();
        }

        protected bool EndsWithLineFeed(string line)
        {
            int lastLineFeed = line.LastIndexOf('\n');

            if (lastLineFeed == -1)
                return false;

            if (lastLineFeed == line.Length - 1)
                return true;

            if (line.Substring(lastLineFeed + 1).Trim().Length == 0)
                return true;

            return false;
        }

        private bool CommentOnNewLine(IList<IToken> hiddenChannel, IToken testToken)
        {
            IToken priorToken = hiddenChannel?.FirstOrDefault(token => token.StopIndex == testToken.StartIndex - 1);
            return priorToken != null && EndsWithLineFeed(priorToken.Text);
        }

        protected string PreserveOnlyLineFeeds(string line)
        {
            return new string(Array.FindAll(line.ToCharArray(), c => c == '\r' || c == '\n'));
        }

        protected string RemoveLastLineFeed(string line)
        {
            int index = line.LastIndexOf("\r\n", StringComparison.Ordinal);

            if (index == -1)
                index = line.LastIndexOf('\n');

            if (index > -1)
                line = line.Substring(0, index);

            return line;
        }
    }
}
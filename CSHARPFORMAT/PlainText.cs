using System;
using System.Collections.Generic;
using System.Text;

namespace Manoli.Utils.CSharpFormat
{
    /// <summary>
    /// Generates color-coded HTML 4.01 from Visual Basic source code.
    /// </summary>
    public class PlainTextFormat : CodeFormat
    {
         /// <summary>
        /// Determines if the language is case sensitive.
        /// </summary>
        /// <value>Always <b>true</b>, since VB is not case sensitive.</value>
        public override bool CaseSensitive
        {
            get { return false; }
        }

        /// <summary>
        /// Regular expression string to match comments (' and REM). 
        /// </summary>
        protected override string CommentRegEx
        {
            get { return @""; }
        }

        /// <summary>
        /// Regular expression string to match string and character literals. 
        /// </summary>
        protected override string StringRegEx
        {
            get { return ""; }
        }

        /// <summary>
        /// The list of VB keywords.
        /// </summary>
        protected override string Keywords
        {
            get
            {
                return "";
            }

        }

        /// <summary>
        /// The list of VB preprocessors.
        /// </summary>
        protected override string Preprocessors
        {
            get
            {
                return "";
            }
        }

        //does the formatting job
        private string FormatCode(string source, bool lineNumbers,
            bool alternate, bool embedStyleSheet, bool subCode)
        {
            //replace special characters
            StringBuilder sb = new StringBuilder(source);

            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            sb.Replace("\t", string.Empty.PadRight(this.TabSpaces));

            source = sb.ToString();
            
            sb = new StringBuilder();

            if (embedStyleSheet)
            {
                sb.Append("<style type=\"text/css\">\n");
                sb.Append(GetCssString());
                sb.Append("</style>\n");
            }

            //have to use a <pre> because IE below ver 6 does not understand 
            //the "white-space: pre" CSS value
            if (!subCode)
                sb.Append("<pre class=\"csharpcode\">");
            sb.Append(source);
            if (!subCode)
                sb.Append("</pre>");

            return sb.ToString();
        }
    }
}

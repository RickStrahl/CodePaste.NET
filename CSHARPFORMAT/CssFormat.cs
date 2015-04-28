#region Copyright © 2001-2003 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */ 
#endregion

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Manoli.Utils.CSharpFormat
{
	/// <summary>
	/// Generates color-coded HTML 4.01 from HTML/XML/ASPX source code.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This implementation assumes that code inside &lt;script&gt; blocks 
	/// is JavaScript, and code inside &lt;% %&gt; blocks is C#.</para>
	/// <para>
	/// The default tab width is set to 2 characters in this class.</para>
	/// </remarks>
	public class CssFormat : SourceFormat
	{

		/// <summary/>
		public CssFormat()
		{
            const string regComment = @"/\*.*?\*/";
            const string regNames = @"^.*?\{";
            const string regAttributes = @"[\w|-]*?:";
            const string regCloseTag = @"}";
            const string regValues = @"([\w|\S]*?;)";
			
			//the regex object will handle all the replacements in one pass
            string regAll = "(" + regComment + ")|(" + regNames + ")|(" + regAttributes + ")|(" + regCloseTag + ")|(" + regValues + ")";
            Debug.WriteLine(regAll);

            CodeRegex = new Regex(regAll, RegexOptions.Singleline | RegexOptions.Multiline);

		}

		/// <summary>
		/// Called to evaluate the HTML fragment corresponding to each 
		/// attribute's name/value in the code.
		/// </summary>
		/// <param name="match">The <see cref="Match"/> resulting from a 
		/// single regular expression match.</param>
		/// <returns>A string containing the HTML code fragment.</returns>
		protected override string MatchEval(Match match)
		{
            if (match.Groups[1].Success) //attribute value
                return "<span class=\"rem\">" + match.ToString() + "</span>";


			if(match.Groups[2].Success) //attribute value
				return "<span class=\"html\">" + match.ToString() + "</span>";

			if(match.Groups[3].Success) //attribute name
				return "<span class=\"attr\">" + match.ToString() + "</span>";

            if (match.Groups[4].Success) //close tag
                return "<span class=\"html\">" + match.ToString() + "</span>";

            if (match.Groups[5].Success) //values
                return "<span class=\"kwrd\">" + match.ToString().Replace(";","") + "</span>;";


            return match.ToString();
		}

	}
}


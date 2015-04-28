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

namespace Manoli.Utils.CSharpFormat
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// Generates color-coded HTML 4.01 from C# source code.
	/// </summary>
	public class JavaFormat : CLikeFormat
	{                
		/// <summary>
		/// The list of C# keywords.
		/// </summary>
		protected override string Keywords 
		{
			get 
			{
                return                    
@"abstract assert boolean break byte case catch char class const
continue default do double else enum extends
false final finally float for goto if implements import
instanceof int interface long native new null
package private protected public return
short static strictfp super switch synchronized this throw throws true
transient try void volatile while";			
            }
		}

		/// <summary>
		/// The list of C# preprocessors.
		/// </summary>
		protected override string Preprocessors
		{
			get 
			{ 
				return "";
			}
		}
	}

  
}


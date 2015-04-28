
namespace Manoli.Utils.CSharpFormat
{
	/// <summary>
	/// Generates color-coded HTML 4.01 from Microsoft Windows Command Shell source code (i.e. batch files).
	/// </summary>
	public class MSCmdShellFormat : CodeFormat
	{
        /// <summary>
        /// Deterimines if the language being formatted is case sensitive or not.
        /// The command shell is not case sensitive, so will always return false.
        /// </summary>
        public override bool CaseSensitive
        {
            get { return false; }
        }
        
		/// <summary>
		/// Regular expression string to match single line comments (rem).
		/// </summary>
		protected override string CommentRegEx
		{
			get { return @"rem.*?(?=\r|\n)"; }
		}

		/// <summary>
		/// Regular expression string to match string and character literals. 
		/// </summary>
		protected override string StringRegEx
		{
			get { return @"@?""""|@?"".*?(?!\\).""|''|'.*?(?!\\).'"; }
		}

		/// <summary>
		/// The list of Command Shell keywords.
		/// </summary>
		protected override string Keywords 
		{
			get 
			{ 
				return "assoc attrib break bcdedit cacls call cd chcp chdir"
                    + " chkdsk chkntfs cls cmd color comp compact convert"
                    + " copy date del dir diskcomp diskcopy diskpart doskey"
                    + " driverquery echo endlocal erase exit fc find findstr"
                    + " for format fsutil ftype goto gpresult graftabl help"
                    + " icacls if label md mkdir mklink mode more move"
                    + " openfiles path pause popd print prompt pushd rd"
                    + " recover rem ren rename replace rmdir robocopy set"
                    + " setlocal sc schtasks shift shutdown sort start"
                    + " subst systeminfo tasklist taskkill time title tree"
                    + " type ver verify vol xcopy wmic";
			}
		}

		/// <summary>
		/// Use preprocessors property to hilight operators.
		/// </summary>
		protected override string Preprocessors
		{
			get
			{
                // TODO: the code below doesn't work for parms and 'expression values'

                // TODO: not sure how to match the following without defining a new regex
                // SEE: http://ss64.com/nt/syntax-args.html for more details on these parameter extensions
                // %~[0123456789]   // could just write these out %~1 %~2 %~3 etc.
                // %~[fdpnxsatz|{or any combinations][0123456789]
                // %~$PATH:[0123456789]
                return @"";
                //+" | > < == equ neq lss leq gtr geq"
                //+ " defined errorlevel else exist in not off on";

                //  @"%1 %2 %3 %4 %5 %6 %7 %8 %9 %*";
			}
		}
	}
}

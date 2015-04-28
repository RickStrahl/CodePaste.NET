
namespace Manoli.Utils.CSharpFormat
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Generates color-coded HTML 4.01 from C# source code.
    /// </summary>
    public class CodeFormatFactory 
    {
        /// <summary>
        /// Creates an instance of a formatter for a given language
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public static SourceFormat Create(SourceLanguages Language)
        {
            if (Language == SourceLanguages.CSharp)
                return new CSharpFormat();
            if (Language == SourceLanguages.Html)
                return new HtmlFormat();
            if (Language == SourceLanguages.VisualBasic)
                return new VisualBasicFormat();
            if (Language == SourceLanguages.FoxPro)
                return new FoxProFormat();
            if (Language == SourceLanguages.TSql)
                return new TsqlFormat();
            if (Language == SourceLanguages.JavaScript)
                return new JavaScriptFormat();
            if (Language == SourceLanguages.Monad)
                return new MshFormat();
            if (Language == SourceLanguages.Xml)
                return new XmlFormat();
            if (Language == SourceLanguages.Css)
                return new CssFormat();
            if ((Language == SourceLanguages.Java))
                return new JavaFormat();
            if ((Language == SourceLanguages.CPlusPlus))
                return new CPlusPlusFormat();
            if (Language == SourceLanguages.HtmlPhp)
                return new HtmlPhpFormat();
            if (Language == SourceLanguages.Php)
                return new PhpFormat();
            if (Language == SourceLanguages.MSCmdShell)
                return new MSCmdShellFormat();
            else
                return new PlainTextFormat();
        }

        public static SourceFormat Create(string language)
        {
            if (language == null)
                language = string.Empty;

            language = language.ToLower();

            return CodeFormatFactory.Create( GetLanguageFromString(language));
        }

        public static SourceLanguages GetLanguageFromString(string language)
        {
            if (language == "c#" || language == "csharp" || language== "cs")
                return SourceLanguages.CSharp;
            else if (language == "html" || language == "htm" || language == "aspx")
                return SourceLanguages.Html;
            else if (language == "css")
                return SourceLanguages.Css;
            else if (language == "xml")
                return SourceLanguages.Xml;
            else if (language == "javascript" || language == "jscript" || language == "js")
                return SourceLanguages.JavaScript;
            else if (language == "vb" || language == "vb.net" ||
                     language == "visualbasic" ||
                     language == "visual basic")
                return SourceLanguages.VisualBasic;
            else if (language == "sql" || language == "tsql")
                return SourceLanguages.TSql;
            else if (language == "fox" || language == "vfp" || language == "foxpro" || language == "prg")
                return SourceLanguages.FoxPro;
            else if ((language == "msh" || language == "powershell"))
                return SourceLanguages.Monad;
            else if ((language == "java"))
                return SourceLanguages.Java;
            else if (language == "htmlphp")
                return SourceLanguages.HtmlPhp;
            else if ((language == "php"))
                return SourceLanguages.Php;
            else if (language == "c++" || language.StartsWith("cplus") || language == "cpp")
                return SourceLanguages.CPlusPlus;
            else if (language == "cmd" || language == "bat" || language == "dos" || language == "mscmdshell")
                return SourceLanguages.MSCmdShell;

            return SourceLanguages.PlainText;
        }


        public static string GetStringLanguageFromExtension(string extension)
        {
            if (extension.StartsWith(".") && extension.Length > 2)
                extension = extension.Substring(1);

            if (extension == "c#" || extension == "csharp" || extension == "cs")
                return "C#";
            else if (extension == "html" || extension == "htm" || extension == "aspx")
                return "HTML";
            else if (extension == "css")
                return "CSS";
            else if (extension == "xml")
                return "XML";
            else if (extension == "javascript" || extension == "jscript" || extension == "js")
                return "JavaScript";
            else if (extension == "vb" || extension == "vb.net" ||
                     extension == "visualbasic" ||
                     extension == "visual basic")
                return "VB.NET";
            else if (extension == "sql" || extension == "tsql")
                return "SQL";
            else if (extension == "fox" || extension == "vfp" || extension == "foxpro" || extension == "prg")
                return "FoxPro";
            else if ((extension == "msh" || extension == "powershell"))
                return "PowerShell";
            else if ((extension == "java"))
                return "JAVA";
            else if (extension == "htmlphp")
                return "HtmlPhp";
            else if ((extension == "php"))
                return "PHP";
            else if (extension == "c++" || extension.StartsWith("cplus") || extension == "cpp")
                return "C++";
            else if (extension == "bat" || extension == "cmd")
                return "MSCmdShell";

            return "No Format";                
        }
    }

    public enum SourceLanguages
    {
        CSharp,
        Html,
        VisualBasic,
        TSql,
        JavaScript,
        FoxPro,
        Monad,
        PlainText,
        Undefined,
        Css,
        Xml,
        Java,
        CPlusPlus,
        HtmlPhp,
        Php,
        MSCmdShell,
    }
}


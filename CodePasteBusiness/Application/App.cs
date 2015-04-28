using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CodePasteBusiness
{
    public class App
    {
        public static ApplicationConfiguration Configuration;
        public static ApplicationSecrets Secrets;

        ///// <summary>
        ///// Languages supported in language drop down 
        ///// </summary>        
        public static Dictionary<string, string> CodeLanguages = new Dictionary<string, string>
                {
                    { "NoFormat", "No code formatting"},    
                    { "C#", "C#" },
                    { "VB.NET", "Visual Basic .NET" },
                    { "HTML","HTML, ASP.NET, JavaScript"},                    
                    { "JavaScript", "JavaScript" },		            
                    { "CSS", "CSS"},
                    { "XML", "XML"},
                    { "SQL", "SQL and TSQL"},
                    { "PowerShell","Power Shell (Monad)" },
                    { "FoxPro", "FoxPro" },
                    { "Java", "Java" },
                    { "HtmlPhp", "Php, Html, JavaScript" },
                    { "C++", "C++" },
                    { "MSCmdSHell", "MS Batch Files"}
                };

        /// <summary>
        /// Postfix attached as static salt to the end of a password hash
        /// (in addition to the dynamic hash)
        /// </summary>
        public static string PasswordEncodingPostfix = "|~";

        static App()
        {
            Configuration = new ApplicationConfiguration();
            Configuration.Initialize();

            Secrets = new ApplicationSecrets();
            Secrets.Initialize();
        }

        /// <summary>
        /// Returns an hashed and salted password.
        /// 
        /// Encoded Passwords end in || to indicate that they are 
        /// encoded so that bus objects can validate values.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="uniqueSalt">Unique per instance salt - use id</param>
        /// <param name="appSalt">Optional salt added to the encrypted value.</param>
        /// <returns></returns>
        public static string EncodePassword(string password, string uniqueSalt,
                                            string appSalt = "$!23@1f2c9d3432!@")
        {
            // don't allow empty password
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            string s2 = appSalt;  // app specific salt
            string s3 = uniqueSalt + password + s2;

            var sha = new SHA1CryptoServiceProvider();
            byte[] Hash = sha.ComputeHash(Encoding.ASCII.GetBytes(s3));

            var sha2 = new SHA256CryptoServiceProvider();
            Hash = sha2.ComputeHash(Hash);

            return Convert.ToBase64String(Hash).Replace("==", "") +
                // add a marker so we know whether a password is encoded 
                App.PasswordEncodingPostfix;
        }
    }
}

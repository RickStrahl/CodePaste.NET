using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodePasteBusiness
{
    public class CodePasteFactory
    {
        /// <summary>
        /// Return an instance of a CodeSnippet Business object
        /// </summary>
        /// <returns></returns>
        public static busCodeSnippet GetCodeSnippet()
        {
            return new busCodeSnippet();
        }

        /// <summary>
        /// Return an instance of User Business object
        /// </summary>
        /// <returns></returns>
        public static busUser GetUser()
        {
            return new busUser();
        }

        public static busUserToken GetUserToken()
        {
            return new busUserToken();
        }


        public static busComment GetComment()
        {
            return new busComment();
        }

        public static busAdministration GetAdministration()
        {
            return new busAdministration();
        }
    }
}

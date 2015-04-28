using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Westwind.Web
{
    /// <summary>
    /// Implements the Gravatar API for retrieving a Gravatar image to display
    /// </summary>
    public class Gravatar
    {
        public const string GravatarBaseUrl = "http://www.gravatar.com/avatar.php";

        /// <summary>
        /// Returns a Gravatar URL only
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Size"></param>
        /// <param name="Rating"></param>
        public static string GetGravatarLink(string Email, int Size, string Rating, string DefaultImageUrl)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] Hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Email));

            StringBuilder sb = new System.Text.StringBuilder();
            for (int x = 0; x < Hash.Length; x++)
            {
                sb.Append(Hash[x].ToString("x2"));
            }

            if (!string.IsNullOrEmpty(DefaultImageUrl))
                DefaultImageUrl = "&default=" + DefaultImageUrl;
            else
                DefaultImageUrl = "";

            return string.Format("{0}?gravatar_id={1}&size={2}&rating={3}{4}",
                                   Gravatar.GravatarBaseUrl, sb.ToString(), Size, Rating, DefaultImageUrl);
        }

        /// <summary>
        /// Returns a Gravatar Image Tag that can be directly embedded into
        /// an HTML document.
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Size"></param>
        /// <param name="Rating"></param>
        /// <param name="ExtraImageAtributes"></param>
        /// <returns></returns>
        public static string GetGravatarImage(string Email, int Size, string Rating,                                               
                                              string ExtraImageAttributes,string DefaultImageUrl)
        {
            string Url = GetGravatarLink(Email, Size, Rating, DefaultImageUrl);
            return string.Format("<img src='{0}' {1}>",Url,ExtraImageAttributes,DefaultImageUrl);
        }


        /// <summary>
        /// Return a full Html img tag for a GravatarBaseUrl
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="ExtraImageAttributes"></param>
        /// <returns></returns>
        public static string GetGravatarImage(string Email, string ExtraImageAttributes)
        {
            return GetGravatarImage(Email, 80, "G", ExtraImageAttributes, string.Empty);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodePasteBusiness;
using Westwind.Web;

namespace CodePasteMvc
{

    public class AppUserState 
    {
        public string Name = string.Empty;
        public string Email = string.Empty;       
        public string Theme = "visualstudio";
        public string UserId = string.Empty;
        public bool IsAdmin = false;
        

        /// <summary>
        /// Exports a short string list of Id, Email, Name separated by |
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join("|", new string[] { this.UserId, this.Name, this.IsAdmin.ToString(), this.Theme });
        }
        

        /// <summary>
        /// Imports Id, Email and Name from a | separated string
        /// </summary>
        /// <param name="itemString"></param>
        public bool FromString(string itemString)
        {
            if (string.IsNullOrEmpty(itemString))
                return false;

            string[] strings = itemString.Split('|');
            if (strings.Length < 3)
                return false;

            UserId = strings[0];
            Name = strings[1];
            IsAdmin = strings[2] == "True";
            if(strings.Length > 3)
                Theme = strings[3];

            return true;
        }

        /// <summary>
        /// Populates the AppUserState properties from a
        /// User instance
        /// </summary>
        /// <param name="user"></param>
        public void FromUser(User user)
        {
            UserId = user.Id;
            Name = user.Name;
            Email = user.Email;
            IsAdmin = user.IsAdmin;
            Theme = user.Theme;
        }

        /// <summary>
        /// Determines if the user is logged in
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            if (string.IsNullOrEmpty(this.UserId) || string.IsNullOrEmpty(this.Name))
                return true;

            return false;
        }
    }
}

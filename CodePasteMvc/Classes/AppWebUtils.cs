using System;
using System.Collections.Generic;
using System.Linq;
using CodePasteBusiness;

using System.Text;
using Westwind.Web;
using System.Web.Mvc;
using Westwind.Utilities;
using Westwind.Utilities.Logging;
using System.Collections;
using Westwind.Utilities.InternetTools;

namespace CodePasteMvc
{
    public class AppWebUtils
    {
        /// <summary>
        /// Returns a list of SelectListItems from 
        /// an Enum's string values. Useful for quickly filling
        /// lists.
        /// </summary>
        /// <param name="enumType">Type of an Enum</param>
        /// <param name="defaultValue">Optional default value as a string that is to be selected</param>
        /// <returns></returns>
        public static List<SelectListItem> GetSelectListFromEnum(Type enumType, string defaultValue) 
        {
           return (from modes in Enum.GetNames(enumType)
             select new SelectListItem
             {
                 Text = modes,
                 Value = modes,
                 Selected = (defaultValue != null && modes == defaultValue)
             }).ToList();
        }

        

        /// <summary>
        /// Returns a list of SelectListItems from 
        /// an Enum's string values. Useful for quickly filling
        /// lists.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSelectListFromEnum(Type enumType)
        {
            return GetSelectListFromEnum(enumType, null);
        }


        /// <summary>
        /// Gets a select list from a Dictionary<string,string> mapping
        /// a key to the value and the value to the text string.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetSelectListFromDictionary(IDictionary<string, string> dict, 
                                                                       string defaultValue)
        {
            return (from item in dict
                    select new SelectListItem
                    {
                        Text = item.Value,
                        Value = item.Key,
                        Selected = defaultValue != null && item.Key == defaultValue
                    }).ToList();
        }

        /// <summary>
        /// Gets a select list from a Dictionary<string,string> mapping
        /// a key to the value and the value to the text string.
        /// </summary>
        /// <param name="dict"></param>
        public static List<SelectListItem> GetSelectListFromDictionary(IDictionary<string, string> dict)
        {
            return GetSelectListFromDictionary(dict, null);
        }


        /// <summary>
        /// Creates a GravatarLink for a snippet based on the user Email address.
        /// </summary>
        /// <param name="snippet"></param>
        /// <returns></returns>
        public static string GravatarLink(CodeSnippet snippet)
        {
            if (snippet.User == null)
                return string.Empty;

            string DefaultImg = WebUtils.ResolveServerUrl("~/images/space.gif");
            return Gravatar.GetGravatarImage(snippet.User.Email, 70, "R", 
                   "style='opacity:.75;filter:alpha(opacity=\"75\");margin: 0 0 0 5px;' align='right' hspace='5'", 
                   DefaultImg);
        }


        /// <summary>
        /// Sends email using the WebStoreConfig Email Configuration defaults.
        /// 
        /// <seealso>Class WebUtils</seealso>
        /// </summary>
        /// <param name="Subject">
        /// The subject of the message.
        /// </param>
        /// <param name="Message">
        /// The body of the message.
        /// </param>
        /// <param name="Recipient">
        /// The recipient as an email address.
        /// </param>
        /// <param name="SendName">
        /// Name descriptive of the sender.
        /// </param>
        /// <param name="SendEmail">
        /// The email address of the sender.
        /// </param>
        /// <param name="NoAsync">
        /// Whether this message is send asynchronously or not.
        /// </param>
        /// <param name="Boolean SendAsText">
        /// Determines whether the message is sent as Text or HTML.
        /// </param>
        /// <returns>Boolean</returns>
        public static bool SendEmail(string Subject, string Message, string Recipient, string SendName,
                                     string SendEmail, bool NoAsync, bool SendAsText, delSmtpNativeEvent OnErrorHandler)
        {
            try
            {
                SmtpClientNative Email = new SmtpClientNative();
                Email.MailServer = App.Configuration.MailServer;
                if (!string.IsNullOrEmpty(App.Configuration.MailServerUsername))
                {
                    Email.Username = App.Configuration.MailServerUsername;
                    Email.Password = App.Configuration.MailServerPassword;
                }
                Email.Recipient = Recipient;

                if (SendAsText)
                    Email.ContentType = "text/plain";
                else
                {
                    Email.ContentType = "text/html; charset=utf-8";
                    Email.Encoding = Encoding.UTF8;
                }


                if (SendName == null || SendName == "")
                {
                    Email.SenderEmail = App.Configuration.SenderEmailAddress;
                    Email.SenderName = App.Configuration.SenderEmailName;
                }
                else
                {
                    Email.SenderEmail = SendEmail;
                    Email.SenderName = SendName;
                }

                Email.Subject = Subject;
                Email.Message = Message;

                // *** Capture any error messages and log them
                if (OnErrorHandler == null)
                    Email.SendError += OnSmtpError;
                else
                    Email.SendError += OnErrorHandler;

                if (NoAsync)
                    return Email.SendMail();
                else
                    Email.SendMailAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sends email using the WebStoreConfig Email Configuration defaults.
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Message"></param>
        /// <param name="Recipient"></param>
        /// <returns></returns>
        public static bool SendEmail(string Subject, string Message, string Recipient)
        {
            return AppWebUtils.SendEmail(Subject, Message, Recipient, null, null, false, false, null);
        }

        /// <summary>
        /// Sends email using the WebStoreConfig Email Configuration defaults.
        /// <seealso>Class WebUtils</seealso>
        /// </summary>
        /// <param name="Subject">
        /// The subject of the message.
        /// </param>
        /// <param name="Message">
        /// The body of the message.
        /// </param>
        /// <param name="Recipient">
        /// The recipient as an email address.
        /// </param>
        /// <param name="Boolean SendAsText">
        /// Determines whether the message is sent as Text or HTML.
        /// </param>
        /// <returns>Boolean</returns>
        public static bool SendEmail(string Subject, string Message, string Recipient, bool SendAsText)
        {
            return AppWebUtils.SendEmail(Subject, Message, Recipient, null, null, false, SendAsText, null);
        }

        /// <summary>
        /// Sends an Admin email with the Admin email name and email address from Web Store Config
        /// </summary>
        /// <param name="Subject"></param>
        /// <param name="Message"></param>
        /// <param name="Recipient"></param>
        /// <returns></returns>
        public static bool SendAdminEmail(string Subject, string Message, string ccList, string emailAddress = null)
        {
            if (string.IsNullOrEmpty(emailAddress))
                emailAddress = App.Configuration.AdminEmailAddress;

            try
            {
                SmtpClientNative Email = new SmtpClientNative();
                Email.MailServer = App.Configuration.MailServer;
                Email.Recipient = emailAddress;

                Email.SenderEmail = App.Configuration.AdminEmailAddress;
                Email.SenderName = App.Configuration.AdminEmailName;
                Email.ContentType = "text/plain";


                Email.Subject = Subject;
                Email.Message = Message;

                // *** Capture errors so we can log them
                Email.SendError += OnSmtpError;

                Email.SendMailAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static void OnSmtpError(SmtpClientNative Smtp)
        {
            string ErrorMessage = "Error sending Email: " + Smtp.ErrorMessage + "\r\n" +
                       Smtp.Subject + "\r\nTo: " +
                       Smtp.Recipient + "\r\nServer: " + Smtp.MailServer;

            // *** Log into SQL Error Log
            LogManager.Current.WriteEntry(
                new WebLogEntry
                {
                    Message = "Email Sending Error",
                    Details = ErrorMessage,
                    ErrorLevel = ErrorLevels.Error
                }
            );
        }


    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Westwind.Utilities.Configuration;

namespace CodePasteBusiness
{

    /// <summary>
    /// Class that holds CodePaste specific configuration settings.
    /// 
    /// Pe
    /// </summary>
    public class ApplicationConfiguration : Westwind.Utilities.Configuration.AppConfiguration
    {

        /// <summary>
        /// Allow for provider instantiation without any
        /// behavior.
        /// 
        /// REQUIRED!
        /// </summary>
        public ApplicationConfiguration()
        { 
        }


        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new ConfigurationFileConfigurationProvider<ApplicationConfiguration>()
            {
                ConfigurationSection = "CodePaste",
                //ConfigurationFile = "CodePaste.config"
            };

            return provider;            
        }
        
        /// <summary>
        /// Title of the application
        /// </summary>
        public string ApplicationTitle
        {
            get { return _ApplicationTitle; }
            set { _ApplicationTitle = value; }
        }
        private string _ApplicationTitle = "CodePaste.NET - paste and link .NET code";


        /// <summary>
        /// The maximum number of items to display in listing pages
        /// </summary>
        public int MaxListDisplayCount
        {
            get { return _MaxListDisplayCount; }
            set { _MaxListDisplayCount = value; }
        }
        private int _MaxListDisplayCount = 25;

        public int MaxCodeLength
        {
            get { return _MaxCodeLength; }
            set { _MaxCodeLength = value; }
        }
        private int _MaxCodeLength = 100000;

        public string DefaultTheme
        {
            get { return _DefaultTheme; }
            set { _DefaultTheme = value; }
        }
        private string _DefaultTheme = "visualstudio";
        
        
        /// <summary>
        /// Determines how errors are handled
        /// </summary>
        public DebugModes DebugMode
        {
            get { return _DebugMode; }
            set { _DebugMode = value; }
        }
        private DebugModes _DebugMode = DebugModes.Default;


        /// <summary>
        /// The days after which an anonymous snippet is deleted
        /// </summary>
        public int HoursToDeleteAnonymousSnippets
        {
            get { return _hoursToDeleteAnonymousSnippets; }
            set { _hoursToDeleteAnonymousSnippets = value; }
        }
        private int _hoursToDeleteAnonymousSnippets = 1;

        /// <summary>
        /// The minimum number of Views to keep a snippet
        /// from not getting deleted as anonymous
        /// </summary>
        public int MinViewBeforeDeleteAnonymousSnippets
        {
            get { return _MinViewBeforeDeleteAnonymousSnippets; }
            set { _MinViewBeforeDeleteAnonymousSnippets = value; }
        }
        private int _MinViewBeforeDeleteAnonymousSnippets = 30;



 #region Email Settings

        /// <summary>
        /// The Email Address used to send out emails
        /// </summary>
        public string SenderEmailAddress
        {
            get { return _SenderEmailAddress; }
            set { _SenderEmailAddress = value; }
        }
        private string _SenderEmailAddress = "";

        /// <summary>
        /// The Name of the senders Email 
        /// </summary>
        public string SenderEmailName
        {
            get { return _SenderEmailName; }
            set { _SenderEmailName = value; }
        }
        private string _SenderEmailName = "";

        /// <summary>
        /// Email address use for admin emails
        /// </summary>
        public string AdminEmailAddress
        {
            get { return _AdminEmailAddress; }
            set { _AdminEmailAddress = value; }
        }
        private string _AdminEmailAddress = "";

        /// <summary>
        /// Email name used for Admin emails
        /// </summary>
        public string AdminEmailName
        {
            get { return _AdminEmailName; }
            set { _AdminEmailName = value; }
        }
        private string _AdminEmailName = "";

        /// <summary>
        /// Email address used for error emails
        /// </summary>
        public string ErrorEmailAddress
        {
            get { return _ErrorEmailAddress; }
            set { _ErrorEmailAddress = value; }
        }
        private string _ErrorEmailAddress = string.Empty;
        
        public string MailServer
        {
            get { return _MailServer; }
            set { _MailServer = value; }
        }
        private string _MailServer = "";


        /// <summary>
        /// Mail Server username if required
        /// </summary>
        public string MailServerUsername
        {
            get { return _MailServerUsername; }
            set { _MailServerUsername = value; }
        }
        private string _MailServerUsername = "";


        /// <summary>
        /// Mail Server password if required
        /// </summary>
        public string MailServerPassword
        {
            get { return _MailServerPassword; }
            set { _MailServerPassword = value; }
        }
        private string _MailServerPassword = "";

        #endregion  
        
    }

    public class ApplicationSecrets : AppConfiguration
    {
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }

        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }

        public string GitHubClientId { get; set; }
        public string GitHubClientSecret { get; set; }

        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            // force JSON.NET to load if not loaded already
            string t = JsonConvert.SerializeObject("X");

            var provider = new  JsonFileConfigurationProvider<ApplicationSecrets>()
            {
                 JsonConfigurationFile= Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"CodePasteKeys.json"),
                 // don't really need to encrypt - just keep out of source control
                 //EncryptionKey = "CodePaste|App:Secrets@",
                 //PropertiesToEncrypt = "GoogleClientId,GoggleClientSecret,TwitterConsumerKey,TwitterConsumerSecret,GitHubClientId,GitHubClientSecret"
            };

            return provider;
        }

    }

    /// <summary>
    /// Different modes that errors are displayed in the application
    /// </summary>
    public enum DebugModes
    {
        Default,
        ApplicationErrorMessage,
        DeveloperErrorMessage
    }
}

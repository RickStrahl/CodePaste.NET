using System;
using System.Web;
using System.Web.Mvc;
using CodePasteBusiness;
using Westwind.Web.Mvc;
using Westwind.Utilities;
using System.Text;
using System.Collections.Generic;
using System.Web.Routing;

namespace CodePasteMvc.Controllers
{
    public class NewSnippetViewModel
    {
        public CodeSnippet Snippet = null;
        public AppUserState AppUserState = null;
        public string Theme { get; set;  }
    }
}

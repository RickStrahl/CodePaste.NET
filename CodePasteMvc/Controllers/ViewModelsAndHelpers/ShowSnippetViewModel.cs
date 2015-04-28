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
    public class ShowSnippetViewModel
    {
        public ShowSnippetViewModel(SnippetController controller)
        {
            this.Controller = controller;
            Theme = "visualstudio";
        }
        public SnippetController Controller = null;

        public CodeSnippet Snippet = null;
        public bool AllowEdit = false;
        public AppUserState AppUserState = null;
        public string FormattedCode = null;
        public bool IsFavoritedByUser { get; set; }
        public string Theme { get; set;  }
    }
}

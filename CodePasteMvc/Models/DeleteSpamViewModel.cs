using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CodePasteBusiness;

namespace CodePasteMvc
{
    public class DeleteSpamViewModel 
    {
        public string SearchTerm { get; set; }
        public IEnumerable<CodeSnippet> Snippets { get; set; }
        public List<string> SelectedSnippets { get; set;  }

        public DeleteSpamViewModel()
        {
            Snippets = new List<CodeSnippet>();
            SelectedSnippets = new List<string>();
        }
    }
}
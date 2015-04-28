using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CodePasteBusiness
{
    public partial class User
    {
    }

   public partial class CodeSnippet
    {        
        /// <summary>
        /// Explicitly loaded user instance - not part of LINQ to SQL Model
        /// and forced to load manually only if and when needed to avoid lazy load
        /// </summary>
        public User User
        {
            get { return _User; }
            set { _User = value; }
        }
        private User _User = null;

        /// <summary>
        /// Explicitly loaded list of comments. Not part of Linq to SQL model
        /// and forced to load manually only if an when needed to avoid lazy load
        /// </summary>
        public List<Comment> Comments
        {
            get { return _Comments; }
            set { _Comments = value; }
        }
        private List<Comment> _Comments = null;

        public List<AdditionalSnippet> AdditionalSnippets
        {
          get
            { return _AdditionalSnippets; }
          set { _AdditionalSnippets = value; }
        }
        private List<AdditionalSnippet> _AdditionalSnippets = null;
    }
}

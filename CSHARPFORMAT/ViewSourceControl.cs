using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;

[assembly: WebResource("Manoli.Utils.CSharpFormat.csharp.css", "text/css")]

namespace Manoli.Utils.CSharpFormat
{   

    /// <summary>
    /// Simple control that lets you show code in a page.
    /// 
    /// </summary>
    [ToolboxData("<{0}:ViewSourceControl runat=\"server\" />")]
    public class ViewSourceControl : WebControl
    {
        protected Button btnShowCode = null;
        protected string Output = null;

        const string STR_CSHARP_RESOURCE = "Manoli.Utils.CSharpFormat.csharp.css";
        
        [Description("The location of the code file using ~/ url syntax.")]
        [DefaultValue("")]
        public string CodeFile
        {
            get
            {
                return _CodeFile;
            }
            set
            {
                _CodeFile = value;
            }
        }
        private string _CodeFile = "";


        [Description("Determines which mode the control displays either as a button or displaying the code")]
        public DisplayStates DisplayState
        {
            get
            {
                return _DisplayState;
            }
            set
            {
                _DisplayState = value;
            }
        }
        private DisplayStates _DisplayState = DisplayStates.Button;

        [Description("Optional location of the CSS file that formats code. WebResource specifies loading from internal resource.")]
        public string CssLocation
        {
            get { return _CssLocation; }
            set { _CssLocation = value; }
        }
        private string _CssLocation = "WebResource";


        [Description("The button text.")]
        [DefaultValue("Show Code")]
        public string Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
                if (this.btnShowCode != null)
                    this.btnShowCode.Text = value;
            }
        }
        private string _Text = "Show Code";
        

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.btnShowCode = new Button();
            this.btnShowCode.Text = this.Text;
            this.btnShowCode.Click += new EventHandler(this.btnShowCode_Click);
            this.Controls.Add(this.btnShowCode);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        
            this.btnShowCode.Width = this.Width;

            // Add the stylesheet only once
            if (Context.Items["StyleAdded"] == null)
            {
                HtmlLink link = new HtmlLink();
                link.Attributes.Add("type", "text/css");
                link.Attributes.Add("rel", "stylesheet");

                if (string.IsNullOrEmpty(this.CssLocation) || this.CssLocation == "WebResource")
                {
                    // use WebResource
                    string url = this.Page.ClientScript.GetWebResourceUrl(typeof(ViewSourceControl), STR_CSHARP_RESOURCE);
                  
                    link.Attributes.Add("href", url);
                }
                else
                    link.Attributes.Add("href", this.ResolveUrl(this.CssLocation));

                this.Page.Header.Controls.Add(link);
                Context.Items["StyleAdded"] = "1";
            }
        }


        protected void btnShowCode_Click(object sender, EventArgs e)
        {
            DisplayCode();
        }

        protected void DisplayCode()
        {
            string File = this.Page.Server.MapPath(this.ResolveUrl(this.CodeFile));
            File = File.ToLower();

            // Allow only source and aspx files
            string extension = Path.GetExtension(File).ToLower();

            if ( !",.cs,.vb,.aspx,.asmx,.js,.ashx,".Contains("," + extension + ",") )
            {
                this.Output = "Invalid Filename specified...";
                return;
            }

            if (System.IO.File.Exists(File))
            {
                StreamReader sr = new StreamReader(File);
                string FileOutput = sr.ReadToEnd();
                sr.Close();

                if (File.ToLower().EndsWith(".cs") || File.ToLower().EndsWith(".asmx") || File.ToLower().EndsWith(".ashx"))
                {
                    JavaFormat Format = new JavaFormat();
                    this.Output = "<div class='showcode'>" + Format.FormatCode(FileOutput) + "</div>";
                }
                else if (File.ToLower().EndsWith(".js"))
                {
                    JavaScriptFormat Format = new JavaScriptFormat();
                    this.Output = "<div class='showcode'>" + Format.FormatCode(FileOutput) + "</div>";
                }
                else
                {
                    HtmlFormat Format = new HtmlFormat();
                    this.Output = "<div class='showcode'>" + Format.FormatCode(FileOutput) + "</div>";
                }

                //this.txtOutput.Text = "<pre>" + Server.HtmlEncode(FileOutput) + "</pre>";

                this.Page.ClientScript.RegisterStartupScript(typeof(ViewSourceControl), "scroll",
                    "var codeContainer = document.getElementById('" + this.btnShowCode.ClientID + "');codeContainer.focus();setTimeout(function() { window.scrollBy(0,200);},100);", true);
            }
            
            this.btnShowCode.Visible = true;
            
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            base.RenderControl(writer);

            if (!string.IsNullOrEmpty(this.Output))
                writer.Write(this.Output);
        }
    }

    public enum DisplayStates
    {
        Button,
        Code
    }
}

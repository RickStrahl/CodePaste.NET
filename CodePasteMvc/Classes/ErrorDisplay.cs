using System.Web;
using System.Text;
using System.Web.Mvc;
using Westwind.Utilities;

namespace Westwind.Web.Mvc
{
    /// <summary>
    /// An error display component that allows rich rendering of individual messages
    /// as well as validation error messages.
    /// 
    /// Object should be passed in to view end rendered with 
    /// &lt;%= ((ErrorDisplay) ViewData["ErrorDisplay"]).Show(450,true) %&gt;
    /// or via a view.
    /// 
    /// Relies on several CSS Styles:
    /// .errordisplay, errordisplay-text, errordisplay-warning-icon, errordisplay-info-icon
    /// The icon links link to images.
    /// </summary>
    public class ErrorDisplay
    {
        /// <summary>
        /// The message that is displayed
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        private string _message = null;


        /// <summary>
        /// Determines whether there is a message present.
        /// </summary>
        public bool HasMessage
        {
            get
            {
                if (DisplayErrors.Count > 0 && string.IsNullOrEmpty(Message))
                    Message = "Please correct the following errors:";

                return !string.IsNullOrEmpty(Message);
            }
        }

        /// <summary>
        /// Flag that determines whether the message is displayed
        /// as HTML or as text. By default message is encoded as text (true).
        /// </summary>
        public bool HtmlEncodeMessage
        {
            get { return _HtmlEncodeMessage; }
            set { _HtmlEncodeMessage = value; }
        }
        private bool _HtmlEncodeMessage = true;


        /// <summary>
        /// Timeout in milliseconds before the error display is hidden
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Holds a modelstate errors collection
        /// </summary>
        public ValidationErrorCollection DisplayErrors
        {
            get
            {
                if (_DisplayErrors == null)
                    _DisplayErrors = new ValidationErrorCollection();

                return _DisplayErrors;
            }
        }
        private ValidationErrorCollection _DisplayErrors = null;


        StringBuilder writer = new StringBuilder();
        bool visible = false;

        public ErrorDisplayTypes ErrorDisplayType = ErrorDisplayTypes.Error;

        protected void RenderTop(int width, bool center)
        {
            writer.Length = 0;

            writer.Append("<div class=\"errordisplay\" ");

            if (width != 0)
                writer.Append("style=\"width: " + width.ToString() + "px;");
            if (center)
                writer.Append("margin-left: auto; margin-right: auto");
            writer.Append("\"");

            writer.Append(" />\r\n");
        }

        protected void RenderBottom()
        {
            // close out the dialog
            writer.AppendLine("</div>");
        }

        protected void RenderDisplayErrors()
        {
            if (DisplayErrors.Count > 0)
            {
                writer.AppendLine("<hr/>");
                writer.AppendLine(DisplayErrors.ToHtml());
            }
        }

        public HtmlString Show(int width = 400, bool center = true)
        {
            return Render(width, center);
        }

        /// <summary>
        /// Method 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public HtmlString Render(int width = 400, bool center = true)
        {
            if (!visible || !HasMessage)
                return new HtmlString(string.Empty);

            RenderTop(width, center);

            if (ErrorDisplayType == ErrorDisplayTypes.Error)
                writer.AppendLine(" <div class=\"errordisplay-warning-icon\"></div>");
            else
                writer.AppendLine(" <div class=\"errordisplay-info-icon\"></div>");

            writer.AppendLine("<div class=\"errordisplay-text\">");

            writer.AppendLine(HtmlEncodeMessage ? HttpUtility.HtmlEncode(Message) : Message);
            RenderDisplayErrors();

            writer.AppendLine("</div>");

            RenderBottom();

            return new HtmlString(writer.ToString());
        }


        public void ShowError(string errorMessage)
        {
            ErrorDisplayType = ErrorDisplayTypes.Error;
            Message = errorMessage;
            visible = true;
        }


        public void ShowMessage(string message)
        {
            ErrorDisplayType = ErrorDisplayTypes.Message;
            Message = message;
            visible = true;
        }

        /// <summary>
        /// Adds ModelState errors to the validationErrors
        /// </summary>
        /// <param name="modelErrors"></param>
        public void AddMessages(ModelStateDictionary modelErrors, string fieldPrefix = null)
        {
            fieldPrefix = fieldPrefix ?? string.Empty;

            foreach (var state in modelErrors)
            {
                if ((state.Value.Errors.Count > 0))
                    DisplayErrors.Add(state.Value.Errors[0].ErrorMessage, fieldPrefix + state.Key);
            }
            visible = true;
        }

        /// <summary>
        /// Adds an existing set of Validation Errors to the DisplayErrors
        /// </summary>
        /// <param name="validationErrors"></param>
        public void AddMessages(ValidationErrorCollection validationErrors, string fieldPrefix = null)
        {
            fieldPrefix = fieldPrefix ?? string.Empty;

            foreach (ValidationError error in validationErrors)
            {
                DisplayErrors.Add(error.Message, fieldPrefix + error.ControlID);
            }
            visible = true;
        }

        /// <summary>
        /// Adds an individual model error
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="control"></param>
        public void AddMessage(string errorMessage, string control = null)
        {
            DisplayErrors.Add(errorMessage, control);
            visible = true;
        }
    }

    public enum ErrorDisplayTypes
    {
        Error,
        Message
    }
}

#if false
    /// <summary>
    /// An error display component that allows rich rendering of individual messages
    /// as well as validation error messages.
    /// 
    /// Object should be passed in to view end rendered with 
    /// &lt;%= ((ErrorDisplay) ViewData["ErrorDisplay"]).Show(450,true) %&gt;
    /// </summary>
    public class ErrorDisplay
    {
        public string Message { get;set;}


        
        public bool HtmlEncodeMessage
        {
            get { return _HtmlEncodeMessage; }
            set { _HtmlEncodeMessage = value; }
        }
        private bool _HtmlEncodeMessage = true;


        /// <summary>
        /// Holds a modelstate errors collection
        /// </summary>
        public ValidationErrorCollection DisplayErrors
        {
            get 
            {
                if (_DisplayErrors == null)
                    _DisplayErrors = new ValidationErrorCollection();
            
                return _DisplayErrors; 
            }
        }
        private ValidationErrorCollection _DisplayErrors = null;


        StringBuilder writer = new StringBuilder();
        bool visible = false;
        private ErrorDisplayTypes ErrorDisplayType = ErrorDisplayTypes.Error;

        private void RenderTop(int width, bool center)
        {
            writer.Length = 0;
            if (center)            
                writer.AppendLine("<center>");            

            writer.Append("<div class=\"errordisplay\" style=\"text-align: left;");            

            if (width != 0)
                writer.Append("width: " + width.ToString() +"px;\"");

            writer.Append(" />\r\n");
        }
        private void RenderBottom(bool center)
        {
            this.RenderDisplayErrors();

            writer.AppendLine("</div>");
            if (center)
                writer.AppendLine("</center>");
        }

        private void RenderDisplayErrors()
        {
            if (this.DisplayErrors.Count > 0)
            {
                writer.Append("<div style=\"margin-left: 30px;\"><hr/>");
                writer.Append(this.DisplayErrors.ToHtml());
                writer.Append("</div>");
            }
        }

        public string Show(int width, bool center)
        {
            if (!visible) 
                return string.Empty;

            RenderTop(width, center);

            if (this.ErrorDisplayType == ErrorDisplayTypes.Error)
                writer.Append(" <img src=\"" + VirtualPathUtility.ToAbsolute("~/css/images/warning.gif") + "\" style=\"float: left; margin: 2px 12px;\" />");
            else
                writer.Append(" <img src=\"" + VirtualPathUtility.ToAbsolute("~/css/images/info.gif") + "\" style=\"float: left; margin: 2px 12px;\" />");

            writer.Append(  this.HtmlEncodeMessage ? HttpUtility.HtmlEncode(this.Message) : this.Message);
            RenderBottom(center);

            return writer.ToString();
        }
        public void ShowError(string errorMessage)
        {
            this.ErrorDisplayType = ErrorDisplayTypes.Error;
            this.Message = errorMessage;
            this.visible = true;
        }
        public void ShowMessage(string message)
        {
            this.ErrorDisplayType = ErrorDisplayTypes.Message;
            this.Message = message;
            this.visible = true;
        }

        /// <summary>
        /// Adds ModelState errors to the validationErrors
        /// </summary>
        /// <param name="modelErrors"></param>
        public void AddMessages(ModelStateDictionary modelErrors)
        {
            foreach(var state in modelErrors)
            {
                if ((state.Value.Errors.Count > 0))
                    this.DisplayErrors.Add(state.Value.Errors[0].ErrorMessage);
            }
            this.visible = true;
        }

        /// <summary>
        /// Adds an existing set of Validation Errors to the DisplayErrors
        /// </summary>
        /// <param name="validationErrors"></param>
        public void AddMessages(ValidationErrorCollection validationErrors)
        {
            foreach (ValidationError error in validationErrors)
            {
                this.DisplayErrors.Add(error.Message,error.ControlID);
            }
            this.visible = true;
        }

        /// <summary>
        /// Adds an individual model error
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="control"></param>
        public void AddMessage(string errorMessage, string control)
        {            
            this.DisplayErrors.Add(errorMessage,control);
            this.visible = true;
        }
    }

    public enum ErrorDisplayTypes
    {
        Error,
        Message
    }

}
#endif

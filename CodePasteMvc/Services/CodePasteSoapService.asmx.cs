using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using CodePasteBusiness;
using System.Web.Services.Protocols;
using System.Xml;
using Westwind.Utilities;
using System.Xml.Serialization;
using System.Threading;

namespace CodePasteMvc
{

    /// <summary>
    /// CodePaste.NET Soap Service
    /// </summary>
    [WebService(Namespace = "http://codepaste.net/soap")]
    public class CodePasteSoapService : CodePasteServiceBase
    {
        /// <summary>
        /// Throw SoapExceptions explicitly and embed message into Detail
        /// for easier retrieval (otherwise stacktrace gets returned)
        /// </summary>
        /// <param name="message"></param>
        protected override void ThrowException(string message)
        {            
            XmlDocument xdoc = new XmlDocument();
            string xml = string.Format("<detail><message>" + message + "</message></detail>");
            xdoc.LoadXml(xml);
            
            throw new SoapException(message, SoapException.ServerFaultCode,null,xdoc,null);
        }
    }
}

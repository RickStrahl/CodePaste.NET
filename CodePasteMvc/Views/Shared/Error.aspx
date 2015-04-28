<%@ Page Language="C#" MasterPageFile="~/Views/Shared/CodePasteMaster.Master" 
         Inherits="System.Web.Mvc.ViewPage<CodePasteMvc.Controllers.ErrorViewModel>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="headers" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">
&nbsp;
    <h2><%= !string.IsNullOrEmpty(this.Model.Title) ? this.Model.Title : "An error occurred" %></h2>
    
   <fieldset>
   <div class="containercontent">
        <%= this.Model.MessageIsHtml ? this.Model.Message : Html.Encode(this.Model.Message) %>
    </div>
    <% if (!string.IsNullOrEmpty(this.Model.RedirectTo )) {%>
    <div class="containercontent">
        <a href="<%= this.Model.RedirectTo %>">Click here to continue...</a>        
    </div>
    <% 
           // refresh page
           if (this.Model.RedirectToTimeout >0)
           Response.Headers.Add("Refresh", this.Model.RedirectToTimeout.ToString() + ";" + 
                                           this.Model.RedirectTo);   
       } 
    %>
    </fieldset>
</asp:Content>

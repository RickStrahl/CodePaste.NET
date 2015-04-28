<%@ Page Title="CodePaste Administration" Language="C#" MasterPageFile="~/Views/Shared/CodePasteMaster.Master" 
                  Inherits="System.Web.Mvc.ViewPage<CodePasteMvc.Controllers.UsersViewModel>" %>

<asp:Content ContentPlaceHolderID="headers" runat="server">
    <style type="text/css">
        #divFilterDialog a
        {
        	display: block;
        }
    </style>
</asp:Content>

<asp:Content ContentPlaceHolderID="content" runat="server">

    <h2>CodePaste Administration</h2>
    
    <%= this.Model.ErrorDisplay.Show(500, true) %>
    
    <fieldset id="divFilterDialog">
    <div class="containercontent">     
        <a href="<%= ResolveUrl("~/admin/users") %>" class="hoverbutton">Show Users</a>
        <a href="<%= ResolveUrl("~/admin/AbuseSnippets") %>" class="hoverbutton">Code Snippets with Abuse</a>
        <a href="<%= ResolveUrl("~/admin/DeleteSpam") %>" class="hoverbutton">Delete Spam Keywords</a>
        <a href="<%= ResolveUrl("~/admin/AddSpamFilters") %>" class="hoverbutton">Manage Spam Filters</a>
        <hr />
        <a href="<%= ResolveUrl("~/admin/webrequestlog.aspx") %>" class="hoverbutton">Web Request Log</a>
        <a href="<%= Url.Action("UpdateFormattedCode") %>" class="hoverbutton">Updated Formatted Code Snippets (slow!)</a>
        
        <a href="<%= ResolveUrl("~/admin/configuration") %>" class="hoverbutton">Configuration Settings</a>
        <hr />
        <%= Html.ActionLink("Update User Passwords","UpdatePasswords",null, new {@class="hoverbutton"}) %>
        <%= Html.ActionLink("Clear Anonymous Snippets","ClearAnonymousSnippets",null, new {@class="hoverbutton" } ) %>
        <%= Html.ActionLink("Database Housekeeping/Cleanup","DatabaseHousekeeping",null, new {@class="hoverbutton" } ) %>
        <%= Html.ActionLink("Shrink Database","ShrinkDatabase",null, new {@class="hoverbutton"} ) %>
        
    </div>
    </fieldset>

<script src="<%= ClientScript.GetWebResourceUrl(typeof(ClientScriptProxy),"Westwind.Web.Controls.Resources.ww.jquery.js") %>"></script>
        
</asp:Content>



<%@ Page  Language="C#" MasterPageFile="~/Views/Shared/CodePasteMaster.Master" 
         Inherits="System.Web.Mvc.ViewPage<CodePasteMvc.Controllers.ConfigurationViewModel>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="headerstop" runat="server">
<title>CodePaste.NET - Configuration Settings</title>   
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="headers" runat="server">
<style type="text/css">
.inputfield 
{
	width: 400px;
}    
</style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">

    <h2>Configuration</h2>   
    <form id="ConfigForm" action="" method="post">
    <fieldset>
    <div class="containercontent">
        <div class="labelheader">Application Title</div>
        <%= Html.TextBox("ApplicationTitle", Model.Configuration.ApplicationTitle, new { @class="inputfield" })  %>

        <div class="labelheader">Max Display Count</div>
        <%= Html.TextBox("MaxListDisplayCount", Model.Configuration.MaxListDisplayCount, new { style = "min-width:150px" })%>
                
        <div class="labelheader">Error Display Mode</div>
        <%= Html.DropDownList("DebugMode", Model.DebugModeList, new { style="min-width:150px" })  %>
        <hr />
        <input type="submit" id="btnSave" name="btnSave" value="Save" class="labelheader    "/>
    </div>
    </fieldset> 
    </form>

</asp:Content>


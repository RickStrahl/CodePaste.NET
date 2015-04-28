<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/CodePasteMaster.Master" Inherits="System.Web.Mvc.ViewPage<UsersViewModel>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="headers" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">
    
    <h2>Users</h2>
    
    <fieldset id="divFilterDialog">
    <div class="containercontent">           
    
    
    <%  foreach (User user in this.Model.UserList)
        { %>
        
        <div class="user" id="user_<%= user.Id %>">         
            <div class="username"><a href="<%= Url.Action("user","admin", new { id= user.Id }) %>"><%= Html.Encode(user.Name) %></a></div>
            <div style="float: right"><a href="javascript:{}" class="hoverbutton" onclick="RemoveUser();">Remove</a></div>
            <div><small>Entered: <%= user.Entered.ToString("d MMM, yyyy")%>  | <%=  user.Snippets %> snippets</small></div>
        </div>
            
    <% } %>

    </div>
    </fieldset>
</asp:Content>


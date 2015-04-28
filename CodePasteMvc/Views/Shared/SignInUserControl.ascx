<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" EnableViewState="false" %>
<%
    if (Request.IsAuthenticated) {
        AppUserState userState = (AppUserState) this.ViewData["UserState"];                
        
%>
        <small>Signed in as: <b><%= Html.ActionLink(userState.Name, "Register", "Account", new { id = userState.UserId  }, new { @class = "hoverbutton" })%></b> | 
        <%= Html.ActionLink("Sign out", "LogOff", "Account", null, new { @class="hoverbutton" } ) %></small>
<%
    }
    else {
%> 
        <small><b class="hoverbutton"><%= Html.ActionLink("Sign in", "LogOn", "Account") %></b>or <b class="hoverbutton"><%= Html.ActionLink("Register","Register","Account") %></b></small> 
<%
    }
%>

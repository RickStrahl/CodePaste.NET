<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/CodePasteMaster.Master" Inherits="System.Web.Mvc.ViewPage<UsersViewModel>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="headers" runat="server">
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="content" runat="server">
    
    <h2>Code Snippets with Abuse flagged</h2>
    
    <fieldset id="divFilterDialog">
    <div class="containercontent">           
    <% busCodeSnippet busSnippet = ViewBag.busSnippet; %>
    
    <%  foreach (CodeSnippet snippet in this.Model.SnippetList)
        { %>
        
        <div class="snippet" id="snippet_<%= snippet.Id %>">                     
            <div class="snippettitle" style="font-weight: bold">
                <a  href="<%= Url.Content("~/" + snippet.Id) %>"><%= string.IsNullOrEmpty(snippet.Title) ? "No Title" : snippet.Title %></a>
            </div>
            <hr />
            <div class="snippetcode"><pre><%= busSnippet.GetFormattedCodeLines(snippet.Code, 10, snippet.Language)%></pre></div> 
            <div class="dialog-statusbar">                
                Entered: <%= snippet.Entered.ToString("d MMM, yyyy")%> | 
                Author: <%= snippet.Author ?? "n/a" %> | 
                <a href="javascript:{}" class="removesnippet">Remove Snippet</a> |
                <a href="javascript:{}" class="reportabuse"><%= snippet.IsAbuse ? "Undo Abuse" : "Report Abuse" %></a>       
            </div>
        </div>
            
    <% } %>

    </div>
    </fieldset>

    <script type="text/javascript">

        var callbackHandler = '<%= Url.Content("~/CodePasteHandler.ashx")  %>';

        $(document).ready(function () {
            $(".removesnippet").click(removeSnippet);
            $(".reportabuse").click(reportAbuse);
        });

        function removeSnippet(e) {            
    	    var $el = $(this);
    	    $cont = $el.parents(".snippet");
            var id = $cont.attr("id").replace("snippet_","");
    	    
    	    ajaxCallMethod(callbackHandler, "RemoveSnippet", [id], function () {
    	        $cont.fadeOut("slow");
    	    }, onPageError);
    	}
    	function reportAbuse() {
    	    var $el = $(this);
    	    $cont = $el.parents(".snippet");
    	    var id = $cont.attr("id").replace("snippet_", "");

    	    ajaxCallMethod(callbackHandler, "ReportAbuse", [id],
                    function (result) {
                        
                        if (result === true)
                            $el.text("Undo Abuse");
                        else
                            $el.text("Report Abuse");

                        showStatus("Thank you: Abuse status updated.", 8000, true);
                    }, onPageError);
    	}
    </script>
</asp:Content>


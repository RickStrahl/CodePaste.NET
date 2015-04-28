/// <reference path="~/scripts/jquery.js" />
/// <reference path="~/scripts/ww.jquery.js" />

$(document).ready(function() {
    showStatus({ afterTimeoutText: "hide" });
    showStatus("hide");

    $(".removelink").click(RemoveSnippet);
    $("#ApiFormats").change(displayApiFormat);
});

function RemoveSnippet(e) {
    var jSnip = $(this).parents(".snippet,.snippetofauthor");
    
    var id = jSnip.attr("id");
    id = id.replace("snippet_", "");

    ajaxCallMethod(serverVars.callbackHandler, "RemoveSnippet", [id], function() {
        jSnip.fadeOut("slow", function() { jSnip.remove(); });
    }, onPageError);

}

function displayApiFormat() {
    var val = $(this).val();

    if (val == "Html")
        return;
    
    var prefix = "?";
    var url = window.location.href;
    debugger;
    if (url.indexOf("?") > -1)
        prefix = "&";

    window.location = url + prefix + "format=" + val.toLowerCase();

    // always display Text in Web page for externally loaded formats
    setTimeout(function() { $(this).val("Text"); }, 3000); 
}
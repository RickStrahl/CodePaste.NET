/// <reference path="~/scripts/jquery.js" />
/// <reference path="~/scripts/ww.jquery.js" />

$(document).ready(function() {
    $("#btnOpenIdLogin").click(function() {
        $(this).hide();
        $("#imgOpenIdLoginProgress").show();
    });
});


function openIdUrl(site)
{
    var value = "";
    var autoClick = false;
    
    if (site == "openid") {
        value = "<Your Account>.myopenid.com"
    }
    else if (site == "google") {
        value = "https://www.google.com/accounts/o8/id";
        autoClick = true;
    }
    else if (site == "yahoo") {
        value = "http://yahoo.com/"
        autoClick = true;
    }
        
    if (value) {
        var jText = $("#openid_identifier");
       jText.val(value)
            .focus();
       if (autoClick)
           $("#btnOpenIdLogin").trigger("click");
    }
       
}
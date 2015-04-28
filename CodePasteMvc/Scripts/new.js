/// <reference path="~/scripts/jquery.js" />
/// <reference path="~/scripts/ww.jquery.js" />
$(document).ready(function() {
    
    $("#snippetform").submit(function (e) {
        $("#Code").val(aceEditor.getSession().getValue());

        var lang = $("#Language").val();
        if (!lang || lang == "text") {
            if (!confirm("You haven't selected a language for your snippet.\r\nAre you sure you want to continue?")) {
                $("#Language").focus();
                e.preventDefault();
                return false;
            }
        }

        $("<input type='hidden' name='qx'>")
                .val($("#Code").val().length)
                .appendTo($("#snippetform"));
        
        return true;
    });


    $("#ShowLineNumbers").click(function() {
        var checked = $(this).attr("checked");
        aceEditor.renderer.setShowGutter(checked);
    });
    $("#Code").focus();
});

function showLayover() {

    $("#divSignInNag")
                .shadow()
                .closable({cssClass: "closebox-container"})
                .draggable()
                .fadeIn(700)
                .centerInClient({
                    keepCentered: true,
                    container: $("#divContainerContent")
                });                

                

    $("#Title,#Code")
        .keydown(function() {
            $("#divSignInNag").fadeOut();
            $(this).unbind("keydown");
        })
        .mousedown(function() {
            $("#divSignInNag").fadeOut();
            $(this).unbind("mousedown");
        });    

}

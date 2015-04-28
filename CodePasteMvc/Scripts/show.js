/// <reference path="~/scripts/jquery.js" />
/// <reference path="~/scripts/ww.jquery.js" />
//(function ($, undefined) {
$().ready( function() {
    $("#imgEditTitle").click(titleEdit);
    $("#imgPostToTwitter").click(twitterClick);
    $("#btnShowLineNumbers").click(showLineNumbers);
    $("#btnEditLanguage").click(langEdit);
    $("#btnSaveComment").click(saveComment);
    $("#btnEditTags").click(editTags);
    $("#btnEditComment").click(editComment);
    $("#btnCopyCode").click(copyToClipboard);
    $("#btnFavorite").click(addFavorite);
    $("#btnUpdateCode").click(updateSnippet);
    $("#Language").change(langEdit);

    showStatus({ afterTimeoutText: "hide" });
    showStatus("hide");

    // ace editor
    window.aceEditor = configureAceEditor(serverVars);
});

    function copyToClipboard() {
        $('#CodeDisplay').editable();
        $('#_contenteditor').val(aceEditor.getValue())
        $('#_contenteditor')
            .attr("readonly", "readonly")
            .focus()
            .select()
            .keydown(function(e) {
                // capture Ctl-C, Ctl-X to remove editable
                if (e.which == 27) {
                    e.preventDefault();
                    e.stopPropagation();
                }
                if (e.which == 67 || e.which == 88 || e.which == 27) {
                    // have to delay so text doesn't go away before copy operation
                    setTimeout(function() {
                        $("#divCopyDialog").hide();
                        $("#CodeDisplay").editable("cleanup");
                    }, 200);
                }
            });

        setTimeout(function() {
            // show overlay message
            $("#divCopyDialog")
                .stop()
                .show()
                .shadow()
                .closable()
                .draggable()
                .fadeIn(1000)
                .centerInClient({ container: $("#divContainerContent") })
                .css("top", 350);
        }, 10);

        setTimeout(function() { $("#divCopyDialog").fadeOut(1000); }, 3000);
        return false;
    };

    function updateSnippet() {
        var code = aceEditor.getValue();
        var id = $("#SnippetId").val();

        ajaxCallMethod(serverVars.callbackHandler, "SaveCode", [id, code],
            function() {
                showStatus("Code Snippet has been updated and saved.", 8000);
            });
    }

    function titleEdit(evt) {
        var id = $("#SnippetId").val();
        $("#Title").editable(
        {
            editClass: "tagedit",
            saveHandler: function(res) {
                ajaxCallMethod(serverVars.callbackHandler, "SaveTitle", [id, res.text],
                    function(title) {
                        $("#Title").text(title);
                        res.cleanup();
                    },
                    onPageError);
            }
        });
    }


    function langEdit() {
        var $lang = $(this);
        var lang = $lang.val();
        var id = $("#SnippetId").val();
        aceEditor.getSession().setMode("ace/mode/" + lang);

        ajaxCallMethod(serverVars.callbackHandler, "SaveLanguage", [id, lang],
            function () {
                showStatus("Language updated to " + lang + " on snippet.", 5000);
            }, onPageError);
    }



    function editTags() {
        var id = $("#SnippetId").val();
        var jTags = $("#Tags");
        var tags = jTags.text();
        jTags.editable(
        {
            editClass: "tagedit",
            saveHandler: function(res) {
                ajaxCallMethod(serverVars.callbackHandler, "SaveTags", [id, res.text],
                    function(html) {
                        jTags.html(html);
                        res.cleanup()
                    },
                    onPageError);
            }
        });
    }

    function editComment() {
        var id = $("#SnippetId").val();
        var jComment = $("#Comment");
        var comment = jComment.text();

        jComment.editable(
        {
            editClass: "commentedit",
            saveHandler: function(res) {
                ajaxCallMethod(serverVars.callbackHandler, "SaveMainComment", [id, res.text],
                    function(html) {
                        jComment.html(html);
                        res.cleanup()
                    },
                    onPageError);
            }
        });
    }

    function RemoveSnippet(id) {
        ajaxCallMethod(serverVars.callbackHandler, "RemoveSnippet", [id], function() {
            $("body").fadeOut("slow", function() { window.location = serverVars.baseUrl; });
        }, onPageError);

    }

    function ReportAbuse() {
        var id = $("#SnippetId").val();
        if (!id)
            return;

        ajaxCallMethod(serverVars.callbackHandler, "ReportAbuse", [id],
            function(result) {
                showStatus("Thank you: Abuse for this snippet has been reported.", 8000, true);
            }, onPageError);
    }

    function addFavorite() {
        var btn = $(this);

        var title = $("#Title").text();
        var snippetId = $("#SnippetId").val();

        ajaxCallMethod(serverVars.callbackHandler, "AddFavorite", [title, snippetId],
            function(result) {
                if (result) {
                    var txt = btn.find("#btnFavorite-text");
                    var btnText = txt.text();
                    if (btnText == "Favorite") {
                        showStatus("Added to your favorites: " + title, 4000);
                        txt.text("Unfavorite");
                    } else {
                        showStatus("Removed from your Favorites: " + title, 4000);
                        txt.text("Favorite");
                    }
                } else
                    showStatus("Couldn't add topic to favorites.", 4000);


            });

    }

    function saveComment() {
        ajaxCallMethod(serverVars.callbackHandler, "SaveComment", [$("#txtNewComment").val(), $("#SnippetId").val()],
            function(result) {
                var jNew = $(parseTemplate($("#commentTemplate").html(), result)).hide();

                $("#divNewComment")
                    .before(jNew)
                    .fadeOut(function() {
                        jNew.fadeIn("slow");
                    });
            }, onPageError);
    }

    function showLineNumbers() {
        serverVars.showLineNumbers = !serverVars.showLineNumbers;
        aceEditor.renderer.setShowGutter(serverVars.showLineNumbers);
        aceEditor.renderer.updateText();
    }

    function twitterClick() {
        var id = $("#SnippetId").val();
        window.open("http://twitter.com/home?status=" + $("#Title").text() + ": http://codepaste.net/" + id + escape(" #") + id, "_blank");
    }

//})(jQuery);

// called from Show Page itself at the end
function configureAceEditor(serverVars) {
    var editor = ace.edit("CodeDisplay");
    editor.setTheme("ace/theme/" + serverVars.theme);
    //editor.setTheme("ace/theme/textmate");
    //editor.setTheme("ace/theme/clouds");
    //editor.setTheme("ace/theme/xcode");
    //editor.setTheme("ace/theme/eclipse");
    //editor.setTheme("ace/theme/mono_industrial");
    editor.getSession().setMode("ace/mode/" + serverVars.language);
    editor.setFontSize(13);

    if (!serverVars.allowEdit) {
        editor.setReadOnly(true);
        editor.setHighlightActiveLine(false);
    } else
        editor.setHighlightActiveLine(true);

    editor.renderer.setShowGutter(serverVars.showLineNumbers);
    editor.renderer.setPadding(10);

    // fill entire view
    editor.setOptions({
        maxLines: Infinity,
        wrapBehavioursEnabled: true

    });

    editor.setShowPrintMargin(false);

    var session = editor.getSession();
    session.setTabSize(3);

    // allow editor to soft wrap text
    session.setUseWrapMode(true);
    session.setWrapLimitRange();

    //editor.getSession().setUseWrapMode(true);
    $("#CodeDisplay").css("opacity", "1");

    return editor;
}

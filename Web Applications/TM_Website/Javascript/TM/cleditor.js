﻿TM.Gui.GuidanceItemEditor_Div = "guidanceItem";

TM.Gui.GuidanceItemEditor.applyCssFix = function () {
    $('#rawGuidanceItem').width("99%").height($(window.document).height() - 150)

    /*if ($.browser.msie)
    {				
    $('#rawGuidanceItem').width("99%").height($(window.document).height() -150)
    }
    else
    {
    $('#rawGuidanceItem').absolute().left(2).right(2).bottom(10).top(180);
    }*/

    $(".cleditorMain").css("border", "0");
    //$(".cleditorToolbar").css("border","1px solid");
    $(window).resize(TM.Gui.GuidanceItemEditor.onResize);
}

TM.Gui.GuidanceItemEditor.onResize = function () {
    //console.log('on resize')
    var height = $(window.document).height()
                                 - 170
                                 - $(".cleditorToolbar").height();
    $(".cleditorMain").height(height);
    $(".cleditorMain iframe").height('99%').width('100%');
}

TM.Gui.GuidanceItemEditor.showGuidanceItemHtml = function (htmlCode, dataType) {
    $("#rawGuidanceItem").show();
    $.cleditor.defaultOptions.width = "99%";
    //$.cleditor.defaultOptions.height = 	$(window.document).height() -150;
    $('#rawGuidanceItem').val(htmlCode);
    TM.Gui.GuidanceItemEditor.cleditor = $("#rawGuidanceItem").cleditor();
    TM.Gui.GuidanceItemEditor.applyCssFix();
    TM.Gui.GuidanceItemEditor.onResize();

    //Only enabled WYSIWYG for Html
    cleditor = TM.Gui.GuidanceItemEditor.cleditor[0];
    if (dataType.toLowerCase() == "html") {
        $(".cleditorToolbar").show();
        cleditor.$area.hide();
        cleditor.$frame.show();
    }
    else {
        $(".cleditorToolbar").hide();
        cleditor.$area.show();
        cleditor.$frame.hide();
        $("#rawGuidanceItem").css("border", "1px solid").css("padding", "5px");
    }

    //fix the weird bug that happens when the user presses on enter in the new link text box
    if ($.browser.msie)
        $(".cleditorPopup input[type=text]").keypress(function (e) { if (e.which == 13) { return false; } });
    //fix the bug that happens in chrome on where the #document.body tag has styles (which get propagated in actions like bold)                
    if ($.browser.safari && $.browser.webkit)
        $(cleditor.doc.body).attr("style", "")
}

TM.Gui.GuidanceItemEditor.getEditorCode = function () {
    return $('#rawGuidanceItem').val();
}

$(function () {
    $("#rawGuidanceItem").hide();


});	
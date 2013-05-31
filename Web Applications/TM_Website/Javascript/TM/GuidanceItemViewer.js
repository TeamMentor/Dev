var guidanceId = "";
//		var topUrl = window.location.href.split('#');

var currentHtml = "";


var scrollToTopOfPage = function () {
    $('html, body').animate({ scrollTop: 0 }, 'slow');
}

var selectDisplayMode = function () {

    var urlElements = document.URL.split('/');
    urlElements.pop();

    var mode = urlElements.pop();

    switch (mode) {
        case "preview":
            {
                previewEditorCode();
                break;
            }
        case "article":
            {
                getGuidFromUrl();
                break;
            }
        default:
            {
                break;
            }
    }
};

var setGuidanceIdfromUrlHash = function () {
    getGuidFromUrl();
}

var getGuidFromUrl = function () {
    var mappingText = document.URL.split('/').pop()     //get last segment
                                                      .split("#").shift()   // remove hash
                                                 ;
    //mapping =    { mapping : mappingText }    ;                           
    //TM.WebServices.Helper.invoke_TM_WebService("getGuidForMapping",mapping, 						
    TM.WebServices.WS_Libraries.getGuidForMapping(mappingText, function (guid) {
        guidanceId = guid;
        loadGuidanceItem();
    });
}
//                alert(getGuidanceIdfromUrl());

var previewEditorCode = function () {
    if (window.opener === undefined || window.opener === null || window.opener.GetEditedLibraryItemCode === undefined) {
        showError("Sorry, we couldn't find the edited article.");
        return;
    }

    previewGuidanceItem(window.opener.GetEditedLibraryItemCode(), showEditorPreview);
}

var showEditorPreview = function (data) {
    data.d.isPreview = true;
    showGuidanceItemData(data);
}

var loadGuidanceItem = function () {
    //getGuidanceItemHtml(guidanceId, function(data) { showGuidanceItemHtml(data.d) } )                                  				
    getGuidanceItemData(guidanceId, showGuidanceItemData);
    //getSliderData();

}

showGuidanceItemData = function (data) {
    loadedGuidanceItem = data.d;
    if (loadedGuidanceItem != null) {
        $("#itemContent").show();
        "#Title".$().html(loadedGuidanceItem.Metadata.Title);
        document.title = loadedGuidanceItem.Metadata.Title + " - " + TM.ArticleTitle;
        if ((loadedGuidanceItem.Metadata.Category == null || loadedGuidanceItem.Metadata.Category == "") &&
                            (loadedGuidanceItem.Metadata.Phase == null || loadedGuidanceItem.Metadata.Phase == "") &&
                            (loadedGuidanceItem.Metadata.Technology == null || loadedGuidanceItem.Metadata.Technology == "") &&
                            (loadedGuidanceItem.Metadata.Type == null || loadedGuidanceItem.Metadata.Type == "")) {
            $(".ItemDetailTable").hide();
        }
        else {
            $(".ItemDetailTable").show();
            "CategoriesLabel".$().html(loadedGuidanceItem.Metadata.Category);
            "PhaseLabel".$().html(loadedGuidanceItem.Metadata.Phase);
            "TechnologyLabel".$().html(loadedGuidanceItem.Metadata.Technology);
            "GuidanceTypeLabel".$().html(loadedGuidanceItem.Metadata.Type);
        }
        if (loadedGuidanceItem.Content.DataType.toLowerCase() === "wikitext") {
            showGuidanceItemWikiText(loadedGuidanceItem.Content.Data_Json);
            $("#Edit_WYSIWYG").html("Edit WikiText");
        }
        else {
            showGuidanceItemHtml(loadedGuidanceItem.Content.Data_Json);
            $("#Edit_WYSIWYG").html("Edit WYSIWYG");
        }
        if (TM.Gui.CurrentUser.isEditor() && loadedGuidanceItem.isPreview != true) {
            $(".EditButtons").show();
        }
    }
    else {
        showError("Sorry, we couldn't find the requested article.");
        "CategoriesLabel".$().html("");
        "PhaseLabel".$().html("");
        "TechnologyLabel".$().html("");
        "GuidanceTypeLabel".$().html("");
        //"DateLabel".$().html("");
        showGuidanceItemHtml("");
    }
}

var showError = function(message) {
    "ItemContent".$().hide();
    "#Title".$().html("").append("<h4>" + message +"</h4>");

}

var fixGuidanceItemLinks = function () {
    //$("a").css('color','#339')			// fix link color
    if (window.opener == null)
        return;
    $(".ItemContent a[href]").each(function () {
        var href = $(this).attr('href');
        $(this).css('color', '#339');
        if (href.indexOf('http') > -1)
            $(this).attr('target', '_blank');

        /*							else
        {
        var id =  href.replace("/article/","");				   					
        var giData= window.opener.$.data[id];
        if (isUndefined(giData))					
        {
        $(this).removeAttr('href')
        .css("text-decoration" , "underline");							
        }						
        }*/
    })

};

showGuidanceItemWikiText = function (wikiText) {
    var targetDiv = document.createElement('div');
    new creole().parse(targetDiv, wikiText)
    var html = targetDiv.innerHTML;
    showGuidanceItemHtml(html);
}

showGuidanceItemHtml = function (html) {
    $("#guidanceItem").html(html);
    currentHtml = html;
    fixGuidanceItemLinks();

    //scrollToTopOfPage();

    //apply prettyprint
    $(".ItemContent pre").addClass("prettyprint");
    prettyPrint();

    $("#copyrightNotice").show();
}

showPageChange = function (changeID) {
    if (changeID == 0) {
        $("#guidanceItem").html(currentHtml);
        $("#byUser").html('');
        $("#userComment").html('');
        $("#userActivity").html('');
    }
    else {
        var changePage = sliderData[sliderData.length - changeID];
        newHtml = changePage.PageContent
        if (newHtml != "")
            $("#guidanceItem").html(newHtml);
        $("#byUser").html(changePage.UserName);
        $("#userComment").html(changePage.UserComment);
        $("#userActivity").html(changePage.UserActivity);
    }
}

showSlider = function (data) {
    sliderData = data.d;
    $('#previousVersions').html("<b>There are " + sliderData.length + " previous versions</b>");

    //                         	alert(JSON.stringify(data.d));  
    $('#slider').slider({
        min: 0,
        max: sliderData.length,
        change: function (event, ui) { showPageChange($('#slider').slider('value')) },
        slide: function (event, ui) { showPageChange($('#slider').slider('value')) }
    });

    $('#slider').slider('value', 0);

}
getSliderData = function () {
    getPagesHistory_by_PageId(guidanceId, showSlider);
}



var openGuidanceItemEditor = function () {
    //document.location = "EditGuidanceItem.aspx?#id:" + guidanceId;
    //document.location = "/html_pages/GuidanceItemEditor/GuidanceItemEditor.html?#id:" + guidanceId;
    document.location = "/editor/" + guidanceId;
}

var openGuidanceItemNotepad = function () {
    //document.location = "EditGuidanceItem.aspx?#id:" + guidanceId;
    document.location = "/notepad/" + guidanceId;
}

var logUserActivity = function (activity) {
    //alert('logUserActivity: ' + activity);
    guidanceItem_LogPageUserActivity(guidanceId, activity, getSliderData);
    //			checkForActivitys();
}

var logUserComment = function () {
    var userComment = "UserCommentTextBox".$().val();
    guidanceItem_LogPageUserComment(guidanceId, userComment, getSliderData);
    "addCommentBox".$().dialog().parent().hide();
}

var showAddCommentDialog = function () {
    "addCommentBox".$().dialog().parent().show();
    "addCommentBox".$().dialog({ title: 'Add Comment' });
}

var loadUserDataAndBuildGui = function () {
    TM.Gui.CurrentUser.loadUserData();
}

var buildGui = function () {

    if (TM.Gui.CurrentUser.isViewer()) {
        selectDisplayMode();
        $("#notAViewerErrorMessage").hide();
    }
    else {
        $("#notAViewerErrorMessage").show().html("<h2>Access denied.</h2><br/><p>Please close this window or <a href='/Login'>login</a></p>");
    }

    /*if(TM.Gui.CurrentUser.isEditor() )				
    {						    
    $(".EditButtons").show();			
    }
    else
    $(".EditButtons").hide();
    */
    //"ItemTitle".$().css('padding',10)	
}

$(function () {
    $("#loadingMessage").remove();
    $("#itemContent").hide();

    TM.Events.onUserDataLoaded.add(buildGui);

    loadUserDataAndBuildGui();

    /*"links".$().add_Link("Edit WYSIWYG", "javascript:openGuidanceItemEditor()");
    "links".$().append(" ");
    "links".$().add_Link("Edit Source", "javascript:openGuidanceItemNotepad()");*/
    /*"SidePanel".$().css({ border:'solid 2px'}).absolute().right(10).top(10)										   
    .width(140).css('padding',8)
    .hide();
    if($.browser.msie)	
    "SidePanel".$().width(170)*/
    TM.Gui.ShowProgressBar.close();
    $("#SI_Footer").bottom(0).width("100%").relative()
});

$('a[href]').live('click', function () {
    var href = $(this).attr('href');
    if (href.slice(0, 11) == "ruledisplay") {
        window.location.hash = href;
        return false;
    }
    return true;
});
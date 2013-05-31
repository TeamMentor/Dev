<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" doctype-system="about:legacy-compat" />


  <xsl:template match="/">    
    <!--<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>-->
    <html>
      <head>
        <title>TeamMentor 'Notepad' Editor</title>        
		    <link rel="stylesheet" href="/Css/NotepadEditor.css" type="text/css"></link>
        <script src="/aspx_Pages/scriptCombiner.ashx?s=NotepadEdit_JS&amp;dontMinify=true"						type="text/javascript"></script>
      </head>
      <body>

        <span id="DataType_RawValue" class="NEHiddenValue"><xsl:value-of select="*/Content/@DataType"/></span>        
        <span id="Title_RawValue"    class="NEHiddenValue"><xsl:value-of select="*/Metadata/Title"/></span>        
        <span id="Id_RawValue"       class="NEHiddenValue"><xsl:value-of select="*/Metadata/Id"/></span>
        <span id="Category_RawValue"    class="NEHiddenValue"><xsl:value-of  select="*/Metadata/Category"/></span>
        <span id="Phase_RawValue"    class="NEHiddenValue"><xsl:value-of select="*/Metadata/Phase"/></span>
        <span id="Technology_RawValue"    class="NEHiddenValue"><xsl:value-of select="*/Metadata/Technology"/></span>
        <span id="Type_RawValue"    class="NEHiddenValue"><xsl:value-of select="*/Metadata/Type"/></span>

        <script>
           jQuery.ctrl      = function(key, callback, args)                                       //from http://www.gmarwaha.com/blog/2009/06/16/ctrl-key-combination-simple-jquery-plugin/
                                    {
                                        $(document).keydown(function(e) 
                                        {
                                            if(!args) args=[];                              // IE barks when args is null
                                            if(e.keyCode == key.charCodeAt(0) &amp;&amp; e.ctrlKey) 
                                            {
                                                callback.apply(this, args);
                                                return false;
                                            }
                                         });
                                    };
        </script>
        
        <script>

            var id       = $("#Id_RawValue").html();
            var title    = $("#Title_RawValue").html();
            var dataType = $("#DataType_RawValue").html().toString().toLowerCase();

            document.title = "Editing: " + title;

            var GetEditedLibraryItemCode = function()
              {
                var loadedGuidanceItem  = { };

                loadedGuidanceItem.Metadata = {};
                loadedGuidanceItem.Content = {};

                loadedGuidanceItem.Content.DataType = $("#DataType").val();
                loadedGuidanceItem.Content.Data_Json = $(".Content").val();

                loadedGuidanceItem.Metadata.Id = id;
                loadedGuidanceItem.Metadata.Title = title;
                loadedGuidanceItem.Metadata.Type = $("#Type_RawValue").html();
                loadedGuidanceItem.Metadata.Technology = $("#Technology_RawValue").html();
                loadedGuidanceItem.Metadata.Phase = $("#Phase_RawValue").html();
                loadedGuidanceItem.Metadata.Category = $("#Category_RawValue").html();
                loadedGuidanceItem.Metadata.DirectLink = loadedGuidanceItem.Metadata.Title;

                var savedGuidanceItem = { guidanceItem: loadedGuidanceItem };

                return savedGuidanceItem;
              }
            
            var onSave = function(result)
              {
                if (result)
                  $("#SaveButton").html('Saved OK');
                else
                  $("#SaveButton").html('Save Failed');
              }
              
            var saveContent = function()
              {                
                var html =      $(".Content").val(); 
                var dataType =  $("#DataType").val(); 
                $("#SaveButton").html('Saving Content');
                TM.WebServices.WS_Libraries
                              .set_Article_Content(id, dataType, html, onSave , function(error) { alert(error.responseText)});            
                return false;
              }
          
             var setSaveButtonText = function()
              {
                $("#SaveButton").html('Save Changes');
              }
              
             var openArticle = function()
              {
                //window.open("/article/"+id);
                document.location = "/article/"+id;
                return false;
              }
              
             var setupGui = function()
              {
                if (TM.Gui.CurrentUser.isEditor())
                {
                  $("#SaveButton").click(saveContent);
                  $.ctrl('S', saveContent);
                  $(".Content").keydown(setSaveButtonText);
                  $("#OpenArticle").click(openArticle);
                  $("#PreviewChanges").click(function () { previewEditorCode(id); });
                  setSaveButtonText();     
                  $("textarea").tabby();
                  $("body").show();                  
                }
                else
                  document.location = '/Login';
              }
              
            $(function() 
                {
                  $("body").hide();                 
                  TM.Events.onUserDataLoaded.add(setupGui);       
                  TM.Gui.CurrentUser.loadUserData();
                  $("#DataType").val(dataType);
                });
        </script>
        
        <xsl:apply-templates select="*"/>
      </body>
    </html>
  </xsl:template>


                
  <xsl:template match="Metadata">
    <div class="NEHeader">
      <h2>
        <xsl:value-of select='Title'/>        
      </h2>
      id: <xsl:value-of select="Id"/>
    </div>
    <span class="NEToolbar">
		<a href="" type="submit" class="NEButton" id="OpenArticle">View Article</a>
    <a href="" type="submit" class="NEButton" id="PreviewChanges">Preview Changes</a>
		<a href="" type="submit" class="NEButton" id="SaveButton">Save Changes
		</a>
    </span>
  </xsl:template>

  <xsl:template match="Content">
	<span class="NEDataTypeLabel">Data Type:</span>
    <select id="DataType" class="NEDataType">
      <option value ="html">Html</option>
      <option value ="wikitext">WikiText</option>      
     </select>
    <span id="WikiTextHelp">WikiText uses <a href="http://docs.teammentor.net/article/Team Mentor Wiki Markup" target="_blank"> WikiCreole</a> </span>
	
	<div class="NEContent">
    <textarea class="Content" id="Content">
        <xsl:value-of select="Data" disable-output-escaping="yes"/>
    </textarea>
	</div>
  </xsl:template>
</xsl:stylesheet>

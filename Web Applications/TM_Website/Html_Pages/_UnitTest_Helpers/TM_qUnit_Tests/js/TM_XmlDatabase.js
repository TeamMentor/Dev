//Ensure TM GuiObject are loaded

module("TM Xml Database");

var disableSILibrary = true;
disableSILibrary = false;

var getLibrariesToDisable = function()
	{
		var librariesToDisable = { disabledLibraries: []}
		if (disableSILibrary)
			librariesToDisable.disabledLibraries.push("SI");
		
		return librariesToDisable;
	};

asyncTest("login As Admin", function () 
    {
        stop();
		QUnit.login_as_Admin(
			function(sessionID)
				{
					notEqual(sessionID, window.TM.Const.emptyGuid, "valid SessionID: " + sessionID);
					start();
				});
	});

asyncTest("get config settings", function() 
	{		
		stop();
		"XmlDatabase_GetLibraryPath".invokeWebService(
			{}, 
			function(libraryPath) 
				{				
					ok(libraryPath, "LibraryPath: " + libraryPath);
					start();
				});
				
		"TMConfigFile".invokeWebService(
			{}, 
			function(tmConfigFile) 
				{
					ok(tmConfigFile						, "TMConfigFile ok ")					
					ok(true								, "TMConfigFile: " 		+ JSON.stringify(tmConfigFile))										
					ok(tmConfigFile.XmlLibrariesPath	, "XmlLibrariesPath: " 	+ tmConfigFile.XmlLibrariesPath);					
					start();
				});				
	});	

asyncTest("disabled libraries", function() 
	{
		"GetDisabledLibraries".invokeWebService(
			{}, 
			function(disabledLibraries)
				{
					ok(disabledLibraries	, "disabledLibraries: " + disabledLibraries);
					start();
		
		});
				
		"SetDisabledLibraries".invokeWebService(
			getLibrariesToDisable(), 
			function(disabledLibraries)
				{
					ok(disabledLibraries	, "disabledLibraries: " + disabledLibraries);
					start();
				});				
	});

asyncTest("reload data", function() 
	{			
		var startTime = new Date();
		"XmlDatabase_ReloadData".invokeWebService(
			{}, 
			function(reloadMessage) 
				{
					ok(reloadMessage, "reloadMessage: " + reloadMessage);
					var timeSpan =  startTime.toNow_SecondsAndMiliSeconds();
					ok(timeSpan, "Data reloaded in: " + timeSpan);
					start();
				});
	});		
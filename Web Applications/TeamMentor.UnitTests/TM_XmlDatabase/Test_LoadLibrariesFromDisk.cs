﻿using System;
using System.Linq;
using NUnit.Framework;
using O2.DotNetWrappers.ExtensionMethods;
using O2.FluentSharp;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.TM_XmlDatabase
{
    [TestFixture]
    public class Test_LoadLibrariesFromDisk  : TM_XmlDatabase_InMemory
    {                        
        public Test_LoadLibrariesFromDisk()
        {
            if (Tests_Config.offline)
                Assert.Ignore("Ignoring Test because we are offline");   

           if(new O2.Kernel.CodeUtils.O2Kernel_Web().online().isFalse())
                Assert.Ignore("Ignoring Test because we are offline");   

            Install_LibraryFromZip_OWASP();            
        }
        
        [Test] public void GetGuidanceExplorerFilesInPath()
        {
            var xmlFiles = tmXmlDatabase.Path_XmlLibraries.getGuidanceExplorerFilesInPath();
            Assert.IsNotEmpty(xmlFiles);
            foreach (var xmlFile in xmlFiles)
            {
                var fileContents = xmlFile.fileContents().fixCRLF();
                var secondLine  = fileContents.lines().second();
                Assert.That(secondLine.starts("<guidanceExplorer"));                                                            
            }
        }
        [Test] public void LoadGuidanceExplorerFilesDirectly()
        {
            foreach (var xmlFile in tmXmlDatabase.Path_XmlLibraries.getGuidanceExplorerFilesInPath())
            {
                "Loading libraryXmlFile: {0}".info(xmlFile.fileName());                
                var guidanceExplorer = xmlFile.getGuidanceExplorerObject();
                Assert.IsNotNull(guidanceExplorer);
            }
        }
        [Test] public void LoadGuidanceExplorerFiles()
        {            
            //tmXmlDatabase.setGuidanceExplorerObjects();
            var xmlFiles    = tmXmlDatabase.Path_XmlLibraries.getGuidanceExplorerFilesInPath();
            var tmLibraries = tmXmlDatabase.tmLibraries();
            Assert.AreEqual(xmlFiles.size(), tmLibraries.size());
        }
        [Test] public void Test_getGuidanceExplorerObjects()
        {
            var guidanceExplorers = tmXmlDatabase.Path_XmlLibraries.getGuidanceExplorerObjects();			
            Assert.IsNotNull(guidanceExplorers, "guidanceExplorers");
            Assert.That(guidanceExplorers.size()>0 , "guidanceExplorers was empty");			
            Assert.That(tmXmlDatabase.GuidanceExplorers_XmlFormat.size() > 0, "GuidanceExplorers_XmlFormat was empty");    		
        }    	    	
        [Test] public void Test_getLibraries()
        {             
            var guidanceExplorers = tmXmlDatabase.GuidanceExplorers_XmlFormat.Values.toList();
            var tmLibraries = tmXmlDatabase.tmLibraries();
            Assert.IsNotNull(tmLibraries,"tmLibraries"); 
            for(var i=0;  i < guidanceExplorers.size() ; i++)
            {
                Assert.AreEqual(tmLibraries[i].Caption,  guidanceExplorers[i].library.caption, "caption");
                Assert.AreEqual(tmLibraries[i].Id, guidanceExplorers[i].library.name.guid(), "caption");
            }
            Assert.That(tmXmlDatabase.GuidanceExplorers_XmlFormat.size()>0, "GuidanceExplorers_XmlFormat empty");    		
        }    	     	   
        [Test] public void Test_getFolders()
        {
           // LoadGuidanceExplorerFiles();
            //var guidanceExplorers = TM_Xml_Database.loadGuidanceExplorerObjects();    		
            var libraryId = tmXmlDatabase.GuidanceExplorers_XmlFormat.Keys.first();
            Assert.AreNotEqual(Guid.Empty, libraryId, "Library id was empty");
            libraryId = "4738d445-bc9b-456c-8b35-a35057596c16".guid();          // set it to the OWASP library since that has a folder
            var guidanceExplorerFolders = tmXmlDatabase.GuidanceExplorers_XmlFormat[libraryId].library.libraryStructure.folder;    		
            Assert.That(guidanceExplorerFolders.size() > 0,"guidanceExplorerFolders was empty");
            
            var tmFolders = tmXmlDatabase.tmFolders(libraryId);
            Assert.IsNotNull(tmFolders,"folders"); 
            Assert.That(tmFolders.size() > 0,"folders was empty");    		

            var mappedById = tmFolders.ToDictionary(tmFolder => tmFolder.folderId);

            //Add checks for sub folders	
            foreach(var folder in guidanceExplorerFolders)
            {
                Assert.That(mappedById.hasKey(folder.folderId.guid()), "mappedById didn't have key: {0}".format(folder.folderId));    				
                var tmFolder = mappedById[folder.folderId.guid()];				
                Assert.That(tmFolder.name == folder.caption);				
                Assert.That(tmFolder.libraryId == libraryId, "libraryId");	
            }      		
        }     	
        [Test][Assert_Editor]
        public void Test_getGuidanceHtml()
        {
            HttpContextFactory.Context = new API_Moq_HttpContext().httpContext();       // need for the logUserActivity method need to map the IP
            //LoadGuidanceExplorerFiles();            
            //tmXmlDatabase.reloadGuidanceExplorerObjects();                

            var guidanceItems = tmXmlDatabase.tmGuidanceItems();
            var firstGuidanceItem = guidanceItems.first();
            Assert.IsNotNull(firstGuidanceItem,"firstGuidanceItem");
            var guid = firstGuidanceItem.Metadata.Id;
            Assert.IsNotNull(guid,"guid");
            Assert.AreNotEqual(guid, Guid.Empty,"guid.isGuid");
            var sessionId = Guid.Empty;
            var html = tmXmlDatabase.getGuidanceItemHtml(sessionId,guid);
            Assert.IsNotNull(html, "html");    		
            Assert.That(html.valid(), "html was empty");    		
        }


        

    }
}

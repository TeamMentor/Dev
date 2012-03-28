using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using SecurityInnovation.TeamMentor.WebClient.WebServices;
using Moq;
using O2.Kernel;
using O2.DotNetWrappers.ExtensionMethods;
using O2.DotNetWrappers.DotNet;
using O2.DotNetWrappers.Windows;
using urn.microsoft.guidanceexplorer;
using urn.microsoft.guidanceexplorer.guidanceItem;
using Microsoft.Security.Application;
//O2File:TM_Xml_Database.cs
//O2File:../O2_Scripts_APIs/_O2_Scripts_Files.cs

namespace SecurityInnovation.TeamMentor.WebClient.WebServices
{
	public static class TM_Xml_Database_ExtensionMethods_XmlDataSources_GuidanceItems_Search
	{				
		public static List<Guid> guidanceItems_SearchTitleAndHtml(this TM_Xml_Database tmDatabase, string searchText)
		{
			return tmDatabase.guidanceItems_SearchTitleAndHtml(tmDatabase.xmlDB_GuidanceItems() , searchText);
		}
		
		public static List<Guid> guidanceItems_SearchTitleAndHtml(this TM_Xml_Database tmDatabase, List<Guid> guidanceItemsIds, string searchText)
		{
			if (guidanceItemsIds.size() > 0)
				return tmDatabase.guidanceItems_SearchTitleAndHtml(tmDatabase.xmlDB_GuidanceItems(guidanceItemsIds) , searchText);
			// if there are no guidanceItemsIds provided, search on all of them
			return tmDatabase.guidanceItems_SearchTitleAndHtml(tmDatabase.xmlDB_GuidanceItems() , searchText);
		}
		
		public static List<Guid> guidanceItems_SearchTitleAndHtml(this TM_Xml_Database tmDatabase, List<TeamMentor_Article> guidanceItems, string searchText)
		{
            var searchTextEncoded = HttpUtility.HtmlEncode(searchText).lower();   
            
			//var maxNumberOfItemsToReturn = 100;			
			"There are {0} GIs to search".error(guidanceItems.size());
			return 	(from guidanceItem in guidanceItems
					 where guidanceItem.Metadata.Title.valid() &&
                           (guidanceItem.Metadata.Title.lower().contains(searchTextEncoded)       ||
//					        guidanceItem.title.regEx	   				(searchText) 	 ||
                            guidanceItem.Content.Data_Raw.lower().contains(searchTextEncoded) )
//                       || guidanceItem.htmlContent.regEx			(searchText)           )									
					 select guidanceItem.Metadata.Id
					).toList(); 
		}		
		
/*		public static List<Guid> guidanceItem_SearchTitle(this TM_Xml_Database tmDatabase, string searchText)
		{
			var maxNumberOfItemsToReturn = 250000;
			var lowercaseSearchText = searchText.lower();
			return 	(from guidanceItem in tmDatabase.GuidanceItems
					 where guidanceItem.title.lower().contains(lowercaseSearchText) ||
					       guidanceItem.title.regEx(searchText)
					 select new Item() { Key =guidanceItem.title, 
										Value= guidanceItem.guidanceItemId.str() }
					).Take(maxNumberOfItemsToReturn)
					 .toList(); 
		}*/				
		
		public static List<TeamMentor_Article> xmlDB_GuidanceItems(this TM_Xml_Database tmDatabase)
		{
			return TM_Xml_Database.Cached_GuidanceItems.Values.toList();
		}		
		
		public static List<TeamMentor_Article> xmlDB_GuidanceItems(this TM_Xml_Database tmDatabase, List<Guid> guidanceItemsIds)
		{
			return (from guidanceItem in TM_Xml_Database.Cached_GuidanceItems.Values
					where guidanceItemsIds.contains(guidanceItem.Metadata.Id)
					select guidanceItem).toList();
		}
	}
	
	public static class TM_Xml_Database_ExtensionMethods_XmlDataSources_GuidanceItems_Load
	{		
		
		public static TM_Xml_Database xmlDB_Load_GuidanceItems(this TM_Xml_Database tmDatabase)
		{						
			var pathXmlLibraries = TM_Xml_Database.Path_XmlLibraries;			
			if (pathXmlLibraries.getCacheLocation().fileExists().isFalse())
			{
				"[TM_Xml_Database] in xmlDB_Load_GuidanceItems, cache file didn't exist, so creating it".debug();
				var o2Timer = new O2Timer("loaded GuidanceItems from disk").start();
				//Load GuidanceItem from the disk				
				foreach(var guidanceExplorer in tmDatabase.xmlDB_GuidanceExplorers())
				{					
					var libraryId = guidanceExplorer.library.name.guid();
					var pathToLibraryGuidanceItems = pathXmlLibraries.pathCombine(guidanceExplorer.library.caption);
					"libraryId: {0} : {1}".info(libraryId,pathToLibraryGuidanceItems);
					var filesToLoad = pathToLibraryGuidanceItems.files(true,"*.xml");		
					tmDatabase.xmlDB_Load_GuidanceItemsV3(libraryId, filesToLoad );				
				}								
				
				//save it to the local cache file (reduces load time from 8s to 0.5s)
				tmDatabase.save_GuidanceItemsCache();    
				o2Timer.stop();
			
				tmDatabase.ensureFoldersAndViewsIdsAreUnique();
				tmDatabase.removeMissingGuidanceItemsIdsFromViews();
			}
			return tmDatabase;
		}
		
		public static List<TeamMentor_Article> xmlDB_Load_GuidanceItemsV3(this TM_Xml_Database tmDatabase, Guid libraryId, List<string> guidanceItemsFullPaths)
		{
			var o2Timer = new O2Timer("xmlDB_GuidanceItems").start();
			var itemsLoaded = 0;
			//var maxToLoad = 1000;
			var guidanceItems = new List<TeamMentor_Article>();
			foreach(var fullPath in guidanceItemsFullPaths)
			{ 
				var guidanceItemId = fullPath.fileName().remove(".xml");
				if (guidanceItemId.isGuid())
				{
					var guidanceItem = tmDatabase.xmlDB_GuidanceItem(guidanceItemId.guid(),fullPath);
					if (guidanceItem.notNull())
					{
						guidanceItems.add(guidanceItem);						    
						guidanceItem.Metadata.Library_Id = libraryId;						
					}
					//if (maxToload-- < 1)
					//	break;
					if (itemsLoaded++ % 200 == 0)
						"loaded {0} items".info(itemsLoaded);
				}
				//if (itemsLoaded > maxToLoad)
				//	break; 
					
			}
			o2Timer.stop();
			return guidanceItems;
		}
		
		public static TeamMentor_Article xmlDB_GuidanceItem(this TM_Xml_Database tmDatabase, Guid guidanceItemId)
		{
			return tmDatabase.xmlDB_GuidanceItem(guidanceItemId, null);
		}

		public static TeamMentor_Article fixGuidanceItemFileDueToGuidConflict(this TM_Xml_Database tmDatabase, Guid original_Guid, string fullPath)
		{			
			var newGuid = Guid.NewGuid();
			var newPath = fullPath.replace(original_Guid.str(), newGuid.str());
			Files.MoveFile(fullPath, newPath);
			"[xmlDB_GuidanceItem] resolved GuidanceItem ID conflict for  Id '{0}' was already mapped. \nExisting path: \t{1} \nNew path:  \t{2}".error(original_Guid, fullPath , newPath);
			var guidanceItemV3 = tmDatabase.xmlDB_GuidanceItem(newGuid, newPath);			
			return guidanceItemV3;
		}
		
		//[PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static TeamMentor_Article xmlDB_GuidanceItem(this TM_Xml_Database tmDatabase, Guid guidanceItemId, string fullPath)
		{
			try
			{
				if (TM_Xml_Database.Cached_GuidanceItems.hasKey(guidanceItemId))
				{
					//"found match for id: {0} in {1}".info(guidanceItemId, fullPath);
					if (TM_Xml_Database.GuidanceItems_FileMappings[guidanceItemId] != fullPath)
					{						
						//"[xmlDB_GuidanceItem] GuidanceItem ID conflict, the Id '{0}' was already mapped. \nExisting path: \t{1} \nNew path:  \t{2}".error(
						//	guidanceItemId, TM_Xml_Database.GuidanceItems_FileMappings[guidanceItemId] , fullPath);
						return tmDatabase.fixGuidanceItemFileDueToGuidConflict(guidanceItemId, fullPath);
					}
					return TM_Xml_Database.Cached_GuidanceItems[guidanceItemId]; 
				}
				
				if(fullPath.notNull())
				{
					//"trying to load id {0} from virtualPath: {1}".info(guidanceItemId, virtualPath);				
					var pathXmlLibraries = TM_Xml_Database.Path_XmlLibraries;
					/*var fullPath = virtualPath.fileExists() 
										? virtualPath
										: pathXmlLibraries.pathCombine(virtualPath).fullPath();*/
					if (fullPath.fileExists())									
					{
						//"loading {0}".info(fullPath);

                        var _guidanceItem = fullPath.load<TeamMentor_Article>(); //.transform_into_guidanceItem();
                        if (_guidanceItem.isNull())
						    // _guidanceItem = guidanceItem.Load(fullPath).transform();
                            _guidanceItem = fullPath.load<Guidance_Item_Import>().transform();
						if (_guidanceItem.notNull())
						{
							if(_guidanceItem.Metadata.Id != guidanceItemId)
							{
								"FOUND GUID CHANGE".error();
								_guidanceItem.Metadata.Id_History += _guidanceItem.Metadata.Id.str() + ",";
								_guidanceItem.Metadata.Id 		   = guidanceItemId;
								_guidanceItem.saveAs(fullPath);								
							}
						//guidanceItemV3.guidanceItemId		 = original_Guid;		// this gives us the ability to track its source 
						//guidanceItemV3.source_guidanceItemId = newGuid;				// also provides support for moving GuidanceItems across libraries
							//var _guidanceItemV3 = _guidanceItem.tmGuidanceItemV3();
							
							TM_Xml_Database.Cached_GuidanceItems.Add(guidanceItemId, _guidanceItem);
							TM_Xml_Database.GuidanceItems_FileMappings.add(guidanceItemId, fullPath);
							
							
							return _guidanceItem;
						}					
					}
					else
						"[xmlDB_GuidanceItem] could not find file: {0}".error(fullPath);
				}
				else
					"no virtualPath provided for id: {0}".error(guidanceItemId);
			}
			catch(Exception ex)
			{
				"[TM_Xml_Database] in xmlDB_GuidanceItem: {0}".error(ex.Message);
			}
			return null;
		}								
	}

	public static class TM_Xml_Database_ExtensionMethods_XmlDataSources_GuidanceItem
	{
		[PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static TeamMentor_Article xmlDB_RandomGuidanceItem(this TM_Xml_Database tmDatabase)
		{
			return tmDatabase.xmlDB_NewGuidanceItem(Guid.Empty,
													"GI title",
													"GI images", 
			//										DateTime.Now, 
													"Topic..",  
													"Technology....", 
													"Category...", 
													"RuleType...", 
													"Priority...", 
													"Status.." , 
													"Author...", 
													"Phase...",
													"GI HTML content", 
													Guid.NewGuid());
		}
		
		[PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static TeamMentor_Article xmlDB_NewGuidanceItem(this TM_Xml_Database tmDatabase, Guid guidanceItemId, 
															   string title, string images,
//															   DateTime lastUpdate, 
															   string topic, string technology, string category, 
															   string ruleType, string priority, string status, 
															   string author,string phase,  string htmlContent, 
															   Guid libraryId)
		{			
				
		    var guidanceItem = new TeamMentor_Article();
            guidanceItem.Metadata = new TeamMentor_Article_Metadata()
                					{
										Id = (guidanceItemId == Guid.Empty) 
													? Guid.NewGuid()
													: guidanceItemId,
                                        Library_Id = libraryId,
										Author = author,
										Category = category,
										Priority = priority,
										Type = ruleType,
										//.Source ;
										Status = status,
										Technology = technology,
										Title = title,										
										Phase = phase, 										
							//			.Type1;
							//			.type;
									};
            guidanceItem.Content = new TeamMentor_Article_Content()
                                    {
                                        Data_Raw  = htmlContent,
                                        DataType  = "html"//,
                                        //Sanitized = false
                                    };
			/*var guidanceItem  = new guidanceItem()
									{
										id = (guidanceItemId == Guid.Empty) 
													? Guid.NewGuid().str()
													: guidanceItemId.str(),
										Author = author,
										Category = category,
										content = htmlContent,
					//					Date = lastUpdate.str(),	
										Priority = priority,
										Rule_Type = ruleType,
										//.Source ;
										Status = status,
										Technology = technology,
										title = title,
										Topic = topic,
										phase = phase, 
										libraryId = libraryId.str()
							//			.Type1;
							//			.type;
									};
             */ 
			guidanceItem.xmlDB_Save_GuidanceItem(libraryId, tmDatabase);			
			return guidanceItem;
		} 
		
		
		[PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static bool xmlDB_Save_GuidanceItem(this TeamMentor_Article guidanceItem, Guid libraryId, TM_Xml_Database tmDatabase)
		{
			
			var xmlLibraries = TM_Xml_Database.Path_XmlLibraries;
			var guidanceXmlPath = tmDatabase.getXmlFilePathForGuidanceId(guidanceItem.Metadata.Id, libraryId);
			
			"Saving GuidanceItem {0} to {1}".info(guidanceItem.Metadata.Id, guidanceXmlPath);				
			
			//guidanceItem.libraryId = libraryId.str();			
			guidanceItem.saveAs(guidanceXmlPath);			
			//add it to in Memory cache

//			tmDatabase.setGuidanceExplorerObjects();
			guidanceItem.update_Cache_GuidanceItems(tmDatabase);			
			
			return guidanceXmlPath.fileExists();			
		}
		
        [PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static bool xmlDB_Delete_GuidanceItems(this TM_Xml_Database tmDatabase, List<Guid> guidanceItemIds)
		{
			var result = true;
			foreach(var guidanceItemId in guidanceItemIds)
			{
				if (tmDatabase.xmlDB_Delete_GuidanceItem(guidanceItemId).isFalse())
					result = false;
			}
			return result;
		}

		[PrincipalPermission(SecurityAction.Demand, Role = "EditArticles")]
		public static bool xmlDB_Delete_GuidanceItem(this TM_Xml_Database tmDatabase, Guid guidanceItemId)
		{
			var guidanceItemXmlPath = tmDatabase.removeGuidanceItemFileMapping(guidanceItemId);
			"removing GuidanceItem with Id:{0} located at {1}".info(guidanceItemId, guidanceItemXmlPath);
			if (guidanceItemXmlPath.valid())				
			Files.deleteFile(guidanceItemXmlPath);
			if (TM_Xml_Database.Cached_GuidanceItems.hasKey(guidanceItemId))
				TM_Xml_Database.Cached_GuidanceItems.Remove(guidanceItemId);
			//TM_Xml_Database.mapGuidanceItemsViews();
			return true;
		}
		
		
        [PrincipalPermission(SecurityAction.Demand, Role = "ReadArticles")]
		public static string xmlDB_guidanceItemXml(this TM_Xml_Database tmDatabase, Guid guidanceItemId)
		{
			var guidanceXmlPath = tmDatabase.getXmlFilePathForGuidanceId(guidanceItemId);
			return guidanceXmlPath.fileContents().xmlFormat();
		}

        public static Guid xmlBD_resolveMappingToArticleGuid(this TM_Xml_Database tmDatabase, string mapping)
		{
            if (mapping.isGuid())
                return mapping.guid();

            if (mapping.isInt())
            {   
                var pos = mapping.toInt();
                if(pos < TM_Xml_Database.Cached_GuidanceItems.Keys.size())
                    return TM_Xml_Database.Cached_GuidanceItems.Keys.toList()[pos];            
            }

            var results = tmDatabase.guidanceItems_SearchTitleAndHtml(mapping);
            if (results.size() >0)
                return results.first();
            return Guid.Empty;
		}
	}
}
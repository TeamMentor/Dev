﻿using System;
using System.Threading;
using FluentSharp.CoreLib;

namespace TeamMentor.CoreLib
{
    public class TM_Authentication
    {        
        public static bool          Global_Disable_Csrf_Check   { get; set; }    
        public  TM_WebServices      TmWebServices               { get; set; }    
        public  bool                Disable_Csrf_Check          { get; set; }    

        public TM_Authentication    (TM_WebServices tmWebServices)
        {
            TmWebServices = tmWebServices;
            Disable_Csrf_Check = false;	
        }        

        public Guid                 sessionID
        {
            get
            {                
                // first check if there s a session variable already set                
                if (HttpContextFactory.Session.notNull() && HttpContextFactory.Session["sessionID"].notNull() && HttpContextFactory.Session["sessionID"] is Guid)
                    return (Guid)HttpContextFactory.Session["sessionID"];

                // then check the cookie
                var sessionCookie = HttpContextFactory.Request.cookie("Session");
                if (sessionCookie.notNull() && sessionCookie.isGuid())
                    return sessionCookie.guid();

                var sessionHeader = HttpContextFactory.Request.header("Session");
                if (sessionHeader.notNull() && sessionHeader.isGuid())
                    return sessionHeader.guid();

                //if none is set, return an empty Guid	
                return Guid.Empty;                
            }
            set
            {                
                var previousSessionId = sessionID;
             
                if (HttpContextFactory.Session.notNull())                
                    HttpContextFactory.Session["sessionID"] = value;

                HttpContextFactory.Response.set_Cookie("Session", value.str()).httpOnly();
                HttpContextFactory.Request .set_Cookie("Session", value.str()).httpOnly();   
             
                if (value == Guid.Empty)
                {
                    UserGroup.Anonymous.setThreadPrincipalWithRoles();          // ensure that from now on the current user as no more privileges
                    previousSessionId.logout();				                    // and that the previous sessionIS is logged out
                }		
                else    
                    new UserRoleBaseSecurity().MapRolesBasedOnSessionGuid(value);
            }
        }
        public TMUser               currentUser
        {
            get
            {
                var tmUser = sessionID.session_TmUser();
                if (tmUser.notNull())
                    tmUser.SecretData.CSRF_Token = sessionID.csrfToken();	
                return tmUser;
            }
        }
        public bool                 check_CSRF_Token()
        {
            if (Global_Disable_Csrf_Check)
            {
                "[TM_Authentication] Global_Disable_Csrf_Check was set".error();
                return true;
            }
            if (Disable_Csrf_Check)
                return true;
            var header_Csrf_Token = HttpContextFactory.Context.Request.Headers["CSRF-Token"];
            
            if (header_Csrf_Token != null && header_Csrf_Token.valid())
            {            
                if (header_Csrf_Token == sessionID.csrfToken())		
                    return true;
            }            
            return false;            
        }
        public TM_Authentication    mapUserRoles()
        {
            return mapUserRoles(false);
        }
        public TM_Authentication    mapUserRoles(bool disable_Csrf_Check)
        {
            setGitUser();            
            Disable_Csrf_Check = disable_Csrf_Check;            
            if (sessionID == Guid.Empty || sessionID.validSession() == false)
                /*if (SingleSignOn.singleSignOn_Enabled)
                {
                    sessionID = new SingleSignOn().authenticateUserBasedOn_SSOToken();
                }
                else*/
                if (WindowsAuthentication.windowsAuthentication_Enabled)
                {                    
                    sessionID = new WindowsAuthentication().authenticateUserBaseOn_ActiveDirectory();
                }            
            
            
            var userGroup = UserGroup.None;
            
            if (sessionID != Guid.Empty)
            {                
                if (check_CSRF_Token())		// only map the roles if the CSRF check passed
                {                    
                    userGroup = new UserRoleBaseSecurity().MapRolesBasedOnSessionGuid(sessionID);					
                }                
            }            
            if (userGroup == UserGroup.None)
            {
                if (TMConfig.Current.TMSecurity.Show_ContentToAnonymousUsers)
                    UserGroup.Reader.setThreadPrincipalWithRoles();
                else
                    UserGroup.Anonymous.setThreadPrincipalWithRoles();
            }            
            //var userRoles = Thread.CurrentPrincipal.roles().toList().join(",");            
            if (HttpContextFactory.Session.notNull())
            {                
                HttpContextFactory.Session["principal"] = Thread.CurrentPrincipal;
            }
            return this;
        }
        public void                 setGitUser()
        {
            if(currentUser.notNull())
             TM_UserData.Current.NGit_Author_Name  = currentUser.UserName;
             TM_UserData.Current.NGit_Author_Email =  currentUser.EMail;
        }
        public Guid                 logout()
        {
            sessionID = Guid.Empty;
            return sessionID;
        }
    }
}
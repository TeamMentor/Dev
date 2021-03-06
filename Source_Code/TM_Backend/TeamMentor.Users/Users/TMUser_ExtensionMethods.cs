using System;
using System.Collections.Generic;
using System.Linq;
using FluentSharp.CoreLib;
using FluentSharp.Web;


namespace TeamMentor.CoreLib
{
    public static class TMUser_ExtensionMethods
    {
        public static bool      account_Enabled(this TMUser tmUser)
        {
            return tmUser.notNull() && tmUser.AccountStatus.UserEnabled;
        }

        public static TMUser    disable_Account(this TMUser tmUser)
        {
            if (tmUser.notNull())
            {
                tmUser.AccountStatus.UserEnabled = false;                
                tmUser.logUserActivity("Account was Disabled", "");
            }
            return tmUser;
        }
        public static TMUser    enable_Account(this TMUser tmUser)
        {
            if (tmUser.notNull())
            {
                tmUser.AccountStatus.UserEnabled = true;                
                tmUser.logUserActivity("Account was Enabled", "");
            }
            return tmUser;
        }
        public static bool      account_Expired    (this TMUser tmUser)
        {
            if (tmUser.isNull())
                return true;
            if (tmUser.AccountStatus.AccountNeverExpires)                   // this overwrites the ExpirationDate value
                return false;
            if (tmUser.AccountStatus.ExpirationDate == default(DateTime))   // if this ExpirationDate is not set, the user account is NOT expired
                return false;
            return tmUser.AccountStatus.ExpirationDate < DateTime.Now;      // if it is set, check if value is bigger than now
        }
        public static bool      password_Expired   (this TMUser tmUser)
        {
            if (tmUser.isNull())
                return true;
            return tmUser.AccountStatus.PasswordExpired;            
        }
        public static TMUser    expire_Account     (this TMUser tmUser)
        {
            if (tmUser.notNull())            
                tmUser.AccountStatus.ExpirationDate = DateTime.Now.AddMilliseconds(-10); // 10 miliseconds so that the value is in the past                            
            return tmUser;
        }
        public static TMUser    expire_Password    (this TMUser tmUser)
        {
            if (tmUser.notNull())
            {
                tmUser.AccountStatus.PasswordExpired = true;
                //10.sleep();
                tmUser.logUserActivity("Account was Expired", "");
            }
            return tmUser;
        }
        public static string    createPasswordHash (this TMUser tmUser, string password)
        {		    		                
            // first we hash the password with the user's name as salt (what used to happen before the )           
            var sha256Hash = tmUser.UserName.hash_SHA256(password);
            
            //then we create a PBKDF2 hash using the user's ID (as GUID) as salt
            var passwordHash = sha256Hash.hash_PBKDF2(tmUser.ID);

            return passwordHash;
        }
       
        public static bool      sendPasswordReminder(this string email)
        {
            var tmUser = email.tmUser_FromEmail();
            if (tmUser.isNull())
                return false;
            var resetToken = email.passwordResetToken_getHash();
            if (resetToken != Guid.Empty)
                return SendEmails.SendPasswordReminderToUser(tmUser, resetToken);
            return false;
        }


        public static ResetPassword_Result sendPasswordReminder_Response(this string email)
        {
            var tmConfig = TMConfig.Current;
            var response = new ResetPassword_Result();
            var tmUser = email.tmUser_FromEmail();
            //Email does not exist
            if (tmUser.isNull())
            {
                response.PasswordReseted = false;
                response.Message = TMConfig.Current.showDetailedErrorMessages() 
                                    ? tmConfig.TMErrorMessages.Email_Does_Not_Exist_ErrorMessage 
                                    : tmConfig.TMErrorMessages.General_PasswordReset_Error_Message;
                return response;
            }
            //Account Expired
            if (tmUser.account_Expired())
            {
                response.PasswordReseted = false;
                response.Message = TMConfig.Current.showDetailedErrorMessages() 
                                   ? tmConfig.TMErrorMessages.AccountExpiredErrorMessage
                                   : tmConfig.TMErrorMessages.General_PasswordReset_Error_Message;
                return response;
            }
            //Account Disabled
            if (!tmUser.account_Enabled())
            {
                response.PasswordReseted = false;
                response.Message = TMConfig.Current.showDetailedErrorMessages() 
                                   ? tmConfig.TMErrorMessages.AccountDisabledErrorMessage
                                   : tmConfig.TMErrorMessages.General_PasswordReset_Error_Message;
                return response;
            }
            var resetToken = email.passwordResetToken_getHash();
            if (resetToken != Guid.Empty)
            {
               var result= SendEmails.SendPasswordReminderToUser(tmUser, resetToken);
               response.PasswordReseted = result;
               response.Message = string.Empty;
               return response;
            }
            return new ResetPassword_Result();
        }


        public static bool      passwordResetToken_isValid(this TMUser tmUser, Guid resetToken)
        {
            if(tmUser.notNull())
                if (tmUser.SecretData.PasswordResetToken == tmUser.passwordResetToken_getHash(resetToken))
                    return true;
            return false;
        }        
        public static string    passwordResetToken_getHash(this TMUser tmUser, Guid resetToken)
        {
            if(tmUser.notNull())                
                return tmUser.UserName.hash_PBKDF2(resetToken);
            return null;
        }
        public static Guid      passwordResetToken_getHash(this string email)
        {
            var tmUser = email.tmUser_FromEmail();
            if (tmUser.notNull())
                return tmUser.passwordResetToken_getTokenAndSetHash();
            return Guid.NewGuid();
        }
        public static Guid      passwordResetToken_getTokenAndSetHash(this TMUser tmUser)
        {
            if (tmUser.notNull())
            {
                var newToken = Guid.NewGuid();
                tmUser.SecretData.PasswordResetToken = tmUser.passwordResetToken_getHash(newToken);                
                tmUser.event_User_Updated();  //tmUser.saveTmUser();
                return newToken;
            }
            return Guid.Empty;
        }
        public static string    passwordExpiredUrl(this TM_User user)
        {
            if (user.notNull())
            {
                var tmUser = user.UserName.tmUser();
                if (tmUser.notNull())
                    if (tmUser.password_Expired())
                    {
                        tmUser.logUserActivity("Password Expired", "Created password Reset Url for user");
                        return "/passwordReset/{0}/{1}".format(user.UserName, tmUser.passwordResetToken_getTokenAndSetHash());
                    }                                    
            }
            return "/error";
        }

        public static string    userHostAddress     (this TMUser tmUser)
        {
            try
            {
                return HttpContextFactory.Request.UserHostAddress;
            }
            catch (Exception ex)
            {
                ex.log("in userHostAddress");
                return "0.0.0.0";
            }
        }
        public static string    fullName                (this TMUser tmUser)
        {
            if (tmUser.notNull())
                if (tmUser.FirstName.valid())
                {
                    if (tmUser.LastName.valid())
                        return "{0} {1}".format(tmUser.FirstName, tmUser.LastName);
                    return tmUser.FirstName;
                }
            return "";
        } 

        public static TMUser    enableUser_UsingToken   (this Guid token)
        {
            var tmUser = token.enableUser_UserForToken();
            if (tmUser.notNull())
            {
                tmUser.AccountStatus.UserEnabled     = true;
                tmUser.SecretData.EnableUserToken = Guid.Empty;
                tmUser.logUserActivity("User Enabled", "Using Token: {0}".format(token));
            }
            return tmUser;
        }
        public static Guid      enableUser_Token        (this TMUser user)
        {
            if (user.SecretData.EnableUserToken == Guid.Empty)
                user.SecretData.EnableUserToken = Guid.NewGuid();
            return  user.SecretData.EnableUserToken;
        }
        public static bool      enableUser_IsTokenValid (this Guid token)
        {
            return token.enableUser_UserForToken() != null;
        }
        public static TMUser    enableUser_UserForToken (this Guid token)
        {
            if (token == Guid.Empty)
                return null;
            return (from tmUser in TM_UserData.Current.tmUsers()
                    where tmUser.SecretData.EnableUserToken == token
                    select tmUser).first();
        }

        public static TMUser    set_UserGroup        (this TMUser tmUser, UserGroup userGroup)
        {
            if (tmUser.notNull())
                tmUser.GroupID = (int)userGroup;
            return tmUser;
        }
        public static TMUser    make_Admin           (this TMUser tmUser)
        {
            return tmUser.set_UserGroup(UserGroup.Admin);            
        }
        public static TMUser    make_Editor          (this TMUser tmUser)
        {
            return tmUser.set_UserGroup(UserGroup.Editor);            
        }        
        public static TMUser    make_Reader          (this TMUser tmUser)
        {
            return tmUser.set_UserGroup(UserGroup.Reader);            
        }
        public static TMUser    make_Viewer          (this TMUser tmUser)
        {
            return tmUser.set_UserGroup(UserGroup.Viewer);            
        }

        public static bool      isAdmin              (this TMUser tmUser)        
        {
            return tmUser.GroupID == (int)UserGroup.Admin;
        }
        public static bool      isEditor             (this TMUser tmUser)        
        {
            return tmUser.GroupID == (int)UserGroup.Editor;
        }
        public static bool      isReader             (this TMUser tmUser)        
        {
            return tmUser.GroupID == (int)UserGroup.Reader;
        }
        public static bool      isViewer             (this TMUser tmUser)        
        {
            return tmUser.GroupID == (int)UserGroup.Viewer;
        }
        public static List<UserTag> userTags         (this TMUser tmUser)
        {
            return tmUser.notNull() && tmUser.UserTags.notNull()
                        ? tmUser.UserTags
                        : new List<UserTag>();
        }
        public static WhoAmI    whoAmI               (this TMUser tmUser)        
        {
            var whoAmI = new WhoAmI();
            if (tmUser.notNull())
            {
                whoAmI.UserName = tmUser.UserName;
                whoAmI.UserId = tmUser.UserID;
                whoAmI.GroupId = tmUser.GroupID;
                whoAmI.GroupName = tmUser.userGroup().str();
                whoAmI.UserRoles = tmUser.userRoles().toStringArray().toList().join(",");
            }
            return whoAmI;
        }
    }
}
﻿using System;
using NUnit.Framework;
using O2.DotNetWrappers.ExtensionMethods;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.TM_XmlDatabase
{
    [TestFixture][Assert_Admin]
    public class Test_Users_Sessions : TM_XmlDatabase_InMemory
    {
        [Test] public void TMUserLoginAndLogout()
        {
            var userId                   = userData.newUser();
            var tmUser                   = userId.tmUser();            
            var sessionId                = tmUser.login();
            var validSessionAfterLogin   = sessionId.validSession();
            var logoutResult             = tmUser.logout();
            var validSessionAfterLogout  = sessionId.validSession();
            var logoutResult2            = tmUser.logout();
            var validSessionAfterLogout2 = sessionId.validSession();

            Assert.Less        (0,userId);
            Assert.IsNotNull   (tmUser);
            Assert.AreNotEqual (Guid.Empty, sessionId);            
            Assert.IsTrue      (logoutResult);
            Assert.IsTrue      (validSessionAfterLogin);
            Assert.IsFalse     (validSessionAfterLogout);            
            Assert.IsFalse     (logoutResult2);
            Assert.IsFalse     (validSessionAfterLogout2);
        }
        [Test] public void GuidLoginAndLogout()
        {
            var sessionId = userData.newUser()
                                    .tmUser()
                                    .login();
            var validSessionAfterLogin  = sessionId.validSession();
            var logoutResult            = sessionId.logout();
            var validSessionAfterLogout = sessionId.validSession();

            Assert.IsTrue      (logoutResult);
            Assert.IsTrue      (validSessionAfterLogin);
            Assert.IsFalse     (validSessionAfterLogout);
        }        
        [Test] public void UserAccount_Enabled_and_Disabled()
        {            
            var testUser  = "user".add_RandomLetters(10);                       
            var testPwd   = "!!Pwd".add_RandomLetters(10);
            var tmUser    = userData.newUser(testUser, testPwd).tmUser();                           //create test user     
            var sessionId = userData.login(testUser, testPwd);

            Assert.IsTrue     (tmUser.account_Enabled(), "New user account should be enabled");
            Assert.AreNotEqual(Guid.Empty, sessionId   , "login should work");

            tmUser.disable_Account();                                                               // disable account
            sessionId = userData.login(testUser, testPwd);

            Assert.IsFalse    (tmUser.account_Enabled(), "Account should be disabled now");
            Assert.AreEqual   (Guid.Empty, sessionId   , "After disable, login should not work");

            tmUser.enable_Account();                                                               // re-enable account
            sessionId = userData.login(testUser, testPwd);

            Assert.IsTrue     (tmUser.account_Enabled(), "Account should be enabled now");
            Assert.AreNotEqual(Guid.Empty, sessionId   , "After enable, login should not work");
        }
        [Test] public void UserAccount_Expired()
        {
            tmConfig.TMSecurity.EvalAccounts_Enabled  = true;
            tmConfig.TMSecurity.EvalAccounts_Days     = 15;
            Assert.AreEqual(15  , TMConfig.Current.TMSecurity.EvalAccounts_Days         , "Eval_Accounts.Days");
            Assert.AreEqual(true, TMConfig.Current.TMSecurity.EvalAccounts_Enabled      , "Eval_Accounts.Enabled");

            var tmUser = userData.newUser().tmUser();

            var expirationDate              = tmUser.AccountStatus.ExpirationDate                        .ToLongDateString();
            var expectedExpirationDate      = DateTime.Now.AddDays(tmConfig.TMSecurity.EvalAccounts_Days).ToLongDateString();
            var calculatedExpirationDate    = tmConfig.currentExpirationDate()                           .ToLongDateString(); 

            var isExpired_Before_Expire_Account      = tmUser.account_Expired();
            tmUser.expire_Account();
            var isExpired_After_Expire_Account       = tmUser.account_Expired();
            tmConfig.TMSecurity.EvalAccounts_Enabled = false;
            var isExpired_EvalAccounts_Disable       = tmUser.account_Expired();

            Assert.IsFalse(isExpired_Before_Expire_Account          , "before expire, user should Not be expired");
            Assert.IsTrue (isExpired_After_Expire_Account           , "after expired user should be expired");
            Assert.IsTrue (isExpired_EvalAccounts_Disable           , "user account should still be disabled (regardless of the EvalAccounts_Enabled value)");
            Assert.AreEqual(expirationDate, expectedExpirationDate  , "expirationDate != expectedExpirationDate");
            Assert.AreEqual(expirationDate, calculatedExpirationDate, "expirationDate != calculatedExpirationDate");
        }

        [Test]
        public void UserAccount_EvalAccounts_Behaviour()
        {
            tmConfig.TMSecurity.EvalAccounts_Enabled  = true;
            var tmUser1                               = userData.newUser().tmUser();
            var expirationDate_NewUser_Eval_Enabled   = tmUser1.AccountStatus.ExpirationDate;
            tmConfig.TMSecurity.EvalAccounts_Enabled  = false;
            var tmUser2                               = userData.newUser().tmUser();
            var expirationDate_NewUser_Eval_Disabled  = tmUser2.AccountStatus.ExpirationDate;

            Assert.IsNotNull(tmUser2, "tmUser1");
            Assert.IsNotNull(tmUser2, "tmUser2");
            Assert.AreNotEqual(expirationDate_NewUser_Eval_Enabled , default(DateTime) , "ExpirationDate should be set when EvalAccounts_Enabled is true");
            Assert.AreEqual   (expirationDate_NewUser_Eval_Disabled, default(DateTime) , "ExpirationDate should NOT be set when EvalAccounts_Enabled is false");

            "tmUser1: {0}".info(tmUser1.toXml());
            "tmUser2: {0}".info(tmUser2.toXml());
        }

        [Test] public void UserPassword_Expired()
        {
            var tmUser        = userData.newUser().tmUser();

            Assert.IsFalse    (tmUser.password_Expired());

            tmUser            .expire_Password();

            Assert.IsTrue     (tmUser.password_Expired());

            var newPassword   = "QAsdf1234!";
            tmUser            .setPassword(newPassword);    
  
            Assert.IsFalse    (tmUser.password_Expired(), "Password expiry should not be set after password change");
            Assert.AreNotEqual(Guid.Empty               , userData.login(tmUser.UserName, newPassword));
        }        
        [Test, Ignore("under dev")]
        public void PasswordComplexity()
        {
            
        }
    }
}

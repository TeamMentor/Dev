﻿using System;
using NUnit.Framework;
using O2.DotNetWrappers.ExtensionMethods;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.TM_XmlDatabase
{
    [TestFixture]
    class Test_Users_Management : TM_XmlDatabase_InMemory
    {		
        [TestFixtureSetUp]
        public void setUp()
        {
            UserGroup.Admin.setThreadPrincipalWithRoles();	
        }

        [Test] public void CreateLoginDelete_User()
        {			
            string testUserName = "testUser_".add_RandomLetters(5);
            string password = "123";

            Assert.IsNull(userData.tmUser(testUserName), "testUserName shouldn't exist before create");
            //create user
            var userCount = userData.TMUsers.size();
            var userId = userData.newUser(testUserName, password);			
            Assert.AreEqual(userCount+1, tmXmlDatabase.UserData.TMUsers.size() , "TMUsers,size() after create");
            var tmUser = userData.tmUser(userId);
            Assert.NotNull(tmUser, "tmUser was null after create");
            Assert.AreEqual(tmUser.UserID, userId, "UserID");
            Assert.AreEqual(tmUser.UserName, testUserName, "UserID");

            //Login with new user
            var sessionId = userData.login(testUserName, password);
            Assert.AreNotEqual(Guid.Empty, sessionId, "empty session id");
            sessionId = userData.login(testUserName, "BAD PWD");
            Assert.AreEqual(Guid.Empty, sessionId, "session id should be empty");

            UserGroup.Admin.setThreadPrincipalWithRoles();  //assert admin so that we can delete the user
            //delete user			
            var result = userData.deleteTmUser(tmUser.UserID);
            Assert.That(result, "User delete");
            tmUser = userData.tmUser(userId);
            Assert.IsNull(tmUser, "tmUser should be null after delete");			
        }
        [Test] public void PBKDF2_Multiple()
        {
            var password = "!1234567!";
            var salt = Guid.NewGuid().crypto().str();	        
            "{0}\n{1}\n-------\n".info(password, salt);
            for (int i = 1; i < 6; i++)
            {
                var now = DateTime.Now;
                var interactions = i * 5000;                
                var passwordHash = password.hash_PBKDF2(salt, interactions,64);
                var timeSpan = DateTime.Now - now;	            
                "password: {0}\ninteractions: {1}  timespan: {2}\n".info(passwordHash, interactions, timeSpan);
                
                Assert.NotNull  (passwordHash);
                Assert.AreEqual (64, passwordHash.base64Decode_AsByteArray().size());
                Assert.Less     (timeSpan.Seconds,2);             // slowest calculation should be faster than 2 seconds
                Assert.Greater  (timeSpan.TotalMilliseconds, 50); // slowest calculation should be slower than 50 milliseconds
            }
        }
        [Test] public void PBKDF2_Default()
        {
            20000.set_DEFAULT_PBKDF2_INTERACTIONS(); // restore this value since that is what we are testing
            Action<string,string> checkPassword = 
                (password, salt) =>
                    {
                        var now = DateTime.Now;
                        var passwordHash = password.hash_PBKDF2(salt);
                        var timeSpan = DateTime.Now - now;	            

                        Assert.NotNull  (passwordHash);
                        Assert.AreEqual (64, passwordHash.base64Decode_AsByteArray().size());
                        Assert.AreEqual (0,timeSpan.Seconds);
                        Assert.Less     (100, timeSpan.TotalMilliseconds); // slowest calculation should be slower than 500 milliseconds                        
                        "ok: {0} : {1}".info(timeSpan.Milliseconds,passwordHash);
                    };
            checkPassword("!1234567!", Guid.NewGuid().crypto().str()); // normal values
            checkPassword("a", Guid.NewGuid().crypto().str());         // weak password
            checkPassword("!1234567!", "a");                  // weak salt
            checkPassword("", Guid.NewGuid().crypto().str());          // no password
            checkPassword("!1234567!", "");                   // no salt
            checkPassword("","");                             // no password and no salt	        
        }
        [Test] public void ResolveUser_ByNameEmailAndId()
        {
            string userName     = "user_".add_RandomLetters(5);            
            string password     = "pwd_".add_RandomLetters(5);         
            string email        = "testUser@teammentor.net";
            var newUserId       = userData.newUser(userName, password, email);
            var tmUser_ById     = newUserId.tmUser();
            var tmUser_ByName   = userName.tmUser();
            var tmUser_ByEmail  = email.tmUser_FromEmail();

            Assert.Greater  (newUserId, 0);
            Assert.NotNull  (tmUser_ById        ,"tmUser_byId");
            Assert.NotNull  (tmUser_ByName      ,"tmUser_byName");
            Assert.NotNull  (tmUser_ByEmail     ,"tmUser_byEmail");
            Assert.IsTrue   (tmUser_ById == tmUser_ByName &&  tmUser_ById ==tmUser_ByEmail);

            //Creating another user with the same email
            string userName2 = userName + "AA";            
            var newUser2Id    = userData.newUser(userName2, password, email);
            var tmUser2_ById     = newUser2Id.tmUser();
            var tmUser2_ByName   = userName2.tmUser();
            var tmUser2_ByEmail  = email.tmUser_FromEmail();

            Assert.Greater  (newUser2Id, 0);
            Assert.NotNull  (tmUser2_ById        ,"tmUser2_byId");
            Assert.NotNull  (tmUser2_ByName      ,"tmUser2_byName");
            Assert.IsNull   (tmUser2_ByEmail     ,"tmUser2_byEmail should not work for repeated emails");
            Assert.IsTrue   (tmUser2_ById == tmUser2_ByName && tmUser2_ById != tmUser2_ByEmail);
            Assert.IsTrue   (tmUser2_ById != tmUser_ByName  &&  tmUser2_ById != tmUser_ByEmail);
        }


    }
}

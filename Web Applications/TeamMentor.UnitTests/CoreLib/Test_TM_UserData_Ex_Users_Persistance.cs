using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.CoreLib.Users
{
    [TestFixture]
    internal class TM_UserData_Ex_Users_Persistance_Tests
    {
        private static TMUser CreateTMUser
        {
            get
            {
                var tmUser = new TMUser
                {
                    EMail = "a@a.com",
                    UserName = "User1",
                    FirstName = "FName",
                    LastName = "LName",
                    Title = "Mr",
                    Company = "Company",
                    Country = "Country",
                    State = "State",
                    GroupID = 1
                };

                tmUser.AccountStatus.PasswordExpired = false;
                tmUser.AccountStatus.UserEnabled = true;

                return tmUser;
            }
        }

        [Test]
        public void Cannot_Update_User_With_Invalid_Email()
        {
            TMUser tmUser = CreateTMUser;

            const string invalidEmail = "notvalid";

            Assert.Throws(typeof(ValidationException),
                          () =>
                          tmUser.updateTmUser(tmUser.UserName, tmUser.FirstName, tmUser.LastName, tmUser.Title,
                                              tmUser.Company, invalidEmail, tmUser.Country, tmUser.State,
                                              tmUser.AccountStatus.PasswordExpired, tmUser.AccountStatus.UserEnabled,
                                              tmUser.GroupID));
        }

        [Test]
        public void Can_Update_User_With_Valid_Email()
        {
            TMUser tmUser = CreateTMUser;

            const string validEmail = "valid@valid.com";

            Assert.Throws(typeof(NullReferenceException),
                          () =>
                          tmUser.updateTmUser(tmUser.UserName, tmUser.FirstName, tmUser.LastName, tmUser.Title,
                                              tmUser.Company, validEmail, tmUser.Country, tmUser.State,
                                              tmUser.AccountStatus.PasswordExpired, tmUser.AccountStatus.UserEnabled,
                                              tmUser.GroupID));
        }
    }
}
using System;
using NUnit.Framework;
using TeamMentor.CoreLib;

namespace TeamMentor.UnitTests.CoreLib
{
    [TestFixture]
    internal class Test_TM_UserData_Ex_Users_Persistance
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
                                     GroupID = 1,
                                     AccountStatus = {PasswordExpired = false, UserEnabled = true}
                                 };


                return tmUser;
            }
        }

        [Test]
        public void Can_Update_User_With_Valid_Email()
        {
            TMUser tmUser = CreateTMUser;

            Assert.Throws(typeof (NullReferenceException),
                          () =>
                          tmUser.updateTmUser(tmUser.user()));
        }

        [Test]
        public void Cannot_Update_User_With_Invalid_Email()
        {
            TMUser tmUser = CreateTMUser;

            const string invalidEmail = "notvalid";

            TM_User userViewModel = tmUser.user();
            userViewModel.Email = invalidEmail;

            bool success = tmUser.updateTmUser(userViewModel);

            Assert.IsFalse(success);
        }
    }
}
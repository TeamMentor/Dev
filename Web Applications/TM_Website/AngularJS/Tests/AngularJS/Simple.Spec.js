describe('Test Angular Simple page', function()
    {
        beforeEach(function() 
            {            
                browser().navigateTo('/Tests/AngularJS/Simple.html');            
            });
        it('Open Webpage and check URL', function()
            {                
                expect(browser ().window().path()).toBe("/Tests/AngularJS/Simple.html");
                expect(browser ().location().path()).toBe("");
                browser().navigateTo('/Tests/AngularJS/aaa.html');
                expect(browser ().window().path()).not().toBe("/Tests/AngularJS/Simple.html");
                expect(browser ().window().path()).toBe("/Tests/AngularJS/aaa.html");
            });

        it('Set Field value and check Angular scope update', function()
            {
                var testValue = "This is a value";
                var expectedValue = "Hello "  + testValue + "!";
                input('yourName').enter(testValue);
                expect(element('.ng-binding').text()).toEqual(expectedValue);
            });
    });

describe('Markdown Editor - View funcionality', function()
    {
        it("Should open", function()
            {
                browser().navigateTo('/Markdown/Editor');
                expect(browser ().window().path()).toBe("/Markdown/Editor");
                expect(browser ().location().path()).toBe("");
                //sleep(10);
            });
        it("Should fail", function()
            {
               expect("aaa").toBe("aaa");
            });
    });
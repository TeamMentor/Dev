module.exports = function(karma)
{
    karma.configure({
        frameworks: ['ng-scenario'],

        files:
            [
                '../Tests/**/*.Spec.js'
            ],

        urlRoot: '/__karma/',

        autoWatch: true,

        proxies: {
            '/'     : 'http://localhost:12120/',
            '/Tests': 'http://localhost:12120/AngularJs/Tests/'

        },

        //browsers: ['Chrome'],

        reporters: ['dots'],
        //reporters: ['progress'],
        plugins: [
                    'karma-ng-scenario',
                    'karma-chrome-launcher'
                    //,'karma-firefox-launcher'
                 ]  ,
        //logLevel: karma.LOG_DEBUG
        logLevel: karma.LOG_INFO
    });
};

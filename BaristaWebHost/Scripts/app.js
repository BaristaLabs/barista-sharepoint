var baristaFiddle = angular.module('baristafiddle', [
    'ui.bootstrap',
    'ui.codemirror',
    'ui.router.state',
    'kendo.directives',
    'ngStorage',
    'snap'
]);

baristaFiddle.config(function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise('/');
});

baristaFiddle.directive('focusOn', function () {
    return function (scope, elem, attr) {
        scope.$on('focusOn', function (e, name) {
            if (name === attr.focusOn) {
                elem[0].focus();
            }
        });
    };
});

baristaFiddle.directive('selectOn', function () {
    return function (scope, elem, attr) {
        scope.$on('selectOn', function (e, name) {
            if (name === attr.selectOn) {
                elem[0].select();
            }
        });
    };
});

baristaFiddle.factory('focus', function ($rootScope, $timeout) {
    return function (name) {
        $timeout(function () {
            $rootScope.$broadcast('focusOn', name);
        });
    }
});

baristaFiddle.factory('select', function ($rootScope, $timeout) {
    return function (name) {
        $timeout(function () {
            $rootScope.$broadcast('selectOn', name);
        });
    }
});


(function () {
    "use strict";

    CodeMirror.registerHelper("hint", "barista", function (editor, options) {
        var cur = editor.getCursor(), curLine = editor.getLine(cur.line);
        var start = cur.ch, end = start;

        
        return { list: list, from: CodeMirror.Pos(cur.line, start), to: CodeMirror.Pos(cur.line, end) };
    });
})();
var baristaFiddle = angular.module('baristafiddle', [
    'ui.bootstrap',
    'ui.codemirror',
    'ui.state',
    'kendo.directives'
]);

baristaFiddle.config(function ($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise('/');
});
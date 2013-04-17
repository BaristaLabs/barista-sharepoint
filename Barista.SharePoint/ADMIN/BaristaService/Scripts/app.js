window.managebarista = angular.module('managebarista', ['ui', 'ui.bootstrap', 'kendo.directives']);

// Configure routes
managebarista.config(['$routeProvider', function($routeProvider) {
    $routeProvider.
        when('/', { templateUrl: 'views/main.html', controller: 'ManageMainCtrl' }).
        when('/indexes', { templateUrl: 'views/indexes.html', controller: 'ManageIndexesCtrl' }).
        when('/about', { templateUrl: 'views/about.html', controller: 'AboutBaristaCtrl' }).
        otherwise({ redirectTo: '/' });
}])
    .directive('stopEvent', function() {
        return {
            restrict: 'A',
            link: function(scope, element, attr) {
                element.bind(attr.stopEvent, function(e) {
                    e.stopPropagation();
                });
            }
        }
    });


//Main Controller
function ManageMainCtrl($scope, $window, $http, $templateCache) {

    $scope.tabs = [
        {
            title: "General",
            route: "#/",
        },
        {
            title: "Indexes",
            route: "#/indexes",
        }
    ];

    $scope.addNewLocation = function() {
        var grid = $("#trustedLocationsGrid").data("kendoGrid");
        grid.addRow();
    };

    $scope.reload = function ($event, route) {
        $event.preventDefault();
        $window.location.href = route;
    };

    $scope.openModal = function () {
        $scope.shouldBeOpen = true;
    };

    $scope.closeModal = function () {
        $scope.closeMsg = 'I was closed at: ' + new Date();
        $scope.shouldBeOpen = false;
    };

    $scope.opts = {
        backdropFade: true,
        dialogFade: true
    };
}
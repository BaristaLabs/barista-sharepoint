var manageBarista = angular.module('managebarista', ['ui.bootstrap', 'ngGrid']);
manageBarista.config(['$locationProvider', function($locationProvider) {
    $locationProvider.html5Mode(true);
}]);

//Main Controller
manageBarista.controller("ManageMainCtrl", ['$scope', '$http', '$modal',
	function ($scope, $http, $modal) {

	    $scope.baristaBaseUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";

	}]);
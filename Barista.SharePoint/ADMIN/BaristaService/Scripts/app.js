var manageBarista = angular.module('managebarista', ['ui.bootstrap', 'ngGrid']);
manageBarista.config(['$locationProvider', function($locationProvider) {
    //Don't use html5Mode since it blows up all versions of IE under CA because the CA uses a compatability mode of 8
    //$locationProvider.html5Mode(true);
}]);

//Main Controller
manageBarista.controller("ManageMainCtrl", ['$scope', '$http', '$modal', '$window',
	function ($scope, $http, $modal, $window) {

	    $scope.getQueryVariable = function (variable) {
	        var query = $window.location.search.substring(1);
	        var vars = query.split('&');
	        for (var i = 0; i < vars.length; i++) {
	            var pair = vars[i].split('=');
	            if (decodeURIComponent(pair[0]) == variable) {
	                return decodeURIComponent(pair[1]);
	            }
	        }
	    }

	    $scope.baristaBaseUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";

	}]);
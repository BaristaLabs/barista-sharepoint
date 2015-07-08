manageBarista.controller("ManageGeneralCtrl", ['$scope', '$http', '$location', '$modal',
	function ($scope, $http, $location, $modal) {

	    $scope.trustedLocations = null;

	    $scope.addNewLocation = function () {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/addedittrustedlocationdialog.html",
                    controller: "AddEditTrustedLocationCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        trustedLocation: function () { return null; },
                        isNew: function () { return true; }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getTrustedLocations();
                });
	    };

	    $scope.editLocation = function (trustedLocation) {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/addedittrustedlocationdialog.html",
                    controller: "AddEditTrustedLocationCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        trustedLocation: function () { return trustedLocation; },
                        isNew: function () { return false; }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getTrustedLocations();
                });
	    };

	    $scope.removeLocation = function (trustedLocation) {
	        var msgbox = $modal.open({
	            templateUrl: "confirmRemoveLocation.html"
	        });

	        msgbox.result.then(function(result) {
	            toastr.info('Removing Trusted Location...');
	            $http({
	                    method: "POST",
	                    url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/DeleteTrustedLocation.js',
	                    params: {
	                        serviceApplicationId: $location.search()["appid"]
	                    },
	                    data: trustedLocation
	                })
	                .success(function(trustedLocations) {
	                    toastr.success("Removed Trusted Location!");
	                    $scope.getTrustedLocations();
	                })
	                .error(function(trustedLocations) {
	                    toastr.error("Unable to remove Trusted Location :(<br/>" + $(trustedLocations).children(".intro").text());
	                });
	        });
	    };

	    $scope.getTrustedLocations = function() {
	        toastr.options = {
	            "debug": false,
	            "positionClass": "toast-bottom-right",
	            "onclick": null,
	            "fadeIn": 300,
	            "fadeOut": 1000,
	            "timeOut": 5000,
	            "extendedTimeOut": 1000
	        };

	        toastr.info('Loading Trusted Locations...');
	        $http({
	                method: "GET",
	                url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/GetTrustedLocations.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                }
	            })
	            .success(function(trustedLocations) {

	                toastr.success("Loaded!");
	                toastr.clear();

	                $scope.trustedLocations = trustedLocations;
	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function(trustedLocations) {
	                toastr.error("Unable to load Trusted Locations :(<br/>" + $(trustedLocations).children(".intro").text());
	            });
	    };

	    $scope.getTrustedLocations();

	    $scope.gridOptions = {
	        data: 'trustedLocations',
	        enableColumnResize: true,
	        columnDefs: [
                { field: 'Url', displayName: 'Url' },
                { field: 'Description', displayName: 'Description' },
                { field: 'LocationType', displayName: 'Location Type' },
                { field: 'TrustChildren', displayName: 'Trust Children' },
                { field: '', displayName: '', cellTemplate: '<div class="btn btn-xs btn-primary" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="editLocation(row.entity)"><i class="icon-edit"></i>Edit</div><div class="btn btn-xs btn-danger" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="removeLocation(row.entity)"><i class="icon-remove"></i>Remove</div>' }
	        ]
	    };
	}]);

manageBarista.controller('AddEditTrustedLocationCtrl', ['$scope', '$http', '$location', '$modalInstance', 'baristaBaseUrl', 'trustedLocation', 'isNew',
	function ($scope, $http, $location, $modalInstance, baristaBaseUrl, trustedLocation, isNew) {
	    $scope.isNew = isNew;
	    $scope.trustedLocation = trustedLocation;

	    if ($scope.trustedLocation == null)
	        $scope.trustedLocation = {};

	    $scope.addOrUpdate = function () {
	        if ($scope.isNew)
	            toastr.info('Adding Trusted Location...');
	        else
	            toastr.info('Updating Trusted Location...');
	        $http({
	                method: "POST",
	                url: baristaBaseUrl + '/_admin/BaristaService/API/AddOrUpdateTrustedLocation.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                },
	                data: $scope.trustedLocation
	            })
	            .success(function(trustedLocations) {
	                if ($scope.isNew)
	                    toastr.success('Added Trusted Location!');
	                else
	                    toastr.success('Updated Trusted Location!');
	                $modalInstance.close(trustedLocations);
	            })
	            .error(function(trustedLocations) {
	                if ($scope.isNew)
	                    toastr.error("Unable to add Trusted Location :(<br/>" + $(trustedLocations).children(".intro").text());
	                else
	                    toastr.error("Unable to update Trusted Location :(<br/>" + $(trustedLocations).children(".intro").text());
	            });
	    };

	    $scope.cancel = function () {
	        $modalInstance.dismiss();
	    };
	}]);
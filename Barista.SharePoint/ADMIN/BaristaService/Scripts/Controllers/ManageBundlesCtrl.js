manageBarista.controller('ManageBundlesCtrl', ['$scope', '$http', '$location', '$modal',
	function ($scope, $http, $location, $modal) {

	    $scope.bundles = null;
        $scope.bundleWhitelist = null

	    $scope.editBundleApproval = function (index) {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/editbundleapprovaldialog.html",
                    controller: "EditBundleApprovalCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        index: function () { return index; }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getBundles();
                });
	    };

	    $scope.getBundles = function() {
	        toastr.options = {
	            "debug": false,
	            "positionClass": "toast-bottom-right",
	            "onclick": null,
	            "fadeIn": 300,
	            "fadeOut": 1000,
	            "timeOut": 5000,
	            "extendedTimeOut": 5000
	        };

	        toastr.info('Loading Indexes...');
	        $http({
	                method: "GET",
	                url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/GetBundlesWithApproval.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                },
	            })
	            .success(function(indexes) {

	                toastr.success("Loaded!");
	                toastr.clear();

	                $scope.indexes = indexes;
	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function(indexes) {
	                toastr.error("Unable to load Bundles :(<br/>" + $(index).children(".intro").text());
	            });
	    };

	    $scope.getIndexes();

	    $scope.gridOptions = {
	        data: 'bundles',
	        enableColumnResize: true,
	        columnDefs: [
                { field: 'name', displayName: 'Name' },
                { field: 'version', displayName: 'Version' },
                { field: 'description', displayName: 'Description' },
                { field: '', displayName: '', cellTemplate: '<div class="btn btn-xs btn-primary" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="editBundleApproval(row.entity)"><i class="icon-edit"></i>Edit Approval</div>' }
	        ]

	    };
	}]);

manageBarista.controller('EditBundleApprovalCtrl', ['$scope', '$http', '$location', '$modalInstance', 'baristaBaseUrl', 'bundle',
	function ($scope, $http, $location, $modalInstance, baristaBaseUrl, bundle) {
	    $scope.bundle = bundle;

	    $scope.update = function () {
	        toastr.info('Updating Bundle Approval...');
	        $http({
	                method: "POST",
	                url: baristaBaseUrl + '/_admin/BaristaService/API/UpdateBundleApproval.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                },
	                data: $scope.bundle.approval
	            })
	            .success(function(index) {
	                toastr.success('Updated Bundle Approval!');
	                $modalInstance.close(index);
	            })
	            .error(function(index) {
	                toastr.error("Unable to update Bundle Approval  :(<br/>" + $(index).children(".intro").text());
	            });
	    };

	    $scope.cancel = function () {
	        $modalInstance.dismiss();
	    };
	}]);
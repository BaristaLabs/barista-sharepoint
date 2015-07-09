manageBarista.controller('ManagePackagesCtrl', ['$scope', '$http', '$location', '$modal',
	function ($scope, $http, $location, $modal) {

	    $scope.packages = null;

	    $scope.editPackageApproval = function (baristaPackage) {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/editPackageapprovaldialog.html",
                    controller: "EditPackageApprovalCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        baristaPackage: function () { return angular.copy(baristaPackage); }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getPackages();
                });
	    };

	    $scope.getPackages = function () {
	        toastr.options = {
	            "debug": false,
	            "positionClass": "toast-bottom-right",
	            "onclick": null,
	            "fadeIn": 300,
	            "fadeOut": 1000,
	            "timeOut": 5000,
	            "extendedTimeOut": 5000
	        };

	        toastr.info('Loading Packages...');
	        $http({
	            method: "GET",
	            url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/GetPackagesWithApproval.js',
	            params: {
	                serviceApplicationId: $location.search()["appid"]
	            },
	        })
	            .success(function (packages) {

	                toastr.success("Loaded Packages!");
	                toastr.clear();

	                $scope.packages = packages;
	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function (packages) {
	                toastr.error("Unable to load Packages :(<br/>" + $(packages).children(".intro").text());
	            });
	    };

	    $scope.getPackages();

	    $scope.getPackageBackgroundColor = function (baristaPackage) {
	        if (baristaPackage.approval.approvalLevel === 'approved')
	            return "#DFF0D8";
	        return "#F2DEDE";
	    };

	    $scope.gridOptions = {
	        data: 'packages',
	        enableColumnResize: true,
	        columnDefs: [
                { field: 'name', displayName: 'Name' },
                { field: 'version', displayName: 'Version' },
                { field: 'description', displayName: 'Description' },
                { field: '', displayName: '', cellTemplate: '<div class="btn btn-xs btn-primary" style="margin-left: 10px; margin-top: 5px; width: 95px;" ng-click="editPackageApproval(row.entity)"><i class="icon-edit"></i>Edit Approval</div>' }
	        ],
	        rowTemplate: '<div ng-style="{\'cursor\': row.cursor, \'z-index\': col.zIndex(), \'background-color\': getPackageBackgroundColor(row.entity)}" ng-repeat="col in renderedColumns" ng-class="col.colIndex()" class="ngCell {{col.cellClass}}" ng-cell></div>'
	    };
	}]);

manageBarista.controller('EditPackageApprovalCtrl', ['$scope', '$http', '$location', '$modalInstance', 'baristaBaseUrl', 'baristaPackage',
	function ($scope, $http, $location, $modalInstance, baristaBaseUrl, baristaPackage) {
	    $scope.baristaPackage = baristaPackage;

	    $scope.update = function () {
	        toastr.info('Updating Package Approval...');
	        $http({
	            method: "POST",
	            url: baristaBaseUrl + '/_admin/BaristaService/API/UpdatePackageApproval.js',
	            params: {
	                serviceApplicationId: $location.search()["appid"]
	            },
	            data: $scope.baristaPackage
	        })
	            .success(function (baristaPackage) {
	                toastr.success('Updated Package Approval!');
	                $modalInstance.close(baristaPackage);
	            })
	            .error(function (baristaPackage) {
	                toastr.error("Unable to update baristaPackage Approval  :(<br/>" + $(baristaPackage).children(".intro").text());
	            });
	    };

	    $scope.cancel = function () {
	        $modalInstance.dismiss();
	    };
	}]);
manageBarista.controller('ManagePackagesCtrl', ['$scope', '$http', '$modal',
	function ($scope, $http, $modal) {

	    $scope.packages = null;

	    $scope.editPackageApproval = function(baristaPackage) {

	        var modalInstance = $modal.open(
	        {
	            templateUrl: "Views/editPackageapprovaldialog.html",
	            controller: "EditPackageApprovalCtrl",
	            modalFade: true,
	            dialogFade: true,
	            resolve: {
	                baristaBaseUrl: function() { return $scope.baristaBaseUrl; },
	                baristaPackage: function() { return angular.copy(baristaPackage); },
	                doesBaristaPackageMatchApproval: function() { return $scope.doesBaristaPackageMatchApproval; }
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
	                serviceApplicationId: $scope.getQueryVariable("appid")
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
	                toastr.error("Unable to load Packages :(<br/>" + $(packages).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.savePackagesAsDefault = function () {
	        toastr.info('Saving current Package Approvals as default for new service applications...');
	        $http({
	            method: "GET",
	            url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/SavePackageApprovalsAsDefault.js',
	            params: {
	                serviceApplicationId: $scope.getQueryVariable("appid")
	            }
	        })
	            .success(function () {

	                toastr.success("Saved Package Approvals as default!");
	                toastr.clear();

	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function (result) {
	                toastr.error("Unable to save Package Approvals as default:(<br/>" + $(result).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.doesBaristaPackageMatchApproval = function(baristaPackage) {
	        if (!baristaPackage.packageInfo || !baristaPackage.approval.packageInfo)
	            return false;

	        if (!baristaPackage.packageInfo.bundles || !baristaPackage.approval.packageInfo.bundles)
	            return false;

	        if (!angular.isArray(baristaPackage.packageInfo.bundles) || !angular.isArray(baristaPackage.approval.packageInfo.bundles))
	            return false;

	        if (baristaPackage.packageInfo.bundles.length !== baristaPackage.approval.packageInfo.bundles.length)
	            return false;

	        var bundlesMatch = true;

	        _.forEach(baristaPackage.packageInfo.bundles, function(bundle) {
	            var foundApprovedBundle = _.find(baristaPackage.approval.packageInfo.bundles, function(approvedBundle) {
	                return (bundle.bundleTypeFullName.toUpperCase() === approvedBundle.bundleTypeFullName.toUpperCase() &&
                            bundle.assemblyPath.toUpperCase() === approvedBundle.assemblyPath.toUpperCase() &&
                            bundle.assemblyFullName.toUpperCase() === approvedBundle.assemblyFullName.toUpperCase() &&
                            bundle.assemblyHash === approvedBundle.assemblyHash)

	            });

	            if (!foundApprovedBundle)
	                bundlesMatch = false;
	        });

	        return bundlesMatch;
	    };

	    $scope.getPackageBackgroundColor = function (baristaPackage) {
	        if (baristaPackage.approval.approvalLevel.toUpperCase() === 'approved'.toUpperCase()) {

	            if ($scope.doesBaristaPackageMatchApproval(baristaPackage))
	                return "#DFF0D8";
                else
	                return "#FCF8E3";
	        }
	            
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


	    $scope.getPackages();
	}]);

manageBarista.controller('EditPackageApprovalCtrl', ['$scope', '$rootScope', '$http', '$modalInstance', 'baristaPackage', 'doesBaristaPackageMatchApproval',
	function ($scope, $rootScope, $http, $modalInstance, baristaPackage, doesBaristaPackageMatchApproval) {
	    $scope.baristaPackage = baristaPackage;

	    $scope.doesCurrentBaristaPackageMatchApproval = function() {
	        return doesBaristaPackageMatchApproval($scope.baristaPackage);
	    };

	    $scope.update = function () {
	        toastr.info('Updating Package Approval...');
	        $http({
	            method: "POST",
	            url: $rootScope.baristaBaseUrl + '/_admin/BaristaService/API/UpdatePackageApproval.js',
	            params: {
	                serviceApplicationId: $rootScope.getQueryVariable("appid")
	            },
	            data: $scope.baristaPackage
	        })
	            .success(function (baristaPackage) {
	                toastr.success('Updated Package Approval!');
	                $modalInstance.close(baristaPackage);
	            })
	            .error(function (baristaPackage) {
	                toastr.error("Unable to update baristaPackage Approval  :(<br/>" + $(baristaPackage).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.cancel = function () {
	        $modalInstance.dismiss();
	    };
	}]);
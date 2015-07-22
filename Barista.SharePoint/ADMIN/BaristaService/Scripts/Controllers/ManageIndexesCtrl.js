manageBarista.controller('ManageIndexesCtrl', ['$scope', '$http', '$location', '$modal',
	function ($scope, $http, $location, $modal) {

	    $scope.indexes = null;

	    $scope.addNewIndex = function () {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/addeditindexdialog.html",
                    controller: "AddEditIndexCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        index: function () { return null; },
                        isNew: function () { return true; }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getIndexes();
                });
	    };

	    $scope.editIndex = function (index) {

	        var modalInstance = $modal.open(
                {
                    templateUrl: "Views/addeditindexdialog.html",
                    controller: "AddEditIndexCtrl",
                    modalFade: true,
                    dialogFade: true,
                    resolve: {
                        baristaBaseUrl: function () { return $scope.baristaBaseUrl; },
                        index: function () { return index; },
                        isNew: function () { return false; }
                    }
                });

	        modalInstance.result
                .then(function (result) {
                    $scope.getIndexes();
                });
	    };

	    $scope.removeIndex = function (index) {
	        var msgbox = $modal.open({
	            templateUrl: "confirmRemoveIndex.html"
	        });

	        msgbox.result.then(function(result) {
	            toastr.info('Removing Index...');
	            $http({
	                    method: "POST",
	                    url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/DeleteSearchIndex.js',
	                    params: {
	                        serviceApplicationId: $location.search()["appid"]
	                    },
	                    data: index
	                })
	                .success(function(index) {
	                    toastr.success("Removed Index!");
	                    $scope.getIndexes();
	                })
	                .error(function(index) {
	                    toastr.error("Unable to remove Index :(<br/>" + $(index).children(".intro").text(), null, { timeOut: 0 });
	                });
	        });
	    };

	    $scope.getIndexes = function() {
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
	                url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/GetSearchIndexes.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                },
	            })
	            .success(function(indexes) {

	                toastr.success("Loaded Indexes!");
	                toastr.clear();

	                $scope.indexes = indexes;
	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function(indexes) {
	                toastr.error("Unable to load Indexes :(<br/>" + $(indexes).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.saveIndexesAsDefault = function () {
	        toastr.info('Saving current Indexes as default for new service applications...');
	        $http({
	                method: "GET",
	                url: $scope.baristaBaseUrl + '/_admin/BaristaService/API/SaveIndexSettingsAsDefault.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                }
	            })
	            .success(function() {

	                toastr.success("Saved Index Settings as default!");
	                toastr.clear();

	                if (!$scope.$$phase) {
	                    $scope.$apply();
	                }
	            })
	            .error(function(result) {
	                toastr.error("Unable to save Index Settings as default:(<br/>" + $(result).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.getIndexes();

	    $scope.gridOptions = {
	        data: 'indexes',
	        enableColumnResize: true,
	        columnDefs: [
                { field: 'name', displayName: 'Name' },
                { field: 'description', displayName: 'Description' },
                { field: 'indexType', displayName: 'Index Type' },
                { field: 'indexStoragePath', displayName: 'Index Storage Path' },
                { field: '', displayName: '', cellTemplate: '<div class="btn btn-xs btn-primary" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="editIndex(row.entity)"><i class="icon-edit"></i>Edit</div><div class="btn btn-xs btn-danger" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="removeIndex(row.entity)"><i class="icon-remove"></i>Remove</div>' }
	        ]

	    };
	}]);

manageBarista.controller('AddEditIndexCtrl', ['$scope', '$http', '$location', '$modalInstance', 'baristaBaseUrl', 'index', 'isNew',
	function ($scope, $http, $location, $modalInstance, baristaBaseUrl, index, isNew) {
	    $scope.isNew = isNew;
	    $scope.index = index;

	    if ($scope.index == null)
	        $scope.index = {};

	    $scope.addOrUpdate = function () {
	        if ($scope.isNew)
	            toastr.info('Adding Index...');
	        else
	            toastr.info('Updating Index...');
	        $http({
	                method: "POST",
	                url: baristaBaseUrl + '/_admin/BaristaService/API/AddOrUpdateSearchIndex.js',
	                params: {
	                    serviceApplicationId: $location.search()["appid"]
	                },
	                data: $scope.index
	            })
	            .success(function(index) {
	                if ($scope.isNew)
	                    toastr.success('Added Index!');
	                else
	                    toastr.success('Updated Index!');
	                $modalInstance.close(index);
	            })
	            .error(function(index) {
	                if ($scope.isNew)
	                    toastr.error("Unable to add Index :(<br/>" + $(index).children(".intro").text(), null, { timeOut: 0 });
	                else
	                    toastr.error("Unable to update Index :(<br/>" + $(index).children(".intro").text(), null, { timeOut: 0 });
	            });
	    };

	    $scope.cancel = function () {
	        $modalInstance.dismiss();
	    };
	}]);
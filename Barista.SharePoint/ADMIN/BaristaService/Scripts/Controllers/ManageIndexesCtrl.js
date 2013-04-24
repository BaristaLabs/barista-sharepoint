var indexServiceBaseUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";
var indexServiceBasePostUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";

//Main Controller
function ManageIndexesCtrl($scope, $window, $http, $dialog, $templateCache) {

    $scope.addNewIndex = function () {

        var d = $dialog.dialog(
            {
                modalFade: true,
                dialogFade: true,
                resolve: {
                    index: function () { return null; },
                    isNew: function () { return true; }
                }
            });

        d.open('Views/addeditindexdialog.html', 'AddEditIndexCtrl')
            .then(function (result) {
                if (result === "ok") {
                    $scope.getIndexes();
                }
            });
    };

    $scope.editIndex = function (index) {

        var d = $dialog.dialog(
            {
                modalFade: true,
                dialogFade: true,
                resolve: {
                    index: function () { return index; },
                    isNew: function () { return false; }
                }
            });

        d.open('Views/addeditindexdialog.html', 'AddEditIndexCtrl')
            .then(function (result) {
                if (result === "ok") {
                    $scope.getIndexes();
                }
            });
    };

    $scope.removeIndex = function (index) {
        var msgbox = $dialog.messageBox('Delete Index', 'Are you sure?', [{ label: 'Yes, I\'m sure', result: 'yes' }, { label: 'Nope', result: 'no' }]);
        msgbox.open().then(function (result) {
            if (result === 'yes') {
                toastr.info('Removing Index...');
                $http.post(indexServiceBasePostUrl + '/_admin/BaristaService/WebServices/DeleteSearchIndex.js', index)
                    .success(function (index) {
                        toastr.success("Removed Index!");
                        $scope.getIndexes();
                    })
                    .error(function (index) {
                        toastr.error("Unable to remove Index :(");
                    });
            }
        });
    };

    $scope.getIndexes = function () {
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
        $http.get(indexServiceBaseUrl + '/_admin/BaristaService/WebServices/GetSearchIndexes.js')
            .success(function (indexes) {

                toastr.success("Loaded!");
                toastr.clear();

                $scope.indexes = indexes;
                if (!$scope.$$phase) {
                    $scope.$apply();
                }
            })
            .error(function (indexes) {
                toastr.error("Unable to load Indexes :(");
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
            { field: '', displayName: '', cellTemplate: '<div class="btn btn-mini btn-primary" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="editIndex(row.entity)"><i class="icon-edit"></i>Edit</div><div class="btn btn-mini btn-danger" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="removeIndex(row.entity)"><i class="icon-remove"></i>Remove</div>' }
        ]

    };
}

function AddEditIndexCtrl($scope, $http, dialog, index, isNew) {
    $scope.isNew = isNew;
    $scope.index = index;

    $scope.addOrUpdate = function () {
        if ($scope.isNew)
            toastr.info('Adding Index...');
        else
            toastr.info('Updating Index...');
        $http.post(indexServiceBasePostUrl + '/_admin/BaristaService/WebServices/AddOrUpdateSearchIndex.js', $scope.index)
            .success(function (index) {
                if ($scope.isNew)
                    toastr.success('Added Index!');
                else
                    toastr.success('Updated Index!');
                dialog.close('ok');
            })
            .error(function (index) {
                if ($scope.isNew)
                    toastr.error("Unable to add Index :(");
                else
                    toastr.error("Unable to update Index :(");
            });
    };

    $scope.cancel = function () {
        dialog.close();
    };
}
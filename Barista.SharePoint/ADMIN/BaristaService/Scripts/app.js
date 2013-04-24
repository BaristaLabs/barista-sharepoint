var trustedLocationsServiceBaseUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";
var trustedLocationsServiceBasePostUrl = "/_vti_bin/Barista/v1/Barista.svc/eval?c=";

window.managebarista = angular.module('managebarista', ['ui', 'ui.bootstrap', 'ngGrid']);

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
function ManageMainCtrl($scope, $window, $http, $dialog, $templateCache) {

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

        var d = $dialog.dialog(
            {
                modalFade: true,
                dialogFade: true,
                resolve: {
                    trustedLocation: function() { return null; },
                    isNew: function() { return true; }
                }
            });

        d.open('Views/addedittrustedlocationdialog.html', 'AddEditTrustedLocationCtrl')
            .then(function(result) {
                if (result === "ok") {
                    $scope.getTrustedLocations();
                }
            });
    };

    $scope.editLocation = function (trustedLocation) {

        var d = $dialog.dialog(
            {
                modalFade: true,
                dialogFade: true,
                resolve: {
                    trustedLocation: function () { return trustedLocation; },
                    isNew: function() { return false; }
                }
            });

        d.open('Views/addedittrustedlocationdialog.html', 'AddEditTrustedLocationCtrl')
            .then(function (result) {
                if (result === "ok") {
                    $scope.getTrustedLocations();
                }
            });
    };

    $scope.removeLocation = function (trustedLocation) {
        var msgbox = $dialog.messageBox('Delete Trusted Location', 'Are you sure?', [{ label: 'Yes, I\'m sure', result: 'yes' }, { label: 'Nope', result: 'no' }]);
        msgbox.open().then(function (result) {
            if (result === 'yes') {
                toastr.info('Removing Trusted Location...');
                $http.post(trustedLocationsServiceBasePostUrl + '/_admin/BaristaService/WebServices/DeleteTrustedLocation.js', trustedLocation)
                    .success(function(trustedLocations) {
                        toastr.success("Removed Trusted Location!");
                        $scope.getTrustedLocations();
                    })
                    .error(function(trustedLocations) {
                        toastr.error("Unable to remove Trusted Location :(");
                    });
            }
        });
    };

    $scope.getTrustedLocations = function () {
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
        $http.get(trustedLocationsServiceBaseUrl + '/_admin/BaristaService/WebServices/GetTrustedLocations.js')
            .success(function(trustedLocations) {

                toastr.success("Loaded!");
                toastr.clear();

                $scope.trustedLocations = trustedLocations;
                if (!$scope.$$phase) {
                    $scope.$apply();
                }
            })
            .error(function (trustedLocations) {
                toastr.error("Unable to load Trusted Locations :(");
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
            { field: '', displayName: '', cellTemplate: '<div class="btn btn-mini btn-primary" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="editLocation(row.entity)"><i class="icon-edit"></i>Edit</div><div class="btn btn-mini btn-danger" style="margin-left: 10px; margin-top: 5px; width: 62px;" ng-click="removeLocation(row.entity)"><i class="icon-remove"></i>Remove</div>' }
        ]
    };

    $scope.reload = function ($event, route) {
        $event.preventDefault();
        $window.location.href = route;
    };
}

function AddEditTrustedLocationCtrl($scope, $http, dialog, trustedLocation, isNew) {
    $scope.isNew = isNew;
    $scope.trustedLocation = trustedLocation;

    $scope.addOrUpdate = function () {
        if ($scope.isNew)
            toastr.info('Adding Trusted Location...');
        else
            toastr.info('Updating Trusted Location...');
        $http.post(trustedLocationsServiceBasePostUrl + '/_admin/BaristaService/WebServices/AddOrUpdateTrustedLocation.js', $scope.trustedLocation)
            .success(function (trustedLocations) {
                if ($scope.isNew)
                    toastr.success('Added Trusted Location!');
                else
                    toastr.success('Updated Trusted Location!');
                dialog.close('ok');
            })
            .error(function (trustedLocations) {
                if ($scope.isNew)
                    toastr.error("Unable to add Trusted Location :(");
                else
                    toastr.error("Unable to update Trusted Location :(");
            });
    };

    $scope.cancel = function () {
        dialog.close();
    };
}
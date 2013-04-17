function ManageIndexesCtrl($scope, $http, $templateCache) {
    $scope.addNewIndex = function () {
        var grid = $("#searchIndexesGrid").data("kendoGrid");
        grid.addRow();
    }
}
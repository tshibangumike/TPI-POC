
angular.module("tpiApp")
    .controller("BaseController",
        [
            "$scope", "appService",
            function($scope, appService) {

                $scope.CurrentState = appService.GetCurrentState();
                $scope.ShowToolBar = $scope.CurrentState.name == "AccountLogin" ? false: true;
                
            }
        ]);
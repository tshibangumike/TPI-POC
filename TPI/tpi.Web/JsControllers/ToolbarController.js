/// <reference path="../Libs/lodash/js/lodash.min.js" />
/// <reference path="../Scripts/ShareFunctions.js" />

angular.module("tpiApp")
    .controller("ToolbarController",
        [
            "$rootScope", "$scope", "appService", "sessionService", "currentUser",
            function ($rootScope, $scope, appService, sessionService, currentUser) {

                $scope.CurrentUser = $rootScope.CurrentUser = currentUser;
                $scope.Contact = {};

                if (_.isNull($scope.CurrentUser) ||
                    _.isUndefined($scope.CurrentUser) ||
                    _.isEqual($scope.CurrentUser, "")) {
                    $scope.IsLoggedIn = false;
                } else {
                    $scope.IsLoggedIn = true;
                }

                $scope.Login = function() {

                    tpi.Loading.Start();
                    appService.NavigateTo("AccountLogin");

                };

                $scope.Register = function() {

                    tpi.Loading.Start();
                    appService.NavigateTo("AccountRegister");

                };

                $scope.Account = function() {

                    $rootScope.LoadingText = "Working...";
                    appService.NavigateTo("Profile");

                };

                $scope.Logout = function() {

                    try {

                        $rootScope.LoadingText = "Logging out...";

                        sessionService.User.Logout();

                        var currentStateName = appService.GetCurrentState().name;
                        if (currentStateName === "InspectionList") {
                            appService.RefreshCurrentState();
                        } else
                            appService.NavigateTo("InspectionList");

                    } catch (err) {

                        console.log(err);

                    }

                };

            }
        ]);

angular.module("tpiApp")
    .controller("AccountLoginController",
    [
        "$rootScope", "$scope", "appService", "authService", "sessionService",
        function ($rootScope, $scope, appService, authService, sessionService) {

            $scope.User = {};
            $scope.AuthenticationErrorMessage = null;

            this.init = function () {
                $(".navbar").hide();
            };

            $scope.checkIfEnterKeyWasPressed = function ($event) {
                var keyCode = $event.which || $event.keyCode;
                if (keyCode === 13) {
                    $scope.Authenticate();
                }
            };

            $scope.Authenticate = function () {

                try {

                    $rootScope.LoadingText = "Authenticating...";

                    if (_.isNull($scope.User.Username) || _.isUndefined($scope.User.Username) ||
                        _.isNull($scope.User.Password) || _.isUndefined($scope.User.Password))
                        return;

                    appService.GetData("/PortalUser/GetPortalUser",
                        { username: $scope.User.Username, password: $scope.User.Password })
                        .success(function (response) {

                            if (_.isNull(response) || _.isUndefined(response)) {

                                $scope.User.Password = "";
                                $scope.AuthenticationErrorMessage = "Incorrect username or password combination!";
                                return null;

                            } else if (_.isNull(response.PortalUser) || _.isUndefined(response.PortalUser)) {

                                $scope.User.Password = "";
                                $scope.AuthenticationErrorMessage = "Incorrect username or password combination!";
                                return null;

                            } else if (_.isNull(response.PortalUser.Name) ||
                                _.isUndefined(response.PortalUser.Name)) {

                                $scope.User.Password = "";
                                $scope.AuthenticationErrorMessage = "Incorrect username or password combination!";
                                return null;

                            }

                            $rootScope.LoadingText = "Signing in...";

                            sessionService.User.SetCurrentUser(response);

                            if (_.isNull(sessionStorage.getItem("returnToUrl")) || _.isUndefined(sessionStorage.getItem("returnToUrl")))
                                return appService.NavigateTo("InspectionList");
                            else {
                                var url = sessionStorage.getItem("returnToUrl");
                                sessionStorage.removeItem("returnToUrl");
                                window.location.href = url;
                            }

                        });

                } catch (err) {
                    console.log(err);
                }

            };

            $scope.Register = function () {
                appService.NavigateTo("AccountRegister");
            };

            $scope.ResetPassword = function () {
                appService.NavigateTo("ResetPassword");
            };

            this.init();

        }
    ])
    .controller("AccountRegisterController",
    [
        "$rootScope", "$scope", "appService",
        function ($rootScope, $scope, appService) {

            $scope.Customer = {};
            $scope.PortalUser = { PortalUserRole: "858890000"};
            $scope.Password1 = null;
            $scope.Password2 = null;
            $scope.FormErrorMessages = [];
            $scope.FormHasBeenSubmitted = false;

            /*Functions - START*/

            this.init = function () {
            };

            $scope.OnChange_Password = function () {

                if ($scope.Password1 == null ||
                    $scope.Password1 == undefined ||
                    $scope.Password2 == null ||
                    $scope.Password2 == undefined)
                    return;

                if ($scope.Password1.length > 0 &&
                    $scope.Password2.length > 0 &&
                    $scope.Password1 === $scope.Password2) {
                    $scope.PortalUser.Password = $scope.Password1;
                }
                else {
                    $scope.PortalUser.Password = null;
                }

            };

            $scope.ProcessForm = function () {

                if (_.includes($scope.PortalUser.Password, "%") ||
                    _.includes($scope.PortalUser.Password, "(") ||
                    _.includes($scope.PortalUser.Password, ")") ||
                    _.includes($scope.PortalUser.Password, "=") ||
                    _.includes($scope.PortalUser.Password, "+") ||
                    _.includes($scope.PortalUser.Password, "/") ||
                    _.includes($scope.PortalUser.Password, "*") ||
                    _.includes($scope.PortalUser.Password, "\\") ||
                    _.includes($scope.PortalUser.Password, "{") ||
                    _.includes($scope.PortalUser.Password, "}") ||
                    _.includes($scope.PortalUser.Password, "[") ||
                    _.includes($scope.PortalUser.Password, "]") ||
                    _.includes($scope.PortalUser.Password, ";") ||
                    _.includes($scope.PortalUser.Password, ":")) {


                    var errorMessage = "Password should not contain any special characters, symbols or spaces: %()/\;:+=[]{}";

                    var existingError = _.find($scope.FormErrorMessages,
                        function (x) { return x === errorMessage });

                    if (existingError === undefined)
                        $scope.FormErrorMessages.push(errorMessage);

                    return;
                }

                $rootScope.LoadingText = "Working...";

                //appService.PostForm("/PortalUser/DoesUsernameExist",
                //    {
                //        username: $scope.PortalUser.Username
                //    })
                //    .success(function successCallback(response) {

                //        if (response === false) {

                var url = "/Account/CreateAccount";
                            //if (_.isEqual($scope.Customer["Type"], "1")) {
                            //    url = "/Account/CreateContact";
                            //} else if (_.isEqual($scope.Customer["Type"], "2")) {
                            //    url = "/Account/CreateAccount";
                            //}

                            appService.PostForm(url,
                                {
                                    customer: $scope.Customer,
                                    portalUser: $scope.PortalUser
                                })
                                .success(function successCallback(data) {

                                    appService.NavigateTo("AccountRegisterSuccess");

                                })
                                .error(function errorCallback() {
                                });

                        //} else if (response === true) {

                        //    $scope.Password1 = "";
                        //    $scope.Password2 = "";

                        //    var errorMessage = "Username already exists.";

                        //    var existingError = _.find($scope.FormErrorMessages,
                        //        function (x) { return x === errorMessage });

                        //    if (existingError === undefined)
                        //        $scope.FormErrorMessages.push(errorMessage);

                        //}

                    //})
                    //.error(function errorCallback(response) {
                    //});

            };

            /*Functions - END*/

            this.init();

        }
    ])
    .controller("AccountProfileController",
    [
        "$rootScope", "$scope", "appService", "sessionService", "customer", "orders",
        function ($rootScope, $scope, appService, sessionService, customer, orders) {

            $scope.Customer = customer.data;
            $scope.Title = $scope.Customer.Name;
            $scope.Content = { Profile: true, Orders: true, Credentials: false }
            $scope.CurrentUser = sessionService.User.GetCurrentUser();
            $scope.PortalUser = $scope.CurrentUser.PortalUser;
            $scope.Orders = orders.data;
            $scope.FormErrorMessages = [];

            this.init = function () {
            };

            $scope.HideShow = function () {

                if (arguments.length === 0) return null;
                var record = arguments[0] == null ? null : arguments[0];

                $scope.Content[record] = !$scope.Content[record];

            };

            $scope.base64ToArrayBuffer = function (base64) {
                var binaryString = window.atob(base64);
                var binaryLen = binaryString.length;
                var bytes = new Uint8Array(binaryLen);
                for (var i = 0; i < binaryLen; i++) {
                    var ascii = binaryString.charCodeAt(i);
                    bytes[i] = ascii;
                }
                return bytes;
            };

            $scope.saveByteArray = function () {

                var a = document.createElement("a");
                document.body.appendChild(a);
                a.style = "display: none";
                return function (data, name) {
                    var blob = new Blob(data, { type: "octet/stream" }),
                        url = window.URL.createObjectURL(blob);
                    a.href = url;
                    a.download = name;
                    a.click();
                    window.URL.revokeObjectURL(url);
                };

            };

            $scope.DownloadReport = function (jobNumber, id, url) {

                var filename = url.substring(url.lastIndexOf('/') + 1);

                appService.GetData("/Account/DownloadReport",
                    { jobNumber: jobNumber, id: id, documentName: filename })
                    .success(function (response) {

                        var dlnk = document.getElementById('dwnldLnk');
                        dlnk.href = 'data:application/pdf;base64,' + response;
                        dlnk.click();

                    });

            };

            $scope.GetFilename = function(url)
            {
                if (url) {
                    var m = url.toString().match(/.*\/(.+?)\./);
                    if (m && m.length > 1) {
                        return m[1];
                    }
                }
                return "";
            }

            $scope.OnChange_Password = function () {

                if ($scope.Password1 == null ||
                    $scope.Password1 == undefined ||
                    $scope.Password2 == null ||
                    $scope.Password2 == undefined)
                    return;

                if ($scope.Password1.length > 0 &&
                    $scope.Password2.length > 0 &&
                    $scope.Password1 === $scope.Password2) {
                    $scope.PortalUser.Password = $scope.Password1;
                } else {
                    $scope.PortalUser.Password = null;
                }

            };

            $scope.ProcessFormCustomer = function () {

                appService.GetData("/Account/UpdateCustomer",
                    { customer: $scope.Customer })
                    .success(function (response) {

                        if (response == true) {
                            toastr.success("Profile has been successfully saved");
                        } else {
                            toastr.error("An error occured. Please try again later");
                        }

                    });

            };

            $scope.ProcessFormPortalUser = function () {

                if (_.includes($scope.PortalUser.Password, "%") ||
                    _.includes($scope.PortalUser.Password, "(") ||
                    _.includes($scope.PortalUser.Password, ")") ||
                    _.includes($scope.PortalUser.Password, "=") ||
                    _.includes($scope.PortalUser.Password, "+") ||
                    _.includes($scope.PortalUser.Password, "/") ||
                    _.includes($scope.PortalUser.Password, "*") ||
                    _.includes($scope.PortalUser.Password, "\\") ||
                    _.includes($scope.PortalUser.Password, "{") ||
                    _.includes($scope.PortalUser.Password, "}") ||
                    _.includes($scope.PortalUser.Password, "[") ||
                    _.includes($scope.PortalUser.Password, "]") ||
                    _.includes($scope.PortalUser.Password, ";") ||
                    _.includes($scope.PortalUser.Password, ":")) {


                    var errorMessage = "Password should not contain any special characters, symbols or spaces: %()/\;:+=[]{}";

                    var existingError = _.find($scope.FormErrorMessages,
                        function (x) { return x === errorMessage });

                    if (existingError === undefined)
                        $scope.FormErrorMessages.push(errorMessage);

                    return;
                }

                appService.GetData("/PortalUser/UpdatePortalUser",
                    { portalUser: $scope.PortalUser })
                    .success(function (response) {

                        if (response == true) {
                            toastr.success("Credentials has been successfully saved");
                        } else {
                            $scope.Password1 = "";
                            $scope.Password2 = "";
                            toastr.error("An error occured. Please try again later");
                        }

                        appService.RefreshCurrentState();

                    });

            };

            this.init();

        }
    ])
    .controller("ResetPasswordController",
    [
        "$rootScope", "$scope", "appService",
        function ($rootScope, $scope, appService) {

            $scope.FormErrorMessages = [];
            $scope.PortalUser = {};

            this.init = function () {
            };

            $scope.ProcessFormPortalUser = function () {

                if (_.includes($scope.PortalUser.Password, "%") ||
                    _.includes($scope.PortalUser.Password, "(") ||
                    _.includes($scope.PortalUser.Password, ")") ||
                    _.includes($scope.PortalUser.Password, "=") ||
                    _.includes($scope.PortalUser.Password, "+") ||
                    _.includes($scope.PortalUser.Password, "/") ||
                    _.includes($scope.PortalUser.Password, "*") ||
                    _.includes($scope.PortalUser.Password, "\\") ||
                    _.includes($scope.PortalUser.Password, "{") ||
                    _.includes($scope.PortalUser.Password, "}") ||
                    _.includes($scope.PortalUser.Password, "[") ||
                    _.includes($scope.PortalUser.Password, "]") ||
                    _.includes($scope.PortalUser.Password, ";") ||
                    _.includes($scope.PortalUser.Password, ":")) {


                    var errorMessage = "Password should not contain any special characters, symbols or spaces: %()/\;:+=[]{}";

                    var existingError = _.find($scope.FormErrorMessages,
                        function (x) { return x === errorMessage });

                    if (existingError === undefined)
                        $scope.FormErrorMessages.push(errorMessage);

                    return;
                }

                appService.GetData("/PortalUser/ValidatePortalUser",
                    { username: $scope.PortalUser.Username, password: $scope.PortalUser.OldPassword })
                    .success(function (response) {

                        if (!_.isNull(response) && !_.isUndefined(response) && !_.isEqual(response, "")) {

                            $scope.PortalUser.Id = response.Id;

                            appService.GetData("/PortalUser/UpdatePortalUser",
                                { portalUser: $scope.PortalUser })
                                .success(function (response) {

                                    if (response == true) {
                                        toastr.success("Credentials has been successfully changed");
                                    } else {
                                        toastr.error("An error occured. Please try again later");
                                    }

                                    appService.RefreshCurrentState();

                                });

                        }
                        else {

                            $scope.PortalUser.OldPassword = "";
                            $scope.PortalUser.Password = "";

                            var errorMessage = "Invalid Username and Old Password combination.";

                            var existingError = _.find($scope.FormErrorMessages,
                                function (x) { return x === errorMessage });

                            if (existingError === undefined)
                                $scope.FormErrorMessages.push(errorMessage);

                        }

                    });

            };

            this.init();

        }
    ]);
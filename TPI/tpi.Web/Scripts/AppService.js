

angular.module("tpiApp")
    .service("appService",
    [
        "$http", "$state", "$q",
        function ($http, $state) {
            return {
                GetData: function (url, paramObject) {
                    return $http({
                        method: "POST",
                        url: url,
                        data: paramObject
                    });
                },
                PostForm: function (url, formData) {
                    return $http({
                        method: "POST",
                        url: url,
                        data: $.param(formData),
                        headers: { 'Content-Type': "application/x-www-form-urlencoded" }
                    });
                },
                NavigateTo: function () {
                    if (arguments.length === 0) return null;
                    var entityView = arguments[0] == null ? null : arguments[0];
                    var parameters = arguments[1] == null ? null : arguments[1];
                    if (parameters != null) {
                        $state.go(entityView, parameters);
                    } else {
                        $state.go(entityView);
                    }
                    return null;
                },
                UploadFile: function (url, formData, currentFile, paramObject, inputIdName) {
                    if (paramObject == null) return $http.get(url);
                    var parameterUrl = "";
                    var count = 0;
                    angular.forEach(paramObject,
                        function (value, key) {
                            if (count > 0)
                                parameterUrl += "&";
                            parameterUrl += key + "=" + value;
                            count++;
                        });
                    for (var i = 0; i < currentFile.length; i++) {
                        formData.append(inputIdName, currentFile[i]);
                    }
                    return $http.post(url + "?" + parameterUrl,
                        formData,
                        {
                            headers: { 'Content-Type': undefined },
                            transformRequest: angular.identity
                        });
                },
                RefreshCurrentState: function () {
                    $state.go($state.current, {}, { reload: true });
                },
                GetCurrentState: function() {
                    return $state.current;
                }
            };
        }
    ])
    .service("authService",
    [
        "$http", 
        function ($http) {
            return {
                IsLoggedIn: function isLoggedIn(sessionService) {
                    return sessionService.GetCurrentUser() !== null;
                },
                LogIn: function (username, password) {
                    return $http
                        .post("/PortalUser/GetPortalUser", { username: username, password: password });
                },
                LogOut: function (sessionService) {
                    return $http
                        .get("/api/logout")
                        .then(function (response) {
                            sessionService.Destroy();
                        });

                }

            }
        }
    ])
    .service("sessionService",
    [
        "$http",
        function ($http) {
            return {
                User: {
                    GetCurrentUser: function () {
                        return JSON.parse(sessionStorage.getItem("session.user"));
                    },
                    GetCurrentUserRaw: function () {
                        return sessionStorage.getItem("session.user");
                    },
                    SetCurrentUser: function (user) {
                        this._user = user;
                        sessionStorage.setItem("session.user", JSON.stringify(user));
                        return this;
                    },
                    Logout: function (user) {
                        sessionStorage.clear();
                        localStorage.clear();
                    }
                },
                Destroy: function () {
                    sessionStorage.clear();
                }
            }
        }
    ]);


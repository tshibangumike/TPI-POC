if (typeof (tpi) == "undefined") {
    tpi = {};
};

tpi.Functions = {
    GetBaseRouteView: function() {
        return {
            templateUrl: "/Templates/Base/Base.html",
            controller: "BaseController"
        };
    },
    GetToolbarRouteView: function () {
        return {
            templateUrl: "/Templates/Toolbar/Toolbar.html",
            controller: "ToolbarController",
            resolve: {
                currentUser:
                    [
                        "sessionService", function (sessionService) {
                            return sessionService.User.GetCurrentUser();
                        }
                    ]
            }
        };
    },
    HasSessionExpired: function() {
        if (arguments.length === 0) return null;
        var appService = arguments[0] == null ? null : arguments[0];
        var currentUser = arguments[1] == null ? null : arguments[1];
        var redirectUrl = arguments[2] == null ? null : arguments[2];

        if (_.isEqual(currentUser, "")) {
            tpi.Loading.Start();
            sessionStorage.setItem("redirectAfterLogin", redirectUrl);
            appService.NavigateTo("AccountLogin");
        }

        return null;

    },
    GetDateFromJsonDate: function () {
        if (arguments.length === 0) return null;
        var jsonDate = arguments[0] == null ? null : arguments[0];
        var type = arguments[1] == null ? null : arguments[1];
        if (jsonDate == null) return null;
        var pattern = /Date\(([^)]+)\)/;
        var results = pattern.exec(jsonDate);
        var dt = new Date(parseFloat(results[1]));
        var date = null;
        if (type === "datetime") {
            date = (tpi.Functions.AddTrailingZero(dt.getMonth() + 1)) + "/" + tpi.Functions.AddTrailingZero(dt.getDate()) + "/" + dt.getFullYear() + " " + tpi.Functions.AddTrailingZero(dt.getHours()) + ":" + tpi.Functions.AddTrailingZero(dt.getMinutes());
        } else {
            date = (tpi.Functions.AddTrailingZero(dt.getMonth() + 1)) + "/" + tpi.Functions.AddTrailingZero(dt.getDate()) + "/" + dt.getFullYear();
        }
        return date;
    },
    AddTrailingZero: function () {
        if (arguments.length === 0) return null;
        var number = arguments[0] == null ? null : arguments[0];
        if (number < 10) {
            return "0" + number;
        } else {
            return number;
        }
    },
};

tpi.Loading = {
    Start: function (message) {
        if (message == null)
            return $("body").loading({
                message: "Working..."
            });
        else
            return $("body").loading({
                message: message
            });
    },
    Stop: function () {
        return $("body").loading("stop");
    }
};
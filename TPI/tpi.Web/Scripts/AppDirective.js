angular.module("tpiApp")
    .factory("myInterceptor", [
        "$rootScope", "$q", function ($rootScope, $q) {
            var pendingRequests = 0;
            return {
                request: function (config) {
                    pendingRequests++;
                    tpi.Loading.Start($rootScope.LoadingText);
                    return config || $q.when(config);
                },
                response: function (response) {
                    if ((--pendingRequests) === 0) {
                        tpi.Loading.Stop();
                    }
                    return response || $q.when(response);
                },
                responseError: function (response) {
                    if ((--pendingRequests) === 0) {
                        tpi.Loading.Stop();
                    }
                    return $q.reject(response);
                }
            };
        }
    ])
    .directive('ngSetFocus', ['$timeout', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                var delay = 300;

                // set focus on broadcast
                scope.$on(attrs.ngSetFocus, function (e) {
                    $timeout(function () {
                        element[0].focus();
                    }, delay);

                });

                // apply default focus after other events have complete
                $timeout(function () {
                    if (attrs.hasOwnProperty('setFocusDefault')) {
                        element[0].focus();
                    }
                }, delay);

                // fix for default focus on iOS. Does not show keyboard
                element.on('touchstart', function (event) {
                    element[0].blur();
                });

            }
        };
    }]);


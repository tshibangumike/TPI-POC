/// <reference path="../Libs/lodash/js/lodash.min.js" />

angular.module("tpiApp")
    .controller("ReportListController",
        [
            "$scope", "$mdToast", "appService",
            function($scope, $mdToast, appService) {

                //if (_.isNull(parentReports) || _.isNull(parentReports.data)) return;
                //_.forEach(parentReports.data,
                //    function(entityValue) {
                //        var id = null;
                //        var name = null;
                //        var attributes = entityValue.Attributes;
                //        _.forEach(attributes,
                //            function(attributeValue) {
                //                if (_.isEqual(attributeValue.Key, "productid")) {
                //                    id = attributeValue.Value;
                //                } else if (_.isEqual(attributeValue.Key, "name")) {
                //                    name = attributeValue.Value;
                //                }
                //            });
                //        $scope.ParentReports.push({ Id: id, Name: name });
                //    });

                //$scope.LoadChildReport = function() {
                //    if (arguments.length === 0) return null;
                //    var parentReportId = arguments[0] == null ? null : arguments[0];
                //    if ($scope.ChildReports.length > 0) return $scope.ChildReports;
                //    return appService.GetData("/Report/GetChildReports",
                //            { parentReportId: parentReportId })
                //        .success(function(response) {
                //            $scope.ChildReports = [];
                //            _.forEach(response,
                //                function(entityValue) {
                //                    var id = null;
                //                    var name = null;
                //                    var attributes = entityValue.Attributes;
                //                    _.forEach(attributes,
                //                        function(attributeValue) {
                //                            if (_.isEqual(attributeValue.Key, "productid")) {
                //                                id = attributeValue.Value;
                //                            } else if (_.isEqual(attributeValue.Key, "name")) {
                //                                name = attributeValue.Value;
                //                            }
                //                        });
                //                    $scope.ChildReports.push({ Id: id, Name: name });
                //                });
                //        });
                //};

                //$scope.LoadPriceList = function() {
                //    if (arguments.length === 0) return null;
                //    var childReportId = arguments[0] == null ? null : arguments[0];
                //    if ($scope.PriceListItems.length > 0) return $scope.PriceListItems;
                //    return appService.GetData("/Report/GetPriceLists",
                //            { childReportId: childReportId })
                //        .success(function(response) {
                //            _.forEach(response,
                //                function(entityValue) {
                //                    var id = null;
                //                    var name = null;
                //                    var amount = null;
                //                    var attributes = entityValue.Attributes;
                //                    _.forEach(attributes,
                //                        function(attributeValue) {
                //                            if (_.isEqual(attributeValue.Key, "productpricelevelid")) {
                //                                id = attributeValue.Value;
                //                            }
                //                            if (_.isEqual(attributeValue.Key, "pricelevelid")) {
                //                                name = attributeValue.Value.Name;
                //                            } else if (_.isEqual(attributeValue.Key, "amount")) {
                //                                amount = attributeValue.Value.Value;
                //                            }
                //                        });
                //                    $scope.PriceListItems.push({ Id: id, Name: name, Amount: amount });
                //                });
                //        });
                //};

                //$scope.BuyReport = function() {
                //    appService.PostForm("/Report/BuyReport", { itemDescription: "Retail", totalAmount: $scope.SelectedPriceListItemAmount })
                //        .success(function successCallback(response) {

                //            if (_.startsWith(response, "success")) {
                                
                //                var sharedPaymentUrl = _.replace(response, "success:", "");
                //                window.location.href = sharedPaymentUrl;
                //            }

                //        }, function error(response) {

                //            $mdToast.show(
                //                $mdToast.simple()
                //                    .textContent(response)
                //                .position("top right")
                //                .hideDelay(10000)
                //            );

                //        });
                //}

            }
        ]);

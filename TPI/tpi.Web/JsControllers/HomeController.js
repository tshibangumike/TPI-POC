
angular.module("tpiApp")
    .controller("HomeController",
        [
            "$scope", "appService",
            function($scope, appService) {

                $scope.InspectionDetails = [];

                $scope.SearchForAddress = function() {
                    if (arguments.length === 0) return null;
                    var searchText = arguments[0] == null ? null : arguments[0];
                    appService.GetData("/Inspection/GetInspectionsByAddress", { searchText: searchText })
                        .then(function(response) {
                            $scope.Inspections = [];
                            _.forEach(response.data,
                                function(entityValue, key) {
                                    var id = null;
                                    var inspectionAddress = null;
                                    var attributes = entityValue.Attributes;
                                    _.forEach(attributes,
                                        function(attributeValue) {
                                            if (_.isEqual(attributeValue.Key, "blu_inspectionid")) {
                                                id = attributeValue.Value;
                                            } else if (_.isEqual(attributeValue.Key, "blu_inspectionaddress")) {
                                                inspectionAddress = attributeValue.Value;
                                            }
                                        });
                                    $scope.Inspections.push({ Id: id, InspectionAddress: inspectionAddress });
                                    
                                });
                        });
                    return null;
                };

                $scope.Navigate = function() {

                    appService.NavigateTo("ReportList", {})

                };

                $scope.GetInspectionDetail = function() {
                    if (arguments.length === 0) return null;
                    var inspectionId = arguments[0] == null ? null : arguments[0];
                    appService.GetData("/Inspection/GetInspectionDetailsByInspection", { inspectionId: inspectionId })
                        .then(function(response) {
                            $scope.InspectionDetails = [];
                            _.forEach(response.data,
                                function(entityValue) {
                                    var id = null;
                                    var product = {};
                                    var attributes = entityValue.Attributes;
                                    _.forEach(attributes,
                                        function(attributeValue) {
                                            if (_.isEqual(attributeValue.Key, "blu_inspectiondetailid")) {
                                                id = attributeValue.Value;
                                            } else if (_.isEqual(attributeValue.Key, "blu_productid")) {
                                                product.Id = attributeValue.Value.Id;
                                                product.Name = attributeValue.Value.Name;
                                            }
                                        });
                                    $scope.InspectionDetails.push({ Id: id, Product: product });
                                });
                        });
                    return null;
                };

                $scope.place = {};

                

            }
        ]);
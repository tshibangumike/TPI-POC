/// <reference path="../Libs/lodash/js/lodash.min.js" />
/// <reference path="~/Scripts/AppDirective.js" />
/// <reference path="~/Scripts/ShareFunctions.js" />

angular.module("tpiApp")
    .controller("BookingCreateController",
        [
            "$rootScope", "$scope", "$location", "appService", "sessionService", "systemUsers",
            function ($rootScope, $scope, $location, appService, sessionService, systemUsers) {

                $scope.Appointments = [];
                $scope.InspectionDetails = [];
                $scope.Appointment = {};
                $scope.SystemUsers = systemUsers.data;
                $scope.FormHasBeenSubmitted = false;
                $scope.FormErrors = [];
                $scope.CurrentUser = sessionService.User.GetCurrentUser();
                $scope.Inspection = {};

                this.init = function() {

                    if (_.isEqual($scope.CurrentUser.Inspection.Id, "00000000-0000-0000-0000-000000000000")) {
                        $scope.UserCanCreateInspection = true;
                    } else {
                        $scope.UserCanCreateInspection = false;
                    }

                    _.forEach($scope.CurrentUser.Products, function (value) {
                        $scope.InspectionDetails.push({ ProductId: value.Id, Product: value, ProductCategory: value.ProductCategory });
                    });

                };

                $scope.Times = [
                    {
                        Id: 1,
                        StartTime: "08:00 AM",
                        EndTime: "10:00 AM",
                        StartValue: "08:00:00",
                        EndValue: "10:00:00",
                        StartHour: "08",
                        EndHour: "10"
                    },
                    {
                        Id: 2,
                        StartTime: "10:00 AM",
                        EndTime: "12:00 PM",
                        StartValue: "10:00:00",
                        EndValue: "12:00:00",
                        StartHour: "10",
                        EndHour: "12"
                    },
                    {
                        Id: 3,
                        StartTime: "12:00 PM",
                        EndTime: "14:00 PM",
                        StartValue: "12:00:00",
                        EndValue: "14:00:00",
                        StartHour: "12",
                        EndHour: "14"
                    },
                    {
                        Id: 4,
                        StartTime: "14:00 PM",
                        EndTime: "16:00 PM",
                        StartValue: "14:00:00",
                        EndValue: "16:00:00",
                        StartHour: "14",
                        EndHour: "16"
                    },
                    {
                        Id: 5,
                        StartTime: "16:00 PM",
                        EndTime: "18:00 PM",
                        StartValue: "16:00:00",
                        EndValue: "18:00:00",
                        StartHour: "16",
                        EndHour: "18"
                    }
                ];

                $scope.CreateInspectionDetails = function() {

                    if (!_.isEqual($scope.InspectionDetails.length, $scope.InspectionDetailObjects.length)) return null;

                    $rootScope.LoadingText = "Creating inspection detail(s)...";

                    appService.GetData("/Inspection/CreateInspectionDetails",
                            { inspectionDetails: $scope.InspectionDetailObjects })
                        .success(function(response) {

                            var createInspectionId = response;
                            tpi.Loading.Stop();

                            appService.NavigateTo("BookingSuccessfull");

                        });

                    return null;

                };

                $scope.CreateInspection = function() {

                    if (_.isEqual($scope.UserCanCreateInspection, true)) {

                        if (!_.isEqual($scope.InspectionDetails.length, $scope.InspectionDetailObjects.length)) {
                            return null;
                        }

                        if (_.isEqual($scope.CurrentUser.PortalUser.PortalUserRole, 858890000)) {
                            $scope.Inspection.ContactId = $scope.CurrentUser.PortalUser.CustomerId;
                        } else if (_.isEqual($scope.CurrentUser.PortalUser.PortalUserRole, 858890001)) {
                            $scope.Inspection.AccountId = $scope.CurrentUser.PortalUser.CustomerId;
                        }

                        $rootScope.LoadingText = "Creating inspection...";

                        appService.GetData("/Inspection/CreateInspection",
                                { inspection: $scope.Inspection })
                            .success(function(response) {

                                var createInspectionId = response;

                                _.forEach($scope.InspectionDetailObjects,
                                    function(value) {
                                        value.InspectionId = createInspectionId;
                                    });

                                $scope.CreateInspectionDetails();

                            });
                    } else if (_.isEqual($scope.UserCanCreateInspection, false)) {

                        _.forEach($scope.InspectionDetailObjects,
                            function(value) {
                                value.InspectionId = $scope.CurrentUser.Inspection.Id;
                            });

                        $scope.CreateInspectionDetails();

                    }
                    return null;

                };

                $scope.GetInspectorBySkills = function() {
                    if (arguments.length === 0) return null;
                    var skills = arguments[0] == null ? null : arguments[0];
                    var inspectors = [];
                    //return _.filter($scope.SystemUsers, function (x) { return _.includes(x.InspectorSkills, skills); });

                    _.forEach($scope.SystemUsers, function (value) {
                        if (_.intersection(value.InspectorSkills, skills).length > 0) {
                            inspectors.push(value);
                        }
                    });
                    console.log(inspectors);
                    return inspectors;

                };

                $scope.ProcessForm = function() {

                    try {

                        $scope.Appointments = [];
                        $scope.InspectionDetailObjects = [];

                        if (_.isEqual($scope.InspectionDetails.length, 0)) return null;

                        /*Create Appointment - START*/
                        _.forEach($scope.InspectionDetails,
                            function(value) {

                                var time = _.find($scope.Times,
                                    function(x) { return x.StartValue === value.Appointment.SelectedTimeSlot; });

                                var dateParts = _.split(value.Appointment.SelectedDate, "/", 3);

                                var startDate =
                                    (new Date(dateParts[2] + "/" + dateParts[1] + "/" + dateParts[0])).setHours(
                                        time.StartHour);

                                var endDate =
                                    (new Date(dateParts[2] + "/" + dateParts[1] + "/" + dateParts[0])).setHours(
                                        time.EndHour);

                                var appointmentObject = {
                                    Subject: "Appointment for " + value.Product.Name,
                                    StartTime: new Date(startDate),
                                    EndTime: new Date(endDate),
                                    OwnerId: value.Appointment.OwnerId,
                                    RegardingObjectId: $scope.CurrentUser.OrderId,
                                    ProductId: value.ProductId
                                };

                                $rootScope.LoadingText = "Creating appointment...";

                                appService.GetData("/Appointment/CreateAppointment",
                                        { appointment: appointmentObject })
                                    .success(function(response) {

                                        var newAppointmentId = response;

                                        var inspectionDetailObject = {
                                            Name: "Inspection for " + value.Product.Name,
                                            ProductId: appointmentObject.ProductId,
                                            OrderId: $scope.CurrentUser.OrderId,
                                            AppointmentId: newAppointmentId,
                                            OwnerId: value.Appointment.OwnerId,
                                            ProductCategory: value.ProductCategory 
                                        };

                                        $scope.InspectionDetailObjects.push(inspectionDetailObject);

                                        $scope.CreateInspection();

                                    });

                            });
                        /*Create Appointment - END*/

                        return null;

                    } catch (err) {

                        console.log(err);

                    }

                    return null;

                };

                this.init();

            }
        ])
    .controller("BookingSuccessController",
        [
            "$rootScope", "$scope", "appService", "booking",
            function($rootScope, $scope, appService, booking) {

                $scope.Booking = booking.data;

                this.init = function() {

                };

                $scope.AddTrailingZero = function() {
                    if (arguments.length === 0) return null;
                    var number = arguments[0] == null ? null : arguments[0];
                    if (number < 10) {
                        return "0" + number;
                    } else {
                        return number;
                    }
                };

                this.init();

            }
        ]);




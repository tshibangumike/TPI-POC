angular.module("tpiApp")
    .controller("AppointmentCreateController",
    [
        "$rootScope", "$scope", "appService", "sessionService", "product", "inspectors", "TsAndCs",
        function ($rootScope, $scope, appService, sessionService, product, inspectors, TsAndCs) {

            $scope.SelectedInspector = {Id: "all"}
            $scope.Product = product;
            $scope.Inspectors = inspectors.data;
            $scope.tsAndCs = TsAndCs.data;
            $scope.EventSources = [];
            $scope.Dropped = false;
            $scope.ErrorMessage;
            $scope.Content = { Scheduler: true };

            this.Init = function () {

                if (_.isNull($scope.CurrentUser.Appointments) || _.isUndefined($scope.CurrentUser.Appointments)) {
                    $scope.CurrentUser.Appointments = [];
                }

                var hours = Math.floor(($scope.Product.AppointmentDuration) / 60);
                var minutes = ($scope.Product.AppointmentDuration) % 60;
                var seconds = "00";
                $scope.EventDuration = tpi.Functions.AddTrailingZero(hours) + ":" + tpi.Functions.AddTrailingZero(minutes) + ":00";

                $scope.AllowEventOverLap = _.isEqual($scope.Product.ReportPriority, 858890001) ? true : false;

                $scope.InitialiseCalendar();

                $scope.OnChangeInspector();

            }

            $scope.Back = function () {

                appService.NavigateTo("InspectionPayment");
            };

            $scope.HideShow = function () {

                if (arguments.length === 0) return null;
                var record = arguments[0] == null ? null : arguments[0];

                $scope.Content[record] = !$scope.Content[record];

            };

            $scope.InitialiseCalendar = function () {

                $("#external-events .fc-event").each(function () {
                    // store data so the calendar knows to render an event upon drop
                    $(this).data("event", {
                        title: $.trim($(this).text()), // use the element's text as the event title
                        stick: true, // maintain when user navigates (see docs on the renderEvent method)
                        color: $(this).data('color')
                    });
                    $(this).draggable({
                        zIndex: 999,
                        revert: true,      // will cause the event to go back to its
                        revertDuration: 0  //  original position after the drag
                    });
                });

                $("#calendar").fullCalendar({
                    header: {
                        left: "prev,next today",
                        center: "title",
                        right: 'month,agendaWeek,agendaDay'
                    },
                    allDaySlot: false,
                    minTime: "08:00:00",
                    maxTime: "18:00:00",
                    height: ($(window).height()*0.5),
                    lang: 'en',
                    views: {
                        agenda: {
                            minTime: '06:00',
                            maxTime: '18:00'
                        },
                        week: {
                            titleFormat: 'D MM YYYY'
                        },
                    },
                    defaultDate: (new Date()),
                    defaultView: "agendaWeek",
                    eventSources: [$scope.EventSources],
                    droppable: true,
                    navLinks: true,
                    editable: true,
                    eventOverlap: $scope.AllowEventOverLap,
                    eventDurationEditable: false,
                    eventLimit: true,
                    defaultTimedEventDuration: $scope.EventDuration,
                    forceEventDuration: true,
                    drop: function (date, jsEvent, ui, resourceId) {
                    },
                    eventReceive: function (event) {
                        if ($scope.SelectedInspector == null || $scope.SelectedInspector.Id == null) {
                            $("#calendar").fullCalendar('removeEvents', [event._id]);
                            toastr.error("Please select an inspector first!");
                            return;
                        }
                        if (!$scope.Dropped) {
                            $scope.EventId = event._id;
                            $scope.Appointment = {
                                Subject: "Appointment for " + $scope.Product.ProductName,
                                StartTime: new Date(event.start.format()),
                                StartTimeText: new Date(event.start.format()).toISOString(),
                                EndTime: new Date(event.end.format()),
                                EndTimeText: new Date(event.end.format()).toISOString(),
                                OwnerId: $scope.SelectedInspector.Id,
                                RegardingObjectId: $scope.CurrentUser.OrderId,
                                ProductId: $scope.Product.ProductId,
                                ProductCategory: $scope.Product.ProductCategory,
                                PriorityCode: $scope.AllowEventOverLap == true ? 2 : 1
                            };
                            $scope.Dropped = true;
                            event.title = $scope.Product.ProductName;
                            $('#calendar').fullCalendar('updateEvent', event);
                        }
                        else {
                            $("#calendar").fullCalendar('removeEvents', [event._id]);
                            
                            toastr.error("You can only schedule ONE appointment!");
                        }
                    },
                    eventDrop: function (event) {
                        $scope.Appointment = {
                            Subject: "Appointment for " + $scope.Product.ProductName,
                            StartTime: new Date(event.start.format()),
                            StartTimeText: event.start.format(),
                            EndTime: new Date(event.end.format()),
                            EndTimeText: event.end.formart(),
                            OwnerId: $scope.SelectedInspector.Id,
                            RegardingObjectId: $scope.CurrentUser.OrderId,
                            ProductId: $scope.Product.ProductId,
                            ProductCategory: $scope.Product.ProductCategory,
                            PriorityCode: $scope.AllowEventOverLap == true ? 2 : 1
                        };
                    },
                    eventConstraint: {
                        start: moment().format('YYYY-MM-DD'),
                        end: '2100-01-01' // hard coded goodness unfortunately
                    },
                    eventRender: function (event, element, view) {
                        var ntoday = new Date().getTime();
                        var eventEnd = moment(event.end).valueOf();
                        var eventStart = moment(event.start).valueOf();
                        if (!event.end) {
                            if (eventStart < ntoday) {
                                element.addClass("past-event");
                                element.children().addClass("past-event");
                            }
                        } else {
                            if (eventEnd < ntoday) {
                                element.addClass("past-event");
                                element.children().addClass("past-event");
                            }
                        }
                        if (event.allDay === true) {
                            element.addClass("allday-event");
                            element.children().addClass("allday-event");
                        }
                    }
                });

            };

            $scope.OnChangeInspector = function () {

                if ($scope.SelectedInspector == null || $scope.SelectedInspector.Id == null) return [];

                $rootScope.LoadingText = "Getting inspector's appointments...";

                if ($scope.SelectedInspector.Id == "all") {
                    appService.GetData("/Appointment/GetInspectorsAppointments",
                        { numberOfInspectors: $scope.Inspectors.length })
                        .success(function (response) {

                            $rootScope.LoadingText = "Working...";

                            $('#calendar').fullCalendar('removeEventSource', $scope.EventSources);
                            $('#calendar').fullCalendar('refetchEvents');

                            $scope.newEventSources = [];

                            _.forEach(response, function (value) {
                                $scope.newEventSources.push({
                                    id: value.Id,
                                    title: "Booked",
                                    start: value.StartTime,
                                    end: value.EndTime,
                                    color: "#DC3545",
                                    allDay: false,
                                    editable: false,
                                    ownerid: value.OwnerId
                                });
                            });

                            $('#calendar').fullCalendar('addEventSource', $scope.newEventSources);
                            $('#calendar').fullCalendar('refetchEvents');

                            $scope.EventSources = $scope.newEventSources;

                        });
                }
                else {
                    appService.GetData("/Appointment/GetAppointmentsByInspector",
                        { inspectorId: $scope.SelectedInspector.Id })
                        .success(function (response) {

                            $rootScope.LoadingText = "Working...";

                            $('#calendar').fullCalendar('removeEventSource', $scope.EventSources);
                            $('#calendar').fullCalendar('refetchEvents');

                            $scope.newEventSources = [];

                            _.forEach(response, function (value) {
                                $scope.newEventSources.push({
                                    id: value.Id,
                                    title: "Booked",
                                    start: value.StartTime,
                                    end: value.EndTime,
                                    color: "#DC3545",
                                    allDay: false,
                                    editable: false,
                                    ownerid: value.OwnerId
                                });
                            });

                            $('#calendar').fullCalendar('addEventSource', $scope.newEventSources);
                            $('#calendar').fullCalendar('refetchEvents');

                            $scope.EventSources = $scope.newEventSources;

                        });
                }

            };

            $scope.RefreshEvent = function () {

                $("#calendar").fullCalendar('removeEvents', [$scope.EventId]);
                $scope.Dropped = false;

            };

            $scope.ProcessForm = function () {

                var appts = _.remove($scope.CurrentUser.Appointments, function (x) {
                    return x == null || x.ProductId == $scope.Appointment.ProductId;
                });

                $scope.CurrentUser.Appointments.push($scope.Appointment);
                sessionService.User.SetCurrentUser($scope.CurrentUser);

                $scope.CurrentUser.TsAndCs = [];
                $scope.CurrentUser.TsAndCs.push($scope.tsAndCs);
                sessionService.User.SetCurrentUser($scope.CurrentUser);

                appService.NavigateTo("InspectionPayment");

            };

            this.Init();

        }
    ]);
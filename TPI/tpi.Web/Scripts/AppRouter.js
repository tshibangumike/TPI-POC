
angular.module("tpiApp")
    .config([
        "$stateProvider", "$urlRouterProvider", "$httpProvider", 
        function ($stateProvider, $urlRouterProvider, $httpProvider) {

            $httpProvider.interceptors.push("myInterceptor");
            $urlRouterProvider.otherwise("/InspectionList");

            var states = [
                tpi.Routes.SetRoutes("home", "home"),
                tpi.Routes.SetRoutes("Account", "AccountLogin"),
                tpi.Routes.SetRoutes("Account", "AccountRegister"),
                tpi.Routes.SetRoutes("Account", "AccountRegisterSuccess"),
                tpi.Routes.SetRoutes("Account", "Profile"),
                tpi.Routes.SetRoutes("Account", "ResetPassword"),
                tpi.Routes.SetRoutes("Inspection", "InspectionList"),
                tpi.Routes.SetRoutes("Inspection", "InspectionView"),
                tpi.Routes.SetRoutes("Inspection", "InspectionPayment"),
                tpi.Routes.SetRoutes("Inspection", "InspectionSuccessfullPayment"),
                tpi.Routes.SetRoutes("Inspection", "InspectionSuccessfullPayment2"),
                tpi.Routes.SetRoutes("Appointment", "AppointmentCreate")
            ];

            states.forEach(function(state) {
                $stateProvider.state(state);
            });

        }
    ]);

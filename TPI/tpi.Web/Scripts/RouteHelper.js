if (typeof (tpi) == "undefined") {
    tpi = {};
};

tpi.Routes = {
    SetRoutes: function() {

        if (arguments.length === 0) return null;

        var entityName = arguments[0] == null ? null : arguments[0];
        var routeName = arguments[1] == null ? null : arguments[1];

        switch (entityName) {
        case "home":
            return {
                name: "home",
                url: "/home",
                views: {
                    "": tpi.Functions.GetBaseRouteView(),
                    "toolbar@home": tpi.Functions.GetToolbarRouteView(),
                    "content@home": {
                        templateUrl: "/Templates/Home/Home.html",
                        controller: "HomeController"
                    }
                }
            };
        case "Account":
            switch (routeName) {
            case "AccountLogin":
                return {
                    name: "AccountLogin",
                    url: "/AccountLogin",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@AccountLogin": tpi.Functions.GetToolbarRouteView(),
                        "content@AccountLogin": {
                            templateUrl: "/Templates/Account/Login.html",
                            controller: "AccountLoginController"
                        }
                    }
                };
            case "AccountRegister":
                return {
                    name: "AccountRegister",
                    url: "/AccountRegister",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@AccountRegister": tpi.Functions.GetToolbarRouteView(),
                        "content@AccountRegister": {
                            templateUrl: "/Templates/Account/Register.html",
                            controller: "AccountRegisterController"
                        }
                    }
                };
            case "AccountRegisterSuccess":
                return {
                    name: "AccountRegisterSuccess",
                    url: "/AccountRegisterSuccess",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@AccountRegisterSuccess": tpi.Functions.GetToolbarRouteView(),
                        "content@AccountRegisterSuccess": {
                            templateUrl: "/Templates/Account/RegistrationSuccess.html"
                        }
                    }
                };
            case "Profile":
                return {
                    name: "Profile",
                    url: "/Profile",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@Profile": tpi.Functions.GetToolbarRouteView(),
                        "content@Profile": {
                            templateUrl: "/Templates/Account/Profile.html",
                            controller: "AccountProfileController",
                            resolve: {
                                customer: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        return appService.GetData(
                                            "/Account/GetCustomer",
                                            {
                                                customerId: currentUser.PortalUser.CustomerId,
                                                portalUserRole: currentUser.PortalUser.PortalUserRole,
                                                customerTypeName: currentUser.PortalUser.Customer.CustomerTypeText
                                            });
                                    }
                                ],
                                orders: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        return appService.GetData(
                                            "/Inspection/GetCustomerOrders",
                                            {
                                                customerId: currentUser.PortalUser.CustomerId,
                                            }); 
                                    }
                                ]
                            }
                        }
                    }
                };
            case "ResetPassword":
                return {
                    name: "ResetPassword",
                    url: "/ResetPassword",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@ResetPassword": tpi.Functions.GetToolbarRouteView(),
                        "content@ResetPassword": {
                            templateUrl: "/Templates/Account/PasswordReset.html",
                            controller: "ResetPasswordController",
                        }
                    }
                };
            default:
                return null;
            }
        case "Inspection":
            switch (routeName) {
            case "InspectionList":
                return {
                    name: "InspectionList",
                    url: "/InspectionList",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@InspectionList": tpi.Functions.GetToolbarRouteView(),
                        "content@InspectionList": {
                            templateUrl: "/Templates/Inspection/InspectionList.html",
                            controller: "InspectionListController"
                        }
                    }
                };
            case "InspectionView":
                return {
                    name: "InspectionView",
                    url: "/InspectionView/:parameter",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@InspectionView": tpi.Functions.GetToolbarRouteView(),
                        "content@InspectionView": {
                            templateUrl: "/Templates/Inspection/InspectionView.html",
                            controller: "InspectionViewController",
                            resolve: {
                                inspection: [
                                    "$stateParams", function($stateParams) {
                                        var parameter = JSON.parse($stateParams.parameter);
                                        if (parameter == null)
                                            return null;
                                        return parameter.inspection;
                                    }
                                ],
                                inspectionDetails: [
                                    "$stateParams", "appService",
                                    function ($stateParams, appService) {
                                        var parameter = JSON.parse($stateParams.parameter);
                                        if (parameter.inspection == null) return null;
                                        return appService.GetData(
                                            "/Inspection/GetInspectionDetailsByStateCodeByInspection",
                                            { stateCode: 0, inspectionId: parameter.inspection.Id });
                                    }
                                ],
                                prePurchasedProducts: [
                                    "$stateParams", "appService", "sessionService",
                                    function($stateParams, appService, sessionService) {
                                        var parameter = JSON.parse($stateParams.parameter);
                                        if (parameter.inspection == null) return null;
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        if (_.isNull(currentUser) || _.isUndefined(currentUser)) {
                                            return appService.GetData(
                                                "/Product/GetCartItemsByInspection",
                                                {
                                                    inspectionId: parameter.inspection.Id == null
                                                        ? "00000000-0000-0000-0000-000000000000"
                                                        : parameter.inspection.Id,
                                                    priceLists: []
                                                });
                                        }
                                        return appService.GetData(
                                            "/Product/GetCartItemsByInspection",
                                            {
                                                inspectionId: parameter.inspection.Id == null
                                                    ? "00000000-0000-0000-0000-000000000000"
                                                    : parameter.inspection.Id,
                                                priceLists: currentUser.PortalUser.PriceLists
                                            });
                                    }
                                ],
                                unPurchasedProducts: [
                                    "$stateParams", "appService", "sessionService",
                                    function($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        if (_.isNull(currentUser) || _.isUndefined(currentUser)) {
                                            return appService.GetData(
                                                "/Product/GetPriceListItemsByPriceLists",
                                                {
                                                    priceLists: []
                                                });
                                        }
                                        return appService.GetData(
                                            "/Product/GetPriceListItemsByPriceLists",
                                            {
                                                priceLists: currentUser.PortalUser.PriceLists
                                            });
                                    }
                                ],
                                cartItems: [
                                    "$stateParams", "appService", "sessionService",
                                    function($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        if (_.isNull(currentUser) || _.isUndefined(currentUser)) {
                                            return [];
                                        }
                                        return appService.GetData(
                                            "/Opportunity/GetOpportunitiesProductsByCustomerIdByInspectionId",
                                            { opportunityId: currentUser.OpportunityId });
                                    }
                                ],
                                priceLists: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        if (_.isNull(currentUser) || _.isUndefined(currentUser)) {
                                            return appService.GetData(
                                                "/PriceList/GetPriceListItemsByPortalUserRole",
                                                { portalUserRole: 858890000 });
                                        }
                                        return [];
                                    }
                                ],
                                address: [
                                    "$stateParams", function ($stateParams) {
                                        var parameter = JSON.parse($stateParams.parameter);
                                        if (parameter == null)
                                            return null;
                                        return parameter.address;
                                    }
                                ],
                            }
                        }
                    }
                };
            case "InspectionPayment":
                return {
                    name: "InspectionPayment",
                    url: "/InspectionPayment/:parameter",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@InspectionPayment": tpi.Functions.GetToolbarRouteView(),
                        "content@InspectionPayment": {
                            templateUrl: "/Templates/Inspection/InspectionPayment.html",
                            controller: "InspectionPaymentController",
                            resolve: {
                                cart: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        return appService.GetData(
                                            "/Opportunity/GetOpportunity",
                                            { opportunityId: currentUser.OpportunityId });
                                    }
                                ],
                                cartItems: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        return appService.GetData(
                                            "/Opportunity/GetOpportunitiesProductsByCustomerIdByInspectionId",
                                            { opportunityId: currentUser.OpportunityId });
                                    }
                                ],
                                paymentMethods: [
                                    "$stateParams", "appService", "sessionService",
                                    function ($stateParams, appService, sessionService) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        return appService.GetData(
                                            "/PaymentMethod/GetPaymentMethodsByPriceList", { priceListId: currentUser.PriceListId });
                                    }
                                ],
                                contacts: [
                                    "$stateParams", "appService",
                                    function ($stateParams, appService) {
                                        return appService.GetData(
                                            "/Contact/GetInspectionsByAddress");
                                    }
                                ],
                            }
                        }
                    }
                };
            case "InspectionSuccessfullPayment":
                return {
                    name: "InspectionSuccessfullPayment",
                    url: "/InspectionSuccessfullPayment",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@InspectionSuccessfullPayment": tpi.Functions.GetToolbarRouteView(),
                        "content@InspectionSuccessfullPayment": {
                            templateUrl: "/Templates/Inspection/InspectionSuccessfullPayment.html",
                            controller: "InspectionPaymentSuccessfullController",
                            resolve: {
                                order: [
                                    "$stateParams", "appService", "sessionService", "$location",
                                    function ($stateParams, appService, sessionService, $location) {
                                        var currentUser = sessionService.User.GetCurrentUser();
                                        if (currentUser == null || currentUser.OrderId == null)
                                            return appService.GetData("/Order/GetOrderByAccessCode", { accessCode: $location.$$search.AccessCode });
                                        else
                                            return appService.GetData("/Order/GetOrder", { orderId: currentUser.OrderId });
                                    }
                                ],
                                transactionReturnMessage: [
                                    "$stateParams", "appService", "$location",
                                    function ($stateParams, appService, $location) {
                                        console.log("VerifyAccessCode");
                                        return appService.GetData(
                                            "/Transaction/VerifyAccessCode",
                                            {
                                                accessCode: $location.$$search.AccessCode
                                            });
                                    }
                                ]
                            }
                        }
                    }
                };
            case "InspectionSuccessfullPayment2":
                return {
                    name: "InspectionSuccessfullPayment2",
                    url: "/InspectionSuccessfullPayment2?orderNumber&amount&paymentMethod",
                    views: {
                        "": tpi.Functions.GetBaseRouteView(),
                        "toolbar@InspectionSuccessfullPayment2": tpi.Functions.GetToolbarRouteView(),
                        "content@InspectionSuccessfullPayment2": {
                            templateUrl: "/Templates/Inspection/InspectionSuccessfullPayment2.html",
                            controller: "InspectionPaymentSuccessfull2Controller",
                            resolve: {
                                bankingDetails: [
                                    "appService",
                                    function (appService) {
                                        return appService.GetData("/BankingDetail/GetBankAccountDetails");
                                    }
                                ],
                                details: [
                                    "$stateParams","appService",
                                    function ($stateParams, appService) {
                                        return { orderNumber: $stateParams.orderNumber, amount: $stateParams.amount, paymentMethod: $stateParams.paymentMethod }
                                    }
                                ],
                            }
                        }
                    }
                };
            default:
                return null;
            }
        case "Appointment":
            switch (routeName) {
                case "AppointmentCreate":
                    return {
                        name: "AppointmentCreate",
                        url: "/AppointmentCreate/:parameter",
                        views: {
                            "": tpi.Functions.GetBaseRouteView(),
                            "toolbar@AppointmentCreate": tpi.Functions.GetToolbarRouteView(),
                            "content@AppointmentCreate": {
                                templateUrl: "/Templates/Inspection/ScheduleAppointment.html",
                                controller: "AppointmentCreateController",
                                resolve: {
                                    product: [
                                        "$stateParams", function ($stateParams) {
                                            var parameter = JSON.parse($stateParams.parameter);
                                            if (parameter == null)
                                                return null;
                                            return parameter.product;
                                        }
                                    ],
                                    inspectors: [
                                        "$stateParams", "appService", function ($stateParams, appService) {
                                            var parameter = JSON.parse($stateParams.parameter);
                                            if (parameter == null)
                                                return null;
                                            var product = parameter.product;
                                            return appService.GetData("/SystemUser/GetSystemUsersBySkills", { skills: product.ProductSkills });
                                        }
                                    ],
                                    TsAndCs: [
                                        "$stateParams", "appService", "sessionService",
                                        function ($stateParams, appService, sessionService) {
                                            var currentUser = sessionService.User.GetCurrentUser();
                                            if (_.isNull(currentUser) || _.isNull(currentUser.PortalUser) || _.isNull(currentUser.PortalUser) || _.isNull(currentUser.PortalUser.PriceLists)
                                                || currentUser.PortalUser.PriceLists.length === 0)
                                                return [];
                                            var priceListToUse = _.find(currentUser.PortalUser.PriceLists, function (x) {
                                                return x.Id === currentUser.PriceListId
                                            });
                                            if (_.isUndefined(priceListToUse)) return [];
                                            return appService.GetData(
                                                "/QuestionSetup/GetTermsAndConditionsByQuestionSetup", { questionSetupId: priceListToUse.QuestionSetupId });
                                        }
                                    ]
                                }
                            }
                        }
                    };
                default:
                    return null;
            }
        default:
        }
        return null;

    }
};

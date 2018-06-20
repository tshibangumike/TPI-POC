/// <reference path="../Libs/lodash/js/lodash.min.js" />
/// <reference path="~/Scripts/AppDirective.js" />

angular.module("tpiApp")
    .controller("InspectionListController",
    [
        "$rootScope", "$scope", "appService", "sessionService", "$window",
        function ($rootScope, $scope, appService, sessionService, $window) {

            $scope.Inspections = [];
            $scope.SearchText = null;
            $scope.SelectedInspection = null;
            $scope.SearchHasBeenInitiated = false;
            $scope.CurrentUser = _.isNull(sessionService.User.GetCurrentUser()) ? null : sessionService.User.GetCurrentUser();
            $scope.InspectionRelatedProducts = [];
            $scope.InspectionNonRelatedProducts = [];
            $scope.SelectedPriceList = {};
            $scope.Option = "";
            $scope.Detail = "";
            var addressObject = {};
            $scope.SearchHasBeenInitiated = false;

            $scope.Section_SiteSearch = { Name: "Site Search", IsVisible: true };
            $scope.Section_SearchResult = { Name: "Site Search", IsVisible: false };

            this.init = function () {

                if (_.isNull($scope.CurrentUser)) return null;

                $scope.CurrentUser.Inspection = {};
                $scope.CurrentUser.Inspection = { Id: "00000000-0000-0000-0000-000000000000", Name: "" };
                $scope.CurrentUser.OpportunityId = "00000000-0000-0000-0000-000000000000";
                $scope.CurrentUser.PriceListId = "00000000-0000-0000-0000-000000000000";
                $scope.CurrentUser.OrderId = "00000000-0000-0000-0000-000000000000";
                $scope.CurrentUser.PaymentMethodId = "00000000-0000-0000-0000-000000000000";
                $scope.CurrentUser.Appointments = [];
                $scope.CurrentUser.Address = {};
                sessionService.User.SetCurrentUser($scope.CurrentUser);

                return null;
            };

            $scope.checkIfEnterKeyWasPressed = function ($event, searchText) {
                var keyCode = $event.which || $event.keyCode;
                if (keyCode === 13) {
                    $scope.SearchForAddress(searchText);
                }
            };

            $scope.SearchForAddress = function () {
                if (arguments.length === 0) return null;
                var searchText = arguments[0] == null ? null : arguments[0];
                if (searchText == null || searchText.length < 3) return null;

                try {

                    if ($("#addressComponents") == undefined || $("#addressComponents") == null ||
                        $("#addressComponents")[0] == undefined || $("#addressComponents")[0] == null ||
                        $("#addressComponents")[0].value == undefined || $("#addressComponents")[0].value == null ||
                        $("#addressComponents")[0].value == "") {
                        toastr.warning("Please select address from google autocomplete suggestions!");
                        return;
                    }

                    $rootScope.LoadingText = "Searching...";
                    $scope.SearchHasBeenInitiated = true;

                    $scope.InspectionRelatedProducts = [];
                    $scope.SelectedInspection = null;

                    addressObject.FullAddress = searchText;

                    if ($("#addressComponents")[0].value != "") {
                        var jsonAddress = JSON.parse($("#addressComponents")[0].value);

                        if (!_.isNull(jsonAddress) && !_.isUndefined(jsonAddress)) {

                            _.forEach(jsonAddress,
                                function (value) {

                                    if (value != undefined && value.types[0] == "street_number") {
                                        addressObject.StreetNumber = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "route") {
                                        addressObject.StreetAddress = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "sublocality_level_1") {
                                        addressObject.SubLocality = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "locality") {
                                        addressObject.Suburb = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "administrative_area_level_2") {
                                        addressObject.City = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "administrative_area_level_1") {
                                        addressObject.Province = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "country") {
                                        addressObject.Country = value.long_name;
                                    }
                                    if (value != undefined && value.types[0] == "postal_code") {
                                        addressObject.PostalCode = value.long_name;
                                    }

                                });
                        }

                    }

                    appService.GetData("/Inspection/GetInspectionByAddressParts", {
                        searchText: searchText,
                        unitNumber: addressObject.UnitNumber,
                        streetNumber: addressObject.StreetNumber,
                        streetAddress: addressObject.StreetAddress,
                        subLocality: addressObject.SubLocality,
                        suburb: addressObject.Suburb,
                        city: addressObject.City,
                        state: addressObject.State,
                        country: addressObject.Country,
                        postalCode: addressObject.PostalCode
                    })
                        .success(function (response) {

                            $scope.Section_SearchResult.IsVisible = true;
                            $scope.Inspections = response;

                            if ($scope.Inspections.length === 0) {

                                var emptyInspectionObject = {
                                    Id: "00000000-0000-0000-0000-000000000000",
                                    Name: "",
                                    InspectionAddress: searchText
                                };

                                var parameter = { inspection: emptyInspectionObject, address: addressObject };

                                $rootScope.LoadingText = "Working...";

                                appService.NavigateTo("InspectionView", { parameter: JSON.stringify(parameter) });

                            }
                        });

                } catch (err) {

                    tpi.Loading.Stop();

                }

                return null;
            };

            $scope.NavigateTo = function () {


                var inspection = arguments[0] == null ? null : arguments[0];
                if (inspection == null) {

                    var emptyInspectionObject = {
                        Id: "00000000-0000-0000-0000-000000000000",
                        Name: "",
                        InspectionAddress: $scope.SearchText
                    };

                    var parameter = { inspection: emptyInspectionObject, address: addressObject };

                    $rootScope.LoadingText = "Working...";

                    appService.NavigateTo("InspectionView", { parameter: JSON.stringify(parameter) });

                    return;

                }

                $rootScope.LoadingText = "Working...";

                var parameter = { inspection: inspection };
                appService.NavigateTo("InspectionView", { parameter: JSON.stringify(parameter) });
                return null;
            };

            $scope.NewSearch = function (searchText) {

                var emptyInspectionObject = {
                    Id: "00000000-0000-0000-0000-000000000000",
                    Name: "",
                    InspectionAddress: searchText
                };

                var parameter = { inspection: emptyInspectionObject, address: addressObject };

                $rootScope.LoadingText = "Working...";

                appService.NavigateTo("InspectionView", { parameter: JSON.stringify(parameter) });

                return;
            };

            this.init();

        }
    ])
    .controller("InspectionViewController",
    [
        "$rootScope", "$scope", "$location", "appService", "sessionService", "inspection",
        "inspectionDetails", "prePurchasedProducts", "unPurchasedProducts", "cartItems", "priceLists", "address",
        function ($rootScope,
            $scope,
            $location,
            appService,
            sessionService,
            inspection,
            inspectionDetails,
            prePurchasedProducts,
            unPurchasedProducts,
            cartItems,
            priceLists,
            address) {

            /*Scope Variables - START*/
            $scope.Inspection = inspection == null ? null : inspection;
            $scope.PrePurchasedProducts = prePurchasedProducts == null ? [] : prePurchasedProducts.data;
            $scope.UnPurchasedProducts = unPurchasedProducts == null ? [] : unPurchasedProducts.data;
            $scope.CartItems = cartItems.data;
            $scope.Address = address;
            $scope.InspectionDetails = inspectionDetails.data;
            $scope.CurrentUser = sessionService.User.GetCurrentUser();
            $scope.PriceLists = (_.isNull($scope.CurrentUser) || _.isUndefined($scope.CurrentUser)) ?
                priceLists.data : $scope.CurrentUser.PortalUser.PriceLists;
            /*Scope Variables - END*/

            /*Variables - START*/
            $scope.SelectedPriceList = {};
            $scope.Content = { SiteAddress: true, ExistingProducts: true, NewProducts: true };
            /*Variables - END*/

            /*Function - START*/
            this.init = function () {

                tpi.Loading.Stop();

                if (_.isNull($scope.CurrentUser) ||
                    _.isUndefined($scope.CurrentUser) ||
                    _.isEqual($scope.CurrentUser.PriceListId, "00000000-0000-0000-0000-000000000000")) {

                    $scope.SelectedPriceList.Id = $scope.PriceLists[0].Id;
                    return;

                } else {
                    $scope.SelectedPriceList.Id = $scope.CurrentUser.PriceListId;
                }

                $scope.CurrentUser.Inspection = { Id: $scope.Inspection.Id, Name: "" };
                $scope.CurrentUser.Inspection.Name = $scope.Inspection.InspectionAddress;
                $scope.CurrentUser.Inspection.InspectionAddress = $scope.Inspection.InspectionAddress;
                $scope.CurrentUser.Appointments = [];
                $scope.CurrentUser.Address = $scope.Address;
                sessionService.User.SetCurrentUser($scope.CurrentUser);

                return null;
            };
            $scope.HideShow = function () {

                if (arguments.length === 0) return null;
                var record = arguments[0] == null ? null : arguments[0];

                $scope.Content[record] = !$scope.Content[record];

            };

            $scope.GetUnParentPurchasedProducts = function () {

                var productsByPriceList = _.filter($scope.UnPurchasedProducts,
                    function (x) { return (x.PriceListId === $scope.SelectedPriceList.Id && x.ProductId != "00000000-0000-0000-0000-000000000000" && x.ParentProductName != ""); });

                var parentProductsByPriceList = _.uniqBy(productsByPriceList, "ParentProductName")

                _.forEach(parentProductsByPriceList, function (value, key) {
                    var id;
                    _.forEach(value, function (_value, _key) {
                        if (_.isEqual(_key, "ParentProductName")) {
                            id = "un" + _value.replace(/ /g, "a").replace(/,/g, "a").replace(/-/g, "a").replace(/&/g, "a").replace(/,/g, "a");
                            return true;
                        }
                    });
                    value.ParentProductId = id;
                });

                return parentProductsByPriceList;

            };

            $scope.GetInspectionNonRelatedProductByPriceList = function (parentProductName) {

                return _.filter($scope.UnPurchasedProducts,
                    function (x) { return ((x.PriceListId === $scope.SelectedPriceList.Id) && _.isEqual(x.ParentProductName, parentProductName)); });

            };

            $scope.GetPrePurchasedParentProducts = function () {

                var parentProducts = _.uniqBy($scope.PrePurchasedProducts, "ParentProductName");

                _.forEach(parentProducts, function (value, key) {
                    var id;
                    _.forEach(value, function (_value, _key) {
                        if (_.isEqual(_key, "ParentProductName")) {
                            id = "pre" + _value.replace(/ /g, "a").replace(/,/g, "a").replace(/-/g, "a").replace(/&/g, "a").replace(/,/g, "a");
                            return true;
                        }
                    });
                    value.ParentProductId = id;
                });

                return parentProducts;

            };

            $scope.GetPrePurchasedProducts = function (parentProductName) {

                return _.filter($scope.PrePurchasedProducts,
                    function (x) { return (_.isEqual(x.ParentProductName, parentProductName)) });

            };

            $scope.Toggle = function (recordId) {
                var record = _.find($scope.PrePurchasedProducts,
                    function (x) { return (_.isEqual(x.ParentProductId, recordId)) });
                record.Collapse = !record.Collapse;
            };

            $scope.AddItemToShoppingCart = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];

                try {

                    if (_.isNull($scope.CurrentUser) || _.isUndefined($scope.CurrentUser)) {

                        sessionStorage.setItem("returnToUrl", $location.$$absUrl);
                        $("#modal").modal("show");
                        return null;

                    }

                    $rootScope.LoadingText = "Adding item to cart...";

                    /*Create opportunity Object - START*/
                    var opportunity = {
                        CustomerId: $scope.CurrentUser.PortalUser.CustomerId,
                        PriceListId: $scope.SelectedPriceList.Id,
                        InspectionId: $scope.Inspection.Id
                    };
                    if ($scope.CurrentUser.PortalUser.Customer.CustomerTypeText === "account") {
                        opportunity.ParentAccountId = $scope.CurrentUser.PortalUser.CustomerId;
                    } else if ($scope.CurrentUser.PortalUser.Customer.CustomerTypeText === "contact") {
                        opportunity.ParentContactId = $scope.CurrentUser.PortalUser.CustomerId;
                    }
                    /*Create opportunity Object - END*/

                    /*Creat opportunity product object - START*/
                    var opportunityProduct = {
                        ProductId: product.ProductId,
                        UomId: product.UomId,
                        Amount: product.Amount,
                        IsPriceOverriden: product.IsPriceOverriden,
                        ProductCategory: product.ProductCategory,
                        ReportPriority: product.ReportPriority,
                        SellableTo: product.SellableTo,
                        FreeReport: product.FreeReport
                    };
                    /*Creat opportunity product object - END*/

                    if (_.isEqual(product.InspectionDetailStateCode, 1) && _.isEqual(product.InspectionDetailStatusCode, 2)) {
                        opportunityProduct.OnBackOrder = false;
                    }
                    else {
                        opportunityProduct.OnBackOrder = true;
                    }

                    if ($scope.CurrentUser.OpportunityId === "00000000-0000-0000-0000-000000000000") {

                        /*Send Create Opportunity Request to server - START */
                        appService.GetData("/Opportunity/CreateOpportunityAndOpportunityProduct",
                            { opportunity: opportunity, opportunityProduct: opportunityProduct })
                            .success(function (response) {

                                var opportunityId = response;

                                $scope.CurrentUser.OpportunityId = opportunityId;
                                $scope.CurrentUser.PriceListId = $scope.SelectedPriceList.Id;
                                sessionService.User.SetCurrentUser($scope.CurrentUser);

                                appService.RefreshCurrentState();

                            });
                        /*Send Create Opportunity Request to server - END */

                    } else {

                        opportunityProduct.OpportunityId = $scope.CurrentUser.OpportunityId;

                        /*Send Create Opportunity Product Request to server - START */
                        appService.GetData("/Opportunity/CreateOpportunityProduct",
                            { opportunityProduct: opportunityProduct })
                            .success(function (response) {

                                appService.RefreshCurrentState();

                            });
                        /*Send Create Opportunity Product Request to server - END */

                    }

                    return null;

                } catch (err) {

                    console.log(err);
                    return appService.RefreshCurrentState();

                }

            };
            $scope.DisableAddCartButton = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];
                if (product == null) return false;

                var productInCart = _.find($scope.CartItems,
                    function (x) { return x.ProductId === product.ProductId; });

                if (_.isUndefined(productInCart)) return false;
                return true;

            };
            $scope.DisablePriceListOption = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];
                if (product == null) return false;

                if (_.isUndefined($scope.CartItems) || _.isNull($scope.CartItems) || _.isEqual($scope.CartItems.length, 0)) return false;
                return (!_.isEqual(product.PriceListItemId, $scope.CurrentUser.PriceListId));

            };
            $scope.DisableProduct = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];
                if (product == null) return false;

                if (_.isEqual(product.StateCode, 1) && _.isEqual(product.StatusCode, 2)) return false;

                var activeProduct = _.find($scope.InspectionDetails,
                    function (x) { return x.ProductId === product.Id; });

                if (_.isUndefined(activeProduct)) return false;
                return true;

            };
            $scope.DisableExistingProduct = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];
                if (product == null) return false;

                if (_.isEqual(product.InspectionDetailStateCode, 1) && _.isEqual(product.InspectionDetailStatusCode, 2)) {
                    return $scope.DisableProduct(product);
                }
                return true

            };
            $scope.Checkout = function () {

                if ($scope.CartItems.length === 0) {
                    //toastr.warning("Please add item(s) to your cart before you check out!");
                    return null;
                }

                ///*Store Cart Item Ids in session - START*/
                //var products = [];
                //_.forEach($scope.OpportunityProducts,
                //    function (value) {
                //        products.push({ Id: value.ProductId, Name: value.Product.Name, ProductCategory: value.ProductCategory, InspectorSkills: value.Product.InspectorSkills });
                //    });
                //$scope.CurrentUser.Products = products;
                //sessionService.User.SetCurrentUser($scope.CurrentUser);
                ///*Store Cart Item Ids in session - END*/

                $rootScope.LoadingText = "Working...";

                //var parameter = { opportunityProducts: $scope.OpportunityProducts };
                appService.NavigateTo("InspectionPayment");
                return null;

            };
            $scope.Back = function () {

                window.location.href = "index.html#/InspectionList";
                //appService.NavigateTo("InspectionList");

            };
            $scope.Login = function () {
                $("#modal").modal('hide');
                sessionStorage.setItem("returnToUrl", $location.$$absUrl);
                appService.NavigateTo("AccountLogin");
            };
            $scope.Register = function () {
                $("#modal").modal('hide');
                appService.NavigateTo("AccountRegister");
            };
            /*Function - END*/

            this.init();

        }
    ])
    .controller("InspectionPaymentController",
    [
        "$rootScope", "$scope", "$location", "$uibModal", "appService", "sessionService", "cart", "cartItems", "paymentMethods", "contacts",
        function ($rootScope, $scope, $location, $uibModal, appService, sessionService, cart, cartItems, paymentMethods, contacts) {

            $scope.CartItems = cartItems.data;
            $scope.PaymentMethod = null;
            $scope.CurrentUser = sessionService.User.GetCurrentUser();
            $scope.PaymentMethods = paymentMethods.data;
            $scope.Cart = cart.data;
            $scope.Contacts = contacts.data;
            $scope.SelectedPaymentMethod = {};
            $scope.VoucherAppliedSuccessfully = $scope.Cart.DiscountAmount === -1 ? false : true;
            $scope.Appointments = [];
            $scope.InspectionDetailObjects = [];
            $scope.Dropped = false;
            $scope.UserCanCreateInspection = null;
            $scope.Content = { Cart: true, PropertyInformation: false, Questions: false, PaymentMethod: true, TC: false };
            $scope.FormHasBeenSubmitted = false;
            $scope.customer = { Country: "au" };
            $scope.creditCard = {};
            $scope.ExpirtyMonths = ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"];
            $scope.ExpirtyYears = [];

            this.init = function () {

                if (_.isEqual($scope.CurrentUser.Inspection.Id, "00000000-0000-0000-0000-000000000000")) {
                    $scope.UserCanCreateInspection = true;
                    $scope.Inspection = {};
                    $scope.Inspection.Id = "00000000-0000-0000-0000-000000000000";
                    $scope.Inspection.Name = $scope.CurrentUser.Address.FullAddress;
                    $scope.Inspection.StreetNumber = $scope.CurrentUser.Address.StreetNumber;
                    $scope.Inspection.StreetAddress = $scope.CurrentUser.Address.StreetAddress;
                    $scope.Inspection.SubLocality = $scope.CurrentUser.Address.SubLocality;
                    $scope.Inspection.Suburb = $scope.CurrentUser.Address.Suburb;
                    $scope.Inspection.City = $scope.CurrentUser.Address.City;
                    $scope.Inspection.State = $scope.CurrentUser.Address.Province;
                    $scope.Inspection.PostalCode = $scope.CurrentUser.Address.PostalCode;
                    $scope.Inspection.Country = $scope.CurrentUser.Address.Country == null ? "47 Pretorius Rd, Vorna Valley, Midrand, 1686, South Africa" : $scope.CurrentUser.Address.Country;
                } else {
                    $scope.Inspection = $scope.CurrentUser.Inspection;
                    $scope.UserCanCreateInspection = false;
                }

                if (_.isNull($scope.CurrentUser.Appointments) || _.isUndefined($scope.CurrentUser.Appointments)) {
                    return null;
                }

                $scope.Appointments = $scope.CurrentUser.Appointments;

                var productIds = [];

                _.forEach($scope.CartItems, function (value) {
                    productIds.push(value.ProductId);
                });

                var currentYear = (new Date()).getFullYear();
                for (var i = currentYear; i < (currentYear + 30); i++) {
                    $scope.ExpirtyYears.push(i);
                }

                if (!$scope.DisplayQuestions()) return;

                appService.GetData("/QuestionSetup/GetQuestionSetupByQuestionSetup", { productIds: productIds, priceListId: $scope.CurrentUser.PriceListId })
                    .success(function (response) {

                        $scope.Questions = response;

                    });

            };

            $scope.OnChangeDate = function () {

                if (arguments.length === 0) return null;
                var question = arguments[0] == null ? null : arguments[0];

                question.Answer = new Date(question.Answer);

            };

            $scope.DisplayQuestions = function () {

                return $scope.Appointments.length > 0;

            };

            $scope.HideShow = function () {

                if (arguments.length === 0) return null;
                var record = arguments[0] == null ? null : arguments[0];

                $scope.Content[record] = !$scope.Content[record];

            };

            $scope.SetAdditionalProduct = function () {

            };

            $scope.GetPaymentMethods = function () {

                var filteredPaymentMethods = [];

                _.forEach($scope.PaymentMethods, function (value) {

                    var hasAccount = $scope.CurrentUser.PortalUser.Customer.HasAccount;

                    if (value.Name == "Account" && !hasAccount) {
                        return;
                    }

                    filteredPaymentMethods.push(value);

                });

                return filteredPaymentMethods;

            };

            $scope.GetQuestions = function () {

                if (!$scope.DisplayQuestions()) return [];

                return _.orderBy(_.filter($scope.Questions, function (x) {
                    return x.Type == 858890001;
                }), ['Number'], ['asc']);

            };

            $scope.GetTermsAndConditions = function () {

                if (!$scope.DisplayQuestions()) return [];

                return _.filter($scope.Questions, function (x) {
                    return x.Type == 858890002;
                });

            };

            $scope.RedeemVoucher = function () {

                $rootScope.LoadingText = "Validating voucher number...";

                appService.GetData("/Voucher/GetVoucherByVoucherNumberByCustomer",
                    { voucherNumber: $scope.VoucherNumber, customerId: $scope.CurrentUser.PortalUser.CustomerId })
                    .success(function (response) {

                        if (response == "") {
                            toastr.error("Voucher Number is not valid!");
                            return;
                        }

                        $scope.Voucher = response;

                        var opportunityObject = {
                            Id: $scope.CurrentUser.OpportunityId,
                            DiscountAmount: $scope.Voucher.Amount,
                            VoucherId: $scope.Voucher.Id
                        };

                        $rootScope.LoadingText = "Applying voucher...";

                        appService.GetData("/Opportunity/ApplyDiscount",
                            { opportunity: opportunityObject, voucherId: $scope.Voucher.Id })
                            .success(function (response) {

                                $rootScope.LoadingText = "Working...";

                                appService.RefreshCurrentState();

                            });

                    });
            };

            $scope.Schedule = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];

                var parameter = { product: product };
                appService.NavigateTo("AppointmentCreate", { parameter: JSON.stringify(parameter) });

                return;

            };

            $scope.DisableScheduleButton = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];

                if (product)

                    var appointment = _.find($scope.Appointments,
                        function (x) { return _.isEqual(x.ProductId.toUpperCase(), product.ProductId.toUpperCase()); });

                if (_.isUndefined(appointment)) return false

                return true;

            };

            $scope.RemoveAppointment = function () {

                if (arguments.length === 0) return null;
                var product = arguments[0] == null ? null : arguments[0];

                _.remove($scope.Appointments, function (x) {
                    return _.isEqual(x.ProductId, product.ProductId);
                });

                $scope.CurrentUser.Appointments = $scope.Appointments;
                sessionService.User.SetCurrentUser($scope.CurrentUser);

            };

            $scope.Back = function () {

                var parameter = { inspection: $scope.CurrentUser.Inspection };
                appService.NavigateTo("InspectionView", { parameter: JSON.stringify(parameter) });

            };

            $scope.MultiOptionSetClicked = function () {

                if (arguments.length === 0) return null;
                var question = arguments[0] == null ? null : arguments[0];
                var trueOrFalse = arguments[1] == null ? null : arguments[1];
                var value = arguments[2] == null ? null : arguments[2];

                if (trueOrFalse == true) {
                    if (_.isNull(question.Answer) || _.isEqual(question.Answer, "")) {
                        question.Answer = value;
                    }
                    if (!_.includes(question.Answer, value)) {
                        question.Answer += ("," + value);
                    }
                }
                if (trueOrFalse == false) {
                    if (_.includes(question.Answer, ("," + value))) {
                        question.Answer = _.replace(question.Answer, ("," + value), "");
                    }
                    else if (_.includes(question.Answer, (value + ","))) {
                        question.Answer = _.replace(question.Answer, (value + ","), "");
                    }
                    else if (_.includes(question.Answer, value)) {
                        question.Answer = _.replace(question.Answer, value, "");
                    }
                }

            };

            $scope.DisablePaymentMethods = function () {

                if ($scope.Cart.TotalAmount <= 0) return true;
                return false;

            };

            $scope.ShowPaymentMethods = function () {

                var newProducts = _.filter($scope.CartItems, function (x) {
                    return !x.ProductCategory;
                })

                if (newProducts.length > 0) {
                    return $scope.DisplayQuestions();
                }
                else {
                    return true;
                }

            };

            $scope.ProcessForm = function () {

                if (arguments.length === 0) return null;
                var form = arguments[0] == null ? null : arguments[0];

                try {

                    var combinedPropertyObjects = [];

                    var filteredCartItems = _.filter($scope.CartItems, function (x) {
                        return !x.ProductCategory;
                    })

                    if (!_.isEqual($scope.Appointments.length, filteredCartItems.length)) {
                        toastr.warning("Please schedule an appointment first!");
                        return;
                    }

                    var paymentMethod = _.find($scope.PaymentMethods, function (x) { return _.isEqual(x.Name, $scope.SelectedPaymentMethod.Name) });

                    _.forEach($scope.Appointments,
                        function (value) {

                            var appointment = value;
                            appointment.RegardingObjectId = $scope.CurrentUser.OrderId;

                            var cartItem = _.find($scope.CartItems, function (x) { return _.isEqual(x.ProductId, value.ProductId) });

                            var inspectionDetail = {
                                Name: value.Subject,
                                ProductId: value.ProductId,
                                OrderId: $scope.CurrentUser.OrderId,
                                OwnerId: value.OwnerId,
                                ProductCategory: value.ProductCategory,
                                StatusCode: $scope.SelectedPaymentMethod.Name == "eWAY" ? 858890004 : 858890003,
                                InspectionId: $scope.Inspection.Id,
                                FreeReport: cartItem.FreeReport,
                                OnBackOrder: cartItem.OnBackOrder,
                                ReportIsReleasedTo: cartItem.ReportIsReleasedTo,
                                SellableTo: cartItem.SellableTo,
                                IsStrataReport: cartItem.IsStrataReport,
                                AppointmentStartTime: appointment.StartTime
                            };

                            if ($scope.CurrentUser.PortalUser.Customer.CustomerTypeText === "account") {
                                $scope.Inspection.AccountId = $scope.CurrentUser.PortalUser.CustomerId;
                                inspectionDetail.AccountId = $scope.CurrentUser.PortalUser.CustomerId;
                            } else if ($scope.CurrentUser.PortalUser.Customer.CustomerTypeText === "contact") {
                                $scope.Inspection.ContactId = $scope.CurrentUser.PortalUser.CustomerId;
                                inspectionDetail.ContactId = $scope.CurrentUser.PortalUser.CustomerId;
                            }

                            combinedPropertyObjects.push({
                                Apppointment: value,
                                Inspection: inspectionDetail,
                                DoCreateAnInspection: $scope.UserCanCreateInspection,
                                OpportunityId: $scope.CartItems[0].OpportunityId,
                                Property: $scope.Inspection
                            });

                        });

                    if ($scope.Appointments.length == 0) {

                        _.forEach($scope.CartItems,
                            function (value) {

                                combinedPropertyObjects.push({
                                    OpportunityId: $scope.CartItems[0].OpportunityId,
                                    Property: $scope.Inspection
                                });

                            });
                    }

                    if ($scope.Cart.TotalAmount <= 0) {
                        $scope.SelectedPaymentMethod.Name = "Voucher";
                    }

                    $scope.CurrentUser.BackOrderProductExists = _.isUndefined(_.find($scope.CartItems, function (x) { return x.OnBackOrder; })) ? false : true;
                    sessionService.User.SetCurrentUser($scope.CurrentUser);

                    _.forEach($scope.Questions,
                        function (value) {

                            value.OpportunityId = $scope.CurrentUser.OpportunityId;

                        });

                    switch ($scope.SelectedPaymentMethod.Name) {
                        case "eWAY":
                            {
                                appService.GetData("/Appointment/ProceedWithCheckout",
                                    { combinedPropertyObjects: combinedPropertyObjects, questions: $scope.Questions })
                                    .success(function (response) {

                                        $rootScope.LoadingText = "Redirecting to eWAY...";
                                        $scope.Order = response;
                                        $scope.CurrentUser.OrderId = $scope.Order.Id;
                                        $scope.CurrentUser.PaymentMethodId = paymentMethod.Id;
                                        sessionService.User.SetCurrentUser($scope.CurrentUser);

                                        appService.GetData("/Transaction/EwayTransaction",
                                            { order: $scope.Order })
                                            .success(function (response) {

                                                var transactionObject = response;
                                                if (transactionObject == null ||
                                                    transactionObject.Order == null ||
                                                    transactionObject.SharedPaymentUrl == null) {
                                                    return null;
                                                }
                                                tpi.Loading.Start("Redirecting to eWAY...");
                                                window.location.href = transactionObject.SharedPaymentUrl;
                                                return null;

                                            });

                                    });
                                break;
                            }
                        case "EFT":
                            {

                                appService.GetData("/Appointment/ProceedWithCheckout",
                                    { combinedPropertyObjects: combinedPropertyObjects, questions: $scope.Questions })
                                    .success(function (response) {

                                        $rootScope.LoadingText = "Creating Payment...";
                                        var order = response;
                                        var payment = {
                                            Name: "EFT Payment",
                                            PaymentMethodId: paymentMethod.Id,
                                            PaymentDate: new Date(),
                                            Reference: order.OrderNumber,
                                            OrderId: order.Id,
                                            StateCode: 0,
                                            StatusCode: 1
                                        };
                                        appService.GetData("/Payment/CreatePayment",
                                            { payment: payment })
                                            .then(function (response) {

                                                appService.NavigateTo("InspectionSuccessfullPayment2", { orderNumber: order.OrderNumber, amount: order.TotalAmount, paymentMethod: $scope.SelectedPaymentMethod.Name });

                                            });

                                    });
                                break;
                            }
                            break;
                        case "Account":
                            {

                                appService.GetData("/Appointment/ProceedWithCheckout",
                                    { combinedPropertyObjects: combinedPropertyObjects, questions: $scope.Questions })
                                    .success(function (response) {

                                        $rootScope.LoadingText = "Creating Payment...";

                                        var order = response;
                                        var payment = {
                                            Name: "Account Payment",
                                            Amount: 0,
                                            PaymentMethodId: paymentMethod.Id,
                                            PaymentDate: new Date(),
                                            Reference: order.OrderNumber,
                                            OrderId: order.Id,
                                            StatusCode: 858890002,
                                            StateCode: 0
                                        };
                                        appService.GetData("/Payment/CreatePayment",
                                            { payment: payment })
                                            .then(function (response) {

                                                appService.NavigateTo("InspectionSuccessfullPayment2", { orderNumber: order.OrderNumber, amount: order.TotalAmount, paymentMethod: $scope.SelectedPaymentMethod.Name });

                                            });

                                    });
                                break;
                            }
                            break;
                        case "Voucher":
                            {

                                appService.GetData("/Appointment/ProceedWithCheckout",
                                    { combinedPropertyObjects: combinedPropertyObjects, questions: $scope.Questions, currentUserDetail: sessionService.User.GetCurrentUserRaw() })
                                    .success(function (response) {

                                        var order = response;
                                        appService.NavigateTo("InspectionSuccessfullPayment2", { orderNumber: order.OrderNumber, amount: order.TotalAmount, paymentMethod: $scope.SelectedPaymentMethod.Name });

                                    });
                                break;
                            }
                            break;
                        default:
                    }

                    return null;

                } catch (err) {

                    console.log(err);

                    return null;

                }
            };

            this.init();

        }
    ])
    .controller("InspectionPaymentSuccessfullController",
    [
        "$rootScope", "$scope", "$location", "appService", "sessionService", "order", "transactionReturnMessage",
        function ($rootScope, $scope, $location, appService, sessionService, order, transactionReturnMessage) {

            $scope.CurrentUser = sessionService.User.GetCurrentUser();
            $scope.Products = $scope.CurrentUser.Products;
            $scope.Inspection = $scope.CurrentUser.Inspection;
            $scope.Order = order.data;
            $scope.TransactionReturnMessage = transactionReturnMessage.data;
            $scope.TransactionStatus = null;

            this.init = function () {

                if (_.startsWith($scope.TransactionReturnMessage, "success")) {

                    $scope.TransactionStatus = 1;
                    $scope.ReferenceNumber = _.replace($scope.TransactionReturnMessage, "success", "");

                    var payment = {
                        Name: $scope.Order.OrderNumber,
                        PaymentMethodId: $scope.CurrentUser.PaymentMethodId,
                        PaymentDate: new Date(),
                        Amount: $scope.Order.TotalAmount,
                        Reference: $scope.ReferenceNumber,
                        OrderId: $scope.Order.Id,
                        StateCode: 1,
                        StatusCode: 2

                    };

                    appService.GetData("/Payment/HasPaymentBeenMade",
                        { paymentReference: $scope.ReferenceNumber, orderNumnber: $scope.Order.OrderNumber })
                        .success(function (response) {

                            if (response === true) {
                                return null;
                            }

                            appService.GetData("/Payment/CreatePayment",
                                { payment: payment })
                                .then(function (response) {

                                });

                            return null;

                        });

                    return null;

                }

                return null;
            };

            $scope.Search = function () {

                appService.NavigateTo("InspectionList");

            };

            this.init();

        }
    ])
    .controller("InspectionPaymentSuccessfull2Controller",
    [
        "$rootScope", "$scope", "appService", "sessionService", "bankingDetails", "details",
        function ($rootScope, $scope, appService, sessionService, bankingDetails, details) {

            $scope.CurrentUser = sessionService.User.GetCurrentUser();
            $scope.BankingDetails = bankingDetails.data;
            $scope.Details = details;

            $scope.Search = function () {

                appService.NavigateTo("InspectionList");

            };

        }
    ]);




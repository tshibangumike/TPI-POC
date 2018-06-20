var app = angular.module("InspectionQA_App", []);
app.controller("InspectionQAController", function ($scope, $timeout) {

    var Xrm = window.parent.Xrm;
    var inspectionId = Xrm.Page.data.entity.getId().substring(1, 37);
    var waitFor = 500;

    $scope.categories = [];
    $scope.categoryIdsOfUnAnsweredQuestions = [];
    $scope.questions = [];
    $scope.answers = [];
    $scope.explanations = [];
    $scope.options = [];
    $scope.inspectionPhotos = [];
    $scope.optionPhotos = [];
    $scope.answerPhotos = [];

    $scope.selectedCategory;
    $scope.selectedQuestion;
    $scope.selectedAnswer;
    $scope.selectedExplanation;
    $scope.selectedOption;

    $scope.showCategory = true;
    $scope.showQuestion = false;
    $scope.showAnswer = false;
    $scope.showExplanation = false;
    $scope.showOption = false;

    $scope.thereHasBeenAnUpdateForAnswers = false;
    $scope.thereHasBeenAnUpdateForOptions = false;
    $scope.thereHasBeenAnUpdateForExplanations = false;
    $scope.showPhotos = false;

    /*START*/
    this.init = function () {
        try {
            $scope.showCategories();
        }
        catch (err) {
            console.log(err);
            $scope.stopLoading();
        }
        finally {
            $scope.stopLoading();
        }
    };

    $scope.startLoading = function (text) {
        Xrm.Utility.showProgressIndicator("Working..");
    };
    $scope.stopLoading = function () {
        $timeout(function () {
            Xrm.Utility.closeProgressIndicator();
        }, waitFor);
    };

    $scope.odataQuery = function (query) {
        var results = null;
        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query, false);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    results = (JSON.parse(this.response)).value;
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
        return results;
    };

    $scope.showOneSectionHideOthers = function (sectionName) {
        switch (sectionName) {
            case "category":
                $scope.showCategory = true;
                $scope.showQuestion = false;
                $scope.showAnswer = false;
                $scope.showExplanation = false;
                $scope.showOption = false;
                $scope.showNote = false;
                break
            case "question":
                $scope.showCategory = false;
                $scope.showQuestion = true;
                $scope.showAnswer = false;
                $scope.showExplanation = false;
                $scope.showOption = false;
                $scope.showNote = false;
                break;
            case "answer":
                $scope.showCategory = false;
                $scope.showQuestion = false;
                $scope.showAnswer = true;
                $scope.showExplanation = false;
                $scope.showOption = false;
                $scope.showNote = false;
                break;
            case "explanation":
                $scope.showCategory = false;
                $scope.showQuestion = false;
                $scope.showAnswer = false;
                $scope.showExplanation = true;
                $scope.showOption = false;
                $scope.showNote = false;
                break;
            case "option":
                $scope.showCategory = false;
                $scope.showQuestion = false;
                $scope.showAnswer = false;
                $scope.showExplanation = false;
                $scope.showOption = true;
                $scope.showNote = false;
                break;
            case "note":
                $scope.showCategory = false;
                $scope.showQuestion = false;
                $scope.showAnswer = false;
                $scope.showExplanation = false;
                $scope.showOption = false;
                $scope.showNote = true;
                break;
            default:
        }
    };

    $scope.doesCategoryIdExistInThisCollection = function (categoryId) {
        for (var i = 0; i < $scope.categoryIdsOfUnAnsweredQuestions.length; i++) {
            if ($scope.categoryIdsOfUnAnsweredQuestions[i] == categoryId) return true;
        }
        return false;
    };

    $scope.createNote = function (entity) {
        try {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/annotations",
                data: JSON.stringify(entity),
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var uri = xhr.getResponseHeader("OData-EntityId");
                    var regExp = /\(([^)]+)\)/;
                    var matches = regExp.exec(uri);
                    var newEntityId = matches[1];
                },
                error: function (xhr, textStatus, errorThrown) {
                    Xrm.Utility.alertDialog(textStatus + " " + errorThrown);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "createNote";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };

    $scope.createLog = function (entity) {

        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_logs",
            data: JSON.stringify(entity),
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            async: true,
            success: function (data, textStatus, xhr) {
                var uri = xhr.getResponseHeader("OData-EntityId");
                var regExp = /\(([^)]+)\)/;
                var matches = regExp.exec(uri);
                var newEntityId = matches[1];
            },
            error: function (xhr, textStatus, errorThrown) {
            }
        });

    };

    $scope.openCamera = function (saveToNote) {
        try {
            if (saveToNote == 'true') {
                if (!$scope.isThereAnAnswerForAtLeastOneOption()) {
                    Xrm.Utility.alertDialog("Please select an option first!");
                    return;
                }
                Xrm.Device.captureImage({ allowEdit: false, height: 500, preferFrontCamera: false, quality: 100, width: 650 }).then(function (result) {

                    var entity = {};
                    entity.filename = result.fileName;
                    entity.subject = result.fileName;
                    entity.mimetype = result.mimeType;
                    entity.documentbody = result.fileContent;
                    entity["objectid_blu_inspectionoption@odata.bind"] = "/blu_inspectionoptions(" + $scope.selectedOption.id + ")";

                    $scope.createNote(entity);
                    $scope.showOptions($scope.selectedExplanation);

                }, function () { });
            }
            else {
                Xrm.Device.captureImage().then(function (result) {

                    var entity = {};
                    entity.filename = result.fileName;
                    entity.subject = result.fileName;
                    entity.mimetype = result.mimeType;
                    entity.documentbody = result.fileContent;
                    entity["objectid_blu_inspectiondetail@odata.bind"] = "/blu_inspectiondetails(" + inspectionId + ")";

                    $scope.createNote(entity);

                    thereHasBeenANewPhotoInCategory = true;

                }, function () { });
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "openCamera";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
    };

    $scope.getCategories = function () {
        try {
            var query = "blu_inspectioncategories?$select=blu_inspectioncategoryid,blu_name&$filter=statecode eq 0 and _blu_inspectionid_value eq " + inspectionId + "&$orderby=blu_name asc";
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data;
                    $scope.categories = [];
                    for (var i = 0; i < results.value.length; i++) {
                        $scope.categories.push(
                            {
                                id: results.value[i]["blu_inspectioncategoryid"],
                                name: results.value[i]["blu_name"],
                                allQuestionHasBeenAnswered: !$scope.doesCategoryIdExistInThisCollection(results.value[i]["blu_inspectioncategoryid"])
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getAnswers";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getCategories";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getQuestions = function (category) {
        try {
            var query = "blu_inspectionquestions?$select=blu_answertype,blu_inspectorisrequiredtoanswerthisquestion,blu_name,blu_questionhasbeenanswered,blu_allowmultipleanswers&$filter=_blu_categoryid_value eq " + category.id + " and  statecode eq 0&$orderby=blu_questionorder asc";
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data;
                    $scope.questions = [];
                    for (var i = 0; i < results.value.length; i++) {
                        $scope.questions.push(
                            {
                                answerType: results.value[i]["blu_answertype"],
                                id: results.value[i]["blu_inspectionquestionid"],
                                name: results.value[i]["blu_name"],
                                inspectorIsRequiredToAnswerThisQuestion: results.value[i]["blu_inspectorisrequiredtoanswerthisquestion"],
                                questionHasBeenAnswered: results.value[i]["blu_questionhasbeenanswered"],
                                allowMultipleAnswers: results.value[i]["blu_allowmultipleanswers"]
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getAnswers";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getQuestions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getAnswers = function (question) {
        try {
            var query = "blu_inspectionanswers?$select=blu_explanationrequired,blu_name,blu_supplementarytext,blu_selected&$filter=statecode eq 0 and  _blu_questionid_value eq " + question.id + "&$orderby=blu_name asc";
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data;
                    $scope.answers = [];
                    for (var i = 0; i < results.value.length; i++) {
                        $scope.answers.push(
                            {
                                explanationRequired: results.value[i]["blu_explanationrequired"],
                                id: results.value[i]["blu_inspectionanswerid"],
                                name: results.value[i]["blu_name"],
                                selected: results.value[i]["blu_selected"],
                                supplementaryText: results.value[i]["blu_supplementarytext"],
                                recordNeedToBeUpdated: false
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getAnswers";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getAnswers";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getExplanations = function (answer) {
        try {
            var query = "blu_inspectionexplanations?$select=blu_explanationhasbeengiven,blu_explanationtype,blu_name,blu_answerismandatory&$filter=statecode eq 0 and  _blu_answerid_value eq " + answer.id + "&$orderby=blu_name asc";
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data;
                    $scope.explanations = [];
                    for (var i = 0; i < results.value.length; i++) {
                        $scope.explanations.push(
                            {
                                explanationHasBeenGiven: results.value[i]["blu_explanationhasbeengiven"],
                                id: results.value[i]["blu_inspectionexplanationid"],
                                name: results.value[i]["blu_name"],
                                explanationType: results.value[i]["blu_explanationtype"],
                                answerIsMandatory: results.value[i]["blu_answerismandatory"]
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getExplanations";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getExplanations";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getOptions = function (explanation) {
        try {
            var query = "blu_inspectionoptions?$select=blu_name,blu_selected&$filter=statecode eq 0 and  _blu_explanationid_value eq " + explanation.id + "&$orderby=blu_name asc";
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/" + query,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data;
                    $scope.options = [];
                    for (var i = 0; i < results.value.length; i++) {
                        $scope.options.push(
                            {
                                id: results.value[i]["blu_inspectionoptionid"],
                                name: results.value[i]["blu_name"],
                                selected: results.value[i]["blu_selected"],
                                recordNeedToBeUpdated: false
                            });
                        if (results.value[i]["blu_selected"]) {
                            $scope.selectedOption = {
                                id: results.value[i]["blu_inspectionoptionid"],
                                name: results.value[i]["blu_name"],
                                selected: results.value[i]["blu_selected"],
                                recordNeedToBeUpdated: false
                            };
                        }
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getOptions";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getOptions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getCategoriesOfUnAnsweredQuestions = function () {
        try {

            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectionquestions?fetchXml=%3Cfetch%20version%3D%221.0%22%20output-format%3D%22xml-platform%22%20mapping%3D%22logical%22%20distinct%3D%22false%22%3E%3Centity%20name%3D%22blu_inspectionquestion%22%3E%3Cattribute%20name%3D%22blu_inspectionquestionid%22%20%2F%3E%3Cattribute%20name%3D%22blu_categoryid%22%20%2F%3E%3Cfilter%20type%3D%22and%22%3E%3Ccondition%20attribute%3D%22statecode%22%20operator%3D%22eq%22%20value%3D%220%22%20%2F%3E%3Ccondition%20attribute%3D%22blu_questionhasbeenanswered%22%20operator%3D%22ne%22%20value%3D%221%22%20%2F%3E%3C%2Ffilter%3E%3Clink-entity%20name%3D%22blu_inspectioncategory%22%20from%3D%22blu_inspectioncategoryid%22%20to%3D%22blu_categoryid%22%20link-type%3D%22inner%22%20alias%3D%22ai%22%3E%3Clink-entity%20name%3D%22blu_inspectiondetail%22%20from%3D%22blu_inspectiondetailid%22%20to%3D%22blu_inspectionid%22%20link-type%3D%22inner%22%20alias%3D%22aj%22%3E%3Cfilter%20type%3D%22and%22%3E%3Ccondition%20attribute%3D%22blu_inspectiondetailid%22%20operator%3D%22eq%22%20value%3D%22" + inspectionId + "%22%20%2F%3E%3C%2Ffilter%3E%3C%2Flink-entity%3E%3C%2Flink-entity%3E%3C%2Fentity%3E%3C%2Ffetch%3E",
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data.value;
                    $scope.categoryIdsOfUnAnsweredQuestions = [];
                    for (var i = 0; i < results.length; i++) {
                        $scope.categoryIdsOfUnAnsweredQuestions.indexOf(results[i]["_blu_categoryid_value"]) === -1 ? $scope.categoryIdsOfUnAnsweredQuestions.push(results[i]["_blu_categoryid_value"]) : console.log("This item already exists");
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getCategoriesOfUnAnsweredQuestions";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getCategoriesOfUnAnsweredQuestions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getOptionPhotos = function (explanation) {
        try {
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/annotations?fetchXml=%3Cfetch%20version%3D%221.0%22%20output-format%3D%22xml-platform%22%20mapping%3D%22logical%22%20distinct%3D%22false%22%3E%3Centity%20name%3D%22annotation%22%3E%3Cattribute%20name%3D%22filename%22%20%2F%3E%3Cattribute%20name%3D%22mimetype%22%20%2F%3E%3Cattribute%20name%3D%22documentbody%22%20%2F%3E%3Corder%20attribute%3D%22createdon%22%20descending%3D%22true%22%20%2F%3E%3Clink-entity%20name%3D%22blu_inspectionoption%22%20from%3D%22blu_inspectionoptionid%22%20to%3D%22objectid%22%20link-type%3D%22inner%22%20alias%3D%22az%22%3E%3Cfilter%20type%3D%22and%22%3E%3Ccondition%20attribute%3D%22blu_explanationid%22%20operator%3D%22eq%22%20value%3D%22 " + explanation.id + " %22%20%2F%3E%3C%2Ffilter%3E%3C%2Flink-entity%3E%3C%2Fentity%3E%3C%2Ffetch%3E",
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data.value;
                    $scope.optionPhotos = [];
                    for (var i = 0; i < results.length; i++) {
                        $scope.optionPhotos.push(
                            {
                                image: "data:" + results[i]["mimetype"] + ";base64," + results[i]["documentbody"],
                                name: results[i]["filename"]
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getOptionPhotos";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getOptionPhotos";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getInspectionPhotos = function () {
        try {
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/annotations?$select=documentbody,filename,mimetype&$filter=_objectid_value eq " + inspectionId,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data.value;
                    $scope.inspectionPhotos = [];
                    for (var i = 0; i < results.length; i++) {
                        $scope.inspectionPhotos.push(
                            {
                                image: "data:" + results[i]["mimetype"] + ";base64," + results[i]["documentbody"],
                                name: results[i]["filename"]
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getInspectionPhotos";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getInspectionPhotos";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.getAnswersSupplementaryTexts = function () {
        var array = [];
        for (var i = 0; i < $scope.answers.length; i++) {
            if ($scope.answers[i].supplementaryText == null || $scope.answers[i].supplementaryText == undefined || $scope.answers[i].supplementaryText == "") {
                continue;
            }
            array.push($scope.answers[i]);
        }
        return array;
    };
    $scope.getAnswerPhotos = function () {
        try {
            var filter = "";
            for (var i = 0; i < $scope.answers.length; i++) {
                if (($scope.answers.length - 1) == i) {
                    filter += "_objectid_value eq " + $scope.answers[i].id + "";
                }
                else {
                    filter += "_objectid_value eq " + $scope.answers[i].id + " or ";
                }
            }
            $.ajax({
                type: "GET",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/annotations?$select=documentbody,_objectid_value,mimetype&$filter=(" + filter + ")",
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    XMLHttpRequest.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    var results = data.value;
                    $scope.answerPhotos = [];
                    var photo = null;
                    for (var j = 0; j < $scope.answers.length; j++) {
                        $scope.answers[j].images = [];
                        for (var i = 0; i < results.length; i++) {
                            if (results[i]["_objectid_value"].toLowerCase() == $scope.answers[j].id.toLowerCase()) {
                                $scope.answers[j].images.push("data:" + results[i]["mimetype"] + ";base64," + results[i]["documentbody"]);
                            }
                        }
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "getInspectionPhotos";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getAnswerPhotos";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };

    $scope.showCategories = function () {
        $scope.startLoading();
        $timeout(function () {
            $scope.getCategoriesOfUnAnsweredQuestions();
            $scope.getCategories();
            if ($scope.showPhotos) {
                $scope.getInspectionPhotos();
            }
            $scope.showOneSectionHideOthers("category");
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.showQuestions = function (category) {
        $scope.startLoading();
        $timeout(function () {
            $scope.selectedCategory = category;
            $scope.selectedQuestion = null;
            $scope.getQuestions(category);
            $scope.showOneSectionHideOthers("question");
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.showAnswers = function (question) {
        $scope.startLoading();
        $timeout(function () {
            $scope.selectedQuestion = question;
            $scope.selectedAnswer = null;
            $scope.getAnswers(question);
            $scope.showOneSectionHideOthers("answer");
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.showExplanations = function (answer) {
        $scope.startLoading();
        $timeout(function () {
            $scope.selectedExplanation = null;
            $scope.selectedAnswer = answer;
            if (answer.explanationRequired == null || !answer.explanationRequired) {
                $scope.backToShowingQuestions();
            }
            else {
                $scope.getExplanations(answer);
                $scope.showOneSectionHideOthers("explanation");
            }
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.showOptions = function (explanation) {
        $scope.startLoading();
        $timeout(function () {
            $scope.selectedOption = null;
            $scope.selectedExplanation = explanation;
            $scope.getOptions(explanation);
            $scope.getOptionPhotos(explanation);
            $scope.showOneSectionHideOthers("option");
            $scope.thereHasBeenAnUpdateForOptions = false;
            $scope.updateExplanation($scope.selectedExplanation);
            $scope.stopLoading();
        }, waitFor);
    };

    $scope.selectedAnswer = function (answer) {
        answer.selected = true;
        if ($scope.selectedQuestion.allowMultipleAnswers) {

        }
        else {

        }
    };

    $scope.isThereAnAnswerForAtLeastOneOption = function () {
        try {
            var countOfSelectedOptions = 0;
            for (var i = 0; i < $scope.options.length; i++) {
                if ($scope.options[i].selected) countOfSelectedOptions++;
            }
            return countOfSelectedOptions > 0;
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "isThereAnAnswerForAtLeastOneOption";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.isThereAnAnswerForAtLeastOneQuestion = function () {
        try {
            var countOfSelectedAnswers = 0;
            for (var i = 0; i < $scope.answers.length; i++) {
                if ($scope.answers[i].selected) countOfSelectedAnswers++;
            }
            return countOfSelectedAnswers > 0;
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "isThereAnAnswerForAtLeastOneQuestion";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };

    $scope.backToShowingCategories = function () {
    };
    $scope.backToShowingQuestions = function () {
        $scope.startLoading();
        $timeout(function () {
            if ($scope.selectedQuestion.inspectorIsRequiredToAnswerThisQuestion) {
                if (!$scope.isThereAnAnswerForAtLeastOneQuestion()) {
                    Xrm.Utility.alertDialog("This question requires an answer!");
                    $scope.stopLoading();
                    return;
                } else {
                    $scope.updateAnswers();
                    $scope.updateQuestion($scope.selectedQuestion);
                    $scope.showQuestions($scope.selectedCategory);
                }
            } else {
                if ($scope.isThereAnAnswerForAtLeastOneQuestion()) {
                    $scope.updateAnswers();
                    $scope.updateQuestion($scope.selectedQuestion);
                }
                $scope.showQuestions($scope.selectedCategory);
            }
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.backToShowingAnswers = function () {
    };
    $scope.backToShowingExplanations = function () {
        $scope.startLoading();
        $timeout(function () {
            if ($scope.selectedExplanation.answerIsMandatory) {
                if ($scope.thereHasBeenAnUpdateForOptions) {
                    $scope.updateOptions();
                    $scope.selectedExplanation.explanationHasBeenGiven = $scope.isThereAnAnswerForAtLeastOneOption();
                    $scope.updateExplanation($scope.selectedExplanation);
                    $scope.showExplanations($scope.selectedAnswer);
                } else {
                    $scope.selectedExplanation.explanationHasBeenGiven = $scope.isThereAnAnswerForAtLeastOneOption();
                    if (!$scope.selectedExplanation.explanationHasBeenGiven) {
                        Xrm.Utility.alertDialog("This question requires an answer!");
                        $scope.stopLoading();
                        return;
                    } else {
                        $scope.updateExplanation($scope.selectedExplanation);
                        $scope.showExplanations($scope.selectedAnswer);
                    }
                }
            } else {
                if ($scope.thereHasBeenAnUpdateForOptions) {
                    $scope.updateOptions();
                    $scope.selectedExplanation.explanationHasBeenGiven = $scope.isThereAnAnswerForAtLeastOneOption();
                    $scope.updateExplanation($scope.selectedExplanation);
                    $scope.showExplanations($scope.selectedAnswer);
                }
                $scope.showExplanations($scope.selectedAnswer);
            }
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.showNotes = function () {
        $scope.startLoading();
        $timeout(function () {
            $scope.getAnswerPhotos();
            $scope.showOneSectionHideOthers("note");
            $scope.stopLoading();
        }, waitFor);
    };
    $scope.backToShowingAnswersFromNotes = function () {
        $scope.showOneSectionHideOthers("answer");
    };

    $scope.UpdateAnswerRecordsForInputOfTypeText = function (answer) {
        try {
            for (var i = 0; i < $scope.answers.length; i++) {
                if ($scope.answers[i].id.toUpperCase() == answer.id.toUpperCase()) {
                    $scope.thereHasBeenAnUpdateForAnswers = true;
                    if ($scope.answers[i].name == "" || $scope.answers[i].name == null || $scope.answers[i].name == undefined) {
                        $scope.answers[i].selected = false;
                        $scope.answers[i].recordNeedToBeUpdated = true;
                    } else {
                        $scope.answers[i].selected = true;
                        $scope.answers[i].recordNeedToBeUpdated = true;
                    }
                }
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateAnswerRecordsForInputOfTypeText";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.UpdateAnswerRecordsForInputOfTypeOptionSet = function (answer) {
        try {
            $scope.selectedAnswer = answer;
            if ($scope.selectedQuestion.allowMultipleAnswers) {
                for (var i = 0; i < $scope.answers.length; i++) {
                    $scope.thereHasBeenAnUpdateForAnswers = true;
                    if ($scope.answers[i].id.toUpperCase() == answer.id.toUpperCase()) {
                        $scope.answers[i].selected = true;
                        $scope.answers[i].recordNeedToBeUpdated = true;
                    }
                }
                if (answer.explanationRequired) {
                    $scope.updateAnswers();
                    $scope.showExplanations(answer);
                }
            }
            else {
                for (var i = 0; i < $scope.answers.length; i++) {
                    $scope.thereHasBeenAnUpdateForAnswers = true;
                    if ($scope.answers[i].id.toUpperCase() == answer.id.toUpperCase()) {
                        $scope.answers[i].selected = true;
                        $scope.answers[i].recordNeedToBeUpdated = true;
                    }
                    else {
                        if ($scope.answers[i].selected) {
                            $scope.answers[i].selected = false;
                            $scope.answers[i].recordNeedToBeUpdated = true;
                        }
                    }
                }
                if (answer.explanationRequired) {
                    $scope.updateAnswers();
                    $scope.showExplanations(answer);
                }
                else {
                    $scope.backToShowingQuestions();
                }
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateAnswerRecordsForInputOfTypeOptionSet";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.UpdateAnswerRecordsForInputOfTypeTwoOptions = function (answer) {
        try {
            if (answer.name == "" || answer.name == null || answer.name == undefined) {
                answer.selected = false;
            }
            answer.selected = true;
            $scope.thereHasBeenAnUpdateForOptions = true;
            answer.recordNeedToBeUpdated = true;
            $scope.backToShowingQuestions();
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateAnswerRecordsForInputOfTypeTwoOptions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };

    $scope.UpdateOptionRecordsForInputOfTypeText = function (option) {
        try {
            $scope.selectedOption = option;
            for (var i = 0; i < $scope.options.length; i++) {
                if ($scope.options[i].id.toUpperCase() == option.id.toUpperCase()) {
                    $scope.thereHasBeenAnUpdateForOptions = true;
                    if ($scope.options[i].name == "" || $scope.options[i].name == null || $scope.options[i].name == undefined) {
                        $scope.options[i].selected = false;
                        $scope.options[i].recordNeedToBeUpdated = true;
                    } else {
                        $scope.options[i].selected = true;
                        $scope.options[i].recordNeedToBeUpdated = true;
                    }
                }
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateOptionRecordsForInputOfTypeText";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.UpdateOptionRecordsForInputOfTypeOptionSet = function (option) {
        try {
            $scope.selectedOption = option;
            for (var i = 0; i < $scope.options.length; i++) {
                $scope.thereHasBeenAnUpdateForOptions = true;
                if ($scope.options[i].id.toUpperCase() == option.id.toUpperCase()) {
                    $scope.options[i].selected = true;
                    $scope.options[i].recordNeedToBeUpdated = true;
                }
                else {
                    if ($scope.options[i].selected) {
                        $scope.options[i].selected = false;
                        $scope.options[i].recordNeedToBeUpdated = true;
                    }
                }
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateOptionRecordsForInputOfTypeOptionSet";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.UpdateOptionRecordsForInputOfTypeTwoOptions = function (option) {
        try {
            $scope.selectedOption = option;
            if (option.name == "" || option.name == null || option.name == undefined) {
                option.selected = false;
            }
            option.selected = true;
            $scope.thereHasBeenAnUpdateForOptions = true;
            option.recordNeedToBeUpdated = true;
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "UpdateOptionRecordsForInputOfTypeTwoOptions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };

    $scope.updateQuestion = function (question) {
        try {
            var entity = {};
            entity.blu_questionhasbeenanswered = true;
            $.ajax({
                type: "PATCH",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectionquestions(" + question.id + ")",
                data: JSON.stringify(entity),
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: true,
                success: function (data, textStatus, xhr) {
                    //Success - No Return Data - Do Something
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "updateQuestion";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "updateQuestion";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.updateAnswer = function (answer) {
        try {
            $.ajax({
                type: "PATCH",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectionanswers(" + answer.blu_inspectionanswerid + ")",
                data: JSON.stringify(answer),
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: true,
                success: function (data, textStatus, xhr) {
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "updateAnswer";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "getAnswers";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.updateAnswers = function () {
        try {
            for (var i = 0; i < $scope.answers.length; i++) {
                if (!$scope.answers[i].recordNeedToBeUpdated) continue;
                var entity = {};
                entity.blu_inspectionanswerid = $scope.answers[i].id;
                entity.blu_selected = $scope.answers[i].selected;
                if ($scope.selectedQuestion.answerType != '858890003') {
                    entity.blu_name = $scope.answers[i].name;
                }
                $scope.updateAnswer(entity);
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "updateAnswers";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.updateExplanation = function (explanation) {
        try {
            var entity = {};
            entity.blu_explanationhasbeengiven = $scope.isThereAnAnswerForAtLeastOneOption();
            $.ajax({
                type: "PATCH",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectionexplanations(" + explanation.id + ")",
                data: JSON.stringify(entity),
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                    $scope.startLoading();
                },
                async: false,
                success: function (data, textStatus, xhr) {
                    //Success - No Return Data - Do Something
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "updateExplanation";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "updateExplanation";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    $scope.updateOption = function (option) {
        try {
            $.ajax({
                type: "PATCH",
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectionoptions(" + option.blu_inspectionoptionid + ")",
                data: JSON.stringify(option),
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                    XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                async: true,
                success: function (data, textStatus, xhr) {
                    //Success - No Return Data - Do Something
                },
                error: function (xhr, textStatus, errorThrown) {
                    var entity = {};
                    entity.blu_name = "Inspection App Error";
                    entity.blu_level = 1;
                    entity.blu_functionname = "updateOption";
                    entity.blu_message = xhr.responseText;
                    $scope.createLog(entity);
                }
            });
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "updateOption";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
            //$scope.stopLoading();
        }
    };
    $scope.updateOptions = function () {
        try {
            for (var i = 0; i < $scope.options.length; i++) {
                if (!$scope.options[i].recordNeedToBeUpdated) continue;
                var entity = {};
                entity.blu_inspectionoptionid = $scope.options[i].id;
                entity.blu_name = $scope.options[i].name;
                entity.blu_selected = $scope.options[i].selected;
                $scope.updateOption(entity);
            }
        }
        catch (err) {
            var entity = {};
            entity.blu_name = "Inspection App Error";
            entity.blu_level = 1;
            entity.blu_functionname = "updateOptions";
            entity.blu_message = err;
            $scope.createLog(entity);
        }
        finally {
        }
    };
    /*END*/

    this.init();
});
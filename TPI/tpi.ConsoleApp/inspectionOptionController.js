var app = angular.module("InspectionQA_App", []);
app.controller("InspectionQAController", function ($scope, $timeout) {

    var Xrm = window.parent.Xrm;
    var inspectionOptionId = Xrm.Page.data.entity.getId().substring(1, 37);
    var waitFor = 500;

    $scope.inspectionPhotos = [];

    /*START*/
    this.init = function () {
        try {
            $scope.getInspection();
        }
        catch (err) {
        }
        finally {
        }
    };

    $scope.getInspection = function () {
        $.ajax({
            type: "GET",
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: Xrm.Page.context.getClientUrl() + "/api/data/v8.2/blu_inspectiondetails?fetchXml=%3Cfetch%20version%3D%221.0%22%20output-format%3D%22xml-platform%22%20mapping%3D%22logical%22%20distinct%3D%22true%22%3E%3Centity%20name%3D%22blu_inspectiondetail%22%3E%3Cattribute%20name%3D%22blu_inspectiondetailid%22%20%2F%3E%3Clink-entity%20name%3D%22blu_inspectioncategory%22%20from%3D%22blu_inspectionid%22%20to%3D%22blu_inspectiondetailid%22%20link-type%3D%22inner%22%20alias%3D%22ay%22%3E%3Clink-entity%20name%3D%22blu_inspectionquestion%22%20from%3D%22blu_categoryid%22%20to%3D%22blu_inspectioncategoryid%22%20link-type%3D%22inner%22%20alias%3D%22az%22%3E%3Clink-entity%20name%3D%22blu_inspectionanswer%22%20from%3D%22blu_questionid%22%20to%3D%22blu_inspectionquestionid%22%20link-type%3D%22inner%22%20alias%3D%22ba%22%3E%3Clink-entity%20name%3D%22blu_inspectionexplanation%22%20from%3D%22blu_answerid%22%20to%3D%22blu_inspectionanswerid%22%20link-type%3D%22inner%22%20alias%3D%22bb%22%3E%3Clink-entity%20name%3D%22blu_inspectionoption%22%20from%3D%22blu_explanationid%22%20to%3D%22blu_inspectionexplanationid%22%20link-type%3D%22inner%22%20alias%3D%22bc%22%3E%3Cfilter%20type%3D%22and%22%3E%3Ccondition%20attribute%3D%22blu_inspectionoptionid%22%20operator%3D%22eq%22%20value%3D%22" + inspectionOptionId + "%22%20%2F%3E%3C%2Ffilter%3E%3C%2Flink-entity%3E%3C%2Flink-entity%3E%3C%2Flink-entity%3E%3C%2Flink-entity%3E%3C%2Flink-entity%3E%3C%2Fentity%3E%3C%2Ffetch%3E",
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
                XMLHttpRequest.setRequestHeader("OData-Version", "4.0");
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            async: false,
            success: function (data, textStatus, xhr) {
                var results = data.value;
                var inspectionId = results[0].blu_inspectiondetailid;
                $scope.getInspectionPhotos(inspectionId);
            },
            error: function (xhr, textStatus, errorThrown) {
                Xrm.Utility.alertDialog(textStatus + " " + errorThrown);
            }
        });
    };

    $scope.getInspectionPhotos = function (inspectionId) {
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
                                name: results[i]["filename"],
                                mimeType: results[i]["mimetype"],
                                documentBody: results[i]["documentbody"]
                            });
                    }
                },
                error: function (xhr, textStatus, errorThrown) {
                }
            });
        }
        catch (err) {
        }
        finally {
        }
    };

    $scope.attachSelected = function () {
        for (var i = 0; i < $scope.inspectionPhotos.length; i++) {
            if (!$scope.inspectionPhotos[i].toggle) continue;
            var entity = {};
            entity.filename = $scope.inspectionPhotos[i].name;
            entity.subject = $scope.inspectionPhotos[i].name;
            entity.mimetype = $scope.inspectionPhotos[i].mimeType;
            entity.documentbody = $scope.inspectionPhotos[i].documentBody;
            entity["objectid_blu_inspectionoption@odata.bind"] = "/blu_inspectionoptions(" + inspectionOptionId + ")";
            $scope.createNote(entity);
        }
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
                async: true,
                success: function (data, textStatus, xhr) {
                },
                error: function (xhr, textStatus, errorThrown) {
                    Xrm.Utility.alertDialog(textStatus + " " + errorThrown);
                }
            });
        }
        catch (err) {
        }
        finally {
        }
    };

    /*END*/

    this.init();
});

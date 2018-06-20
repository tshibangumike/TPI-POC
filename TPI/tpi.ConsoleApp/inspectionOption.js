
var filesToUpload = [];
var noOfFilesUploaded = 0;
var title = "";
var description = "";
var recordId = "";
var entityTypecode = "";
var entityCollectionName = "";
var logicalName = "";

_Get = function (controlName) {
    var element = document.getElementById(controlName);
    if (element !== undefined)
        return element;

    return null;
};

_GetValue = function (controlName) {
    var element = _Get(controlName);
    if (element !== null) {
        return element.value;
    }

    return null;
};

GetQueryParameterByName = function (name, url) {
    if (!url) {
        url = window.location.href;
    }
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
};

SetMessage = function (controlName, value) {
    var element = _Get(controlName);
    if (element !== null) {
        element.innerText = value;

    }
};

StopFileLoading = function (e) {
    e.stopPropagation();
    e.preventDefault();
};

ArrayBufferToBase64 = function (buffer) { // Convert Array Buffer to Base 64 string
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
};

AttachEvents = function () {
    var fileSel = _Get("fileCtrl");
    var uploadBtn = _Get("uploadBtn");
    if (fileSel !== null) {
        fileSel.addEventListener("change", FileSelected, false);
    }
    if (uploadBtn !== null) {
        uploadBtn.addEventListener("click", UploadFiles, false);
    }
};

UploadIndividualFile = function (file) {
    var fileReader = new FileReader();

    fileReader.onloadend = function (e) {
        var content = e.target.result;
        console.log(file.name + "--" + content.byteLength);
        UploadFileToCrm(content, file);
    };
    fileReader.readAsArrayBuffer(file);
};


function DocumentLoaded() {

    AttachEvents();
};

function FileDragLeave(e) {
    this.classList.remove("dragover");
    StopFileLoading(e);
};

function FileDrag(e) {
    this.classList.add("dragover");
    StopFileLoading(e);
};

function FileSelected(e) {

    this.classList.remove("dragover");
    StopFileLoading(e);

    var files = e.target.files || e.dataTransfer.files;

    for (var i = 0; i < files.length; i++) {
        filesToUpload.push(files[i]);
    }

    if (filesToUpload.length === 0) {
        return;
    }

    UploadFiles();

};

function UploadFiles() {
    var filesCount = filesToUpload.length;
    if (filesCount <= 0) {
        alert("No Files to upload !! Please Drag & Drop files or use Browse button to select files.");
        return;
    }

    for (var i = 0; i < filesCount; i++) {
        UploadIndividualFile(filesToUpload[i]);
    }

};

function UploadFileToCrm(filecontent, file,) {

    var content = ArrayBufferToBase64(filecontent);

    var data = {};
    data.filename = file.name;
    data.documentbody = content;
    data.mimetype = file.type;
    data.subject = file.name;
    data["objectid_blu_inspectionoption@odata.bind"] = "/blu_inspectionoptions(" + window.parent.Xrm.Page.data.entity.getId().substring(1, 37) + ")";

    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        datatype: "json",
        url: window.parent.Xrm.Page.context.getClientUrl() + "/api/data/v8.2/annotations",
        data: JSON.stringify(data),
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

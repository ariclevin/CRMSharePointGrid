
var AzureApi = (function () {

    function AzureApi() {
    }

    var apiPath = "";
    var fileUploadUrl = "";
    var returnData = "";
    var resultType = 0;

    AzureApi.getResultType = function ()
    {
        return resultType;
    }

    AzureApi.getResultData = function ()
    {
        return returnData;
    }

    AzureApi.retrieveSharePointFiles = function (url, queryString) {

        var urlPath = url;
        urlPath += queryString;
        $.ajax({
            url: urlPath,
            type: "GET",
            dataType: "json",
            async: false,
            crossDomain: true,
            success: function (data, textStatus, xhr) {
                var jsData = data,
                    jsonStr = JSHelper.toJson(jsData);
                jsData = JSHelper.fromJson(jsonStr);
                resultType = 1;
                returnData = jsData;
            },
            error: function () {
                resultType = 2;
            }
        })
        .done(function (data, status, jqxhr) {
            
        });
    }

    AzureApi.setFileAttributes = function (url, data) {
        var rc = false;
        $.ajax({
            url: url,
            type: "POST",
            data: data,
            async: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            },
            success: function (data, textStatus, xhr) {
                rc = true;
            },
            error:
            rc = false,
        })
            .done(function (data, status, jqxhr) {

            });
    }

    AzureApi.buildFileUploadUrl = function (entityName, masterId, masterNumber, masterName)
    {
        var uploadUrl = AzureApi.buildAPIPath(AzureApi.getRoute('UploadDocument'));

        uploadUrl = uploadUrl.replace("{0}", entityName);
        uploadUrl = uploadUrl.replace("{1}", masterId);
        uploadUrl = uploadUrl.replace("{2}", masterNumber);
        uploadUrl = uploadUrl.replace("{3}", masterName);
        fileUploadUrl = uploadUrl;
        return fileUploadUrl;
    };

    AzureApi.uploadFile = function (formData) {

        $.ajax({
            url: fileUploadUrl,
            type: "POST",
            contentType: false,
            processData: false,
            data: formData,
            async: false,
            beforeSend: function (xhr) {
                // disableButton(fileUploadButtonId, fileCancelButtonId);
            },
            success: function (data, textStatus, xhr) {
                var key = data.Key;
                if (key == "FileId") {
                    resultType = 1;
                    returnData = data.Value; // fileId
                }
                else
                    resultType = 2;
            },
            error: function (jqxhr, status, error) {
                resultType = 2;
                returnData = error;
            }

        });
    }

    AzureApi.retrieveCategories = function () {

        var urlPath = AzureApi.buildAPIPath(AzureApi.getRoute('GetDocumentTypes'));

        $.ajax({
            url: urlPath,
            type: "GET",
            dataType: "json",
            async: false,
            crossDomain: true,
            success: function (data, textStatus, xhr) {
                var jsData = data,
                    jsonStr = JSHelper.toJson(jsData);
                jsData = JSHelper.fromJson(jsonStr);
                resultType = 1;
                returnData = jsData;
            },
            error: function () {
                resultType = 2;
            }
        })
            .done(function (data, status, jqxhr) {
            });
    }

    AzureApi.deleteFile = function (fileId) {

        var urlPath = AzureApi.buildAPIPath(AzureApi.getRoute('DeleteFile') + fileId);

        $.ajax({
            url: urlPath,
            type: "DELETE",
            crossDomain: true,
            success: function (data, textStatus, xhr) {
                resultType = 1;
                returnData = "File Deleted";
            },
            error: function () {
                resultType = 2;
                returnData = "File could not be Deleted";
            }
        });
    }

    AzureApi.buildAPIPath = function (path) {
        return apiPath + path;
    }

    AzureApi.getAzureApi = function () {
        return apiPath;
    }

    AzureApi.setAzureApi = function (path) {
        apiPath = path;
    }

    AzureApi.getRoute = function (name) {

        var route = '';
        switch (name) {
            case 'GetDocumentTypes':
                route = "/api/Lookup/GetAllListItems/DocumentTypes";
                break;
            case 'DownloadFile':
                route = "/api/Library/DownloadFile/";
                break;
            case 'GetByMasterId':
                route = "/api/Library/GetByMasterId/";
                break;
            case 'GetByMasterNumber':
                route = "/api/Library/GetByMasterNumber/";
                break;
            case 'GetByAlternateField':
                route = "/api/Library/GetByAlternateField?";
                break;
            case 'UploadDocument':
                route = "/api/Library/UploadDocument?EntityName={0}&MasterId={1}&MasterNumber={2}&MasterName={3}";
                break;
            case 'SetFileAttributes':
                route = "/api/Library/SetFileAttributes";
                break;
            case 'DeleteFile':
                route = "/api/Library/DeleteFile/"
                break;
            default:
                break;
        }
        return route;
    }

    return AzureApi;
})();

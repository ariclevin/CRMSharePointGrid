var JSHelper = (function () {

    function JSHelper() {
    }

    JSHelper.toJson = function (obj, prettyPrint) {
        var spacingStr = prettyPrint ? "\t" : "";
        return JSON.stringify(obj, null, spacingStr);
    };

    JSHelper.fromJson = function (str) {
        var jsData = $.parseJSON(str);
        return jsData;
    }

    JSHelper.isEmptyString = function (value) {
        return (value == null || value === "");
    }

    JSHelper.toAPIUrl = function (api, debugMode)
    {
        var apiUrl = "";
        if (debugMode)
        {
            apiUrl = "http://localhost:49481" + api;
        }
        else
        {
            apiUrl = "https://pbcspdev5.azurewebsites.net" + api;
        }
        return apiUrl;
    }

    return JSHelper;
})();
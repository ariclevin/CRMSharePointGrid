var UTF8 = (function () {

    function UTF8() {
    }

    UTF8.prototype.getBytes = function (stringValue) {

        var bytes = [];

        for (var i = 0; i < stringValue.length; ++i) {
            bytes.push(stringValue.charCodeAt(i));
            // bytes.push(0);
        }

        return bytes;
    };

    UTF8.prototype.getString = function (utftext) {
        var result = "";
        for (var i = 0; i < utftext.length; i++) {
            result += String.fromCharCode(parseInt(utftext[i], 10));
        }
        return result;
    };

    UTF8.prototype.toBase64 = function (buffer) {
        var binary = '';
        var bytes = new Uint8Array(buffer);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    };

    return UTF8;

})();

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
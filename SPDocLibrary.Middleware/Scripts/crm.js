// ----------------------------------------------------------------------------------------------------

$(document).ready(function () {

    function toJson(obj, prettyPrint) {
        var spacingStr = prettyPrint ? "\t" : "";
        return JSON.stringify(obj, null, spacingStr);
    }

    function fromJson(str) {
        var jsData = $.parseJSON(str);
        return jsData;
    }

    function retrieveUserInfo()
    {
        var clientUrl = Xrm.Page.context.getClientUrl();
        var userId = Xrm.Page.context.getUserId();
    }

    function retrieveSharepointFilesCode(msa_cust_id) {

        var urlPath = CUST_DOCS_API_RETRIEVE_URL;
        urlPath += msa_cust_id;

        //var keyValParams = { custId: customerId };

        $.ajax({
            url: urlPath,
            type: "GET",
            dataType: "json",
            //data: keyValParams,
            crossDomain: true,
            success: function (data, textStatus, xhr) {
                //alert("success");
                var jsData = data,
                    jsonStr = toJson(jsData);

                // The server returned data (json string converted to plain js objects) serialized back here as a json string.
                /*DEBUG*/ //alert(jsonStr);

                // De-serialize the json string back into its plain js object equivalent
                jsData = fromJson(jsonStr);
                //alert(jsData.customersList[0].custName);
            },
            error: function (jqxhr, status, error) {
                var msg = "",
                    rawStatus = (jqxhr.status + " - " + jqxhr.statusText),
                    responseText = jqxhr.responseText;

                msg += "\nStatus : " + status + "\nError : " + error;
                msg += "\nRaw : " + rawStatus;
                msg += "\nResponse : " + responseText;

                alert("error");
                alert(msg);

            }

        })
        .done(function (data, status, jqxhr) {
            var rows = [],
                item;

            for (var i = 0; i < data.length; i++) {
                item = data[i];
                rows.push({
                    msaId: item.MsaId,
                    fileId: item.FileId,
                    fileName: item.FileName,
                    filePath: item.FilePath,
                    custIdType: item.CustomerIdType.Title,
                    custIdVal: item.CustomerIdValue,
                    busEntity: item.BusinessEntity.Title,
                    geoRegion: item.GeoRegion.Title,
                    geoLocation: item.GeoLocation
                });
            }

            loadGrid(rows);
        });




    }




});

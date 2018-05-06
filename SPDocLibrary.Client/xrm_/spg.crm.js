/// <reference path="spg.helper.js" />

var SP_CRMData;
SP_CRMData = (function() {

    var Xrm = parent.Xrm;
    var CRMData = {

        MasterId: "",
        MasterNumber: "",
        MasterName: "",
        EmailAddress: "",
        FullName: ""
    };

    var EncodedServiceAccount = "";
    var SharePointAzureAPIUrl = "";
    var EntityName = "";

    function loadCRMData() {

        EntityName = Xrm.Page.data.entity.getEntityName();

        var userId = removeBraces(Xrm.Page.context.getUserId());

        var masterId = removeBraces(Xrm.Page.data.entity.getId());
        var masterNumber = Xrm.Page.getAttribute("accountnumber").getValue();
        var masterName = Xrm.Page.getAttribute("name").getValue();

        CRMData.MasterId = masterId;
        CRMData.MasterNumber = masterNumber;
        CRMData.MasterName = masterName;

        retrieveUserInfo(userId);

        retrieveConfigurationSetting('SharePointCredential');
        retrieveConfigurationSetting('SharePointAPI');

        retrieveGridConfiguration("Account");
    }

    function getCRMData()
    {
        return CRMData;
    }

    function showCRMData()
    {
        var rc = "Master Id: " + CRMData.MasterId + '\n';
        rc += "Master Number: " + CRMData.MasterNumber + '\n';
        rc += "Master Name: " + CRMData.MasterName + '\n';
        rc += "Email Address: " + CRMData.EmailAddress + '\n';
        alert(rc);
    }


    function retrieveUserInfo(userId) {
        var options = "$select=domainname,fullname";
        Xrm.WebApi.retrieveRecord("systemuser", userId, options).then(
            function success(result) {
                CRMData.EmailAddress = result.domainname;
                CRMData.FullName = result.fullname;
            },
            function (error) {
                console.log(error);
            });
    }

    function retrieveConfigurationSetting(key) {
        var options = "$select=xrm_plaintextvalue,xrm_encryptedvalue,xrm_securitymethodcode&$filter=xrm_key eq '" + key + "'";
        Xrm.WebApi.retrieveMultipleRecords("xrm_configurationsetting", options).then(
            function success(result) {
                if (result.entities.length > 0) {
                    var entity = result.entities[0];
                    var securityMode = entity.xrm_securitymethodcode;
                    var rc = '';
                    if (securityMode == 1) {
                        // Plain Text
                        rc = entity.xrm_plaintextvalue;
                    }
                    else {
                        var encryptedValue = entity.xrm_encryptedvalue;
                        // rc = decryptEncryptedValue(encryptedValue);
                        
                    }
                    evaluateConfigurationSetting(key, rc);
                }
                
            },
            function (error) {
                console.log(error);
            });
    }

    function evaluateConfigurationSetting(key, value) {
        switch (key) {
            case "SharePointCredential":
                var SERVICE_ACCOUNT = value;
                var userPass = UTF8.prototype.getBytes(SERVICE_ACCOUNT);
                EncodedServiceAccount = UTF8.prototype.toBase64(userPass);
                break;
            case "SharePointAPI":
                SharePointAzureAPIUrl = value;
                break;
            default:
                break;
        }
    }

    function retrieveServiceAccount()
    {
        return EncodedServiceAccount;
    }

    function retrieveSharePointAzureAPIUrl()
    {
        return SharePointAzureAPIUrl;
    }

    function retrieveEntityName() {
        return EntityName;
    }

    function retrieveGridConfiguration(gridName)
    {
        var configurationId;

        var options = "$select=xrm_gridconfigurationid,xrm_name&$filter=xrm_name eq '" + gridName + "'";
        Xrm.WebApi.retrieveMultipleRecords("xrm_gridconfiguration", options).then(
            function success(result) {
                if (result.entities.length > 0) {
                    var entity = result.entities[0];
                    var configurationId = entity.xrm_gridconfigurationid;
                    retrieveGridColumns(configurationId);
                }
            },
            function (error) {
                alert("There was an error retrieving the Grid Configuration");
            });
    }

    var columnsArray = Array();
    function retrieveGridColumns(gridConfigurationId)
    {
        // var gridConfigurationId = "fb7093fd-f9c1-e611-80f4-5065f38a19e1";
        var options = "$select=xrm_name,xrm_label,xrm_datafield,xrm_columnwidth,xrm_hidden,xrm_pinned,xrm_cellformat,xrm_cellsrenderer";
        options += "&$filter=_xrm_gridconfigurationid_value%20eq%20" + gridConfigurationId + "&$orderby=xrm_orderno%20asc";

        Xrm.WebApi.retrieveMultipleRecords("xrm_gridcolumn", options).then(
            function success(result) {
                if (result.entities.length > 0) {
                    retrieveGridColumnsCallback(result);
                }
            },
            function (error) {
                alert("There was an error retrieving the Grid Configuration");
            });

    }

    function retrieveGridColumnsCallback(results)
    {
        for(var i = 0; i < results.entities.length; i++)
        {
            var row = {};
            var gridColumn = results.entities[i];
            row["text"] = gridColumn.xrm_label;
            row["datafield"] = gridColumn.xrm_datafield;
            row["width"] = gridColumn.xrm_columnwidth;
            row["hidden"] = gridColumn.xrm_hidden;
            row["pinned"] = gridColumn.xrm_pinned;
            if (!JSHelper.isEmptyString(gridColumn.xrm_cellformat))
                row["cellformat"] = gridColumn.xrm_cellformat;

            if (!JSHelper.isEmptyString(gridColumn.xrm_cellsrenderer))
                row["cellsrenderer_name"] = gridColumn.xrm_cellsrenderer;

            columnsArray[i] = row;
        }

        //retrieveGridColumnsComplete();
    }

    function retrieveGridColumnsComplete()
    {
        // alert(JSHelper.toJson(columnsArray,false));
        var fileNameColumn = columnsArray[1];
        fileNameColumn.cellsrenderer = function(rowIdx, columnField, value, defaultHtml, columnProps, rowData) {
            var module = SP_Grid,
                fn     = module[fileNameColumn.cellsrenderer_name];

            //return fn(rowIdx, columnField, value, defaultHtml, columnProps, rowData);
        };
    }

    function retrieveGridColumnsArray()
    {
        // return JSHelper.toJson(columnsArray, false);
        return columnsArray;
    }

    function getLookupValue(attributeName) {
        var lookupControl = parent.Xrm.Page.getAttribute(attributeName);
        if (lookupControl != null) {
            var lookup = lookupControl.getValue();
            if (lookup != null) {
                return lookup[0];
            }
            else
                return null;
        }
    }

    function removeBraces(str) {
        str = str.replace(/[{}]/g, "");
        return str;
    }

    function isMobile()
    {
        var isCrmForMobile = (Xrm.Page.context.client.getClient() == "Mobile")
        return isCrmForMobile;
    }

    // public methods
    return {
        loadCRMData: loadCRMData,
        getCRMData: getCRMData,
        showCRMData: showCRMData,
        isCrmMobile: isMobile,
        getServiceAccount: retrieveServiceAccount,
        getAzureAPI: retrieveSharePointAzureAPIUrl,
        getEntityName: retrieveEntityName,
        getGridColumns: retrieveGridColumnsArray
    };

})();
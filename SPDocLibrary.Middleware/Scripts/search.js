$(document).ready(function()
{
    var debugMode = false;
    "use strict";

    initializeCustomerGrid();
    applyCustomerFilters();

    function initializeCustomerGrid()
    {
        $("#customer-list-grid").jqxGrid({
        width: '100%',
        sortable: true,
        pageable: true,
        columnsheight: 35,
        rowsheight: 25,
        localization: {
            emptydatastring: "No documents on this criteria"
        },
        theme: 'metro',
        height: 600,
        // autoheight: true,
        pagesize: 20,
        pagermode: "simple",
        ready: function ()
        {
            if ($(".grids-popover").length > 0) {
                $("#popover").jqxPopover({theme: 'metro', position: "right", offset: { left: -230, top: 0 }, title: "Document", showCloseButton: false, selector: ".grids-popover" });
                attachPopover();
            }
        },
        columns: [
            {  text: 'Id', datafield: 'FileId', hidden: true, width: 0 },
            {
                text: 'File', datafield: 'FileName', width: '25%', 
                cellsrenderer: function (rowIdx, columnField, value, defaultHtml, columnProps, rowData) {
                    var $elem = $(defaultHtml);
                    var filePath = JSHelper.toAPIUrl("/api/CustomerDocuments/DownloadFile/" + rowData.Id);
                    $elem.wrapInner('<a target="_blank" href="' + filePath + '" />');
                    return $elem[0].outerHTML;
                }
            },
            { text: 'MSA', datafield: 'MsaId', width: '10%' },
            { text: 'Doc Type', datafield: 'CustomerIdType', width: '14%' },
            { text: 'Doc Id', datafield: 'CustomerIdValue', width: '17%' },
            { text: 'Category', datafield: 'DocumentCategory', width: '10%' },
            { text: 'Subcategory', datafield: 'DocumentSubcategory', width: '13%' },
            { text: 'Geography', datafield: 'GeoLeaf', width: '13%' },
            { text: 'Created By', datafield: 'CreatedByName', width: '10%' },
            { text: 'Created On', datafield: 'CreatedOn', width: '9%', cellsformat: 'd' },
            /**/
        ]
    });
    }

    function commonErrorHandler(jqxhr, status, error) {

    var msg          = "",
        rawStatus    = (jqxhr.status + " - " + jqxhr.statusText),
        responseText = jqxhr.responseText;

    msg = "Request Failed:";
    msg += "\nStatus : " + status + "\nError : " + error;
    msg += "\nRaw : " + rawStatus;
    msg += "\nResponse : " + responseText;

    alert(msg);

    }

    function isInArray(value, array)
    {
        return array.indexOf(value) > -1;
    }

    $("#filter-filter-button").on('click', function ()
    {
        if (currentView == SearchViews.MASTER) {
            $('#master-list-grid').jqxGrid('clearselection');
            applyMasterFilters();
        }
        else if (currentView == SearchViews.CUSTOMER)
        {
            $('#customer-list-grid').jqxGrid('clearselection');
            applyCustomerFilters();
        }
    });

    function applyCustomerFilters()
    {
        var qs = buildCustomerQueryString();

        $.ajax({
            url: JSHelper.toAPIUrl("/api/CustomerDocuments/Search?" + qs, debugMode),
            type: "GET",
    //            headers: {
    //                authorization: "Basic " + SP_CRMData.getServiceAccount()
            //            },
            dataType: "json",
            crossDomain: true,
            error: commonErrorHandler
        })
        .done(function (data, status, jqxhr) {
            loadData(data);
        });

        function loadData(data) {

            loadGrid(buildGridRows(data));

//             var sourceDocumentList = new $.jqx.dataAdapter(data);
//             $("#document-list-grid").jqxGrid({ source: sourceDocumentList });
        }

            function loadGrid(data) {

        // Obtain the grid's data source and set the data at same time ...
        var source = {
            localdata: data,
            datatype: "json",
            datafields: [
                { name: "FileId",         type: "string" },
                { name: "FileName",       type: "string" },
                { name: "MsaId",     type: "string" },
                { name: "CustomerIdType",      type: "string" },
                { name: "CustomerIdValue", type: "string" },
                { name: "DocumentCategory", type: "string" },
                { name: "DocumentSubcategory",  type: "string" },
                { name: "GeoLeaf",  type: "string" },
                { name: "CreatedByName", type: "string" },
                { name: "CreatedOn",     type: "date" }

            ],
            // The data field specified here is used in generating the row's unique id property value (ie. row.uid)
            id: "FileId"
        };

        // Create the grid's data source adapter from the above source.
        var dataAdapter = new $.jqx.dataAdapter(source);

        // To refresh the Grid, you need to simply set its 'source' property again.
        $("#customer-list-grid").jqxGrid({ source: source });

    }

        function buildGridRows(data) {
            var rows = [],
                item;

            for (var i = 0; i < data.length; i++) {
                item = data[i];
                rows.push({
                    FileId: item.FileId,
                    FileName: item.FileName,
                    MsaId: item.MsaId,
                    CustomerIdType: item.CustomerIdType.Title,
                    CustomerIdValue: item.CustomerIdValue,
                    DocumentCategory: item.DocumentCategory.Title,
                    DocumentSubcategory: item.DocumentSubcategory.Title, 
                    GeoLeaf: item.GeoLeaf,
                    CreatedByName: item.CreatedByName,
                    CreatedOn: item.CreatedOn
                });
            }

            return rows;
        }
    }

    function buildCustomerQueryString()
    {
        var qs = "";

        /*
        var identifierType = $("input#IdentifierType").val();
        var identifierValue = $("input#IdentifierValue").val();

        if (identifierType == "MSA")
        {
            qs += "MSA_ID=" + identifierValue.toString();
        }
        else if (identifierType == "PCC")
        {
             qs += "CUSTOMER_ID_TYPE=PCC&CUSTOMER_ID_VALUE=" + identifierValue.toString();
        }
        */
        
        var pageQueryString = getPageQueryString();
        var msaId = pageQueryString["MSA_ID"];
        var pccId = pageQueryString["PCC_ID"];

        if (!JSHelper.isEmptyString(msaId))
        {
            qs += "&MSA_ID=" + msaId.toString();
        }
        else if (!JSHelper.isEmptyString(pccId))
        {
            qs += "&CUSTOMER_ID_TYPE=PCC&CUSTOMER_ID_VALUE=" + pccId.toString();
        }
        

        return qs

    }

    function getPageQueryString()
    {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for(var i = 0; i < hashes.length; i++)
        {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }

    function getSelectedItem(controlId)
    {
        var item = $(controlId).jqxComboBox('getSelectedItem');
        if (item) {
            return item.value;
        } else {
            return "";
        }
    }

    function getSelectedLabel(controlId)
    {
        var item = $(controlId).jqxComboBox('getSelectedItem');
        if (item) {
            return item.label;
        } else {
            return "";
        }
    }

    function populateDropDown(controlId, apiMessage, apiMessageId)
    {
        var apiUrl = "";
        if (JSHelper.isEmptyString(apiMessageId))
        {
            apiUrl = "/api/Lookup/" + apiMessage;
        }
        else
        {
            apiUrl = "/api/Lookup/" + apiMessage + "/" + apiMessageId;
        }

        $.ajax({
            url: JSHelper.toAPIUrl(apiUrl, debugMode),
            type: "GET",
    //            headers: {
    //                authorization: "Basic " + SP_CRMData.getServiceAccount()
    //            },
            dataType: "json",
            datafields: [{ name: "ItemId"}, { name: "Title"}],
            crossDomain: true,
            error: commonErrorHandler
        })
        .done(function (data, status, jqxhr) {
            loadData(data);
        });

        function loadData(data) {
            $(controlId).jqxComboBox({ source: data, displayMember: "Title", valueMember: "ItemId" });
        }

    }

    function populateBusinessDivisionCategories(controlId, businessDivisionCode)
    {
        var url = "";
        if (!JSHelper.isEmptyString(businessDivisionCode))
            url = "/api/Lookup/GetBusinessDivisionCategories/" + businessDivisionCode.toString();
        else
            url = "/api/Lookup/GetAllBusinessDivisionCategories";

        $.ajax({
            url: JSHelper.toAPIUrl(url, debugMode),
            type: "GET",
    //            headers: {
    //                authorization: "Basic " + SP_CRMData.getServiceAccount()
    //            },
            dataType: "json",
            datafields: [{ name: "ItemId"}, { name: "Title"}],
            crossDomain: true,
            error: commonErrorHandler
        })
        .done(function (data, status, jqxhr) {
            loadData(data);
        });

        function loadData(data) {
            $(controlId).jqxComboBox({ source: data, displayMember: "Title", valueMember: "ItemId" });
            // $("#filter-divisioncategory-combobox").jqxComboBox({ source: data, displayMember: "Title", valueMember: "ItemId" });
        }
    }

    function populateCustomerAccountType(controlId, includeMsa)
    {
    $.ajax({
        url: JSHelper.toAPIUrl("/api/Lookup/GetAllListItems/CustomerIdType", debugMode),
        type: "GET",
//            headers: {
//                authorization: "Basic " + SP_CRMData.getServiceAccount()
//            },
        dataType: "json",
        datafields: [{ name: "ItemId"}, { name: "Title"}],
        crossDomain: true,
        error: commonErrorHandler
    })
    .done(function (data, status, jqxhr) {
        loadData(data);
    });

    function loadData(data) {
        // Add 0/MSA to Data
        if (includeMsa)
        {
            var item = { "ItemId": "0", "Title": "MSA" };
            data.push(item);
        }
        $(controlId).jqxComboBox({ source: data, displayMember: "Title", valueMember: "ItemId" });
    }

    }

    function getItemId(event)
    {
        var emptyArgs = {};
        var args = event.args || emptyArgs;
        var item = args.item || emptyArgs;
        var itemId = item.value || "";
        var itemTitle = item.label || "";

        return itemId;
    }

    function showMasterDesign()
    {
        // $("#filter-divisioncategory-combobox").show();
        // $("#filter-region-combobox").show();

        $("#filter-keyaccounttype-combobox").show();
        $("#filter-keyaccountid-inputbox").show();

        $("#filter-documentcategory-combobox").hide();
        $("#filter-documentsubcategory-combobox").hide();
        $("#filter-customertype-combobox").hide();
        $("#filter-customerid-inputbox").hide();

        $("#customer-list-grid").hide();
        $("#master-list-grid").show();

        // populateMasterAccountType();
        populateDropDown("#filter-keyaccounttype-combobox", "GetAllListItems", "MasterAccountType");
    }

    function showCustomerDesign()
    {
        $("#filter-keyaccounttype-combobox").hide();
        $("#filter-keyaccountid-inputbox").hide();

        $("#filter-documentcategory-combobox").show();
        $("#filter-documentsubcategory-combobox").show();
        $("#filter-customertype-combobox").show();
        $("#filter-customerid-inputbox").show();

        $("#master-list-grid").hide();
        $("#customer-list-grid").show();

        var businessDivisionCategory = getSelectedItem("#filter-divisioncategory-combobox");
        if (!JSHelper.isEmptyString(businessDivisionCategory))
            populateDropDown("#filter-documentcategory-combobox", "GetCategories", businessDivisionCategory);
        
        // populateDocumentCategories(businessDivisionCategory);
        populateCustomerAccountType("#filter-customertype-combobox", true);
    }

    function uploadCustomer()
    {
            var formData = new FormData();
            var file = $("#customerFileToUpload").prop("files")[0];
            formData.append("file", file);

            if (window.FormData !== undefined) {

                $.ajax({
                    url: getUploadUrl(),
                    type: "POST",
//                        headers: {
//                            authorization: "Basic " + SP_CRMData.getServiceAccount()
//                        },
                    contentType: false,
                    processData: false,
                    data: formData,
                    beforeSend: function (xhr) {
                        disableButton(fileUploadButtonId, fileCancelButtonId);
                    },
                    success: function (data, textStatus, xhr) {
                        var fileId = data.Value;
                        //alert("success");
                        setFileAttributes(fileId);
                    },
                    error: function (jqxhr, status, error) {
                        commonErrorHandler(jqxhr, status, error);
                        // TODO: Some specific handler here ...
                        enableButton(fileUploadButtonId, fileCancelButtonId);
                    }

                });
            }
            else {
                alert("This browser doesn't support HTML5 file uploads!");
            }


    }

});

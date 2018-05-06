/// <reference path="spg.helper.js" />
/// <reference path="spg.api.js" />

var SP_Grid;
SP_Grid = (function(win, $) {

    var THEME = "office",
        MAX_FILE_UPLOAD_IN_MB = 5,
        MAX_FILE_UPLOAD_IN_BYTES = MAX_FILE_UPLOAD_IN_MB * 1024 * 1024,
        IMAGES_PATH = "/_imgs/Ribbon/";


    var gridId                = "jqxGrid",
        fileUploadDialogId = "fileUploadDialog",
        fileCancelButtonId = "fileCancelButton",
        fileUploadButtonId = "fileUploadButton",
        fileToUploadInputId = "fileUploadDialogInput",
        fileUploadDialogDocumentTypeSelect = "fileUploadDialogDocumentTypeSelect",
        reloadButtonId        = "sp-reloadButton",
        uploadButtonId        = "sp-uploadButton",
        messageContainerId    = "sp-message";

    var encodedString             = "",
        showSearchCompleteStatus = true,      // Override switch that indicates whether to suppress showing the search complete status message
        azureDomainUrl            = "";
        

    // The collection of meta-data attributes to be used for tagging the customer document
    var documentAttributeMap = {
        masterId: "",
        masterNumber: "",
        masterName: "",
        emailAddress: "",
        documentType    : ""
    };

    var dialogWinPropsMap = {
        POS_X: 0,
        POS_Y: 0
    };

    /**
     * Returns whether the passed in string value is neither undefined, null nor empty
     * @param value
     */
    function isEmptyStr(value) {
        return (value == null || value === "");
    }


    function selectFirstOption($htmlSelectList) {
        $htmlSelectList.find("option:first-child").prop("selected", true).trigger("change");
    }


    function showInlineMessage($messageContainer, msg) {

        // Show the message and fade it out of view after 4 seconds ...
        $messageContainer.show("slow").delay(4000).queue(function(nxt) {
            $(this).fadeOut("slow", "swing");
            // Dequeues so that next function in line executes ...
            nxt();
        });

    }

    function showInlineSuccessMessage(msg) {

        var $messageContainer = $("#" + messageContainerId);

        // Set the message text
        $messageContainer.text(msg);
        $messageContainer.removeClass("red").addClass("green").addClass("attention");
        showInlineMessage($messageContainer, msg);
    }

    function showInlineErrorMessage(msg) {

        var $messageContainer = $("#" + messageContainerId);

        // Set the message text
        $messageContainer.text(msg);
        $messageContainer.removeClass("green").addClass("red").addClass("attention");
        showInlineMessage($messageContainer, msg);
    }


    function getSelectedRowIndex(gridId) {
        // Gets the index of the selected row as a number.
        var rowIndex = $("#" + gridId).jqxGrid("selectedrowindex");
        return rowIndex;
    }

    function getRowDataByIndex(gridId, rowIdx) {
        var row = $("#" + gridId).jqxGrid("getrowdata", rowIdx);
        return row;
    }

    function getSelectedRowData(gridId) {
        var rowIndex = getSelectedRowIndex(gridId);
        return getRowDataByIndex(gridId, rowIndex);
    }


    function enableButton(/*buttonId_1, buttonId_2, ... buttonId_N*/) {
        var buttonId, i;
        for (i = 0; i < arguments.length; i++) {
            buttonId = arguments[i];
            $("#" + buttonId).jqxButton({ disabled: false });
        }
    }

    /**
     * A variable arguments function for disabling button(s)
     * @param {...string} var_args - The buttonId(s) passed as variable arguments from which to disable associated button(s)
     */
    function disableButton(/*buttonId_1, buttonId_2, ... buttonId_N*/) {
        var buttonId, i;
        for (i = 0; i < arguments.length; i++) {
            buttonId = arguments[i];
            $("#" + buttonId).jqxButton({ disabled: true });
        }
    }

    function loadGrid(data) {

        // Obtain the grid's data source and set the data at same time ...
        var source = {
            localdata: data,
            datatype: "json",
            datafields: [
                { name: "FileId", type: "string" },
                { name: "FileName", type: "string" },
                { name: "FilePath", type: "string" },
                { name: "MasterId", type: "string" },
                { name: "MasterNumber",         type: "string" },
                { name: "MasterName",       type: "string" },
                { name: "DocumentType",     type: "string" }
            ],
            // The data field specified here is used in generating the row's unique id property value (ie. row.uid)
            id: "FileId"
        };

        // Create the grid's data source adapter from the above source.
        var dataAdapter = new $.jqx.dataAdapter(source);

        // To refresh the Grid, you need to simply set its 'source' property again.
        $("#" + gridId).jqxGrid({ source: source });

    }


    function createButton(bag) {

        var buttonSelector = "#" + bag.id;

        var width    = bag.width || "75px",
            theme    = bag.theme || THEME,
            disabled = ((typeof bag.disabled === "boolean") && (bag.disabled));

        // Create the button.
        var $button = $(buttonSelector).jqxButton({ width: width, theme: theme, disabled: disabled });

        /*
         * Bind the handler function to the button click event.
         */
        if (typeof bag.clickHandler === "function") {
            $button.on("click", function (evt) {
                bag.clickHandler(evt);
            });
        }

        return $button;

    }


    function createDialog(bag) {

        var $dialog       = $("#" + bag.id),
            title         = bag.title || "",
            width         = bag.width || "500px",
            height        = bag.height || "350px",
            theme         = bag.theme || THEME,
            $cancelButton = $("#" + bag.cancelButtonId),
            initHandler   = bag.initHandler;

        $dialog.jqxWindow({
            title: title,
            isModal: true, autoOpen: false, resizable: false,
            height: height, width: width, theme: theme,
            cancelButton: $cancelButton,
            initContent: initHandler
        });

        return $dialog;

    }


    function closeDialog(dialogId) {
        var dialogSelector = "#" + dialogId;
        $(dialogSelector).jqxWindow("close");
    }

    var errorMessages = Array();
    function commonErrorHandler(jqxhr, status, error) {

       var errorMessage =
        {
            Message: "Request Failed",
            Status: jqxhr.status,
            StatusText: jqxhr.statusText,
            Error: error,
            ResponseText: jqxhr.responseText
        };

       errorMessages.push(errorMessage);
    }


    function searchCompleteHandler(data) {

        loadGrid(buildGridRows(data));

        if (!showSearchCompleteStatus) {
            // Do not show the message and just reset the switch for next time
            showSearchCompleteStatus = true;
        }
        else {
            showInlineSuccessMessage("Document search has successfully completed");
        }
    }

    function buildGridRows(data) {
        var rows = [],
            item;

        for (var i = 0; i < data.length; i++) {
            item = data[i];
            rows.push({
                FileId: item.FileId,
                FileName: item.FileName,
                FilePath: item.FilePath,
                MasterId: item.MasterId,
                MasterNumber: item.MasterNumber,
                MasterName: item.MasterName,
                DocumentType: item.DocumentType.Title
            });
        }
        return rows;
    }

    function retrieveSharePointFilesByMasterId(masterId) {
        var url = AzureApi.getAzureApi() + AzureApi.getRoute('GetByMasterId'); // + masterId;
        retrieveSharePointFiles(url, masterId);
    }

    function retrieveSharePointFilesByMasterNumber(masterNumber) {
        var url = AzureApi.getAzureApi() + AzureApi.getRoute('GetByMasterNumber'); // + masterNumber;
        retrieveSharePointFiles(url, masterNumber);
    }

    function retrieveSharePointFilesByAlternateId(fieldName, fieldValue) {
        var url = AzureApi.getAzureApi() + AzureApi.getRoute('GetByAlternateField');
        var qs = 'CUSTOMER_ID_TYPE=' + fieldName + '&CUSTOMER_ID_VALUE=' + fieldValue;
        retrieveSharePointFiles(url, qs);
    }

    function retrieveSharePointFilesByDocumentType(masterId, documentType) {
        var url = AzureApi.getAzureApi() + AzureApi.getRoute('GetByMasterId') + masterId;
        // var qs = 'CUSTOMER_ID_TYPE=' + idType + '&CUSTOMER_ID_VALUE=' + idValue + '&DOCUMENT_CATEGORY=' + category;
        retrieveSharePointFiles(url, qs);
    }

    function retrieveSharePointFiles(url, qs) {
        AzureApi.retrieveSharePointFiles(url, qs);
        if (AzureApi.getResultType() == 1) {
            var data = AzureApi.getResultData();
            searchCompleteHandler(data);
        }
        else {
            
        }
    }

    function setFileAttributes(fileId) {
        var url = AzureApi.getAzureApi() + AzureApi.getRoute('SetFileAttributes'),
            params = documentAttributeMap,
            $docCategorySelect = $("#" + fileUploadDialogDocumentTypeSelect);

        var data = {
            FileId: fileId,
            MasterId: params.masterId,
            MasterNumber: params.masterNumber,
            MasterName: params.masterName,
            DocumentType: $docCategorySelect.find("option:selected").text()
        };

        AzureApi.setFileAttributes(url, data);
        if (AzureApi.getResultType() == 1) {
            closeDialog(fileUploadDialogId);
            showInlineSuccessMessage("File upload has successfully completed");
            showSearchCompleteStatus = false;
            searchDocuments();
        }
        else {
            commonErrorHandler;
        }
        enableButton(fileUploadButtonId, fileCancelButtonId);
    }

    function showMessage() {
        parent.Xrm.Page.ui.setFormNotification("File Upload Process Completed.", "INFO", 1);
    }

    function buildFileUploadDialog() {

        var $uploadContext      = $("#sp-file-upload-container"),
            $fileToUploadInput  = $("#" + fileToUploadInputId, $uploadContext),
            $fileUploadDialog   = $("#" + fileUploadDialogId);

        function initFileUploadControl() {
            // Do file validation here ...
            $fileToUploadInput.on("change", function (e) {
                if (e.target.files.length > 0)
                {
                    var file  = e.target.files[0],
                        name  = file.name,
                        size  = file.size,
                        type  = file.type,
                        types = ["application/vnd.openxmlformats-officedocument", "application/pdf", "image/jpeg", "image/gif", "image/png"],
                        valid = false;

                    for (var i = 0, len = types.length; i < len; i++) {
                        if (type.indexOf(types[i]) > -1) {
                            valid = true;
                            break;
                        }
                    }

                    if (!valid) {
                        alert("An invalid file type was provided.  \nOnly Office documents, PDF and image files are allowed");
                        //alert("File - name:" + name + ", size:" + size + ", type:" + type);
                        this.value = "";
                    }
                    else if (size > MAX_FILE_UPLOAD_IN_BYTES) {
                        alert("The size of the file to upload exceeded the maximum allowed.  \nFile size must be less than " + MAX_FILE_UPLOAD_IN_MB + "MB.");
                        this.value = "";
                    }

                    if (!isEmptyStr(this.value)) {
                        enableButton(fileUploadButtonId);
                    }
                 }

            });

        }

        function onFileUploadButtonClick(event) {
            getUploadUrl();
            var formData = new FormData();
            var file = $fileToUploadInput.prop("files")[0];
            formData.append("file", file);

            if (window.FormData !== undefined) {
                AzureApi.uploadFile(formData);
                if (AzureApi.getResultType() == 1) {
                    var fileId = AzureApi.getResultData();
                    setFileAttributes(fileId);
                }

            }
            else {
                alert("This browser doesn't support HTML5 file uploads!");
            }
        }

        createDialog({
            id: fileUploadDialogId,
            title: "Select File To Upload",
            width: 350,
            height: 225,
            cancelButtonId: fileCancelButtonId,
            initHandler: function () {
                createButton({ id: fileCancelButtonId, width: 65 });
                createButton({ id: fileUploadButtonId, disabled: true, clickHandler: onFileUploadButtonClick, width: 65 });
                initFileUploadControl();
                $fileUploadDialog.jqxWindow("focus");
            }
        });

    }


    function populateDocumentTypeSelectList() {

        var params = documentAttributeMap,
            $docCategorySelect = $("#" + fileUploadDialogDocumentTypeSelect);

        function loadData(data) {
            var items = data;
            $.each(items, function (i, item) {
                $docCategorySelect.append($("<option>", {
                    value: item.ItemId,
                    text: item.Title
                }));
            });
        }

        AzureApi.retrieveCategories();
        if (AzureApi.getResultType() == 1) {
            var data = AzureApi.getResultData();
            loadData(data);
        }
    }


    function getUploadUrl() {

        var params = documentAttributeMap;
        setDataParams();
        AzureApi.buildFileUploadUrl(SP_CRMData.getEntityName(), params.masterId, params.masterNumber, params.masterName);
    }

    function onUploadButtonClick(event) {

        var params = documentAttributeMap,
            masterId = params.masterId,
            x          = event.clientX,
            y          = event.clientY;

        if (isEmptyStr(params.masterId)) {
            showInlineErrorMessage("This master id cannot be empty.  Upload is not allowed.");
            return;
        }

        dialogWinPropsMap.POS_X = x + 5;
        dialogWinPropsMap.POS_Y = y + 5;

        // Position the file upload consent dialog window near the upload button
        $("#" + fileUploadDialogId).jqxWindow({
            position: { x: dialogWinPropsMap.POS_X, y: dialogWinPropsMap.POS_Y }
        });

        // Show the file upload consent dialog window.
        $("#" + fileUploadDialogId).jqxWindow("open");

    }

    function onOpenButtonClick(event)
    {
        var rowData = getSelectedRowData(gridId) || {},
            isRowSelected = (typeof rowData.FileId !== 'undefined'),
            fileId = rowData.FileId,
            urlPath = AzureApi.getAzureApi() + AzureApi.getRoute('DownloadFile') + fileId;

        if (isRowSelected)
            parent.Xrm.Page.OpenSharePointDocument(urlPath);
    }

    function onReloadButtonClick(event) {
        retrieveSharePointFilesByMasterId(documentAttributeMap.masterId);
    }

    function onSearchButtonClick(event) {
        var isMaster = documentAttributeMap.isMaster;
        retrieveSharePointFilesByMasterId(documentAttributeMap.masterId);
    }

    function onDeleteButtonClick(event) {
        var rowData = getSelectedRowData(gridId) || {},
            fileId = rowData.fileId;

        var isRowSelected = (typeof rowData.fileId !== 'undefined');
        if (!isRowSelected) {
            showInlineErrorMessage("Please select row of file to delete and click delete button again");
            return;
        }

        // If you want, you can create logic to verify user can Delete SharePoint files not created by self
        deleteFile(fileId);
    }

    function deleteFile(fileId) {
        var success = AzureApi.deleteFile(fileId);
        if (success) {
            closeDialog(fileUploadDialogId);
            showInlineSuccessMessage("Deletion of file has successfully completed");
            showSearchCompleteStatus = false;
            searchDocuments();
        }
        else {
            commonErrorHandler;
        }
    }

    function buildGrid() {

        var winWidth = $(win).width();
        var gridColumns = SP_CRMData.getGridColumns();

        $("#" + gridId).jqxGrid({
            width: winWidth - 100,
            height: 300,
            theme: THEME,
            editable: false,
            selectionmode: 'singlerow',
            enableellipsis: true,
            rowsheight: 25,
            columns: gridColumns,
            showtoolbar: true,
            rendertoolbar: function (toolbar) {
                // appends buttons to the status bar.
                var container = $("<div style='overflow: hidden; position: relative; margin: 5px;'></div>");
                var uploadButton = $("<div id='" + uploadButtonId + "' style='float: left; margin-left: 5px;'><img style='position: relative; margin-top: 2px;' src='" + IMAGES_PATH + "uploaddocument_16.png'/><span style='margin-left: 4px; position: relative; top: -3px;'>Upload</span></div>");
                var openButton = $("<div id='jqxButtonSearch' style='float: left; margin-left: 5px;'><img style='position: relative; margin-top: 2px;' src='" + IMAGES_PATH + "verbosetracing_16.png'/></div>");
                var reloadButton = $("<div id='" + reloadButtonId + "' style='float: left; margin-left: 5px;'><img style='position: relative; margin-top: 2px;' src='" + IMAGES_PATH + "Refresh_16.png'/><span style='margin-left: 4px; position: relative; top: -3px;'>Refresh</span></div>");
                var deleteButton = $("<div id='jqxButtonDelete' style='float: left; margin-left: 5px;'><img style='position: relative; margin-top: 2px;' src='" + IMAGES_PATH + "Delete16.png'/><span style='margin-left: 4px; position: relative; top: -3px;'>Delete</span></div>");
                var helpButton = $("<div id='jqxButtonHelp' style='float: right; margin-left: 5px;'><img style='position: relative; margin-top: 2px;' src='" + IMAGES_PATH + "Help_16.png'/><span style='margin-left: 4px; position: relative; top: -3px;'></span></div>");
                container.append(uploadButton);
                container.append(openButton);
                container.append(reloadButton);
                container.append(deleteButton);
                container.append(helpButton);
                toolbar.append(container);
                uploadButton.jqxButton({ width: 80, height: 20, theme: THEME });
                openButton.jqxButton({ width: 30, height: 20, theme: THEME });
                reloadButton.jqxButton({ width: 80, height: 20, theme: THEME });
                deleteButton.jqxButton({ width: 80, height: 20, theme: THEME });
                helpButton.jqxButton({ width: 30, height: 20, theme: THEME });


                // upload file.
                uploadButton.click(function (event) {
                    onUploadButtonClick(event);
                });

                // reload grid data.
                reloadButton.click(function (event) {
                    onReloadButtonClick(event);
                });

                // open File
                openButton.click(function (event) {
                    onOpenButtonClick();
                });

                deleteButton.click(function (event) {
                    onDeleteButtonClick(event);
                });

                helpButton.click(function (event) {
                    SP_CRMData.showCRMData();                   
                    if (errorMessages.length > 0)
                    {
                        for (var i=0; i< errorMessages.length; i++)
                        {
                            var errorMessage = errorMessages.pop();
                            alert(errorMessage.responseText);
                        }
                    }
                });

            }
        });

    }

    function buildAPIPath(path) {
        return azureDomainUrl + path;
    }
    
    function setDataParams() {
        var params = documentAttributeMap,
            data        = SP_CRMData.getCRMData();
        params.masterId = data.MasterId;
        params.masterNumber = data.MasterNumber;
        params.masterName = data.MasterName;
        params.documentType = data.DocumentType;
        params.emailAddress = data.EmailAddress;
        azureDomainUrl = SP_CRMData.getAzureAPI();
        AzureApi.setAzureApi(azureDomainUrl);
    }

    function searchDocuments() {
        retrieveSharePointFilesByMasterId(documentAttributeMap.masterId);
    }

    function openLoader()
    {
        $("#jqxLoader").jqxLoader({  width: 100, height: 60, imagePosition: 'top', autoOpen: true });
    }

    function closeLoader()
    {
        $('#jqxLoader').jqxLoader('close');
    }

    function onScreenLoad() {

        // Obtain CRM data and load into integration module for subsequent availability to this module
        openLoader();
        SP_CRMData.loadCRMData();

        win.setTimeout(function () {

            buildGrid();
            setDataParams();
            buildFileUploadDialog();
            populateDocumentTypeSelectList()
			searchDocuments();
			closeLoader();
        }, 5000);


    }
   
    return {
        onScreenLoad: onScreenLoad,
        searchDocuments: searchDocuments
        // renderFileNameColumn: renderFileNameColumn

    };


})(window, jQuery);



$(document).ready(function() {
    var spGrid = SP_Grid;
    spGrid.onScreenLoad();
    // spGrid.searchDocuments();
});

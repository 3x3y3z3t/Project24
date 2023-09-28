/*  home/management/updater.js
    Version: v1.3 (2023.09.27)

    Author
        Arime-chan
 */

const UPDATER_HEADER_BUFFER = 5 * 1024 * 1024; // max header buffer size is 5 KiB;

const UPDATER_BLOCK_NAME_PREV = "Prev";
const UPDATER_BLOCK_NAME_NEXT = "Next";

// ==================================================
// Upload File Status Codes;

const UPDATER_FILE_STATUS_NO_CHANGE = P24_TEXT_COLOR_GREEN;
const UPDATER_FILE_STATUS_CREATE = P24_TEXT_COLOR_NORMAL;
const UPDATER_FILE_STATUS_UPDATE = P24_TEXT_COLOR_YELLOW;
const UPDATER_FILE_STATUS_ERROR = P24_TEXT_COLOR_RED;

// ==================================================
// Batch Status Codes;

const UPDATER_BATCH_STATUS_NOT_STARTED = 0;
const UPDATER_BATCH_STATUS_IN_PROGRESS = 1;
const UPDATER_BATCH_STATUS_SUCCESS = 3;
const UPDATER_BATCH_STATUS_ERROR = 4;

// ==================================================
// Updater Status Codes;

const UPDATER_STATUS_NONE = 0;

const UPDATER_STATUS_PREV_PURGE_QUEUED = -1;
const UPDATER_STATUS_PREV_PURGE_RUNNING = -2;
const UPDATER_STATUS_PREV_APPLY_QUEUED = -3;
const UPDATER_STATUS_PREV_APPLY_RUNNING = -4;

const UPDATER_STATUS_NEXT_PURGE_QUEUED = 1;
const UPDATER_STATUS_NEXT_PURGE_RUNNING = 2;
const UPDATER_STATUS_NEXT_APPLY_QUEUED = 3;
const UPDATER_STATUS_NEXT_APPLY_RUNNING = 4;


// ==================================================
window.UpdaterPage = {
    Data: null,
    UI: null,

    m_AwaitingData: false,


    init: function () {
        this.Data.init();
        this.UI.init();
    },

    reload: function () {
        this.ajax_fetchPageData();
    },

    // ==================================================

    startUploadMetadata: function () {
        if (this.Data.Metadata.Status == UPDATER_BATCH_STATUS_SUCCESS) {
            console.log("Metadata uploaded, no need to reupload.");
            this.startUploadBatches();
            return;
        }

        this.UI.refreshUploadBlock("Metadata");
        this.ajax_uploadMetadata();
    },

    startUploadBatches: function () {
        for (let i = 0; i < this.Data.Batches.length; ++i) {
            switch (this.Data.Metadata.BatchesMetadata[i].Status) {
                case UPDATER_BATCH_STATUS_SUCCESS:
                    console.log("Batch " + i + " uploaded, no need to reupload.");
                    continue;
                case UPDATER_BATCH_STATUS_IN_PROGRESS:
                    console.log("Batch " + i + " is in progress, no need to restart.");
                    continue;
            }

            let batch = this.Data.Batches[i];
            this.Data.Metadata.BatchesMetadata[i].Status = UPDATER_BATCH_STATUS_IN_PROGRESS;
            this.UI.refreshUploadBlock(i);
            this.uploadBatch(batch);
        }
    },

    uploadBatch: function (_batch) {
        let header = {
            Id: _batch.Id,
            Size: _batch.Size,
            Count: _batch.Count
        };

        let formData = new FormData();
        for (let i = 0; i < _batch.Files.length; ++i) {
            formData.append("_files", _batch.Files[i].File);
        }

        this.ajax_uploadBatch(header, formData);
    },

    tryFinalizeUpload: function () {
        for (let i = 0; i < this.Data.Metadata.BatchesCount; ++i) {
            if (this.Data.Metadata.BatchesMetadata[i].Status != UPDATER_BATCH_STATUS_SUCCESS)
                return;
        }

        let content = P24Localization[LOCL_DESC_UPDATER_UPLOAD_SUCCESS];
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], content, MODAL_ICON_SUCCESS, "OK", "UpdaterPage.ajax_fetchPageData()");
    },

    // ==================================================

    ajax_fetchPageData: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "Updater?handler=FetchPageData",
            success: function (_content, _textStatus, _xhr) { UpdaterPage.ajax_fetchPageData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_clearInternalError: function () {
        if (this.m_AwaitingData || this.Data.PageData == null || this.Data.PageData.Message == "")
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Updater?handler=ClearInternalError",
            headers: { RequestVerificationToken: token },
            cache: false,
            success: function (_content, _textStatus, _xhr) { UpdaterPage.ajax_clearInternalError_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_uploadMetadata: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Updater?handler=UploadMetadata",
            headers: { RequestVerificationToken: token },
            data: JSON.stringify(this.Data.Metadata),
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { UpdaterPage.ajax_uploadMetadata_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_uploadMetadata_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_uploadBatch: function (_header, _formData) {
        if (this.m_AwaitingData)
            return;

        let batchId = _header.Id;
        _header.RequestVerificationToken = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Updater?handler=UploadBatch",
            headers: _header,
            data: _formData,
            cache: false,
            contentType: false,
            processData: false,
            success: function (_content, _textStatus, _xhr) { UpdaterPage.ajax_uploadBatch_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrown) {
                UpdaterPage.Data.Metadata.BatchesMetadata[batchId].Status = UPDATER_BATCH_STATUS_ERROR;
                UpdaterPage.UI.refreshUploadBlock(batchId);
                P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
            }
        });
    },

    ajax_performPurge: function (_blockName) {
        if (this.m_AwaitingData)
            return;

        if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
            this.m_AwaitingData = false;
            return;
        }

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        let fn = null;
        if (_blockName == UPDATER_BLOCK_NAME_PREV)
            fn = UpdaterPage.ajax_purgePrev_success;
        else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
            fn = UpdaterPage.ajax_purgeNext_success;

        $.ajax({
            type: "POST",
            url: "Updater?handler=Purge" + _blockName,
            headers: { RequestVerificationToken: token },
            cache: false,
            success: fn,
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_performAbort: function (_blockName) {
        if (this.m_AwaitingData)
            return;

        if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
            this.m_AwaitingData = false;
            return;
        }

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        let fn = null;
        if (_blockName == UPDATER_BLOCK_NAME_PREV)
            fn = UpdaterPage.ajax_abortPrev_success;
        else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
            fn = UpdaterPage.ajax_abortNext_success;

        $.ajax({
            type: "POST",
            url: "Updater?handler=Abort" + _blockName,
            headers: { RequestVerificationToken: token },
            cache: false,
            success: fn,
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_performApply: function (_blockName) {
        if (this.m_AwaitingData)
            return;

        if (_blockName != UPDATER_BLOCK_NAME_PREV && _blockName != UPDATER_BLOCK_NAME_NEXT) {
            this.m_AwaitingData = false;
            return;
        }

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        let fn = null;
        if (_blockName == UPDATER_BLOCK_NAME_PREV)
            fn = UpdaterPage.ajax_applyPrev_success;
        else if (_blockName == UPDATER_BLOCK_NAME_NEXT)
            fn = UpdaterPage.ajax_applyNext_success;

        $.ajax({
            type: "POST",
            url: "Updater?handler=Apply" + _blockName,
            headers: { RequestVerificationToken: token },
            cache: false,
            success: fn,
            error: function (_xhr, _textStatus, _errorThrow) { UpdaterPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    // ==================================================

    ajax_error: function (_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },

    ajax_fetchPageData_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            let processedData = this.Data.processPageData(body);
            if (processedData == null)
                return;

            this.UI.refreshPage(processedData);

            return;
        }
    },

    ajax_clearInternalError_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        if (_content.startsWith(P24_MSG_TAG_SUCCESS))
            console.log("Internal Error cleared.");
        else
            console.error("Error clearing Internal Error :))");
    },

    ajax_uploadMetadata_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (P24Utils.Ajax.successContentCheckCommon(_content, body)) {
            this.Data.Metadata.Status = UPDATER_BATCH_STATUS_SUCCESS;
            UpdaterPage.UI.refreshUploadBlock("Metadata");
            this.startUploadBatches();
            return;
        }

        this.Data.Metadata.Status = UPDATER_BATCH_STATUS_ERROR;
        UpdaterPage.UI.refreshUploadBlock("Metadata");
    },

    ajax_uploadMetadata_error: function (_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;

        this.Data.Metadata.Status = UPDATER_BATCH_STATUS_ERROR;
        UpdaterPage.UI.refreshUploadBlock("Metadata");
        P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
    },

    ajax_uploadBatch_success: function (_content, _textStatus, _xhr) {
        let body = _content.substring(6);

        if (_content.startsWith(P24_MSG_TAG_SUCCESS)) {
            this.Data.Metadata.BatchesMetadata[body].Status = UPDATER_BATCH_STATUS_SUCCESS;
            UpdaterPage.UI.refreshUploadBlock(body);

            this.tryFinalizeUpload();
            return;
        }

        let obj = JSON.parse(body);
        let id = obj[0];
        if (_content.startsWith(P24_MSG_TAG_ERROR)) {
            let msg = "<div>Có lỗi xảy ra khi upload batch " + id + ". Vui lòng upload lại.</div><div><b>Msg:</b> " + obj[1] + "</div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_FAIL], msg, "error");
        }
        else if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_EXCEPTION], "<pre>" + body + "</pre>");
        }
        else {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_UNKNOWN_ERR], "<pre>" + _content + "</pre>", "error");
        }

        this.Data.Metadata.BatchesMetadata[id].Status = UPDATER_BATCH_STATUS_ERROR;
        UpdaterPage.UI.refreshUploadBlock(id);
    },

    ajax_purgePrev_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_purgeCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
    },

    ajax_purgeNext_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_purgeCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
    },

    ajax_abortPrev_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_abortCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
    },

    ajax_abortNext_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_abortCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
    },

    ajax_applyPrev_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_applyCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_PREV);
    },

    ajax_applyNext_success: function (_content, _textStatus, _xhr) {
        UpdaterPage.ajax_applyCommon_success(_content, _textStatus, _xhr, UPDATER_BLOCK_NAME_NEXT);
    },

    ajax_purgeCommon_success: function (_content, _textStatus, _xhr, _blockName) {
        this.m_AwaitingData = false;

        let fname = "ajax_purge" + _blockName;
        let statusCode = 0;
        let descKey = "";

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                statusCode = UPDATER_STATUS_PREV_PURGE_QUEUED;
                descKey = LOCL_DESC_UPDATER_PREV_PURGE_QUEUED;
                break;
            case UPDATER_BLOCK_NAME_NEXT:
                statusCode = UPDATER_STATUS_NEXT_PURGE_QUEUED;
                descKey = LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED;
                break;
            default:
                let htmlInner = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code></div><div>fn: <code>" + fname + "Common</code></div>";
                Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], htmlInner, MODAL_ICON_WARNING);
                return;
        }

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let htmlInner = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + "</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], htmlInner, MODAL_ICON_WARNING);
            return;
        }

        if (status == statusCode && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[descKey], MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
            + "<div>fn: <code>" + fname + "</code></div>"
            + "<div>body: <code>" + body + "</code></div>"
            + "<div>pageData.Status: <code>" + this.Data.PageData.Status + "</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
    },

    ajax_applyCommon_success: function (_content, _textStatus, _xhr, _blockName) {
        this.m_AwaitingData = false;

        let fname = "ajax_apply" + _blockName;
        let statusCode = 0;
        let descKey = "";

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                statusCode = UPDATER_STATUS_PREV_APPLY_QUEUED;
                descKey = LOCL_DESC_UPDATER_PREV_APPLY_QUEUED;
                break;
            case UPDATER_BLOCK_NAME_NEXT:
                statusCode = UPDATER_STATUS_NEXT_APPLY_QUEUED;
                descKey = LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED;
                break;
            default:
                let htmlInner = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code></div><div>fn: <code>" + fname + "Common</code></div>";
                Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], htmlInner, MODAL_ICON_WARNING);
                return;
        }

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let htmlInner = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + "</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], htmlInner, MODAL_ICON_WARNING);
            return;
        }

        if (status == statusCode && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[descKey], MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
            + "<div>fn: <code>" + fname + "</code></div>"
            + "<div>body: <code>" + body + "</code></div>"
            + "<div>pageData.Status: <code>" + this.Data.PageData.Status + "</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
    },

    ajax_abortCommon_success: function (_content, _textStatus, _xhr, _blockName) {
        this.m_AwaitingData = false;

        let fname = "ajax_abort" + _blockName;

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV: break;
            case UPDATER_BLOCK_NAME_NEXT: break;
            default:
                let htmlInner = "<div><code>" + ERRCODE_UPDATER_INVALID_BLOCK_NAME + "</code></div><div>fn: <code>" + fname + "Common</code></div>";
                Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], htmlInner, MODAL_ICON_WARNING);
                return;
        }

        let body = _content.substring(6);

        if (!P24Utils.Ajax.successContentCheckCommon(_content, body))
            return;

        let err = true;
        let status = UPDATER_STATUS_NONE;

        if (body[1] == ':') {
            let num = parseInt(body.substring(2));

            if (num != NaN) {
                status = num;
                err = false;
            }
        }

        if (err) {
            let htmlInner = "<div>" + P24Localization[LOCL_DESC_MALFORMED] + ".</div><div>fn: <code>" + fname + "</code></div><div>body: <code>" + body + "</code></div>";
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], htmlInner, MODAL_ICON_WARNING);
            return;
        }

        if (status == UPDATER_STATUS_PREV_PURGE_RUNNING || status == UPDATER_STATUS_NEXT_PURGE_RUNNING) {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], P24Localization[LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT], MODAL_ICON_WARNING);
            return;
        }

        if (status == UPDATER_STATUS_NONE && body[0] != '0') {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_SUCCESS], P24Localization[LOCL_DESC_UPDATER_ABORT_SUCCESS], MODAL_ICON_SUCCESS, "OK", "location.reload()");
            return;
        }

        let html = "<div><code>" + ERRCODE_UNEXPECTED_CONTROL + "</code>.</div>"
            + "<div>fn: <code>" + fname + "</code></div>"
            + "<div>body: <code>" + body + "</code></div>"
            + "<div>pageData.Status: <code>" + this.Data.PageData.Status + "</code></div>";
        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_WARN], html, MODAL_ICON_WARNING);
    },
};

UpdaterPage.Data = {
    PageData: null,

    Metadata: null,
    Batches: null,

    ProcessedFilesList: null,
    TotalFilesSize: 0,


    init: function () {

    },

    // ==================================================

    processPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        if (parsedData == null || parsedData.MainVer == null) {
            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_ERR], P24Localization[LOCL_DESC_UPDATER_NOMAIN], MODAL_ICON_ERROR);
            return null;
        }

        let dueTime = new Date(parsedData.DueTime);
        if (dueTime != null && !isNaN(dueTime))
            parsedData.DueTime = dueTime;

        let remainingTime = TimeSpan.parse(parsedData.Remaining);
        if (remainingTime != null)
            parsedData.Remaining = remainingTime;

        // we use Map to keep the file list in order;
        let map = new Map();
        if (parsedData.Files != null) {
            for (let i = 0; i < parsedData.Files.length; ++i) {
                let fi = parsedData.Files[i];
                map.set(fi.HashCode, fi);
            }
        }
        parsedData.Files = map;

        //console.log(parsedData);

        this.PageData = parsedData;

        return parsedData;
    },

    // ==================================================

    processUploadFilesList: function (_files) {
        this.TotalFilesSize = 0;

        if (_files.length == 0) {
            this.Metadata = null;
            this.Batches = null;
            this.ProcessedFilesList = null;
            return;
        }

        this.ProcessedFilesList = new Map();
        this.Batches = [];

        let batchesMetadata = [];

        //let lastModDates = [];
        let filesCount = 0;
        let filesSize = 0;

        let batch = [];
        let batchIndex = 0;
        let batchFilesSize = 0;
        let batchFilesCount = 0;
        let filesMetadata = {};

        for (let i = 0; i < _files.length; ++i) {
            let file = _files[i];
            let fileItem = this.processSingleFile(file);

            this.ProcessedFilesList.set(fileItem.HashCode, fileItem);
            this.TotalFilesSize += file.size;

            if (fileItem.Status == UPDATER_FILE_STATUS_NO_CHANGE)
                continue;

            if (batchFilesSize >= P24_MAX_UPLOAD_SIZE - UPDATER_HEADER_BUFFER || batchFilesCount >= P24_MAX_UPLOAD_COUNT) {
                this.Batches[batchIndex] = {
                    Id: batchIndex,
                    //Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                    Size: batchFilesSize,
                    Count: batchFilesCount,
                    Files: batch
                };

                batchesMetadata[batchIndex] = {
                    Id: batchIndex,
                    Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                    FilesCount: batchFilesCount,
                    FilesSize: batchFilesSize,
                    FilesMetadata: filesMetadata
                };

                batch = [];
                batchFilesSize = 0;
                batchFilesCount = 0;
                ++batchIndex;
                filesMetadata = {};
            }

            filesMetadata[fileItem.HashCode] = { HashCode: fileItem.HashCode, LastMod: file.lastModified };

            //lastModDates.push({ Key: fileItem.HashCode, Value: file.lastModified });
            filesSize += file.size;
            ++filesCount;

            batch.push(fileItem);
            batchFilesSize += file.size;
            ++batchFilesCount;
        }

        if (batchFilesCount > 0) {
            this.Batches[batchIndex] = {
                Id: batchIndex,
                //Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                Size: batchFilesSize,
                Count: batchFilesCount,
                Files: batch
            };

            batchesMetadata[batchIndex] = {
                Id: batchIndex,
                Status: UPDATER_BATCH_STATUS_NOT_STARTED,
                FilesCount: batchFilesCount,
                FilesSize: batchFilesSize,
                FilesMetadata: filesMetadata
            };
        }

        this.Metadata = {
            Status: UPDATER_BATCH_STATUS_NOT_STARTED,
            FilesCount: filesCount,
            FilesSize: filesSize,
            BatchesCount: this.Batches.length,
            BatchesMetadata: batchesMetadata,
        };
    },

    processSingleFile: function (_file) {
        let prePos = _file.webkitRelativePath.indexOf("/");
        if (prePos < 0) {
            console.error("Invalid file: " + _file.name);
            return null;
        }

        let path = "";

        let pos = _file.webkitRelativePath.lastIndexOf("/");
        if (pos > prePos) {
            path = _file.webkitRelativePath.substring(prePos + 1, pos);
        }

        if (path != "" && !path.endsWith("/"))
            path += "/";

        let hashCode = cyrb53(path + _file.name);
        let dateString = P24Utils.formatDateString_endsAtMinute(_file.lastModifiedDate);

        let status = UPDATER_FILE_STATUS_CREATE;

        let svrFile = this.PageData.Files.get(hashCode);
        if (svrFile != null) {
            if (svrFile.LastMod == dateString)
                status = UPDATER_FILE_STATUS_NO_CHANGE;
            else
                status = UPDATER_FILE_STATUS_UPDATE;
        }

        let fileItem = {
            Path: path,
            LastMod: dateString,
            HashCode: hashCode,
            Status: status,
            File: _file,
        };

        return fileItem;
    },
};

UpdaterPage.UI = {
    m_DivLblUpdaterStatus: null,

    m_DivLblMain: null,
    m_DivLblPrev: null,
    m_DivLblNext: null,
    m_DivLstFilesSvr: null,
    m_DivLstFilesUpl: null,
    m_LblInputFiles: null,
    m_InputFiles: null,
    m_BtnClear: null,
    m_BtnUpload: null,

    m_DivUploadStatPanel: null,
    m_DivUploadStatMetadata: null,
    m_DivUploadStatBatches: null,

    m_Timer: null,
    m_EventSet: false,
    m_DebugHashCode: false,


    init: function () {
        this.m_DivLblUpdaterStatus = $("#div-lbl-updater-status");

        this.m_DivLblMain = $("#div-lbl-main");
        this.m_DivLblPrev = $("#div-lbl-prev");
        this.m_DivLblNext = $("#div-lbl-next");
        this.m_DivLstFilesSvr = $("#div-lst-files-svr");
        this.m_DivLstFilesUpl = $("#div-lst-files-upl");
        this.m_LblInputFiles = $("#lbl-input-files");
        this.m_InputFiles = $("#input-files");
        this.m_BtnClear = $("#btn-clear");
        this.m_BtnUpload = $("#btn-upload");

        this.m_DivUploadStatPanel = $("#div-upload-stat-panel");

        m_Timer = setInterval(function () {
            $("#div-files-table-head").css("padding-right", P24Utils.getScrollbarWidth());
        }, 10 * 1000);
    },

    // ==================================================

    cancelModal: function () {
        UpdaterPage.m_AwaitingData = false;
    },

    inputFiles_onChange: function () {
        UpdaterPage.Data.processUploadFilesList(this.m_InputFiles[0].files);
        this.refreshPanelUploadedFiles();
    },

    btnClear_onClick: function () {
        this.m_InputFiles.val(null);
        this.clearUploadPanel();
    },

    btnUpload_onClick: function () {
        UpdaterPage.startUploadMetadata();
    },

    btnPurge_onClick: function (_blockName) {
        let content = "";

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV];
                break;
            case UPDATER_BLOCK_NAME_NEXT:
                content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT];
                break;

            default: return;
        }

        this.openConfirmationModal(content, "UpdaterPage.ajax_performPurge('" + _blockName + "')", "UpdaterPage.UI.cancelModal()");
    },

    btnAbort_onClick: function (_blockName) {
        let content = "";
        let status = UpdaterPage.Data.PageData.Status;

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                switch (status) {
                    case UPDATER_STATUS_PREV_PURGE_QUEUED:
                        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE];
                        break;
                    case UPDATER_STATUS_PREV_APPLY_QUEUED:
                        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY];
                        break;
                }
                break;

            case UPDATER_BLOCK_NAME_NEXT:
                switch (status) {
                    case UPDATER_STATUS_NEXT_PURGE_QUEUED:
                        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE];
                        break;
                    case UPDATER_STATUS_NEXT_APPLY_QUEUED:
                        content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY];
                        break;
                }
                break;

            default: return;
        }

        this.openConfirmationModal(content, "UpdaterPage.ajax_performAbort('" + _blockName + "')", "UpdaterPage.UI.cancelModal()");
    },

    btnApply_onClick: function (_blockName) {
        let content = "";

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV];
                break;
            case UPDATER_BLOCK_NAME_NEXT:
                content = P24Localization[LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT];
                break;

            default: return;
        }

        this.openConfirmationModal(content, "UpdaterPage.ajax_performApply('" + _blockName + "')", "UpdaterPage.UI.cancelModal()");
    },

    lblInputFiles_dragIn: function () {
        let lbl = $("#lbl-input-files");

        lbl.addClass("border");
        lbl.addClass("border-success");

        //lbl.attr("style", "border-width:2px!important");
    },

    lblInputFiles_dragOut: function (_element) {
        let lbl = $("#lbl-input-files");

        lbl.removeClass("border");
        lbl.removeClass("border-success");

        //lbl.removeAttr("style");
    },

    // ==================================================

    refreshPage: function (_data) {
        this.refreshPanelVer("main");
        this.refreshPanelVer(UPDATER_BLOCK_NAME_PREV);
        this.refreshPanelVer(UPDATER_BLOCK_NAME_NEXT);

        let html = "";
        for (let pair of UpdaterPage.Data.PageData.Files) {
            html += this.constructFileItemHtml(pair[1], true);
        }
        this.m_DivLstFilesSvr.html(html);

        let element = $("#div-svr-title");
        html = element.html();

        let pos = html.indexOf("(");
        if (pos > 0)
            html = html.substring(0, pos - 1);

        html += " (" + UpdaterPage.Data.PageData.Files.size + ")";
        element.html(html);

        this.refreshPanelUploadedFiles();

        this.m_InputFiles.removeAttr("disabled");

        if (!this.m_EventSet) {
            $("#div-input-files").on("mouseenter dragenter", UpdaterPage.UI.lblInputFiles_dragIn);
            $("#div-input-files").on("mouseleave dragend drop dragexit dragleave", UpdaterPage.UI.lblInputFiles_dragOut);
            this.m_EventSet = true;
        }

        // open modal if there is error message;
        if (UpdaterPage.Data.PageData.Message != null && UpdaterPage.Data.PageData.Message != "") {
            Modal.Common.openTwoBtnModal("Internal Message", UpdaterPage.Data.PageData.Message, MODAL_ICON_ERROR, "Clear Message", "UpdaterPage.ajax_clearInternalError()", "Close");
        }

        this.m_DivLblUpdaterStatus.html(this.constructLblUpdaterStatusHtml(_data));
    },

    refreshPanelUploadedFiles: function () {
        let data = UpdaterPage.Data;

        if (data.Metadata == null) {
            this.clearUploadPanel();
            return;
        }

        let html = "";

        // ;
        let uploadSizeString = P24Utils.formatDataLength(data.Metadata.FilesSize);
        let totalSizeString = P24Utils.formatDataLength(data.TotalFilesSize);

        html = data.Metadata.FilesCount + "/" + data.ProcessedFilesList.size + " file";
        if (data.ProcessedFilesList.size > 1)
            html += "s";
        html += " (" + uploadSizeString + "/" + totalSizeString + ")";
        this.m_LblInputFiles.html(html);

        // ==========;
        html = "";
        for (let pair of data.ProcessedFilesList) {
            html += this.constructFileItemHtml(pair[1]);
        }

        this.m_DivLstFilesUpl.html(html);

        // ==========;
        let element = $("#div-upl-title");
        html = element.html();

        let pos = html.indexOf("(");
        if (pos > 0)
            html = html.substring(0, pos - 1);

        html += " (" + data.Metadata.FilesCount + ")";
        element.html(html);

        // ==========;
        if (data.Metadata.BatchesCount > 0) {
            this.refreshPanelUploadStatus();

            this.m_BtnUpload.removeAttr("disabled");
        } else {
            this.m_BtnUpload.attr("disabled", true);
        }

        if (this.m_InputFiles[0].files.length > 0) {
            this.m_BtnClear.removeAttr("disabled");
        } else {
            this.m_BtnClear.attr("disabled", true);
        }
    },

    refreshPanelUploadStatus: function () {
        let metadata = UpdaterPage.Data.Metadata;

        const id_divUploadStatMetadata = "div-upload-stat-metadata";
        const id_divUploadStatBatchX = "div-upload-stat-batch-";

        let html = this.constructUploadBlockHtml(id_divUploadStatMetadata, "Metadata", metadata.Status);

        html += "<div class=\"vr mx-1\"></div>";

        for (let i = 0; i < metadata.BatchesCount; ++i) {
            let tooltipText = metadata.BatchesMetadata[i].FilesCount + " file";
            if (metadata.BatchesMetadata[i].FilesCount > 1)
                tooltipText += "s";
            tooltipText += ", " + P24Utils.formatDataLength(metadata.BatchesMetadata[i].FilesSize);
            html += this.constructUploadBlockHtml(id_divUploadStatBatchX + i, "Batch " + i, metadata.BatchesMetadata[i].Status, tooltipText);
        }

        this.m_DivUploadStatPanel.html(html);
        P24Utils.reloadAllTooltips();

        this.m_DivUploadStatMetadata = $("#" + id_divUploadStatMetadata);
        this.m_DivUploadStatBatches = [];
        for (let i = 0; i < metadata.BatchesCount; ++i) {
            this.m_DivUploadStatBatches[i] = $("#" + id_divUploadStatBatchX + i);
        }
    },

    refreshPanelVer: function (_blockName = "main") {
        let data = null;
        let element = null;
        let btnStat = [];

        switch (_blockName) {
            case "main":
                data = UpdaterPage.Data.PageData.MainVer;
                element = this.m_DivLblMain;
                break;
            case UPDATER_BLOCK_NAME_PREV:
                data = UpdaterPage.Data.PageData.PrevVer;
                element = this.m_DivLblPrev;
                btnStat = this.computePanelVerBtnStatus(_blockName);
                break;
            case UPDATER_BLOCK_NAME_NEXT:
                data = UpdaterPage.Data.PageData.NextVer;
                element = this.m_DivLblNext;
                btnStat = this.computePanelVerBtnStatus(_blockName);
                break;
            default: return;
        }

        let colClass0 = "col-6 col-md-12";
        let colClass1 = "col-6 col-md-12";

        let html = "";

        if (data == null) {
            html += "<div class=\"" + colClass0 + "\">&times;</div><div class=\"" + colClass1 + "\">" + this.constructVersionPanelButton("", true) + "</div>";
        } else {
            html += "<div class=\"" + colClass0 + "\">" + data + "</div>";

            // TODO: fix svg icon;
            if (_blockName == "main") {
                html += "<div class=\"" + colClass1 + "\">" + this.constructVersionPanelButton("arrow-clockwise", "primary", "UpdaterPage.ajax_fetchPageData()") + "</div>";
            } else {
                html += "<div class=\"" + colClass1 + "\">"
                    + this.constructVersionPanelButton("trash3", "danger", "UpdaterPage.UI.btnPurge_onClick('" + _blockName + "')", btnStat[0])
                    + this.constructVersionPanelButton("x-lg", "warning", "UpdaterPage.UI.btnAbort_onClick('" + _blockName + "')", btnStat[1])
                    + this.constructVersionPanelButton("check-lg", "success", "UpdaterPage.UI.btnApply_onClick('" + _blockName + "')", btnStat[2])
                    + "</div>";
            }
        }

        element.html(html);
    },

    refreshUploadBlock: function (_id, _blockText, _status) {
        let element = null;
        let status = null;
        let blockText = null;

        if (_id == "Metadata") {
            element = this.m_DivUploadStatMetadata;
            status = UpdaterPage.Data.Metadata.Status;
            blockText = _id;
        }
        else {
            element = this.m_DivUploadStatBatches[_id];
            status = UpdaterPage.Data.Metadata.BatchesMetadata[_id].Status;
            blockText = "Batch " + _id;
        }

        if (element == null)
            return;

        let blockClass = "alert-secondary";
        switch (status) {
            case UPDATER_BATCH_STATUS_IN_PROGRESS:
                blockClass = "alert-warning";
                break;
            case UPDATER_BATCH_STATUS_SUCCESS:
                blockClass = "alert-success";
                break;
            case UPDATER_BATCH_STATUS_ERROR:
                blockClass = "alert-danger";
                break;
        }

        // TODO: svg is now classes;
        let html = this.constructUploadBlockSvg(_status) + blockText;
        element.html(html);

        element.removeClass("alert-secondary alert-warning alert-success alert-danger");
        element.addClass(blockClass);
    },

    // ==================================================

    constructVersionPanelButton: function (_bsIconClass, _subClass = "", _onClick = "", _isEnabled = true) {
        if (_subClass != "")
            _subClass = " btn-outline-" + _subClass;

        let attr = "";
        if (!_isEnabled)
            attr = " disabled";

        let html = "<button type=\"button\" class=\"btn" + _subClass + " m-1 px-2 py-1\" onclick=\"" + _onClick + "\"" + attr + ">"
            + "<svg width=\"16\" height=\"16\" fill=\"currentColor\">"
            + "<use xlink:href=\"/lib/bootstrap-icons/bootstrap-icons.svg#" + _bsIconClass + "\" />"
            + "</svg></button>";

        return html;
    },

    constructLblUpdaterStatusHtml: function (_data) {
        let classCommon = "border-start border-primary border-2 bg-light px-2 py-1 mx-2";
        let dueTimeString = DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm:ss.fff", _data.DueTime);
        let remainingTimeString = DotNetString.formatCustomTimeSpan("d.hh:mm:ss.fff", _data.Remaining);

        let html = ""
            + "<div class=\"" + classCommon + "\"><b>Status:</b> <code>" + _data.Status + "</code></div>"
            + "<div class=\"" + classCommon + "\"><b>QueuedAction:</b> <code>" + _data.QueuedAction + "</code></div>"
            + "<div class=\"" + classCommon + "\"><b>DueTime:</b> <code>" + dueTimeString + "</code></div>"
            + "<div class=\"" + classCommon + "\"><b>Remaining:</b> <code>" + remainingTimeString + "</code></div>"
            + "<div class=\"" + classCommon + "\"><b>AppSide:</b> <code>" + _data.AppSide + "</code></div>";

        return html;
    },

    constructFileItemHtml: function (_fileItem, _noColor = false) {
        let colorClass = "";

        if (!_noColor) {
            switch (_fileItem.Status) {
                case UPDATER_FILE_STATUS_NO_CHANGE:
                    colorClass = "text-success ";
                    break;
                case UPDATER_FILE_STATUS_UPDATE:
                    colorClass = "text-warning ";
                    break;
                case UPDATER_FILE_STATUS_ERROR:
                    colorClass = "text-danger ";
                    break;
            }
        }

        let hiddenAttr = " hidden";
        if (this.m_DebugHashCode)
            hiddenAttr = "";

        let path = _fileItem.Path;
        if (path != "" && !path.endsWith("/"))
            path += "/";

        let html = "<div class=\"d-flex " + colorClass + "me-2\">"
            + "<div class=\"text-end me-1\" style=\"width:18ch\"" + hiddenAttr + ">" + _fileItem.HashCode + "</div>"
            + "<div class=\"text-break me-1\"><span class=\"fw-light\">" + path + "</span>" + _fileItem.File.name + "</div>"
            + "<div class=\"text-break text-end fs-6 ms-auto\">" + _fileItem.LastMod + "</div>"
            + "<div class=\"text-end text-nowrap ms-1\" style=\"width:11ch\">" + P24Utils.formatDataLength(_fileItem.File.size) + "</div>"
            + "</div>";

        return html;
    },

    constructUploadBlockHtml: function (_blockId, _blockText, _status, _tooltipText = null) {
        let blockClass = "alert ";

        // TODO; construct svgContent;
        switch (_status) {
            case UPDATER_BATCH_STATUS_NOT_STARTED:
                blockClass += "alert-secondary";
                break;
            case UPDATER_BATCH_STATUS_IN_PROGRESS:
                blockClass += "alert-warning";
                break;
            case UPDATER_BATCH_STATUS_SUCCESS:
                blockClass += "alert-success";
                break;
            case UPDATER_BATCH_STATUS_ERROR:
                blockClass += "alert-danger";
                break;
            default:
                blockClass += "alert-info";
                break;
        }

        blockClass += " m-1 py-1 px-3";

        let tooltipHtml = "";
        if (_tooltipText != null)
            tooltipHtml = " data-toggle=\"tooltip\" data-placement=\"bottom\" title=\"" + _tooltipText + "\"";

        let html = "<div id=\"" + _blockId + "\" class=\"" + blockClass + "\"" + tooltipHtml + ">"
            + this.constructUploadBlockSvg(_status)
            + _blockText
            + "</div>";

        return html;
    },

    constructUploadBlockSvg: function (_status) {
        let bsIconClass = "";

        switch (_status) {
            case UPDATER_BATCH_STATUS_IN_PROGRESS:
                bsIconClass = "box-arrow-up";
                break;
            case UPDATER_BATCH_STATUS_SUCCESS:
                bsIconClass = "check-lg";
                break;
            case UPDATER_BATCH_STATUS_ERROR:
                bsIconClass = "x-lg";
                break;
        }

        let html = "<svg width=\"16\" height=\"16\" fill=\"currentColor\">"
            + "<use xlink:href=\"/lib/bootstrap-icons/bootstrap-icons.svg#" + bsIconClass + "\" />"
            + "</svg>";

        return html;
    },

    clearUploadPanel: function () {
        this.m_LblInputFiles.html(P24Localization[LOCL_DESC_UPDATER_INPUTFILES_INITIAL]);
        this.m_DivLstFilesUpl.html("");
        this.m_DivUploadStatPanel.html("");

        this.m_DivUploadStatMetadata = null;
        this.m_DivUploadStatBatches = null;

        this.m_BtnClear.attr("disabled", true);
        this.m_BtnUpload.attr("disabled", true);
    },

    // ==================================================

    openConfirmationModal: function (_content, _actionName0, _actionName1) {
        Modal.Common.openTwoBtnModal(
            P24Localization[LOCL_STR_CONFIRM],
            _content,
            MODAL_ICON_QUESTION,
            P24Localization[LOCL_STR_CONFIRM], _actionName0,
            P24Localization[LOCL_STR_CANCEL], _actionName1,
            false);
    },

    computePanelVerBtnStatus: function (_blockName) {
        let samePQ = 0;
        let sameAQ = 0;
        let crosAQ = 0;

        let status = UpdaterPage.Data.PageData.Status;
        

        // If any Purge task or Apply task is in progress: No action is available.
        switch (status) {
            case UPDATER_STATUS_PREV_PURGE_RUNNING:
            case UPDATER_STATUS_NEXT_PURGE_RUNNING:
            case UPDATER_STATUS_PREV_APPLY_RUNNING:
            case UPDATER_STATUS_NEXT_APPLY_RUNNING:
                return [false, false, false];
        }

        let btnApplyStatus = true;

        switch (_blockName) {
            case UPDATER_BLOCK_NAME_PREV:
                samePQ = UPDATER_STATUS_PREV_PURGE_QUEUED;
                sameAQ = UPDATER_STATUS_PREV_APPLY_QUEUED;
                crosAQ = UPDATER_STATUS_NEXT_APPLY_QUEUED;

                if (UpdaterPage.Data.PageData.PrevVer.includes("same"))
                    btnApplyStatus = false;
                break;

            case UPDATER_BLOCK_NAME_NEXT:
                samePQ = UPDATER_STATUS_NEXT_PURGE_QUEUED;
                sameAQ = UPDATER_STATUS_NEXT_APPLY_QUEUED;
                crosAQ = UPDATER_STATUS_PREV_APPLY_QUEUED;

                if (UpdaterPage.Data.PageData.NextVer.includes("same") || UpdaterPage.Data.PageData.NextVer.includes("outdated"))
                    btnApplyStatus = false;
                break;
        }

        // If this side's Purge task is queued: Abort is available.
        if (status == samePQ)
            return [false, true, false];

        // If this side's Apply task is queued: Abort and Purge is available.
        if (status == sameAQ)
            return [true, true, false];

        // If opposite side's Apply task is queued: No action on this side is available.
        if (status == crosAQ)
            return [false, false, false];

        // Else: Purge and Apply is available.
        return [true, false, btnApplyStatus];
    },

};


// ==================================================
$(function () {
    UpdaterPage.init();
    UpdaterPage.reload();

});

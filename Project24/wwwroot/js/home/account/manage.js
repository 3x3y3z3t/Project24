/*  home/account/manage.js
 *  Version: v1.1 (2024.01.02)
 *  Spec:    v0.1
 *
 *  Contributor
 *      Arime-chan (Author)
 */

window.AccManagePage = {
    Data: null,
    UI: null,

    m_Id: null,
    m_AwaitingData: false,


    init: function () {
        this.Data.init();
        this.UI.init();
    },

    reload: function () {
        this.ajax_fetchPageData();
    },

    // ==================================================

    ajax_fetchPageData: function () {
        if (this.m_AwaitingData)
            return;

        let searchParams = new URLSearchParams(window.location.search);
        let id = searchParams.get("_id");

        if (id == null || id == "") {
            this.UI.showErrorIdEmpty();
            return;
        }

        this.m_Id = id;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "Manage?handler=FetchPageData&_id=" + id,
            success: function (_content, _textStatus, _xhr) { AccManagePage.ajax_fetchPageData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { AccManagePage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_updateRole: function (_data) {
        if (this.m_AwaitingData)
            return;

        let data = JSON.stringify(_data);
        if (data == "{}") {
            console.log("Nothing to update.");
            return;
        }

        this.m_AwaitingData = true;
        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "Manage?handler=UpdateRole",
            headers: { RequestVerificationToken: token },
            data: data,
            cache: false,
            contentType: "application/json; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { AccManagePage.ajax_updateRole_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { AccManagePage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    // ==================================================

    ajax_error: function (_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },

    ajax_fetchPageData_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        if (!P24Utils.Ajax.successContentCheckCommon({ Content: _content, Error: { Check: false } }))
            return;

        let body = _content.substring(6);

        if (_content.startsWith(P24_MSG_TAG_ERROR)) {
            if (body == "")
                this.UI.showErrorIdEmpty();
            else
                this.UI.showErrorIdNotFound(body);

            return;
        }

        let processedData = this.Data.processPageData(body);
        if (processedData == null)
            return;

        this.UI.refreshPage(processedData);
    },

    ajax_updateRole_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        if (!P24Utils.Ajax.successContentCheckCommon({ Content: _content }))
            return;

        let body = _content.substring(6);
        let result = JSON.parse(body);

        this.Data.refreshModifies(result);
        this.UI.refreshRoleModifiesHighlight(result);
    },



};

AccManagePage.Data = {
    PageData: null,
    RoleModState: null,


    init: function () {

    },

    // ==================================================

    processPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        if (parsedData.User.AddedDate != null)
            parsedData.User.AddedDate = new Date(parsedData.User.AddedDate);

        if (parsedData.User.RemovedDate != null)
            parsedData.User.RemovedDate = new Date(parsedData.User.RemovedDate);

        //console.log(parsedData);
        this.PageData = parsedData;
        this.RolesModState = parsedData.Roles;

        return parsedData;
    },

    refreshModifies: function (_resultList) {
        for (const role in _resultList) {
            if (_resultList[role]) {
                this.PageData.Roles[role] = this.RolesModState[role];
            }
        }
    },
};

AccManagePage.UI = {
    m_DivMain: null,
    m_DivError: null,
    m_DivAccDetails: null,
    m_DivAccAccess: null,

    m_RowStyles: [],


    init: function () {
        this.m_DivError = $("#div-error");
        this.m_DivMain = $("#div-main");
        this.m_DivAccDetails = $("#div-acc-details");
        this.m_DivAccAccess = $("#div-acc-access");

        this.m_RowStyles = ["10em", "20em", "15em", "15em", "15em"];
        for (let i = 0; i < this.m_RowStyles.length; ++i) {
            $("#tr" + i).css("width", this.m_RowStyles[i]);
        }

        /*
        
    <div class="d-flex flex-nowrap">
        <div id="tr0" class="" style="width:10em">ID</div>
        <div id="tr1" class="" style="width:20em;min-width:20em">Username</div>
        <div id="tr2" class="" style="width:10em">Access</div>
        <div id="tr3" class="" style="max-width:20em;min-width:20em">Joined</div>
        <div id="tr4" class="" style="max-width:20em;min-width:20em">Left</div>
    </div>

        */

    },

    // ==================================================

    refreshPage: function (_data) {
        let html = "";

        // TODO: do a lot of work here..





        this.m_DivAccDetails.html(this.constructAccDetailsHtml(_data.User));
        this.m_DivAccAccess.html(this.constructAccAccessHtml(_data.Roles));


        //this.m_DivMain.html(html);
    },

    refreshRoleModifiesHighlight: function (_resultList) {
        for (const role in _resultList) {
            let id = "check-" + role.toLowerCase().replaceAll("/", "-");
            let element = $("#" + id);

            if (element == null) {
                console.warn("Element does not exist: " + id);
                continue;
            }

            if (_resultList[role]) {
                P24Utils.Visual.refreshHighlightSingleItem(element.parent().parent(), CONFIG_PANEL_ITEM_STATUS_SAVED);
            } else {
                P24Utils.Visual.refreshHighlightSingleItem(element.parent().parent(), CONFIG_PANEL_ITEM_STATUS_ERROR);
            }
        }
    },

    showErrorIdEmpty: function () {
        this.m_DivError.html("Id is null.");
        this.m_DivError.removeAttr("hidden");

        this.m_DivMain.attr("hidden", true);
    },

    showErrorIdNotFound: function (_id) {
        this.m_DivError.html("Id <code>" + _id + "</code> not found.");
        this.m_DivError.removeAttr("hidden");

        this.m_DivMain.attr("hidden", true);
    },

    // ==================================================

    btnSave_onclick: function () {
        let modified = {};

        let rolesOrgState = AccManagePage.Data.PageData.Roles;
        for (const role in rolesOrgState) {
            let id = "check-" + role.toLowerCase().replaceAll("/", "-");
            let element = $("#" + id);

            if (element == null) {
                console.warn("Element does not exist: " + id);
                continue;
            }

            P24Utils.Visual.refreshHighlightSingleItem(element.parent().parent(), CONFIG_PANEL_ITEM_STATUS_NO_CHANGE);

            let newState = element.prop("checked");
            if (newState == rolesOrgState[role])
                continue;

            modified[role] = newState;
            AccManagePage.Data.RolesModState[role] = newState;
        }

        AccManagePage.ajax_updateRole({ User: { Id: AccManagePage.m_Id }, Roles: modified });
    },

    changeRecord: function (_id) {
        let orgState = null;

        let rolesOrgState = AccManagePage.Data.PageData.Roles;
        for (const role in rolesOrgState) {
            if ("check-" + role.toLowerCase().replaceAll("/", "-") == _id)
                orgState = rolesOrgState[role];
        }
        let element = $("#" + _id);

        if (orgState == null || element == null) {
            console.warn("Invalid id: " + _id);
            return;
        }

        if (element.prop("checked") == orgState) {
            P24Utils.Visual.refreshHighlightSingleItem(element.parent().parent(), CONFIG_PANEL_ITEM_STATUS_NO_CHANGE);
        } else {
            P24Utils.Visual.refreshHighlightSingleItem(element.parent().parent(), CONFIG_PANEL_ITEM_STATUS_MODIFIED);
        }
    },

    // ==================================================

    constructAccDetailsHtml: function (_user) {
        let html = "";

        html += "<div>Id: " + _user.Id + "</div>";
        html += "<div>Username: " + _user.Username + "</div>";

        html += "<div>AddedDateTime: ";
        if (_user.AddedDate != null)
            html += DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm:ss", _user.AddedDate);
        else html += "null";
        html += "</div>";

        html += "<div>RemovedDateTime: ";
        if (_user.RemovedDate != null)
            html += DotNetString.formatCustomDateTime("yyyy/MM/dd HH:mm:ss", _user.RemovedDate);
        else
            html += "null";
        html += "</div>";

        html += "<div>Email: " + _user.Email;
        if (_user.EmailConfirmed)
            html += " (confirmed)";
        html += "<div>";

        html += "<div>Name: " + _user.DisplayName + "</div>";

        html += "<div>PhoneNumber: " + _user.PhoneNumber;
        if (_user.PhoneNumberConfirmed)
            html += " (confirmed)";
        html += "</div>";

        return html;
    },

    constructAccAccessHtml: function (_roles) {
        let html = "";

        //_roles.sort();
        for (const role in _roles) {
            let id = "check-" + role.toLowerCase().replaceAll("/", "-");

            html += "<div class=\"border-2\" style=\"padding-left:2px!important;padding-right:2px!important;\"><div class=\"form-check form-switch ms-2\">"
                + "<input id=\"" + id + "\" type=\"checkbox\" class=\"form-check-input\" onchange=\"AccManagePage.UI.changeRecord('" + id + "')\" ";
            if (_roles[role])
                html += "checked";

            html += "/>"
                + "<label for=\"" + id + "\" class=\"form-check-label\">" + role + "</label>"
                + "</div></div>";
        }

        return html;
    },

    //constructSingleAccountHtml: function (_accData) {
    //    const dateFormatString = "yyyy.MM.dd";

    //    let accessCount = _accData.Access + " page";
    //    if (_accData.Access > 1)
    //        accessCount += "s";

    //    let removedDateTime = "-";
    //    if (_accData.RemovedDateTime != null)
    //        removedDateTime = DotNetString.formatCustomDateTime(dateFormatString, _accData.RemovedDateTime);

    //    let html = "<div class=\"d-flex flex-nowrap\">"
    //        + "<div style=\"width:" + this.m_RowStyles[0] + "\"><a href>" + _accData.Id.substring(0, 8) + "</a></div>"
    //        + "<div style=\"width:" + this.m_RowStyles[1] + "\"><a href>" + _accData.UserName + "</a></div>"
    //        + "<div style=\"width:" + this.m_RowStyles[2] + "\">" + accessCount + "</div>"
    //        + "<div style=\"width:" + this.m_RowStyles[3] + "\">" + DotNetString.formatCustomDateTime(dateFormatString, _accData.AddedDateTime) + "</div>"
    //        + "<div style=\"width:" + this.m_RowStyles[4] + "\">" + removedDateTime + "</div>"
    //        + "</div>";

    //    return html;
    //},
};


// ==================================================
$(function () {
    AccManagePage.init();
    AccManagePage.reload();
});

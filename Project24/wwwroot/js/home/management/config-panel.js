/*  home/management/config-panel.js
    Version: v1.0 (2023.09.02)

    Author
        Arime-chan
 */

// ==================================================
// Item Status Code;

const CONFIG_PANEL_TAB_STATUS_NO_CHANGE = P24_TEXT_COLOR_NORMAL;
const CONFIG_PANEL_TAB_STATUS_MODIFIED = P24_TEXT_COLOR_YELLOW;
const CONFIG_PANEL_TAB_STATUS_SAVED = P24_TEXT_COLOR_GREEN;
const CONFIG_PANEL_TAB_STATUS_ERROR = P24_TEXT_COLOR_RED;

const CONFIG_PANEL_ITEM_STATUS_NO_CHANGE = "CONFIG_PANEL_ITEM_STATUS_NO_CHANGE";
const CONFIG_PANEL_ITEM_STATUS_MODIFIED = "CONFIG_PANEL_ITEM_STATUS_MODIFIED";
const CONFIG_PANEL_ITEM_STATUS_SAVED = "CONFIG_PANEL_ITEM_STATUS_SAVED";
const CONFIG_PANEL_ITEM_STATUS_ERROR = "CONFIG_PANEL_ITEM_STATUS_ERROR";

// ==================================================
// Trackable Value Type;

const TRACKABLE_VALUE_TYPE_INT32 = "TRACKABLE_VALUE_TYPE_INT32";
const TRACKABLE_VALUE_TYPE_STRING = "TRACKABLE_VALUE_TYPE_STRING";
const TRACKABLE_VALUE_TYPE_SELECT = "TRACKABLE_VALUE_TYPE_SELECT";

// ==================================================
// Trackable Value Range Type;

const TRACKABLE_VALUE_RANGE_TYPE_RANGE = "TRACKABLE_VALUE_RANGE_TYPE_RANGE";
const TRACKABLE_VALUE_RANGE_TYPE_LIST = "TRACKABLE_VALUE_RANGE_TYPE_LIST";

// ==================================================


class ConfigPanelTabData {
    //Status = null;

    Name = null;
    Items = null;


    constructor(_name) {
        //this.Status = CONFIG_PANEL_TAB_STATUS_NO_CHANGE;
        this.Name = _name;
        this.Items = [];
    }

    isTabModified() {
        for (const key in this.Items) {
            if (this.Items[key].Status == CONFIG_PANEL_ITEM_STATUS_MODIFIED)
                return true;
        }

        return false;
    }
}

class ConfigPanelItemData {
    Status = null;

    Key = null;
    Value = null;

    ValueType = null;
    ValueRangeType = null;
    ValueRange = null;

    TabName = null;

    //constructor(_key, _value, _valueType, _valueRangeType = null, _valueRange = null) {
    //    Status = CONFIG_PANEL_ITEM_STATUS_NO_CHANGE;

    //    this.Key = _key;
    //    this.Value = _value

    //    this.ValueType = _valueType;
    //    this.ValueRangeType = _valueRangeType;
    //    this.ValueRange = _valueRange;
    //}

    constructor(_object) {
        this.Status = CONFIG_PANEL_ITEM_STATUS_NO_CHANGE;

        this.Key = _object.Key;
        this.Value = _object.Value;

        this.ValueType = _object.ValueType;
        this.ValueRangeType = _object.ValueRangeType;
        this.ValueRange = _object.ValueRange;

        this.TabName = _object.TabName;
    }


    #processValueRange() {
        if (this.ValueRangeType == TRACKABLE_VALUE_TYPE_INT32) {

            this.ValueRange = JSON.parse(this.ValueRange);


            return;
        }

        // TODO: error;
    }


}



window.ConfigPanelPage = {
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

    ajax_fetchPageData: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "ConfigPanel?handler=FetchPageData",
            success: function (_content, _textStatus, _xhr) { ConfigPanelPage.ajax_fetchPageData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { ConfigPanelPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajaxPost_submitChanges: function (_data) {
        if (this.m_AwaitingData)
            return;
        if (this.Data == null || !this.Data.hasChanges())
            return;

        this.m_AwaitingData = true;

        let token = $("input[name='__RequestVerificationToken']").val();

        $.ajax({
            type: "POST",
            url: "ConfigPanel?handler=SubmitChanges",
            headers: { RequestVerificationToken: token },
            data: JSON.stringify(_data),
            contentType: "application/json; charset=utf-8",
            success: function (_content, _textStatus, _xhr) { ConfigPanelPage.ajaxPost_submitChanges_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { ConfigPanelPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_error(_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
    },

    ajax_fetchPageData_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (this.ajaxSuccessContentCheckCommon(_content, body)) {
            let processedData = this.Data.loadPageData(body);
            if (processedData == null)
                return;

            this.UI.refreshPage(processedData);

            return;
        }
    },

    ajaxPost_submitChanges_success: function (_content, _textStatus, _xhr) {
        this.m_AwaitingData = false;

        let body = _content.substring(6);

        if (this.ajaxSuccessContentCheckCommon(_content, body)) {
            let isLocaleChanged = false;

            if (body == "") {
                let pairs = this.Data.getChangedDataAsKeyValuePairs();
                for (let i = 0; i < pairs.length; ++i) {
                    let pair = pairs[i];
                    this.Data.WorkingData[pair.Key].Status = CONFIG_PANEL_ITEM_STATUS_NO_CHANGE;
                    this.UI.refreshHighlightSingleItem(pair.Key);
                }
            } else {
                let changedKeys = JSON.parse(body);
                for (let i = 0; i < changedKeys.length; ++i) {
                    let key = changedKeys[i];
                    if (key == "CONFIG_GLOBAL_LOCALIZATION") {
                        // locale has changed, so we reload the page anyway, so no need to refresh page (save client's resource);
                        Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_INFO], P24Localization[LOCL_DESC_CFG_LOCALE_CHANGED], MODAL_ICON_INFO, "OK", "location.reload()");
                        return;
                    }

                    this.Data.WorkingData[key].Status = CONFIG_PANEL_ITEM_STATUS_SAVED;
                    this.Data.OriginalData[key].Value = this.Data.WorkingData[key].Value;

                    this.UI.refreshHighlightSingleItem(key);
                }
            }

            this.UI.refreshHighlightAllTabs();

            this.UI.BtnSave.attr("disabled", true);
            return;
        }
    },

    ajaxSuccessContentCheckCommon: function (_content, _body) {
        if (_content.startsWith(P24_MSG_TAG_ERROR)) {
            Modal.Common.openOneBtnModal(P24Localization.get(LOCL_STR_FAIL), _body, MODAL_ICON_ERROR);
            return false;
        }

        if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
            Modal.Common.openOneBtnModal(P24Localization.get(LOCL_STR_EXCEPTION), "<pre>" + HtmlUtils.escape(_body) + "</pre>");
            return false;
        }

        if (!_content.startsWith(P24_MSG_TAG_SUCCESS)) {
            Modal.Common.openOneBtnModal(P24Localization.get(LOCL_STR_UNKNOWN_ERR), "<pre>" + HtmlUtils.escape(_content) + "</pre>", MODAL_ICON_ERROR);
            return false;
        }

        return true;
    },

    // ==================================================

    btnReload_onclick: function () {
        this.ajax_fetchPageData();
    },

    btnSave_onclick: function () {
        let data = this.Data.getChangedDataAsKeyValuePairs();
        // TODO: perform data validation;

        this.ajaxPost_submitChanges(data);
    },
};

ConfigPanelPage.Data = {
    WorkingData: null,
    OriginalData: null,
    StructuredData: null,

    init: function () {
        this.WorkingData = {};
        this.OriginalData = {};
        this.StructuredData = {};
    },

    loadPageData: function (_json) {
        let parsedData = JSON.parse(_json);

        this.init();

        let currentTabName = null;
        let tabData = null;

        for (let i = 0; i < parsedData.length; ++i) {
            if (parsedData[i].TabName != currentTabName) {
                if (tabData != null)
                    this.StructuredData[currentTabName] = tabData;

                currentTabName = parsedData[i].TabName;
                tabData = new ConfigPanelTabData(currentTabName);
            }

            let itemKey = parsedData[i].Key;

            this.OriginalData[itemKey] = new ConfigPanelItemData(parsedData[i]);
            this.WorkingData[itemKey] = new ConfigPanelItemData(parsedData[i]);
            tabData.Items[itemKey] = this.WorkingData[itemKey];
        }
        this.StructuredData[currentTabName] = tabData;

        return this.StructuredData;
    },

    getTab: function (_tabName) {
        return this.StructuredData[_tabName];
    },

    getChangedDataAsKeyValuePairs: function () {
        let data = [];
        for (const key in this.WorkingData) {
            if (this.WorkingData[key].Status == CONFIG_PANEL_ITEM_STATUS_NO_CHANGE && false)
                continue;

            data.push({
                Key: key,
                Value: this.WorkingData[key].Value
            });
        }

        return data;
    },

    hasChanges: function () {
        for (const key in this.WorkingData) {
            if (this.WorkingData[key].Status != CONFIG_PANEL_ITEM_STATUS_NO_CHANGE)
                return true;
        }
    },





};

ConfigPanelPage.UI = {
    BtnSave: null,

    m_DivMain: null,


    init: function () {
        this.m_DivMain = $("#div-main");
        this.BtnSave = $("#btn-save");
    },

    refreshPage: function (_pageData) {
        let html = "";

        for (const key in _pageData) {
            html += this.constructTabHtml(_pageData[key]);
        }

        this.m_DivMain.html(html);

        this.BtnSave.attr("disabled", true);
        //this.BtnSave.removeAttr("disabled"); // TODO: remove this;
    },

    refreshHighlightSingleItem: function (_itemKey) {
        let item = ConfigPanelPage.Data.WorkingData[_itemKey];

        let itemElm = $("#div-item-" + _itemKey);
        if (item.Status == CONFIG_PANEL_ITEM_STATUS_NO_CHANGE) {
            itemElm.removeClass("border-success border-danger border-warning border-start border-end");
            itemElm.css({
                "padding-left": "2px",
                "padding-right": "2px"
            });
        }
        else if (item.Status == CONFIG_PANEL_ITEM_STATUS_MODIFIED) {
            itemElm.removeClass("border-success border-danger");
            itemElm.addClass("border-warning border-start border-end");
            itemElm.css({
                "padding-left": "0px",
                "padding-right": "0px"
            });
        }
        else if (item.Status == CONFIG_PANEL_ITEM_STATUS_SAVED) {
            itemElm.removeClass("border-warning border-danger");
            itemElm.addClass("border-success border-start border-end");
            itemElm.css({
                "padding-left": "0px",
                "padding-right": "0px"
            });
        }
    },

    refreshHighlightAllTabs: function () {
        for (const key in ConfigPanelPage.Data.StructuredData) {
            this.refreshHighlightSingleTab(key);
        }
    },

    refreshHighlightSingleTab: function (_tabName) {
        let tab = ConfigPanelPage.Data.getTab(_tabName);

        let tabElm = $("#div-tab-" + _tabName);
        if (tab.isTabModified()) {
            tabElm.removeClass("border-success border-danger border-primary");
            tabElm.addClass("border-warning");
        } else {
            tabElm.removeClass("border-success border-danger border-warning");
            tabElm.addClass("border-primary");
        }
    },

    // ==================================================

    input_onchange: function (_itemKey) {
        let item = ConfigPanelPage.Data.WorkingData[_itemKey];

        // ========== get modified value ==========;
        let value = null;
        if (item.ValueType == TRACKABLE_VALUE_TYPE_INT32) {
            value = $("#input-" + _itemKey)[0].value;
        } else if (item.ValueType == TRACKABLE_VALUE_TYPE_SELECT) {
            value = $("#select-" + _itemKey).val();
        }

        // ========== write modified value ==========;
        ConfigPanelPage.Data.WorkingData[_itemKey].Value = value;

        // ========== check modified value against original value ==========;
        if (value == ConfigPanelPage.Data.OriginalData[_itemKey].Value) {
            ConfigPanelPage.Data.WorkingData[_itemKey].Status = CONFIG_PANEL_ITEM_STATUS_NO_CHANGE;
        } else {
            ConfigPanelPage.Data.WorkingData[_itemKey].Status = CONFIG_PANEL_ITEM_STATUS_MODIFIED;
        }

        let tabName = ConfigPanelPage.Data.WorkingData[_itemKey].TabName;

        this.refreshHighlightSingleItem(_itemKey);
        this.refreshHighlightSingleTab(tabName);

        // ========== enable/disable save button ==========;
        if (ConfigPanelPage.Data.hasChanges()) {
            this.BtnSave.removeAttr("disabled");
        } else {
            this.BtnSave.attr("disabled", true);
        }
    },

    // ==================================================

    constructTabHtml: function (_tab) {
        let html = "<div id=\"div-tab-" + _tab.Name + "\" class=\"border-start border-primary border-3 col-12 col-lg-10 col-xl-8 my-2 p-2\">"
            + "<h6 class=\"mx-2 mb-2\">" + _tab.Name + "</h6>";

        for (const key in _tab.Items) {
            html += this.constructSingleItemHtml(_tab.Items[key]);
        }

        html += "</div>";

        return html;
    },

    constructSingleItemHtml: function (_item) {
        let html = "<div id=\"div-item-" + _item.Key + "\" class=\"border-2\" style=\"padding-left:2px!important; padding-right:2px!important\">"

            + "<div class=\"d-flex flex-wrap px-2\">"
            + "<label for=\"input-" + _item.Key + "\" class=\"d-flex flex-wrap text-break col-form-label col-6 px-2\"><code>" + _item.Key + "</code> <span class=\"ms-auto\">=</span></label>"
            + this.constructInputHtml(_item)
            + "</div>"

            + "</div>";

        return html;
    },

    constructInputHtml: function (_item) {
        let html = "<div class=\"col-6\">";

        if (_item.ValueType == TRACKABLE_VALUE_TYPE_INT32) {
            html += this.constructInputHtmlForInput(_item, "number");
        }
        else if (_item.ValueType == TRACKABLE_VALUE_TYPE_SELECT) {
            html += this.constructInputHtmlForSelect(_item);
        }
        else {
            // no type, just display value;
            html += "<label class=\"col-form-label\"><code>" + _item.Value + "</code></label>";
        }



        html += "</div>";

        return html;
    },

    constructInputHtmlForInput: function (_item, _inputType) {
        let html = "<input id=\"input-" + _item.Key + "\" class=\"form-control\" type=\"" + _inputType + "\" ";

        if (_item.ValueRangeType == TRACKABLE_VALUE_RANGE_TYPE_RANGE)
            html += "min =\"" + _item.ValueRange[0] + "\" max=\"" + _item.ValueRange[1] + "\" ";

        html += "value =\"" + _item.Value + "\" oninput=\"ConfigPanelPage.UI.input_onchange('" + _item.Key + "')\" /> ";

        return html;
    },

    constructInputHtmlForSelect: function (_item) {
        let html = "<select id=\"select-" + _item.Key + "\" class=\"form-select\" onchange=\"ConfigPanelPage.UI.input_onchange('" + _item.Key + "')\">";

        for (let i = 0; i < _item.ValueRange.length; ++i) {
            let selected = "";
            if (_item.ValueRange[i] == _item.Value) {
                selected = " selected";
            }

            html += "<option value=\"" + _item.ValueRange[i] + "\"" + selected + ">" + _item.ValueRange[i] + "</option>";
        }

        html += "</select>";

        return html;
    },



    // TODO: add more input type;



};


$(document).ready(function () {
    ConfigPanelPage.init();
    ConfigPanelPage.reload();






});

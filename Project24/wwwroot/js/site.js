/*  site.js
 *  Version: v1.10 (2024.01.02)
 *  Spec:    v0.1
 *
 *  Contributor
 *      Arime-chan (Author)
 */

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Project24 allows maximum 32 MiB per upload;
const P24_MAX_UPLOAD_SIZE = 32 * 1024 * 1024;
const P24_MAX_UPLOAD_COUNT = 1024; // ASP.NET Core impose a limit on 1024 files per request;

const P24_TEXT_COLOR_NORMAL = 1;
const P24_TEXT_COLOR_RED = 2;
const P24_TEXT_COLOR_GREEN = 3;
const P24_TEXT_COLOR_BLUE = 4;
const P24_TEXT_COLOR_YELLOW = 5;

const P24_MSG_TAG_INFO = "<info>";
const P24_MSG_TAG_SUCCESS = "<done>";
const P24_MSG_TAG_WARNING = "<warn>";
const P24_MSG_TAG_ERROR = "<fail>";
const P24_MSG_TAG_CRITICAL = "<crit>";
const P24_MSG_TAG_EXCEPTION = "<excp>";

// ==================================================

const P24_ARG_DATA_TYPE_STRING = "AnnouncementArgDataString";
const P24_ARG_DATA_TYPE_DATETIME = "AnnouncementArgDataDateTime";
const P24_ARG_DATA_TYPE_TIMESPAN = "AnnouncementArgDataTimeSpan";

// ==================================================

const CONFIG_PANEL_ITEM_STATUS_NO_CHANGE = "CONFIG_PANEL_ITEM_STATUS_NO_CHANGE";
const CONFIG_PANEL_ITEM_STATUS_MODIFIED = "CONFIG_PANEL_ITEM_STATUS_MODIFIED";
const CONFIG_PANEL_ITEM_STATUS_SAVED = "CONFIG_PANEL_ITEM_STATUS_SAVED";
const CONFIG_PANEL_ITEM_STATUS_ERROR = "CONFIG_PANEL_ITEM_STATUS_ERROR";

// ==================================================

const BTN_CLASS_PRIMARY = "btn-primary";
const BTN_CLASS_SECONDARY = "btn-secondary";
const BTN_CLASS_WARNING = "btn-warning";
const BTN_CLASS_DANGER = "btn-danger";
const BTN_CLASS_INFO = "btn-info";

// ==================================================

var HttpStatusCodeName = {
    '200': 'OK',
    '201': 'Created',
    '202': 'Accepted',
    '203': 'Non-Authoritative Information',
    '204': 'No Content',
    '205': 'Reset Content',
    '206': 'Partial Content',
    '300': 'Multiple Choices',
    '301': 'Moved Permanently',
    '302': 'Found',
    '303': 'See Other',
    '304': 'Not Modified',
    '305': 'Use Proxy',
    '306': 'Unused',
    '307': 'Temporary Redirect',
    '400': 'Bad Request',
    '401': 'Unauthorized',
    '402': 'Payment Required',
    '403': 'Forbidden',
    '404': 'Not Found',
    '405': 'Method Not Allowed',
    '406': 'Not Acceptable',
    '407': 'Proxy Authentication Required',
    '408': 'Request Timeout',
    '409': 'Conflict',
    '410': 'Gone',
    '411': 'Length Required',
    '412': 'Precondition Required',
    '413': 'Request Entry Too Large',
    '414': 'Request-URI Too Long',
    '415': 'Unsupported Media Type',
    '416': 'Requested Range Not Satisfiable',
    '417': 'Expectation Failed',
    '418': 'I\'m a teapot',
    '429': 'Too Many Requests',
    '500': 'Internal Server Error',
    '501': 'Not Implemented',
    '502': 'Bad Gateway',
    '503': 'Service Unavailable',
    '504': 'Gateway Timeout',
    '505': 'HTTP Version Not Supported',
};


$(function () {
    P24Utils.reloadAllTooltips();
});


// =====
window.P24Utils = {
    Ajax: null,


    svg: function (_bsIconName, _colorClass = null, _size = 16) {
        let html = "<svg width=\"" + _size + "\" height=\"" + _size + "\" fill=\"currentColor\"";
        if (_colorClass != null)
            html += " class=\"" + _colorClass + "\"";
        html += "><use xlink:href=\"/lib/bootstrap-icons/bootstrap-icons.svg#" + _bsIconName + "\" /></svg>";

        return html;
    },

};


P24Utils.Ajax = {
    error: function (_xhr, _textStatus, _errorThrown) {
        if (window.Modal == null) {
            let msg = "Ajax request error:\n"
                + "    Status Code:    " + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "\n"
                + "    jq Status:      " + _textStatus + "\n"
                + "    Message:        " + _errorThrown.message;

            console.error(msg);
        } else {
            let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div>"
                + "<div>jq Status: <code>" + _textStatus + "</code></div>"
                + "<div>Message: " + _errorThrown.message + "</div>";

            Modal.openModal({
                TitleHtml: "Ajax request error",
                Content: body,
                IconData: Modal.IconData.Error
            });   
        }
    },

    /**
     * 
     * @param {any} _data
     *  {
     *      Content:
     *      Info: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default true
     *      }
     *      Success: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default true
     *      }
     *      Warning: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default false
     *      }
     *      Error: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default false
     *      }
     *      Critical: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default true
     *      }
     *      Exception: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default false
     *      }
     *      Unknown: {
     *          Check:          {boolean}   default true
     *          Return:         {boolean}   default false
     *      }
     *  }
     */
    successContentCheckCommon: function (_data) {
        _data = this.validateDataObject(_data);
        if (_data == null)
            return false;

        switch (_data.Tag) {
            case P24_MSG_TAG_INFO:
                if (_data.Info.Check) {
                    console.info(_data.Body);
                    return _data.Info.Return;
                }
                return true;

            case P24_MSG_TAG_SUCCESS:
                if (_data.Success.Check) {

                    return _data.Success.Return;
                }
                return true;

            case P24_MSG_TAG_WARNING:
                if (_data.Warning.Check) {
                    this.successContentCheckCommonInternal({
                        Body: _data.Body,
                        ConsoleLogFunc: console.warn,
                        ConsoleLogMsg: "Warning:\n" + _data.Body,
                        ModalIconData: Modal.IconData.Warning,
                        ModalTitle: P24Localization[LOCL_STR_WARN],
                    });

                    return _data.Warning.Return;
                }
                return true;

            case P24_MSG_TAG_ERROR:
                if (_data.Error.Check) {
                    this.successContentCheckCommonInternal({
                        Body: _data.Body,
                        ConsoleLogFunc: console.error,
                        ConsoleLogMsg: "Error:\n" + _data.Body,
                        ModalIconData: Modal.IconData.Error,
                        ModalTitle: P24Localization[LOCL_STR_FAIL],
                    });

                    return _data.Error.Return;
                }
                return true;

            case P24_MSG_TAG_CRITICAL:
                if (_data.Critical.Check) {
                    this.successContentCheckCommonInternal({
                        Body: _data.Body,
                        ConsoleLogFunc: console.error,
                        ConsoleLogMsg: "Critical error:\n" + _data.Body,
                        ModalIconData: Modal.IconData.Error,
                        ModalTitle: P24Localization.get(LOCL_STR_CRIT),
                    });

                    return _data.Critical.Return;
                }
                return true;

            case P24_MSG_TAG_EXCEPTION:
                if (_data.Exception.Check) {
                    this.successContentCheckCommonInternal({
                        Body: "<pre>" + _data.Body + "</pre>",
                        ConsoleLogFunc: console.error,
                        ConsoleLogMsg: "Exception:\n" + HtmlUtils.escape(_data.Body),
                        ModalTitle: P24Localization[LOCL_STR_EXCEPTION],
                    });

                    return _data.Exception.Return;
                }
                return true;
        }

        if (_data.Unknown.Check) {
            this.successContentCheckCommonInternal({
                Body: "<pre>" + _data.Body + "</pre>",
                ConsoleLogFunc: console.error,
                ConsoleLogMsg: "Unknown error:\n" + HtmlUtils.escape(_data.Body),
                ModalTitle: P24Localization[LOCL_STR_UNKNOWN_ERR],
            });

            return _data.Unknown.Return;
        }

        return true;
    },
        
    validateDataObject: function (_data) {
        if (_data == null || _data.Content == null)
            return null;

        if (_data.Info == null)
            _data.Info = { Check: true };
        if (_data.Info.Return == null)
            _data.Info.Return = true;

        if (_data.Success == null)
            _data.Success = { Check: true };
        if (_data.Success.Return == null)
            _data.Success.Return = true;

        if (_data.Warning == null)
            _data.Warning = { Check: true };
        if (_data.Warning.Return == null)
            _data.Warning.Return = false;

        if (_data.Error == null)
            _data.Error = { Check: true };
        if (_data.Error.Return == null)
            _data.Error.Return = false;

        if (_data.Critical == null)
            _data.Critical = { Check: true };
        if (_data.Critical.Return == null)
            _data.Critical.Return = false;

        if (_data.Exception == null)
            _data.Exception = { Check: true };
        if (_data.Exception.Return == null)
            _data.Exception.Return = false;

        if (_data.Unknown == null)
            _data.Unknown = { Check: true };
        if (_data.Unknown.Return == null)
            _data.Unknown.Return = false;

        _data.Tag = _data.Content.substring(0, 6);
        _data.Body = _data.Content.substring(6);

        return _data;
    },

    successContentCheckCommonInternal: function (_data) {
        if (window.Modal == null) {
            _data.ConsoleLogFunc(_data.ConsoleLogMsg);
        } else {
            Modal.openModal({
                Content: _data.Body,
                IconData: _data.ModalIconData,
                TitleHtml: _data.ModalTitle,
            });
        }
    },

    
    //successContentCheckCommon: function (_content, _body) {
    //    if (_content.startsWith(P24_MSG_TAG_ERROR)) {
    //        if (window.Modal == null) {
    //            console.error(P24Localization[LOCL_STR_FAIL] + ":\n" + _body);
    //        } else {
    //            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_FAIL], _body, MODAL_ICON_ERROR);
    //        }

    //        return false;
    //    }

    //    if (_content.startsWith(P24_MSG_TAG_EXCEPTION)) {
    //        if (window.Modal == null) {
    //            console.error(P24Localization[LOCL_STR_EXCEPTION] + ":\n" + HtmlUtils.escape(_body));
    //        } else {
    //            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_EXCEPTION], "<pre>" + _body + "</pre>");
    //        }

    //        return false;
    //    }

    //    if (!_content.startsWith(P24_MSG_TAG_SUCCESS)) {
    //        if (window.Modal == null) {
    //            console.error(P24Localization[LOCL_STR_UNKNOWN_ERR] + ":\n" + _content);
    //        } else {
    //            Modal.Common.openOneBtnModal(P24Localization[LOCL_STR_UNKNOWN_ERR], "<pre>" + HtmlUtils.escape(_content) + "</pre>", MODAL_ICON_ERROR);
    //        }

    //        return false;
    //    }

    //    return true;
    //},


};

//P24Utils.ajax_error = function (_xhr, _textStatus, _errorThrown) {
//    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div>"
//        + "<div>jq Status: <code>" + _textStatus + "</code></div>"
//        + "<div>Message: " + _errorThrown.message + "</div>";
//    Modal.Common.openOneBtnModal("Ajax request error", body, "error", "OK");
//}

// https://stackoverflow.com/a/13382873;
P24Utils.getScrollbarWidth = function () {
    // Creating invisible container
    const outer = document.createElement('div');
    outer.style.visibility = 'hidden';
    outer.style.overflow = 'scroll'; // forcing scrollbar to appear
    outer.style.msOverflowStyle = 'scrollbar'; // needed for WinJS apps
    document.body.appendChild(outer);

    // Creating inner element and placing it in the container
    const inner = document.createElement('div');
    outer.appendChild(inner);

    // Calculating difference between container's full width and the child width
    const scrollbarWidth = (outer.offsetWidth - inner.offsetWidth);

    // Removing temporary elements from the DOM
    outer.parentNode.removeChild(outer);

    return scrollbarWidth;
}

P24Utils.reloadAllTooltips = function () {
    $("[data-toggle=\"tooltip\"]").tooltip();
}

// ==================================================
// datetime

P24Utils.DateTime = {
    toLocal: function (_utcDate) {
        let minuteOffset = new Date(0).getTimezoneOffset();
        _utcDate.setMinutes(_utcDate.getMinutes() - minuteOffset);

        return _utcDate;
    },

    toUtc: function (_localDate) {
        let minuteOffset = new Date(0).getTimezoneOffset();
        _localDate.setMinutes(_localDate.getMinutes() + minuteOffset);

        return _localDate;
    },
};

P24Utils.formatDateString_endsAtMinute = function (_date) {
    let year = _date.getFullYear();
    let month = DotNetString.padZeroesBefore(_date.getMonth() + 1, 2);
    let date = DotNetString.padZeroesBefore(_date.getDate(), 2);
    let hour = DotNetString.padZeroesBefore(_date.getHours(), 2);
    let minute = DotNetString.padZeroesBefore(_date.getMinutes(), 2);
    let second = DotNetString.padZeroesBefore(_date.getSeconds(), 2);

    return year + "/" + month + "/" + date + " " + hour + ":" + minute;
}

P24Utils.formatDateString_endsAtSecond = function (_date) {
    let second = String(_date.getSeconds()).padStart(2, "0");
    return P24Utils.formatDateString_endsAtMinute(_date) + ":" + second;
}

P24Utils.formatDateString = function (_date) {
    let year = _date.getFullYear();
    let month = DotNetString.padZeroesBefore(_date.getMonth() + 1, 2);
    let date = DotNetString.padZeroesBefore(_date.getDate(), 2);
    let hour = DotNetString.padZeroesBefore(_date.getHours(), 2);
    let minute = DotNetString.padZeroesBefore(_date.getMinutes(), 2);
    let second = DotNetString.padZeroesBefore(_date.getSeconds(), 2);

    return year + "/" + month + "/" + date + " " + hour + ":" + minute + ":" + second;
}

// END: datetime
// ==================================================

P24Utils.formatNumberWithSeparator = function (_number) {
    if (_number == null)
        return "";

    let numberString = "" + _number;
    let result = "";

    if (numberString.startsWith("-")) {
        result += "-";
        numberString = numberString.substring(1);
    }

    if (numberString.length <= 3)
        return result + numberString;
        
    let pos = numberString.length % 3;
    let index = 0;
    while (index < numberString.length) {
        if (pos != 0) {
            result += numberString.substring(index, pos);
            if (pos < numberString.length)
                result += ",";
        }

        index = pos;
        pos += 3;
    }

    return result;
};

P24Utils.formatDataLength = function (_length) {
    const oneKiB = 1024;
    const oneMiB = 1024 * oneKiB;
    const oneGiB = 1024 * oneMiB;
    const oneTiB = 1024 * oneGiB;

    if (_length > oneTiB) {
        return (_length / oneTiB).toFixed(2) + " TiB";
    }

    if (_length > oneGiB) {
        return (_length / oneGiB).toFixed(2) + " GiB";
    }

    if (_length > oneMiB) {
        return (_length / oneMiB).toFixed(2) + " MiB";
    }

    if (_length > oneKiB) {
        return (_length / oneKiB).toFixed(2) + " KiB";
    }

    return (_length).toFixed(0) + " B";
}

P24Utils.Visual = {
    refreshHighlightSingleItem: function (_element, _status) {
        if (_element == null)
            return;

        if (_status == CONFIG_PANEL_ITEM_STATUS_NO_CHANGE) {
            _element.removeClass("border-success border-danger border-warning border-start border-end");
            _element.css({
                "padding-left": "2px",
                "padding-right": "2px"
            });
        }
        else if (_status == CONFIG_PANEL_ITEM_STATUS_MODIFIED) {
            _element.removeClass("border-success border-danger");
            _element.addClass("border-warning border-start border-end");
            _element.css({
                "padding-left": "0px",
                "padding-right": "0px"
            });
        }
        else if (_status == CONFIG_PANEL_ITEM_STATUS_SAVED) {
            _element.removeClass("border-warning border-danger");
            _element.addClass("border-success border-start border-end");
            _element.css({
                "padding-left": "0px",
                "padding-right": "0px"
            });
        }
        else if (_status == CONFIG_PANEL_ITEM_STATUS_ERROR) {
            _element.removeClass("border-warning border-success");
            _element.addClass("border-danger border-start border-end");
            _element.css({
                "padding-left": "0px",
                "padding-right": "0px"
            });
        }
    },

};

// ==================================================
// html

class HtmlUtils {
    constructor() { }


    static escape(_html) {
        return _html
            .replace(/&/g, '&amp')
            .replace(/'/g, '&apos')
            .replace(/"/g, '&quot')
            .replace(/>/g, '&gt')
            .replace(/</g, '&lt');
    }

    static unescape(_html) {
        return _html
            .replace(/&amp/g, '&')
            .replace(/&apos/g, "'")
            .replace(/&quot/g, '"')
            .replace(/&gt/g, '>')
            .replace(/&lt/g, '<');
    }
}

// END: html
// ==================================================

/*
    cyrb53 (c) 2018 bryc (github.com/bryc)
    A fast and simple hash function with decent collision resistance.
    Largely inspired by MurmurHash2/3, but with a focus on speed/simplicity.
    Public domain. Attribution appreciated.
*/
const cyrb53 = function (str, seed = 0) {
    let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
    for (let i = 0, ch; i < str.length; i++) {
        ch = str.charCodeAt(i);
        h1 = Math.imul(h1 ^ ch, 2654435761);
        h2 = Math.imul(h2 ^ ch, 1597334677);
    }
    h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507) ^ Math.imul(h2 ^ (h2 >>> 13), 3266489909);
    h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507) ^ Math.imul(h1 ^ (h1 >>> 13), 3266489909);
    return 4294967296 * (2097151 & h2) + (h1 >>> 0);
};

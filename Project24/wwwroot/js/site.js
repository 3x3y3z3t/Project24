/*  site.js
 *  Version: 1.3 (2023.08.26)
 *
 *  Contributor
 *      Arime-chan
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

const P24_MSG_TAG_SUCCESS = "<done>";
const P24_MSG_TAG_WARNING = "<warn>";
const P24_MSG_TAG_ERROR = "<fail>";
const P24_MSG_TAG_EXCEPTION = "<excp>";

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


$(document).ready(function () {
    P24Utils.reloadAllTooltips();
});


// =====
window.P24Utils = {};

P24Utils.ajax_error = function (_xhr, _textStatus, _errorThrown) {
    let body = "<div>Status Code: <code>" + _xhr.status + " - " + HttpStatusCodeName[_xhr.status] + "</code></div>"
        + "<div>jq Status: <code>" + _textStatus + "</code></div>"
        + "<div>Message: " + _errorThrown.message + "</div>";
    Modal.Common.openOneBtnModal("Ajax request error", body, "error", "OK");
}

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

P24Utils.formatDateString_endsAtMinute = function (_date) {
    let year = _date.getFullYear();
    let month = String(_date.getMonth() + 1).padStart(2, "0");
    let date = String(_date.getDate()).padStart(2, "0");
    let hour = String(_date.getHours()).padStart(2, "0");
    let minute = String(_date.getMinutes()).padStart(2, "0");
    let second = String(_date.getSeconds()).padStart(2, "0");

    return year + "/" + month + "/" + date + " " + hour + ":" + minute;
}

P24Utils.formatDateString_endsAtSecond = function (_date) {
    let second = String(_date.getSeconds()).padStart(2, "0");
    return P24Utils.formatDateString_endsAtMinute(_date) + ":" + second;
}

P24Utils.formatDateString = function (_date) {
    let year = _date.getFullYear();
    let month = String(_date.getMonth() + 1).padStart(2, "0");
    let date = String(_date.getDate()).padStart(2, "0");
    let hour = String(_date.getHours()).padStart(2, "0");
    let minute = String(_date.getMinutes()).padStart(2, "0");
    let second = String(_date.getSeconds()).padStart(2, "0");

    return year + "/" + month + "/" + date + " " + hour + ":" + minute + ":" + second;
}

// END: datetime
// ==================================================

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

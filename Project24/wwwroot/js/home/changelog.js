/*  home/changelog.js
    Version: v1.0 (2023.05.15)

    Contributor
        Arime-chan
 */

window.ChangelogPage = {};

ChangelogPage.Backend = {};

ChangelogPage.Elements = {};
ChangelogPage.Elements.m_BtnUpdateNotes = null;
ChangelogPage.Elements.m_BtnChangelog = null;
//ChangelogPage.Elements.m_DivTabUpdateNotes = null;
//ChangelogPage.Elements.m_DivTabChangelog = null;
ChangelogPage.Elements.m_DivUpdateNotes = null;
ChangelogPage.Elements.m_DivChangelog = null;

$(document).ready(function () {
    ChangelogPage.Elements.m_BtnUpdateNotes = $("#btn-update-notes");
    ChangelogPage.Elements.m_BtnChangelog = $("#btn-changelog");

    //ChangelogPage.Elements.m_DivTabUpdateNotes = $("#div-tab-update-notes");
    //ChangelogPage.Elements.m_DivTabChangelog = $("#div-tab-changelog");

    ChangelogPage.Elements.m_DivUpdateNotes = $("#div-update-notes");
    ChangelogPage.Elements.m_DivChangelog = $("#div-changelog");

    ChangelogPage.Elements.m_BtnUpdateNotes.on("show.bs.tab", function (_event) {
        btn_updateNotes_onClick();
    });

    ChangelogPage.Elements.m_BtnChangelog.on("show.bs.tab", function (_event) {
        btn_changelog_onClick();
    });

    (new bootstrap.Tab(ChangelogPage.Elements.m_BtnChangelog[0])).show();

});


// ==================================================
// ajax request sender

ChangelogPage.ajax_fetchChangelog = function () {
    $.ajax({
        type: "GET",
        url: "Changelog?handler=Changelog",
        success: ChangelogPage.ajax_fetchChangelog_success,
        error: ChangelogPage.ajax_error
    });
}

ChangelogPage.ajax_fetchUpdateNotes = function (_tag, _latest = false) {
    $.ajax({
        type: "GET",
        url: "Changelog?handler=UpdateNoteByTag&_tag=" + _tag + "&_latest=" + _latest,
        success: ChangelogPage.ajax_fetchUpdateNotes_success,
        error: ChangelogPage.ajax_error
    });
}

// END: ajax request sender
// ==================================================

// ==================================================
// event

ChangelogPage.ajax_error = function (_xhr, _textStatus, _errorThrown) {
    ChangelogPage.Elements.m_BtnUpdateNotes.removeAttr("disabled");
    ChangelogPage.Elements.m_BtnChangelog.removeAttr("disabled");
    P24Utils.ajax_error(_xhr, _textStatus, _errorThrown);
}

ChangelogPage.ajax_fetchUpdateNotes_success = function (_content, _textStatus, _xhr) {
    ChangelogPage.Elements.m_BtnUpdateNotes.removeAttr("disabled");
    let body = _content.substring(6);

    if (_content.startsWith("<fail>")) {
        Modal.Common.openOneBtnModal("Thất bại", body, "fail");
        return;
    }

    if (_content.startsWith("<excp>")) {
        Modal.Common.openOneBtnModal("Exception", "<pre>" + body + "</pre>");
        return;
    }

    body = "<div>" + marked.parse(body) + "</div>";
    ChangelogPage.Elements.m_DivUpdateNotes.html(body);

    return;
}

ChangelogPage.ajax_fetchChangelog_success = function (_content, _textStatus, _xhr) {
    ChangelogPage.Elements.m_BtnChangelog.removeAttr("disabled");
    let body = _content.substring(6);

    if (_content.startsWith("<fail>")) {
        Modal.Common.openOneBtnModal("Thất bại", body, "fail");
        return;
    }

    if (_content.startsWith("<excp>")) {
        Modal.Common.openOneBtnModal("Exception", "<pre>" + body + "</pre>");
        return;
    }

    body = "<div>" + marked.parse(body) + "</div>";
    ChangelogPage.Elements.m_DivChangelog.html(body);

    return;
}

function btn_updateNotes_onClick() {
    ChangelogPage.Elements.m_BtnUpdateNotes.attr("disabled", true);
    ChangelogPage.ajax_fetchUpdateNotes("fail");
}

function btn_changelog_onClick() {
    ChangelogPage.Elements.m_BtnChangelog.attr("disabled", true);
    ChangelogPage.ajax_fetchChangelog();
}

// END: event
// ==================================================

// ==================================================
// helper
// END: helper
// ==================================================

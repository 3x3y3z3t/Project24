/*  server-announcement.js
    Version: v1.1 (2023.09.22)

    Author
        Arime-chan
 */

window.ServerAnnouncementPage = {
    Data: null,
    UI: null,

    m_LastUpdate: new Date(),
    m_IsUpdating: false,

    m_Timer: null,
    m_AwaitingData: false,

    c_UpdateInterval: 100,
    c_RefreshDataInterval: 60 * 1000,


    init: function () {
        this.Data.init();
        this.UI.init();

        m_Timer = setInterval(function () { ServerAnnouncementPage.update(); }, this.c_UpdateInterval);
    },

    reload: function () {
        this.ajax_fetchAnnouncementData();
    },

    update: function () {
        if (this.m_IsUpdating)
            return;

        this.m_IsUpdating = true;

        let deltaTimeMillis = new Date() - this.m_LastUpdate;
        this.m_LastUpdate = new Date();

        // =====
        this.Data.update(deltaTimeMillis);

        this.UI.refreshPage(this.Data.Announcements);

        //let html = "<div class=\"ms-2\">Client Time: " + P24Utils.formatDateString(new Date()) + " (should match with Server Time)</div>";
        //this.UI.m_DivSvrMsgModal.html(html);

        this.m_IsUpdating = false;
    },

    // ==================================================

    ajax_fetchAnnouncementData: function () {
        if (this.m_AwaitingData)
            return;

        this.m_AwaitingData = true;

        $.ajax({
            type: "GET",
            url: "/ServerAnnouncement?handler=FetchAnnouncementData",
            success: function (_content, _textStatus, _xhr) { ServerAnnouncementPage.ajax_fetchAnnouncementData_success(_content, _textStatus, _xhr); },
            error: function (_xhr, _textStatus, _errorThrow) { ServerAnnouncementPage.ajax_error(_xhr, _textStatus, _errorThrow); }
        });
    },

    ajax_error: function(_xhr, _textStatus, _errorThrown) {
        this.m_AwaitingData = false;
        P24Utils.Ajax.error(_xhr, _textStatus, _errorThrown);
    },

    ajax_fetchAnnouncementData_success: function (_content, _textStatus, _xhr) {
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
};

/*
    Data.
*/
ServerAnnouncementPage.Data = {
    Announcements: null,


    init: function () {

    },

    processPageData: function (_json) {

        let parsedData = JSON.parse(_json);

        this.Announcements = parsedData;




        // =====
        // TODO: remove this;
            //console.log(_json);
            //console.log(this.Announcements);
        // =====

        if (false) {

            console.log("========== TimeSpen utils test ==========\n\n");

            let i = 0;
            for (const timeSpanTest of parsedData) {
                //let parsedMillis = P24Utils.TimeSpan.parse(timeSpanTest.TimeSpan);
                let timeSpan = TimeSpan.parse(timeSpanTest.TimeSpan);
                let parsedMillis = timeSpan.TotalMilliseconds;
                let orgMillis = +((timeSpanTest.TotalMillis).toFixed(0));
                if (parsedMillis == orgMillis) {
                    console.log("Test " + i + ": PASS (" + parsedMillis + "|" + orgMillis + ")");
                } else {
                    console.warn("Test " + i + ": FAIL (" + parsedMillis + "|" + orgMillis + ")");
                }

                ++i;
            }

            console.log("\n=========================================");

            return null;
        }




        for (announcement of this.Announcements) {
            this.processSingleItem(announcement);
        }

        return parsedData;
    },

    processSingleItem: function (_announcement) {
        _announcement.AddedDate = new Date(_announcement.AddedDate);
        _announcement.ExpireDate = new Date(_announcement.ExpireDate);

        for (argument of _announcement.Arguments) {
            this.processSingleArgument(argument);
        }
    },

    processSingleArgument: function (_argument) {
        switch (_argument.Type) {
            case P24_ARG_DATA_TYPE_DATETIME:
                _argument.Value = new Date(_argument.Value);
                if (_argument.ShouldUpdate) {
                    _argument.CountToward = new Date(_argument.CountToward);
                }
                break;
            case P24_ARG_DATA_TYPE_TIMESPAN:
                _argument.Value = TimeSpan.parse(_argument.Value);
                if (_argument.ShouldUpdate) {
                    _argument.CountToward = TimeSpan.parse(_argument.CountToward);
                }
                break;





                // TODO: process other type;
        }







    },

    // ==================================================

    update: function (_deltaTime) {
        for (announcement of this.Announcements) {
            if (announcement.IsExpired)
                continue;

            this.updateSingleAnnouncement(announcement, _deltaTime);

            if (this.isAnnouncementExpired(announcement))
                announcement.IsExpired = true;
        }
    },

    updateSingleAnnouncement: function (_announcement, _deltaTime) {
        for (argument of _announcement.Arguments) {
            if (this.isArgDoneUpdating(argument))
                continue;

            switch (argument.Type) {
                case P24_ARG_DATA_TYPE_DATETIME:
                    this.updateArgTypeDateTime(argument, _deltaTime);
                    break;

                case P24_ARG_DATA_TYPE_TIMESPAN:
                    this.updateArgTypeTimeSpan(argument, _deltaTime);
                    break;





                // TODO: update other type;
            }

        }






    },

    // ==================================================

    isAnnouncementExpired: function (_announcement) {

        // TODO: check other rules;









        if (announcement.ExpireDate.valueOf() != NaN) {
            if (new Date() > announcement.ExpireDate) {
                return true;
            }
        }


        return false;
    },

    isArgDoneUpdating: function (_argument, _deltaTime) {
        if (!_argument.ShouldUpdate)
            return true;

        if (_argument.InfiniteCount)
            return false;

        return this.isValueSurpassedThreshold(_argument.Value, _argument.CountsToward, _argument.CountsPerMillis);
    },

    isValueSurpassedThreshold: function (_value, _threshold, _countsAmount) {
        if (_countsAmount > 0 && _value >= _threshold)
            return true;
        if (_countsAmount < 0 && _value <= _threshold)
            return true;

        return false;
    },

    updateArgTypeDateTime: function (_argument, _deltaTime) {
        let millis = _argument.Value.getMilliseconds();
        let millisCount = _argument.CountsPerMillis * _deltaTime;
        _argument.Value.setMilliseconds(millis + millisCount);

        if (_argument.InfiniteCount)
            return;

        if (this.isValueSurpassedThreshold(_argument.Value, _argument.CountsToward, _argument.CountsPerMillis))
            _argument.Value = _argument.CountsToward;
    },

    updateArgTypeTimeSpan: function (_argument, _deltaTime) {
        let millisCount = _argument.CountsPerMillis * _deltaTime;
        _argument.Value.addMilliseconds(millisCount);

        if (_argument.InfiniteCount)
            return;

        if (this.isValueSurpassedThreshold(_argument.Value, _argument.CountsToward, _argument.CountsPerMillis))
            _argument.Value = _argument.CountsToward;
    },

}

/*
    UI.
*/
ServerAnnouncementPage.UI = {
    m_DivSvrMsg: null,
    m_DivSvrMsgModal: null,


    init: function () {
        this.m_DivSvrMsg = $("#div-svr-msg");
        this.m_DivSvrMsgModal = $("#div-svr-msg-modal");
    },

    refreshPage: function (_data) {
        let html = "";

        for (const announcement of _data) {
            if (announcement.IsExpired)
                continue;

            if (announcement.Tag == "ANNOUNCEMENT_TEST")
                continue;

            html += this.constructSingleAnnouncementHtml(announcement);
        }

        this.m_DivSvrMsg.html(html);
    },

    constructSingleAnnouncementHtml(_announcement) {
        //let html = "<div class=\"alert-" + _announcement.Severity + " rounded-3 px-2 py-2 my-1\">"
        let html = "<div class=\"alert-" + _announcement.Severity + " border border-" + _announcement.Severity + " border-1 rounded-3 px-2 py-2 my-1\">"


            + DotNetString.format(_announcement.FormatString, _announcement.Arguments)





            + "</div>";

        return html;
    },
};


// ==================================================
$(function () {
    ServerAnnouncementPage.init();
    ServerAnnouncementPage.reload();

});












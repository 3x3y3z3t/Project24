/*  localization-string.js
    Version: v1.0 (2023.06.30)

    Contributor
        Arime-chan
 */


const LOCL_LANG_VIE = "LOCL_LANG_VIE";
const LOCL_LANG_ENG = "LOCL_LANG_ENG";
const LOCL_LANG_JAP = "LOCL_LANG_JAP";

const ERRCODE_UNEXPECTED_CONTROL = "UNEXPECTED_CONTROL";
const ERRCODE_UPDATER_INVALID_BLOCK_NAME = "INVALID_BLOCK_NAME";

const LOCL_STR_CONFIRM = "LOCL_STR_CONFIRM";
const LOCL_STR_YES = "LOCL_STR_YES";
const LOCL_STR_NO = "LOCL_STR_NO";
const LOCL_STR_ACCEPT = "LOCL_STR_ACCEPT";
const LOCL_STR_CANCEL = "LOCL_STR_CANCEL";

const LOCL_STR_SUCCESS = "LOCL_STR_SUCCESS";
const LOCL_STR_INFO = "LOCL_STR_INFO";
const LOCL_STR_WARN = "LOCL_STR_WARN";
const LOCL_STR_ERR = "LOCL_STR_ERR";
const LOCL_STR_FAIL = "LOCL_STR_FAIL";
const LOCL_STR_UNKNOWN_ERR = "LOCL_STR_UNKNOWN_ERR";
const LOCL_STR_EXCEPTION = "LOCL_STR_EXCEPTION";

const LOCL_DESC_MALFORMED = "LOCL_DESC_MALFORMED";
const LOCL_DESC_UPDATER_NOMAIN = "LOCL_DESC_UPDATER_NOMAIN";
const LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT = "LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT";
const LOCL_DESC_UPDATER_PREV_PURGE_QUEUED = "LOCL_DESC_UPDATER_PREV_PURGE_QUEUED";
const LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED = "LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED";
const LOCL_DESC_UPDATER_PREV_APPLY_QUEUED = "LOCL_DESC_UPDATER_PREV_APPLY_QUEUED";
const LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED = "LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED";
const LOCL_DESC_UPDATER_ABORT_SUCCESS = "LOCL_DESC_UPDATER_ABORT_SUCCESS";
const LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV = "LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV";
const LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT = "LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT";
const LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE = "LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE";
const LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY = "LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY";
const LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE = "LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE";
const LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY = "LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY";
const LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV = "LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV";
const LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT = "LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT";

const LOCL_DESC_UPDATER_INPUTFILES_INITIAL = "LOCL_DESC_UPDATER_INPUTFILES_INITIAL";
const LOCL_DESC_UPDATER_UPLOAD_SUCCESS = "LOCL_DESC_UPDATER_UPLOAD_SUCCESS";








class P24LocalizationClass {
    constructor(_lang) {
        if (_lang == LOCL_LANG_VIE)
            this.#loadVie();
        else if (_lang == LOCL_LANG_ENG)
            this.#loadEng();
        else if (_lang == LOCL_LANG_JAP)
            this.#loadJap();
        else {
            window.alert("P24 Localization failed.");
        }
    }


    get(_key) {
        return this.#m_Active.get(_key);
    }


    #loadVie() {
        this.#m_Vie = new Map();

        this.#m_Vie.set(LOCL_STR_CONFIRM, "Xác nhận");
        this.#m_Vie.set(LOCL_STR_YES, "Có");
        this.#m_Vie.set(LOCL_STR_NO, "Không");
        this.#m_Vie.set(LOCL_STR_ACCEPT, "Đồng ý");
        this.#m_Vie.set(LOCL_STR_CANCEL, "Hủy bỏ");

        this.#m_Vie.set(LOCL_STR_SUCCESS, "Thành công");
        this.#m_Vie.set(LOCL_STR_INFO, "Thông báo");
        this.#m_Vie.set(LOCL_STR_WARN, "Cảnh báo");
        this.#m_Vie.set(LOCL_STR_ERR, "Lỗi");
        this.#m_Vie.set(LOCL_STR_FAIL, "Thất bại");
        this.#m_Vie.set(LOCL_STR_UNKNOWN_ERR, "Lỗi không xác định");
        this.#m_Vie.set(LOCL_STR_EXCEPTION, "Exception (lỗi ngoại lệ)");

        this.#m_Vie.set(LOCL_DESC_MALFORMED, "Dữ liệu không đúng định dạng");
        this.#m_Vie.set(LOCL_DESC_UPDATER_NOMAIN, "Không có thông tin phiên bản hiện tại");
        this.#m_Vie.set(LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT, "Tác vụ Xóa file đang chạy, không thể hủy tác vụ.");
        this.#m_Vie.set(LOCL_DESC_UPDATER_PREV_PURGE_QUEUED, "Tác vụ Xóa file phiên bản trước đã được khởi chạy");
        this.#m_Vie.set(LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED, "Tác vụ Xóa file phiên bản mới đã được khởi chạy");
        this.#m_Vie.set(LOCL_DESC_UPDATER_PREV_APPLY_QUEUED, "Tác vụ Hạ xuống phiên bản trước đã được khởi chạy");
        this.#m_Vie.set(LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED, "Tác vụ Nâng cấp phiên bản mới đã được khởi chạy");
        this.#m_Vie.set(LOCL_DESC_UPDATER_ABORT_SUCCESS, "Hủy tác vụ thành công");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV, "<div>Xóa file phiên bản cũ cũng sẽ <b>hủy tác vụ Hạ xuống phiên bản cũ</b>.</div><div>Bạn có chắc muốn xóa file?</div>");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT, "<div>Xóa file phiên bản mới cũng sẽ <b>hủy tác vụ Nâng cấp phiên bản mới</b>.</div><div>Bạn có chắc muốn xóa file?</div>");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE, "Bạn có chắc muốn <b>hủy tác vụ Xóa file phiên bản cũ</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY, "Bạn có chắc muốn <b>hủy tác vụ Hạ xuống phiên bản cũ</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE, "Bạn có chắc muốn <b>hủy tác vụ Xóa file phiên bản mới</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY, "Bạn có chắc muốn <b>hủy tác vụ Nâng cấp phiên bản mới</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV, "Bạn có chắc muốn <b>khởi chạy tác vụ Hạ xuống phiên bản cũ</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT, "Bạn có chắc muốn <b>khởi chạy tác vụ Nâng cấp phiên bản mới</b>?");
        this.#m_Vie.set(LOCL_DESC_UPDATER_INPUTFILES_INITIAL, "Click hoặc thả thư mục vào đây..");
        this.#m_Vie.set(LOCL_DESC_UPDATER_UPLOAD_SUCCESS, "Upload thành công. Click OK để tải lại trang.");







        this.#m_Active = this.#m_Vie;
    }

    #loadEng() {
        this.#m_Active = new Map();

    }

    #loadJap() {
        this.#m_Active = new Map();

    }

    #m_Active = null;

    #m_Vie = null;
    #m_Eng = null;
    #m_Jap = null;
}


window.P24Localization = new P24LocalizationClass(LOCL_LANG_VIE);



//$(document).ready(function () {
//    P24Localization = new P24LocalizationClass(LOCL_LANG_VIE);
//});

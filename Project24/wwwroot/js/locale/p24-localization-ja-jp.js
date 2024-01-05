/*  locale/p24-localization-ja-jp.js
    Version: v1.0 (2023.09.02)

    Author
        Arime-chan
 */

window.P24Localization = {
    get: function (_key) { if (this[_key] == null || this[_key].trim() == "") return "<code>{" + _key + "}</code>"; return this[_key]; },

    LOCL_LANG_VIE                                   : "", 
    LOCL_LANG_ENG                                   : "", 
    LOCL_LANG_JAP                                   : "", 

    ERRCODE_UNEXPECTED_CONTROL                      : "", 
    ERRCODE_UPDATER_INVALID_BLOCK_NAME              : "", 

    LOCL_BTN_CLOSE                                  : "", 
    LOCL_BTN_CHANGE_PASS                            : "", 

    LOCL_STR_CONFIRM                                : "確認", 
    LOCL_STR_YES                                    : "はい", 
    LOCL_STR_NO                                     : "いいえ", 
    LOCL_STR_ACCEPT                                 : "同意", 
    LOCL_STR_CANCEL                                 : "キャンセル", 

    LOCL_STR_SUCCESS                                : "成功", 
    LOCL_STR_INFO                                   : "メッセージ", 
    LOCL_STR_WARN                                   : "警告", 
    LOCL_STR_ERR                                    : "エラー", 
    LOCL_STR_FAIL                                   : "失敗", 
    LOCL_STR_CRIT                                   : "", 
    LOCL_STR_UNKNOWN_ERR                            : "エラー（未記録）", 
    LOCL_STR_EXCEPTION                              : "エクセプション", 

    LOCL_DESC_DEFAULT_PASSWORD_USAGE                : "", 

    LOCL_DESC_MALFORMED                             : "LOCL_DESC_MALFORMED", 
    LOCL_DESC_UPDATER_NOMAIN                        : "LOCL_DESC_UPDATER_NOMAIN", 
    LOCL_DESC_UPDATER_PURGE_CANNOT_ABORT            : "実行中のファイル削除のタスクが中断ができません。", 
    LOCL_DESC_UPDATER_PREV_PURGE_QUEUED             : "前バージョンファイル削除タスクを開始しました。", 
    LOCL_DESC_UPDATER_NEXT_PURGE_QUEUED             : "新バージョンファイル削除タスクを開始しました。", 
    LOCL_DESC_UPDATER_PREV_APPLY_QUEUED             : "アップグレードタスクを開始しました。", 
    LOCL_DESC_UPDATER_NEXT_APPLY_QUEUED             : "ダウングレードタスクを開始しました。", 
    LOCL_DESC_UPDATER_ABORT_SUCCESS                 : "実行中のタスクを中断しました。", 
    LOCL_DESC_UPDATER_CONFIRM_PURGE_PREV            : "<div>前バージョンファイル削除タスクを開始すると <b>ダウングレードタスクを中断する</b>ことになります。</div><div>よろしいですか？</div>", 
    LOCL_DESC_UPDATER_CONFIRM_PURGE_NEXT            : "<div>新バージョンファイル削除タスクを開始すると <b>アップグレードタスクを中断する</b>ことになります。</div><div>よろしいですか？</div>", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_PURGE      : "<b>前バージョンファイル削除タスクを中断</b>しますか？", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_PURGE      : "<b>新バージョンファイル削除タスクを中断</b>しますか？", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_PREV_APPLY      : "<b>ダウングレードタスクを中断</b>しますか？", 
    LOCL_DESC_UPDATER_CONFIRM_ABORT_NEXT_APPLY      : "<b>アップグレードタスクを中断</b>しますか？", 
    LOCL_DESC_UPDATER_CONFIRM_APPLY_PREV            : "<b>ダウングレードタスクを開始</b>しますか？", 
    LOCL_DESC_UPDATER_CONFIRM_APPLY_NEXT            : "<b>アップグレードタスクを開始</b>しますか？", 

    LOCL_DESC_UPDATER_INPUTFILES_INITIAL            : "Click hoặc thả thư mục vào đây..", 
    LOCL_DESC_UPDATER_UPLOAD_SUCCESS                : "アップロード完了しました。「OK」ボタンを押すとページが再読み込まれます。", 
    LOCL_DESC_UPDATER_BATCH_ERROR                   : "", 

    LOCL_DESC_CFG_LOCALE_CHANGED                    : "<div>表示言語を変更しました。「OK」ボタンを押すとページが再読み込まれます。</div><div>（ページを再読み込むまで更新されない部分があります。）</div>", 

};

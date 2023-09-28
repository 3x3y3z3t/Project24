/*  dotmet.js
 *  Version: 1.2 (2023.09.27)
 *
 *  This file contains implementation of .NET's various classses.
 * 
 *  Author
 *      Arime-chan
 */

/*
    Unlike .NET, dotnet.js's TimeSpan works with Millisecond instead of Tick.
    Also due to Javascript's limitation, dotnet.js TimeSpan only have one constructor that takes in Millisecond.
*/
class TimeSpan {
    get Days() { return int(this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerDay); }
    get Hours() { return int((this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerHrs) % 24); }
    get Minutes() { return int((this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerMin) % 60); }
    get Seconds() { return int((this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerSec) % 60); }
    get Milliseconds() { return this.#m_TotalMilliseconds % TimeSpan.#s_MillisPerSec; }

    get TotalDays() { return this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerDay; }
    get TotalHours() { return this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerHrs; }
    get TotalMinutes() { return this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerMin; }
    get TotalSeconds() { return this.#m_TotalMilliseconds / TimeSpan.#s_MillisPerSec; }
    get TotalMilliseconds() { return this.#m_TotalMilliseconds; }

    static Zero = new TimeSpan();

    constructor(_milliseconds = 0) {
        this.#m_TotalMilliseconds = int(_milliseconds);
    }


    static parse(_string) {
        let signFactor = 1;
        if (_string.startsWith('-'))
            signFactor = -1;

        let timeSpan = null;

        let arr = _string.split('.');
        switch (arr.length) {
            case 1: timeSpan = TimeSpan.#parseInnerCase1(arr[0]); break;
            case 2: timeSpan = TimeSpan.#parseInnerCase2(arr[0], arr[1]); break;
            case 3: timeSpan = TimeSpan.#parseInnerCase3(arr[0], arr[1], arr[2]); break;
        }

        if (timeSpan == null)
            return null;

        let totalMillis =
            timeSpan.Days * this.#s_MillisPerDay +
            timeSpan.Hours * this.#s_MillisPerHrs +
            timeSpan.Mins * this.#s_MillisPerMin +
            timeSpan.Secs * this.#s_MillisPerSec +
            timeSpan.Millis;

        totalMillis *= signFactor;

        return new TimeSpan(totalMillis);
    }


    /* Case "hh:mm:ss" */
    static #parseInnerCase1(_str) {
        let arr = _str.split(':');
        if (arr.length != 3) {
            console.error("Invalid string: " + _str);
            return 0;
        }

        return {
            Days: 0,
            Hours: +arr[0],
            Mins: +arr[1],
            Secs: +arr[2],
            Millis: 0
        };
    }

    /* Case "hh:mm:ss.fffffff" */
    /* Case "dddddddd.hh:mm:ss" */
    static #parseInnerCase2(_elm0, _elm1) {
        let timeSpan = null;

        if (_elm1.length == 7) {
            // case hh:mm:ss.fffffff;
            timeSpan = TimeSpan.#parseInnerCase1(_elm0);
            timeSpan.Millis = int(+_elm1 / TimeSpan.#s_TicksPerMilli);
        } else {
            // case ddd.hh:mm:ss;
            timeSpan = TimeSpan.#parseInnerCase1(_elm1);
            timeSpan.Days = +_elm0;
        }

        return timeSpan;
    }

    /* Case "dddddddd.hh:mm:ss.fffffff" */
    static #parseInnerCase3(_elm0, _elm1, _elm2) {
        let timeSpan = TimeSpan.#parseInnerCase2(_elm1, _elm2);
        timeSpan.Days = +_elm0;

        return timeSpan;
    }

    addMilliseconds(_milliseconds) {
        this.#m_TotalMilliseconds += _milliseconds;
    }

    toString() {
        let str = "";

        if (this.Days > 0)
            str += this.Days + ".";

        str += this.Hours + ":" + DotNetString.padZeroesBefore(this.Minutes, 2) + ":" + DotNetString.padZeroesBefore(this.Seconds, 2);

        if (this.Milliseconds > 0)
            str += "." + this.Milliseconds;

        return str;
    }


    #m_TotalMilliseconds = 0;

    static #s_TicksPerMilli = 10000;
    static #s_MillisPerSec = 1000;
    static #s_MillisPerMin = 1000 * 60;
    static #s_MillisPerHrs = 1000 * 60 * 60;
    static #s_MillisPerDay = 1000 * 60 * 60 * 24;
}

/*
    `String` has been taken up by vanilla Javascript, so we use `DotNetString` instead.
*/
class DotNetString {
    static padZeroesBefore(_value, _desiredLength) { return String(_value).padStart(_desiredLength, '0'); }

    /*
        Feature of this function will be added over time.
    */
    static format(_formatString, _args) {
        const regex = /\{([0-9]+)(:[a-zA-Z0-9: /.]*)?\}/g;
        const matches = _formatString.matchAll(regex);

        let str = "";

        let lastMatchIndex = 0;

        for (const match of matches) {
            let index = match[1];
            if (index >= _args.length)
                continue;

            let arg = _args[index];
            let value = arg.Value;

            switch (arg.Type) {

                case P24_ARG_DATA_TYPE_TIMESPAN:
                    //value = new TimeSpan(arg.Value);
                    if (match[2] != null)
                        value = DotNetString.formatCustomTimeSpan(match[2].substring(1), arg.Value);
                    break;

                case P24_ARG_DATA_TYPE_DATETIME:
                    if (match[2] != null)
                        value = DotNetString.formatCustomDateTime(match[2].substring(1), arg.Value);
                    break;

            }

            str += _formatString.substring(lastMatchIndex, match.index) + value;
            lastMatchIndex = match.index + match[0].length;
        }
        str += _formatString.substring(lastMatchIndex);

        return str;
    }

    /*
        https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
    */
    static formatCustomDateTime(_formatString, _dateTime) {
        const regex = /([dfHMmsy]{1,4})/g;
        const matches = _formatString.matchAll(regex);

        let str = "";

        let lastMatchIndex = 0;
        for (const match of matches) {
            let matchLength = match[1].length;
            let formatChar = match[1][0];

            str += _formatString.substring(lastMatchIndex, match.index);

            switch (formatChar) {
                case "d": str += DotNetString.padZeroesBefore(_dateTime.getDate(), matchLength); break;

                case "f":
                    switch (matchLength) {
                        case 1: str += DotNetString.padZeroesBefore(_dateTime.getMilliseconds() / 100, 1); break;
                        case 2: str += DotNetString.padZeroesBefore(_dateTime.getMilliseconds() / 10, 2); break;
                        default: str += DotNetString.padZeroesBefore(_dateTime.getMilliseconds(), 3); break;
                    }
                    break;

                case "H": str += DotNetString.padZeroesBefore(_dateTime.getHours(), matchLength); break;

                case "m": str += DotNetString.padZeroesBefore(_dateTime.getMinutes(), matchLength); break;

                case "M": str += DotNetString.padZeroesBefore(_dateTime.getMonth() + 1, matchLength); break;

                case "s": str += DotNetString.padZeroesBefore(_dateTime.getSeconds(), matchLength); break;

                case "y":
                    switch (matchLength) {
                        case 1: str += (_dateTime.getFullYear() % 100); break;
                        case 2: str += DotNetString.padZeroesBefore(_dateTime.getFullYear() % 100, 2); break;
                        case 3: str += DotNetString.padZeroesBefore(_dateTime.getFullYear(), 3); break;
                        case 4: str += DotNetString.padZeroesBefore(_dateTime.getFullYear(), 4); break;
                        default: str += DotNetString.padZeroesBefore(_dateTime.getFullYear(), matchLength); break;
                    }
                    break;

                default:
                    str += match[1];
                    break;
            }

            lastMatchIndex = match.index + match[0].length;
        }
        str += _formatString.substring(lastMatchIndex);

        return str;
    }

    /*
        https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-timespan-format-strings
    */
    static formatCustomTimeSpan(_formatString, _timeSpan) {
        const regex = /([dfhms]{1,4})/g;
        const matches = _formatString.matchAll(regex);

        let str = "";

        let lastMatchIndex = 0;
        for (const match of matches) {
            let matchLength = match[1].length;
            let formatChar = match[1][0];

            str += _formatString.substring(lastMatchIndex, match.index);

            switch (formatChar) {
                case "d": str += DotNetString.padZeroesBefore(_timeSpan.Days, matchLength); break;

                case "h": str += DotNetString.padZeroesBefore(_timeSpan.Hours, matchLength); break;

                case "m": str += DotNetString.padZeroesBefore(_timeSpan.Minutes, matchLength); break;

                case "s": str += DotNetString.padZeroesBefore(_timeSpan.Seconds, matchLength); break;

                case "f":
                    switch (matchLength) {
                        case 1: str += DotNetString.padZeroesBefore(_timeSpan.Milliseconds / 100, 1); break;
                        case 2: str += DotNetString.padZeroesBefore(_timeSpan.Milliseconds / 10, 2); break;
                        default: str += DotNetString.padZeroesBefore(_timeSpan.Milliseconds, 3); break;
                    }
                    break;

                default:
                    str += match[1];
                    break;
            }

            lastMatchIndex = match.index + match[0].length;
        }
        str += _formatString.substring(lastMatchIndex);

        return str;
    }
}

function int(_number) { return Math.floor(_number); }

window.DotNet = {};



/*  userManager-addUser.js
 *  Version: 1.0 (2022.09.06)
 *
 *  Contributor
 *      Arime-chan
 */

function replaceUnicodeChar(_string) {
    const aChars = "aàáảãạăằắẳẵặâầấẩẫậ";
    const iChars = "iìíỉĩị";
    const uChars = "uùúủũụưừứửữự";
    const eChars = "eèéẻẽẹêềếểễệ";
    const oChars = "oòóỏõọôồốổỗộơờớởỡợ";
    const yChars = "yỳýỷỹỵ";

    let string = "";

    for (let i = 0; i < _string.length; ++i) {
        if (aChars.includes(_string[i])) {
            string += 'a';
        } else if (iChars.includes(_string[i])) {
            string += 'i';
        } else if (uChars.includes(_string[i])) {
            string += 'u';
        } else if (eChars.includes(_string[i])) {
            string += 'e';
        } else if (oChars.includes(_string[i])) {
            string += 'o';
        } else if (yChars.includes(_string[i])) {
            string += 'y';
        } else {
            string += _string[i];
        }
    }

    return string;
}

function constructUsername(_fullname) {
    if (_fullname == null)
        return "";

    let tokens = _fullname.split(" ");
    tokens = tokens.filter(_x => _x);
    tokens = tokens.filter(_x => isNaN(_x));

    if (tokens.length == 0)
        return "";

    if (tokens.length == 1) {
        if (tokens[0] == "")
            return "";

        let lastName = tokens[0].trim();
        return lastName[0].toUpperCase() + lastName.slice(1);
    }

    if (tokens.length == 2) {
        let familyName = tokens[0].trim();
        let lastName = tokens[1].trim();

        return lastName[0].toUpperCase() + lastName.slice(1) + familyName[0].toUpperCase();
    }

    let familyName = tokens[0].trim();
    let lastName = tokens[tokens.length - 1].trim();
    let middlename = "";

    for (let i = 1; i < tokens.length - 1; ++i) {
        middlename += tokens[i][0].toUpperCase();
    }

    return lastName[0].toUpperCase() + lastName.slice(1) + familyName[0].toUpperCase() + middlename;
}

function updateUsername(_textBox) {
    let content = constructUsername(_textBox.value);
    content = replaceUnicodeChar(content);

    $("#add-user-input-username").val(content);

}

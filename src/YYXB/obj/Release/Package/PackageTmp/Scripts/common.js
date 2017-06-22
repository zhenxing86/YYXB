
$(function () {
    $('#checkall').click(function () {
        var val = $("#checkall").prop("checked");
        $("input[name='checkbox']").prop("checked", val);

    });
});



function hashJump(json) {
    var hash = location.hash;
    for (var key in json) {
        if (hash.indexOf(key + "=") >= 0) {
            eval("var re = /\\b" + key + "\\b=.*?(?=&|$)/;");
            hash = hash.replace(re, key + "=" + json[key]);
        } else {
            if (hash.length == 0) {
                hash += "#" + key + "=" + json[key];
            } else {
                hash += "&" + key + "=" + json[key];
            }
        }
    }
    location.hash = hash;
}
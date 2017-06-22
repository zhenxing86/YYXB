var editor;
KindEditor.ready(function (K) {
    editor = K.create('#url_contents', {
        items: [
    'source', '|', 'undo', 'redo', '|', 'justifyleft', 'justifycenter', 'justifyright',
    'justifyfull','indent', 'outdent', 'subscript',
    'superscript', 'selectall', '|', '/',
    'fontsize', 'formatblock', 'fontname', '|', 'forecolor', 'hilitecolor', 'bold',
    'italic', 'underline', 'strikethrough', 'lineheight', 'removeformat', '|', 'table', 'hr', 'link', 'unlink'//, '|', 'image'
        ],
        //cssPath: ['../Content/style/style.css?v=1', '../Content/style/style_view.css'],
        uploadJson: '/Utils/Upload/publics/UpyunUpload.ashx?mttype=1',
        resizeType: 0,
        height: 500,
        width: 695,
        minWidth:480,
        pasteType : 2,
        //newlineTag: "br",
        colorTable: [
    ['#E53333', '#E56600', '#FF9900', '#64451D', '#DFC5A4', '#FFE500'],
    ['#009900', '#006600', '#99BB00', '#B8D100', '#60D978', '#00D5FF'],
    ['#337FE5', '#003399', '#4C33E5', '#9933E5', '#CC33E5', '#EE33EE'],
    ['#FFFFFF', '#CCCCCC', '#999999', '#666666', '#333333', '#000000'],
    ['#f8a9c3', '#a4d7ea', '#9edfa3', '#7bc2dd', '#ef89aa']
        ],
        filterMode: false,
        afterCreate: function () {
            var self = this;
            K.ctrl(document, 13, function () {
                self.sync();
                K('form[name=example]')[0].submit();
            });
            K.ctrl(self.edit.doc, 13, function () {
                self.sync();
                K('form[name=example]')[0].submit();
            });
        }
    });
});

function Validate() {
    if ($('#title').val() == "") {
        alert("标题不能为空！");
        $('#title').focus();
        return false;
    }
    //else if ($('#content').val() == "") {
    //    alert("内容描述不能为空！");
    //    $('#content').focus();
    //    return false;
    //}
    //else if ($('#img_url').val() == "") {
    //    alert("图片不能为空！");
    //    $('#fileToUpload').focus();
    //    return false;
    //}
    return true;
}


$(function () {
    $("#fileToEdit").uploadify({
        'uploader': '/Content/uploadify.swf',
        'script': '/Utils/Upload',
        'queueID': 'some_file_queue',
        'cancelImg': '/Content/cancel.png',
        'folder': 'UploadFile',
        'auto': true,
        'multi': true,
        'scriptData': { 'mttype': 1 },
        'fileExt': '*.bmp;*.png;*.jpeg;*.jpg;*.gif',
        'fileDesc': '请选择(*.bmp;*.png;*.jpeg;*.jpg;*.gif)',
        'onComplete': function (event, ID, fileObj, response, data) {
            var html = "<p align='center'><img class='abimg' style='max-width:100%; ' src='" + response + "' /></p></br>";
            editor.insertHtml(html);
        }
    });
})




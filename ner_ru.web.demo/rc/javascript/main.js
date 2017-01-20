$(document).ready(function () {
    var MAX_INPUTTEXT_LENGTH  = 10000,
        LOCALSTORAGE_TEXT_KEY = 'ner-ru-text',
        DEFAULT_TEXT = 'В Петербурге перед судом предстанет высокопоставленный офицер Генерального штаба ВС РФ. СКР завершил расследование уголовного дела против главы военно-топографического управления Генштаба контр-адмирала Сергея Козлова, обвиняемого в превышении должностных полномочий и мошенничестве.\n' +
'"Следствием собрана достаточная доказательственная база, подтверждающая виновность контр-адмирала Козлова в инкриминируемых преступлениях, в связи с чем уголовное дело с утвержденным обвинительным заключением направлено в суд для рассмотрения по существу", - рассказали следователи.\n' +
'Кроме того, по инициативе следствия представителем Минобороны России к С.Козлову заявлен гражданский иск о возмещении причиненного государству ущерба на сумму свыше 27 млн руб.\n' +
'По данным следователей, в июле 2010г. военный чиновник отдал подчиненному "заведомо преступный приказ" о заключении лицензионных договоров с компаниями "Чарт-Пилот" и "Транзас". Им необоснованно были переданы права на использование в коммерческих целях навигационных морских карт, являвшихся интеллектуальной собственностью РФ. В результате ущерб составил более 9,5 млн руб.\n' +
'Контр-адмирал также умолчал о наличии у него в собственности квартиры в городе Истра Московской области. В результате в 2006г. центральной жилищной комиссии Минобороны и Управления делами президента РФ С.Козлов был признан нуждающимся в жилье и в 2008г. получил от государства квартиру в Москве площадью 72 кв. м и стоимостью 18,5 млн руб. Квартиру позднее приватизировала его падчерица.\n' +
'Против С. Козлова возбуждено дело по п."в" ч.3 ст.286 (превышение должностных полномочий, совершенное с причинением тяжких последствий) и ч.4 ст.159 (мошенничество, совершенное в особо крупном размере) УК РФ.\n' +
'\n' +
'(Скоро в российскую столицу придет Новый Год.)\n' +
'(На самом деле iPhone - это просто смартфон.)';

    var textOnChange = function () {
        var _len = $("#text").val().length; 
        var len = _len.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        var $textLength = $("#textLength");
        $textLength.html("длина текста: " + len + " символов");
        if (MAX_INPUTTEXT_LENGTH < _len) $textLength.addClass("max-inputtext-length");
        else                             $textLength.removeClass("max-inputtext-length");
    };
    var getText = function( $text ) {
        var text = trim_text( $text.val().toString() );
        if (is_text_empty(text)) {
            alert("Введите текст для обработки.");
            $text.focus();
            return (null);
        }
        
        if (text.length > MAX_INPUTTEXT_LENGTH) {
            if (!confirm('Превышен рекомендуемый лимит ' + MAX_INPUTTEXT_LENGTH + ' символов (на ' + (text.length - MAX_INPUTTEXT_LENGTH) + ' символов).\r\nТекст будет обрезан, продолжить?')) {
                return (null);
            }
            text = text.substr( 0, MAX_INPUTTEXT_LENGTH );
            $text.val( text );
            $text.change();
        }
        return (text);
    };

    $("#text").focus(textOnChange).change(textOnChange).keydown(textOnChange).keyup(textOnChange).select(textOnChange).focus();

    (function () {
        function isGooglebot() {
            return (navigator.userAgent.toLowerCase().indexOf('googlebot/') != -1);
        };
        if (isGooglebot())
            return;

        var text = localStorage.getItem(LOCALSTORAGE_TEXT_KEY);
        if (!text || !text.length) {
            text = DEFAULT_TEXT;
        }
        $('#text').text(text).focus();
    })();

    $('#mainPageContent').on('click', '#processButton', function () {
        if($(this).hasClass('disabled')) return (false);

        var text = getText( $("#text") );
        if (!text) return (false);

        processing_start();
        if (text != DEFAULT_TEXT) {
            localStorage.setItem(LOCALSTORAGE_TEXT_KEY, text);
        } else {
            localStorage.removeItem(LOCALSTORAGE_TEXT_KEY);
        }

        $.ajax({
            type: "POST",
            url:  "RESTProcessHandler.ashx",
            data: {
                splitBySmiles: true,
                html         : false,
                text         : text
            },
            success: function (responce) {
                if (responce.err) {
                    if (responce.err == "goto-on-captcha") {
                        window.location.href = "Captcha.aspx";
                    } else {
                        processing_end();
                        $('.result-info').addClass('error').text(responce.err);
                    }
                } else {
                    if (responce.words && responce.words.length != 0) {
                        $('.result-info').removeClass('error').text('');
                        var ner_html = '<tr><td>';
                        var startIndex = 0;
                        for (var i = 0, len = responce.words.length; i < len; i++) {
                            var word = responce.words[i];
                            ner_html += text.substr( startIndex, word.i - startIndex ) +
                                        '<span class="' + word.ner + '">' + text.substr( word.i, word.l ) + '</span>';
                            startIndex = word.i + word.l;
                        }
                        ner_html += text.substr(startIndex, text.length - startIndex) +
                                    '</td></tr>';
                        ner_html = ner_html.replaceAll('\r\n', '<br/>').replaceAll('\n', '<br/>').replaceAll('\t', '&nbsp;&nbsp;&nbsp;&nbsp;');
                        $('#processResult tbody').html( ner_html );
                        processing_end();
                        $('.result-info').hide();
                        apply_ner_titles();
                    } else if (responce.html) {                        
                        $('.result-info').removeClass('error').text('');
                        $('#processResult tbody').html( responce.html );
                        processing_end();
                    } else {
                        processing_end();
                        $('.result-info').text('именованных сущностей в тексте не найденно');
                    }
                }
            },
            error: function () {
                processing_end();
                $('.result-info').text('ошибка сервера');
            }
        });
        
    });

    force_load_model();

    function processing_start(){
        $('#text').addClass('no-change').attr('readonly', 'readonly').attr('disabled', 'disabled');
        $('.result-info').show().removeClass('error').text('Идет обработка...');
        $('#processButton').addClass('disabled');
        $('#processResult tbody').empty();
    };
    function processing_end(){
        $('#text').removeClass('no-change').removeAttr('readonly').removeAttr('disabled');
        $('.result-info').removeClass('error').text('');
        $('#processButton').removeClass('disabled');
    };
    function trim_text(text) {
        return (text.replace(/(^\s+)|(\s+$)/g, ""));
    };
    function is_text_empty(text) {
        return (text.replace(/(^\s+)|(\s+$)/g, "") == "");
    };
    function force_load_model() {
        $.ajax({
            type: "POST",
            url: "RESTProcessHandler.ashx",
            data: { splitBySmiles: true, html: false, text: "_dummy_" }
        });
    };

    String.prototype.insert = function (index, str) {
        if (0 < index)
            return (this.substring( 0, index ) + str + this.substring( index, this.length ));
        return (str + this);
    };
    String.prototype.replaceAll = function (token, newToken, ignoreCase) {
        var _token;
        var str = this + "";
        var i   = -1;
        if (typeof token === "string") {
            if (ignoreCase) {
                _token = token.toLowerCase();
                while (( i = str.toLowerCase().indexOf( token, i >= 0 ? i + newToken.length : 0 )) !== -1) {
                    str = str.substring(0, i) + newToken + str.substring(i + token.length);
                }
            } else {
                return this.split(token).join(newToken);
            }
        }
        return (str);
    };

    function apply_ner_titles() {
        $("span.B_NAME, span.I_NAME, span.NAME").attr("title", "Физ. лица");
        $("span.B_ORG, span.I_ORG, span.ORG").attr("title", "Юр. лица");
        $("span.B_GEO, span.I_GEO, span.GEO").attr("title", "Географические объекты");
        $("span.B_ENTR, span.I_ENTR, span.ENTR").attr("title", "События");
        $("span.B_PROD, span.I_PROD, span.PROD").attr("title", "Торговые марки/Продукты");

        $("span.O").attr("title", "O  - !!!!!!!");
        $("span.__UNDEFINED__").attr("title", "__UNDEFINED__ - !!!!!!!");
        $("span.__UNKNOWN__").attr("title", "__UNKNOWN__ - !!!!!!!!");
    };
});
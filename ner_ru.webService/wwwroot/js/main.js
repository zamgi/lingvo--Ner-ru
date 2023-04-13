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
        let len = $('#text').val().length, len_txt = len.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
        $('#textLength').toggleClass('max-inputtext-length', MAX_INPUTTEXT_LENGTH < len).html('length of text: ' + len_txt + ' characters');
    };
    var getText = function( $text ) {
        var text = trim_text( $text.val().toString() );
        if (is_text_empty(text)) {
            alert('Введите текст для обработки.');
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

    $('#text').focus(textOnChange).change(textOnChange).keydown(textOnChange).keyup(textOnChange).select(textOnChange).focus();

    (function () {
        function isGooglebot() { return (navigator.userAgent.toLowerCase().indexOf('googlebot/') !== -1); };
        if (isGooglebot()) return;

        var text = localStorage.getItem(LOCALSTORAGE_TEXT_KEY);
        if (!text || !text.length) text = DEFAULT_TEXT;
        $('#text').val(text).focus();
    })();    
    $('#detailedView').click(function () {
        var $this = $(this), isDetailedView = $this.is(':checked');
        if (isDetailedView) {
            $this.parent().css({ 'color': 'cadetblue', 'font-weight': 'bold' });
        } else {
            $this.parent().css({ 'color': 'gray', 'font-weight': 'normal' });
        }

        if (_LastQuery) {
            var html;
            if (isDetailedView) {
                html = detailedView(_LastQuery.resp, _LastQuery.text);
            } else {
                html = shortView(_LastQuery.resp, _LastQuery.text);
            }
            $('#processResult tbody').html(html);
        }
    });
    $('#resetText2Default').click(function () {
        $('#text').val('');
        setTimeout(function () { $('#text').val(DEFAULT_TEXT).focus(); }, 100);
    });

    $('#processButton').click(function () {
        if($(this).hasClass('disabled')) return (false);

        var text = getText( $('#text') );
        if (!text) return (false);

        var isDetailedView = $('#detailedView').is(':checked');
        _LastQuery = undefined;

        processing_start();
        if (text !== DEFAULT_TEXT) {
            localStorage.setItem(LOCALSTORAGE_TEXT_KEY, text);
        } else {
            localStorage.removeItem(LOCALSTORAGE_TEXT_KEY);
        }        

        var model = {
            splitBySmiles: true,
            text         : text
        };
        $.ajax({
            type       : 'POST',
            contentType: 'application/json',
            dataType   : 'json',
            url        : '/Process/Run',
            data       : JSON.stringify( model ),
            success: function (resp) {
                if (resp.err) {
                    if (resp.err === 'goto-on-captcha') {
                        window.location.href = '/Captcha/GetNew';
                    } else {
                        processing_end();
                        $('.result-info').addClass('error').text(resp.err);
                    }
                } else {
                    if (resp.words && resp.words.length) {
                        $('.result-info').removeClass('error').text('');

                        var ner_htmls = ['<tr><td>'];
                        var startIndex = 0;
                        for (var i = 0, len = resp.words.length; i < len; i++) {
                            var word = resp.words[i];
                            ner_htmls.push( text.substr(startIndex, word.i - startIndex) +
                                            '<span class="' + word.ner + '">' + text.substr(word.i, word.l) + '</span>' );
                            startIndex = word.i + word.l;
                        }
                        ner_htmls.push( text.substr(startIndex, text.length - startIndex) );
                        ner_htmls.push( '</td></tr>' );
                        var html = ner_htmls.join('').replaceAll('\r\n', '<br/>').replaceAll('\n', '<br/>').replaceAll('\t', '&nbsp;&nbsp;&nbsp;&nbsp;');

                        $('#processResult tbody').html(html);
                        $('.result-info').hide();
                        processing_end();
                        _LastQuery = { resp: resp, text: text };
                    } else if (resp.html) {
                        $('.result-info').removeClass('error').text('');
                        $('#processResult tbody').html(resp.html);
                        processing_end();
                    } else {
                        processing_end();
                        $('.result-info').text('значимых сущностей в тексте не найденно');
                    }
                }
            },
            error: function () {
                processing_end();
                $('.result-info').text('ошибка сервера');
            }
        });
    });

    function processing_start(){
        $('#text').addClass('no-change').attr('readonly', 'readonly').attr('disabled', 'disabled');
        $('.result-info').show().removeClass('error').html('Идет обработка... <label id="processingTickLabel"></label>');
        $('#processButton').addClass('disabled');
        $('#processResult tbody').empty();
        processingTickCount = 1; setTimeout(processing_tick, 1000);
    };
    function processing_end(){
        $('#text').removeClass('no-change').removeAttr('readonly').removeAttr('disabled');
        $('.result-info').removeClass('error').text('');
        $('#processButton').removeClass('disabled');
    };
    function trim_text(text) { return (text.replace(/(^\s+)|(\s+$)/g, '')); };
    function is_text_empty(text) { return (!trim_text(text)); };

    var processingTickCount = 1,
        processing_tick = function() {
            var n2 = function (n) {
                n = n.toString();
                return ((n.length === 1) ? ('0' + n) : n);
            }
            var d = new Date(new Date(new Date(new Date().setHours(0)).setMinutes(0)).setSeconds(processingTickCount));
            var t = n2(d.getHours()) + ':' + n2(d.getMinutes()) + ':' + n2(d.getSeconds()); //d.toLocaleTimeString();
            var $s = $('#processingTickLabel');
            if ($s.length) {
                $s.text(t);
                processingTickCount++;
                setTimeout(processing_tick, 1000);
            } else {
                processingTickCount = 1;
            }
        };
});

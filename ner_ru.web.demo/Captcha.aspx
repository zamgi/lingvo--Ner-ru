<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Captcha.aspx.cs" Inherits="lingvo.Captcha" %>

<%@ Register Assembly="captcha" Namespace="captcha" TagPrefix="captcha" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Определение именованных сущностей (NER) в тексте на русском языке</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <style type="text/css">
        body {
            background: #b6b7bc;
            font-size: .80em;
            font-family: "Helvetica Neue", "Lucida Grande", "Segoe UI", Arial, Helvetica, Verdana, sans-serif;
            margin: 0px;
            padding: 0px;
            color: #696969;
        }
        .page {
            width: 960px;
            background-color: #fff;
            margin: 0px auto 0px auto;
            border: 1px solid #496077;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="page" style="background-color:#FBFBFB; margin-top: 30px;">
    <div style="text-align: center; padding: 30px;">
        <table style="width: 100%; height: 100%">
        <tr><td align="center">
        <table style="border: 1px solid silver; border-collapse: separate; border-spacing: 5px;">
            <tr>
                <td style="text-align: center">
                    <captcha:CaptchaControl ID="captchaControl" runat="server" 
                                            CaptchaHeight="60" 
                                            CaptchaWidth="200"
                                            CaptchaLineNoise="Medium" 
                                            CaptchaBackgroundNoise="Low" 
                                            CaptchaLength="5" 
                                            CaptchaMinTimeout="1"
                                            CaptchaMaxTimeout="240"
                                            CaptchaIgnoreCase="false"
                                            CaptchaImageHandlerUrl="CaptchaImageHandler.ashx" />
                </td>
            </tr>
            <tr>
                <td style="text-align: center">
                    <asp:Label ID="captchaError" runat="server" Visible="false" ForeColor="Red" />
                </td>
            </tr>
            <tr>
                <td style="border: 1px solid silver;">
                    <label style="padding-left: 40px; padding-right: 5px;">enter the code / вводим код:</label>
                    <asp:TextBox ID="captchaText" runat="server" autocomplete="off" />
                </td>
            </tr>
            <tr>
                <td style="padding-top: 15px; padding-bottom: 15px; text-align: center">
                    <asp:Button ID="captchaButton" runat="server" Height="25" Text="Confirm / Подтвердить" OnClick="captchaButton_Click" />
                </td>
            </tr>
            <tr>
                <td style="padding-top: 15px; padding-bottom: 15px; text-align: center">
                    <label>...or wait for / или ждем</label>
                    <label id="waitRemainSecondsLabel" style="font-weight: bold;">?</label>
                    <label>...</label>
                </td>
            </tr>
        </table>
        </td></tr> </table>
    </div>
    <script type="text/javascript">
        var waitRemainSeconds = <%= WaitRemainSeconds.ToString() %>;
        tick();

        function tick() {
            var n2 = function (n) {
                n = n.toString();
                return ((n.length == 1) ? ('0' + n) : n);
            }
            var d = new Date(new Date(new Date(new Date().setHours(0)).setMinutes(0)).setSeconds(waitRemainSeconds));
            var t = n2(d.getHours()) + ':' + n2(d.getMinutes()) + ':' + n2(d.getSeconds()); //d.toLocaleTimeString();
            var waitRemainSecondsLabel = document.getElementById("waitRemainSecondsLabel");
            if (waitRemainSecondsLabel.innerText !== undefined) {
                waitRemainSecondsLabel.innerText = t;
            } else {
                waitRemainSecondsLabel.textContent = t;
            }
            if ( waitRemainSeconds <= 0 ) {
                window.location.href = "<%= AllowContinuePageUrlJavaScript %>";
            } else {
                waitRemainSeconds -= 1;
                window.setTimeout( tick, 1000 )
            }
        }
    </script>
    </div>
    </form>
</body>
</html>

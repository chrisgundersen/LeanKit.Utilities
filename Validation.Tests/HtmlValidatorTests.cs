namespace Validation.Html.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class HtmlValidatorTests
    {
        private static readonly HtmlValidator Validator = HtmlValidator.Instance;

        [Test]
        public void Does_Not_Modify_Good_Html()
        {
            const string Expected = @"<div style=""background-color: test"">"
                                    + @"Test<img style=""margin: 10px"" src=""test.gif""></div>";

            RunStandardNonModifyingTest(Expected, Expected);
        }

        [Test]
        public void Removes_Div_Onload()
        {
            const string TestValue = @"<div onload=""alert('xss')"""
                                             + @"style=""background-color: test"">Test<img src=""test.gif"""
                                             + @"style=""margin: 10px""></div>";

            const string Expected = @"<div style=""background-color: test"">"
                                    + @"Test<img style=""margin: 10px"" src=""test.gif""></div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Does_Not_Remove_Div_Onload_When_Whitelisted()
        {
            const string TestValue = @"<div onload=""alert('xss')"""
                                             + @"style=""background-color: test"">Test<img src=""test.gif"""
                                             + @"style=""margin: 10px""></div>";

            const string Expected = @"<div style=""background-color: test"" onload=""alert('xss')"">"
                                    + @"Test<img style=""margin: 10px"" src=""test.gif""></div>";

            string sanitized;
            bool wasModified;

            var allowedAttributes = new HashSet<string> { "onload" };

            Validator.IsValidHtml(TestValue, out sanitized, out wasModified, null, null, allowedAttributes);

            Assert.True(wasModified);
            Assert.AreEqual(Expected, sanitized);
        }

        #region XSS Cheat Sheet https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet

        [Test, Ignore]
        public void XSS_Locator()
        {
            const string TestValue = @"<div>Test</div>';alert(String.fromCharCode(88,83,83))//';alert(String.fromCharCode(88,83,83))//"";"
                + @"alert(String.fromCharCode(88, 83, 83))//"";alert(String.fromCharCode(88,83,83))//--"
                + @"></ SCRIPT > "">'><SCRIPT>alert(String.fromCharCode(88,83,83))</SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void XSS_Locator_2()
        {
            const string TestValue = @"<div>Test</div>'';!--"" < XSS >= &{ ()}";

            const string Expected = @"<div>Test</div>";
            /*
            <div>Test</div>'';!--&quot; &lt; XSS &gt;= &amp;{ ()}
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void No_filter_evasion()
        {
            const string TestValue = @"<div>Test</div><SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Image_XSS_using_the_JavaScript_directive()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""javascript: alert('XSS'); "">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void No_quotes_and_no_semicolon()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=javascript:alert('XSS')>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Case_insensitive_XSS_attack_vector()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=JaVaScRiPt:alert('XSS')>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void HTML_entities()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=javascript:alert(""XSS"")>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Grave_accent_obfuscation()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=`javascript:alert(""RSnake says, 'XSS'"")`>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Malformed_A_tags_with_quotes()
        {
            const string TestValue = @"<div>Test</div><a onmouseover=""alert(document.cookie)"">xxs link</a>";

            const string Expected = @"<div>Test</div><a>xxs link</a>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Malformed_A_tags_withput_quotes()
        {
            const string TestValue = @"<div>Test</div><a onmouseover=alert(document.cookie)>xxs link</a>";

            const string Expected = @"<div>Test</div><a>xxs link</a>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void Malformed_IMG_tags()
        {
            const string TestValue = @"<div>Test</div><IMG "" >< SCRIPT > alert(""XSS"") </ SCRIPT > "">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void From_Char_Code()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=javascript:alert(String.fromCharCode(88,83,83))>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void Default_SRC_tag_to_get_past_filters_that_check_SRC_domain()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=# onmouseover=""alert('xxs')"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void Default_SRC_tag_by_leaving_it_empty()
        {
            const string TestValue = @"<div>Test</div><IMG SRC= onmouseover=""alert('xxs')"">";

            const string Expected = @"<div>Test</div><img>";
            /*
            <div>Test</div><img src="onmouseover=%22alert('xxs')%22">
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Default_SRC_tag_by_leaving_it_out_entirely()
        {
            const string TestValue = @"<div>Test</div><IMG onmouseover=""alert('xxs')"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void On_error_alert()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=/ onerror=""alert(String.fromCharCode(88, 83, 83))""></img>";

            const string Expected = @"<div>Test</div><img>";
            /*
            <div>Test</div><img src="x">
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void IMG_onerror_and_javascript_alert_encode()
        {
            const string TestValue = @"<div>Test</div><img src=x onerror="" &#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041"">";

            const string Expected = @"<div>Test</div><img>";
            /*
            <div>Test</div><img src="x">
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Decimal_HTML_character_references()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;"
                + @"&#39;&#88;&#83;&#83;&#39;&#41;>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Decimal_HTML_character_references_without_trailing_semicolons()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&"
                + @"#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Hexadecimal_HTML_character_references_without_trailing_semicolons()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Embedded_tab()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""jav ascript:alert('XSS'); "">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Embedded_encoded_tab()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""jav &#x09;ascript:alert('XSS');"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Embedded_newline_to_break_up_XSS()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""jav &#x0A;ascript:alert('XSS');"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Embedded_carriage_return_to_break_up_XSS()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""jav &#x0D;ascript:alert('XSS');"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Spaces_and_meta_chars_before_the_JavaScript_in_images_for_XSS()
        {
            const string TestValue = @"<div>Test</div><IMG SRC="" &#14;  javascript:alert('XSS');"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Non_alpha_non_digit_XSS()
        {
            const string TestValue = @"<div>Test</div><SCRIPT/XSS SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Non_alpha_non_digit_XSS_using_Rnake_fuzzer()
        {
            const string TestValue = @"<div>Test</div><BODY onload!#$%&()*~+-_.,:;?@[/|\]^`=alert(""XSS"")>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Non_alpha_non_digit_XSS_with_no_spaces()
        {
            const string TestValue = @"<div>Test</div><SCRIPT/SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void Extraneous_open_brackets()
        {
            const string TestValue = @"<div>Test</div><<SCRIPT>alert(""XSS"");//<</SCRIPT>";

            const string Expected = @"<div>Test</div>";
            /*
            <div>Test</div>&lt;
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void No_closing_script_tags()
        {
            const string TestValue = @"<div>Test</div><SCRIPT SRC=http://ha.ckers.org/xss.js?< B >";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Protocol_resolution_in_script_tags()
        {
            const string TestValue = @"<div>Test</div><SCRIPT SRC=//ha.ckers.org/.j>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Half_open_HTML_JavaScript_XSS_vector()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""javascript: alert('XSS')""";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Double_open_angle_brackets()
        {
            const string TestValue = @"<div>Test</div><iframe src=http://ha.ckers.org/scriptlet.html <";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void Escaping_JavaScript_escapes()
        {
            const string TestValue = @"<div>Test</div>\""; alert('XSS');//";

            const string Expected = @"<div>Test</div>";
            /*
            <div>Test</div>\\&quot;; alert('XSS');//
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Escaping_JavaScript_escapes_finish_block()
        {
            const string TestValue = @"<div>Test</div></script><script>alert('XSS');</script>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void End_title_tag()
        {
            const string TestValue = @"<div>Test</div></TITLE><SCRIPT>alert(""XSS"");</SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Input_image()
        {
            const string TestValue = @"<div>Test</div><INPUT TYPE=""IMAGE"" SRC=""javascript: alert('XSS'); "">";

            const string Expected = @"<div>Test</div><input type=""IMAGE"">";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Body_image()
        {
            const string TestValue = @"<div>Test</div><BODY BACKGROUND=""javascript: alert('XSS')"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Img_Dynsrc()
        {
            const string TestValue = @"<div>Test</div><IMG DYNSRC=""javascript: alert('XSS')"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Img_lowsrc()
        {
            const string TestValue = @"<div>Test</div><IMG LOWSRC=""javascript: alert('XSS')"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void List_style_image()
        {
            const string TestValue = @"<div>Test</div><STYLE>li {list-style-image: url(""javascript: alert('XSS')"");}</STYLE><UL><LI>XSS</br>";

            const string Expected = @"<div>Test</div><ul><li>XSS<br></li></ul>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void VBscript_in_an_image()
        {
            const string TestValue = @"<div>Test</div><IMG SRC='vbscript:msgbox(""XSS"")'>";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Livescript_older_versions_of_Netscape_only()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""livescript:[code]"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Body_tag()
        {
            const string TestValue = @"<div>Test</div><BODY ONLOAD=alert('XSS')>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void BG_sound()
        {
            const string TestValue = @"<div>Test</div><BGSOUND SRC=""javascript: alert('XSS'); "">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Ampersand_Javascript_includes()
        {
            const string TestValue = @"<div>Test</div><BR SIZE="" &{ alert('XSS')}"">";

            const string Expected = @"<div>Test</div><br>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Style_sheet()
        {
            const string TestValue = @"<div>Test</div><LINK REL=""stylesheet"" HREF=""javascript: alert('XSS'); "">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Remote_style_sheet_1()
        {
            const string TestValue = @"<div>Test</div><LINK REL=""stylesheet"" HREF=""http://ha.ckers.org/xss.css"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Remote_style_sheet_2()
        {
            const string TestValue = @"<div>Test</div><STYLE>@import'http://ha.ckers.org/xss.css';</STYLE>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Remote_style_sheet_3()
        {
            const string TestValue = @"<div>Test</div><META HTTP-EQUIV=""Link"" Content="" < http://ha.ckers.org/xss.css>; REL=stylesheet"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Remote_style_sheet_4()
        {
            const string TestValue = @"<div>Test</div><STYLE>BODY{-moz-binding:url(""http://ha.ckers.org/xssmoz.xml#xss"")}</STYLE>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void STYLE_tags_with_broken_up_JavaScript_for_XSS()
        {
            const string TestValue = @"<div>Test</div><STYLE>@im\port'\ja\vasc\ript:alert(""XSS"")';</STYLE>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void STYLE_attribute_using_a_comment_to_break_up_expression()
        {
            const string TestValue = @"<div>Test</div><IMG STYLE=""xss: expr/*XSS*/ession(alert('XSS'))"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void IMG_STYLE_with_expression()
        {
            const string TestValue = @"<div>Test</div>exp/*<A STYLE='no\xss:noxss("" *//*"");"
                + @"xss:ex/*XSS*//*/*/pression(alert(""XSS""))'>";

            const string Expected = @"<div>Test</div><img>";
            /*
            <div>Test</div>exp/*<a></a>
            */

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Style_tag_older_versions_of_Netscape_only()
        {
            const string TestValue = @"<div>Test</div><STYLE TYPE=""text / javascript"">alert('XSS');</STYLE>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Style_tag_using_background_image()
        {
            const string TestValue = @"<div>Test</div><STYLE>.XSS{background-image:url(""javascript: alert('XSS')"");}</STYLE><A CLASS=XSS></A>";

            const string Expected = @"<div>Test</div><a></a>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Style_tag_using_background()
        {
            const string TestValue = @"<div>Test</div><STYLE type=""text / css"">BODY{background:url(""javascript: alert('XSS')"")}</STYLE>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Anonymous_HTML_with_STYLE_attribute()
        {
            const string TestValue = @"<div>Test</div><XSS STYLE=""xss: expression(alert('XSS'))"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Local_htc_file()
        {
            const string TestValue = @"<div>Test</div><XSS STYLE=""behavior: url(xss.htc); "">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void US_ASCII_encoding()
        {
            const string TestValue = @"<div>Test</div>¼script¾alert(¢XSS¢)¼/script¾";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Meta()
        {
            const string TestValue = @"<div>Test</div><META HTTP-EQUIV=""refresh"" CONTENT=""0; url = javascript:alert('XSS'); "">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Meta_using_data()
        {
            const string TestValue = @"<div>Test</div><META HTTP-EQUIV=""refresh"" CONTENT=""0; url = data:text / html base64,PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4K"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Meta_with_additional_URL_parameter()
        {
            const string TestValue = @"<div>Test</div><META HTTP-EQUIV=""refresh"" CONTENT=""0; URL = http://;URL=javascript:alert('XSS');"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Iframe()
        {
            const string TestValue = @"<div>Test</div><IFRAME SRC=""javascript: alert('XSS'); ""></IFRAME>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Iframe_event_based()
        {
            const string TestValue = @"<div>Test</div><IFRAME SRC=# onmouseover=""alert(document.cookie)""></IFRAME>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Frame()
        {
            const string TestValue = @"<div>Test</div><FRAMESET><FRAME SRC=""javascript: alert('XSS'); ""></FRAMESET>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Table()
        {
            const string TestValue = @"<div>Test</div><TABLE BACKGROUND=""javascript: alert('XSS')"">";

            const string Expected = @"<div>Test</div><table></table>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void TD()
        {
            const string TestValue = @"<div>Test</div><TABLE><TD BACKGROUND=""javascript: alert('XSS')"">";

            const string Expected = @"<div>Test</div><table><tbody><tr><td></td></tr></tbody></table>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Div_background_image()
        {
            const string TestValue = @"<div>Test</div><DIV STYLE=""background - image: url(javascript: alert('XSS'))"">";

            const string Expected = @"<div>Test</div><div></div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Div_background_image_with_unicoded_XSS_exploit()
        {
            const string TestValue = @"<div>Test</div><DIV STYLE=""background - image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028.1027\0058.1053\0053\0027\0029'\0029"">";

            const string Expected = @"<div>Test</div><div></div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Div_background_image_plus_extra_characters()
        {
            const string TestValue = @"<div>Test</div><DIV STYLE=""background - image: url(&#1;javascript:alert('XSS'))"">";

            const string Expected = @"<div>Test</div><div></div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Div_expression()
        {
            const string TestValue = @"<div>Test</div><DIV STYLE=""width: expression(alert('XSS')); "">";

            const string Expected = @"<div>Test</div><div></div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Downlevel_hidden_block()
        {
            const string TestValue = @"<div>Test</div>"
                + @"<!--[if gte IE 4]>"
                + @"<SCRIPT>alert('XSS');</SCRIPT>"
                + @"<![endif]-->";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Base_tag()
        {
            const string TestValue = @"<div>Test</div><BASE HREF=""javascript: alert('XSS');//"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Object_tag()
        {
            const string TestValue = @"<div>Test</div><OBJECT TYPE=""text / x - scriptlet"" DATA=""http://ha.ckers.org/scriptlet.html""></OBJECT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Using_an_EMBED_tag_you_can_embed_a_Flash_movie_that_contains_XSS()
        {
            const string TestValue = @"<div>Test</div><EMBED SRC=""http://ha.ckers.Using an EMBED tag you can embed a Flash movie that contains XSS. Click here for a demo. If you add the attributes allowScriptAccess=""never"" and allownetworking=""internal"" it can mitigate this risk (thank you to Jonathan Vanasco for the info).:"
                + @"org / xss.swf"" AllowScriptAccess=""always""></EMBED>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void You_can_EMBED_SVG_which_can_contain_your_XSS_vector()
        {
            const string TestValue = @"<div>Test</div><EMBED SRC=""data: image / svg + xml; base64,PHN2ZyB4bWxuczpzdmc9Imh0dH A6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcv MjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hs aW5rIiB2ZXJzaW9uPSIxLjAiIHg9IjAiIHk9IjAiIHdpZHRoPSIxOTQiIGhlaWdodD0iMjAw IiBpZD0ieHNzIj48c2NyaXB0IHR5cGU9InRleHQvZWNtYXNjcmlwdCI + YWxlcnQoIlh TUyIpOzwvc2NyaXB0Pjwvc3ZnPg == "" type=""image / svg + xml"" AllowScriptAccess=""always""></EMBED>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XML_data_island_with_CDATA_obfuscation()
        {
            const string TestValue = @"<div>Test</div><XML ID=""xss""><I><B><IMG SRC=""javas < !----> cript:alert('XSS')""></B></I></XML>"
                + @"<SPAN DATASRC = ""#xss"" DATAFLD = ""B"" DATAFORMATAS = ""HTML"" ></SPAN>";

            const string Expected = @"<div>Test</div><span></span>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Locally_hosted_XML_with_embedded_JavaScript_that_is_generated_using_an_XML_data_island()
        {
            const string TestValue = @"<div>Test</div><XML SRC=""xsstest.xml"" ID=I></XML>"
                + @"<SPAN DATASRC =#I DATAFLD=C DATAFORMATAS=HTML></SPAN>";

            const string Expected = @"<div>Test</div><span></span>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XML_plus_TIME_in_XML()
        {
            const string TestValue = @"<HTML><BODY>"
                + @"<?xml:namespace prefix=""t"" ns=""urn: schemas - microsoft - com:time"">"
                + @"<? import namespace=""t"" implementation=""#default#time2"">"
                + @"<t:set attributeName = ""innerHTML"" to=""XSS<SCRIPT DEFER>alert(""XSS"")</SCRIPT>"">"
                + @"</BODY><div>Test</div></HTML>";

            RunStandardModifyingTest(TestValue, string.Empty);
        }

        [Test]
        public void Assuming_you_can_only_fit_in_a_few_characters_and_it_filters_against_js()
        {
            const string TestValue = @"<div>Test</div><SCRIPT SRC=""http://ha.ckers.org/xss.jpg""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Server_side_includes()
        {
            const string TestValue = @"<div>Test</div><!--#exec cmd=""/bin/echo '<SCR'""--><!--#exec cmd=""/bin/echo 'IPT SRC=http://ha.ckers.org/xss.js></SCRIPT>'""-->";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void IMG_embedded_commands()
        {
            const string TestValue = @"<div>Test</div><IMG SRC=""http://www.thesiteyouareon.com/somecommand.php?somevariables=maliciouscode"">";

            const string Expected = @"<div>Test</div><img>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void Cookie_manipulation()
        {
            const string TestValue = @"<div>Test</div><META HTTP-EQUIV=""Set-Cookie"" Content=""USERID=<SCRIPT>alert('XSS')</SCRIPT>"">";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void UTF7_encoding()
        {
            const string TestValue = @"<HEAD><META HTTP-EQUIV=""CONTENT-TYPE"" CONTENT=""text/html; charset=UTF-7""> </HEAD>+ADw-SCRIPT+AD4-alert('XSS');+ADw-/SCRIPT+AD4-";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_1()
        {
            const string TestValue = @"<div>Test</div><SCRIPT a="" > "" SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_2()
        {
            const string TestValue = @"<div>Test</div><SCRIPT ="" > "" SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_3()
        {
            const string TestValue = @"<div>Test</div><SCRIPT a="" > "" '' SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_4()
        {
            const string TestValue = @"<div>Test</div><SCRIPT ""a = '>'"" SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_5()
        {
            const string TestValue = @"<div>Test</div><SCRIPT a=`>` SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test]
        public void XSS_using_HTML_quote_encapsulation_6()
        {
            const string TestValue = @"<div>Test</div><SCRIPT a="" > '>"" SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        [Test, Ignore]
        public void XSS_using_HTML_quote_encapsulation_7()
        {
            const string TestValue = @"<div>Test</div><SCRIPT>document.write("" < SCRI"");</SCRIPT>PT SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";

            const string Expected = @"<div>Test</div>";

            RunStandardModifyingTest(TestValue, Expected);
        }

        #endregion

        #region Helper Methods

        private static void RunStandardModifyingTest(string testValue, string expectedValue)
        {
            string sanitized;
            bool wasModified;

            Validator.IsValidHtml(testValue, out sanitized, out wasModified);

            Assert.True(wasModified);
            Assert.AreEqual(expectedValue, sanitized);
        }

        private static void RunStandardNonModifyingTest(string testValue, string expectedValue)
        {
            string sanitized;
            bool wasModified;

            Validator.IsValidHtml(testValue, out sanitized, out wasModified);

            Assert.False(wasModified);
            Assert.AreEqual(expectedValue, sanitized);
        }

        #endregion
    }
}
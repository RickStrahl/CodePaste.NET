/* ***** BEGIN LICENSE BLOCK *****
 * Distributed under the BSD license:
 *
 * Copyright (c) 2012, Ajax.org B.V.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Ajax.org B.V. nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL AJAX.ORG B.V. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *
 * Contributor(s):
 * Rick Strahl, West Wind Technologies (west-wind.com)
 *
 *
 * ***** END LICENSE BLOCK ***** */

define('ace/mode/vbnet', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text', 'ace/tokenizer', 'ace/mode/vbnet_highlight_rules'], function (require, exports, module) {


    var oop = require("../lib/oop");
    var TextMode = require("./text").Mode;
    var Tokenizer = require("../tokenizer").Tokenizer;
    var VbNetHighlightRules = require("./vbnet_highlight_rules").VbNetHighlightRules;

    var Mode = function () {
        this.HighlightRules = VbNetHighlightRules;
    };
    oop.inherits(Mode, TextMode);

    (function () {

        this.lineCommentStart = ["*", "&&"];

        this.$id = "ace/mode/vbnet";
    }).call(Mode.prototype);

    exports.Mode = Mode;
});


define('ace/mode/vbnet_highlight_rules', ['require', 'exports', 'module', 'ace/lib/oop', 'ace/mode/text_highlight_rules'], function (require, exports, module)
{
var oop = require("../lib/oop");
var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;

var VbNetHighlightRules = function() {

    var functions = (
                "Abs|Asc|AscW|AppActivate|Array|Asc|AscB|AscW|Atn|Avg|" +
                "CBool|CByte|" +
                "CallByName|CCur|CDate|CDbl|Cdec|ChDir|ChDrive|Choose|Chr|ChrW|CInt|CLng|CObj|Command|Cos|Count|CreateObject|CSng|CStr|CType|CurDir|CVar|CVDate|CVErr|"+
                "Date|DateAdd|DateDiff|DatePart|DateSerial|DateValue|Day|DDB|DeleteSetting|Dir|DoEvents|Environ|"+
                "EOF|ErrorToString|Exp|" +
                "FileAttr|FileClose|FileCopy|FileDateTime|FileGet|FileLen|FilePut|FilePutObject|FileWidth|Filter|Fix|Format|FormatCurrency|FormatDateTime|FormatNumber|FormatPercent|FreeFile|FV|" +
                "GetAllSettings|GetAttr|GetChar|GetException|GetObject|GetSettings" +
                "Hex|Hour|"+
                "IIf|Input|InputBox|InputStr|Instr|Int|IPmt|IRR|IsArray|IsDate|IsDBNull|IsEmpty|IsError|IsNothing|IsNumeric|IsReference|"+
                "Join|" +
                "Kill|" +
                "LBound|LCase|Left|Len|LineInput|Loc|Lock|LOF|LSet|LTrim|"+
                "Mid|Minute|MIRR|MkDir|Month|MonthName|MsgBox|"+
                "Now|NPer|NPV|"+
                "Oct|"+
                "Partition|Pmt|PPmt|Print|PrintLine|PV|"+
                "QBColor|"+
                "Randomize|Rate|Rename|Replace|Reset|RGB|RmDir|Rnd|RSet|RTrim|"+
                "SaveSetting|Second|Seek|SetAttr|Shell|SLN|Space|Spc|Split|Str|StrComp|StrConv|StrDup|StrRevers|Switch|SYD|SystemTypeName"+
                "Tab|Time|TimeSerial|TimeValue|Trim|TypeName|" +
                "UBound|UCase|Unlock|"+
                "Val|VarType|vbTypeName|"+
                "Weekday|WeekdayName|Write|WriteLine|"+
                "Year" 
    );

    var constants = (
       "True|False|Null|Nothing|[0-9]"
    );

    var keywords = (        
            "AddHandler|AddressOf|Alias|And|AndAlso|As|" +
            "Boolean|ByRef|Byte|ByVal|" +
            "Call|Case|Catch|CBool|CByte|CChar|CDate|CDec|CDbl|Char|Cint|Class|Clng|CObj|Const|Continue|CSByte|CShort|Csng|SStr|CType|CUInt|CULng|CUShort|"+
            "Date|Decimal|Declare|Default|Delegate|Dim|DirectCast|Do|Double|" +
            "Each|End|Enum|Erase|Error|Event|Exit|" +
            "False|Finally|For|Friend|Function|"+ 
            "Get|GetType|GetXmlNamespace|Global|GoSub|Goto|" +
            "Handles|"+
            "If|Implements|Imports|In|Inherits|Integer|Interface|IsNot|" + 
            "Let|Lib|Like|Long|Loop|" +
            "Mod|Module|MustInherit|MustOverride|MyBase|MyClass|" +
            "Namespace|Narrowing|New|Next|Not|Nothing|NotInheritable|NotOverridable|" + 
            "Object|Of|On|Operator|Option|Optional|Or|OrElse|Overloads|Overridable|Overrides|" +
            "ParamArray|Partial|Private|Property|Protected|Public|" +
            "RaiseEvent|ReadOnly|Redim|REM|RemoveHandler|Resume|Return|" +
            "SByte|Select|Set|Shadows|Shared|Short|Single|Static|Step|Stop|String|Structure|Sub|SyncLock|" +
            "Throw|To|True|Try|TryCast|TypeOf|" +
            "Variant|"+
            "Wend|" +
            "UInteger|ULong|UShort|Using|Widening|With|WithEvents|WriteOnly|Xor"
    );

    var keywordMapper = this.createKeywordMapper({
        "support.function": functions,
        "keyword": keywords,
        "constant.language": constants
    }, "identifier", true);

    this.$rules = {
        "start": [
            {
                token: "comment",
                regex: "'.*$",
                //regex: "^\\s?\\*.*$"
            },
            {
                token: "comment",
                regex: "REM.*$"
            },
            {
                token : "keyword", // pre-compiler directives
                regex : "#\\s*(?:include|import|pragma|line|define|undef|if|ifdef|endif|else|elif|ifndef)\\b",
                next  : "directive"
            },
            {
                token: "string", // " string
                regex: '".*?"'
            },
            {
                token: "string", // ' string
                regex: "'.*?'"
            },
            {
                token: "string", // ' string
                regex: "<.*?>"
            },
            {
                token: "constant.numeric", // float
                regex: "[+-]?\\d+(?:(?:\\.\\d*)?(?:[eE][+-]?\\d+)?)?\\b"
            },
            {
                token: keywordMapper,
                regex: "[a-zA-Z_$][a-zA-Z0-9_$]*\\b"
            },
            {
                token: "keyword.operator",
                regex: "\\+|\\-|\\/|\\/\\/|%|<@>|@>|<@|&|\\^|~|<|>|<=|=>|==|!=|<>|="
            },
            {
                token: "paren.lparen",
                regex: "[\\(]"
            },
            {
                token: "paren.rparen",
                regex: "[\\)]"
            },
            {
                token: "text",
                regex: "\\s+"
            }
        ]
    };
};

oop.inherits(VbNetHighlightRules, TextHighlightRules);

exports.VbNetHighlightRules = VbNetHighlightRules;
});
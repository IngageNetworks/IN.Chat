/// <reference path="_references.js" />

(function ($, window) {
    "use strict";

    // REVIEW: is it safe to assume we do not need to strip tags before decoding?
    function decodeHtml(html) {
        // should we strip tags before running this?
        // obligatory link to SO http://stackoverflow.com/questions/1147359/how-to-decode-html-entities-using-jquery
        // is it safe to assume bad html has been removed before we've reached this function call?
        return $("<div/>").html(html).text();
    }

    function encodeHtml(html) {
        return $("<div/>").text(html).html();
    }

    var utility = {
        decodeHtml: decodeHtml,
        encodeHtml: encodeHtml,
        parseEmojis: function (content) {
            var parser = new Emoji.Parser().parse;
            return (parser(content));
        },
    };

    if (!window.chat) {
        window.chat = {};
    }

    window.chat.utility = utility;

})(jQuery, window);
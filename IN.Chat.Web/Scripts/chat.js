/// <reference path="_references.js" />

$(function () {
    var defaultTitle = document.title;
    var missedMessages = 0;
    var hasFocus = true;
    var util = window.chat.utility;
    var previousMessages = window.chat.previousMessages;
    var chat = $.connection.chat;
    var chatHistoryMax = 50;
    var $chatInput = $('#chat-input');
    var $chatMessages = $('#chat-messages');
    var $chatMessagesOld = $('#chat-messages-old');
    var $chatMessagesNew = $('#chat-messages-new');
    var $chatUsers = $('#chat-users');
    var $commands = $('#commands');
    var $contentProviders = $('#contentproviders');
    var loggedInUsername = $('#navbar-username').html();
    var $chatTips = $('#chat-tips');
    var tipMessageIndex = 0;
    var tipMessages = [
                'Use the [ &uarr; ] and [ &darr; ] keys to cycle through previous messages.',
                'Use the [Esc] key to clear your text while typing.',
            ];

    function cycleTipMessages() {
        setTimeout(function () {
            tipMessageIndex++;
            if (tipMessageIndex >= tipMessages.length) {
                tipMessageIndex = 0;
            }
            $chatTips.fadeOut(2000, function () {
                $chatTips.html(tipMessages[tipMessageIndex]);
            });
            $chatTips.fadeIn(2000, cycleTipMessages);
        }, 10 * 1000 /* 1 minute */);
    };


    $(window)
        .bind('focus', function (ev) {
            hasFocus = true;
            //Reset the title on window focus event
            document.title = defaultTitle;
            missedMessages = 0;
        })
        .bind('blur', function (ev) {
            hasFocus = false;
        });

    //Server callback for new messages
    chat.addMessage = function (message) {
        appendMessage(message, $chatMessagesNew);
    };

    //Server callback for user joined
    chat.joined = function () {
        updateUserList();
    };

    //Server callback for user leave
    chat.leave = function () {
        updateUserList();
    };

    //Server callback for user rejoined
    chat.rejoined = function () {
        updateUserList();
    };

    //Watch for 'enter' key press
    //Send message to the server
    $chatInput.keydown(function (event) {
        var Keys = { Up: 38, Down: 40, Esc: 27, Enter: 13, Slash: 47 };
        var keycode = (event.keyCode ? event.keyCode : event.which);

        switch (keycode) {
            case Keys.Esc:
                $chatInput.val("");
                previousMessages.reset();
                break;
            case Keys.Up:
                $chatInput.val(previousMessages.cycleUp());
                break;
            case Keys.Down:
                $chatInput.val(previousMessages.cycleDown());
                break;
            case Keys.Enter:
                var message = $('#chat-input').val();
                chat.send(message);
                previousMessages.addMessage(message);
                $chatInput.val("");
                event.preventDefault();
                return false;
        }
    });

    //Helper to add new messages to the UI
    function appendMessage(message, location) {
        var username = message.From;
        var type = message.Type;
        var content = util.parseEmojis(message.ProcessedContent)
        var labelinfo;

        //public
        if (type == 1) {
            if (username == loggedInUsername) {
                labelinfo = 'label-info';
            }
            else {
                labelinfo = 'label-inverse';
            }
        }
        //private
        else if (type == 2) {
            labelinfo = 'label-important';
        }

        var markup = '<p class="chat-message" data-username="${From}"><span class="label {{if $item.labelinfo}}${$item.labelinfo}{{/if}}"><i class="icon-user icon-white"></i> ${From}</span> {{html $item.content}}</p>';
        $.template("messageTemplate", markup);
        $.tmpl("messageTemplate", message, { labelinfo: labelinfo, content: content }).appendTo(location);

        //Clear old messages
        if (location.children('p').size() > chatHistoryMax) {
            location.children('p').first().remove();
        }

        //Scroll to bottom
        scrollChatToBottom();

        //Update the title, if out of focus
        if (!hasFocus) {
            missedMessages++;
            document.title = '(' + missedMessages + ') ' + defaultTitle
        }
    }

    //Helper to update the current user list
    function updateUserList() {
        chat.getConnectedUsers()
            .done(function (result) {
                $chatUsers.empty();
                $.each(result, function (index, username) {
                    $chatUsers.append('<p><span class="label label-info"><i class="icon-user icon-white"></i> ' + username + '</i></span></p>');
                });
            });
    }

    //Helper to populate the Content Provider Descriptions
    function appendContentProviderDescription(contentProviderDescription) {
        var markup = '<div class="contentprovider-description"><h4>${Name} <small>${Description}</small></h4></div>';
        $.template("contentProviderDescriptionTemplate", markup);
        $.tmpl("contentProviderDescriptionTemplate", contentProviderDescription).appendTo($contentProviders);
    }

    //Helper to populate the Command Descriptions
    function appendCommandDescription(commandDescription) {
        var markup = '<div class="command-description"><h4>${Name} <small>${Description}</small></h4><ul>{{each Usage}}<li>${$value}</li>{{/each}}</ul></div>';
        $.template("commandDescriptionTemplate", markup);
        $.tmpl("commandDescriptionTemplate", commandDescription).appendTo($commands);
    }

    //Helper to scroll the chat window to the bottom
    function scrollChatToBottom() {
        $chatMessages.scrollTop(99999999);
        setTimeout(function () {
            $chatMessages.scrollTop(99999999);
        }, 1000);
    }

    //Start hub connection
    $.connection.hub.start(function () {

        //Start cursor in the chat input
        $chatInput.focus();

        //Cycle chat tips
        cycleTipMessages();

        //On start, get recent messages
        chat.getRecentMessages()
            .done(function (result) {
                //Clear and populate messages
                $chatMessagesOld.empty();
                $.each(result, function (index, message) {
                    appendMessage(message, $chatMessagesOld);
                });
            });

        //On start, get current user list
        //updateUserList();

        //On start, get content provider descriptioins
        chat.getContentProviderDescriptions()
            .done(function (result) {
                $.each(result, function (index, contentProviderDescription) {
                    appendContentProviderDescription(contentProviderDescription);
                });
            });

        //On start, get command descriptions
        chat.getCommandDescriptions()
            .done(function (result) {
                $.each(result, function (index, commandDescription) {
                    appendCommandDescription(commandDescription);
                });
            });
    });
});

//Previous Message Module
(function ($, window) {
    "use strict";

    var currentIndex = 0;
    var messageArray = new Array();

    function addMessage(message){
        messageArray.push(message);
        trim();
        reset();
    }

    function trim() {
        if(messageArray.length > 16) {
            messageArray.splice(0, 1);
        }
    }

    function reset() {
        currentIndex = messageArray.length == 0 ? 0 : messageArray.length - 1;
    }

    function cycleUp() {
        if(messageArray.length == 0){
            return "";
        }
        else {
            if(currentIndex < 0)
            {
                currentIndex = messageArray.length - 1;
            }

            return messageArray[currentIndex--];
        }
    }

    function cycleDown() {
        if(messageArray.length == 0){
            return "";
        }
        else {
            if(currentIndex > messageArray.length - 1)
            {
                currentIndex = 0;
            }

            return messageArray[currentIndex++];
        }
    }

    var previousMessages = {
        addMessage: addMessage,
        reset: reset,
        cycleUp: cycleUp,
        cycleDown: cycleDown
    };

    if (!window.chat) {
        window.chat = {};
    }

    window.chat.previousMessages = previousMessages;

})(jQuery, window);
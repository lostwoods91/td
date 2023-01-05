using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

class Spammer
{
    public Spammer(Td.Client client)
    {
        _client = client;
    }

    private static Td.Client _client;
    private static string _usersListPath;
    private static double _delaySeconds;

    public void Spam(string usersListPath)
    {
        Spam(usersListPath, 0.0);
    }

    public void Spam(string usersListPath, double delaySeconds)
    {
        _usersListPath = usersListPath;
        _delaySeconds = delaySeconds;
        _client.Send(new TdApi.SearchChats("MessageToSpam", 1), new OnMessageToSpamChannelFoundHandler());
    }

    private class OnMessageToSpamChannelFoundHandler : Td.ClientResultHandler
    {
        void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
        {
            if (@object is TdApi.Chats)
            {
                TdApi.Chats chats = @object as TdApi.Chats;
                if (chats.ChatIds.Length > 0)
                {
                    long chatId = chats.ChatIds[0];
                    _client.Send(new TdApi.GetChat(chatId), new OnGetMessageToSpamChatHandler());
                }
            }
        }
    }

    private class OnGetMessageToSpamChatHandler : Td.ClientResultHandler
    {
        void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
        {
            if (@object is TdApi.Chat)
            {
                TdApi.Chat chat = @object as TdApi.Chat;
                Spam(chat.Id, chat.LastMessage.Id);
            }
        }
    }

    private static void Spam(long chatId, long messageId)
    {
        List<string> usernames = GetUsernamesFromUsersList(_usersListPath);

        foreach (string username in usernames)
        {
            _client.Send(new TdApi.SearchPublicChat(username), new OnPublicChatFoundHandler(chatId, messageId, _delaySeconds));
        }
    }

    private class OnPublicChatFoundHandler : Td.ClientResultHandler
    {
        private long _chatId;
        private long _messageId;
        private double _delaySeconds;
        public OnPublicChatFoundHandler(long chatId, long messageId, double delaySeconds)
        {
            _chatId = chatId;
            _messageId = messageId;
            _delaySeconds = delaySeconds;
        }
        void Td.ClientResultHandler.OnResult(TdApi.BaseObject @object)
        {
            if (@object is TdApi.Chat)
            {
                TdApi.Chat chat = @object as TdApi.Chat;
                //Console.WriteLine(chat.ToString());

                TdApi.MessageCopyOptions messageCopyOptions = new TdApi.MessageCopyOptions(true, false, null);
                TdApi.InputMessageContent inputMessageContent = new TdApi.InputMessageForwarded(_chatId, _messageId, false, messageCopyOptions);
                //TdApi.MessageSchedulingState messageSchedulingState = null;
                //TdApi.MessageSchedulingState messageSchedulingState = new TdApi.MessageSchedulingStateSendWhenOnline();
                DateTimeOffset dateTimeOffset = DateTimeOffset.Now.AddSeconds(_delaySeconds);
                TdApi.MessageSchedulingState messageSchedulingState = new TdApi.MessageSchedulingStateSendAtDate((int)dateTimeOffset.ToUnixTimeSeconds());
                TdApi.MessageSendOptions messageSendOptions = new TdApi.MessageSendOptions(true, true, true, false, messageSchedulingState);
                _client.Send(new TdApi.SendMessage(chat.Id, 0, 0, messageSendOptions, null, inputMessageContent), null);
            }
        }
    }

    private static List<string> GetUsernamesFromUsersList(string userslist)
    {
        List<string> usernames = new List<string>();

        string[] lines = File.ReadAllLines(userslist);

        for (int i = 1; i < lines.Length; ++i)
        {
            string[] tokens = lines[i].Split(new char[] { ',' }, 2);
            string username = tokens[0];
            if (username.Length > 0)
            {
                usernames.Add(tokens[0]);
            }
        }

        return usernames;
    }
}

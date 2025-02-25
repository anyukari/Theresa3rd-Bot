﻿using log4net;
using log4net.Config;
using log4net.Repository;
using Mirai.CSharp.HttpApi.Models.ChatMessages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Theresa3rd_Bot.Common;
using Theresa3rd_Bot.Exceptions;
using Theresa3rd_Bot.Model.Error;

namespace Theresa3rd_Bot.Util
{
    public static class LogHelper
    {
        private static readonly string RepositoryName = "NETCoreRepository";
        private static readonly string ConfigFile = "log4net.config";

        private static int LastSendHour = DateTime.Now.Hour;
        private static List<SendError> SendErrorList = new List<SendError>();

        private static ILog RollingLog { get; set; }
        private static ILog ConsoleLog { get; set; }
        private static ILog FileLog { get; set; }
        private static ILoggerRepository repository { get; set; }

        /// <summary>
        /// 初始化日志
        /// </summary>
        public static void ConfigureLog()
        {
            repository = LogManager.CreateRepository(RepositoryName);
            XmlConfigurator.Configure(repository, new FileInfo(ConfigFile));
            RollingLog = LogManager.GetLogger(RepositoryName, "RollingLog");
            ConsoleLog = LogManager.GetLogger(RepositoryName, "ConsoleLog");
            FileLog = LogManager.GetLogger(RepositoryName, "FileLog");
        }

        /// <summary>
        /// 记录Info级别的日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(object message)
        {
            FileLog.Info(message);
            ConsoleLog.Info(message);
        }

        /// <summary>
        /// 记录Error级别的日志
        /// </summary>
        /// <param name="ex"></param>
        public static void Error(Exception ex, bool sendError = true)
        {
            ConsoleLog.Error(ex.Message);
            RollingLog.Error("", ex);
            if (sendError) SendError(ex);
        }

        /// <summary>
        /// 记录Error级别的日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void Error(Exception ex, string message, bool sendError = true)
        {
            ConsoleLog.Error($"{message}：{ex.Message}");
            RollingLog.Error(message, ex);
            if (sendError) SendError(ex, message);
        }

        /// <summary>
        /// 记录FATAL级别的日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void FATAL(Exception ex, string message, bool sendError)
        {
            ConsoleLog.Error(ex.Message);
            RollingLog.Error(message, ex);
            if (sendError) SendErrorAnyway(ex, message);
        }

        /// <summary>
        /// 将错误日志发送到日志群中
        /// 每个小时最多发送10种类型且每种类型最多不超过3次的日志
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public static void SendError(Exception exception, string message = "")
        {
            try
            {
                if (BotConfig.GeneralConfig?.ErrorGroups == null) return;
                if (IsSendError(exception) == false) return;
                StringBuilder messageBuilder = new StringBuilder();
                if (string.IsNullOrWhiteSpace(message) == false)
                {
                    messageBuilder.AppendLine(message);
                }
                if (string.IsNullOrWhiteSpace(exception.Message) == false)
                {
                    messageBuilder.AppendLine(exception.Message);
                }
                if (exception is BaseException && exception.InnerException != null)
                {
                    messageBuilder.AppendLine(exception.InnerException?.Message ?? "");
                }
                messageBuilder.Append("详细请查看Log日志");
                foreach (var groupId in BotConfig.GeneralConfig.ErrorGroups)
                {
                    sendErrorToGroup(groupId, messageBuilder.ToString());
                }
                AddSendError(exception);
            }
            catch (Exception ex)
            {
                Error(ex, false);
            }
        }

        /// <summary>
        /// 将错误日志发送到日志群中，忽略发送限制
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        public static void SendErrorAnyway(Exception exception, string message)
        {
            try
            {
                if (BotConfig.GeneralConfig?.ErrorGroups == null) return;
                string sendMessage = $"{message}\r\n{exception.Message}\r\n{exception.StackTrace}";
                foreach (var groupId in BotConfig.GeneralConfig.ErrorGroups)
                {
                    sendErrorToGroup(groupId, sendMessage);
                }
            }
            catch (Exception ex)
            {
                Error(ex, false);
            }
        }

        private static void sendErrorToGroup(long groupId, string message)
        {
            try
            {
                MiraiHelper.Session.SendGroupMessageAsync(groupId, new PlainMessage(message)).Wait();
            }
            catch (Exception ex)
            {
                Error(ex, false);
            }
            finally
            {
                Task.Delay(1000).Wait();
            }
        }

        /// <summary>
        /// 判断这个小时能是否还可以发送日志
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static bool IsSendError(Exception exception)
        {
            if (LastSendHour != DateTime.Now.Hour) return true;
            if (SendErrorList.Count >= 10) return false;
            string message = BaseErrorMessage(exception);
            if (string.IsNullOrWhiteSpace(message)) return false;
            SendError sendError = SendErrorList.Where(m => m.Message == message || m.InnerMessage == message).FirstOrDefault();
            if (sendError == null) return true;
            return sendError.SendTimes < 3;
        }

        /// <summary>
        /// 添加发送记录
        /// </summary>
        /// <param name="exception"></param>
        private static void AddSendError(Exception exception)
        {
            if (LastSendHour != DateTime.Now.Hour) SendErrorList.Clear();
            LastSendHour = DateTime.Now.Hour;
            string message = BaseErrorMessage(exception);
            SendError sendError = SendErrorList.Where(m => m.Message == message || m.InnerMessage == message).FirstOrDefault();
            if (sendError == null)
            {
                SendErrorList.Add(new SendError(exception));
            }
            else
            {
                sendError.SendTimes++;
                sendError.LastSendTime = DateTime.Now;
            }
        }


        private static string BaseErrorMessage(Exception exception)
        {
            if (exception is BaseException) return exception.InnerException?.Message?.Trim() ?? exception.Message?.Trim();
            return exception.Message;
        }

    }
}

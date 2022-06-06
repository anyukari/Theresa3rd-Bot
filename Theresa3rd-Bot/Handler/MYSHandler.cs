﻿using Mirai.CSharp.HttpApi.Models.ChatMessages;
using Mirai.CSharp.HttpApi.Models.EventArgs;
using Mirai.CSharp.HttpApi.Session;
using Mirai.CSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Theresa3rd_Bot.Business;
using Theresa3rd_Bot.Cache;
using Theresa3rd_Bot.Common;
using Theresa3rd_Bot.Model.Cache;
using Theresa3rd_Bot.Model.Mys;
using Theresa3rd_Bot.Model.PO;
using Theresa3rd_Bot.Type;
using Theresa3rd_Bot.Util;


namespace Theresa3rd_Bot.Handler
{
    public class MYSHandler
    {
        private MYSBusiness mysBusiness;
        private SubscribeBusiness subscribeBusiness;

        public MYSHandler()
        {
            mysBusiness = new MYSBusiness();
            subscribeBusiness = new SubscribeBusiness();
        }

        /// <summary>
        /// 订阅米游社用户
        /// </summary>
        /// <param name="session"></param>
        /// <param name="args"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task subscribeMYSUserAsync(IMiraiHttpSession session, IGroupMessageEventArgs args, string message)
        {
            try
            {
                MysSectionType? mysSection = null;
                string userId = null;

                string[] paramArr = message.splitParam(BotConfig.SubscribeConfig.Mihoyo.AddCommand);
                if (paramArr == null || paramArr.Length == 0)
                {
                    StepInfo stepInfo = await StepCache.CreateStepAsync(session, args);
                    if (stepInfo == null) return;
                    StepDetail uidStep = new StepDetail(60, " 请在60秒内发送要订阅用户的id", CheckUserIdAsync);
                    StepDetail sectionStep = new StepDetail(60, $" 请在60秒内发送数字选择你要订阅的频道：\r\n{EnumHelper.MysSectionOption()}", CheckSectionAsync);
                    stepInfo.AddStep(uidStep);
                    stepInfo.AddStep(sectionStep);
                    bool isSuccess = await stepInfo.StartStep(session, args);
                    if (isSuccess == false) return;
                    userId = uidStep.Answer;
                    mysSection = (MysSectionType)Convert.ToInt32(sectionStep.Answer);
                }
                else
                {
                    userId = paramArr.Length > 0 ? paramArr[0] : null;
                    string mysSectionStr = paramArr.Length > 1 ? paramArr[1] : "0";
                    if (await CheckUserIdAsync(session, args, userId) == false) return;
                    if (await CheckSectionAsync(session, args, mysSectionStr) == false) return;
                    mysSection = (MysSectionType)Convert.ToInt32(mysSectionStr);
                }

                MysResult<MysUserFullInfoDto> userInfoDto = await mysBusiness.geMysUserFullInfoDtoAsync(userId, (int)mysSection.Value);
                if (userInfoDto == null || userInfoDto.retcode != 0)
                {
                    await session.SendMessageWithAtAsync(args, new PlainMessage(" 订阅失败，目标用户不存在"));
                    return;
                }

                SubscribePO fullSubscribe = subscribeBusiness.getSubscribe(userId, SubscribeType.米游社用户, (int)MysSectionType.全部);
                if (fullSubscribe != null && subscribeBusiness.getCountBySubscribe(args.Sender.Group.Id, fullSubscribe.Id) > 0)
                {
                    await session.SendMessageWithAtAsync(args, new PlainMessage($" 已订阅了米游社用户[{userId}]的[{Enum.GetName(typeof(MysSectionType), MysSectionType.全部)}]频道~"));
                    return;
                }

                SubscribePO dbSubscribe = subscribeBusiness.getSubscribe(userId, SubscribeType.米游社用户, (int)mysSection.Value);
                if (dbSubscribe == null) dbSubscribe = subscribeBusiness.insertSurscribe(userInfoDto.data.user_info, mysSection.Value, userId);

                if (subscribeBusiness.getCountBySubscribe(args.Sender.Group.Id, dbSubscribe.Id) > 0)
                {
                    await session.SendMessageWithAtAsync(args, new PlainMessage($" 已订阅了米游社用户[{userId}]的[{Enum.GetName(typeof(MysSectionType), mysSection.Value)}]频道~"));
                    return;
                }

                if (mysSection.Value == MysSectionType.全部) mysBusiness.delAllSubscribe(args.Sender.Group.Id, userId);
                subscribeBusiness.insertSubscribeGroup(args.Sender.Group.Id, dbSubscribe.Id);

                List<IChatMessage> chailList = new List<IChatMessage>();
                chailList.Add(new PlainMessage($"米游社用户[{dbSubscribe.SubscribeName}]订阅成功!\r\n"));
                chailList.Add(new PlainMessage($"uid：{dbSubscribe.SubscribeCode}\r\n"));
                chailList.Add(new PlainMessage($"频道：{Enum.GetName(typeof(MysSectionType), mysSection)}\r\n"));
                chailList.Add(new PlainMessage($"签名：{dbSubscribe.SubscribeDescription}\r\n"));
                FileInfo fileInfo = string.IsNullOrEmpty(userInfoDto.data.user_info.avatar_url) ? null : await HttpHelper.DownImgAsync(userInfoDto.data.user_info.avatar_url);
                if (fileInfo != null) chailList.Add((IChatMessage)await MiraiHelper.Session.UploadPictureAsync(UploadTarget.Group, fileInfo.FullName));
                await session.SendMessageWithAtAsync(args, chailList);
                ConfigHelper.loadSubscribeTask();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "订阅米游社用户异常");
                throw;
            }
        }

        /// <summary>
        /// 退订米游社用户
        /// </summary>
        /// <param name="e"></param>
        /// <param name="message"></param>
        /// <param name="isGroupSubscribe"></param>
        /// <returns></returns>
        public async Task cancleSubscribeMysUserAsync(IMiraiHttpSession session, IGroupMessageEventArgs args, string message)
        {
            try
            {
                MysSectionType? mysSection = null;
                string userId = null;

                string[] paramArr = message.splitParam(BotConfig.SubscribeConfig.Mihoyo.RmCommand);
                if (paramArr == null || paramArr.Length == 0)
                {
                    StepInfo stepInfo = await StepCache.CreateStepAsync(session, args);
                    if (stepInfo == null) return;
                    StepDetail uidStep = new StepDetail(60, " 请在60秒内发送要退订用户的id", CheckUserIdAsync);
                    StepDetail sectionStep = new StepDetail(60, CancleSectionQuestion, CheckSectionAsync);
                    stepInfo.AddStep(uidStep);
                    stepInfo.AddStep(sectionStep);
                    bool isSuccess = await stepInfo.StartStep(session, args);
                    if (isSuccess == false) return;
                    userId = uidStep.Answer;
                    mysSection = (MysSectionType)Convert.ToInt32(sectionStep.Answer);
                }
                else
                {
                    userId = paramArr.Length > 0 ? paramArr[0] : null;
                    string mysSectionStr = paramArr.Length > 1 ? paramArr[1] : "0";
                    if (await CheckUserIdAsync(session, args, userId) == false) return;
                    if (await CheckSectionAsync(session, args, mysSectionStr) == false) return;
                    mysSection = (MysSectionType)Convert.ToInt32(mysSectionStr);
                }

                List<SubscribePO> subscribeList = mysBusiness.getSubscribeList(args.Sender.Group.Id, userId);
                if (subscribeList == null || subscribeList.Count == 0)
                {
                    await session.SendMessageWithAtAsync(args, new PlainMessage(" 并没有订阅这个用户哦~"));
                    return;
                }

                foreach (var item in subscribeList)
                {
                    if (mysSection.Value == MysSectionType.全部 || (int)mysSection.Value == item.SubscribeSubType)
                    {
                        subscribeBusiness.delSubscribe(args.Sender.Group.Id, item.Id);
                    }
                }

                await session.SendMessageWithAtAsync(args, new PlainMessage(" 退订成功~"));
                ConfigHelper.loadSubscribeTask();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "退订米游社用户异常");
                throw;
            }
        }


        private async Task<string> CancleSectionQuestion(IMiraiHttpSession session, IGroupMessageEventArgs args, StepInfo stepInfo, StepDetail currentStep)
        {
            string userIdstr = stepInfo.StepDetails[0].Answer;
            List<SubscribePO> subscribeList = mysBusiness.getSubscribeList(args.Sender.Group.Id, userIdstr);
            if (subscribeList == null || subscribeList.Count == 0)
            {
                await session.SendMessageWithAtAsync(args, new PlainMessage(" 并没有订阅这个用户哦~"));
                return null;
            }

            StringBuilder questionBuilder = new StringBuilder();
            questionBuilder.AppendLine($"请在{currentStep.WaitSecond}秒内发送数字选择你要退订的频道：");
            questionBuilder.AppendLine($"{(int)MysSectionType.全部}：{Enum.GetName(typeof(MysSectionType), MysSectionType.全部)}");
            foreach (var item in subscribeList)
            {
                if (item.SubscribeSubType == (int)MysSectionType.全部) continue;
                if (Enum.IsDefined(typeof(MysSectionType), item.SubscribeSubType) == false) continue;
                MysSectionType mysSectionType = (MysSectionType)item.SubscribeSubType;
                questionBuilder.AppendLine($"{item.SubscribeSubType}：{Enum.GetName(typeof(MysSectionType), mysSectionType)}");
            }
            return questionBuilder.ToString();
        }


        private async Task<bool> CheckUserIdAsync(IMiraiHttpSession session, IGroupMessageEventArgs args, string value)
        {
            long userId = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                await session.SendMessageWithAtAsync(args, new PlainMessage(" 用户id不可以为空"));
                return false;
            }
            if (long.TryParse(value, out userId) == false)
            {
                await session.SendMessageWithAtAsync(args, new PlainMessage(" 用户id必须为数字"));
                return false;
            }
            return true;
        }


        private async Task<bool> CheckSectionAsync(IMiraiHttpSession session, IGroupMessageEventArgs args, string value)
        {
            int sectionId = 0;
            if (int.TryParse(value, out sectionId) == false)
            {
                await session.SendMessageWithAtAsync(args, new PlainMessage(" 频道必须为数字"));
                return false;
            }
            if (Enum.IsDefined(typeof(MysSectionType), sectionId) == false)
            {
                await session.SendMessageWithAtAsync(args, new PlainMessage(" 频道不在范围内"));
                return false;
            }
            return true;
        }




    }
}

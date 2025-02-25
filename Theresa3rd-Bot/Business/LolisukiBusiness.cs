﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Theresa3rd_Bot.Common;
using Theresa3rd_Bot.Model.Lolisuki;
using Theresa3rd_Bot.Util;

namespace Theresa3rd_Bot.Business
{
    public class LolisukiBusiness : SetuBusiness
    {
        public string getWorkInfo(LolisukiData lolisukiData, FileInfo fileInfo, DateTime startTime, long todayLeft, string template = "")
        {
            if (string.IsNullOrWhiteSpace(template)) return getDefaultWorkInfo(lolisukiData, fileInfo, startTime);
            int costSecond = DateTimeHelper.GetSecondDiff(startTime, DateTime.Now);
            double sizeMB = fileInfo == null ? 0 : MathHelper.getMbWithByte(fileInfo.Length);
            template = template.Replace("{TodayLeft}", todayLeft.ToString());
            template = template.Replace("{MemberCD}", BotConfig.SetuConfig.MemberCD.ToString());
            template = template.Replace("{RevokeInterval}", BotConfig.SetuConfig.RevokeInterval.ToString());
            template = template.Replace("{IllustTitle}", lolisukiData.title);
            template = template.Replace("{PixivId}", lolisukiData.pid.ToString());
            template = template.Replace("{UserName}", lolisukiData.author);
            template = template.Replace("{UserId}", lolisukiData.uid);
            template = template.Replace("{Level}", lolisukiData.level.ToString());
            template = template.Replace("{SizeMB}", sizeMB.ToString());
            template = template.Replace("{CostSecond}", costSecond.ToString());
            template = template.Replace("{Tags}", BusinessHelper.JoinPixivTagsStr(lolisukiData.tags, BotConfig.GeneralConfig.PixivTagShowMaximum));
            template = template.Replace("{Urls}", lolisukiData.urls.original.ToProxyUrl());
            return template;
        }

        public string getDefaultWorkInfo(LolisukiData lolisukiData, FileInfo fileInfo, DateTime startTime)
        {
            StringBuilder workInfoStr = new StringBuilder();
            int costSecond = DateTimeHelper.GetSecondDiff(startTime, DateTime.Now);
            double sizeMB = fileInfo == null ? 0 : MathHelper.getMbWithByte(fileInfo.Length);
            workInfoStr.AppendLine($"标题：{lolisukiData.title}，画师：{lolisukiData.author}，画师id：{lolisukiData.uid}，Level：{lolisukiData.level}，大小：{sizeMB}MB，耗时：{costSecond}s");
            workInfoStr.AppendLine($"标签：{BusinessHelper.JoinPixivTagsStr(lolisukiData.tags, BotConfig.GeneralConfig.PixivTagShowMaximum)}");
            workInfoStr.Append(lolisukiData.urls.original.ToProxyUrl());
            return workInfoStr.ToString();
        }

        public async Task<LolisukiResult> getLolisukiResultAsync(int r18Mode, string level, string[] tags = null)
        {
            string[] postTags = tags == null || tags.Length == 0 ? new string[0] : tags;
            LolisukiParam param = new LolisukiParam(r18Mode, 1, "i.pixiv.re", postTags, level, 0);
            string httpUrl = HttpUrl.getLolisukiApiUrl();
            string postJson = JsonConvert.SerializeObject(param);
            string json = await HttpHelper.PostJsonAsync(httpUrl, postJson);
            return JsonConvert.DeserializeObject<LolisukiResult>(json);
        }

        

        public async Task<FileInfo> downImgAsync(LolisukiData lolisukiData)
        {
            try
            {
                string fullFileName = $"{lolisukiData.pid}.jpg";
                string fullImageSavePath = Path.Combine(FilePath.getDownImgSavePath(), fullFileName);
                string imgUrl = getDownImgUrl(lolisukiData.urls.original);
                if (BotConfig.GeneralConfig.PixivFreeProxy || string.IsNullOrWhiteSpace(BotConfig.GeneralConfig.PixivImgProxy) == false)
                {
                    return await HttpHelper.DownFileAsync(imgUrl.ToProxyUrl(), fullImageSavePath);
                }
                Dictionary<string, string> headerDic = new Dictionary<string, string>();
                headerDic.Add("Referer", HttpUrl.getPixivArtworksReferer(lolisukiData.pid.ToString()));
                headerDic.Add("Cookie", BotConfig.WebsiteConfig.Pixiv.Cookie);
                if (string.IsNullOrWhiteSpace(BotConfig.GeneralConfig.PixivHttpProxy) == false)
                {
                    return await HttpHelper.DownFileWithProxyAsync(imgUrl.ToPximgUrl(), fullImageSavePath, headerDic);
                }
                else
                {
                    return await HttpHelper.DownFileAsync(imgUrl.ToPximgUrl(), fullImageSavePath, headerDic);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "LolisukiBusiness.downImg下载图片失败");
                return null;
            }
        }

    }
}

﻿using System.Collections.Generic;
using Theresa3rd_Bot.Common;

namespace Theresa3rd_Bot.Util
{
    public static class PermissionsHelper
    {
        public static bool IsDownImg(this List<long> groupIds, bool isR18Img)
        {
            foreach (long groupId in groupIds)
            {
                if (groupId.IsShowSetuImg(isR18Img)) return true;
            }
            return false;
        }

        public static bool IsShowSetuImg(this long groupId, bool isR18Img)
        {
            List<long> SetuShowImgGroups = BotConfig.PermissionsConfig?.SetuShowImgGroups;
            if (SetuShowImgGroups == null) return false;
            if (SetuShowImgGroups.Contains(groupId) == false) return false;
            if (isR18Img) return false;
            return true;
        }

        public static bool IsShowSaucenaoImg(this long groupId, bool isR18Img)
        {
            List<long> SetuShowImgGroups = BotConfig.PermissionsConfig?.SetuShowImgGroups;
            if (SetuShowImgGroups == null) return false;
            if (SetuShowImgGroups.Contains(groupId) == false) return false;
            if (isR18Img) return false;
            return true;
        }

        public static bool IsShowR18Setu(this long groupId)
        {
            if (BotConfig.PermissionsConfig?.SetuShowR18Groups == null) return false;
            return BotConfig.PermissionsConfig.SetuShowR18Groups.Contains(groupId);
        }


        public static bool IsShowR18Saucenao(this long groupId)
        {
            if (BotConfig.PermissionsConfig?.SaucenaoR18Groups == null) return false;
            return BotConfig.PermissionsConfig.SaucenaoR18Groups.Contains(groupId);
        }

        public static bool IsSuperManager(this long memberId)
        {
            if (BotConfig.PermissionsConfig?.SuperManagers == null) return false;
            return BotConfig.PermissionsConfig.SuperManagers.Contains(memberId);
        }

    }
}

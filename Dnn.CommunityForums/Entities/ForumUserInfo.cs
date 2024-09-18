﻿// Copyright (c) 2013-2024 by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.



namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Web;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.UserControls;

    [TableName("activeforums_UserProfiles")]
    [PrimaryKey("ProfileId", AutoIncrement = true)]
    [Scope("PortalId")]
    public class ForumUserInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        private DotNetNuke.Entities.Users.UserInfo userInfo;
        private PortalSettings portalSettings;
        private SettingsInfo mainSettings;
        private ModuleInfo moduleInfo;
        private string userRoles = Globals.DefaultAnonRoles + "|-1;||";

        public ForumUserInfo()
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
        }

        public ForumUserInfo(int moduleId)
        {
            this.userInfo = new DotNetNuke.Entities.Users.UserInfo();
            this.ModuleId = moduleId;
        }

        public int ProfileId { get; set; }

        public int UserId { get; set; } = -1;

        public int PortalId { get; set; }

        [IgnoreColumn]
        internal int ModuleId { get; set; }

        public int TopicCount { get; set; }

        public int ReplyCount { get; set; }

        public int ViewCount { get; set; }

        public int AnswerCount { get; set; }

        public int RewardPoints { get; set; }

        public string UserCaption { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateUpdated { get; set; }

        public DateTime? DateLastActivity { get; set; }

        public DateTime? DateLastPost { get; set; }

        public DateTime? DateLastReply { get; set; }

        public string Signature { get; set; }

        public bool SignatureDisabled { get; set; }

        public int TrustLevel { get; set; }

        public bool AdminWatch { get; set; }

        public bool AttachDisabled { get; set; }

        public string Avatar { get; set; }

        public AvatarTypes AvatarType { get; set; }

        public bool AvatarDisabled { get; set; }

        public string PrefDefaultSort { get; set; } = "ASC";

        public bool PrefDefaultShowReplies { get; set; }

        public bool PrefJumpLastPost { get; set; }

        public bool PrefTopicSubscribe { get; set; }

        public SubscriptionTypes PrefSubscriptionType { get; set; }

        public bool PrefBlockAvatars { get; set; }

        public bool PrefBlockSignatures { get; set; }

        public int PrefPageSize { get; set; } = 20;

        [IgnoreColumn]
        public string[] Roles => this.UserInfo?.Roles;

        [IgnoreColumn] public string FirstName => this.UserInfo?.FirstName;

        [IgnoreColumn]
        public string LastName => string.IsNullOrEmpty(this.UserInfo?.LastName) ? string.Empty : this.UserInfo?.LastName;

        [IgnoreColumn]
        public string FullName => string.Concat(this.UserInfo?.FirstName, " ", this.UserInfo?.LastName);

        [IgnoreColumn]
        public string DisplayName => string.IsNullOrEmpty(this.UserInfo?.DisplayName) == null ? string.Empty : this.UserInfo?.DisplayName;

        [IgnoreColumn]
        public string Username => this.UserInfo?.Username;

        [IgnoreColumn]
        public string Email => string.IsNullOrEmpty(this.UserInfo?.Email) ? string.Empty : this.UserInfo?.Email;

        [IgnoreColumn]
        public bool GetIsMod(int ModuleId)
        {
            return (!(string.IsNullOrEmpty(DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.UserRoles, this.PortalId, ModuleId, "CanApprove"))));
        }

        [IgnoreColumn]
        public bool IsSuperUser => this.UserInfo != null && this.UserInfo.IsSuperUser;

        [IgnoreColumn]
        public bool IsAdmin => this.UserInfo != null && this.UserInfo.IsAdmin;

        [IgnoreColumn]
        public bool IsAnonymous => this.UserId == -1;

        [IgnoreColumn]
        public bool IsUserOnline => this.DateLastActivity > DateTime.UtcNow.AddMinutes(-5);

        [IgnoreColumn]
        public CurrentUserTypes CurrentUserType { get; set; }

        [IgnoreColumn]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used")]
        public string ForumsAllowed { get; set; }

        [IgnoreColumn]
        public string UserForums { get; set; }

        [IgnoreColumn]
        public int PostCount => this.TopicCount + this.ReplyCount;

        [IgnoreColumn]
        public DotNetNuke.Entities.Profile.ProfilePropertyDefinitionCollection Properties => this.UserInfo?.Profile?.ProfileProperties;

        [IgnoreColumn]
        TimeSpan TimeZoneOffsetForUser => Utilities.GetTimeZoneOffsetForUser(this.UserInfo);

        [IgnoreColumn]
        public PortalSettings PortalSettings
        {
            get => this.portalSettings ?? (this.portalSettings = Utilities.GetPortalSettings(this.PortalId));
            set => this.portalSettings = value;
        }

        [IgnoreColumn]
        public SettingsInfo MainSettings
        {
            get => this.mainSettings ?? (this.mainSettings = SettingsBase.GetModuleSettings(this.ModuleId));
            set => this.mainSettings = value;
        }

        [IgnoreColumn]
        public ModuleInfo ModuleInfo
        {
            get => this.moduleInfo ?? (this.moduleInfo = this.LoadModuleInfo());
            set => this.moduleInfo = value;
        }

        internal ModuleInfo LoadModuleInfo()
        {
            return DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.ModuleId, DotNetNuke.Common.Utilities.Null.NullInteger, false);
        }

        [IgnoreColumn]
        public DotNetNuke.Entities.Users.UserInfo UserInfo
        {
            get => this.userInfo ?? (this.userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, this.UserId));
            set => this.userInfo = value;
        }

        [IgnoreColumn]
        public string UserRoles
        {
            get
            {
                PortalSettings _portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(this.PortalId);
                var ids = this.GetRoleIds(this.UserInfo, this.PortalId);
                if (string.IsNullOrEmpty(ids))
                {
                    ids = Globals.DefaultAnonRoles + "|-1;||";
                }

                if (this.IsSuperUser)
                {
                    ids += Globals.DefaultAnonRoles + _portalSettings.AdministratorRoleId + ";";
                }

                ids += "|" + this.UserId + "|" + string.Empty + "|";
                this.userRoles = ids;

                return this.userRoles;
            }

            set
            {
                this.userRoles = value;
            }
        }

        private string GetRoleIds(UserInfo u, int PortalId)
        {
            string RoleIds = string.Empty;
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: PortalId))
            {
                string roleName = r.RoleName;
                foreach (string role in u?.Roles)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        if (roleName == role)
                        {
                            RoleIds += r.RoleID.ToString() + ";";
                            break;
                        }
                    }
                }
            }

            foreach (DotNetNuke.Security.Roles.RoleInfo r in u?.Social?.Roles)
            {
                RoleIds += r.RoleID.ToString() + ";";
            }

            return RoleIds;
        }

        internal int GetLastReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, ti.ForumId);
            if (forumTrak?.MaxReplyRead > topicTrak?.LastReplyId || topicTrak == null)
            {
                if (forumTrak != null)
                {
                    return forumTrak.MaxReplyRead;
                }
            }
            else
            {
                return topicTrak.LastReplyId;
            }

            return 0;

            // from stored procedure
            // CASE WHEN FT.MaxReplyRead > TT.LastReplyId OR TT.LastReplyID IS NULL THEN ISNULL(FT.MaxReplyRead,0) ELSE TT.LastReplyId END AS UserLastReplyRead, 
        }

        internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, ti.ForumId);
            if (forumTrak?.MaxTopicRead > topicTrak?.TopicId || topicTrak == null)
            {
                if (forumTrak != null)
                {
                    return forumTrak.MaxTopicRead;
                }
            }
            else
            {
                return topicTrak.TopicId;
            }

            return 0;

            // from stored procedure
            // CASE WHEN FT.MaxTopicRead > TT.TopicId OR TT.TopicId IS NULL THEN ISNULL(FT.MaxTopicRead,0) ELSE TT.TopicId END AS UserLastTopicRead,
        }

        internal bool GetIsTopicRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            if (topicTrak?.LastReplyId >= ti.LastReplyId)
            {
                return true;
            }

            return false;
        }

        internal bool GetIsReplyRead(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo ri)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ri.TopicId);
            if (topicTrak?.LastReplyId >= ri.ReplyId)
            {
                return true;
            }

            return false;
        }

        internal int GetLastTopicRead(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            var forumTrak = new DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController().GetByUserIdForumId(this.UserId, fi.ForumID);
            if (forumTrak != null)
            {
                return forumTrak.MaxTopicRead;
            }

            return 0;
        }

        internal int GetTopicReadCount(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetTopicsReadCountForUserForum(this.UserId, fi.ForumID);
        }

        internal int GetLastTopicReplyRead(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti)
        {
            var topicTrak = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController().GetByUserIdTopicId(this.UserId, ti.TopicId);
            if (topicTrak != null)
            {
                return topicTrak.LastReplyId;
            }

            return 0;
        }

        [IgnoreColumn]
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability
        {
            get
            {
                return DotNetNuke.Services.Tokens.CacheLevel.notCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {

            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.PortalSettings, this)
                {
                    AccessingUser = accessingUser,
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "userid":
                    return PropertyAccess.FormatString(this.UserId.ToString(), format);
                case "username":
                    return PropertyAccess.FormatString(HttpUtility.HtmlEncode(this.Username).Replace("&amp;#", "&#"), format);
                case "avatar":
                    return PropertyAccess.FormatString(
                        !this.PrefBlockAvatars && !this.AvatarDisabled ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(
                            this.UserId, this.MainSettings.AvatarWidth, this.MainSettings.AvatarHeight) : string.Empty, format);
                case "usercaption":
                    return PropertyAccess.FormatString(this.UserCaption, format);
                case "displayname":
                    return PropertyAccess.FormatString(
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.MainSettings,
                        isMod: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(
                            portalId: accessingUser.PortalID,
                            userId: accessingUser.UserID).GetIsMod(this.ModuleId),
                        isAdmin: new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(
                            portalId: accessingUser.PortalID,
                            userId: accessingUser.UserID).IsAdmin,
                        this.UserId, this.Username, this.FirstName, this.LastName, this.DisplayName), format);
                case "email":
                    return PropertyAccess.FormatString(this.Email, format);
                case "fullname":
                    return PropertyAccess.FormatString(this.FullName, format);
                case "firstname":
                    return PropertyAccess.FormatString(HttpUtility.HtmlEncode(this.FirstName).Replace("&amp;#", "&#"), format);
                case "lastname":
                    return PropertyAccess.FormatString(HttpUtility.HtmlEncode(this.LastName).Replace("&amp;#", "&#"), format);
                case "datecreated":
                    return Utilities.GetUserFormattedDateTime((DateTime?)this.DateCreated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "dateupdated":
                    return Utilities.GetUserFormattedDateTime(this.DateUpdated, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "datelastpost":
                    return Utilities.GetUserFormattedDateTime(this.DateLastPost, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "datelastreply":
                    return Utilities.GetUserFormattedDateTime(this.DateLastReply, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "datelastactivity":
                    return Utilities.GetUserFormattedDateTime(this.DateLastActivity, formatProvider, accessingUser.Profile.PreferredTimeZone.GetUtcOffset(DateTime.UtcNow));
                case "postcount":
                    return PropertyAccess.FormatString(this.PostCount.ToString(), format);
                case "replycount":
                    return PropertyAccess.FormatString(this.ReplyCount.ToString(), format);
                case "viewcount":
                    return PropertyAccess.FormatString(this.ViewCount.ToString(), format);
                case "topiccount":
                    return PropertyAccess.FormatString(this.TopicCount.ToString(), format);
                case "answercount":
                    return PropertyAccess.FormatString(this.MainSettings.EnablePoints && this.UserId > 0 ? this.AnswerCount.ToString() : string.Empty, format);
                case "rewardpoints":
                    return PropertyAccess.FormatString(this.MainSettings.EnablePoints && this.UserId > 0 ? this.RewardPoints.ToString() : string.Empty, format);
                case "totalpoints":
                    return PropertyAccess.FormatString(
                        this.MainSettings.EnablePoints && this.UserId > 0 ?
                       ((this.TopicCount * this.MainSettings.TopicPointValue) +
                        (this.ReplyCount * this.MainSettings.ReplyPointValue) +
                        (this.AnswerCount * this.MainSettings.AnswerPointValue) + this.RewardPoints).ToString() : string.Empty, format);
                case "rankdisplay":
                    return PropertyAccess.FormatString(
                        this.UserId > 0
                            ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(
                                this.ModuleId,
                                this,
                                0)
                            : string.Empty,
                        format);
                case "rankname":
                    return PropertyAccess.FormatString(
                        this.UserId > 0
                            ? DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserRank(
                                this.ModuleId,
                                this,
                                1)
                            : string.Empty,
                        format);
                case "userprofilelink":
                    return PropertyAccess.FormatString(
                        DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.CanLinkToProfile(
                            this.PortalSettings,
                            this.MainSettings,
                            this.ModuleId,
                            new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(
                                accessingUser.PortalID,
                                accessingUser.UserID),
                            this)
                            ? Utilities.NavigateURL(
                                this.PortalSettings.UserTabId,
                                string.Empty,
                                new[] { $"userId={this.UserId}" })
                            : string.Empty,
                        format);
                case "signature":
                    var sSignature = string.Empty;
                    if (this.MainSettings.AllowSignatures != 0 && !this.PrefBlockSignatures && !this.SignatureDisabled)
                    {
                        sSignature = this?.Signature ?? string.Empty;
                        if (!string.IsNullOrEmpty(sSignature))
                        {
                            sSignature = Utilities.ManageImagePath(sSignature);

                            switch (this.MainSettings.AllowSignatures)
                            {
                                case 1:
                                    sSignature = System.Web.HttpUtility.HtmlEncode(sSignature);
                                    sSignature = sSignature.Replace(System.Environment.NewLine, "<br />");
                                    break;
                                case 2:
                                    sSignature = System.Web.HttpUtility.HtmlDecode(sSignature);
                                    break;
                            }
                        }
                    }

                    return PropertyAccess.FormatString(sSignature, format);
                case "userstatus":
                    return PropertyAccess.FormatString(this.MainSettings.UsersOnlineEnabled && this.UserId > 0 ?
                        DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINE]" :
                            "[FORUMUSER-USEROFFLINE]", this.PortalSettings, accessingUser.Profile.PreferredLocale) : string.Empty, format);
                case "userstatuscss":
                    return PropertyAccess.FormatString(this.MainSettings.UsersOnlineEnabled && this.UserId > 0 ?
                            DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.GetTokenFormatString(this.IsUserOnline ? "[FORUMUSER-USERONLINECSS]" : "[FORUMUSER-USEROFFLINECSS]", this.PortalSettings, accessingUser.Profile.PreferredLocale) :
                            string.Empty, format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}

//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
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
//

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{

    public class ForumController
    {
        private ForumsDB _forumDB;
        internal ForumsDB ForumsDB
        {
            get { return _forumDB ?? (_forumDB = new ForumsDB()); }
        }

        public string GetForumsForUser(string userRoles, int portalId, int moduleId, string permissionType = "CanView", bool strict = false)
        {
            // Setting strict to true enforces the actual permission
            // If strict is false, forums will show up in the list if they are not hidden for users
            // that don't otherwise have access

            var forumIds = string.Empty;
            var fc = ForumsDB.Forums_List(portalId, moduleId);
            foreach (Forum f in fc)
            {
                string roles;
                switch (permissionType)
                {
                    case "CanView":
                        roles = f.Security.View;
                        break;
                    case "CanRead":
                        roles = f.Security.Read;
                        break;
                    case "CanApprove":
                        roles = f.Security.ModApprove;
                        break;
                    case "CanEdit":
                        roles = f.Security.ModEdit;
                        break;
                    default:
                        roles = f.Security.View;
                        break;
                }

                var hasPermissions = Permissions.HasPerm(roles, userRoles);

                if ((hasPermissions || (!strict && !f.Hidden && (permissionType == "CanView" || permissionType == "CanRead"))) && f.Active)
                {
                    forumIds += string.Concat(f.ForumID, ";");
                }
            }

            return forumIds;
        }

        public Forum GetForum(int portalId, int moduleId, int forumId)
        {
            return GetForum(portalId, moduleId, forumId, false);
        }

        public Forum GetForum(int portalId, int moduleId, int forumId, bool ignoreCache)
        {

            var cachekey = string.Format(CacheKeys.ForumInfo, moduleId, forumId);
            var forum = DataCache.SettingsCacheRetrieve(moduleId, cachekey) as Forum;
            if (forum == null || ignoreCache)
            {
                using (var dr = ForumsDB.Forums_Get(portalId, moduleId, forumId))
                {
                    while (dr.Read())
                    {
                        forum = FillForum(dr);
                    }
                    dr.Close();
                }
                if (forum != null)
                {
                    if (forum.HasProperties)
                    {
                        var propC = new PropertiesController();
                        forum.Properties = propC.ListProperties(portalId, 1, forumId);
                    }

                }
                forum.ForumSettings = DataCache.GetSettings(moduleId, forum.ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, moduleId, forum.ForumSettingsKey), !ignoreCache);
                DataCache.SettingsCacheStore(moduleId, cachekey, forum);
            }
            return forum;
        }

        private static Forum FillForum(IDataRecord dr)
        {
            var fi = new Forum
            {
                ForumGroup = new ForumGroupInfo(),
                ForumID = Convert.ToInt32(dr["ForumId"].ToString()),
                Active = Convert.ToBoolean(dr["Active"]),
                ModuleId = Convert.ToInt32(dr["ModuleId"].ToString()),
                ForumGroupId = Convert.ToInt32(dr["ForumGroupId"].ToString()),
                ParentForumId = Convert.ToInt32(dr["ParentForumId"].ToString()),
                ForumName = dr["ForumName"].ToString(),
                ForumDesc = dr["ForumDesc"].ToString(),
                SortOrder = Convert.ToInt32(dr["SortOrder"].ToString()),
                Hidden = Convert.ToBoolean(dr["Hidden"]),
                TotalTopics = Convert.ToInt32(dr["TotalTopics"].ToString()),
                TotalReplies = Convert.ToInt32(dr["TotalReplies"].ToString()),
                LastTopicId = Convert.ToInt32(dr["LastTopicId"].ToString()),
                LastReplyId = Convert.ToInt32(dr["LastReplyId"].ToString()),
                GroupName = dr["GroupName"].ToString(),
                PermissionsId = Convert.ToInt32(dr["PermissionsId"].ToString()),
                ForumSettingsKey = dr["ForumSettingsKey"].ToString(),
                InheritSecurity = Convert.ToBoolean(dr["InheritSecurity"]),
                PrefixURL = dr["PrefixURL"].ToString(),
                SocialGroupId = Convert.ToInt32(dr["SocialGroupId"].ToString()),
                HasProperties = Convert.ToBoolean(dr["HasProperties"])
            };

            fi.ForumGroup.ForumGroupId = fi.ForumGroupId;
            fi.ForumGroup.GroupName = fi.GroupName;
            fi.ForumGroup.PrefixURL = dr["GroupPrefixURL"].ToString();
            fi.Security.Announce = dr["CanAnnounce"].ToString();
            fi.Security.Attach = dr["CanAttach"].ToString();
            fi.Security.Create = dr["CanCreate"].ToString();
            fi.Security.Delete = dr["CanDelete"].ToString();
            fi.Security.Edit = dr["CanEdit"].ToString();
            fi.Security.Lock = dr["CanLock"].ToString();
            fi.Security.ModApprove = dr["CanModApprove"].ToString();
            fi.Security.ModDelete = dr["CanModDelete"].ToString();
            fi.Security.ModEdit = dr["CanModEdit"].ToString();
            fi.Security.ModLock = dr["CanModLock"].ToString();
            fi.Security.ModMove = dr["CanModMove"].ToString();
            fi.Security.ModPin = dr["CanModPin"].ToString();
            fi.Security.ModSplit = dr["CanModSplit"].ToString();
            fi.Security.ModUser = dr["CanModUser"].ToString();
            fi.Security.Pin = dr["CanPin"].ToString();
            fi.Security.Poll = dr["CanPoll"].ToString();
            fi.Security.Block = dr["CanBlock"].ToString();
            fi.Security.Read = dr["CanRead"].ToString();
            fi.Security.Reply = dr["CanReply"].ToString();
            fi.Security.Subscribe = dr["CanSubscribe"].ToString();
            fi.Security.Trust = dr["CanTrust"].ToString();
            fi.Security.View = dr["CanView"].ToString();
            fi.Security.Tag = dr["CanTag"].ToString();
            fi.Security.Prioritize = dr["CanPrioritize"].ToString();
            fi.Security.Categorize = dr["CanCategorize"].ToString();

            return fi;
        }

        public string GetForumIdsBySocialGroup(int portalId, int socialGroupId)
        {
            var forumIds = string.Empty;
            using (var dr = ForumsDB.Forums_GetForSocialGroup(portalId, socialGroupId))
            {
                while (dr.Read())
                {
                    forumIds += string.Concat(dr["ForumId"], ";");
                }
                dr.Close();
            }
            return forumIds;
        }
        internal Forum Forums_Get(int portalId, int moduleId, int forumId, bool useCache)
        {
            return Forums_Get(portalId, moduleId, forumId, useCache, -1);
        }
        internal Forum Forums_Get(int portalId, int moduleId, int forumId, bool useCache, int topicId)
        {
            if (forumId <= 0 && topicId <= 0)
            {
                return null;
            }

            // Get the forum by topic id
            if (topicId > 0 & forumId <= 0)
            {
                forumId = ForumsDB.Forum_GetByTopicId(topicId);
            }

            return forumId <= 0 ? null : GetForum(portalId, moduleId, forumId, !useCache);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use Forums_Save(int portalId, Forum fi, bool isNew, bool useGroupFeatures, bool useGroupSecurity)")]
        public int Forums_Save(int portalId, Forum fi, bool isNew, bool useGroup)
        {
            return Forums_Save(portalId, fi,  isNew, useGroup, useGroup);
        }
        public int Forums_Save(int portalId, Forum fi, bool isNew, bool useGroupFeatures, bool useGroupSecurity)
        {
            var permissionsId = -1;
            var fc = new ForumGroupController();
            var fg = fc.Groups_Get(fi.ModuleId, fi.ForumGroupId);
            if (useGroupSecurity)
            {
                if (fg != null)
                {
                    fi.PermissionsId = fg.PermissionsId;
                }
            }
            else 
            {
                if (fi.PermissionsId <= 0 || fi.InheritSecurity) /* if new forum or changing from inheriting security, create new permissions set */
                {
                    var rc = new RoleController();
                    var ri = rc.GetRoleByName(portalId, "Administrators");
                    if (ri != null)
                    {
                        var db = new Data.Common();
                        fi.PermissionsId = db.CreatePermSet(ri.RoleID.ToString());
                        permissionsId = fi.PermissionsId;
                        Permissions.CreateDefaultSets(portalId, permissionsId);
                    }
                }
                else
                {

                }
            }
            fi.ForumSettingsKey = useGroupFeatures ? (fg != null ? fg.GroupSettingsKey : string.Empty) : (fi.ForumID > 0 ? $"F:{fi.ForumID}" : string.Empty);

            if (fi.ForumID <= 0)
            {
                isNew = true;
            }
            var forumId = Convert.ToInt32(DataProvider.Instance().Forum_Save(portalId, fi.ForumID, fi.ModuleId, fi.ForumGroupId, fi.ParentForumId, fi.ForumName, fi.ForumDesc, fi.SortOrder, fi.Active, fi.Hidden, fi.ForumSettingsKey, fi.PermissionsId, fi.PrefixURL, fi.SocialGroupId, fi.HasProperties));
            if (!useGroupFeatures && String.IsNullOrEmpty(fi.ForumSettingsKey))
            {
                fi.ForumSettingsKey = $"F:{forumId}";
            }
            if (fi.ForumSettingsKey.StartsWith("G:"))
            {
                DataProvider.Instance().Forum_ConfigCleanUp(fi.ModuleId, $"F:{fi.ForumID}");
            }

            if (isNew && useGroupFeatures == false)
            {
                var sKey = $"F:{fi.ForumID}";
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicsTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicTemplateId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.TopicFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.ReplyFormId, "0");
                Settings.SaveSetting(fi.ModuleId, sKey, ForumSettingKeys.AllowRSS, "false");
            }

            // Clear the caches
            DataCache.SettingsCacheClear(fi.ModuleId, string.Format(CacheKeys.ForumList, fi.ModuleId));
            DataCache.SettingsCacheClear(fi.ModuleId, string.Format(CacheKeys.ForumInfo, fi.ModuleId, forumId));

            return forumId;
        }

        public string GetForumsHtmlOption(int portalId, int moduleId, User currentUser)
        {
            var userForums = GetForumsForUser(currentUser.UserRoles, portalId, moduleId, "CanView");
            var dt = DataProvider.Instance().UI_ForumView(portalId, moduleId, currentUser.UserId, currentUser.IsSuperUser, userForums).Tables[0];
            var i = 0;
            var n = 1;
            var tmpGroupCount = 0;
            var tmpForumCount = 0;
            var tmpGroupKey = string.Empty;
            var tmpForumKey = string.Empty;
            var sb = new StringBuilder();
            foreach (DataRow dr in dt.Rows)
            {
                var bView = Permissions.HasPerm(dr["CanView"].ToString(), currentUser.UserRoles);
                var groupName = Convert.ToString(dr["GroupName"]);
                var groupId = Convert.ToInt32(dr["ForumGroupId"]);
                var groupKey = groupName + groupId.ToString();
                var forumName = Convert.ToString(dr["ForumName"]);
                var forumId = Convert.ToInt32(dr["ForumId"]);
                var forumKey = forumName + forumId.ToString();
                var parentForumId = Convert.ToInt32(dr["ParentForumId"]);

                //TODO - Need to add support for Group Permissions and GroupHidden

                if (tmpGroupKey != groupKey)
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", groupName); n += 1;
                    tmpGroupKey = groupKey;
                }

                if (bView)
                {
                    if (parentForumId == 0)
                    {
                        sb.AppendFormat("<option value=\"{0}\">{1}</option>", dr["ForumID"], "--" + dr["ForumName"]);
                        n += 1;
                        sb.Append(GetSubForums(n, Convert.ToInt32(dr["ForumId"]), dt, ref n));
                    }

                }
            }

            return sb.ToString();
        }
        private static string GetSubForums(int itemCount, int parentForumId, DataTable dtForums, ref int n)
        {
            var sb = new StringBuilder();
            dtForums.DefaultView.RowFilter = string.Concat("ParentForumId = ", parentForumId);
            if (dtForums.DefaultView.Count > 0)
            {
                foreach (DataRow dr in dtForums.DefaultView.ToTable().Rows)
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", dr["ForumID"], dr["ForumName"]);
                    itemCount += 1;
                }
            }
            n = itemCount;
            return sb.ToString();
        }

        public DataTable GetForumView(int portalId, int moduleId, int currentUserId, bool isSuperUser, string forumIds)
        {
            DataSet ds;
            DataTable dt;
            var cachekey = string.Format(CacheKeys.ForumViewForUser, moduleId, currentUserId, forumIds);

            var dataSetXML = DataCache.ContentCacheRetrieve(moduleId, cachekey) as string;

            // cached datatable is held as an XML string (because data vanishes if just caching the DT in this instance)
            if (dataSetXML != null)
            {
                var sr = new StringReader(dataSetXML);
                ds = new DataSet();
                ds.ReadXml(sr);
                dt = ds.Tables[0];
            }
            else
            {
                ds = DataProvider.Instance().UI_ForumView(portalId, moduleId, currentUserId, isSuperUser, forumIds);
                dt = ds.Tables[0];

                var sw = new StringWriter();

                dt.WriteXml(sw);
                var result = sw.ToString();

                sw.Close();
                sw.Dispose();

                DataCache.ContentCacheStore(moduleId, cachekey, result);
            }

            return dt;
        }
        public int CreateGroupForum(int portalId, int moduleId, int socialGroupId, int forumGroupId, string forumName, string forumDescription, bool isPrivate, string forumConfig)
        {
            var forumId = -1;

            try
            {
                var forumsDb = new Data.Common();
                var fgc = new ForumGroupController();
                var gi = fgc.Groups_Get(moduleId, forumGroupId);
                var socialGroup = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId: portalId, roleId: socialGroupId);
                var groupAdmin = string.Concat(socialGroupId.ToString(), ":0");
                var groupMember = socialGroupId.ToString();

                var ri = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleByName(portalId: portalId, roleName: "Administrators");
                var permissionsId = forumsDb.CreatePermSet(ri.RoleID.ToString());

                moduleId = gi.ModuleId;

                var fi = new Forum
                {
                    ForumDesc = forumDescription,
                    Active = true,
                    ForumGroupId = forumGroupId,
                    ForumID = -1,
                    ForumName = forumName,
                    Hidden = isPrivate,
                    ModuleId = gi.ModuleId,
                    ParentForumId = 0,
                    PortalId = portalId,
                    PermissionsId = gi.PermissionsId,
                    SortOrder = 0,
                    SocialGroupId = socialGroupId
                };

                forumId = Forums_Save(portalId, fi, true, true, true);
                fi = GetForum(portalId, moduleId, forumId);
                fi.PermissionsId = permissionsId;
                Forums_Save(portalId, fi, false, false, false);

                var xDoc = new XmlDocument();
                xDoc.LoadXml(forumConfig);

                var xRoot = xDoc.DocumentElement;
                if (xRoot != null)
                {
                    var xSecList = xRoot.SelectSingleNode("//security[@type='groupadmin']");
                    string permSet;
                    string secKey;
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            secKey = n.Name;
                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                continue;
                            permSet = forumsDb.GetPermSet(permissionsId, secKey);
                            permSet = Permissions.AddPermToSet(groupAdmin, 2, permSet);
                            forumsDb.SavePermSet(permissionsId, secKey, permSet);
                        }
                    }

                    xSecList = xRoot.SelectSingleNode("//security[@type='groupmember']");
                    if (xSecList != null)
                    {
                        foreach (XmlNode n in xSecList.ChildNodes)
                        {
                            secKey = n.Name;

                            if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                continue;

                            permSet = forumsDb.GetPermSet(permissionsId, secKey);
                            permSet = Permissions.AddPermToSet(groupMember, 0, permSet);
                            forumsDb.SavePermSet(permissionsId, secKey, permSet);
                        }
                    }

                    if (socialGroup.IsPublic)
                    {
                        xSecList = xRoot.SelectSingleNode("//security[@type='registereduser']");
                        ri = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleByName(portalId: portalId, roleName: "Registered Users");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                secKey = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                    continue;

                                permSet = forumsDb.GetPermSet(permissionsId, secKey);
                                permSet = Permissions.AddPermToSet(ri.RoleID.ToString(), 0, permSet);
                                forumsDb.SavePermSet(permissionsId, secKey, permSet);
                            }
                        }

                        xSecList = xRoot.SelectSingleNode("//security[@type='anon']");
                        if (xSecList != null)
                        {
                            foreach (XmlNode n in xSecList.ChildNodes)
                            {
                                secKey = n.Name;

                                if (n.Attributes == null || n.Attributes["value"].Value != "true")
                                    continue;

                                permSet = forumsDb.GetPermSet(permissionsId, secKey);
                                permSet = Permissions.AddPermToSet(DotNetNuke.Common.Globals.glbRoleAllUsers, 0, permSet); //
                                forumsDb.SavePermSet(permissionsId, secKey, permSet);
                            }
                        }
                    }
                }
            }
            catch
            {
                // do nothing?? 
            }

            DataCache.SettingsCacheClear(moduleId, string.Format(CacheKeys.ForumListXml, moduleId));

            return forumId;
        }
    }
}
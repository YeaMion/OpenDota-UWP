﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
//using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;
using System.Net;
using Windows.Web.Http;
using Dota2灵魂小助手.Helpers;

namespace Dota2灵魂小助手
{
    public class DotaMatchHelper
    {
        private const string Key = "****";

        public static Dictionary<string, string> PlayersNameCache = new Dictionary<string, string>();
        public static Dictionary<string, string> PlayersPhotoCache = new Dictionary<string, string>();

        //用于保存用户的Steam64位ID，以"账号绑定"的形式
        public static ApplicationDataContainer SteamID = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// 绑定保存用户的SteamID
        /// </summary>
        /// <param name="input"></param>
        public static void SetSteamID(string input)
        {
            //我的Steam64位ID:76561198194624815
            if (input.Length > 14)
            {
                //这说明输入的是64位的,要先转换成32位
                input = ConvertSteamID(Convert.ToDecimal(input));
            }
            SteamID.Values["SteamID"] = input;
            //SharedData.Share.PlayerID = input;
        }

        /// <summary>
        /// 读取保存的用户的SteamID
        /// </summary>
        /// <returns></returns>
        public static string GetSteamID()
        {
            if (SteamID.Values["SteamID"] == null)
            {
                return "";
            }
            return SteamID.Values["SteamID"].ToString();
        }

        /// <summary>
        /// 根据Steam64位ID获得32位ID，也可以用户直接输入Dota2客户端内的ID，记得处理一下
        /// </summary>
        /// <param name="id_64"></param>
        /// <returns></returns>
        public static string ConvertSteamID(decimal id_64)
        {
            return (id_64 - 76561197960265728).ToString();
        }

        /// <summary>
        /// 获得用户的个人信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<PlayerProfile> GetPlayerProfileAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}", id);
            //http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}
            HttpClient http = new HttpClient();
            PlayerProfile player;
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    return null;
                }

                player = JsonConvert.DeserializeObject<PlayerProfile>(jsonMessage);
            }
            catch
            {
                return null;
            }
            return player;
        }

        /// <summary>
        /// 获得用户的胜局败局数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<WinNLose> GetPlayerWLAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}/wl", id);

            HttpClient http = new HttpClient();
            WinNLose wl;
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    return new WinNLose() { win = 0, lose = 0 };
                }

                wl = JsonConvert.DeserializeObject<WinNLose>(jsonMessage);
            }
            catch
            {
                wl = new WinNLose() { win = 0, lose = 0 };
            }
            return wl;
        }

        /// <summary>
        /// 获取比赛总数据统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<string[]> GetTotalAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}/totals", id);

            HttpClient http = new HttpClient();

            string[] total = new string[13];
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    return null;
                }

                Match totalMatch = Regex.Match(jsonMessage, "n\\\":([\\d]*?),\\\"sum");
                Match killsMatch = Regex.Match(jsonMessage, "kills\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match deadMatch = Regex.Match(jsonMessage, "deaths\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match assistsMatch = Regex.Match(jsonMessage, "assists\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match gpmMatch = Regex.Match(jsonMessage, "gold_per_min\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match xpmMatch = Regex.Match(jsonMessage, "xp_per_min\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match lasthitMatch = Regex.Match(jsonMessage, "last_hits\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match deniesMatch = Regex.Match(jsonMessage, "denies\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match levelMatch = Regex.Match(jsonMessage, "level\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match herodamageMatch = Regex.Match(jsonMessage, "hero_damage\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match towerdamageMatch = Regex.Match(jsonMessage, "tower_damage\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match healingMatch = Regex.Match(jsonMessage, "hero_healing\\\",\\\"n\\\":[\\d]*?,\\\"sum\\\":([\\d]*?)}");
                Match apmMatch = Regex.Match(jsonMessage, "actions_per_min\\\",\\\"n\\\":([\\d]*?),\\\"sum\\\":([\\d]*?)}");

                //Total
                double totalCount = Convert.ToDouble(totalMatch.Groups[1].Value);

                //KillAvr
                total[0] = (Convert.ToDouble(killsMatch.Groups[1].Value) / totalCount).ToString("f1");
                //DeadAvr
                total[1] = (Convert.ToDouble(deadMatch.Groups[1].Value) / totalCount).ToString("f1");
                //AssistAvr
                total[2] = (Convert.ToDouble(assistsMatch.Groups[1].Value) / totalCount).ToString("f1");
                //KDA
                total[3] = ((Convert.ToDouble(killsMatch.Groups[1].Value) + Convert.ToDouble(assistsMatch.Groups[1].Value)) / (Convert.ToDouble(deadMatch.Groups[1].Value))).ToString("f2");

                //GPM
                total[4] = (Convert.ToDouble(gpmMatch.Groups[1].Value) / totalCount).ToString("f1");
                //XPM
                total[5] = (Convert.ToDouble(xpmMatch.Groups[1].Value) / totalCount).ToString("f1");
                //lasthit
                total[6] = (Convert.ToDouble(lasthitMatch.Groups[1].Value) / totalCount).ToString("f1");
                //denies
                total[7] = (Convert.ToDouble(deniesMatch.Groups[1].Value) / totalCount).ToString("f1");

                //LevelAvr
                total[8] = (Convert.ToDouble(levelMatch.Groups[1].Value) / totalCount).ToString("f1");
                //HeroDamageAvr
                total[9] = (Convert.ToDouble(herodamageMatch.Groups[1].Value) / totalCount).ToString("f1");
                //TowerDamageAvr
                total[10] = (Convert.ToDouble(towerdamageMatch.Groups[1].Value) / totalCount).ToString("f1");
                //HeroHealingAvr
                total[11] = (Convert.ToDouble(healingMatch.Groups[1].Value) / totalCount).ToString("f1");

                //APM
                total[12] = (Convert.ToDouble(apmMatch.Groups[2].Value) / Convert.ToDouble(apmMatch.Groups[1].Value)).ToString("f1");
            }
            catch
            {
                return null;
            }
            return total;
        }

        /// <summary>
        /// 获取最近20场比赛
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<RecentMatch>> GetRecentMatchAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}/recentMatches", id);
            HttpClient http = new HttpClient();

            //RecentMatch[] recentMatches = new RecentMatch[20];
            List<RecentMatch> recentMatches = new List<RecentMatch>();
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    //ShowDialog("查询数据太频繁啦，让服务器休息一下吧~");
                    return null;
                }

                MatchCollection match_idCollection = Regex.Matches(jsonMessage, "\\\"match_id\\\":([\\s\\S]*?),");
                MatchCollection player_slotCollection = Regex.Matches(jsonMessage, "\\\"player_slot\\\":([\\s\\S]*?),");
                MatchCollection radiant_winCollection = Regex.Matches(jsonMessage, "\\\"radiant_win\\\":([\\s\\S]*?),");
                MatchCollection durationCollection = Regex.Matches(jsonMessage, "\\\"duration\\\":([\\s\\S]*?),");
                MatchCollection game_modeCollection = Regex.Matches(jsonMessage, "\\\"game_mode\\\":([\\s\\S]*?),");
                MatchCollection lobby_typeCollection = Regex.Matches(jsonMessage, "\\\"lobby_type\\\":([\\s\\S]*?),");
                MatchCollection hero_idCollection = Regex.Matches(jsonMessage, "\\\"hero_id\\\":([\\s\\S]*?),");
                MatchCollection start_timeCollection = Regex.Matches(jsonMessage, "\\\"start_time\\\":([\\s\\S]*?),");
                MatchCollection killsCollection = Regex.Matches(jsonMessage, "\\\"kills\\\":([\\s\\S]*?),");
                MatchCollection deathsCollection = Regex.Matches(jsonMessage, "\\\"deaths\\\":([\\s\\S]*?),");
                MatchCollection assistsCollection = Regex.Matches(jsonMessage, "\\\"assists\\\":([\\s\\S]*?),");
                MatchCollection skillCollection = Regex.Matches(jsonMessage, "\\\"skill\\\":([\\s\\S]*?),");
                MatchCollection xp_per_minCollection = Regex.Matches(jsonMessage, "\\\"xp_per_min\\\":([\\s\\S]*?),");
                MatchCollection gold_per_minCollection = Regex.Matches(jsonMessage, "\\\"gold_per_min\\\":([\\s\\S]*?),");
                MatchCollection hero_damageCollection = Regex.Matches(jsonMessage, "\\\"hero_damage\\\":([\\s\\S]*?),");
                MatchCollection tower_damageCollection = Regex.Matches(jsonMessage, "\\\"tower_damage\\\":([\\s\\S]*?),");
                MatchCollection hero_healingCollection = Regex.Matches(jsonMessage, "\\\"hero_healing\\\":([\\s\\S]*?),");
                MatchCollection last_hitsCollection = Regex.Matches(jsonMessage, "\\\"last_hits\\\":([\\s\\S]*?),");

                for (int i = 0; i < match_idCollection.Count; i++)
                {
                    string color = "#FFFFFF";
                    switch (player_slotCollection[i].Groups[1].Value)
                    {
                        case "0":
                            color = "#3375FF";
                            break;
                        case "1":
                            color = "#66FFBF";
                            break;
                        case "2":
                            color = "#BF00BF";
                            break;
                        case "3":
                            color = "#F3F00B";
                            break;
                        case "4":
                            color = "#FF6B00";
                            break;
                        case "128":
                            color = "#FE86C2";
                            break;
                        case "129":
                            color = "#A1B447";
                            break;
                        case "130":
                            color = "#65D9F7";
                            break;
                        case "131":
                            color = "#008321";
                            break;
                        case "132":
                            color = "#A46900";
                            break;
                        default:
                            color = "#FFFFFF";
                            break;
                    }
                    recentMatches.Add(new RecentMatch
                    {
                        match_id = match_idCollection[i].Groups[1].Value,
                        player_slot = player_slotCollection[i].Groups[1].Value,
                        radiant_win = radiant_winCollection[i].Groups[1].Value,
                        duration = durationCollection[i].Groups[1].Value,
                        game_mode = game_modeCollection[i].Groups[1].Value,
                        lobby_type = lobby_typeCollection[i].Groups[1].Value,
                        hero_id = hero_idCollection[i].Groups[1].Value,
                        start_time = start_timeCollection[i].Groups[1].Value,
                        kills = killsCollection[i].Groups[1].Value,
                        deaths = deathsCollection[i].Groups[1].Value,
                        assists = assistsCollection[i].Groups[1].Value,
                        skill = skillCollection[i].Groups[1].Value,
                        xp_per_min = xp_per_minCollection[i].Groups[1].Value,
                        gold_per_min = gold_per_minCollection[i].Groups[1].Value,
                        hero_damage = hero_damageCollection[i].Groups[1].Value,
                        tower_damage = tower_damageCollection[i].Groups[1].Value,
                        hero_healing = hero_healingCollection[i].Groups[1].Value,
                        last_hits = last_hitsCollection[i].Groups[1].Value,
                        player_color = color
                    });
                }
            }
            catch
            {
                return null;
            }
            return recentMatches;
        }

        /// <summary>
        /// 请求更新数据
        /// </summary>
        /// <param name="id"></param>
        public async static void PostRefreshAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}/refresh", id);
            HttpClient http = new HttpClient();
            await http.PostAsync(new Uri(url), null);
        }

        /// <summary>
        /// 获取指定编号的比赛的详细信息（没解析玩家）
        /// </summary>
        /// 技能图片：https://www.dota2.com.cn/images/heroes/abilities/{英雄名加技能名，如chaos_knight_chaos_bolt}_hp1.png
        /// <param name="matchid"></param>
        public static async Task<List<string>> GetMatchInfoAsync(string matchid)
        {
            //示例比赛编号3792271763
            string url = String.Format("https://api.opendota.com/api/matches/{0}", matchid);
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.UseProxy = false;   //不加这个会非常慢
            HttpClient http = new HttpClient();
            List<string> matchInfoList = new List<string>();
            string jsonMessage;
            try
            {
                if (HeroPlayerInfo.CurrentMatchID != matchid || HeroPlayerInfo.buffer.StartsWith("{\"error\""))
                {
                    var response = await http.GetAsync(new Uri(url));
                    jsonMessage = await response.Content.ReadAsStringAsync();

                    HeroPlayerInfo.buffer = jsonMessage;
                    HeroPlayerInfo.CurrentMatchID = matchid;
                    if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                    {
                        matchInfoList.Clear();
                        matchInfoList.Add("time_limit");
                        return matchInfoList;
                    }
                    else if (jsonMessage == "{\"error\":\"Internal Server Error\"}")
                    {
                        matchInfoList.Clear();
                        matchInfoList.Add("server_error");
                        return matchInfoList;
                    }
                }
                else
                {
                    jsonMessage = HeroPlayerInfo.buffer;
                }
                Match first_blood_timeMatch = Regex.Match(jsonMessage, "\\\"first_blood_time\\\":([\\d\\D]*?),");
                //Match start_timeMatch = Regex.Match(jsonMessage, "\\\"start_time\\\":([\\d\\D]*?),");
                Match durationMatch = Regex.Match(jsonMessage, "\\\"duration\\\":([\\d\\D]*?),");
                //Match levelMatch = Regex.Match(jsonMessage, "\\\"skill\\\":([\\d\\D]*?),");
                Match game_modeMatch = Regex.Match(jsonMessage, "\\\"game_mode\\\":([\\d\\D]*?),");
                Match replay_urlMatch = Regex.Match(jsonMessage, "\\\"replay_url\\\":\\\"([\\d\\D]*?)\\\"}");
                Match radiant_scoreMatch = Regex.Match(jsonMessage, "\\\"radiant_score\\\":([\\d\\D]*?),");
                Match dire_scoreMatch = Regex.Match(jsonMessage, "\\\"dire_score\\\":([\\d\\D]*?),");
                Match lobby_typeMatch = Regex.Match(jsonMessage, "\\\"lobby_type\\\":([\\d\\D]*?),");
                Match radiant_winMatch = Regex.Match(jsonMessage, "\\\"radiant_win\\\":([\\d\\D]*?),");
                Match radiant_gold_advMatch = Regex.Match(jsonMessage, "\\\"radiant_gold_adv\\\":\\[([\\d\\D]*?)\\],");
                Match radiant_xp_advMatch = Regex.Match(jsonMessage, "\\\"radiant_xp_adv\\\":\\[([\\d\\D]*?)\\],");


                matchInfoList.AddRange(new List<string> {
                    first_blood_timeMatch.Groups[1].Value,
                    durationMatch.Groups[1].Value,
                    game_modeMatch.Groups[1].Value,
                    replay_urlMatch.Groups[1].Value,
                    radiant_scoreMatch.Groups[1].Value,
                    dire_scoreMatch.Groups[1].Value,
                    lobby_typeMatch.Groups[1].Value,
                    radiant_winMatch.Groups[1].Value,
                    radiant_gold_advMatch.Groups[1].Value,
                    radiant_xp_advMatch.Groups[1].Value
                });
            }
            catch
            {
                matchInfoList.Clear();
                matchInfoList.Add("data_error");
                return matchInfoList;
            }
            return matchInfoList;
        }

        /// <summary>
        /// 获取本场比赛指定玩家的具体数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static HeroPlayerInfoViewModel GetHeroPlayerInfo(int index)
        {
            #region 匹配
            MatchCollection ability_upgrades_arrMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"ability_upgrades_arr\\\":\\[([\\s\\S]*?)\\],");
            MatchCollection backpackItemsMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"assists\\\":([\\d]*?),\\\"backpack_0\\\":([\\s\\S]*?),\\\"backpack_1\\\":([\\s\\S]*?),\\\"backpack_2\\\":([\\s\\S]*?),");
            MatchCollection hero_idMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"hero_id\\\":([\\s\\S]*?),");
            MatchCollection hero_damageMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"hero_damage\\\":([\\s\\S]*?),");
            MatchCollection hero_healingMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"hero_healing\\\":([\\s\\S]*?),");
            MatchCollection itemsMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"hero_id\\\":[\\d]*?,\\\"item_0\\\":([\\s\\S]*?),\\\"item_1\\\":([\\s\\S]*?),\\\"item_2\\\":([\\s\\S]*?),\\\"item_3\\\":([\\s\\S]*?),\\\"item_4\\\":([\\s\\S]*?),\\\"item_5\\\":([\\s\\S]*?),");
            MatchCollection permanent_buffsMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"permanent_buffs\\\":([\\s\\S]*?),\\\"pings");
            MatchCollection total_goldMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"total_gold\\\":([\\s\\S]*?),");
            MatchCollection total_xpMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"total_xp\\\":([\\s\\S]*?),");
            MatchCollection party_sizeMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"party_size\\\":([\\s\\S]*?),");
            MatchCollection rank_tierMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"rank_tier\\\":([\\s\\S]*?),");
            MatchCollection benchmarksMatch = Regex.Matches(HeroPlayerInfo.buffer, "\\\"benchmarks\\\":{([\\s\\S]*?)}}");
            #endregion

            #region 赋值
            HeroPlayerInfo heroPlayerInfo = new HeroPlayerInfo();
            try
            {
                heroPlayerInfo.Ability_upgrades_arr = ability_upgrades_arrMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Ability_upgrades_arr = ""; }
            try
            {
                heroPlayerInfo.Benchmarks = benchmarksMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Benchmarks = ""; }
            try
            {
                heroPlayerInfo.Hero_id = hero_idMatch[index + hero_idMatch.Count - 10].Groups[1].Value;
            }
            catch { heroPlayerInfo.Hero_id = ""; }
            try
            {
                heroPlayerInfo.Hero_damage = hero_damageMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Hero_damage = ""; }
            try
            {
                heroPlayerInfo.Hero_healing = hero_healingMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Hero_healing = ""; }
            try
            {
                heroPlayerInfo.Rank_tier = rank_tierMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Rank_tier = ""; }
            try
            {
                heroPlayerInfo.Total_gold = total_goldMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Total_gold = ""; }
            try
            {
                heroPlayerInfo.Total_xp = total_xpMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Total_xp = ""; }
            try
            {
                heroPlayerInfo.Party_size = party_sizeMatch[index].Groups[1].Value;
                switch (heroPlayerInfo.Party_size)
                {
                    case "1":
                        heroPlayerInfo.Party_size = "单排";
                        break;
                    case "2":
                        heroPlayerInfo.Party_size = "二黑";
                        break;
                    case "3":
                        heroPlayerInfo.Party_size = "三黑";
                        break;
                    case "4":
                        heroPlayerInfo.Party_size = "四黑";
                        break;
                    case "5":
                        heroPlayerInfo.Party_size = "五黑";
                        break;
                    default:
                        break;
                }
            }
            catch { heroPlayerInfo.Party_size = ""; }
            try
            {
                heroPlayerInfo.Permanent_buffs = permanent_buffsMatch[index].Groups[1].Value;
            }
            catch { heroPlayerInfo.Permanent_buffs = ""; }
            heroPlayerInfo.Item_0 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[1].Value]);
            heroPlayerInfo.Item_1 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[2].Value]);
            heroPlayerInfo.Item_2 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[3].Value]);
            heroPlayerInfo.Item_3 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[4].Value]);
            heroPlayerInfo.Item_4 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[5].Value]);
            heroPlayerInfo.Item_5 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[itemsMatch[index].Groups[6].Value]);
            heroPlayerInfo.Backpack_0 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[backpackItemsMatch[index].Groups[1].Value]);
            heroPlayerInfo.Backpack_1 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[backpackItemsMatch[index].Groups[2].Value]);
            heroPlayerInfo.Backpack_2 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[backpackItemsMatch[index].Groups[3].Value]);
            #endregion
            return new HeroPlayerInfoViewModel(heroPlayerInfo);
        }

        /// <summary>
        /// 其实和上一个方法基本一样，只不过单独返回团战的json，而且要格式化一下，所以可能很慢，单独做一个方法
        /// </summary>
        /// <param name="matchid"></param>
        /// <returns></returns>
        public static async Task<string> GetTeamfightInfoAsync(string matchid)
        {
            //示例比赛编号3792271763
            string url = String.Format("https://api.opendota.com/api/matches/{0}", matchid);
            HttpClient http = new HttpClient();
            List<string> matchInfoList = new List<string>();
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    return "";
                }

                Match team_fightMatch = Regex.Match(jsonMessage, "(\\\"teamfights\\\":[\\s\\S]*?),\\\"tower_status_dire\\\"");
                return FormatJson(new StringBuilder(team_fightMatch.Groups[1].Value.Replace("\\", "")));
            }
            catch { return "数据读取错误！请联系开发者yaoyiming123@live.com，我们将尽快解决这个问题，感激不尽！"; }
        }

        /// <summary>
        /// 格式化JSON数据
        /// </summary>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        private static string FormatJson(StringBuilder stringBuilder)
        {
            string jsonSource = stringBuilder.ToString();
            int stack = 0;
            int index = 0;
            Stack<int> indent = new Stack<int>();
            StringBuilder formatSB = new StringBuilder("");
            while (index < stringBuilder.Length)
            {
                if (stack == 0 && (stringBuilder[index] == ' ' || stringBuilder[index] == '\r' || stringBuilder[index] == '\n' || stringBuilder[index] == '\t'))
                {
                    index++;
                    continue;
                }
                if (stringBuilder[index] == '\"' && stack == 0)
                {
                    formatSB.Append('\"');
                    stack = 1;
                }
                else if (stringBuilder[index] == '\"' && stack == 1)
                {
                    formatSB.Append('\"');
                    stack = 0;
                }
                else if (stack == 0 && stringBuilder[index] == '{')
                {
                    formatSB.Append("{");
                    formatSB.AppendLine();
                    indent.Push(0);
                    for (int i = 0; i < indent.Count; i++)
                    {
                        formatSB.Append("  ");
                    }
                }
                else if (stack == 0 && stringBuilder[index] == ',')
                {
                    formatSB.Append(",");
                    formatSB.AppendLine();
                    for (int i = 0; i < indent.Count; i++)
                    {
                        formatSB.Append("  ");
                    }
                }
                else if (stack == 0 && stringBuilder[index] == '[')
                {
                    formatSB.Append("[");
                    formatSB.AppendLine();
                    indent.Push(0);
                    for (int i = 0; i < indent.Count; i++)
                    {
                        formatSB.Append("  ");
                    }
                }
                else if (stack == 0 && stringBuilder[index] == '}')
                {
                    formatSB.AppendLine();
                    indent.Pop();
                    for (int i = 0; i < indent.Count; i++)
                    {
                        formatSB.Append("  ");
                    }
                    formatSB.Append("}");
                }
                else if (stack == 0 && stringBuilder[index] == ']')
                {
                    formatSB.AppendLine();
                    indent.Pop();
                    for (int i = 0; i < indent.Count; i++)
                    {
                        formatSB.Append("  ");
                    }
                    formatSB.Append("]");
                }
                else if (stack == 0 && stringBuilder[index] == ':')
                {
                    formatSB.Append(": ");
                }
                else
                {
                    formatSB.Append(stringBuilder[index]);
                }
                index++;
            }
            return formatSB.ToString();
        }

        /// <summary>
        /// 获取一场比赛的各名玩家的信息
        /// </summary>
        /// <param name="matchid"></param>
        /// <returns></returns>
        public static async Task<List<PlayersInfo>> GetPlayersInfoAsync(string matchid)
        {
            string url = String.Format("https://api.opendota.com/api/matches/{0}", matchid);
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.UseProxy = false;   //不加这个会非常慢
            HttpClient http = new HttpClient();
            List<PlayersInfo> playersInfoList = new List<PlayersInfo>();
            string jsonMessage;
            try
            {
                if (HeroPlayerInfo.CurrentMatchID != matchid || HeroPlayerInfo.buffer.StartsWith("{\"error\""))
                {
                    var response = await http.GetAsync(new Uri(url));
                    jsonMessage = await response.Content.ReadAsStringAsync();
                    HeroPlayerInfo.CurrentMatchID = matchid;
                    if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                    {
                        return null;
                    }
                }
                else
                {
                    jsonMessage = HeroPlayerInfo.buffer;
                }

                MatchCollection account_idMatch = Regex.Matches(jsonMessage, "\\\"account_id\\\":([\\s\\S]*?),");
                MatchCollection assistsMatch = Regex.Matches(jsonMessage, "\\\"assists\\\":([\\s\\S]*?),");
                MatchCollection deathsMatch = Regex.Matches(jsonMessage, "\\\"damage_targets\\\"[\\s\\S]*?,\\\"deaths\\\":([\\s\\S]*?),\\\"denies");
                MatchCollection deniesMatch = Regex.Matches(jsonMessage, "\\\"denies\\\":([\\s\\S]*?),");
                MatchCollection killsMatch = Regex.Matches(jsonMessage, "\\\"kills\\\":([\\s\\S]*?),\\\"kills_log");
                MatchCollection last_hitsMatch = Regex.Matches(jsonMessage, "\\\"last_hits\\\":([\\s\\S]*?),");
                MatchCollection levelMatch = Regex.Matches(jsonMessage, "\\\"level\\\":([\\s\\S]*?),");
                MatchCollection hero_damageMatch = Regex.Matches(jsonMessage, "\\\"hero_damage\\\":([\\s\\S]*?),");
                MatchCollection hero_idItemsMatch = Regex.Matches(jsonMessage, "\\\"hero_id\\\":([\\d]*?),\\\"item_0\\\":([\\s\\S]*?),\\\"item_1\\\":([\\s\\S]*?),\\\"item_2\\\":([\\s\\S]*?),\\\"item_3\\\":([\\s\\S]*?),\\\"item_4\\\":([\\s\\S]*?),\\\"item_5\\\":([\\s\\S]*?),");

                //String.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", id);
                for (int i = 0; i < 10; i++)
                {
                    string color = "#FFFFFF";
                    switch (i)
                    {
                        case 0:
                            color = "#3375FF";
                            break;
                        case 1:
                            color = "#66FFBF";
                            break;
                        case 2:
                            color = "#BF00BF";
                            break;
                        case 3:
                            color = "#F3F00B";
                            break;
                        case 4:
                            color = "#FF6B00";
                            break;
                        case 5:
                            color = "#FE86C2";
                            break;
                        case 6:
                            color = "#A1B447";
                            break;
                        case 7:
                            color = "#65D9F7";
                            break;
                        case 8:
                            color = "#008321";
                            break;
                        case 9:
                            color = "#A46900";
                            break;
                        default:
                            color = "#FFFFFF";
                            break;
                    }
                    try
                    {
                        playersInfoList.Add(new PlayersInfo
                        {
                            Account_id = account_idMatch[i].Groups[1].Value,
                            Assists = assistsMatch[i].Groups[1].Value,
                            Deaths = deathsMatch[i].Groups[1].Value,
                            Denies = deniesMatch[i].Groups[1].Value,
                            Kills = killsMatch[i].Groups[1].Value,
                            Last_hits = last_hitsMatch[i].Groups[1].Value,
                            Level = levelMatch[i].Groups[1].Value,
                            Hero_damage = hero_damageMatch[i].Groups[1].Value,
                            Hero_id = hero_idItemsMatch[i].Groups[1].Value,
                            Item_0 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[2].Value]),
                            Item_1 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[3].Value]),
                            Item_2 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[4].Value]),
                            Item_3 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[5].Value]),
                            Item_4 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[6].Value]),
                            Item_5 = string.Format("http://www.dota2.com.cn/items/images/{0}_lg.png", ConstantsHelper.ItemsDictionary[hero_idItemsMatch[i].Groups[7].Value]),
                            Color = color
                        });
                    }
                    catch
                    {
                        playersInfoList.Add(new PlayersInfo
                        {
                            Account_id = "Unknown",
                            Assists = "0",
                            Deaths = "0",
                            Denies = "0",
                            Kills = "0",
                            Last_hits = "0",
                            Level = "0",
                            Hero_damage = "0",
                            Hero_id = "1",
                            Item_0 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Item_1 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Item_2 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Item_3 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Item_4 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Item_5 = string.Format("http://www.dota2.com.cn/items/images/null_lg.png"),
                            Color = color
                        });
                    }
                }
            }
            catch
            {
                return null;
            }
            return playersInfoList;
        }

        /// <summary>
        /// 获取玩家常用英雄数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<HeroUsingInfo>> GetHeroUsingAsync(string id)
        {
            string url = String.Format("https://api.opendota.com/api/players/{0}/heroes", id);
            HttpClient http = new HttpClient();
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage.Length < 256)
                {
                    return null;
                }

                List<HeroUsingInfo> result = JsonConvert.DeserializeObject<List<HeroUsingInfo>>(jsonMessage);
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 判断比赛类型，仅判断是否为普通、天梯、勇士联赛
        /// </summary>
        /// <param name="lobby_type_id"></param>
        /// <returns></returns>
        public static int GetLobbyType(string lobby_type_id)
        {
            switch (lobby_type_id)
            {
                case "9":
                    return 2;//"勇士联赛";
                case "5":
                case "6":
                case "7":
                    return 1;// "天梯";
                default:
                    return 0;// "普通";
            }
        }

        /// <summary>
        /// 获取当前在线人数
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNumberOfCurrentPlayers()
        {
            string url = "http://api.steampowered.com/ISteamUserStats/GetNumberOfCurrentPlayers/v1?appid=570&format=json";
            HttpClient http = new HttpClient();
            List<PlayersInfo> playersInfoList = new List<PlayersInfo>();
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}")
                {
                    return "";
                }

                Match numberMatch = Regex.Match(jsonMessage, "\\\"player_count\\\":([\\d\\D]*?),");
                return "当前在线玩家：" + numberMatch.Groups[1].Value;
            }
            catch
            {
                return "";
            }
        }
    }

    /// <summary>
    /// 用户的游戏信息比如分段排名等
    /// </summary>
    public class PlayerProfile
    {
        /// <summary>
        /// 
        /// </summary>
        public string tracked_until { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Profile profile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string competitive_rank { get; set; }
        /// <summary>
        /// 冠绝一世排名
        /// </summary>
        public string leaderboard_rank { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string solo_competitive_rank { get; set; }
        /// <summary>
        /// 分段，十位数表示徽章，个位数表示星级
        /// </summary>
        public string rank_tier { get; set; }
        /// <summary>
        /// MMR（估计值，非天梯分数）
        /// </summary>
        public Mmr_estimate mmr_estimate { get; set; }

        public PlayerProfile()
        {
            this.tracked_until = "";
            this.competitive_rank = "";
            this.leaderboard_rank = "";
            this.solo_competitive_rank = "0";
            this.rank_tier = "10";
            this.mmr_estimate = new Mmr_estimate();
            this.profile = new Profile();
        }
    }
    // 用户个人信息,例如ID,名字等
    public class Profile
    {
        /// <summary>
        /// 游戏内ID
        /// </summary>
        public int account_id { get; set; }
        /// <summary>
        /// Steam名字
        /// </summary>
        public string personaname { get; set; }

        public string name { get; set; }

        public int cheese { get; set; }
        /// <summary>
        /// SteamID
        /// </summary>
        public string steamid { get; set; }

        public string avatar { get; set; }

        public string avatarmedium { get; set; }
        /// <summary>
        /// 大图头像
        /// </summary>
        public string avatarfull { get; set; }

        public string profileurl { get; set; }

        public string last_login { get; set; }

        public string loccountrycode { get; set; }

        public Profile()
        {
            this.account_id = 0;
            this.avatar = "";
            this.avatarfull = "";
            this.avatarmedium = "";
            this.cheese = 0;
            this.last_login = "";
            this.loccountrycode = "";
            this.name = "";
            this.personaname = "";
            this.profileurl = "";
            this.steamid = "";
        }
    }
    //MMR评估(非天梯)
    public class Mmr_estimate
    {
        /// <summary>
        /// MMR分数评估
        /// </summary>
        public int estimate { get; set; }

        public Mmr_estimate() { estimate = 0; }
    }

    /// <summary>
    /// 用户的胜局败局数量
    /// </summary>
    public class WinNLose
    {
        /// <summary>
        /// 胜利
        /// </summary>
        public double win { get; set; }
        /// <summary>
        /// 失败
        /// </summary>
        public double lose { get; set; }

        public WinNLose() { win = 0; lose = 0; }
    }

    /// <summary>
    /// 最近比赛的各项数据,没办法只能用正则了
    /// </summary>
    public class RecentMatch
    {
        /// <summary>
        /// 比赛ID
        /// </summary>
        public string match_id { get; set; }
        /// <summary>
        /// 玩家的位置（几楼）0-4：天辉   128-132：夜魇
        /// </summary>
        public string player_slot { get; set; }
        /// <summary>
        /// 天辉获胜与否
        /// </summary>
        public string radiant_win { get; set; }
        /// <summary>
        /// 游戏时间
        /// </summary>
        public string duration { get; set; }
        /// <summary>
        /// 游戏模式
        /// </summary>
        public string game_mode { get; set; }
        /// <summary>
        /// 比赛类型，比如练习赛、锦标赛、天梯等
        /// </summary>
        public string lobby_type { get; set; }
        /// <summary>
        /// 玩家所用的英雄
        /// </summary>
        public string hero_id { get; set; }
        /// <summary>
        /// 开始时间，自1970年开始计秒
        /// </summary>
        public string start_time { get; set; }
        /// <summary>
        /// 击杀数
        /// </summary>
        public string kills { get; set; }
        /// <summary>
        /// 死亡数
        /// </summary>
        public string deaths { get; set; }
        /// <summary>
        /// 助攻
        /// </summary>
        public string assists { get; set; }
        /// <summary>
        /// 判定N/H/VH局
        /// </summary>
        public string skill { get; set; }
        /// <summary>
        /// 每分钟经验
        /// </summary>
        public string xp_per_min { get; set; }
        /// <summary>
        /// 每分钟金钱
        /// </summary>
        public string gold_per_min { get; set; }
        /// <summary>
        /// 英雄伤害
        /// </summary>
        public string hero_damage { get; set; }
        /// <summary>
        /// 建筑伤害
        /// </summary>
        public string tower_damage { get; set; }
        /// <summary>
        /// 英雄治疗
        /// </summary>
        public string hero_healing { get; set; }
        /// <summary>
        /// 正补——————————————————————————竟然没有反补的数据，记得去别的API找一下
        /// </summary>
        public string last_hits { get; set; }
        /// <summary>
        /// 玩家ID颜色
        /// </summary>
        public string player_color { get; set; }
    }

    public class RecentMatchViewModel
    {
        public string SelectedHero { get; set; }

        public string HeroPhoto { get; set; }

        /// <summary>
        /// 比赛结果，该玩家所在阵营胜利/失败，两个结果肯定是相反的
        /// </summary>
        public Windows.UI.Xaml.Visibility PlayerWin { get; set; }
        public Windows.UI.Xaml.Visibility PlayerLose { get; set; }

        public string Skill { get; set; }

        public string Time { get; set; }

        public string KDA { get; set; }

        public string GPM { get; set; }

        public string XPM { get; set; }

        public string Match_ID { get; set; }
    }

    public class PlayersInfo
    {
        public string Account_id { get; set; }
        public string Assists { get; set; }
        public string Deaths { get; set; }
        public string Denies { get; set; }
        public string Kills { get; set; }
        public string Last_hits { get; set; }
        public string Level { get; set; }
        public string Item_0 { get; set; }
        public string Item_1 { get; set; }
        public string Item_2 { get; set; }
        public string Item_3 { get; set; }
        public string Item_4 { get; set; }
        public string Item_5 { get; set; }
        public string Hero_damage { get; set; }
        public string Hero_id { get; set; }
        public string Color { get; set; }
    }

    public class PlayersInfoViewModel
    {
        public string HeroPhoto { get; set; }
        public string PlayerName { get; set; }
        public string Fight_rate { get; set; }  //在外面计算
        public string Damage_rate { get; set; } //在外面计算
        public string LD { get; set; }
        public double Kills { get; set; }
        public double Deaths { get; set; }
        public double Assists { get; set; }
        public string K_D_A { get; set; }
        public string KDA { get; set; }
        public string Item_0 { get; set; }
        public string Item_1 { get; set; }
        public string Item_2 { get; set; }
        public string Item_3 { get; set; }
        public string Item_4 { get; set; }
        public string Item_5 { get; set; }
        public string PlayerPhoto { get; set; }
        public string Level { get; set; }
        public string Hero_damage { get; set; }
        public string Account_id { get; set; }
        public string Color { get; set; }

        public PlayersInfoViewModel(PlayersInfo playersInfo)
        {
            this.Item_0 = playersInfo.Item_0 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_0;
            this.Item_1 = playersInfo.Item_1 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_1;
            this.Item_2 = playersInfo.Item_2 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_2;
            this.Item_3 = playersInfo.Item_3 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_3;
            this.Item_4 = playersInfo.Item_4 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_4;
            this.Item_5 = playersInfo.Item_5 == "http://www.dota2.com.cn/items/images/_lg.png" ? "ms-appx:///Assets/Pictures/null.png" : playersInfo.Item_5;
            this.Level = playersInfo.Level;
            this.Hero_damage = playersInfo.Hero_damage;
            this.HeroPhoto = ConstantsHelper.dotaHerosDictionary[ConstantsHelper.HeroID[Convert.ToInt32(playersInfo.Hero_id)]].Pic;
            this.K_D_A = playersInfo.Kills + "/" + playersInfo.Deaths + "/" + playersInfo.Assists;
            double deaths = playersInfo.Deaths == "0" ? 1 : Convert.ToDouble(playersInfo.Deaths);
            this.KDA = "KDA：" + ((Convert.ToDouble(playersInfo.Kills) + Convert.ToDouble(playersInfo.Assists)) / deaths).ToString("0.00");
            this.LD = "正/反：" + playersInfo.Last_hits + "/" + playersInfo.Denies;
            this.Account_id = playersInfo.Account_id;
            this.PlayerName = "匿名玩家";
            this.PlayerPhoto = "ms-appx:///Assets/Pictures/null.png";
            this.Kills = Convert.ToDouble(playersInfo.Kills);
            this.Deaths = Convert.ToDouble(playersInfo.Deaths);
            this.Assists = Convert.ToDouble(playersInfo.Assists);
            this.Color = playersInfo.Color;
        }
    }

    public class HeroUsingInfo
    {
        public string hero_id { get; set; }
        public string last_played { get; set; }
        public double games { get; set; }
        public double win { get; set; }
        public double with_games { get; set; }
        public double with_win { get; set; }
        public double against_games { get; set; }
        public double against_win { get; set; }
    }

    public class HeroUsingInfoViewModel
    {
        public string Photo { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public double Games { get; set; }
        public double Win { get; set; }
        public double Win_rate { get; set; }
        public double With { get; set; }
        public double With_win { get; set; }
        public double Against { get; set; }
        public double Against_win { get; set; }

        public HeroUsingInfoViewModel() { }
        public HeroUsingInfoViewModel(HeroUsingInfo heroUsingInfo)
        {
            this.Photo = ConstantsHelper.dotaHerosDictionary[ConstantsHelper.HeroID[Convert.ToInt32(heroUsingInfo.hero_id)]].IconPic;
            this.Name = ConstantsHelper.dotaHerosDictionary[ConstantsHelper.HeroID[Convert.ToInt32(heroUsingInfo.hero_id)]].Name;
            this.Games = heroUsingInfo.games;
            this.Win = heroUsingInfo.win;
            this.Win_rate = Math.Round(100 * heroUsingInfo.win / heroUsingInfo.games, 1);
            this.With = heroUsingInfo.with_games;
            this.With_win = heroUsingInfo.with_win;
            this.Against = heroUsingInfo.against_games;
            this.Against_win = heroUsingInfo.against_win;
        }
    }

    public class HeroPlayerInfo
    {
        /// <summary>
        /// 缓存字符串，当查看比赛数据时将获取到的比赛json缓存，避免点击玩家查看其更详细数据时又要用API获取一次
        /// </summary>
        public static string buffer = "";

        /// <summary>
        /// 当前缓存的字符串对应的比赛编号
        /// </summary>
        public static string CurrentMatchID { get; set; }

        public string Item_0 { get; set; }
        public string Item_1 { get; set; }
        public string Item_2 { get; set; }
        public string Item_3 { get; set; }
        public string Item_4 { get; set; }
        public string Item_5 { get; set; }
        public string Backpack_0 { get; set; }
        public string Backpack_1 { get; set; }
        public string Backpack_2 { get; set; }
        public string Hero_damage { get; set; }
        public string Hero_healing { get; set; }
        public string Hero_id { get; set; }
        public string Party_size { get; set; }
        public string Total_gold { get; set; }
        public string Total_xp { get; set; }
        public string Rank_tier { get; set; }
        public string Ability_upgrades_arr { get; set; }
        public string Permanent_buffs { get; set; }
        public string Benchmarks { get; set; }
    }

    public class HeroPlayerInfoViewModel
    {
        public class Buff
        {
            public string Permanent_buff { get; set; }

            private string _stack_count;
            public string Stack_count
            {
                get { return _stack_count; }
                set
                {
                    if (value == "0")
                    {
                        _stack_count = "";
                    }
                    else
                    {
                        _stack_count = value;
                    }
                }
            }
        }

        public class Benchmark
        {
            private string _raw;
            public string Raw
            {
                get { return _raw; }
                set
                {
                    if (value == null || value == "null")
                    {
                        _raw = "Null";
                        return;
                    }
                    if (value.Length > 6)
                    {
                        _raw = value.Substring(0, 6);
                    }
                    else
                    {
                        _raw = value;
                    }
                }
            }

            private string _pct;
            public string Pct
            {
                get { return _pct; }
                set
                {
                    if (value == null || value == "null")
                    {
                        _pct = "Null";
                        return;
                    }
                    if (value.Length > 6)
                    {
                        _pct = value.Substring(2, 2) + "." + value.Substring(4, 2) + "%";
                    }
                    else
                    {
                        _pct = double.Parse(value) * 100 + "%";
                    }
                }
            }
        }


        // 这些取自十个人的总体比赛数据页面
        public string Account_id { get; set; }
        public string Personaname { get; set; }
        public string KDAString { get; set; }
        public string Last_hits { get; set; }
        public string Denies { get; set; }
        public string KDA { get; set; }
        public string Level { get; set; }

        // 这些取自新解析的数据
        public string Hero_id { get; set; }
        public string Hero_name { get; set; }
        public string Hero_photo { get; set; }
        public string Item_0 { get; set; }
        public string Item_1 { get; set; }
        public string Item_2 { get; set; }
        public string Item_3 { get; set; }
        public string Item_4 { get; set; }
        public string Item_5 { get; set; }
        public string Backpack_0 { get; set; }
        public string Backpack_1 { get; set; }
        public string Backpack_2 { get; set; }
        public string Hero_damage { get; set; }
        public string Hero_healing { get; set; }
        public string Party_size { get; set; }
        public string Total_gold { get; set; }
        public string Total_xp { get; set; }
        public List<string> Ability_upgrades_arr { get; set; }
        public List<Buff> Permanent_buffs { get; set; }
        public string Rank_tier { get; set; }

        //这三个数据直接从benchmark取
        public string GPM { get; set; }
        public string XPM { get; set; }
        public string Tower_damage { get; set; }

        /// <summary>
        /// GPM排名
        /// </summary>
        public Benchmark Benchmarks_gpm { get; set; }

        /// <summary>
        /// XPM排名
        /// </summary>
        public Benchmark Benchmarks_xpm { get; set; }

        /// <summary>
        /// Kill per minute排名
        /// </summary>
        public Benchmark Benchmarks_kill_per_minute { get; set; }

        /// <summary>
        /// last hit per minute排名
        /// </summary>
        public Benchmark Benchmarks_last_hit_per_minute { get; set; }

        /// <summary>
        /// hero damage per minute排名
        /// </summary>
        public Benchmark Benchmarks_hero_damage_per_minute { get; set; }

        /// <summary>
        /// hero healing per minute排名
        /// </summary>
        public Benchmark Benchmarks_hero_healing_per_minute { get; set; }

        /// <summary>
        /// tower damage排名
        /// </summary>
        public Benchmark Benchmarks_tower_damage { get; set; }

        public HeroPlayerInfoViewModel(HeroPlayerInfo heroPlayerInfo)
        {
            List<Benchmark> got = GetBenchmarks(heroPlayerInfo.Benchmarks);
            this.Benchmarks_gpm = got[0];
            this.Benchmarks_hero_damage_per_minute = got[4];
            this.Benchmarks_hero_healing_per_minute = got[5];
            this.Benchmarks_kill_per_minute = got[2];
            this.Benchmarks_last_hit_per_minute = got[3];
            this.Benchmarks_tower_damage = got[6];
            this.Benchmarks_xpm = got[1];

            this.Ability_upgrades_arr = GetAbility_upgrades_arr(heroPlayerInfo.Ability_upgrades_arr);
            this.Permanent_buffs = GetBuffs(heroPlayerInfo.Permanent_buffs);
            this.Backpack_0 = heroPlayerInfo.Backpack_0;
            this.Backpack_1 = heroPlayerInfo.Backpack_1;
            this.Backpack_2 = heroPlayerInfo.Backpack_2;
            this.Item_0 = heroPlayerInfo.Item_0;
            this.Item_1 = heroPlayerInfo.Item_1;
            this.Item_2 = heroPlayerInfo.Item_2;
            this.Item_3 = heroPlayerInfo.Item_3;
            this.Item_4 = heroPlayerInfo.Item_4;
            this.Item_5 = heroPlayerInfo.Item_5;
            this.GPM = this.Benchmarks_gpm.Raw;
            this.Hero_damage = heroPlayerInfo.Hero_damage;
            this.Hero_healing = heroPlayerInfo.Hero_healing;
            this.Party_size = heroPlayerInfo.Party_size;
            this.Total_gold = heroPlayerInfo.Total_gold;
            this.Total_xp = heroPlayerInfo.Total_xp;
            this.Tower_damage = this.Benchmarks_tower_damage.Raw;
            this.XPM = this.Benchmarks_xpm.Raw;
            this.Rank_tier = heroPlayerInfo.Rank_tier;

            this.Hero_id = heroPlayerInfo.Hero_id;
            this.Hero_name = ConstantsHelper.dotaHerosDictionary[ConstantsHelper.HeroID[Convert.ToInt32(heroPlayerInfo.Hero_id)]].Name;
            this.Hero_photo = ConstantsHelper.dotaHerosDictionary[ConstantsHelper.HeroID[Convert.ToInt32(heroPlayerInfo.Hero_id)]].Pic;
        }

        private List<Benchmark> GetBenchmarks(string regular)
        {
            Match gold_per_minMatch = Regex.Match(regular, "\\\"gold_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match xp_per_minMatch = Regex.Match(regular, "\\\"xp_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match kill_per_minMatch = Regex.Match(regular, "\\\"kills_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match lh_per_minMatch = Regex.Match(regular, "\\\"last_hits_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match hero_damage_per_minMatch = Regex.Match(regular, "\\\"hero_damage_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match hero_healing_per_minMatch = Regex.Match(regular, "\\\"hero_healing_per_min\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            Match tower_damage_per_minMatch = Regex.Match(regular, "\\\"tower_damage\\\":{\\\"raw\\\":([\\d\\D]*?),\\\"pct\\\":([\\d\\D]*?)}");
            List<Benchmark> benchmarks = new List<Benchmark>();
            benchmarks.Add(new Benchmark { Raw = gold_per_minMatch.Groups[1].Value, Pct = gold_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = xp_per_minMatch.Groups[1].Value, Pct = xp_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = kill_per_minMatch.Groups[1].Value, Pct = kill_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = lh_per_minMatch.Groups[1].Value, Pct = lh_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = hero_damage_per_minMatch.Groups[1].Value, Pct = hero_damage_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = hero_healing_per_minMatch.Groups[1].Value, Pct = hero_healing_per_minMatch.Groups[2].Value });
            benchmarks.Add(new Benchmark { Raw = tower_damage_per_minMatch.Groups[1].Value, Pct = tower_damage_per_minMatch.Groups[2].Value });
            return benchmarks;
        }

        private List<Buff> GetBuffs(string regular)
        {
            MatchCollection buffsCollection = Regex.Matches(regular, "{\\\"permanent_buff\\\":([\\d\\D]*?),\\\"stack_count\\\":([\\d\\D]*?)}");
            List<Buff> buffsList = new List<Buff>();
            for (int i = 0; i < buffsCollection.Count; i++)
            {
                buffsList.Add(new Buff { Permanent_buff = buffsCollection[i].Groups[1].Value, Stack_count = buffsCollection[i].Groups[2].Value });
            }
            return buffsList;
        }

        private List<string> GetAbility_upgrades_arr(string regular)
        {
            string[] aua = regular.Replace("\"", "").Split(",");
            return aua.ToList();
        }

    }

    public class AbilityViewModel
    {
        public string ID { get; set; }
        public string Ability { get; set; }
    }

}

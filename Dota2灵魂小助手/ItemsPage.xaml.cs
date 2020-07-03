using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Dota2灵魂小助手
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ItemsPage : Page
    {
        public static DotaItems SelectedItem = new DotaItems();
        public static string json = "";
        public static string jsonComponents = "";

        public ObservableCollection<DotaItems> Consumables = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Attributes = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Armaments = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Arcane = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Common = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Support = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Caster = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Weapons = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Armor = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Artifacts = new ObservableCollection<DotaItems>();
        public ObservableCollection<DotaItems> Secret = new ObservableCollection<DotaItems>();

        public ItemsPage()
        {
            this.InitializeComponent();

            ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
            if (NetworkCheckHelper.CheckNetwork() == false)
            {
                //断网
                this.Frame.Navigate(typeof(NoNetworkPage));
                return;
            }
            else
            {
                if (Secret.Count == 0)
                {
                    try
                    {
                        AddAllItems();
                    }
                    catch
                    {
                        AddAllItems();
                    }
                }
            }
        }

        /// <summary>
        /// 重写导航至此页面的代码,显示动画
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavigationTransitionInfo transition)
            {
                navigationTransition.DefaultNavigationTransitionInfo = transition;
            }
            MainPage.Current.ShowItem.Begin();
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// 将所有的物品添加到列表
        /// </summary>
        private async void AddAllItems()
        {
            ItemProgressRing.Visibility = Visibility.Visible;
            ItemProgressRing.IsActive = true;

            if (json == null || json == "")
            {
                try
                {
                    json = await DotaItemHelper.GetItemDataAsync();
                }
                catch
                {
                    NetworkSlowTextBlock.Visibility = Visibility.Visible;
                    try
                    {
                        NetworkSlowTextBlock.Visibility = Visibility.Visible;
                        json = await DotaItemHelper.GetItemDataAsync();
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            if (jsonComponents == null || jsonComponents == "")
            {
                try
                {
                    jsonComponents = await DotaItemHelper.GetItemDataENAsync();
                }
                catch
                {
                    NetworkSlowTextBlock.Visibility = Visibility.Visible;
                    try
                    {
                        NetworkSlowTextBlock.Visibility = Visibility.Visible;
                        json = await DotaItemHelper.GetItemDataENAsync();
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            Consumables.Add(new DotaItems("净化药水", "clarity"));
            Consumables.Add(new DotaItems("仙灵之火", "faerie_fire"));
            Consumables.Add(new DotaItems("魔法芒果", "enchanted_mango"));
            Consumables.Add(new DotaItems("树之祭祀", "tango"));
            Consumables.Add(new DotaItems("治疗药膏", "flask"));
            Consumables.Add(new DotaItems("诡计之雾", "smoke_of_deceit"));
            Consumables.Add(new DotaItems("回城卷轴", "tpscroll"));
            Consumables.Add(new DotaItems("显影之尘", "dust"));
            Consumables.Add(new DotaItems("动物信使", "courier"));
            //Consumables.Add(new DotaItems("飞行信使", "flying_courier"));
            Consumables.Add(new DotaItems("侦察守卫", "ward_observer"));
            Consumables.Add(new DotaItems("岗哨守卫", "ward_sentry"));
            Consumables.Add(new DotaItems("知识之书", "tome_of_knowledge"));
            Consumables.Add(new DotaItems("魔瓶", "bottle"));

            Attributes.Add(new DotaItems("铁树枝干", "branches"));
            Attributes.Add(new DotaItems("力量手套", "gauntlets"));
            Attributes.Add(new DotaItems("敏捷便鞋", "slippers"));
            Attributes.Add(new DotaItems("智力斗篷", "mantle"));
            Attributes.Add(new DotaItems("圆环", "circlet"));
            Attributes.Add(new DotaItems("力量腰带", "belt_of_strength"));
            Attributes.Add(new DotaItems("精灵布带", "boots_of_elves"));
            Attributes.Add(new DotaItems("法师长袍", "robe"));
            Attributes.Add(new DotaItems("王冠", "crown"));
            Attributes.Add(new DotaItems("食人魔之斧", "ogre_axe"));
            Attributes.Add(new DotaItems("欢欣之刃", "blade_of_alacrity"));
            Attributes.Add(new DotaItems("魔力法杖", "staff_of_wizardry"));

            Armaments.Add(new DotaItems("守护指环", "ring_of_protection"));
            Armaments.Add(new DotaItems("圆盾", "stout_shield"));
            Armaments.Add(new DotaItems("压制之刃", "quelling_blade"));
            Armaments.Add(new DotaItems("凝魂之露", "infused_raindrop"));
            Armaments.Add(new DotaItems("枯萎之石", "blight_stone"));
            Armaments.Add(new DotaItems("淬毒之珠", "orb_of_venom"));
            Armaments.Add(new DotaItems("攻击之爪", "blades_of_attack"));
            Armaments.Add(new DotaItems("锁子甲", "chainmail"));
            Armaments.Add(new DotaItems("短棍", "quarterstaff"));
            Armaments.Add(new DotaItems("铁意头盔", "helm_of_iron_will"));
            Armaments.Add(new DotaItems("阔剑", "broadsword"));
            Armaments.Add(new DotaItems("大剑", "claymore"));
            Armaments.Add(new DotaItems("标枪", "javelin"));
            Armaments.Add(new DotaItems("秘银锤", "mithril_hammer"));

            Arcane.Add(new DotaItems("风灵之纹", "wind_lace"));
            Arcane.Add(new DotaItems("魔棒", "magic_stick"));
            Arcane.Add(new DotaItems("贤者面罩", "sobi_mask"));
            Arcane.Add(new DotaItems("回复戒指", "ring_of_regen"));
            Arcane.Add(new DotaItems("速度之靴", "boots"));
            Arcane.Add(new DotaItems("加速手套", "gloves"));
            Arcane.Add(new DotaItems("抗魔斗篷", "cloak"));
            Arcane.Add(new DotaItems("恐鳌之戒", "ring_of_tarrasque"));
            Arcane.Add(new DotaItems("治疗指环", "ring_of_health"));
            Arcane.Add(new DotaItems("虚无宝石", "void_stone"));
            Arcane.Add(new DotaItems("真视宝石", "gem"));
            Arcane.Add(new DotaItems("吸血面具", "lifesteal"));
            Arcane.Add(new DotaItems("暗影护符", "shadow_amulet"));
            Arcane.Add(new DotaItems("幽魂权杖", "ghost"));
            Arcane.Add(new DotaItems("闪烁匕首", "blink"));

            Common.Add(new DotaItems("怨灵系带", "wraith_band"));
            Common.Add(new DotaItems("空灵挂件", "null_talisman"));
            Common.Add(new DotaItems("魔杖", "magic_wand"));
            Common.Add(new DotaItems("护腕", "bracer"));
            Common.Add(new DotaItems("灵魂之戒", "soul_ring"));
            Common.Add(new DotaItems("相位鞋", "phase_boots"));
            Common.Add(new DotaItems("动力鞋", "power_treads"));
            Common.Add(new DotaItems("空明杖", "oblivion_staff"));
            Common.Add(new DotaItems("坚韧球", "pers"));
            Common.Add(new DotaItems("迈达斯之手", "hand_of_midas"));
            Common.Add(new DotaItems("远行鞋", "travel_boots"));
            Common.Add(new DotaItems("银月之晶", "moon_shard"));

            Support.Add(new DotaItems("王者之戒", "ring_of_basilius"));
            Support.Add(new DotaItems("恢复头巾", "headdress"));
            Support.Add(new DotaItems("玄冥盾牌", "buckler"));
            Support.Add(new DotaItems("影之灵龛", "urn_of_shadows"));
            Support.Add(new DotaItems("天鹰之戒", "ring_of_aquila"));
            Support.Add(new DotaItems("静谧之鞋", "tranquil_boots"));
            Support.Add(new DotaItems("勇气勋章", "medallion_of_courage"));
            Support.Add(new DotaItems("奥术鞋", "arcane_boots"));
            Support.Add(new DotaItems("韧鼓", "ancient_janggo"));
            Support.Add(new DotaItems("弗拉迪米尔的祭品", "vladmir"));
            Support.Add(new DotaItems("梅肯斯姆", "mekansm"));
            Support.Add(new DotaItems("圣洁吊坠", "holy_locket"));
            Support.Add(new DotaItems("魂之灵瓮", "spirit_vessel"));
            Support.Add(new DotaItems("洞察烟斗", "pipe"));
            Support.Add(new DotaItems("卫士胫甲", "guardian_greaves"));

            Caster.Add(new DotaItems("微光披风", "glimmer_cape"));
            Caster.Add(new DotaItems("原力法杖", "force_staff"));
            Caster.Add(new DotaItems("纷争面纱", "veil_of_discord"));
            Caster.Add(new DotaItems("以太之镜", "aether_lens"));
            Caster.Add(new DotaItems("死灵书", "necronomicon"));
            Caster.Add(new DotaItems("达贡之神力", "dagon"));
            Caster.Add(new DotaItems("EUL的神圣法杖", "cyclone"));
            Caster.Add(new DotaItems("炎阳纹章", "solar_crest"));
            Caster.Add(new DotaItems("阿托斯之棍", "rod_of_atos"));
            Caster.Add(new DotaItems("阿哈利姆神杖", "ultimate_scepter"));
            Caster.Add(new DotaItems("否决挂饰", "nullifier"));
            Caster.Add(new DotaItems("紫怨", "orchid"));
            Caster.Add(new DotaItems("刷新球", "refresher"));
            Caster.Add(new DotaItems("邪恶镰刀", "sheepstick"));
            Caster.Add(new DotaItems("玲珑心", "octarine_core"));

            Weapons.Add(new DotaItems("水晶剑", "lesser_crit"));
            Weapons.Add(new DotaItems("莫尔迪基安的臂章", "armlet"));
            Weapons.Add(new DotaItems("陨星锤", "meteor_hammer"));
            Weapons.Add(new DotaItems("碎颅锤", "basher"));
            Weapons.Add(new DotaItems("影刃", "invis_sword"));
            Weapons.Add(new DotaItems("狂战斧", "bfury"));
            Weapons.Add(new DotaItems("虚灵之刃", "ethereal_blade"));
            Weapons.Add(new DotaItems("白银之锋", "silver_edge"));
            Weapons.Add(new DotaItems("辉耀", "radiance"));
            Weapons.Add(new DotaItems("金箍棒", "monkey_king_bar"));
            Weapons.Add(new DotaItems("代达罗斯之殇", "greater_crit"));
            Weapons.Add(new DotaItems("蝴蝶", "butterfly"));
            Weapons.Add(new DotaItems("圣剑", "rapier"));
            Weapons.Add(new DotaItems("深渊之刃", "abyssal_blade"));
            Weapons.Add(new DotaItems("血棘", "bloodthorn"));

            Armor.Add(new DotaItems("挑战头巾", "hood_of_defiance"));
            Armor.Add(new DotaItems("刃甲", "blade_mail"));
            Armor.Add(new DotaItems("先锋盾", "vanguard"));
            Armor.Add(new DotaItems("振魂石", "soul_booster"));
            Armor.Add(new DotaItems("赤红甲", "crimson_guard"));
            Armor.Add(new DotaItems("永恒之盘", "aeon_disk"));
            Armor.Add(new DotaItems("黑皇杖", "black_king_bar"));
            Armor.Add(new DotaItems("清莲宝珠", "lotus_orb"));
            Armor.Add(new DotaItems("希瓦的守护", "shivas_guard"));
            Armor.Add(new DotaItems("幻影斧", "manta"));
            Armor.Add(new DotaItems("血精石", "bloodstone"));
            Armor.Add(new DotaItems("林肯法球", "sphere"));
            Armor.Add(new DotaItems("飓风长戟", "hurricane_pike"));
            Armor.Add(new DotaItems("强袭胸甲", "assault"));
            Armor.Add(new DotaItems("恐鳌之心", "heart"));

            Artifacts.Add(new DotaItems("支配头盔", "helm_of_the_dominator"));
            Artifacts.Add(new DotaItems("疯狂面具", "mask_of_madness"));
            Artifacts.Add(new DotaItems("魔龙枪", "dragon_lance"));
            Artifacts.Add(new DotaItems("散华", "sange"));
            Artifacts.Add(new DotaItems("夜叉", "yasha"));
            Artifacts.Add(new DotaItems("慧光", "kaya"));
            Artifacts.Add(new DotaItems("回音战刃", "echo_sabre"));
            Artifacts.Add(new DotaItems("漩涡", "maelstrom"));
            Artifacts.Add(new DotaItems("净魂之刃", "diffusal_blade"));
            Artifacts.Add(new DotaItems("天堂之戟", "heavens_halberd"));
            Artifacts.Add(new DotaItems("黯灭", "desolator"));
            Artifacts.Add(new DotaItems("散慧对剑", "sange_and_kaya"));
            Artifacts.Add(new DotaItems("散夜对剑", "sange_and_yasha"));
            Artifacts.Add(new DotaItems("慧夜对剑", "kaya_and_yasha"));
            Artifacts.Add(new DotaItems("雷神之锤", "mjollnir"));
            Artifacts.Add(new DotaItems("斯嘉蒂之眼", "skadi"));
            Artifacts.Add(new DotaItems("撒旦之邪力", "satanic"));

            Secret.Add(new DotaItems("恶魔刀锋", "demon_edge"));
            Secret.Add(new DotaItems("鹰歌弓", "eagle"));
            Secret.Add(new DotaItems("掠夺者之斧", "reaver"));
            Secret.Add(new DotaItems("圣者遗物", "relic"));
            Secret.Add(new DotaItems("板甲", "platemail"));
            Secret.Add(new DotaItems("闪避护符", "talisman_of_evasion"));
            Secret.Add(new DotaItems("振奋宝石", "hyperstone"));
            Secret.Add(new DotaItems("极限法球", "ultimate_orb"));
            Secret.Add(new DotaItems("神秘法杖", "mystic_staff"));
            Secret.Add(new DotaItems("能量之球", "energy_booster"));
            Secret.Add(new DotaItems("精气之球", "point_booster"));
            Secret.Add(new DotaItems("活力之球", "vitality_booster"));

            ItemProgressRing.IsActive = false;
            ItemProgressRing.Visibility = Visibility.Collapsed;
            ProgressStackPanel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击列表显示信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region
        private void ConsumablesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConsumablesListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Consumables[ConsumablesListView.SelectedIndex];
            GetItemInfo();
            ConsumablesListView.SelectedIndex = -1;
        }

        private void AttributesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AttributesListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Attributes[AttributesListView.SelectedIndex];
            GetItemInfo();
            AttributesListView.SelectedIndex = -1;
        }

        private void ArmamentsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArmamentsListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Armaments[ArmamentsListView.SelectedIndex];
            GetItemInfo();
            ArmamentsListView.SelectedIndex = -1;
        }

        private void ArcaneListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArcaneListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Arcane[ArcaneListView.SelectedIndex];
            GetItemInfo();
            ArcaneListView.SelectedIndex = -1;
        }

        private void CommonListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommonListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Common[CommonListView.SelectedIndex];
            GetItemInfo();
            CommonListView.SelectedIndex = -1;
        }

        private void SupportListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SupportListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Support[SupportListView.SelectedIndex];
            GetItemInfo();
            SupportListView.SelectedIndex = -1;
        }

        private void CasterListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CasterListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Caster[CasterListView.SelectedIndex];
            GetItemInfo();
            CasterListView.SelectedIndex = -1;
        }

        private void WeaponsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WeaponsListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Weapons[WeaponsListView.SelectedIndex];
            GetItemInfo();
            WeaponsListView.SelectedIndex = -1;
        }

        private void ArmorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArmorListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Armor[ArmorListView.SelectedIndex];
            GetItemInfo();
            ArmorListView.SelectedIndex = -1;
        }

        private void ArtifactsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ArtifactsListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Artifacts[ArtifactsListView.SelectedIndex];
            GetItemInfo();
            ArtifactsListView.SelectedIndex = -1;
        }

        private void SecretListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecretListView.SelectedIndex == -1)
            {
                return;
            }
            SelectedItem = Secret[SecretListView.SelectedIndex];
            GetItemInfo();
            SecretListView.SelectedIndex = -1;
        }

        #endregion

        /// <summary>
        /// 获取物品信息
        /// </summary>
        public void GetItemInfo()
        {
            ItemPicture.Visibility = Visibility.Collapsed;
            string regex = ("\"" + SelectedItem.ID + "\":{\"id\":\"[\\d]*?\",\"img\":\"[\\s\\S]*?\",\"dname\":\"([\\s\\S]*?)\",\"qual\":[\\s\\S]*?,\"cost\":([\\s\\S]*?),\"desc\":\"([\\s\\S]*?)\",\"notes\":\"([\\s\\S]*?)\",\"attrib\":\"([\\s\\S]*?)\",\"mc\":([\\s\\S]*?),\"cd\":([\\s\\S]*?),\"lore\":\"([\\s\\S]*?)\",\"components\":[\\s\\S]*?,\"created\":[\\s\\S]*?}");
            Match match = Regex.Match(json, regex);

            string regexComponents = ("\"" + SelectedItem.ID + "\":{\"id\":[\\s\\S]*?,\"components\":([\\s\\S]*?),\"created\":[\\s\\S]*?}");
            Match matchComponents = Regex.Match(jsonComponents, regexComponents);

            SelectedItem.Name = UnicodeToString(match.Groups[1].Value);
            SelectedItem.Price = UnicodeToString(match.Groups[2].Value).Replace("\"", "");
            SelectedItem.Info = UnicodeToString(match.Groups[3].Value.Replace(" \\/", "").Replace("<br>", "\r\n").Replace("<br />", "").Replace("<\\/h1>", "").Replace("<h1>", "").Replace("<span class=\\\"attribVal\\\">", "").Replace("<\\/span>", "").Replace("<span class=\\\"attribValText\\\">", "")
                .Replace("\\/", "/").Replace("\t", "\r\n").Replace("\\\\", "\\").Replace("\\r", "").Replace("\\n", "").Replace("<span class=\\\"GameplayValues GameplayVariable\\\">", "").Replace("<font color='#e03e2e'>", "\r\n").Replace("</font>", ""));
            SelectedItem.Tips = UnicodeToString(match.Groups[4].Value.Replace(" \\/", "").Replace("\\\\", "\\").Replace("<br>", "\r\n").Replace("\\r", "").Replace("\\n", ""));
            SelectedItem.Attributes = UnicodeToString(match.Groups[5].Value.Replace("\\/", "/").Replace("<span class=\\\"attribVal\\\">", "").Replace("</span>", "").Replace("<\\/span>", "").Replace("<span class=\\\"attribValText\\\">", "")
                .Replace("\\\\", "\\").Replace("<br>", "\r\n").Replace("< br>", "\r\n").Replace("\\r", "").Replace("\\n", "").Replace("<br />", "\r\n").Replace("<p class=\\\"pop_skill_p\\\">", "").Replace("<span class=\\\"color_yellow\\\">", "").Replace("</p>", "").Replace("+", "\r\n+"));
            SelectedItem.Mana = UnicodeToString(match.Groups[6].Value.Replace("false", "0").Replace("\"\"", "0").Replace("\"", ""));
            SelectedItem.CoolDown = UnicodeToString(match.Groups[7].Value.Replace("false", "0").Replace("\"\"", "0").Replace("\"", ""));
            SelectedItem.Background = UnicodeToString(match.Groups[8].Value).Replace("\\n", "\n");
            SelectedItem.Components = matchComponents.Groups[1].Value.Replace("[", "").Replace("]", "").Replace("\"", "").Replace("\\", "").Split(',');
            ItemFrame.Navigate(typeof(ItemInfoPage));
        }

        /// <summary>
        /// Unicode转换汉字
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string UnicodeToString(string text)
        {
            System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(text, "\\\\u([\\w]{4})");
            if (mc != null && mc.Count > 0)
            {
                foreach (System.Text.RegularExpressions.Match m2 in mc)
                {
                    string v = m2.Value;
                    string word = v.Substring(2);
                    byte[] codes = new byte[2];
                    int code = Convert.ToInt32(word.Substring(0, 2), 16);
                    int code2 = Convert.ToInt32(word.Substring(2), 16);
                    codes[0] = (byte)code2;
                    codes[1] = (byte)code;
                    text = text.Replace(v, System.Text.Encoding.Unicode.GetString(codes));
                }
            }
            return text;
        }
    }
}

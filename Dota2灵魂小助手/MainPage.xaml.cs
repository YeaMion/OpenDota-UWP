using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Background;
using System.Net.Http;
using Newtonsoft.Json;
using Windows.Storage;
//using BackgroundTasks;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Dota2灵魂小助手
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public static ApplicationDataContainer SaveContainer = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();

            //初次启动显示更新日志
            if (SaveContainer.Values["first"] == null || SaveContainer.Values["first"].ToString() == "yes")
            {
                AboutGrid.Visibility = Visibility.Visible;
                SettingGridPopIn.Begin();
                AboutPivot.SelectedIndex = 1;
                SaveContainer.Values["first"] = "no";
            }

            //自动切换主题
            if (SaveContainer.Values["theme"] == null || SaveContainer.Values["theme"].ToString() == "dark")
            {
                this.RequestedTheme = ElementTheme.Dark;
                ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
                DarkRadioButton.IsChecked = true;
            }
            else
            {
                this.RequestedTheme = ElementTheme.Light;
                ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
                LightRadioButton.IsChecked = true;
            }

            //设置标题栏样式
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = new Color() { A = 0, R = 39, G = 40, B = 57 };
            titleBar.ButtonInactiveBackgroundColor = new Color() { A = 0, R = 39, G = 40, B = 57 };
            Window.Current.SetTitleBar(RealTitleGrid);

            SetTile(DotaMatchHelper.GetSteamID());

            Current = this;


            if (NetworkCheckHelper.CheckNetwork() == false)
            {
                //断网
                MainFrame.Navigate(typeof(NoNetworkPage));
            }
            else
            {
                MainFrame.Navigate(typeof(HeroesPage), 1);
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter property is typically used to configure the page.</param>
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    SharedData.Share.PlayerID = DotaMatchHelper.GetSteamID();

        //    this.RegisterBackgroundTask();
        //}

        //private async void RegisterBackgroundTask()
        //{
        //    var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
        //    if (backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed || backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
        //    {
        //        foreach (var task in BackgroundTaskRegistration.AllTasks)
        //        {
        //            if (task.Value.Name == taskName)
        //            {
        //                task.Value.Unregister(true);
        //            }
        //        }

        //        BackgroundTaskBuilder taskBuilder = new BackgroundTaskBuilder
        //        {
        //            Name = taskName,
        //            TaskEntryPoint = taskEntryPoint,
        //            IsNetworkRequested = true
        //        };
        //        taskBuilder.SetTrigger(new TimeTrigger(60, false));
        //        var registration = taskBuilder.Register();
        //    }
        //}

        //private const string taskName = "MatchBackgroundTask";
        //private const string taskEntryPoint = "BackgroundTasks.MatchBackgroundTask";

        /// <summary>
        /// 启动应用时自动更新磁贴内容
        /// </summary>
        /// <param name="id"></param>
        public async void SetTile(string id)
        {
            if (id != "")
            {
                return;
            }
            string url = String.Format("https://api.opendota.com/api/players/{0}", id);
            //http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}
            HttpClient http = new HttpClient();
            PlayerProfile player;
            try
            {
                var response = await http.GetAsync(new Uri(url));
                var jsonMessage = await response.Content.ReadAsStringAsync();

                if (jsonMessage == "{\"error\":\"rate limit exceeded\"}" || jsonMessage == "{\"error\": \"Internal Server Error\"}")
                {
                    return;
                }

                player = JsonConvert.DeserializeObject<PlayerProfile>(jsonMessage);
            }
            catch
            {
                return;
            }

            string playerRankTier = "分段：待定";
            if (player.rank_tier != null)
            {
                switch (player.rank_tier[0])
                {
                    case '0':
                        break;
                    case '1':
                        playerRankTier = "先锋" + player.rank_tier[1];
                        break;
                    case '2':
                        playerRankTier = "卫士" + player.rank_tier[1];
                        break;
                    case '3':
                        playerRankTier = "中军" + player.rank_tier[1];
                        break;
                    case '4':
                        playerRankTier = "统帅" + player.rank_tier[1];
                        break;
                    case '5':
                        playerRankTier = "传奇" + player.rank_tier[1];
                        break;
                    case '6':
                        playerRankTier = "万古流芳" + player.rank_tier[1];
                        break;
                    case '7':
                        playerRankTier = "超凡入圣" + player.rank_tier[1];
                        break;
                    case '8':
                        playerRankTier = "冠绝一世";
                        break;
                    default:
                        playerRankTier = "分段：待定";
                        break;
                }
            }

            try
            {
                TileContent content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = player.profile.avatarfull
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text =player.profile.personaname,
                                        HintStyle=AdaptiveTextStyle.Body
                                    },
                                    new AdaptiveText()
                                    {
                                        Text ="ID："+player.profile.account_id,
                                        HintStyle=AdaptiveTextStyle.Caption
                                    },
                                    new AdaptiveText()
                                    {
                                        Text=playerRankTier+"  "+player.profile.loccountrycode,
                                        HintStyle =AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },

                        TileWide = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = player.profile.avatarfull
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text =player.profile.personaname,
                                        HintStyle=AdaptiveTextStyle.Subtitle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text ="ID："+ player.profile.account_id,
                                        HintStyle = AdaptiveTextStyle.BodySubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text=playerRankTier+"  "+player.profile.loccountrycode,
                                        HintStyle=AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        },

                        TileLarge = new TileBinding()
                        {
                            Branding = TileBranding.Name,
                            Content = new TileBindingContentAdaptive()
                            {
                                PeekImage = new TilePeekImage()
                                {
                                    Source = player.profile.avatarfull
                                },
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text =player.profile.personaname,
                                        HintStyle=AdaptiveTextStyle.Subtitle,
                                        HintWrap=true
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = "ID："+player.profile.account_id,
                                        HintStyle = AdaptiveTextStyle.Base
                                    },
                                    new AdaptiveText()
                                    {
                                        Text=playerRankTier + "  " + player.profile.loccountrycode,
                                        HintStyle=AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text="MMR评估："+player.mmr_estimate.estimate,
                                        HintStyle=AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                };
                var notification = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
            catch { return; }
        }

        /// <summary>
        /// 英雄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            HeroRectangle.Width = 3;
            ItemRectangle.Width = 0;
            MatchRectangle.Width = 0;

            if (NetworkCheckHelper.CheckNetwork() == false)
            {
                //断网
                MainFrame.Navigate(typeof(NoNetworkPage));
            }
            else
            {
                MainFrame.Navigate(typeof(HeroesPage), 1);
            }
        }

        /// <summary>
        /// 物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            HeroRectangle.Width = 0;
            ItemRectangle.Width = 3;
            MatchRectangle.Width = 0;

            if (NetworkCheckHelper.CheckNetwork() == false)
            {
                //断网
                MainFrame.Navigate(typeof(NoNetworkPage));
            }
            else
            {
                MainFrame.Navigate(typeof(ItemsPage));
            }
        }

        /// <summary>
        /// 比赛
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            HeroRectangle.Width = 0;
            ItemRectangle.Width = 0;
            MatchRectangle.Width = 3;

            if (NetworkCheckHelper.CheckNetwork() == false)
            {
                //断网
                MainFrame.Navigate(typeof(NoNetworkPage));
            }
            else
            {
                MainFrame.Navigate(typeof(MatchesPage));
            }
        }

        /// <summary>
        /// 关于此应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click_3(object sender, RoutedEventArgs e)
        {
            AboutGrid.Visibility = Visibility.Visible;
            SettingGridPopIn.Begin();
            AboutPivot.SelectedIndex = 0;
        }

        private void AppBarButton_Click_4(object sender, RoutedEventArgs e)
        {
            SettingGridPopOut.Begin();
        }

        private void SettingGridPopOut_Completed(object sender, object e)
        {
            AboutGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击空白也可以关闭设置窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SettingGridPopOut.Begin();
        }

        /// <summary>
        /// 切换夜间主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            this.RequestedTheme = ElementTheme.Dark;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            SaveContainer.Values["theme"] = "dark";
        }

        /// <summary>
        /// 切换白天主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            this.RequestedTheme = ElementTheme.Light;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
            SaveContainer.Values["theme"] = "light";
        }

        /// <summary>
        /// 访问 GitHub
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/UEMion/OpenDota-UWP"));
        }

        /// <summary>
        /// 打分评价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NSKQN4V8X94"));
        }

        /// <summary>
        /// 访问 Steam 个人页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://steamcommunity.com/profiles/76561198194624815/"));
        }
    }
}

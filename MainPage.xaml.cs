using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Windows.Foundation;

using UnityApp = UnityPlayer.UnityApp;
using UnityBridge = WinRTBridge.WinRTBridge;
using GoogleAds;
using System.Diagnostics;

namespace EyesTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        private bool _unityStartedLoading;
        private static InterstitialAd interstitialAd;
        private static bool isBillboardOK = false;
        // Constructor
        public MainPage()
        {
            var bridge = new UnityBridge();
            UnityApp.SetBridge(bridge);
            InitializeComponent();
            bridge.Control = DrawingSurfaceBackground;
        }

        private void DrawingSurfaceBackground_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_unityStartedLoading)
            {
                _unityStartedLoading = true;

                UnityApp.SetLoadedCallback(() => { Dispatcher.BeginInvoke(Unity_Loaded); });

                var content = Application.Current.Host.Content;
                var nativeWidth = (int)Math.Floor(content.ActualWidth * content.ScaleFactor / 100.0 + 0.5);
                var nativeHeight = (int)Math.Floor(content.ActualHeight * content.ScaleFactor / 100.0 + 0.5);

                var physicalWidth = nativeWidth;
                var physicalHeight = nativeHeight;
                object physicalResolution;

                if (DeviceExtendedProperties.TryGetValue("PhysicalScreenResolution", out physicalResolution))
                {
                    var resolution = (System.Windows.Size)physicalResolution;
                    var nativeScale = content.ActualHeight / content.ActualWidth;
                    var physicalScale = resolution.Height / resolution.Width;
                    // don't use physical resolution for devices that don't have hardware buttons (e.g. Lumia 630)
                    if (Math.Abs(nativeScale - physicalScale) < 0.01)
                    {
                        physicalWidth = (int)resolution.Width;
                        physicalHeight = (int)resolution.Height;
                    }
                }

                UnityApp.SetNativeResolution(nativeWidth, nativeHeight);
                UnityApp.SetRenderResolution(physicalWidth, physicalHeight);
                UnityApp.SetOrientation((int)Orientation);

                DrawingSurfaceBackground.SetBackgroundContentProvider(UnityApp.GetBackgroundContentProvider());
                DrawingSurfaceBackground.SetBackgroundManipulationHandler(UnityApp.GetManipulationHandler());
            }
        }

        private void Unity_Loaded()
        {
            Debug.WriteLine("Unity_Loaded");
            LoadBillboard();
            AdsManager.AdsAction += AdsManager_LetsShow;
        }





        void LoadBillboard()
        {
            interstitialAd = new InterstitialAd("ca-app-pub-7103992668654583/8183469755");
            interstitialAd.ReceivedAd += interstitialAd_ReceivedAd;
            interstitialAd.FailedToReceiveAd += interstitialAd_FailedToReceiveAd;
            AdRequest adRequest = new AdRequest();
            interstitialAd.LoadAd(adRequest);
        }
        public void interstitialAd_FailedToReceiveAd(object sender, AdErrorEventArgs e)
        {
            isBillboardOK = false;
            Debug.WriteLine("interstitialAd_FailedToReceiveAd Lỗi : " + e.ErrorCode);
        }
        public void interstitialAd_ReceivedAd(object sender, AdEventArgs e)
        {
            isBillboardOK = true;
            Debug.WriteLine("interstitialAd_ReceivedAd");
        }
        private void OnAdReceived(object sender, GoogleAds.AdEventArgs e) { Debug.WriteLine("Banner: OnAdReceived"); }
        private void OnFailedToReceiveAd(object sender, GoogleAds.AdErrorEventArgs e) { Debug.WriteLine("Banner: OnFailedToReceiveAd"); }
        public static int count = 0;
        public void AdsManager_LetsShow(AdsOption option)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (option)
                {
                    case AdsOption.BannerHide:
                        Debug.WriteLine("BannerHide");
                        banner.Visibility = System.Windows.Visibility.Collapsed;
                        break;
                    case AdsOption.BannerShow:
                        Debug.WriteLine("BannerShow");
                        AdRequest adRequest = new AdRequest();
                        banner.LoadAd(adRequest);
                        banner.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case AdsOption.ShowBillboard:
                        count++;
                        Debug.WriteLine("Count: " + count);
                        if (count % 5 == 0)
                        {
                            if (isBillboardOK)
                            {
                                Debug.WriteLine("ShowBillboard");
                                interstitialAd.ShowAd();
                            }
                            LoadBillboard();
                        }
                        break;
                    case AdsOption.RefreshBanner:
                        Debug.WriteLine("RefreshBanner");
                        adRequest = new AdRequest();
                        banner.LoadAd(adRequest);
                        banner.Visibility = System.Windows.Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
          );

        }

        private void PhoneApplicationPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = UnityApp.BackButtonPressed();
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            UnityApp.SetOrientation((int)e.Orientation);
        }
    }
}

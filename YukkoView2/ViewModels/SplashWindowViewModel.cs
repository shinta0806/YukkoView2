// ============================================================================
// 
// スプラッシュウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 本クラスは Yv2Model 未生成時に生成されるため、基底クラスに LogWriter を渡さない
// ----------------------------------------------------------------------------

using Livet.Messaging;

using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

using Shinta;
using Shinta.ViewModels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels
{
	internal class SplashWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public override void Initialize()
		{
			base.Initialize();

			try
			{
				// マテリアルデザインの外観を変更
				IEnumerable<Swatch> swatches = new SwatchesProvider().Swatches;
				PaletteHelper paletteHelper = new();
				ITheme theme = paletteHelper.GetTheme();
				Swatch? orangeSwatch = swatches.FirstOrDefault(x => x.Name == "orange");
				if (orangeSwatch != null)
				{
					theme.SetPrimaryColor(orangeSwatch.ExemplarHue.Color);
				}
				Swatch? limeSwatch = swatches.FirstOrDefault(x => x.Name == "yellow");
				if (limeSwatch != null)
				{
					theme.SetSecondaryColor(limeSwatch.ExemplarHue.Color);
				}
				paletteHelper.SetTheme(theme);

				// テンポラリフォルダー準備
				//Common.InitializeTempFolder();

				// 環境
				Yv2Model.Instance.EnvModel.Yv2Settings.Load();
				Yv2Model.Instance.EnvModel.Yv2Settings.SetLogWriter(Yv2Model.Instance.EnvModel.LogWriter);

				// エンコード
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

				// メインウィンドウ表示
				_mainWindowViewModel = new MainWindowViewModel(this);
				if (Yv2Model.Instance.EnvModel.Yv2Settings.DesktopBounds.Width == 0.0 || Yv2Model.Instance.EnvModel.Yv2Settings.DesktopBounds.Height == 0.0)
				{
					// デフォルトウィンドウサイズ
				}
				else
				{
					// 前回のウィンドウサイズ
					Rect adjustedRect = CommonWindows.AdjustWindowRect(Yv2Model.Instance.EnvModel.Yv2Settings.DesktopBounds);
					_mainWindowViewModel.Left = adjustedRect.Left;
					_mainWindowViewModel.Top = adjustedRect.Top;
				}
				Messenger.Raise(new TransitionMessage(_mainWindowViewModel, Yv2Constants.MESSAGE_KEY_OPEN_MAIN_WINDOW));
			}
			catch (Exception ex)
			{
				// YlModel 未生成の可能性があるためまずはメッセージ表示のみ
				MessageBox.Show("スプラッシュウィンドウ初期化時エラー：\n" + ex.Message + "\n" + ex.StackTrace, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);

				// 可能であればログする。_logWriter は常に null なので Yv2Model を使用する
				// YlModel 生成中に例外が発生する可能性があるが、それについては集約エラーハンドラーに任せる
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "スプラッシュウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);

				// 継続できないのでアプリを終了する
				Environment.Exit(1);
			}
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// メインウィンドウ（アプリ終了時まで保持する必要がある）
		private MainWindowViewModel? _mainWindowViewModel;
	}
}

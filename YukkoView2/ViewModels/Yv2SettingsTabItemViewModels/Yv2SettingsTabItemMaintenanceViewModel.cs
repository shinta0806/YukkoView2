// ============================================================================
// 
// メンテナンスタブアイテムの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet.Commands;

using Shinta;
using Shinta.ViewModels;

using System;
using System.Diagnostics;
using System.Windows;

using YukkoView2.Models.Settings;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.MiscWindowViewModels;

namespace YukkoView2.ViewModels.Yv2SettingsTabItemViewModels
{
	internal class Yv2SettingsTabItemMaintenanceViewModel : TabItemViewModel<Yv2Settings>
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemMaintenanceViewModel(Yv2SettingsWindowViewModel yv2SettingsWindowViewModel)
				: base(yv2SettingsWindowViewModel, Yv2Model.Instance.EnvModel.LogWriter)
		{
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemMaintenanceViewModel()
				: base()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ゆっこビュー 2 の最新情報を自動的に確認する
		private Boolean _checkRss;
		public Boolean CheckRss
		{
			get => _checkRss;
			set
			{
				if (_checkRss && !value
						&& MessageBox.Show("最新情報の確認を無効にすると、" + Yv2Constants.APP_NAME_J
						+ "の新版がリリースされた際の更新内容などが表示されません。\n\n"
						+ "本当に無効にしてもよろしいですか？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning)
						!= MessageBoxResult.Yes)
				{
					return;
				}

				RaisePropertyChangedIfSet(ref _checkRss, value);
			}
		}

		// プログレスバー表示
		private Visibility _progressBarCheckRssVisibility;
		public Visibility ProgressBarCheckRssVisibility
		{
			get => _progressBarCheckRssVisibility;
			set
			{
				if (RaisePropertyChangedIfSet(ref _progressBarCheckRssVisibility, value))
				{
					ButtonCheckRssClickedCommand.RaiseCanExecuteChanged();
				}
			}
		}

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 今すぐ最新情報を確認するボタンの制御
		private ViewModelCommand? _buttonCheckRssClickedCommand;

		public ViewModelCommand ButtonCheckRssClickedCommand
		{
			get
			{
				if (_buttonCheckRssClickedCommand == null)
				{
					_buttonCheckRssClickedCommand = new ViewModelCommand(ButtonCheckRssClicked, CanButtonCheckRssClicked);
				}
				return _buttonCheckRssClickedCommand;
			}
		}

		public Boolean CanButtonCheckRssClicked()
		{
			return ProgressBarCheckRssVisibility != Visibility.Visible;
		}

		public async void ButtonCheckRssClicked()
		{
			try
			{
				ProgressBarCheckRssVisibility = Visibility.Visible;
				await Yv2Common.CheckLatestInfoAsync(true);
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "最新情報確認時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
			finally
			{
				ProgressBarCheckRssVisibility = Visibility.Hidden;
			}
		}
		#endregion

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
				// プログレスバー
				ProgressBarCheckRssVisibility = Visibility.Hidden;
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "メンテナンスタブアイテム初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		public override void PropertiesToSettings(Yv2Settings destSettings)
		{
			destSettings.CheckRss = CheckRss;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		public override void SettingsToProperties(Yv2Settings srcSettings)
		{
			CheckRss = srcSettings.CheckRss;
		}
	}
}

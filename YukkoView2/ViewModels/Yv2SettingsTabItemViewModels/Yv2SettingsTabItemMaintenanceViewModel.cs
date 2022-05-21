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
using System.IO;
using System.IO.Compression;
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

		#region 設定のバックアップボタンの制御
		private ViewModelCommand? _buttonBackupClickedCommand;

		public ViewModelCommand ButtonBackupClickedCommand
		{
			get
			{
				if (_buttonBackupClickedCommand == null)
				{
					_buttonBackupClickedCommand = new ViewModelCommand(ButtonBackupClicked);
				}
				return _buttonBackupClickedCommand;
			}
		}

		public void ButtonBackupClicked()
		{
			try
			{
				String? path = _tabControlWindowViewModel.PathBySavingDialog("設定のバックアップ", Yv2Constants.DIALOG_FILTER_SETTINGS_ARCHIVE, Yv2Constants.APP_ID + "Settings_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss"));
				if (path == null)
				{
					return;
				}

				Yv2Common.LogEnvironmentInfo();
				ZipFile.CreateFromDirectory(Common.UserAppDataFolderPath(), path, CompressionLevel.Optimal, true);
				_logWriter?.ShowLogMessage(TraceEventType.Information, "設定のバックアップが完了しました。");
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "設定のバックアップボタンクリック時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
		#endregion

		#region 設定の復元ボタンの制御

		private ViewModelCommand? _buttonRestoreClickedCommand;

		public ViewModelCommand ButtonRestoreClickedCommand
		{
			get
			{
				if (_buttonRestoreClickedCommand == null)
				{
					_buttonRestoreClickedCommand = new ViewModelCommand(ButtonRestoreClicked);
				}
				return _buttonRestoreClickedCommand;
			}
		}

		public void ButtonRestoreClicked()
		{
			try
			{
				String? path = _tabControlWindowViewModel.PathByOpeningDialog("設定の復元", Yv2Constants.DIALOG_FILTER_SETTINGS_ARCHIVE, null);
				if (path == null)
				{
					return;
				}

				if (MessageBox.Show("現在の設定は破棄され、" + Path.GetFileName(path) + "\nの設定に変更されます。\nよろしいですか？", "確認", MessageBoxButton.YesNo,
						MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
				{
					return;
				}

				// 解凍
				String unzipFolder = Common.TempPath() + "\\";
				Directory.CreateDirectory(unzipFolder);
				ZipFile.ExtractToDirectory(path, unzipFolder);

				// 設定更新
				String settingsFilePath = unzipFolder + Path.GetFileName(Path.GetDirectoryName(Common.UserAppDataFolderPath())) + "\\"
						+ Path.GetFileName(Yv2Model.Instance.EnvModel.Yv2Settings.SettingsPath());
				CheckRestore(settingsFilePath);
				File.Copy(settingsFilePath, Yv2Model.Instance.EnvModel.Yv2Settings.SettingsPath(), true);
				Yv2Model.Instance.EnvModel.Yv2Settings.Load();
				_tabControlWindowViewModel.Initialize();
				_logWriter?.ShowLogMessage(TraceEventType.Information, "設定を復元しました。");
			}
			catch (Exception excep)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "設定の復元ボタンクリック時エラー：\n" + excep.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
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

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 設定ファイルが正常に読み込めるか確認
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private void CheckRestore(String settingsFilePath)
		{
			Yv2Settings tempSettings = new();
			if (!tempSettings.Load(settingsFilePath))
			{
				throw new Exception("設定ファイルを読み込めません。");
			}
		}
	}
}

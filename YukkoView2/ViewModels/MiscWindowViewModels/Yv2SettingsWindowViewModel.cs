// ============================================================================
// 
// 環境設定ウィンドウの ViewModel
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
using YukkoView2.ViewModels.Yv2SettingsTabItemViewModels;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class Yv2SettingsWindowViewModel : TabControlWindowViewModel<Yv2Settings>
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsWindowViewModel()
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
			Debug.Assert(_tabItemViewModels.Length == (Int32)Yv2SettingsTabItem.__End__, "Yv2SettingsWindowViewModel() bad tab vm nums");
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// タブアイテム：設定
		public Yv2SettingsTabItemSettingsViewModel Yv2SettingsTabItemSettingsViewModel
		{
			get => (Yv2SettingsTabItemSettingsViewModel)_tabItemViewModels[(Int32)Yv2SettingsTabItem.Settings];
		}

		// タブアイテム：コメント受信
		public Yv2SettingsTabItemReceiveViewModel Yv2SettingsTabItemReceiveViewModel
		{
			get => (Yv2SettingsTabItemReceiveViewModel)_tabItemViewModels[(Int32)Yv2SettingsTabItem.Receive];
		}

		// --------------------------------------------------------------------
		// コマンド
		// --------------------------------------------------------------------

		#region 初期化ボタンの制御
		private ViewModelCommand? _buttonDefaultClickedCommand;

		public ViewModelCommand ButtonDefaultClickedCommand
		{
			get
			{
				if (_buttonDefaultClickedCommand == null)
				{
					_buttonDefaultClickedCommand = new ViewModelCommand(ButtonDefaultClicked);
				}
				return _buttonDefaultClickedCommand;
			}
		}

		public void ButtonDefaultClicked()
		{
			try
			{
				if (MessageBox.Show("環境設定をすべて初期設定に戻します。\nよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
				{
					return;
				}

				SettingsToProperties(new Yv2Settings());
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "初期化ボタンクリック時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
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
				// タイトルバー
				Title = "環境設定";

				// 設定
				SettingsToProperties(Yv2Model.Instance.EnvModel.Yv2Settings);
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウ初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// タブアイテムの ViewModel を生成
		// --------------------------------------------------------------------
		protected override TabItemViewModel<Yv2Settings>[] CreateTabItemViewModels()
		{
			return new TabItemViewModel<Yv2Settings>[]
			{
				new Yv2SettingsTabItemSettingsViewModel(this),
				new Yv2SettingsTabItemReceiveViewModel(this),
			};
		}

		// --------------------------------------------------------------------
		// プロパティーを設定に反映
		// --------------------------------------------------------------------
		protected override void PropertiesToSettings()
		{
			base.PropertiesToSettings();

			PropertiesToSettings(Yv2Model.Instance.EnvModel.Yv2Settings);
		}

		// --------------------------------------------------------------------
		// 設定を保存
		// --------------------------------------------------------------------
		protected override void SaveSettings()
		{
			base.SaveSettings();

			Yv2Model.Instance.EnvModel.Yv2Settings.Save();
		}
	}
}

// ============================================================================
// 
// 設定タブアイテムの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using Shinta.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.Yv2SettingsTabItemViewModels
{
	internal class Yv2SettingsTabItemSettingsViewModel : TabItemViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemSettingsViewModel(TabControlWindowViewModel tabControlWindowViewModel)
				: base(tabControlWindowViewModel, Yv2Model.Instance.EnvModel.LogWriter)
		{
		}

		// --------------------------------------------------------------------
		// ダミーコンストラクター（Visual Studio・TransitionMessage 用）
		// --------------------------------------------------------------------
		public Yv2SettingsTabItemSettingsViewModel()
				: base()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// 起動と同時にコメント表示を開始する
		private Boolean _playOnStart;
		public Boolean PlayOnStart
		{
			get => _playOnStart;
			set => RaisePropertyChangedIfSet(ref _playOnStart, value);
		}

		// 上下の指定パーセントにコメントを表示しない
		private Boolean _enableMargin;
		public Boolean EnableMargin
		{
			get => _enableMargin;
			set => RaisePropertyChangedIfSet(ref _enableMargin, value);
		}

		// パーセント選択肢
		public ObservableCollection<Int32> MarginPercents { get; set; } = new();

		// パーセント
		private Int32 _marginPercent;
		public Int32 MarginPercent
		{
			get => _marginPercent;
			set => RaisePropertyChangedIfSet(ref _marginPercent, value);
		}

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
				// パーセント選択肢
				for (Int32 i = 5; i <= 15; i += 5)
				{
					MarginPercents.Add(i);
				}
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "設定タブアイテム初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// プロパティーから設定に反映
		// --------------------------------------------------------------------
		public override void PropertiesToSettings()
		{
			Yv2Model.Instance.EnvModel.Yv2Settings.PlayOnStart = PlayOnStart;
			Yv2Model.Instance.EnvModel.Yv2Settings.EnableMargin = EnableMargin;
			Yv2Model.Instance.EnvModel.Yv2Settings.MarginPercent = MarginPercent;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		public override void SettingsToProperties()
		{
			PlayOnStart = Yv2Model.Instance.EnvModel.Yv2Settings.PlayOnStart;
			EnableMargin = Yv2Model.Instance.EnvModel.Yv2Settings.EnableMargin;
			MarginPercent = Yv2Model.Instance.EnvModel.Yv2Settings.MarginPercent;
		}
	}
}

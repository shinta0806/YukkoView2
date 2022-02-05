// ============================================================================
// 
// コメント表示ディスプレイ選択ウィンドウの ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Shinta;
using Shinta.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class SelectMonitorWindowViewModel : BasicWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public SelectMonitorWindowViewModel()
				: base(Yv2Model.Instance.EnvModel.LogWriter)
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ディスプレイ選択方法：MPC-BE
		private Boolean _selectMonitorMpcBe;
		public Boolean SelectMonitorMpcBe
		{
			get => _selectMonitorMpcBe;
			set => RaisePropertyChangedIfSet(ref _selectMonitorMpcBe, value);
		}

		// ディスプレイ選択方法：手動
		private Boolean _selectMonitorManual;
		public Boolean SelectMonitorManual
		{
			get => _selectMonitorManual;
			set => RaisePropertyChangedIfSet(ref _selectMonitorManual, value);
		}

		// ディスプレイ選択方法：手動
		// ディスプレイ番号群
		public ObservableCollection<String> MonitorIndices { get; set; } = new();

		// ディスプレイ選択方法：手動
		// 選択されたディスプレイ番号
		private Int32 _selectedMonitorIndex;
		public Int32 SelectedMonitorIndex
		{
			get => _selectedMonitorIndex;
			set => RaisePropertyChangedIfSet(ref _selectedMonitorIndex, value);
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
				Title = "コメント表示ディスプレイ選択";
				SettingsToProperties();
			}
			catch (Exception ex)
			{
				_logWriter?.ShowLogMessage(TraceEventType.Error, "コメント表示ディスプレイ選択ウィンドウ初期化時エラー：\n" + ex.Message);
				_logWriter?.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// プロパティーを設定に反映
		// --------------------------------------------------------------------
		protected override void PropertiesToSettings()
		{
			Yv2Model.Instance.EnvModel.Yv2Settings.SelectMonitorType = SelectMonitorMpcBe ? SelectMonitorType.MpcBe : SelectMonitorType.Manual;
			Yv2Model.Instance.EnvModel.Yv2Settings.MonitorIndex = SelectedMonitorIndex;
		}

		// --------------------------------------------------------------------
		// 設定をプロパティーに反映
		// --------------------------------------------------------------------
		protected override void SettingsToProperties()
		{
			switch (Yv2Model.Instance.EnvModel.Yv2Settings.SelectMonitorType)
			{
				case SelectMonitorType.MpcBe:
					SelectMonitorMpcBe = true;
					break;
				case SelectMonitorType.Manual:
				default:
					SelectMonitorManual = true;
					break;
			}

			Yv2Model.Instance.EnvModel.MonitorRects = CommonWindows.GetMonitorRects();
#if DEBUG
			Yv2Model.Instance.EnvModel.MonitorRects.Add(new System.Windows.Rect(0, 0, 1, 1));
			Yv2Model.Instance.EnvModel.MonitorRects.Add(new System.Windows.Rect(0, 0, 1, 1));
#endif
			MonitorIndices.Clear();
			MonitorIndices.Add("1（プライマリー）");
			for (Int32 i = 1; i < Yv2Model.Instance.EnvModel.MonitorRects.Count; i++)
			{
				MonitorIndices.Add((i + 1).ToString());
			}
			SelectedMonitorIndex = Yv2Model.Instance.EnvModel.Yv2Settings.MonitorIndex;
		}

		// --------------------------------------------------------------------
		// 設定を保存
		// --------------------------------------------------------------------
		protected override void SaveSettings()
		{
			Yv2Model.Instance.EnvModel.Yv2Settings.Save();
		}
	}
}

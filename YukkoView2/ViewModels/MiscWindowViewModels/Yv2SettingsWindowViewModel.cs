﻿// ============================================================================
// 
// 環境設定ウィンドウの ViewModel
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.Yv2SettingsTabItemViewModels;

namespace YukkoView2.ViewModels.MiscWindowViewModels
{
	internal class Yv2SettingsWindowViewModel : TabControlWindowViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2SettingsWindowViewModel()
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
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "環境設定ウィンドウ初期化時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// --------------------------------------------------------------------
		// タブアイテムの ViewModel を生成
		// --------------------------------------------------------------------
		protected override TabItemViewModel[] CreateTabItemViewModels()
		{
			return new TabItemViewModel[]
			{
				new Yv2SettingsTabItemSettingsViewModel(this),
				new Yv2SettingsTabItemReceiveViewModel(this),
			};
		}
	}
}

// ============================================================================
// 
// ゆっこビュー 2 の基底用 ViewModel
// 
// ============================================================================

// ----------------------------------------------------------------------------
// スプラッシュウィンドウ ViewModel 以外のすべてのウィンドウの ViewModel に適用する
// ----------------------------------------------------------------------------

using Livet;

using Shinta;

using System;
using System.Windows;
using System.Windows.Input;
using YukkoView2.Models.YukkoView2Models;


namespace YukkoView2.ViewModels
{
	internal class Yv2ViewModel : ViewModel
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Yv2ViewModel()
		{
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// --------------------------------------------------------------------
		// View 通信用のプロパティー
		// --------------------------------------------------------------------

		// ウィンドウタイトル（デフォルトが null だと実行時にエラーが発生するので Empty にしておく）
		private String _title = String.Empty;
		public String Title
		{
			get => _title;
			set
			{
				String title = value;
#if DEBUG
				title = "［デバッグ］" + title;
#endif
#if TEST
				title = "［テスト］" + title;
#endif
				RaisePropertyChangedIfSet(ref _title, title);
			}
		}

		// カーソル
		private Cursor? _cursor;
		public Cursor? Cursor
		{
			get => _cursor;
			set => RaisePropertyChangedIfSet(ref _cursor, value);
		}

		// --------------------------------------------------------------------
		// 一般のプロパティー
		// --------------------------------------------------------------------

		// OK、Yes、No 等の結果
		public MessageBoxResult Result { get; protected set; } = MessageBoxResult.None;

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化
		// --------------------------------------------------------------------
		public virtual void Initialize()
		{
			Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, GetType().Name + " 初期化中...");
		}

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// リソース解放
		// --------------------------------------------------------------------
		protected override void Dispose(Boolean isDisposing)
		{
			base.Dispose(isDisposing);

			Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, GetType().Name + " 破棄中...");
		}
	}
}

// ============================================================================
// 
// コメントを表示するコントロール
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;

using System;

using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.Views.CustomControls
{
	public class CommentControl : Control
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// static コンストラクター
		// --------------------------------------------------------------------
		static CommentControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(CommentControl), new FrameworkPropertyMetadata(typeof(CommentControl)));
		}

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public CommentControl()
		{
			try
			{
				IsEnabledChanged += IsEnabledChangedEventHandler;

				// ピクセルぴったり描画
				SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示コントロール生成時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

#if false
		// ゆかり検索対象フォルダーの情報
		public static readonly DependencyProperty TargetFolderInfoProperty
				= DependencyProperty.Register("TargetFolderInfo", typeof(TargetFolderInfo), typeof(FolderTreeControl),
				new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SourceTargetFolderInfoPropertyChanged));
		public TargetFolderInfo? TargetFolderInfo
		{
			get => (TargetFolderInfo?)GetValue(TargetFolderInfoProperty);
			set => SetValue(TargetFolderInfoProperty, value);
		}
#endif

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 初期化プロセスが完了
		// --------------------------------------------------------------------
		public override void EndInit()
		{
			base.EndInit();

			try
			{
				_timerDraw.Interval = TimeSpan.FromMilliseconds(50);
				_timerDraw.Tick += new EventHandler((s, e) =>
				{
					InvalidateVisual();
				});

			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示コントロール初期化完了時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// protected 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 描画
		// --------------------------------------------------------------------
		protected override void OnRender(DrawingContext drawingContext)
		{
			try
			{
				// クリア
				Rect drawingRect = new(0, 0, ActualWidth, ActualHeight);
				drawingContext.DrawRectangle(Background, null, drawingRect);

				// クリッピング
				drawingContext.PushClip(new RectangleGeometry(drawingRect));

				// test
				drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, 0, ActualWidth, 20));
				drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, ActualHeight - 20, ActualWidth, 20));
				FormattedText text = new(Environment.TickCount.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, CreateDefaultTypeface(FontFamily),
						FontSize, Foreground, Yv2Constants.DPI);
				drawingContext.DrawText(text, new Point(20, 60));

				// クリッピング解除
				drawingContext.Pop();
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示コントロール描画時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
		}

		// ====================================================================
		// private 変数
		// ====================================================================

		// コメント表示用タイマー
		private readonly DispatcherTimer _timerDraw = new();

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// FontFamily の中でデフォルトの Typeface を取得
		// フォールバックした場合は指定された FontFamily とは異なることに注意
		// --------------------------------------------------------------------
		private static Typeface CreateDefaultTypeface(FontFamily fontFamily)
		{
			FamilyTypeface? familyTypeface;

			// 線の太さが標準、かつ、横幅が標準
			familyTypeface = fontFamily.FamilyTypefaces.FirstOrDefault(x => x.Weight == FontWeights.Regular && x.Stretch == FontStretches.Medium);

			if (familyTypeface == null)
			{
				// 見つからない場合は、線の太さが標準なら何でも良いとする
				familyTypeface = fontFamily.FamilyTypefaces.FirstOrDefault(x => x.Weight == FontWeights.Regular);
			}

			if (familyTypeface == null)
			{
				// 見つからない場合は、何でも良いとする
				familyTypeface = fontFamily.FamilyTypefaces.FirstOrDefault();
			}

			if (familyTypeface == null)
			{
				// それでも見つからない場合は、フォールバック
				return new Typeface(String.Empty);
			}

			// 見つかった情報で Typeface 生成
			return new Typeface(fontFamily, familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch);
		}

		// --------------------------------------------------------------------
		// IsEnabled プロパティーが更新された
		// --------------------------------------------------------------------
		private void IsEnabledChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e)
		{
			Debug.WriteLine("CommentControl.IsEnabledChangedEventHandler() " + IsEnabled);
			if (IsEnabled)
			{
				_timerDraw.Start();
			}
			else
			{
				_timerDraw.Stop();
			}
		}

	}
}

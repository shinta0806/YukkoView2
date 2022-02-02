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
using System.Windows.Media.Imaging;
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
			IsEnabledChanged += CommentControl_IsEnabledChanged;
			SizeChanged += CommentControl_SizeChanged;
			//_offScreen = CreateOffScreen();
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
				// オフスクリーン描画
				// オフスクリーンをクラスメンバーにすると、ガベージコレクトが遅延するのか、一時的に数 GB のメモリを消費してしまう
				// 仕方ないので、オフスクリーンは都度生成する
				RenderTargetBitmap offScreen = CreateOffScreen();
				Rect drawingRect = new(0, 0, offScreen.Width, offScreen.Height);
				DrawingVisual offScreenVisual = new();
				using DrawingContext offScreenContext = offScreenVisual.RenderOpen();

				// 枠
				DrawFrame(offScreenContext);

				// test
				FormattedText text = new(Environment.TickCount.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, CreateDefaultTypeface(FontFamily),
						FontSize, Foreground, Yv2Constants.DPI);
				offScreenContext.DrawText(text, new Point(20, 100));

				// 描画
				offScreenContext.Close();
				offScreen.Render(offScreenVisual);
				drawingContext.DrawImage(offScreen, drawingRect);
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

		// オフスクリーン
		//private RenderTargetBitmap _offScreen;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// IsEnabled プロパティーが更新された
		// --------------------------------------------------------------------
		private void CommentControl_IsEnabledChanged(Object sender, DependencyPropertyChangedEventArgs e)
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

		// --------------------------------------------------------------------
		// ActualHeight, ActualWidthIsEnabled プロパティーが更新された
		// --------------------------------------------------------------------
		private void CommentControl_SizeChanged(Object sender, SizeChangedEventArgs e)
		{
			//_offScreen = CreateOffScreen();
		}

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
		// オフスクリーン作成
		// --------------------------------------------------------------------
		private RenderTargetBitmap CreateOffScreen()
		{
			//Debug.WriteLine("CreateOffScreen() " + ActualWidth + ", " + ActualHeight);
			RenderTargetBitmap offScreen = new(Math.Max((Int32)ActualWidth, 1), Math.Max((Int32)ActualHeight, 1), Yv2Constants.DPI, Yv2Constants.DPI, PixelFormats.Pbgra32);

			// ピクセルぴったり描画
			offScreen.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);

			return offScreen;
		}

		// --------------------------------------------------------------------
		// 描画領域を示す枠（描画対象ディスプレイ一杯）を描画
		// --------------------------------------------------------------------
		private void DrawFrame(DrawingContext drawingContext)
		{
			Int32 borderThick = (Int32)ActualHeight / 20;
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, ActualWidth, borderThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, ActualHeight - borderThick, ActualWidth, borderThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, borderThick, ActualHeight));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(ActualWidth - borderThick, 0, borderThick, ActualHeight));
		}

	}
}

// ============================================================================
// 
// コメントを表示するコントロール
// 
// ============================================================================

// ----------------------------------------------------------------------------
// CommentInfo の描画データ管理も担当する
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Concurrent;
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
	internal class CommentControl : Control
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
			_typeFace = new(String.Empty);
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 表示中のコメント群
		public static readonly DependencyProperty CommentInfosProperty
				= DependencyProperty.Register("CommentInfos", typeof(ConcurrentBag<CommentInfo>), typeof(CommentControl),
				new FrameworkPropertyMetadata(new ConcurrentBag<CommentInfo>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SourceCommentInfosPropertyPropertyChanged));
		public ConcurrentBag<CommentInfo> CommentInfos
		{
			get => (ConcurrentBag<CommentInfo>)GetValue(CommentInfosProperty);
			set => SetValue(CommentInfosProperty, value);
		}

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
				// デフォルトタイプフェース
				_typeFace = CreateTypeface();

				// コメント表示用タイマー
				_timerDraw.Interval = TimeSpan.FromMilliseconds(50);
				_timerDraw.Tick += new EventHandler((s, e) =>
				{
					InvalidateVisual();
				});

#if DEBUGz
				ConcurrentBag<Int32> concurrentBag = new();
				for (Int32 i = 0; i < 10; i++)
				{
					concurrentBag.Add(i);
				}
				foreach(Int32 i in concurrentBag)
				{
					Debug.WriteLine("EndInit() foreach " + i);
				}
				for (Int32 i = 0; i < 10; i++)
				{
					concurrentBag.TryPeek(out Int32 result);
					Debug.WriteLine("EndInit() TryPeek " + result);
				}
#endif

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
				DrawFrameIfNeeded(offScreenContext);

				// 描画データ準備
				PrepareDrawDataIfNeeded();

				// 描画
				DrawCommentInfosIfNeeded(offScreenContext);

				// オンスクリーンへ転写
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
		// private 定数
		// ====================================================================

		// 枠の描画時間 [ms]
		private const Int32 DRAW_FRAME_DURATION = 5 * 1000;

		// 画面の高さに対するフォントサイズの比率（フォントサイズ 1 の時）
		private const Double FONT_UNIT_SCALE = 0.023;

		// コメントが画面端から端まで到達するのに要する時間 [ms]
		private const Int32 COMMENT_VIEWING_TIME = 12000;

		// ====================================================================
		// private 変数
		// ====================================================================

		// コメント表示用タイマー
		private readonly DispatcherTimer _timerDraw = new();

		// オフスクリーン
		//private RenderTargetBitmap _offScreen;

		// 枠を描画するかどうか
		private Boolean _drawFrame;

		// 枠を消去する時刻
		private Int32 _clearFrameTick;

		// フォントタイプフェース
		private Typeface _typeFace;

		// フォントサイズ "1" に対するピクセル数
		private Double _fontUnit;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント描画位置（水平）を算出
		// --------------------------------------------------------------------
		private Int32 CalcCommentLeft(CommentInfo commentInfo)
		{
			return (Int32)ActualWidth - /*DeltaLeft(commentInfo) -*/ commentInfo.Speed * (Environment.TickCount - commentInfo.InitialTick) / 1000;
		}

		// --------------------------------------------------------------------
		// 新規コメント投入時の、コメント描画位置（垂直）を算出
		// --------------------------------------------------------------------
		private Int32 CalcCommentTop(CommentInfo oNewCommentInfo)
		{
			return 0;
#if false
			// 新しいコメントを入れることのできる高さ範囲：Key, Value = 上端, 下端
			List<KeyValuePair<Int32, Int32>> aLayoutRange = new List<KeyValuePair<Int32, Int32>>();
			Int32 aMinTop = 0;
			Int32 aMaxBottom = Height;
			if (mFormControl.YukkoViewSettings.EnableMargin)
			{
				aMinTop = Height * mFormControl.YukkoViewSettings.MarginPercent / 100;
				aMaxBottom = Height * (100 - mFormControl.YukkoViewSettings.MarginPercent) / 100;
			}
			aLayoutRange.Add(new KeyValuePair<Int32, Int32>(aMinTop, aMaxBottom));

			// 既存コメントがある高さ範囲を除外していく
			lock (mCommentInfosLock)
			{
				foreach (CommentInfo aCommentInfo in mCommentInfos)
				{
					// ある程度中央に流れているコメントで、かつ、速度が同等以上なら逃げ切れるので範囲がかぶっても構わない
					if (aCommentInfo.Right + aCommentInfo.Height < Width - DeltaLeft(aCommentInfo) && aCommentInfo.Speed >= oNewCommentInfo.Speed)
					{
						continue;
					}

					for (Int32 i = aLayoutRange.Count - 1; i >= 0; i--)
					{
						if (aCommentInfo.Top <= aLayoutRange[i].Key && aCommentInfo.Bottom >= aLayoutRange[i].Value)
						{
							// aCommentInfo が aLayoutRange[i] を完全に覆っているので、aLayoutRange[i] を削除する
							aLayoutRange.RemoveAt(i);
						}
						else if (aCommentInfo.Top <= aLayoutRange[i].Key && (aLayoutRange[i].Key <= aCommentInfo.Bottom && aCommentInfo.Bottom < aLayoutRange[i].Value))
						{
							// aCommentInfo が aLayoutRange[i] の上方を覆っているので、aLayoutRange[i] の上端を下げる
							aLayoutRange[i] = new KeyValuePair<Int32, Int32>(aCommentInfo.Bottom + 1, aLayoutRange[i].Value);
						}
						else if ((aLayoutRange[i].Key < aCommentInfo.Top && aCommentInfo.Top <= aLayoutRange[i].Value) && aCommentInfo.Bottom >= aLayoutRange[i].Value)
						{
							// aCommentInfo が aLayoutRange[i] の下方を覆っているので、aLayoutRange[i] の下端を上げる
							aLayoutRange[i] = new KeyValuePair<Int32, Int32>(aLayoutRange[i].Key, aCommentInfo.Top - 1);
						}
						else if (aCommentInfo.Top > aLayoutRange[i].Key && aCommentInfo.Bottom < aLayoutRange[i].Value)
						{
							// aCommentInfo が aLayoutRange[i] の内側にあるので、aLayoutRange[i] を分割する
							KeyValuePair<Int32, Int32> aRange = aLayoutRange[i];
							aLayoutRange[i] = new KeyValuePair<Int32, Int32>(aRange.Key, aCommentInfo.Top - 1);
							aLayoutRange.Add(new KeyValuePair<Int32, Int32>(aCommentInfo.Bottom + 1, aRange.Value));
						}
						else
						{
							// aCommentInfo は覆っていない
						}
					}
				}
			}

			// 新しいコメントが入る範囲があるなら位置決め
			foreach (KeyValuePair<Int32, Int32> aRange in aLayoutRange)
			{
				if (aRange.Value - aRange.Key + 1 >= oNewCommentInfo.Height)
				{
					return aRange.Key;
				}
			}

			// 新しいコメントが入る範囲がないので弾幕モードとする
			Random aRand = new Random();
			return aRand.Next(aMaxBottom - aMinTop - oNewCommentInfo.Height) + aMinTop;
#endif
		}

		// --------------------------------------------------------------------
		// IsEnabled プロパティーが更新された
		// --------------------------------------------------------------------
		private void CommentControl_IsEnabledChanged(Object sender, DependencyPropertyChangedEventArgs e)
		{
			Debug.WriteLine("CommentControl.IsEnabledChangedEventHandler() " + IsEnabled);
			if (IsEnabled)
			{
				SetDrawFrame();
				_timerDraw.Start();
			}
			else
			{
				_timerDraw.Stop();
			}
		}

		// --------------------------------------------------------------------
		// ActualHeight, ActualWidth プロパティーが更新された
		// --------------------------------------------------------------------
		private void CommentControl_SizeChanged(Object sender, SizeChangedEventArgs e)
		{
			_fontUnit = ActualHeight * FONT_UNIT_SCALE;
			SetDrawFrame();
		}

		// --------------------------------------------------------------------
		// FontFamily から Typeface を取得
		// フォールバックした場合は指定された FontFamily とは異なることに注意
		// --------------------------------------------------------------------
		private Typeface CreateTypeface()
		{
			FamilyTypeface? familyTypeface;

			// 線の太さが太く、かつ、横幅が標準
			familyTypeface = FontFamily.FamilyTypefaces.FirstOrDefault(x => x.Weight == FontWeights.Bold && x.Stretch == FontStretches.Medium);

			if (familyTypeface == null)
			{
				// 見つからない場合は、線の太さが太ければ何でも良いとする
				familyTypeface = FontFamily.FamilyTypefaces.FirstOrDefault(x => x.Weight == FontWeights.Bold);
			}

			if (familyTypeface == null)
			{
				// 見つからない場合は、何でも良いとする
				familyTypeface = FontFamily.FamilyTypefaces.FirstOrDefault();
			}

			if (familyTypeface == null)
			{
				// それでも見つからない場合は、フォールバック
				return new Typeface(String.Empty);
			}

			// 見つかった情報で Typeface 生成
			return new Typeface(FontFamily, familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch);
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
		// コメントを描画
		// --------------------------------------------------------------------
		private void DrawCommentInfo(DrawingContext drawingContext, CommentInfo commentInfo)
		{
			drawingContext.DrawGeometry(commentInfo.Brush, commentInfo.Pen, commentInfo.MessageGeometry);
		}

		// --------------------------------------------------------------------
		// コメントを移動して描画
		// --------------------------------------------------------------------
		private void DrawCommentInfosIfNeeded(DrawingContext drawingContext)
		{
			foreach (CommentInfo commentInfo in CommentInfos)
			{
				if (commentInfo.IsDrawDataPrepared)
				{
					Int32 left = CalcCommentLeft(commentInfo);
					MoveComment(commentInfo, left, 0);
					DrawCommentInfo(drawingContext, commentInfo);
					if (left + commentInfo.Width <= 0)
					{
						// 移動が完了したので削除
						//CommentInfos.Remove
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 描画領域を示す枠（描画対象ディスプレイ一杯）を描画
		// --------------------------------------------------------------------
		private void DrawFrameIfNeeded(DrawingContext drawingContext)
		{
			if (!_drawFrame)
			{
				return;
			}
			if (Environment.TickCount >= _clearFrameTick)
			{
				// 枠を消去する時刻になった
				_drawFrame = false;
				return;
			}

			// 描画
			Int32 borderThick = (Int32)ActualHeight / 20;
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, ActualWidth, borderThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, ActualHeight - borderThick, ActualWidth, borderThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, borderThick, ActualHeight));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(ActualWidth - borderThick, 0, borderThick, ActualHeight));
		}

		// --------------------------------------------------------------------
		// コメント移動
		// --------------------------------------------------------------------
		private void MoveComment(CommentInfo commentInfo, Int32 left, Int32 top)
		{
			Debug.Assert(commentInfo.MessageGeometry != null, "MoveComment() bad MessageGeometry");
			commentInfo.MessageGeometry.Transform = new TranslateTransform(left, top);
		}

		// --------------------------------------------------------------------
		// 描画データを準備
		// --------------------------------------------------------------------
		private void PrepareDrawData(CommentInfo commentInfo)
		{
			FormattedText formattedText = new(commentInfo.Message, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
					commentInfo.YukariSize * _fontUnit, Brushes.Black, Yv2Constants.DPI);
			commentInfo.MessageGeometry = formattedText.BuildGeometry(new Point(0, 0));

			commentInfo.Brush = new SolidColorBrush(commentInfo.Color);

			commentInfo.EdgeWidth = commentInfo.YukariSize * (Int32)_fontUnit / 15;
			commentInfo.Pen = new Pen(Brushes.Black, commentInfo.EdgeWidth);

			commentInfo.Speed = ((Int32)ActualWidth + commentInfo.Width) / (COMMENT_VIEWING_TIME / 1000);

			//MoveComment(commentInfo, CalcCommentLeft(commentInfo), CalcCommentTop(commentInfo));

			commentInfo.IsDrawDataPrepared = true;
		}

		// --------------------------------------------------------------------
		// 描画データを準備
		// --------------------------------------------------------------------
		private void PrepareDrawDataIfNeeded()
		{
			foreach (CommentInfo commentInfo in CommentInfos)
			{
				if (!commentInfo.IsDrawDataPrepared)
				{
					PrepareDrawData(commentInfo);
				}
			}
		}

		// --------------------------------------------------------------------
		// 枠を描画するようにする
		// --------------------------------------------------------------------
		private void SetDrawFrame()
		{
			_drawFrame = true;
			_clearFrameTick = Environment.TickCount + DRAW_FRAME_DURATION;
		}

		// --------------------------------------------------------------------
		// ViewModel 側で DependencyProperty が変更された（TargetFolderInfoProperty）
		// --------------------------------------------------------------------
		private static void SourceCommentInfosPropertyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			Debug.WriteLine("SourceCommentInfosPropertyPropertyChanged");
		}



	}
}

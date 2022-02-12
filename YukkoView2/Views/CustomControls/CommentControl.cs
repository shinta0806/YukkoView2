// ============================================================================
// 
// コメントを表示するコントロール
// 
// ============================================================================

// ----------------------------------------------------------------------------
// CommentInfo のプロパティーのうち描画部分のプロパティーの生成も担当する
// ----------------------------------------------------------------------------

using Shinta;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using YukkoView2.Models.Database;
using YukkoView2.Models.DatabaseContexts;
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
			_requestListOffScreen = CreateOffScreen();
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 表示中のコメント群（Int32 はダミー）
		// コメントコンテナ（DisplayWindowViewModel）と情報を共有する形になる
		public static readonly DependencyProperty CommentInfosProperty
				= DependencyProperty.Register("CommentInfos", typeof(ConcurrentDictionary<CommentInfo, Int32>), typeof(CommentControl),
				new FrameworkPropertyMetadata(new ConcurrentDictionary<CommentInfo, Int32>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SourceCommentInfosPropertyChanged));
		public ConcurrentDictionary<CommentInfo, Int32> CommentInfos
		{
			get => (ConcurrentDictionary<CommentInfo, Int32>)GetValue(CommentInfosProperty);
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
		// IsEnabled == false の時は全クリア
		// --------------------------------------------------------------------
		protected override void OnRender(DrawingContext drawingContext)
		{
			try
			{
				// 描画の必要性を判定
				if (IsEnabled && !CommentInfos.Any() && !_clearScreen)
				{
					return;
				}
				_clearScreen = false;

				// オフスクリーン描画
				// オフスクリーンをクラスメンバーにすると、ガベージコレクトが遅延するのか、一時的に数 GB のメモリを消費してしまう
				// 仕方ないので、オフスクリーンは都度生成する
				RenderTargetBitmap offScreen = CreateOffScreen();
				Rect drawingRect = new(0, 0, offScreen.Width, offScreen.Height);
				if (IsEnabled)
				{
					DrawingVisual offScreenVisual = new();
					using DrawingContext offScreenContext = offScreenVisual.RenderOpen();

					// 枠
					DrawFrameIfNeeded(offScreenContext);

					// 描画データ準備
					PrepareDrawDataIfNeeded();

					// 予約一覧描画
					DrawRequestListIfNeeded(offScreenContext);

					// コメント描画
					DrawCommentInfosIfNeeded(offScreenContext);

					// オフスクリーンへ描画
					offScreenContext.Close();
					offScreen.Render(offScreenVisual);
				}

				// オンスクリーンへ転写
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

		// 予約一覧の描画時間 [ms]
		private const Int32 DRAW_REQUEST_LIST_DURATION = 10 * 1000;

		// 予約一覧のフォントサイズ
		private const Int32 REQUEST_LIST_FONT_SIZE = 4;

		// 画面の高さに対するフォントサイズの比率（フォントサイズ 1 の時）
		private const Double FONT_UNIT_SCALE = 0.023;

		// コメントが画面端から端まで到達するのに要する時間 [ms]
		private const Int32 COMMENT_VIEWING_TIME = 12000;

		// ====================================================================
		// private 変数
		// ====================================================================

		// コメント表示用タイマー
		private readonly DispatcherTimer _timerDraw = new();

		// 画面を全消去するかどうか
		private Boolean _clearScreen;

		// 枠を描画するかどうか
		private Boolean _drawFrame;

		// 枠を消去する時刻
		private Int32 _clearFrameTick;

		// フォントタイプフェース
		private Typeface _typeFace;

		// フォントサイズ "1" に対するピクセル数
		private Double _fontUnit;

		// 予約一覧を表示するかどうか
		private Boolean _drawRequestList;

		// 予約一覧を消去する時刻
		private Int32 _clearRequestListTick;

		// 予約一覧用オフスクリーン
		private RenderTargetBitmap _requestListOffScreen;

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント描画位置（水平）を算出
		// --------------------------------------------------------------------
		private void CalcCommentLeft(CommentInfo commentInfo)
		{
			commentInfo.Left = (Int32)ActualWidth - commentInfo.Speed * (Environment.TickCount - commentInfo.InitialTick) / 1000;
		}

		// --------------------------------------------------------------------
		// 新規コメント投入時の、コメント描画位置（垂直）を算出
		// --------------------------------------------------------------------
		private Int32 CalcCommentTop(CommentInfo newCommentInfo)
		{
			// 新しいコメントを入れることのできる高さ範囲：Key, Value = 上端, 下端
			List<KeyValuePair<Int32, Int32>> layoutRange = new();
			Int32 minTop = 0;
			Int32 maxBottom = (Int32)ActualHeight;
			if (Yv2Model.Instance.EnvModel.Yv2Settings.EnableMargin)
			{
				minTop = (Int32)ActualHeight * Yv2Model.Instance.EnvModel.Yv2Settings.MarginPercent / 100;
				maxBottom = (Int32)ActualHeight * (100 - Yv2Model.Instance.EnvModel.Yv2Settings.MarginPercent) / 100;
			}
			layoutRange.Add(new KeyValuePair<Int32, Int32>(minTop, maxBottom));

			// 既存コメントがある高さ範囲を除外していく
			foreach (CommentInfo commentInfo in CommentInfos.Keys)
			{
				// 未準備のものは無視
				if (!commentInfo.IsDrawDataPrepared)
				{
					continue;
				}

				// 2 文字分以上中央に流れているコメントで、かつ、速度が同等以上なら逃げ切れるので範囲がかぶっても構わない
				if (commentInfo.Right + commentInfo.Height * 2 < ActualWidth && commentInfo.Speed >= newCommentInfo.Speed)
				{
					continue;
				}

				for (Int32 i = layoutRange.Count - 1; i >= 0; i--)
				{
					if (commentInfo.Top <= layoutRange[i].Key && commentInfo.Bottom >= layoutRange[i].Value)
					{
						// commentInfo が layoutRange[i] を完全に覆っているので、layoutRange[i] を削除する
						layoutRange.RemoveAt(i);
					}
					else if (commentInfo.Top <= layoutRange[i].Key && (layoutRange[i].Key <= commentInfo.Bottom && commentInfo.Bottom < layoutRange[i].Value))
					{
						// commentInfo が layoutRange[i] の上方を覆っているので、layoutRange[i] の上端を下げる
						layoutRange[i] = new KeyValuePair<Int32, Int32>(commentInfo.Bottom + 1, layoutRange[i].Value);
					}
					else if ((layoutRange[i].Key < commentInfo.Top && commentInfo.Top <= layoutRange[i].Value) && commentInfo.Bottom >= layoutRange[i].Value)
					{
						// commentInfo が layoutRange[i] の下方を覆っているので、layoutRange[i] の下端を上げる
						layoutRange[i] = new KeyValuePair<Int32, Int32>(layoutRange[i].Key, commentInfo.Top - 1);
					}
					else if (commentInfo.Top > layoutRange[i].Key && commentInfo.Bottom < layoutRange[i].Value)
					{
						// commentInfo が layoutRange[i] の内側にあるので、layoutRange[i] を分割する
						KeyValuePair<Int32, Int32> range = layoutRange[i];
						layoutRange[i] = new KeyValuePair<Int32, Int32>(range.Key, commentInfo.Top - 1);
						layoutRange.Add(new KeyValuePair<Int32, Int32>(commentInfo.Bottom + 1, range.Value));
					}
					else
					{
						// commentInfo は覆っていない
					}
				}
			}

			// 新しいコメントが入る範囲があるなら位置決め
			foreach (KeyValuePair<Int32, Int32> range in layoutRange)
			{
				if (range.Value - range.Key + 1 >= newCommentInfo.Height)
				{
					return range.Key;
				}
			}

			// 新しいコメントが入る範囲がないので弾幕モードとする
			Random rand = new();
			return rand.Next(maxBottom - minTop - newCommentInfo.Height) + minTop;
		}

		// --------------------------------------------------------------------
		// 画面消去
		// --------------------------------------------------------------------
		private void ClearScreen()
		{
			_clearScreen = true;
			InvalidateVisual();
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
				ClearScreen();
			}
		}

		// --------------------------------------------------------------------
		// ActualHeight, ActualWidth プロパティーが更新された
		// --------------------------------------------------------------------
		private void CommentControl_SizeChanged(Object sender, SizeChangedEventArgs e)
		{
			_fontUnit = ActualHeight * FONT_UNIT_SCALE;
			InvalidateDrawData();
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
			RenderTargetBitmap offScreen = new(Math.Max((Int32)ActualWidth, 1), Math.Max((Int32)ActualHeight, 1), Common.DEFAULT_DPI, Common.DEFAULT_DPI, PixelFormats.Pbgra32);

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
			foreach (CommentInfo commentInfo in CommentInfos.Keys)
			{
				if (commentInfo.IsDrawDataPrepared)
				{
					CalcCommentLeft(commentInfo);
					MoveComment(commentInfo);
					DrawCommentInfo(drawingContext, commentInfo);
					if (commentInfo.Right <= 0)
					{
						// 移動が完了したので削除
						CommentInfos.TryRemove(commentInfo, out _);
						if (!CommentInfos.Any())
						{
							ClearScreen();
						}
						Debug.WriteLine("DrawCommentInfosIfNeeded() 削除: 残: " + CommentInfos.Count);
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

			// 上下マージン
			Int32 margin = TopBottomMargin();

			// 描画
			Int32 frameThick = (Int32)ActualHeight / Yv2Constants.FRAME_DIVIDER;
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, margin, ActualWidth, frameThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, ActualHeight - frameThick - margin, ActualWidth, frameThick));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, margin, frameThick, ActualHeight - margin * 2));
			drawingContext.DrawRectangle(Brushes.GreenYellow, null, new Rect(ActualWidth - frameThick, margin, frameThick, ActualHeight - margin * 2));
		}

		// --------------------------------------------------------------------
		// 予約一覧を描画
		// --------------------------------------------------------------------
		private void DrawRequestListIfNeeded(DrawingContext drawingContext)
		{
			if (!_drawRequestList)
			{
				return;
			}
			if (Environment.TickCount >= _clearRequestListTick)
			{
				// 予約一覧を消去する時刻になった
				_drawRequestList = false;
				return;
			}

			// 描画
			drawingContext.DrawImage(_requestListOffScreen, new Rect(0, 0, _requestListOffScreen.Width, _requestListOffScreen.Height));
		}

		// --------------------------------------------------------------------
		// コマンドコメントの実行
		// --------------------------------------------------------------------
		private void ExecuteCommand(CommentInfo commentInfo)
		{
			switch (commentInfo.Command)
			{
				case Yv2Constants.COMMENT_COMMAND_REQUEST_LIST:
					ExecuteCommandRequestList(commentInfo);
					break;
				default:
					Debug.Assert(false, "ExecuteCommand() bad Command");
					break;
			}
		}

		// --------------------------------------------------------------------
		// コマンドコメントの実行（COMMENT_COMMAND_REQUEST_LIST）
		// 予約一覧描画データ（オフスクリーン）を作成する
		// --------------------------------------------------------------------
		private void ExecuteCommandRequestList(CommentInfo commentInfo)
		{
			// オフスクリーンを生成しサイズを確定
			_requestListOffScreen = CreateOffScreen();

			// 数値計算
			Double fontSize = REQUEST_LIST_FONT_SIZE * _fontUnit;
			Double indexFontSize = fontSize * 0.8;
			Double keyCaptionFontSize = fontSize * 0.3;
			Double keyFontSize = fontSize * 0.7;
			Double padding = _fontUnit;
			Debug.WriteLine("ExecuteCommandRequestList() fontSize: " + fontSize);

			// ペン
			Pen separatorPen = new(Brushes.MidnightBlue, _fontUnit * 0.3);
			Pen keyPen = new(Brushes.SkyBlue, _fontUnit * 0.2);

			DrawingVisual offScreenVisual = new();
			using DrawingContext offScreenContext = offScreenVisual.RenderOpen();

			// 枠描画
			Double lineHeight = fontSize + padding * 2;
			for (Int32 i = 0; i < 4; i++)
			{
				offScreenContext.DrawRectangle(Brushes.DarkBlue, null, new Rect(0, RequestListTop(i, fontSize, padding), _requestListOffScreen.Width, lineHeight));
			}

			// 予約描画
			List<TYukariRequest> requestList = GetRequestList();
			FormattedText keyCaptionText = new("キー", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
					keyCaptionFontSize, Brushes.SkyBlue, Common.DEFAULT_DPI);
			for (Int32 i = 0; i < 3; i++)
			{
				Double lineTop = RequestListTop(i, fontSize, padding);

				// インデックス
				FormattedText indexText = new((i + 1).ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
						indexFontSize, Brushes.SkyBlue, Common.DEFAULT_DPI);
				offScreenContext.DrawText(indexText, new Point((lineHeight - indexText.Width) / 2, lineTop + (lineHeight - indexText.Height) / 2));

				// 区切り線
				offScreenContext.DrawLine(separatorPen, new Point(lineHeight, lineTop), new Point(lineHeight, lineTop + lineHeight));
				if (i >= requestList.Count)
				{
					continue;
				}

				// 曲名
				offScreenContext.PushClip(new RectangleGeometry(new Rect(0, lineTop, _requestListOffScreen.Width - lineHeight - padding, lineHeight)));
				FormattedText titleText = new(Path.GetFileNameWithoutExtension(requestList[i].Path), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
						fontSize, Brushes.White, Common.DEFAULT_DPI);
				offScreenContext.DrawText(titleText, new Point(lineHeight + padding, lineTop + (lineHeight - titleText.Height) / 2));
				offScreenContext.Pop();

				// キー
				offScreenContext.DrawRectangle(null, keyPen, new Rect(_requestListOffScreen.Width - lineHeight, lineTop, lineHeight, lineHeight));
				offScreenContext.DrawText(keyCaptionText, new Point(_requestListOffScreen.Width - lineHeight + keyPen.Thickness, lineTop + keyPen.Thickness));
				FormattedText keyText = new(requestList[i].KeyChange.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
						keyFontSize, Brushes.SkyBlue, Common.DEFAULT_DPI);
				Double keyCaptionHeight = keyPen.Thickness + keyCaptionText.Height;
				offScreenContext.DrawText(keyText, new Point(_requestListOffScreen.Width - lineHeight + (lineHeight - keyText.Width) / 2,
						lineTop + keyCaptionHeight + (lineHeight - keyCaptionHeight - keyText.Height) / 2));
			}

			// 合計描画
			FormattedText totalText = new("合計 " + requestList.Count.ToString() + " 曲", CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
					fontSize, Brushes.White, Common.DEFAULT_DPI);
			offScreenContext.DrawText(totalText, new Point(_requestListOffScreen.Width - totalText.Width, RequestListTop(3, fontSize, padding) + (lineHeight - totalText.Height) / 2));

			offScreenContext.Close();
			_requestListOffScreen.Render(offScreenVisual);
			_requestListOffScreen.Freeze();
			SetDrawRequestList();
		}

		// --------------------------------------------------------------------
		// 予約一覧を取得
		// ゆかり manage-mpc.php 1315 行目「未再生の項目を検索」と同様の条件で検索
		// --------------------------------------------------------------------
		private List<TYukariRequest> GetRequestList()
		{
			using YukariRequestContext yukariRequestContext = new();
			return yukariRequestContext.YukariRequests.Where(x => x.NowPlaying == Yv2Constants.YUKARI_REQUEST_NOW_PLAYING_QUEUED).OrderBy(x => x.Order).ToList();
		}

		// --------------------------------------------------------------------
		// 描画データが再度準備されるようにする
		// --------------------------------------------------------------------
		private void InvalidateDrawData()
		{
			foreach (CommentInfo commentInfo in CommentInfos.Keys)
			{
				commentInfo.IsDrawDataPrepared = false;
			}
		}

		// --------------------------------------------------------------------
		// 上下マージン
		// --------------------------------------------------------------------
		private Int32 TopBottomMargin()
		{
			if (Yv2Model.Instance.EnvModel.Yv2Settings.EnableMargin)
			{
				return (Int32)ActualHeight * Yv2Model.Instance.EnvModel.Yv2Settings.MarginPercent / 100;
			}
			else
			{
				return 0;
			}
		}

		// --------------------------------------------------------------------
		// コメント移動
		// --------------------------------------------------------------------
		private void MoveComment(CommentInfo commentInfo)
		{
			Debug.Assert(commentInfo.MessageGeometry != null, "MoveComment() bad MessageGeometry");
			commentInfo.MessageGeometry.Transform = new TranslateTransform(commentInfo.Left, commentInfo.Top);
		}

		// --------------------------------------------------------------------
		// 描画データを準備
		// --------------------------------------------------------------------
		private void PrepareDrawData(CommentInfo commentInfo)
		{
			FormattedText formattedText = new(commentInfo.Message, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace,
					commentInfo.YukariSize * _fontUnit, Brushes.Black, Common.DEFAULT_DPI);
			commentInfo.MessageGeometry = formattedText.BuildGeometry(new Point(0, 0));

			commentInfo.Brush = new SolidColorBrush(commentInfo.Color);

			commentInfo.EdgeWidth = commentInfo.YukariSize * (Int32)_fontUnit / 15;
			commentInfo.Pen = new Pen(Brushes.Black, commentInfo.EdgeWidth);

			commentInfo.Speed = ((Int32)ActualWidth + commentInfo.Width) / (COMMENT_VIEWING_TIME / 1000);

			commentInfo.Top = CalcCommentTop(commentInfo);

			commentInfo.IsDrawDataPrepared = true;
		}

		// --------------------------------------------------------------------
		// 描画データを準備
		// --------------------------------------------------------------------
		private void PrepareDrawDataIfNeeded()
		{
			foreach (CommentInfo commentInfo in CommentInfos.Keys)
			{
				if (!commentInfo.IsDrawDataPrepared)
				{
					if (commentInfo.IsCommand())
					{
						// コマンド
						ExecuteCommand(commentInfo);
						CommentInfos.TryRemove(commentInfo, out _);
						Debug.WriteLine("PrepareDrawDataIfNeeded() コマンドコメント削除: 残: " + CommentInfos.Count);
					}
					else
					{
						// 一般コメント
						PrepareDrawData(commentInfo);
						Debug.Assert(commentInfo.MessageGeometry != null, "PrepareDrawDataIfNeeded() bad MessageGeometry");
						if (commentInfo.MessageGeometry.Bounds.IsEmpty)
						{
							// スペース等、描画できない文字のみで構成されたコメント
							CommentInfos.TryRemove(commentInfo, out _);
							Debug.WriteLine("PrepareDrawDataIfNeeded() 無効コメント削除: 残: " + CommentInfos.Count);
						}
					}
				}
			}
		}

		// --------------------------------------------------------------------
		// 予約一覧の描画位置
		// --------------------------------------------------------------------
		private Double RequestListTop(Int32 index, Double fontSize, Double padding)
		{
			// 文字サイズ、パディング x2、行間
			return index * (fontSize + padding * 2 + _fontUnit);
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
		// 予約一覧を描画するようにする
		// --------------------------------------------------------------------
		private void SetDrawRequestList()
		{
			_drawRequestList = true;
			_clearRequestListTick = Environment.TickCount + DRAW_REQUEST_LIST_DURATION;
		}

		// --------------------------------------------------------------------
		// ViewModel 側で DependencyProperty が変更された（CommentInfosProperty）
		// --------------------------------------------------------------------
		private static void SourceCommentInfosPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			Debug.WriteLine("SourceCommentInfosPropertyChanged");
		}
	}
}

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.Views.CustomControls
{
	/// <summary>
	/// このカスタム コントロールを XAML ファイルで使用するには、手順 1a または 1b の後、手順 2 に従います。
	///
	/// 手順 1a) 現在のプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
	/// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
	/// 追加します:
	///
	///     xmlns:MyNamespace="clr-namespace:YukkoView2.Views.CustomControls"
	///
	///
	/// 手順 1b) 異なるプロジェクトに存在する XAML ファイルでこのカスタム コントロールを使用する場合
	/// この XmlNamespace 属性を使用場所であるマークアップ ファイルのルート要素に
	/// 追加します:
	///
	///     xmlns:MyNamespace="clr-namespace:YukkoView2.Views.CustomControls;assembly=YukkoView2.Views.CustomControls"
	///
	/// また、XAML ファイルのあるプロジェクトからこのプロジェクトへのプロジェクト参照を追加し、
	/// リビルドして、コンパイル エラーを防ぐ必要があります:
	///
	///     ソリューション エクスプローラーで対象のプロジェクトを右クリックし、
	///     [参照の追加] の [プロジェクト] を選択してから、このプロジェクトを参照し、選択します。
	///
	///
	/// 手順 2)
	/// コントロールを XAML ファイルで使用します。
	///
	///     <MyNamespace:CommentControl/>
	///
	/// </summary>
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
				// ピクセルぴったり描画
				SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示コントロール生成時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
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

				// クリッピング解除
				drawingContext.Pop();
			}
			catch (Exception excep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(TraceEventType.Error, "コメント表示コントロール描画時エラー：\n" + excep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.ShowLogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + excep.StackTrace);
			}
		}
	}
}

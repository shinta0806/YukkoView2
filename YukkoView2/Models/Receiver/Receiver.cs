// ============================================================================
// 
// コメントを受信するクラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Shinta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;

namespace YukkoView2.Models.Receiver
{
	internal class Receiver
	{
		// ====================================================================
		// コンストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// メインコンストラクター
		// --------------------------------------------------------------------
		public Receiver()
		{
		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント受信を終了
		// --------------------------------------------------------------------
		public void Quite()
		{
			_cancellationTokenSource.Cancel();
		}

		// --------------------------------------------------------------------
		// 終了を指示されるまでコメントを受信し続ける
		// --------------------------------------------------------------------
		public Task ReceiveLoopAsync()
		{
			return Task.Run(async () =>
			{
				switch (Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType)
				{
					case CommentReceiveType.Download:
						DownloadLoop();
						break;
					default:
						await ReceivePushLoopAsync();
						break;
				}
			});
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 古すぎて無視するコメントの閾値 [時間]
		private const Int32 IGNORE_HOUR = 12;

		// TCP タイムアウト [ms]
		private const Int32 TCP_TIMEOUT = 5 * 1000;

		// コメント識別子
		private const String COMMENT_BEGIN_MARK = "Comment=";

		// ====================================================================
		// private 変数
		// ====================================================================

		// 終了指示用
		private CancellationTokenSource _cancellationTokenSource = new();

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析
		// --------------------------------------------------------------------
		private CommentInfo? AnalyzeExtendedCommentData(Byte[] array, Int32 beginPos)
		{
			return AnalyzeExtendedCommentData(Encoding.UTF8.GetString(array, beginPos, array.Length - beginPos));
		}

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析
		// --------------------------------------------------------------------
		private CommentInfo? AnalyzeExtendedCommentData(String comment)
		{
			// 拡張バージョン識別子の確認
			if (comment.Substring(1, 1) != "3")
			{
				throw new Exception("未対応の拡張コメントフォーマットです。");
			}

			// 古いコメントは無視
			DateTime commentTime = DateTime.ParseExact(comment.Substring(9, 19), "yyyy-MM-dd HH:mm:ss", null);
			String commentMessage = comment.Substring(28, comment.Length - 29);
			if (DateTime.Now.Subtract(commentTime) >= new TimeSpan(IGNORE_HOUR, 0, 0))
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, IGNORE_HOUR + "時間以上経過しているコメントを無視します：" + commentMessage);
				return null;
			}

			CommentInfo commentInfo = new();
			commentInfo.Message = commentMessage;
			commentInfo.YukariSize = Int32.Parse(comment.Substring(2, 1));
			commentInfo.Color = Color.FromRgb(Convert.ToByte(comment.Substring(3, 2), 16), Convert.ToByte(comment.Substring(5, 2), 16), Convert.ToByte(comment.Substring(7, 2), 16));
			commentInfo.InitialTick = Environment.TickCount;
			return commentInfo;
		}

		// --------------------------------------------------------------------
		// ゆかりと通信できるか確認
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		private async Task CheckYukariConnectionAsync()
		{
			Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "ゆかり通信チェック実施中...");
			Downloader downloader = new();
			downloader.CancellationToken = _cancellationTokenSource.Token;
			for (; ; )
			{
				(Boolean result, _) = await DownloadCommentAsync(downloader);

				// エラー無くダウンロードできたらチェックループ終了
				if (result)
				{
					break;
				}

				// エラー有りの場合はループを続ける
				Yv2Model.Instance.EnvModel.Yv2Status = Yv2Status.Error;
				Thread.Sleep(Yv2Constants.CHECK_CONNECTION_INTERVAL);

				ThrowIfCancellationRequested();
			}
		}

		// --------------------------------------------------------------------
		// コメントサーバーからコメントをダウンロード
		// ＜返値＞ result: 成功なら true
		// --------------------------------------------------------------------
		private async Task<(Boolean result, Byte[] comment)> DownloadCommentAsync(Downloader downloader)
		{
			Boolean result = false;
			Byte[] array = Array.Empty<Byte>();

			try
			{
				using MemoryStream memStream = new();
				(String serverUrl, String roomName) = Yv2Model.Instance.EnvModel.Yv2Settings.ServerUrlAndRoomName();
				await downloader.DownloadAsStreamAsync(serverUrl + "?r=" + HttpUtility.UrlEncode(roomName, Encoding.UTF8) + "&v=3", memStream);
				array = memStream.ToArray();
				result = true;
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメントダウンロード時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}

			return (result, array);
		}

		// --------------------------------------------------------------------
		// コメントをダウンロードし続ける
		// --------------------------------------------------------------------
		private void DownloadLoop()
		{

		}

		// --------------------------------------------------------------------
		// コメントをプッシュ通知で受信し続ける
		// --------------------------------------------------------------------
		private async Task ReceivePushLoopAsync()
		{
			TcpListener? listener = null;
			try
			{
				await CheckYukariConnectionAsync();

				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントプッシュ受信開始");

				// IPv4 と IPv6 の全ての IP アドレスを Listen する
				listener = new TcpListener(IPAddress.IPv6Any, Yv2Model.Instance.EnvModel.Yv2Settings.PushPort);
				listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
				listener.Start();
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "IP アドレス：" + ((IPEndPoint)listener.LocalEndpoint).Address
						+ ", ポート：" + ((IPEndPoint)listener.LocalEndpoint).Port);

				for (; ; )
				{
					try
					{
						// 接続要求があったら受け入れる
						TcpClient client = listener.AcceptTcpClient();

						String receivedString;
						using (NetworkStream networkStream = client.GetStream())
						{
							// ネットワークストリームの設定
							networkStream.ReadTimeout = TCP_TIMEOUT;
							networkStream.WriteTimeout = TCP_TIMEOUT;

							// クライアントから送られたデータを受信する
							Boolean disconnected = false;
							using MemoryStream memoryStream = new MemoryStream();
							Byte[] received = new Byte[1024];
							Int32 receivedSize = 0;
							do
							{
								receivedSize = networkStream.Read(received, 0, received.Length);
								if (receivedSize == 0)
								{
									// クライアントが切断したと判断
									disconnected = true;
									break;
								}

								// 受信したデータを蓄積する
								memoryStream.Write(received, 0, receivedSize);
							} while (networkStream.DataAvailable);

							receivedString = HttpUtility.UrlDecode(memoryStream.GetBuffer(), 0, (Int32)memoryStream.Length, Encoding.UTF8);

							if (!disconnected)
							{
								// クライアントに応答を返す
								String body = "OK";
								String header = "HTTP/1.1 200 OK\n"
										+ "Content-Length: " + body.Length + "\n"
										+ "Content-Type: text/html\n\n";
								String aSendString = header + body;
								Byte[] aSendBytes = Encoding.UTF8.GetBytes(aSendString);
								networkStream.Write(aSendBytes, 0, aSendBytes.Length);
							}
						}

						// コメントを取り出す
						Int32 commentPos = receivedString.IndexOf(COMMENT_BEGIN_MARK);
						if (commentPos < 0)
						{
							throw new Exception("コメントデータが見つかりません。");
						}
						if (commentPos + COMMENT_BEGIN_MARK.Length == receivedString.Length)
						{
							throw new Exception("コメントデータが空です。");
						}
						String comment = receivedString.Substring(commentPos + COMMENT_BEGIN_MARK.Length);


						// サーバーとの通信に成功したのでエラー表示解除
						//ClearIsCommentReceiveError();

						// コメント発行
						CommentInfo? commentInfo = AnalyzeExtendedCommentData(comment);
						if (commentInfo != null)
						{
							Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントを受信しました：" + commentInfo.Message);
							//mFormViewer.AddComment(commentInfo);
						}

						// 閉じる
						client.Close();
					}
					catch (Exception ex)
					{
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメント受信エラー（リトライします）：\n" + ex.Message);
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + ex.StackTrace);
						//EnableIsCommentReceiveError();
					}

					ThrowIfCancellationRequested();
				}
			}
			catch (OperationCanceledException)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントプッシュ受信処理を終了しました。");
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメントプッシュ受信時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
			finally
			{
				if (listener != null)
				{
					listener.Stop();
				}
			}
		}

		// --------------------------------------------------------------------
		// キャンセル要求があるなら例外発生
		// ＜例外＞ OperationCanceledException
		// --------------------------------------------------------------------
		private void ThrowIfCancellationRequested()
		{
			_cancellationTokenSource.Token.ThrowIfCancellationRequested();
			Yv2Model.Instance.EnvModel.AppCancellationTokenSource.Token.ThrowIfCancellationRequested();
		}

	}
}

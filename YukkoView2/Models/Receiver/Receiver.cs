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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;

using YukkoView2.Models.SharedMisc;
using YukkoView2.Models.YukkoView2Models;
using YukkoView2.ViewModels.MiscWindowViewModels;

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
		public Receiver(DisplayWindowViewModel displayWindowViewModel)
		{
			_commentContainer = displayWindowViewModel;
		}

		// ====================================================================
		// public 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 停止を指示されるまでコメントを受信し続ける
		// --------------------------------------------------------------------
		public Task StartAsync()
		{
			return Task.Run(async () =>
			{
				_cancellationTokenSource = new();
				switch (Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType)
				{
					case CommentReceiveType.Download:
						await DownloadLoopAsync();
						break;
					default:
						await ReceivePushLoopAsync();
						break;
				}
			});
		}

		// --------------------------------------------------------------------
		// コメント受信を停止
		// --------------------------------------------------------------------
		public Task StopAsync()
		{
			return Task.Run(() =>
			{
				try
				{
					_cancellationTokenSource.Cancel();

					if (Yv2Model.Instance.EnvModel.Yv2Settings.CommentReceiveType == CommentReceiveType.Push)
					{
						// ダミーコメントを投稿してプッシュ受信を終了させる
						TcpClient client = new(HOST_NAME_LOCAL_HOST, Yv2Model.Instance.EnvModel.Yv2Settings.PushPort);
						using NetworkStream networkStream = client.GetStream();
						networkStream.ReadTimeout = TCP_TIMEOUT;
						networkStream.WriteTimeout = TCP_TIMEOUT;
						Byte[] sendBytes = Encoding.UTF8.GetBytes("\r\n" + COMMENT_BEGIN_MARK + "X30FFFFFF" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " \t");
						networkStream.Write(sendBytes, 0, sendBytes.Length);
						client.Close();
					}
				}
				catch (Exception ex)
				{
					Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメント停止時エラー：\n" + ex.Message);
					Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + ex.StackTrace);
				}
			});
		}

		// ====================================================================
		// private 定数
		// ====================================================================

		// 古すぎて無視するコメントの閾値 [時間]
		private const Int32 COMMENT_IGNORE_HOUR = 12;

		// 古すぎて無視するコマンドの閾値 [分]
		private const Int32 COMMAND_IGNORE_MINUTE = 5;

		// ローカルホスト
		private const String HOST_NAME_LOCAL_HOST = "localhost";

		// TCP タイムアウト [ms]
		private const Int32 TCP_TIMEOUT = 5 * 1000;

		// コメント識別子
		private const String COMMENT_BEGIN_MARK = "Comment=";

		// ====================================================================
		// private 変数
		// ====================================================================

		// コメントを保持しているインスタンス
		private readonly DisplayWindowViewModel _commentContainer;

		// 終了指示用
		private CancellationTokenSource _cancellationTokenSource = new();

		// ====================================================================
		// private 関数
		// ====================================================================

		// --------------------------------------------------------------------
		// コメント文字列を解析（拡張／旧仕様両対応）
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeCommentData(Byte[] array)
		{
			// 先頭の改行を無視する
			Int32 beginPos = 0;
			while (beginPos < array.Length && (array[beginPos] == '\r' || array[beginPos] == '\n'))
			{
				beginPos++;
			}
			if (beginPos == array.Length)
			{
				// コメントの中身が無い
				return null;
			}

			// 文字列を解析してコメント情報化
			CommentInfo? commentInfo;
			if (array[beginPos] == 'X')
			{
				commentInfo = AnalyzeExtendedCommentData(array, beginPos);
			}
			else
			{
				commentInfo = AnalyzeOldFormatCommentData(array, beginPos);
			}
			return commentInfo;
		}

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeExtendedCommentData(Byte[] array, Int32 beginPos)
		{
			return AnalyzeExtendedCommentData(Encoding.UTF8.GetString(array, beginPos, array.Length - beginPos));
		}

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeExtendedCommentData(String comment)
		{
			CommentInfo? commentInfo;
			switch (comment.Substring(1, 1))
			{
				case "3":
					commentInfo = AnalyzeExtendedCommentDataV3(comment);
					break;
				case "4":
					commentInfo = AnalyzeExtendedCommentDataV4(comment);
					break;
				default:
					throw new Exception("未対応の拡張コメントフォーマットです。");
			}

			return commentInfo;
		}

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析（V3：コメント）
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeExtendedCommentDataV3(String comment)
		{
			// 古いコメントは無視
			DateTime commentTime = DateTime.ParseExact(comment.Substring(9, 19), "yyyy-MM-dd HH:mm:ss", null);
			String commentMessage = comment[28..^1];
			if (DateTime.Now.Subtract(commentTime) >= new TimeSpan(COMMENT_IGNORE_HOUR, 0, 0))
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, COMMENT_IGNORE_HOUR + "時間以上経過しているコメントを無視します：" + commentMessage);
				return null;
			}

			CommentInfo commentInfo = new(commentMessage, Int32.Parse(comment.Substring(2, 1)),
					Color.FromRgb(Convert.ToByte(comment.Substring(3, 2), 16), Convert.ToByte(comment.Substring(5, 2), 16), Convert.ToByte(comment.Substring(7, 2), 16)));
			return commentInfo;
		}

		// --------------------------------------------------------------------
		// 拡張コメント文字列を解析（V4：コマンド）
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeExtendedCommentDataV4(String comment)
		{
			// 古いコメントは無視
			DateTime commentTime = DateTime.ParseExact(comment.Substring(7, 19), "yyyy-MM-dd HH:mm:ss", null);
			String commentCommand = comment.Substring(2, 5);
			String commentMessage = comment[26..^1];
			if (DateTime.Now.Subtract(commentTime) >= new TimeSpan(0, COMMAND_IGNORE_MINUTE, 0))
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, COMMAND_IGNORE_MINUTE + "分以上経過しているコマンドを無視します："
						+ commentCommand + " = " + commentMessage);
				return null;
			}

			CommentInfo commentInfo = new(commentCommand, commentMessage);
			return commentInfo;
		}

		// --------------------------------------------------------------------
		// 旧仕様コメント文字列を解析
		// --------------------------------------------------------------------
		private static CommentInfo? AnalyzeOldFormatCommentData(Byte[] array, Int32 beginPos)
		{
			String comment = Encoding.GetEncoding(Common.CODE_PAGE_SHIFT_JIS).GetString(array, beginPos, array.Length - beginPos);

			if (comment == "nothing" || comment.Length <= 7)
			{
				return null;
			}

			CommentInfo commentInfo = new(comment[7..^1], Int32.Parse(comment[..1]),
					Color.FromRgb(Convert.ToByte(comment.Substring(1, 2), 16), Convert.ToByte(comment.Substring(3, 2), 16), Convert.ToByte(comment.Substring(5, 2), 16)));
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
				Thread.Sleep(Yv2Constants.CHECK_CONNECTION_INTERVAL);
				ThrowIfCancellationRequested();
			}
		}

		// --------------------------------------------------------------------
		// コメントサーバーからコメントをダウンロード
		// ＜返値＞ result: 成功なら true
		// --------------------------------------------------------------------
		private static async Task<(Boolean result, Byte[] array)> DownloadCommentAsync(Downloader downloader)
		{
			Boolean result = false;
			Byte[] array = Array.Empty<Byte>();

			try
			{
				using MemoryStream memStream = new();
				(String serverUrl, String roomName) = Yv2Model.Instance.EnvModel.Yv2Settings.ServerUrlAndRoomName();
				HttpResponseMessage response = await downloader.DownloadAsStreamAsync(serverUrl + "?r=" + HttpUtility.UrlEncode(roomName, Encoding.UTF8) + "&v=3", memStream);
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception("データを取得できませんでした。");
				}
				array = memStream.ToArray();

				// サーバーとの通信に成功したのでエラー表示解除
				// コメントが存在しない場合は "nothing" が返ってくるので、成功していればここを通る
				Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors[(Int32)Yv2StatusErrorFactor.ServerNotConnected] = false;

				result = true;
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors[(Int32)Yv2StatusErrorFactor.ServerNotConnected] = true;
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメントダウンロード時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}

			return (result, array);
		}

		// --------------------------------------------------------------------
		// コメントをダウンロードし続ける
		// --------------------------------------------------------------------
		private async Task DownloadLoopAsync()
		{
			try
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントダウンロード開始");

				// ダウンローダー
				Downloader downloader = new();
				downloader.CancellationToken = _cancellationTokenSource.Token;

				for (; ; )
				{
					try
					{
						// サーバーに溜まっているコメントをすべて読み込む
						for (; ; )
						{
							// コメント表示数が多い場合はスキップ
							if (_commentContainer.NumComments() >= Yv2Constants.NUM_DISPLAY_COMMENTS_MAX)
							{
								break;
							}

							// サーバーの負荷軽減のためちょっとだけ休む
							Thread.Sleep(Common.GENERAL_SLEEP_TIME);

							// ダウンロード
							(Boolean result, Byte[] array) = await DownloadCommentAsync(downloader);
							if (!result)
							{
								break;
							}

							// コメント
							CommentInfo? commentInfo = AnalyzeCommentData(array);
							if (commentInfo == null)
							{
								// 溜まっているコメントが無くなった
								break;
							}
							Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントをダウンロードしました：" + commentInfo.Message);

							// コメント発行
							_commentContainer.AddComment(commentInfo);
						}

					}
					catch (Exception ex)
					{
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "ダウンロードエラー（リトライします）：\n" + ex.Message);
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + ex.StackTrace);
					}

					// しばらく休憩
					Thread.Sleep(Yv2Model.Instance.EnvModel.Yv2Settings.DownloadInterval);
					ThrowIfCancellationRequested();
				}

			}
			catch (OperationCanceledException)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントダウンロード処理を終了しました。");
				Debug.WriteLine("コメントダウンロード処理を終了しました。");
			}
			catch (Exception oExcep)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメントダウンロード時エラー：\n" + oExcep.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + oExcep.StackTrace);
			}
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
							using MemoryStream memoryStream = new();
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
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "ReceivePushLoopAsync() receivedString: " + receivedString);
						Int32 commentPos = receivedString.IndexOf(COMMENT_BEGIN_MARK);
						if (commentPos < 0)
						{
							throw new Exception("コメントデータが見つかりません。");
						}
						if (commentPos + COMMENT_BEGIN_MARK.Length == receivedString.Length)
						{
							throw new Exception("コメントデータが空です。");
						}
						String comment = receivedString[(commentPos + COMMENT_BEGIN_MARK.Length)..];

						// サーバーとの通信に成功したのでエラー表示解除
						Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors[(Int32)Yv2StatusErrorFactor.ServerNotConnected] = false;

						// コメント発行
						CommentInfo? commentInfo = AnalyzeExtendedCommentData(comment);
						if (commentInfo != null)
						{
							Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントを受信しました：" + commentInfo.Message);
							_commentContainer.AddComment(commentInfo);
#if DEBUGz
							CommentInfo db = new(Yv2Constants.COMMENT_COMMAND_REQUEST_LIST, "1");
							_commentContainer.AddComment(db);
#endif
						}

						// 閉じる
						client.Close();
					}
					catch (Exception ex)
					{
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメント受信エラー（リトライします）：\n" + ex.Message);
						Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Verbose, "　スタックトレース：\n" + ex.StackTrace);
						Yv2Model.Instance.EnvModel.Yv2StatusErrorFactors[(Int32)Yv2StatusErrorFactor.ServerNotConnected] = true;
					}

					ThrowIfCancellationRequested();
				}
			}
			catch (OperationCanceledException)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "コメントプッシュ受信処理を終了しました。");
				Debug.WriteLine("コメントプッシュ受信処理を終了しました。");
			}
			catch (Exception ex)
			{
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(TraceEventType.Error, "コメントプッシュ受信時エラー：\n" + ex.Message);
				Yv2Model.Instance.EnvModel.LogWriter.LogMessage(Common.TRACE_EVENT_TYPE_STATUS, "　スタックトレース：\n" + ex.StackTrace);
			}
			finally
			{
				listener?.Stop();
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

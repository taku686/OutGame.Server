using MagicOnion.Serialization;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Server;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// MessagePackシリアライザーの設定
// ============================================================================
// 【重要】クライアント（Unity）とサーバーで同じシリアライザー設定を使用する必要がある
// 設定が異なると、シリアライズ/デシリアライズ時にエラーが発生する
// 
// 【背景】
// MagicOnionはgRPC上でMessagePackを使用してデータをシリアライズする
// MessagePackは高速なバイナリシリアライザーだが、型によっては
// 適切なResolver（シリアライズ方法の定義）が必要
// 
// 【発生していたエラーの原因】
// サーバーとクライアントでMessagePackの設定が異なっていたため、
// サーバーがシリアライズしたデータをクライアントが正しくデシリアライズできなかった
// 特にDateTime型はTimestamp拡張型（fixext 8）でシリアライズされるが、
// クライアント側でその形式を解釈するResolverが設定されていなかった
// ============================================================================

// ----------------------------------------
// CompositeResolverの作成
// ----------------------------------------
// 複数のResolverを組み合わせて、様々な型のシリアライズをサポート
// 上から順に評価され、最初にマッチしたResolverが使用される
var resolver = CompositeResolver.Create(
    // BuiltinResolver: 組み込みのプリミティブ型（int, string, bool等）と
    // 基本的なコレクション型（List, Dictionary等）をサポート
    BuiltinResolver.Instance,

    // AttributeFormatterResolver: [MessagePackObject]属性や[Key]属性が付与された
    // ユーザー定義型をシリアライズ可能にする
    AttributeFormatterResolver.Instance,

    // NativeDateTimeResolver: DateTime型を.NETのネイティブ形式でシリアライズ
    // 【重要】クライアント側と同じResolverを使用することで互換性を確保
    // これがないと、DateTime型のデータでエラーが発生する
    NativeDateTimeResolver.Instance,

    // StandardResolverAllowPrivate: privateフィールドもシリアライズ対象にする
    // また、動的にFormatterを生成する機能も含む
    StandardResolverAllowPrivate.Instance
);

// ----------------------------------------
// MessagePackSerializerOptionsの作成
// ----------------------------------------
// シリアライザーの動作オプションを設定
// WithResolver()で上記で作成したResolverを適用
var messagePackOptions = MessagePackSerializerOptions.Standard.WithResolver(resolver);

// ----------------------------------------
// MagicOnionのシリアライザープロバイダーを設定
// ----------------------------------------
// MagicOnionが内部で使用するシリアライザーを上書きする
// これにより、全てのMagicOnion通信で上記のオプションが使用される
// 
// 【注意】この設定はAddMagicOnion()を呼び出す前に行う必要がある
MagicOnionSerializerProvider.Default =
    MessagePackMagicOnionSerializerProvider.Default.WithOptions(messagePackOptions);

// ============================================================================
// KestrelでHTTP/2を有効化（gRPCに必要）
// ============================================================================
// gRPCはHTTP/2プロトコルを使用するため、Kestrelの設定が必要
// 
// 【HTTP/2の特徴】
// - 多重化: 1つのTCP接続で複数のリクエスト/レスポンスを並行処理
// - ヘッダー圧縮: HPACKによる効率的なヘッダー圧縮
// - サーバープッシュ: サーバーからクライアントへの事前送信
// - ストリーミング: 双方向のストリーミング通信をサポート
builder.WebHost.ConfigureKestrel(options =>
{
    // ポート5244でHTTP/2のみを使用（平文=暗号化なし）
    // 開発環境では平文HTTP/2（h2c）を使用
    // 本番環境ではHTTPS（h2）の使用を推奨
    options.ListenLocalhost(5244, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

// ============================================================================
// サービスの登録
// ============================================================================
// AddMagicOnion()でMagicOnionのサービスをDIコンテナに登録
// これにより、ITestServiceを実装したTestServiceクラスが自動的に検出・登録される
builder.Services.AddMagicOnion();

var app = builder.Build();

// ============================================================================
// HTTPリクエストパイプラインの設定
// ============================================================================

// MapMagicOnionService()でMagicOnionのエンドポイントをマッピング
// これにより、gRPCリクエストがMagicOnionサービスにルーティングされる
app.MapMagicOnionService();

// ルートパス（/）へのGETリクエストに対する応答
// ブラウザでアクセスした場合のメッセージを表示
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// アプリケーションを起動
app.Run();

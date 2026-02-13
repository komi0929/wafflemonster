namespace Soyya.WaffleMonster
{
    /// <summary>
    /// ゲーム全体の状態を定義
    /// </summary>
    public enum GameState
    {
        Title,      // タイトル画面
        Ready,      // カウントダウン中
        Playing,    // ゲーム進行中
        GameOver,   // モンスターに捕まった / 時間切れ
        GameClear   // ゴール到達
    }
}

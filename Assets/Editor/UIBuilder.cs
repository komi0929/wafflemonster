using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// 全UI自動構築
    /// HUD, タイトル画面, リザルト画面をコードで生成しSerializedFieldを自動接続
    /// </summary>
    public static class UIBuilder
    {
        [MenuItem("Soyya/UI構築/全UI生成")]
        public static void BuildAllUI()
        {
            // 既存GameCanvasを削除
            var existing = GameObject.Find("GameCanvas");
            if (existing != null) Object.DestroyImmediate(existing);

            var canvas = CreateCanvas("GameCanvas");
            BuildHUD(canvas.transform);
            BuildMobileControls(canvas.transform);
            BuildTitleScreen(canvas.transform);
            BuildResultScreen(canvas.transform);

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("[UIBuilder] 全UI構築完了（スマホ専用）");
        }

        // ─── Canvas作成 ───

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        // ─── HUD ───

        private static void BuildHUD(Transform parent)
        {
            var hudRoot = new GameObject("HUD");
            hudRoot.transform.SetParent(parent, false);
            var hudGroup = hudRoot.AddComponent<CanvasGroup>();

            // MobileHUD コンポーネント
            var hud = hudRoot.AddComponent<MobileHUD>();

            // ── タイマー（左上）──
            var timerPanel = CreatePanel(hudRoot.transform, "TimerPanel",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(20, -20), new Vector2(280, 60));
            SetPanelStyle(timerPanel, new Color(0, 0, 0, 0.5f));

            var timerText = CreateTMP(timerPanel.transform, "TimerText",
                "90", 36, TextAlignmentOptions.Center, Color.white);
            StretchFill(timerText);

            // タイマーバー
            var timerBarBg = CreateImage(timerPanel.transform, "TimerBarBg",
                new Color(0.2f, 0.2f, 0.2f, 0.6f));
            var bgRt = timerBarBg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0);
            bgRt.anchorMax = new Vector2(1, 0);
            bgRt.anchoredPosition = new Vector2(0, -5);
            bgRt.sizeDelta = new Vector2(0, 6);

            var timerFill = CreateImage(timerBarBg.transform, "TimerFill",
                new Color(0f, 0.9f, 1f, 0.8f));
            StretchFill(timerFill);
            timerFill.GetComponent<Image>().type = Image.Type.Filled;
            timerFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;

            // ── ワッフル残数（右上）──
            var wafflePanel = CreatePanel(hudRoot.transform, "WafflePanel",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-20, -20), new Vector2(200, 60));
            SetPanelStyle(wafflePanel, new Color(0, 0, 0, 0.5f));

            // ワッフルアイコン（●）
            var iconText = CreateTMP(wafflePanel.transform, "WaffleIcon",
                "🧇", 28, TextAlignmentOptions.Left, new Color(1f, 0.85f, 0.4f));
            var iconRt = iconText.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0);
            iconRt.anchorMax = new Vector2(0.4f, 1);
            iconRt.offsetMin = new Vector2(10, 0);
            iconRt.offsetMax = Vector2.zero;

            var waffleCountText = CreateTMP(wafflePanel.transform, "WaffleCountText",
                "×10", 32, TextAlignmentOptions.Right, Color.white);
            var wcRt = waffleCountText.GetComponent<RectTransform>();
            wcRt.anchorMin = new Vector2(0.3f, 0);
            wcRt.anchorMax = new Vector2(1, 1);
            wcRt.offsetMin = Vector2.zero;
            wcRt.offsetMax = new Vector2(-10, 0);

            // ── コンボテキスト（右中央）──
            var comboText = CreateTMP(hudRoot.transform, "ComboText",
                "", 42, TextAlignmentOptions.Center,
                new Color(0f, 0.9f, 1f));
            var comboRt = comboText.GetComponent<RectTransform>();
            comboRt.anchorMin = new Vector2(0.5f, 0.5f);
            comboRt.anchorMax = new Vector2(0.5f, 0.5f);
            comboRt.anchoredPosition = new Vector2(250, 80);
            comboRt.sizeDelta = new Vector2(300, 60);
            comboText.SetActive(false);

            // ── カウントダウンテキスト（画面中央）──
            var countdownText = CreateTMP(hudRoot.transform, "CountdownText",
                "3", 100, TextAlignmentOptions.Center, Color.white);
            var cdRt = countdownText.GetComponent<RectTransform>();
            cdRt.anchorMin = new Vector2(0.5f, 0.5f);
            cdRt.anchorMax = new Vector2(0.5f, 0.5f);
            cdRt.anchoredPosition = Vector2.zero;
            cdRt.sizeDelta = new Vector2(300, 150);
            countdownText.SetActive(false);

            // ── ダンジャーオーバーレイ ──
            var dangerOverlay = CreateImage(hudRoot.transform, "DangerOverlay",
                new Color(1, 0, 0, 0));
            StretchFill(dangerOverlay);
            dangerOverlay.GetComponent<Image>().raycastTarget = false;
            dangerOverlay.SetActive(false);

            // ── クロスヘア（中央）──
            var crosshair = CreateCrosshair(hudRoot.transform);

            // SerializedObject でフィールド接続
            var so = new SerializedObject(hud);
            SetProp(so, "_timerText", timerText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_waffleCountText", waffleCountText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_comboText", comboText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_countdownText", countdownText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_timerFill", timerFill.GetComponent<Image>());
            SetProp(so, "_dangerOverlay", dangerOverlay.GetComponent<Image>());
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─── モバイルコントロール ───

        private static void BuildMobileControls(Transform parent)
        {
            var ctrlRoot = new GameObject("MobileControls");
            ctrlRoot.transform.SetParent(parent, false);

            // ── 左: ジョイスティック ──
            var joyBg = new GameObject("JoystickBG");
            joyBg.transform.SetParent(ctrlRoot.transform, false);
            var joyBgImg = joyBg.AddComponent<Image>();
            joyBgImg.color = new Color(1, 1, 1, 0.15f);
            var joyBgRt = joyBg.GetComponent<RectTransform>();
            joyBgRt.anchorMin = new Vector2(0, 0);
            joyBgRt.anchorMax = new Vector2(0, 0);
            joyBgRt.pivot = new Vector2(0.5f, 0.5f);
            joyBgRt.anchoredPosition = new Vector2(140, 140);
            joyBgRt.sizeDelta = new Vector2(200, 200);

            var joyHandle = new GameObject("JoystickHandle");
            joyHandle.transform.SetParent(joyBg.transform, false);
            var handleImg = joyHandle.AddComponent<Image>();
            handleImg.color = new Color(1, 1, 1, 0.4f);
            var handleRt = joyHandle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(80, 80);
            handleRt.anchoredPosition = Vector2.zero;

            // VirtualJoystick コンポーネント
            var joystick = joyBg.AddComponent<VirtualJoystick>();
            var joySo = new SerializedObject(joystick);
            SetProp(joySo, "_background", joyBg.GetComponent<RectTransform>());
            SetProp(joySo, "_handle", handleRt);
            joySo.ApplyModifiedPropertiesWithoutUndo();

            // ── 右: 投擲ボタン ──
            var throwBtn = CreateButton(ctrlRoot.transform, "ThrowButton",
                "🧇", new Color(1f, 0.3f, 0.6f, 0.7f),
                Vector2.zero, new Vector2(120, 120));
            var throwRt = throwBtn.GetComponent<RectTransform>();
            throwRt.anchorMin = new Vector2(1, 0);
            throwRt.anchorMax = new Vector2(1, 0);
            throwRt.pivot = new Vector2(0.5f, 0.5f);
            throwRt.anchoredPosition = new Vector2(-100, 140);
            // ボタンラベルを大きくする
            var label = throwBtn.transform.Find("Label");
            if (label != null)
            {
                var tmp = label.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.fontSize = 48;
            }

            // ── PlayerControllerに接続 ──
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                var pSo = new SerializedObject(player);
                SetProp(pSo, "_joystick", joystick);
                pSo.ApplyModifiedPropertiesWithoutUndo();

                // 投擲ボタンのonClickにPlayerController.ThrowWaffleを接続
                var btn = throwBtn.GetComponent<Button>();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    btn.onClick,
                    new UnityEngine.Events.UnityAction(player.ThrowWaffle));
            }

            Debug.Log("[UIBuilder] モバイルコントロール構築完了");
        }

        // ─── タイトル画面 ───

        private static void BuildTitleScreen(Transform parent)
        {
            var titleRoot = new GameObject("TitleScreen");
            titleRoot.transform.SetParent(parent, false);
            var titleGroup = titleRoot.AddComponent<CanvasGroup>();

            // フルスクリーン背景
            var bg = CreateImage(titleRoot.transform, "TitleBG",
                new Color(0.01f, 0.01f, 0.03f, 0.85f));
            StretchFill(bg);
            bg.GetComponent<Image>().raycastTarget = true;

            // タイトルテキスト
            var titleText = CreateTMP(titleRoot.transform, "TitleText",
                "WAFFLE MONSTER", 72, TextAlignmentOptions.Center,
                new Color(1f, 0.85f, 0.4f));
            var titleRt = titleText.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0, 120);
            titleRt.sizeDelta = new Vector2(800, 100);
            // アウトライン付き
            var outline = titleText.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.3f, 0.1f, 1f);
            outline.effectDistance = new Vector2(3, -3);

            // サブタイトル
            var subText = CreateTMP(titleRoot.transform, "SubTitle",
                "〜 ネオン裏路地の脱出劇 〜", 24, TextAlignmentOptions.Center,
                new Color(0f, 0.9f, 1f, 0.8f));
            var subRt = subText.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.5f, 0.5f);
            subRt.anchorMax = new Vector2(0.5f, 0.5f);
            subRt.anchoredPosition = new Vector2(0, 60);
            subRt.sizeDelta = new Vector2(600, 40);

            // スタートボタン
            var startBtn = CreateButton(titleRoot.transform, "StartButton",
                "▶  START", new Color(1f, 0.3f, 0.6f),
                new Vector2(0, -30), new Vector2(300, 65));

            // 操作説明
            var controlsText = CreateTMP(titleRoot.transform, "ControlsText",
                "左スティック: 移動　|　右スワイプ: 視点操作\n" +
                "🧇ボタン: ワッフル投擲",
                16, TextAlignmentOptions.Center,
                new Color(0.7f, 0.7f, 0.8f, 0.7f));
            var ctrlRt = controlsText.GetComponent<RectTransform>();
            ctrlRt.anchorMin = new Vector2(0.5f, 0.5f);
            ctrlRt.anchorMax = new Vector2(0.5f, 0.5f);
            ctrlRt.anchoredPosition = new Vector2(0, -120);
            ctrlRt.sizeDelta = new Vector2(700, 60);

            // ベストタイム
            var bestText = CreateTMP(titleRoot.transform, "BestTimeText",
                "", 22, TextAlignmentOptions.Center,
                new Color(1f, 0.8f, 0f, 0.9f));
            var bestRt = bestText.GetComponent<RectTransform>();
            bestRt.anchorMin = new Vector2(0.5f, 0.5f);
            bestRt.anchorMax = new Vector2(0.5f, 0.5f);
            bestRt.anchoredPosition = new Vector2(0, -180);
            bestRt.sizeDelta = new Vector2(400, 40);

            // TitleScreen コンポーネント
            var ts = titleRoot.AddComponent<TitleScreen>();
            var so = new SerializedObject(ts);
            SetProp(so, "_startButton", startBtn.GetComponent<Button>());
            SetProp(so, "_titleText", titleText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_bestTimeText", bestText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_controlsText", controlsText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_canvasGroup", titleGroup);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─── リザルト画面 ───

        private static void BuildResultScreen(Transform parent)
        {
            var resultRoot = new GameObject("ResultScreen");
            resultRoot.transform.SetParent(parent, false);
            var resultGroup = resultRoot.AddComponent<CanvasGroup>();
            resultGroup.alpha = 0f;

            // 半透明背景
            var bg = CreateImage(resultRoot.transform, "ResultBG",
                new Color(0, 0, 0, 0.7f));
            StretchFill(bg);

            // ── GameOverパネル ──
            var goPanel = CreatePanel(resultRoot.transform, "GameOverPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(600, 350));
            SetPanelStyle(goPanel, new Color(0.15f, 0.02f, 0.02f, 0.85f));

            var goTitle = CreateTMP(goPanel.transform, "GOTitle",
                "GAME OVER", 56, TextAlignmentOptions.Center,
                new Color(1f, 0.2f, 0.2f));
            var goTitleRt = goTitle.GetComponent<RectTransform>();
            goTitleRt.anchorMin = new Vector2(0, 0.6f);
            goTitleRt.anchorMax = new Vector2(1, 1);
            goTitleRt.offsetMin = Vector2.zero;
            goTitleRt.offsetMax = Vector2.zero;

            var goSubText = CreateTMP(goPanel.transform, "GOSubText",
                "モンスターに捕まった...", 22, TextAlignmentOptions.Center,
                new Color(0.8f, 0.6f, 0.6f));
            var goSubRt = goSubText.GetComponent<RectTransform>();
            goSubRt.anchorMin = new Vector2(0, 0.4f);
            goSubRt.anchorMax = new Vector2(1, 0.6f);
            goSubRt.offsetMin = Vector2.zero;
            goSubRt.offsetMax = Vector2.zero;

            // ── GameClearパネル ──
            var gcPanel = CreatePanel(resultRoot.transform, "GameClearPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(600, 400));
            SetPanelStyle(gcPanel, new Color(0.02f, 0.08f, 0.05f, 0.85f));

            var gcTitle = CreateTMP(gcPanel.transform, "GCTitle",
                "GAME CLEAR!", 56, TextAlignmentOptions.Center,
                new Color(0f, 1f, 0.5f));
            var gcTitleRt = gcTitle.GetComponent<RectTransform>();
            gcTitleRt.anchorMin = new Vector2(0, 0.7f);
            gcTitleRt.anchorMax = new Vector2(1, 1);
            gcTitleRt.offsetMin = Vector2.zero;
            gcTitleRt.offsetMax = Vector2.zero;

            var clearTimeText = CreateTMP(gcPanel.transform, "ClearTimeText",
                "クリアタイム: --.-秒", 28, TextAlignmentOptions.Center, Color.white);
            var ctRt = clearTimeText.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0, 0.5f);
            ctRt.anchorMax = new Vector2(1, 0.7f);
            ctRt.offsetMin = Vector2.zero;
            ctRt.offsetMax = Vector2.zero;

            var clearRankText = CreateTMP(gcPanel.transform, "ClearRankText",
                "", 40, TextAlignmentOptions.Center,
                new Color(1f, 0.85f, 0f));
            var crRt = clearRankText.GetComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0, 0.35f);
            crRt.anchorMax = new Vector2(1, 0.5f);
            crRt.offsetMin = Vector2.zero;
            crRt.offsetMax = Vector2.zero;

            var clearWaffleText = CreateTMP(gcPanel.transform, "ClearWaffleText",
                "残りワッフル: ×0", 22, TextAlignmentOptions.Center,
                new Color(0.8f, 0.8f, 0.9f));
            var cwRt = clearWaffleText.GetComponent<RectTransform>();
            cwRt.anchorMin = new Vector2(0, 0.2f);
            cwRt.anchorMax = new Vector2(1, 0.35f);
            cwRt.offsetMin = Vector2.zero;
            cwRt.offsetMax = Vector2.zero;

            // ── ボタン群（共通）──
            var btnPanel = CreatePanel(resultRoot.transform, "ButtonPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -160), new Vector2(500, 70));

            var retryBtn = CreateButton(btnPanel.transform, "RetryButton",
                "▶  RETRY", new Color(0f, 0.8f, 1f),
                new Vector2(-85, 0), new Vector2(220, 55));

            var titleBtn = CreateButton(btnPanel.transform, "TitleButton",
                "■  TITLE", new Color(0.5f, 0.5f, 0.6f),
                new Vector2(85, 0), new Vector2(220, 55));

            // ResultScreen コンポーネント
            var rs = resultRoot.AddComponent<ResultScreen>();
            var so = new SerializedObject(rs);
            SetProp(so, "_gameOverPanel", goPanel);
            SetProp(so, "_gameClearPanel", gcPanel);
            SetProp(so, "_clearTimeText", clearTimeText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_clearWaffleText", clearWaffleText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_clearRankText", clearRankText.GetComponent<TextMeshProUGUI>());
            SetProp(so, "_retryButton", retryBtn.GetComponent<Button>());
            SetProp(so, "_titleButton", titleBtn.GetComponent<Button>());
            SetProp(so, "_canvasGroup", resultGroup);
            so.ApplyModifiedPropertiesWithoutUndo();

            resultRoot.SetActive(false);
        }

        // ─── ユーティリティ ───

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            rt.pivot = anchorMin; // pivot matches anchor for corner-based panels
            return go;
        }

        private static void SetPanelStyle(GameObject panel, Color bgColor)
        {
            var img = panel.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = false;

            // 角丸っぽくするためにはSprite必要だが、ここではフラットカラーで
        }

        private static GameObject CreateTMP(Transform parent, string name,
            string text, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(300, 50);

            return go;
        }

        private static GameObject CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name,
            string label, Color bgColor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            colors.selectedColor = bgColor;
            btn.colors = colors;

            // ラベル
            var labelGo = CreateTMP(go.transform, "Label",
                label, 24, TextAlignmentOptions.Center, Color.white);
            StretchFill(labelGo);

            return go;
        }

        private static GameObject CreateCrosshair(Transform parent)
        {
            var root = new GameObject("Crosshair");
            root.transform.SetParent(parent, false);

            // 中央ドット
            var dot = CreateImage(root.transform, "Dot", new Color(1, 1, 1, 0.8f));
            var dRt = dot.GetComponent<RectTransform>();
            dRt.anchorMin = dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(4, 4);
            dot.GetComponent<Image>().raycastTarget = false;

            // 水平線
            var h = CreateImage(root.transform, "H", new Color(1, 1, 1, 0.5f));
            var hRt = h.GetComponent<RectTransform>();
            hRt.anchorMin = hRt.anchorMax = new Vector2(0.5f, 0.5f);
            hRt.sizeDelta = new Vector2(20, 2);
            h.GetComponent<Image>().raycastTarget = false;

            // 垂直線
            var v = CreateImage(root.transform, "V", new Color(1, 1, 1, 0.5f));
            var vRt = v.GetComponent<RectTransform>();
            vRt.anchorMin = vRt.anchorMax = new Vector2(0.5f, 0.5f);
            vRt.sizeDelta = new Vector2(2, 20);
            v.GetComponent<Image>().raycastTarget = false;

            return root;
        }

        private static void StretchFill(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetProp(SerializedObject so, string propName, Object value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
                prop.objectReferenceValue = value;
        }
    }
}

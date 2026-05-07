#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Animations;

/// <summary>
/// Unity 메뉴 → FakeSoldier → Setup All Scenes 실행 시
/// MainMenu / Stage_01~05 / Ending_Bad,Normal,True / Credit 씬을 자동 생성.
/// </summary>
public static class ProjectSetup
{
    const string SCENES_PATH = "Assets/Scenes";
    const string ANIMATOR_PATH = "Assets/Animations/PlayerAnimator.controller";

    // ───────────────────────────────────────────────
    // 진입점
    // ───────────────────────────────────────────────

    [MenuItem("FakeSoldier/▶ Setup All Scenes (최초 1회 실행)")]
    static void SetupAllScenes()
    {
        if (!EditorUtility.DisplayDialog("씬 자동 생성",
            "모든 씬을 자동 생성합니다.\n기존 동명 씬 파일은 덮어쓰게 됩니다.\n계속하시겠습니까?", "생성", "취소"))
            return;

        EnsureDirectories();
        SetupSpritesAsSprite();
        EnsurePlayerAnimator();

        CreateMainMenuScene();
        CreateStageScene(1, "Stage_01", "1단계 | 5월 18일 — 전남대 앞", "Assets/bg_03_stage1_university.png", Stage1Data());
        CreateStageScene(2, "Stage_02", "2단계 | 5월 19일 — 골목",       "Assets/bg_04_stage2_alley.png",        Stage2Data());
        CreateStageScene(3, "Stage_03", "3단계 | 5월 20일 — 야간 검문",   "Assets/bg_05_stage3_checkpoint.png",   Stage3Data());
        CreateStageScene(4, "Stage_04", "4단계 | 5월 21일 — 도청 앞",     "Assets/bg_06_stage4_dochung.png",      Stage4Data());
        CreateStageScene(5, "Stage_05", "5단계 | 5월 27일 — 도청 내부",   "Assets/bg_07_stage5_interior.png",     Stage5Data());
        CreateEndingScene("Ending_Bad",    "복종 엔딩",  BadEndingBody(),    "Assets/bg_06_stage4_dochung.png");
        CreateEndingScene("Ending_Normal", "방관 엔딩",  NormalEndingBody(), "Assets/bg_05_stage3_checkpoint.png");
        CreateEndingScene("Ending_True",   "거부 엔딩",  TrueEndingBody(),   "Assets/bg_04_stage2_alley.png");
        CreateCreditScene();

        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료!",
            "씬 생성 완료!\n\nMainMenu 씬을 열고 Play를 눌러 테스트하세요.\n\n* Player 오브젝트에 Tag='Player' 확인\n* 각 씬의 배경 Image에 배경 스프라이트 연결 확인",
            "확인");
    }

    // ───────────────────────────────────────────────
    // 초기 세팅
    // ───────────────────────────────────────────────

    static void EnsureDirectories()
    {
        if (!Directory.Exists(SCENES_PATH))       Directory.CreateDirectory(SCENES_PATH);
        if (!Directory.Exists("Assets/Animations")) Directory.CreateDirectory("Assets/Animations");
        AssetDatabase.Refresh();
    }

    static void SetupSpritesAsSprite()
    {
        string[] paths = {
            "Assets/bg_01_main_menu.png", "Assets/bg_02_settings.png",
            "Assets/bg_03_stage1_university.png", "Assets/bg_04_stage2_alley.png",
            "Assets/bg_05_stage3_checkpoint.png", "Assets/bg_06_stage4_dochung.png",
            "Assets/bg_07_stage5_interior.png",
        };
        foreach (var path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            if (importer.textureType == TextureImporterType.Sprite) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }

    static void EnsurePlayerAnimator()
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_PATH) != null) return;
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ANIMATOR_PATH);
        ctrl.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("DirX",     AnimatorControllerParameterType.Float);
        ctrl.AddParameter("DirY",     AnimatorControllerParameterType.Float);
        AssetDatabase.SaveAssets();
    }

    // ───────────────────────────────────────────────
    // MainMenu 씬
    // ───────────────────────────────────────────────

    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 카메라
        var cam = CreateCamera();

        // 캔버스
        var canvas = CreateCanvas();

        // 배경
        CreateBackground(canvas.transform, "Assets/bg_01_main_menu.png");

        // 타이틀
        var titleGO = new GameObject("TitleGroup");
        titleGO.transform.SetParent(canvas.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        AnchorCenter(titleRT, 800, 300, 0, 100);

        var title = MakeTMP(titleGO.transform, "TitleText", "FAKE SOLDIER", 72, Color.white);
        AnchorCenter(title.rectTransform, 800, 120, 0, 80);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;

        var sub = MakeTMP(titleGO.transform, "SubtitleText", "1980년 5월 광주", 28, new Color(0.7f, 0.7f, 0.7f));
        AnchorCenter(sub.rectTransform, 600, 50, 0, 0);
        sub.alignment = TextAlignmentOptions.Center;

        // 버튼 그룹
        var btnGroup = new GameObject("ButtonGroup");
        btnGroup.transform.SetParent(canvas.transform, false);
        var btnGroupRT = btnGroup.AddComponent<RectTransform>();
        AnchorCenter(btnGroupRT, 300, 160, 0, -120);

        var startBtn = CreateButton(btnGroup.transform, "StartButton", "시 작 하 기", 30, new Color(0.15f, 0.15f, 0.15f, 0.9f));
        AnchorCenter(startBtn.GetComponent<RectTransform>(), 280, 60, 0, 50);

        var quitBtn = CreateButton(btnGroup.transform, "QuitButton", "종 료", 24, new Color(0.1f, 0.1f, 0.1f, 0.9f));
        AnchorCenter(quitBtn.GetComponent<RectTransform>(), 280, 50, 0, -20);

        // GameManager
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // MainMenuManager
        var mmGO = new GameObject("MainMenuManager");
        var mm = mmGO.AddComponent<MainMenuManager>();
        SetRef(mm, "startButton", startBtn.GetComponent<Button>());
        SetRef(mm, "quitButton",  quitBtn.GetComponent<Button>());

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/MainMenu.unity");
    }

    // ───────────────────────────────────────────────
    // Stage 씬 (공통 구조)
    // ───────────────────────────────────────────────

    static void CreateStageScene(int stageNum, string sceneName, string stageTitle, string bgPath, StageData data)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        var canvas = CreateCanvas();

        // 배경
        CreateBackground(canvas.transform, bgPath);

        // 스테이지 타이틀 (좌상단)
        var stageTitleTMP = MakeTMP(canvas.transform, "StageTitle", stageTitle, 18, new Color(0.8f, 0.8f, 0.8f));
        stageTitleTMP.rectTransform.anchorMin = new Vector2(0, 1);
        stageTitleTMP.rectTransform.anchorMax = new Vector2(0, 1);
        stageTitleTMP.rectTransform.pivot     = new Vector2(0, 1);
        stageTitleTMP.rectTransform.anchoredPosition = new Vector2(16, -16);
        stageTitleTMP.rectTransform.sizeDelta = new Vector2(500, 40);

        // 대화 UI
        var (dlgPanel, speakerTMP, bodyTMP) = CreateDialogueUI(canvas.transform);

        // 선택지 UI
        var (choicePanel, choiceLabels, timerTMP) = CreateChoiceUI(canvas.transform);

        // GameManager
        var gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();

        // DialogueManager
        var dmGO = new GameObject("DialogueManager");
        var dm   = dmGO.AddComponent<DialogueManager>();
        SetRef(dm, "dialoguePanel", dlgPanel);
        SetRef(dm, "speakerText",   speakerTMP);
        SetRef(dm, "bodyText",      bodyTMP);

        // ChoiceSystem
        var csGO = new GameObject("ChoiceSystem");
        var cs   = csGO.AddComponent<ChoiceSystem>();
        SetRef(cs, "choicePanel", choicePanel);
        SetRef(cs, "timerText",   timerTMP);
        var so = new SerializedObject(cs);
        var labelsProp = so.FindProperty("choiceLabels");
        labelsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            labelsProp.GetArrayElementAtIndex(i).objectReferenceValue = choiceLabels[i];
        so.ApplyModifiedProperties();

        // Player
        var player = CreatePlayer();

        // EventPoint
        var eventPoint = new GameObject("EventPoint");
        eventPoint.transform.position = new Vector3(2f, 0f, 0f);
        var col = eventPoint.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 4f);

        var sd = eventPoint.AddComponent<StageDirector>();
        var sdSO = new SerializedObject(sd);
        sdSO.FindProperty("stageNumber").intValue = stageNum;
        sdSO.FindProperty("useTimer").boolValue   = data.useTimer;
        sdSO.FindProperty("autoLoadNext").boolValue = true;
        SetDialogueArray(sdSO, "preDialogue",  data.preDialogue);
        SetStringArray(sdSO,   "choiceTexts",  data.choiceTexts);
        SetChoiceTypeArray(sdSO, "choiceTypes", data.choiceTypes);
        SetDialogueArray(sdSO, "postDialogue", new DialogueLine[0]);
        sdSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/{sceneName}.unity");
    }

    // ───────────────────────────────────────────────
    // Ending 씬
    // ───────────────────────────────────────────────

    static void CreateEndingScene(string sceneName, string title, string body, string bgPath)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        var canvas = CreateCanvas();

        // 배경 (어둡게)
        CreateBackground(canvas.transform, bgPath, new Color(0.3f, 0.3f, 0.3f, 1f));

        // 타이틀
        var titleTMP = MakeTMP(canvas.transform, "EndingTitle", title, 48, Color.white);
        titleTMP.rectTransform.anchorMin = new Vector2(0, 1);
        titleTMP.rectTransform.anchorMax = new Vector2(1, 1);
        titleTMP.rectTransform.pivot     = new Vector2(0.5f, 1);
        titleTMP.rectTransform.anchoredPosition = new Vector2(0, -80);
        titleTMP.rectTransform.sizeDelta = new Vector2(0, 70);
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        // 본문
        var bodyTMP = MakeTMP(canvas.transform, "EndingBody", "", 22, new Color(0.9f, 0.9f, 0.9f));
        bodyTMP.rectTransform.anchorMin = new Vector2(0.1f, 0.25f);
        bodyTMP.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        bodyTMP.rectTransform.offsetMin = Vector2.zero;
        bodyTMP.rectTransform.offsetMax = Vector2.zero;
        bodyTMP.alignment = TextAlignmentOptions.Center;
        bodyTMP.enableWordWrapping = true;

        // 프롬프트
        var promptTMP = MakeTMP(canvas.transform, "PromptText", "[ Space ] 계속", 20, new Color(0.6f, 0.6f, 0.6f));
        promptTMP.rectTransform.anchorMin = new Vector2(0, 0);
        promptTMP.rectTransform.anchorMax = new Vector2(1, 0);
        promptTMP.rectTransform.pivot     = new Vector2(0.5f, 0);
        promptTMP.rectTransform.anchoredPosition = new Vector2(0, 30);
        promptTMP.rectTransform.sizeDelta = new Vector2(0, 40);
        promptTMP.alignment = TextAlignmentOptions.Center;

        // EndingManager
        var emGO = new GameObject("EndingManager");
        var em   = emGO.AddComponent<EndingManager>();
        SetRef(em, "titleText",  titleTMP);
        SetRef(em, "bodyText",   bodyTMP);
        SetRef(em, "promptText", promptTMP);
        var emSO = new SerializedObject(em);
        emSO.FindProperty("titleContent").stringValue = title;
        emSO.FindProperty("bodyContent").stringValue  = body;
        emSO.FindProperty("nextScene").stringValue    = "Credit";
        emSO.FindProperty("charDelay").floatValue     = 0.03f;
        emSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/{sceneName}.unity");
    }

    // ───────────────────────────────────────────────
    // Credit 씬
    // ───────────────────────────────────────────────

    static void CreateCreditScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera(Color.black);
        var canvas = CreateCanvas();

        // 검정 배경
        var bg = new GameObject("Background");
        bg.transform.SetParent(canvas.transform, false);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;
        Stretch(bg.GetComponent<RectTransform>());

        // 스크롤 마스크 영역
        var maskGO = new GameObject("ScrollArea");
        maskGO.transform.SetParent(canvas.transform, false);
        maskGO.AddComponent<RectMask2D>();
        Stretch(maskGO.GetComponent<RectTransform>());

        // 크레딧 컨테이너 (ScrollArea 자식)
        var containerGO = new GameObject("CreditContainer");
        containerGO.transform.SetParent(maskGO.transform, false);
        var containerRT = containerGO.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0);
        containerRT.anchorMax = new Vector2(0.5f, 0);
        containerRT.pivot = new Vector2(0.5f, 0);
        containerRT.sizeDelta = new Vector2(800, 1200);

        // 크레딧 텍스트
        var creditTMP = MakeTMP(containerGO.transform, "CreditText", CreditText(), 24, Color.white);
        creditTMP.rectTransform.anchorMin = Vector2.zero;
        creditTMP.rectTransform.anchorMax = Vector2.one;
        creditTMP.rectTransform.offsetMin = Vector2.zero;
        creditTMP.rectTransform.offsetMax = Vector2.zero;
        creditTMP.alignment = TextAlignmentOptions.Center;
        creditTMP.enableWordWrapping = true;
        creditTMP.lineSpacing = 8;

        // CreditManager
        var cmGO = new GameObject("CreditManager");
        var cm   = cmGO.AddComponent<CreditManager>();
        SetRef(cm, "creditContainer", containerRT);
        var cmSO = new SerializedObject(cm);
        cmSO.FindProperty("scrollSpeed").floatValue  = 80f;
        cmSO.FindProperty("endHoldTime").floatValue  = 4f;
        cmSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/Credit.unity");
    }

    // ───────────────────────────────────────────────
    // Build Settings 업데이트
    // ───────────────────────────────────────────────

    static void UpdateBuildSettings()
    {
        string[] sceneNames = {
            "MainMenu",
            "Stage_01", "Stage_02", "Stage_03", "Stage_04", "Stage_05",
            "Ending_Bad", "Ending_Normal", "Ending_True",
            "Credit"
        };

        var scenes = new List<EditorBuildSettingsScene>();
        foreach (var name in sceneNames)
        {
            string path = $"{SCENES_PATH}/{name}.unity";
            if (File.Exists(path))
                scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ───────────────────────────────────────────────
    // UI 생성 헬퍼
    // ───────────────────────────────────────────────

    static Camera CreateCamera(Color? bgColor = null)
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = bgColor ?? new Color(0.1f, 0.1f, 0.1f, 1f);
        cam.depth = -1;
        camGO.AddComponent<AudioListener>();
        return cam;
    }

    static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static void CreateBackground(Transform parent, string spritePath, Color? tint = null)
    {
        var go = new GameObject("Background");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = tint ?? Color.white;

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
        }
        Stretch(go.GetComponent<RectTransform>());
    }

    static (GameObject panel, TextMeshProUGUI speaker, TextMeshProUGUI body) CreateDialogueUI(Transform parent)
    {
        // 대화창 패널 (화면 하단)
        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(parent, false);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.85f);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 0);
        panelRT.anchorMax = new Vector2(1, 0);
        panelRT.pivot = new Vector2(0.5f, 0);
        panelRT.offsetMin = new Vector2(20, 10);
        panelRT.offsetMax = new Vector2(-20, 220);

        // 화자 이름
        var speakerTMP = MakeTMP(panelGO.transform, "SpeakerText", "", 22, new Color(1f, 0.85f, 0.3f));
        speakerTMP.rectTransform.anchorMin = new Vector2(0, 1);
        speakerTMP.rectTransform.anchorMax = new Vector2(1, 1);
        speakerTMP.rectTransform.pivot = new Vector2(0, 1);
        speakerTMP.rectTransform.anchoredPosition = new Vector2(20, -10);
        speakerTMP.rectTransform.sizeDelta = new Vector2(-40, 36);
        speakerTMP.fontStyle = FontStyles.Bold;

        // 대사 본문
        var bodyTMP = MakeTMP(panelGO.transform, "BodyText", "", 20, Color.white);
        bodyTMP.rectTransform.anchorMin = new Vector2(0, 0);
        bodyTMP.rectTransform.anchorMax = new Vector2(1, 1);
        bodyTMP.rectTransform.offsetMin = new Vector2(20, 10);
        bodyTMP.rectTransform.offsetMax = new Vector2(-20, -50);
        bodyTMP.alignment = TextAlignmentOptions.TopLeft;
        bodyTMP.enableWordWrapping = true;

        panelGO.SetActive(false);
        return (panelGO, speakerTMP, bodyTMP);
    }

    static (GameObject panel, TextMeshProUGUI[] labels, TextMeshProUGUI timer) CreateChoiceUI(Transform parent)
    {
        // 선택지 패널 (화면 중앙 하단)
        var panelGO = new GameObject("ChoicePanel");
        panelGO.transform.SetParent(parent, false);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.9f);
        var panelRT = panelGO.GetComponent<RectTransform>();
        AnchorCenter(panelRT, 700, 240, 0, -150);

        // 타이머 텍스트 (상단)
        var timerTMP = MakeTMP(panelGO.transform, "TimerText", "10", 36, new Color(1f, 0.3f, 0.3f));
        timerTMP.rectTransform.anchorMin = new Vector2(0.5f, 1);
        timerTMP.rectTransform.anchorMax = new Vector2(0.5f, 1);
        timerTMP.rectTransform.pivot = new Vector2(0.5f, 1);
        timerTMP.rectTransform.anchoredPosition = new Vector2(0, -8);
        timerTMP.rectTransform.sizeDelta = new Vector2(100, 50);
        timerTMP.alignment = TextAlignmentOptions.Center;
        timerTMP.fontStyle = FontStyles.Bold;
        timerTMP.gameObject.SetActive(false);

        // 선택지 라벨 3개
        var labels = new TextMeshProUGUI[3];
        for (int i = 0; i < 3; i++)
        {
            var lbl = MakeTMP(panelGO.transform, $"ChoiceLabel_{i + 1}", "", 22, Color.white);
            float yOffset = 80 - i * 60;
            lbl.rectTransform.anchorMin = new Vector2(0, 0.5f);
            lbl.rectTransform.anchorMax = new Vector2(1, 0.5f);
            lbl.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            lbl.rectTransform.anchoredPosition = new Vector2(0, yOffset);
            lbl.rectTransform.sizeDelta = new Vector2(-40, 50);
            lbl.alignment = TextAlignmentOptions.Left;
            lbl.enableWordWrapping = true;
            labels[i] = lbl;
        }

        panelGO.SetActive(false);
        return (panelGO, labels, timerTMP);
    }

    static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-3f, 0f, 0f);

        player.AddComponent<SpriteRenderer>();
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.5f);

        var animator = player.AddComponent<Animator>();
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIMATOR_PATH);
        if (ctrl != null) animator.runtimeAnimatorController = ctrl;

        player.AddComponent<PlayerController>();
        return player;
    }

    // ───────────────────────────────────────────────
    // RectTransform 헬퍼
    // ───────────────────────────────────────────────

    static void Stretch(RectTransform rt, float l = 0, float r = 0, float b = 0, float t = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    static void AnchorCenter(RectTransform rt, float w, float h, float ox = 0, float oy = 0)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(ox, oy);
        rt.sizeDelta = new Vector2(w, h);
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        return tmp;
    }

    static GameObject CreateButton(Transform parent, string name, string label, int fontSize, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f);
        colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f);
        btn.colors = colors;

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return go;
    }

    // ───────────────────────────────────────────────
    // SerializedObject 헬퍼
    // ───────────────────────────────────────────────

    static void SetRef(Component comp, string prop, Object target)
    {
        var so = new SerializedObject(comp);
        so.FindProperty(prop).objectReferenceValue = target;
        so.ApplyModifiedProperties();
    }

    static void SetDialogueArray(SerializedObject so, string propName, DialogueLine[] lines)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("speaker").stringValue = lines[i].speaker;
            elem.FindPropertyRelative("body").stringValue    = lines[i].body;
        }
    }

    static void SetStringArray(SerializedObject so, string propName, string[] values)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).stringValue = values[i];
    }

    static void SetChoiceTypeArray(SerializedObject so, string propName, GameManager.ChoiceType[] types)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = types.Length;
        for (int i = 0; i < types.Length; i++)
            prop.GetArrayElementAtIndex(i).enumValueIndex = (int)types[i];
    }

    // ───────────────────────────────────────────────
    // 스테이지 데이터 구조체
    // ───────────────────────────────────────────────

    struct StageData
    {
        public DialogueLine[]          preDialogue;
        public string[]                choiceTexts;
        public GameManager.ChoiceType[] choiceTypes;
        public bool                    useTimer;
    }

    static DialogueLine L(string speaker, string body) => new DialogueLine { speaker = speaker, body = body };

    // ───────────────────────────────────────────────
    // 스테이지 1 — 5월 18일 전남대 앞
    // ───────────────────────────────────────────────

    static StageData Stage1Data() => new StageData
    {
        preDialogue = new[] {
            L("상관", "집합! 전남대 정문 앞에 학생 시위대가 집결했다."),
            L("상관", "명령이다. 즉시 해산시켜라. 필요하면 진압봉을 써도 좋다."),
            L("나레이션", "학생들의 눈빛이 두렵다. 그들도 나와 비슷한 나이다."),
        },
        choiceTexts = new[] {
            "진압봉을 들고 학생들을 향해 돌진한다",
            "뒤에 서서 위협적인 자세만 취한다",
            "조용히 뒤로 물러서며 진압을 거부한다",
        },
        choiceTypes = new[] {
            GameManager.ChoiceType.FullObey,
            GameManager.ChoiceType.Passive,
            GameManager.ChoiceType.ActiveRefusal,
        },
        useTimer = false,
    };

    // ───────────────────────────────────────────────
    // 스테이지 2 — 5월 19일 골목
    // ───────────────────────────────────────────────

    static StageData Stage2Data() => new StageData
    {
        preDialogue = new[] {
            L("나레이션", "골목을 순찰하던 중, 누군가 쓰러져 있는 것을 발견했다."),
            L("시민",    "으... 살려주세요. 저는... 그냥 구경하다가 다쳤어요."),
            L("나레이션", "그의 다리에는 진압 과정에서 생긴 상처가 보인다."),
        },
        choiceTexts = new[] {
            "상관에게 신고한다 (시민을 체포시킨다)",
            "못 본 척 지나친다",
            "부상자를 숨겨주고 치료를 돕는다",
        },
        choiceTypes = new[] {
            GameManager.ChoiceType.FullObey,
            GameManager.ChoiceType.Passive,
            GameManager.ChoiceType.ActiveRefusal,
        },
        useTimer = false,
    };

    // ───────────────────────────────────────────────
    // 스테이지 3 — 5월 20일 야간 검문
    // ───────────────────────────────────────────────

    static StageData Stage3Data() => new StageData
    {
        preDialogue = new[] {
            L("상관",    "검문소를 지켜라. 시민군 트럭이 지나갈 수 있다. 절대 통과시키지 마라."),
            L("운전자",  "제발... 부상자가 있습니다. 병원에 데려가야 합니다. 비켜주세요."),
            L("나레이션", "트럭 짐칸에서 신음 소리가 들린다."),
        },
        choiceTexts = new[] {
            "트럭을 막고 탑승자들을 체포한다",
            "확인만 하고 슬쩍 보내준다",
            "길을 비켜주며 조용히 통과시킨다",
        },
        choiceTypes = new[] {
            GameManager.ChoiceType.FullObey,
            GameManager.ChoiceType.MinorRefusal,
            GameManager.ChoiceType.ActiveRefusal,
        },
        useTimer = false,
    };

    // ───────────────────────────────────────────────
    // 스테이지 4 — 5월 21일 도청 앞 (핵심, 10초 타이머)
    // ───────────────────────────────────────────────

    static StageData Stage4Data() => new StageData
    {
        preDialogue = new[] {
            L("상관",    "전 병사 도열! 발포 명령이 내려졌다!"),
            L("상관",    "도청 앞 군중을 향해 발포하라! 이것은 명령이다!"),
            L("나레이션", "군중 속에 여자도, 아이도 있다. 10초 안에 결정해야 한다."),
        },
        choiceTexts = new[] {
            "명령에 따라 군중을 향해 발포한다",
            "허공을 향해 공포탄을 쏜다",
            "총을 내리고 발포를 거부한다",
        },
        choiceTypes = new[] {
            GameManager.ChoiceType.FullObey,
            GameManager.ChoiceType.MinorRefusal,
            GameManager.ChoiceType.KeyRefusal,
        },
        useTimer = true,
    };

    // ───────────────────────────────────────────────
    // 스테이지 5 — 5월 27일 도청 내부
    // ───────────────────────────────────────────────

    static StageData Stage5Data() => new StageData
    {
        preDialogue = new[] {
            L("상관",    "마지막이다. 도청 안에 아직 저항하는 자들이 있다."),
            L("상관",    "들어가서 모두 제압하라."),
            L("나레이션", "문을 열면 그곳에 몇몇 청년들이 있다. 눈이 마주쳤다."),
        },
        choiceTexts = new[] {
            "명령에 따라 진압에 참여한다",
            "자리를 지키며 교전을 피한다",
            "무기를 내려놓고 도청을 나간다",
        },
        choiceTypes = new[] {
            GameManager.ChoiceType.FullObey,
            GameManager.ChoiceType.Passive,
            GameManager.ChoiceType.ActiveRefusal,
        },
        useTimer = false,
    };

    // ───────────────────────────────────────────────
    // 엔딩 텍스트
    // ───────────────────────────────────────────────

    static string BadEndingBody() =>
@"1980년 5월 27일, 진압 작전은 완료되었다.

당신은 모든 명령을 충실히 이행했다.

2주 후, 부대 앞에서 표창장 수여식이 열렸다.
사진을 찍었다. 박수 소리가 울렸다.

당신은 그 박수 소리가 평생 귓가에서 떠나지 않을 것이다.

─────────────────────────
  5·18 민주화운동 공식 통계
─────────────────────────
  사망        165명
  행방불명     65명
  부상      3,383명
  구금      1,394명
─────────────────────────

그들은 단지 자유를 원했다.";

    static string NormalEndingBody() =>
@"1980년 5월 27일, 진압 작전은 완료되었다.

당신은 명령을 온전히 따르지 않았다.

군법회의가 열렸다.
당신은 재판을 받았고, 불명예 전역을 했다.

오랜 시간이 흘렀다.
당신은 여전히 그날 골목에서, 검문소에서,
마주쳤던 눈빛들을 기억한다.

선택하지 않는 것도 선택이었다.
방관도 결국 공모였다.";

    static string TrueEndingBody() =>
@"1980년 5월 21일.

당신은 총을 내렸다.

명령을 거부하는 순간,
당신은 가짜 군인이 아닌 진짜 인간이 되었다.

당신은 군복을 벗었다.
그리고 시민들 사이에 섞였다.

이름 없이, 기록 없이,
그러나 양심을 지킨 채.

그런 사람들이 있었다.
역사는 그들을 기억하지 못했지만,
그들이 있었기에 우리가 여기 있다.";

    static string CreditText() =>
@"FAKE SOLDIER

1980년 5월 광주,
그날을 기억하며.

────────────────────

개발팀

기획 · 개발  [이름]
개발         [이름]
아트         [이름]

────────────────────

광주소프트웨어마이스터고
교내 5·18 해커톤 2025

Made with Unity 6

────────────────────

5·18 민주화운동 희생자들의
명복을 빕니다.

당신의 선택이
역사를 바꿉니다.";
}
#endif

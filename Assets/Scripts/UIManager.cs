using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* ================= UI（全部代码生成：剧情/对话/信件/HUD/幕尾） ================= */
public class UIManager : MonoBehaviour
{
    public static UIManager I;
    public Canvas UICanvas => canvas;
    public Font UIFont => font;
    public void HideEnding() => endPanel.gameObject.SetActive(false);

    Canvas canvas;
    Font font;
    // HUD
    Text objective, statTruth, statTrust, statLetters, location, prompt, ctrlHint;
    RectTransform hud;
    // 对话
    RectTransform dlgPanel; Text dlgSpeaker, dlgText, dlgHint; RectTransform dlgChoices;
    // 信件
    RectTransform letterPanel; Text letterText, letterHint;
    // 开场
    RectTransform introPanel; Text introTitle, introSub, introText, introHint; Button introSkip;
    Button introContinue; Action introContinueCb;
    // 幕尾
    RectTransform endPanel; Text endTitle, endStats, endTeaser, endHint;
    // 淡入淡出
    Image fadeImg;

    // 对话状态
    List<Line> q; int idx; float ci; bool typing; Action dlgDone; List<Choice> choices; int sel;
    readonly List<Button> choiceBtns = new List<Button>();
    float dlgOpenedAt;
    // 信件
    Action letterCb; float letterAt;
    // 开场
    int slide = -1; float slideCi; bool slideTyping; Action introDone;
    // 结局
    Action endSpace, endR;

    public enum Mode { Intro, Play, Dialog, Letter, End, Transition }
    public Mode mode = Mode.Intro;

    static Font LoadFont()
    {
        var cjk = Resources.Load<Font>("Fonts/cjk");
        if (cjk) return cjk;
        try { var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); if (f) return f; } catch { }
        try { return Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { return null; }
    }

    void Awake()
    {
        I = this;
        font = LoadFont();
        var cgo = new GameObject("Canvas");
        cgo.transform.SetParent(transform, false);
        canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = cgo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960, 540);
        scaler.matchWidthOrHeight = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();
        BuildHUD(); BuildDialog(); BuildLetter(); BuildIntro(); BuildEnding(); BuildFade();
        // 点击推进：剧情 / 对话 / 信纸 / 幕尾
        AddPanelClick(introPanel, () => AdvanceIntro());
        AddPanelClick(dlgPanel, () => { if (choices == null) AdvanceDialog(); });
        AddPanelClick(letterPanel, () => { if (Time.time - letterAt > 0.3f) CloseLetter(); });
        AddPanelClick(endPanel, () => { if (endSpace != null) endSpace(); else endR?.Invoke(); });
    }

    /* ---------- 基础构件 ---------- */
    RectTransform Panel(string name, Transform parent, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = bg;
        return rt;
    }
    Text MkText(Transform parent, string name, int size, Color col, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        var t = go.AddComponent<Text>();
        t.font = font; t.fontSize = size; t.color = col; t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = false;
        return t;
    }
    void At(Text t, float ax, float ay, float bx, float by, float ox, float oy)
    {
        var rt = t.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay); rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(ox, oy); rt.offsetMax = Vector2.zero;
    }
    Button MkButton(Transform parent, string name, string label, int size, Color bg, Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => { suppressPanelClick = true; onClick(); });
        var t = MkText(go.transform, "Label", size, new Color(0.78f, 0.82f, 0.86f), TextAnchor.MiddleCenter);
        t.text = label;
        At(t, 0, 0, 1, 1, 0, 0);
        return btn;
    }

    // 面板点击推进（按钮点击优先，通过 suppressPanelClick 抑制穿透）
    static bool suppressPanelClick;
    void AddPanelClick(RectTransform panel, Action act)
    {
        var tg = panel.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var e = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        e.callback.AddListener(_ => { if (!suppressPanelClick) act(); });
        tg.triggers.Add(e);
    }

    /* ---------- HUD ---------- */
    void BuildHUD()
    {
        hud = Panel("HUD", canvas.transform, new Color(0, 0, 0, 0));
        hud.GetComponent<Image>().raycastTarget = false;
        objective = MkText(hud, "Objective", 15, new Color(0.62f, 0.69f, 0.75f), TextAnchor.UpperLeft);
        At(objective, 0.02f, 0.9f, 0.5f, 1f, 0, -10);
        statTruth = MkText(hud, "Truth", 14, new Color(0.69f, 0.53f, 0.6f), TextAnchor.UpperRight);
        At(statTruth, 0.72f, 0.94f, 0.98f, 1f, 0, -8);
        statTrust = MkText(hud, "Trust", 14, new Color(0.53f, 0.64f, 0.72f), TextAnchor.UpperRight);
        At(statTrust, 0.72f, 0.88f, 0.98f, 0.94f, 0, -8);
        statLetters = MkText(hud, "Letters", 14, new Color(0.78f, 0.72f, 0.48f), TextAnchor.UpperRight);
        At(statLetters, 0.72f, 0.82f, 0.98f, 0.88f, 0, -8);
        location = MkText(hud, "Location", 22, new Color(0.87f, 0.9f, 0.93f), TextAnchor.MiddleCenter);
        At(location, 0.2f, 0.82f, 0.8f, 0.92f, 0, 0);
        prompt = MkText(hud, "Prompt", 14, new Color(0.91f, 0.93f, 0.96f), TextAnchor.MiddleCenter);
        var prt = prompt.GetComponent<RectTransform>();
        var pbg = new GameObject("PromptBG");
        pbg.transform.SetParent(hud, false);
        var pr = pbg.AddComponent<RectTransform>();
        var pimg = pbg.AddComponent<Image>(); pimg.color = new Color(0.03f, 0.05f, 0.09f, 0.78f);
        prompt.transform.SetParent(pbg.transform, false);
        At(prompt, 0, 0, 1, 1, 0, 0);
        promptBg = pbg;
        ctrlHint = MkText(hud, "Ctrl", 11, new Color(0.36f, 0.43f, 0.5f), TextAnchor.MiddleCenter);
        At(ctrlHint, 0.2f, 0f, 0.8f, 0.05f, 0, 4);
        ctrlHint.text = "方向键 / WASD 移动 · E 调查 · R 重开（幕尾）";
        Prompt(null);
        hud.gameObject.SetActive(false);
    }
    GameObject promptBg;

    public void SetObjective(string s) => objective.text = s;
    public void SetStats()
    {
        statTruth.text = "真相度 " + GameState.truth;
        statTrust.text = "镇民信任 " + GameState.trust;
        statLetters.text = $"信件 {GameState.LetterCount()}/5";
    }
    public void ShowHUD(bool v) { hud.gameObject.SetActive(v); if (v) SetStats(); }
    public void ShowLocation(string s)
    {
        location.text = s;
        StopCoroutine("LocFade"); StartCoroutine("LocFade");
    }
    System.Collections.IEnumerator LocFade()
    {
        location.canvasRenderer.SetAlpha(0.95f);
        yield return new WaitForSeconds(2.2f);
        float a = 0.95f;
        while (a > 0) { a -= Time.deltaTime * 0.5f; location.canvasRenderer.SetAlpha(a); yield return null; }
    }
    public void Prompt(string s)
    {
        promptBg.SetActive(s != null);
        if (s != null)
        {
            prompt.text = "E · " + s;
            var rt = promptBg.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.72f);
            rt.sizeDelta = new Vector2(40 + s.Length * 22, 34);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    /* ---------- 对话框 ---------- */
    void BuildDialog()
    {
        dlgPanel = Panel("Dialog", canvas.transform, new Color(0.03f, 0.05f, 0.09f, 0.94f));
        var rt = dlgPanel;
        rt.anchorMin = new Vector2(0.04f, 0.03f); rt.anchorMax = new Vector2(0.96f, 0.3f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        dlgSpeaker = MkText(dlgPanel, "Speaker", 16, new Color(0.85f, 0.79f, 0.63f), TextAnchor.MiddleLeft);
        At(dlgSpeaker, 0.03f, 0.86f, 0.5f, 1.06f, 0, 0);
        dlgText = MkText(dlgPanel, "Text", 17, new Color(0.86f, 0.89f, 0.92f), TextAnchor.UpperLeft);
        At(dlgText, 0.05f, 0.18f, 0.95f, 0.84f, 0, 0);
        dlgHint = MkText(dlgPanel, "Hint", 11, new Color(0.33f, 0.4f, 0.48f), TextAnchor.MiddleRight);
        At(dlgHint, 0.8f, 0.02f, 0.97f, 0.16f, 0, 0);
        dlgHint.text = "空格 继续";
        var chgo = new GameObject("Choices");
        chgo.transform.SetParent(dlgPanel, false);
        dlgChoices = chgo.AddComponent<RectTransform>();
        var vlg = chgo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 6; vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
        var csf = chgo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        At2(dlgChoices, 0.05f, 0.1f, 0.95f, 0);
        dlgPanel.gameObject.SetActive(false);
    }
    void At2(RectTransform rt, float ax, float ay, float bx, float oy)
    {
        rt.anchorMin = new Vector2(ax, ay); rt.anchorMax = new Vector2(bx, ay);
        rt.offsetMin = Vector2.zero; rt.offsetMax = new Vector2(0, oy);
    }

    public void Say(List<Line> lines, Action done)
    {
        q = lines; idx = 0; dlgDone = done;
        choices = null; dlgOpenedAt = Time.time;
        mode = Mode.Dialog;
        dlgPanel.gameObject.SetActive(true);
        Prompt(null);
        StartLine();
    }
    void StartLine()
    {
        var L = q[idx];
        dlgSpeaker.text = L.sp ?? "";
        dlgSpeaker.gameObject.SetActive(L.sp != null);
        ci = 0; typing = true;
        dlgText.text = "";
        foreach (var b in choiceBtns) Destroy(b.gameObject);
        choiceBtns.Clear(); choices = null;
        dlgHint.gameObject.SetActive(true);
    }
    void FinishType()
    {
        typing = false;
        var L = q[idx];
        dlgText.text = L.tx;
        if (L.ch != null && L.ch.Count > 0)
        {
            choices = L.ch; sel = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                int k = i;
                var b = MkButton(dlgChoices, "ch" + i, choices[i].t, 15, new Color(0.1f, 0.14f, 0.2f, 0.9f), () => { sel = k; PickChoice(); });
                b.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
                choiceBtns.Add(b);
            }
            RefreshChoiceSel();
            dlgHint.gameObject.SetActive(false);
        }
    }
    void RefreshChoiceSel()
    {
        for (int i = 0; i < choiceBtns.Count; i++)
            choiceBtns[i].GetComponent<Image>().color = i == sel ? new Color(0.5f, 0.22f, 0.28f, 0.95f) : new Color(0.1f, 0.14f, 0.2f, 0.9f);
    }
    void PickChoice()
    {
        var c = choices[sel];
        choices = null;
        c.fn?.Invoke();
        if (c.then != null && c.then.Count > 0) Say(c.then, dlgDone);
        else EndDialog();
    }
    void AdvanceDialog()
    {
        if (typing) { ci = q[idx].tx.Length; FinishType(); return; }
        idx++;
        if (idx >= q.Count) EndDialog();
        else StartLine();
    }
    void EndDialog()
    {
        dlgPanel.gameObject.SetActive(false);
        mode = Mode.Play;
        var cb = dlgDone; dlgDone = null;
        cb?.Invoke();
    }

    /* ---------- 信纸 ---------- */
    void BuildLetter()
    {
        letterPanel = Panel("Letter", canvas.transform, new Color(0.02f, 0.02f, 0.04f, 0.75f));
        var paper = Panel("Paper", letterPanel, new Color(0.85f, 0.8f, 0.66f, 1f));
        paper.anchorMin = new Vector2(0.2f, 0.12f); paper.anchorMax = new Vector2(0.8f, 0.9f);
        paper.offsetMin = paper.offsetMax = Vector2.zero;
        letterText = MkText(paper, "Text", 17, new Color(0.18f, 0.14f, 0.09f), TextAnchor.UpperLeft);
        At(letterText, 0.08f, 0.08f, 0.92f, 0.85f, 0, 0);
        letterHint = MkText(paper, "Hint", 11, new Color(0.48f, 0.42f, 0.31f), TextAnchor.MiddleRight);
        At(letterHint, 0.6f, 0.02f, 0.92f, 0.08f, 0, 0);
        letterHint.text = "空格 / E 合上信纸";
        letterPanel.gameObject.SetActive(false);
    }
    public void OpenLetter(string text, Action cb)
    {
        mode = Mode.Letter; letterAt = Time.time; letterCb = cb;
        letterText.text = text;
        letterPanel.gameObject.SetActive(true);
    }
    void CloseLetter()
    {
        letterPanel.gameObject.SetActive(false);
        mode = Mode.Play;
        var cb = letterCb; letterCb = null;
        cb?.Invoke();
    }

    /* ---------- 开场剧情 ---------- */
    void BuildIntro()
    {
        introPanel = Panel("Intro", canvas.transform, new Color(0.02f, 0.03f, 0.05f, 0.55f));
        introTitle = MkText(introPanel, "Title", 46, new Color(0.87f, 0.9f, 0.93f), TextAnchor.MiddleCenter);
        At(introTitle, 0.1f, 0.72f, 0.9f, 0.92f, 0, 0);
        introTitle.text = "雾 港 疑 云";
        introSub = MkText(introPanel, "Sub", 16, new Color(0.54f, 0.18f, 0.18f), TextAnchor.MiddleCenter);
        At(introSub, 0.1f, 0.66f, 0.9f, 0.74f, 0, 0);
        introText = MkText(introPanel, "Text", 17, new Color(0.73f, 0.78f, 0.82f), TextAnchor.UpperCenter);
        At(introText, 0.15f, 0.18f, 0.85f, 0.62f, 0, 0);
        introHint = MkText(introPanel, "Hint", 11, new Color(0.33f, 0.4f, 0.48f), TextAnchor.MiddleCenter);
        At(introHint, 0.3f, 0.04f, 0.7f, 0.09f, 0, 0);
        introHint.text = "空格 继续";
        introSkip = MkButton(introPanel, "Skip", "跳过剧情 ▸", 13, new Color(0.04f, 0.06f, 0.1f, 0.7f), () => FinishIntro());
        var srt = introSkip.GetComponent<RectTransform>();
        srt.anchorMin = srt.anchorMax = new Vector2(1, 1);
        srt.sizeDelta = new Vector2(130, 36);
        srt.anchoredPosition = new Vector2(-24, -20);
        introSub.text = "序 章";
        introContinue = MkButton(introPanel, "Continue", "▸ 继续游戏（从存档点开始）", 13, new Color(0.1f, 0.16f, 0.24f, 0.85f),
            () => FinishIntroWith(introContinueCb));
        var crt = introContinue.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = new Vector2(0, 0);
        crt.sizeDelta = new Vector2(220, 36);
        crt.anchoredPosition = new Vector2(24, 20);
        introContinue.gameObject.SetActive(false);
    }
    public void StartIntro(Action done, Action onContinue = null)
    {
        introDone = done;
        introContinueCb = onContinue;
        introContinue.gameObject.SetActive(onContinue != null);
        mode = Mode.Intro;
        introPanel.gameObject.SetActive(true);
        slide = -1; slideTyping = false;
        introText.text = "";
    }
    void AdvanceIntro()
    {
        if (slideTyping)
        {
            slideCi = StoryContent.Slides[slide].Length;
            introText.text = StoryContent.Slides[slide];
            slideTyping = false;
            return;
        }
        slide++;
        if (slide >= StoryContent.Slides.Length) { FinishIntro(); return; }
        if (slide == 0) { introTitle.canvasRenderer.SetAlpha(0.14f); introSub.canvasRenderer.SetAlpha(0.14f); }
        slideCi = 0; slideTyping = true;
        introText.text = "";
    }
    void FinishIntro() => FinishIntroWith(introDone);
    void FinishIntroWith(Action cb)
    {
        if (mode != Mode.Intro) return;
        introPanel.gameObject.SetActive(false);
        introDone = null; introContinueCb = null;
        cb?.Invoke();
    }

    /* ---------- 幕尾/结局 ---------- */
    void BuildEnding()
    {
        endPanel = Panel("Ending", canvas.transform, new Color(0.02f, 0.02f, 0.04f, 1f));
        endTitle = MkText(endPanel, "Title", 30, new Color(0.87f, 0.9f, 0.93f), TextAnchor.MiddleCenter);
        At(endTitle, 0.1f, 0.68f, 0.9f, 0.85f, 0, 0);
        endStats = MkText(endPanel, "Stats", 14, new Color(0.49f, 0.56f, 0.63f), TextAnchor.MiddleCenter);
        At(endStats, 0.1f, 0.52f, 0.9f, 0.66f, 0, 0);
        endTeaser = MkText(endPanel, "Teaser", 16, new Color(0.59f, 0.31f, 0.36f), TextAnchor.MiddleCenter);
        At(endTeaser, 0.1f, 0.2f, 0.9f, 0.5f, 0, 0);
        endHint = MkText(endPanel, "Hint", 11, new Color(0.27f, 0.34f, 0.42f), TextAnchor.MiddleCenter);
        At(endHint, 0.3f, 0.06f, 0.7f, 0.12f, 0, 0);
        endPanel.gameObject.SetActive(false);
    }
    public void ShowEnding(string title, string stats, string teaser, string hint, Action onSpace, Action onR)
    {
        mode = Mode.End;
        endTitle.text = title; endStats.text = stats; endTeaser.text = teaser; endHint.text = hint;
        endSpace = onSpace; endR = onR;
        endPanel.gameObject.SetActive(true);
    }

    /* ---------- 淡入淡出 ---------- */
    void BuildFade()
    {
        var go = new GameObject("Fade");
        go.transform.SetParent(canvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        fadeImg = go.AddComponent<Image>();
        fadeImg.color = new Color(0.01f, 0.02f, 0.04f, 1f);
        fadeImg.raycastTarget = false;
        fadeImg.canvasRenderer.SetAlpha(1f);
    }
    public void FadeTo(Action cb)
    {
        mode = Mode.Transition;
        StartCoroutine(FadeCo(cb));
    }
    System.Collections.IEnumerator FadeCo(Action cb)
    {
        float a = 0;
        fadeImg.canvasRenderer.SetAlpha(1f);
        yield return new WaitForSeconds(0.55f);
        cb?.Invoke();
        while (a < 1) { a += Time.deltaTime * 1.2f; fadeImg.canvasRenderer.SetAlpha(1f - a); yield return null; }
        fadeImg.canvasRenderer.SetAlpha(0f);
        if (mode == Mode.Transition) mode = Mode.Play;
    }
    public void FadeInFromBlack() { StartCoroutine(FadeCo(null)); }

    /* ---------- 输入路由 ---------- */
    public void Tick(float dt, bool advance, bool up, bool down)
    {
        switch (mode)
        {
            case Mode.Intro:
                if (slideTyping)
                {
                    var s = StoryContent.Slides[slide];
                    slideCi += dt * 22;
                    if (slideCi >= s.Length) { slideCi = s.Length; slideTyping = false; }
                    introText.text = s.Substring(0, (int)slideCi);
                }
                if (advance) AdvanceIntro();
                break;
            case Mode.Dialog:
                if (typing)
                {
                    var L = q[idx];
                    ci += dt * 26;
                    if (ci >= L.tx.Length) { ci = L.tx.Length; FinishType(); }
                    else dlgText.text = L.tx.Substring(0, (int)ci);
                }
                if (Time.time - dlgOpenedAt < 0.18f) break;
                if (choices != null)
                {
                    if (up) { sel = (sel - 1 + choices.Count) % choices.Count; RefreshChoiceSel(); }
                    if (down) { sel = (sel + 1) % choices.Count; RefreshChoiceSel(); }
                    if (advance) PickChoice();
                }
                else if (advance) AdvanceDialog();
                break;
            case Mode.Letter:
                if (advance && Time.time - letterAt > 0.3f) CloseLetter();
                break;
            case Mode.End:
                if (advance) endSpace?.Invoke();
                break;
        }
        suppressPanelClick = false;
    }
    public void OnEscape()
    {
        if (mode == Mode.Intro) FinishIntro();
        else if (mode == Mode.Letter && Time.time - letterAt > 0.3f) CloseLetter();
    }
    public void OnR() { if (mode == Mode.End) endR?.Invoke(); }
}

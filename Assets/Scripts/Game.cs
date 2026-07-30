using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ================= 主逻辑（房间装配/交互/三幕流程/四结局） ================= */
public partial class Game : MonoBehaviour
{
    public static float CamX; // 相机左缘（单位）

    Dictionary<string, RoomDef> defs;
    readonly Dictionary<string, GameObject> roomGO = new Dictionary<string, GameObject>();
    readonly Dictionary<string, RoomRT> rts = new Dictionary<string, RoomRT>();
    RoomDef room;
    PlayerController player;
    UIManager ui;
    Camera cam;
    WeatherFX weather;
    float camPx;
    ItemDef nearItem;
    readonly HashSet<string> visited = new HashSet<string>();
    bool prevTouchE, prevTouchUp, prevTouchDown;

    class RoomRT
    {
        public Transform farTf, midTf, frontTf;
        public GameObject watcher, moon, rope, cutRope, shopGlow;
        public readonly List<(string id, GameObject go)> frags = new List<(string, GameObject)>();
    }

    void Awake()
    {
        var camGo = new GameObject("Main Camera");
        cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 1.35f;
        cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioFX>();
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        ui = gameObject.AddComponent<UIManager>();
        defs = RoomData.Build();
        var roomsRoot = new GameObject("Rooms").transform;
        foreach (var kv in defs) BuildRoom(kv.Value, roomsRoot);
        player = PlayerController.Create(transform);
        weather = WeatherFX.Create(transform);
        TouchUI.Create(ui.UICanvas, ui.UIFont);
        ui.StartIntro(StartGame, SaveSystem.HasSave ? ContinueGame : (Action)null);
        ui.FadeInFromBlack();
    }

    /* ---------------- 房间装配 ---------------- */
    void BuildRoom(RoomDef def, Transform parent)
    {
        var root = new GameObject(def.id);
        root.transform.SetParent(parent, false);
        var rt = new RoomRT();
        rts[def.id] = rt;
        rt.farTf = AddLayer(root, def.id + "_far", -30);
        AddLayer(root, def.id + "_mid", -20);
        AddLayer(root, def.id + "_back", -10);
        rt.frontTf = AddLayer(root, def.id + "_front", 300);
        rt.midTf = root.transform.Find(def.id + "_mid");

        foreach (var b in def.beams) BeamFX.Create(root.transform, b.x, b.y, b.scale, b.len, def.id == "cape" ? -5 : -25);
        foreach (var l in def.lamps) LampFX.Create(root.transform, l.x, l.flicker);
        foreach (var b in def.boats) BoatFX.Create(root.transform, b.x, b.y, b.phase);

        if (def.shopWindow)
        {
            var g = new GameObject("shopGlow");
            g.transform.SetParent(root.transform, false);
            g.transform.position = U.P(342, 160);
            var sr = g.AddComponent<SpriteRenderer>();
            sr.sprite = U.Spr("FX/glow"); sr.sortingOrder = 11;
            sr.color = new Color(1f, 0.78f, 0.45f, 0.4f);
            g.transform.localScale = new Vector3(1.6f, 1.2f, 1);
            rt.shopGlow = g;
        }
        if (def.ropeGate)
        {
            var rope = new GameObject("rope");
            rope.transform.SetParent(root.transform, false);
            U.RectProp(rope.transform, "r", 1070, 192, 60, 8, new Color(0.35f, 0.29f, 0.19f), 11);
            U.RectProp(rope.transform, "p1", 1042, 197, 4, 22, new Color(0.18f, 0.14f, 0.09f), 11);
            U.RectProp(rope.transform, "p2", 1098, 197, 4, 22, new Color(0.18f, 0.14f, 0.09f), 11);
            rt.rope = rope;
            var cut = new GameObject("cutRope");
            cut.transform.SetParent(root.transform, false);
            U.RectProp(cut.transform, "c1", 1061, 214, 18, 3, new Color(0.35f, 0.29f, 0.19f), 11);
            U.RectProp(cut.transform, "c2", 1086, 218, 12, 2, new Color(0.35f, 0.29f, 0.19f), 11);
            cut.SetActive(false);
            rt.cutRope = cut;
        }
        if (def.frags)
        {
            foreach (var it in def.items)
            {
                if (!it.id.StartsWith("frag")) continue;
                var p = U.RectProp(root.transform, it.id, it.x, it.y + 8, 7, 9, new Color(0.77f, 0.72f, 0.59f), 11);
                U.RectProp(p.transform, "hi", 0, 0, 7, 2, new Color(0.85f, 0.8f, 0.66f), 12).transform.localPosition = new Vector3(0, 0.035f, 0);
                rt.frags.Add((it.id, p));
            }
        }
        if (def.watcher)
        {
            bool isTop = def.id == "top";
            rt.watcher = WatcherFX.Create(root.transform, isTop ? 352 : 830, isTop ? 146 : 200).gameObject;
        }
        if (def.flame) FlameFX.Create(root.transform, 240, 140);
        if (def.moon)
        {
            var m = new GameObject("moonHolder");
            m.transform.SetParent(root.transform, false);
            MoonFX.Create(m.transform, 380, 42);
            rt.moon = m;
        }
        foreach (var n in def.npcs) NPCView.Create(root.transform, n);
        root.SetActive(false);
        roomGO[def.id] = root;
    }

    Transform AddLayer(GameObject root, string name, int order)
    {
        var spr = U.Spr("Rooms/" + name);
        if (spr == null) return null;
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        go.transform.position = Vector3.zero;
        return go.transform;
    }

    /* ---------------- 房间加载与流程 ---------------- */
    void LoadRoom(string id, float sx, float sy, string sdir)
    {
        if (room != null) roomGO[room.id].SetActive(false);
        room = defs[id];
        roomGO[id].SetActive(true);
        player.SetRoom(room);
        player.Place(sx, sy, sdir);
        camPx = Mathf.Clamp(sx - 240, 0, Mathf.Max(0, room.w - 480));
        ui.ShowLocation(room.title);
        weather.active = !room.interior;
        weather.withClouds = room.clouds;
        if (!visited.Contains(id)) { visited.Add(id); OnEnter(id); }
    }

    void OnEnter(string id)
    {
        switch (id)
        {
            case "station":
                if (GameState.act != 1) break;
                Obj("雨夜 · 抵达灰礁镇\n沿公路往东走，进镇 →");
                Say(new List<Line> {
                    new Line("（雨，比记忆里更冷了。）", "沈砚"),
                    new Line("（二十年了，车站的霓虹还是缺着笔画……）", "沈砚"),
                    new Line("（海岬上……刚才那一闪而过的，是光吗？）", "沈砚"),
                    new Line("—— 沿公路往东走，先进镇吧。")
                });
                break;
            case "street":
                if (GameState.act == 3) Obj("第三幕 · 满月\n去海岬，登上灯塔");
                else if (GameState.act == 2) Obj("第二幕 · 潮水\n去卫生院找护士阿岚（老街东段 · 白十字）");
                else Obj("打听哥哥的下落\n杂货铺的周婶 · 码头的老吴");
                break;
            case "home":
                if (GameState.act == 1) Obj("看看哥哥留下了什么\n（房间里有四处可以调查）");
                break;
            case "archiveIn":
                if (!F("archiveDone") && GameState.FragCount() < 6) Obj("在档案室里找到 6 张报纸碎片");
                break;
            case "cape":
                Obj(GameState.act == 3 ? "塔顶 · 满月\n（灯室里，油灯将尽）" : "登上灯塔看看\n（门虚掩着）");
                break;
            case "top":
                Say(new List<Line> {
                    new Line("满月悬在海平线上方，\n把整个世界照成一张黑白的照片。"),
                    new Line("（油灯快熄了。\n今晚，必须有人做出决定。）", "沈砚")
                });
                break;
        }
    }

    void StartGame()
    {
        ui.FadeTo(() => { LoadRoom("station", 64, 230, "right"); ui.ShowHUD(true); });
    }

    void EndAct1()
    {
        ui.FadeTo(() =>
        {
            ui.ShowHUD(false);
            GameState.endKind = 1;
            ui.ShowEnding("第一幕 · 抵达 —— 完",
                $"真相度 {GameState.truth} ／ 镇民信任 {GameState.trust}\n" + (GameState.trust >= 35 ? "镇民们，似乎愿意对你多说一句。" : "镇民们对你，仍心存戒备。"),
                "「距离满月，还有七天。」\n第二幕 · 潮水 —— 等待着你",
                "空格 进入第二幕 ｜ R 重新开始",
                StartAct2, Restart);
        });
    }
    void StartAct2()
    {
        GameState.act = 2; GameState.endKind = 0;
        SaveSystem.SaveAct(2);
        visited.Clear();
        GameState.SetFlag("gotLetter1");
        ui.HideEnding();
        ui.FadeTo(() => { LoadRoom("street", 810, 218, "down"); ui.ShowHUD(true); });
    }
    void EndAct2()
    {
        Hum();
        ui.FadeTo(() =>
        {
            ui.ShowHUD(false);
            GameState.endKind = 2;
            int lc = GameState.LetterCount();
            ui.ShowEnding("第二幕 · 潮水 —— 完",
                $"真相度 {GameState.truth} ／ 镇民信任 {GameState.trust} ／ 信件 {lc}/5\n" + (lc >= 5 ? "五封信拼在一起，隐约是一句话。" : "还有信，散落在镇子的角落里。"),
                "【满月倒计时：3 天】\n雾更浓了。海浪声里，隐约有呼唤名字的声音。\n第三幕 · 满月 —— 空格进入",
                "空格 进入第三幕 ｜ R 重新开始",
                StartAct3, Restart);
        });
    }
    void StartAct3()
    {
        GameState.act = 3; GameState.endKind = 0;
        SaveSystem.SaveAct(3);
        visited.Clear();
        ui.HideEnding();
        ui.FadeTo(() =>
        {
            LoadRoom("street", 810, 218, "down");
            ui.ShowHUD(true);
            Say(new List<Line> {
                new Line("（雾浓得化不开。\n满月挂在海岬上方，大得反常。）", "沈砚"),
                new Line("（海浪声里，那个呼唤名字的声音……\n越来越清晰了。）", "沈砚"),
                new Line("今晚，一切都会有个了断。")
            });
        });
    }
    void EndGame(string kind)
    {
        var (title, text) = StoryContent.Endings[kind];
        SaveSystem.UnlockEnding(kind);
        Hum();
        ui.FadeTo(() =>
        {
            ui.ShowHUD(false);
            GameState.endKind = 3;
            ui.ShowEnding(title,
                $"真相度 {GameState.truth} ／ 镇民信任 {GameState.trust} ／ 信件 {GameState.LetterCount()}/5\n已解锁结局 {SaveSystem.EndingCount}/4",
                text + "\n\n—— 全剧终 ——",
                "按 R 重新开始", null, Restart);
        });
    }
    void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    /* ---------------- 存档点继续 ---------------- */
    void ContinueGame()
    {
        int a = SaveSystem.SavedAct;
        GrantBaseline(a);
        if (a >= 3) StartAct3();
        else if (a == 2) StartAct2();
        else StartGame();
    }
    void GrantBaseline(int a)
    {
        if (a >= 2)
        {
            foreach (var k in new[] { "gotLetter1", "metZhou", "metWu", "objHome" }) SetF(k);
            GameState.truth = 30; GameState.trust = 25;
        }
        if (a >= 3)
        {
            foreach (var k in new[] { "metLan", "hasKey", "archiveDone", "rosterRead", "metZhou2", "metWu2",
                "letter2", "letter3", "letter4", "letter5" }) SetF(k);
            GameState.truth = 55;
        }
        ui.SetStats();
    }

    /* ---------------- 工具 ---------------- */
    bool F(string k) => GameState.Flag(k);
    void SetF(string k) => GameState.SetFlag(k);
    void Obj(string s) => ui.SetObjective(s);
    void Say(List<Line> lines, Action done = null) => ui.Say(lines, done);
    void Truth(int n) { GameState.truth = Mathf.Clamp(GameState.truth + n, 0, 100); ui.SetStats(); }
    void Trust(int n) { GameState.trust = Mathf.Clamp(GameState.trust + n, 0, 100); ui.SetStats(); }
    void Once(string id, int n) { if (!F(id)) { SetF(id); Truth(n); } }
    void GainLetter(string id, string text, Action after = null)
    {
        Hum();
        ui.OpenLetter(text, () => { SetF(id); ui.SetStats(); after?.Invoke(); });
    }
    void Hum() { if (AudioFX.I) AudioFX.I.Hum(); }

    bool HitBlock(float nx, float ny)
    {
        foreach (var b in room.blocks)
        {
            if (b.gone != null && F(b.gone)) continue;
            if (nx + 4 > b.x && nx - 4 < b.x + b.w && ny > b.y && ny - 4 < b.y + b.h) return true;
        }
        return false;
    }

    void ScanInteract()
    {
        ItemDef best = null; float bd = 1e9f;
        foreach (var it in room.items)
        {
            if (it.avail != null && !it.avail()) continue;
            float dx = it.x - player.x, dy = (it.y - player.y) * 1.6f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            if (d < it.r && d < bd) { bd = d; best = it; }
        }
        nearItem = best;
        ui.Prompt(best != null && ui.mode == UIManager.Mode.Play ? best.label : null);
    }

    /* ---------------- 主循环 ---------------- */
    void Update()
    {
        float dt = Time.deltaTime;
        bool touchE = TouchUI.Get("E"), touchUp = TouchUI.Get("ArrowUp"), touchDown = TouchUI.Get("ArrowDown");
        bool adv = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E) || (touchE && !prevTouchE);
        bool up = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || (touchUp && !prevTouchUp);
        bool down = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || (touchDown && !prevTouchDown);
        prevTouchE = touchE; prevTouchUp = touchUp; prevTouchDown = touchDown;

        if (Input.GetKeyDown(KeyCode.Escape)) ui.OnEscape();
        if (Input.GetKeyDown(KeyCode.R)) ui.OnR();
        if (Input.GetKeyDown(KeyCode.M) && AudioFX.I) AudioFX.I.ToggleMute();
        ui.Tick(dt, adv, up, down);

        if (ui.mode == UIManager.Mode.Play && room != null)
        {
            float dx = (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || TouchUI.Get("ArrowRight") ? 1 : 0)
                     - (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || TouchUI.Get("ArrowLeft") ? 1 : 0);
            float dy = (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || TouchUI.Get("ArrowUp") ? 1 : 0)
                     - (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || TouchUI.Get("ArrowDown") ? 1 : 0);
            player.Tick(dt, dx, dy, HitBlock);
            float target = Mathf.Clamp(player.x - 240, 0, Mathf.Max(0, room.w - 480));
            camPx += (target - camPx) * Mathf.Min(1, dt * 7);
            foreach (var ex in room.exits)
            {
                if (ex.need != null && !F(ex.need)) continue;
                if (player.x > ex.x && player.x < ex.x + ex.w && player.y > ex.y && player.y < ex.y + ex.h)
                {
                    var e = ex;
                    ui.FadeTo(() => LoadRoom(e.to, e.sx, e.sy, e.sdir));
                    break;
                }
            }
            ScanInteract();
            if (adv && nearItem != null)
            {
                var it = nearItem; nearItem = null; ui.Prompt(null);
                Interact(it.id);
            }
        }

        if (room != null)
        {
            // 16:9 信箱适配，避免超宽/竖屏穿帮
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            const float target = 16f / 9f;
            if (aspect > target) { float w = target / aspect; cam.rect = new Rect((1 - w) / 2, 0, w, 1); }
            else { float h = aspect / target; cam.rect = new Rect(0, (1 - h) / 2, 1, h); }
            CamX = camPx / 100f;
            cam.transform.position = new Vector3(CamX + 2.4f, 1.35f, -10);
            var r = rts[room.id];
            if (r.farTf) r.farTf.position = new Vector3(-CamX * 0.22f, 0, 0);
            if (r.midTf) r.midTf.position = new Vector3(-CamX * 0.55f, 0, 0);
            if (r.frontTf) r.frontTf.position = new Vector3(-CamX * 1.18f, 0, 0);
            if (r.watcher) r.watcher.SetActive(GameState.act == 3 && GameState.truth < GameState.TruthHigh);
            if (r.moon) r.moon.SetActive(GameState.act == 3);
            if (r.rope) { bool open = F("archiveDone"); r.rope.SetActive(!open); r.cutRope.SetActive(open); }
            foreach (var (fid, go) in r.frags) if (go.activeSelf == F(fid)) go.SetActive(!F(fid));
            if (r.shopGlow)
            {
                var sr = r.shopGlow.GetComponent<SpriteRenderer>();
                var c = sr.color; c.a = 0.35f * (0.55f + 0.3f * Mathf.Sin(Time.time * 5)); sr.color = c;
            }
        }
    }
}

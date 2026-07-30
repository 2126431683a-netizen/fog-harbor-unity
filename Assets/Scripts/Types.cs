using System.Collections.Generic;
using UnityEngine;

/* ================= 全局状态 ================= */
public static class GameState
{
    public static int truth = 0, trust = 20, act = 1, endKind = 0;
    public static string pendingEnd = "A";
    public static readonly HashSet<string> flags = new HashSet<string>();
    public static bool Flag(string k) => flags.Contains(k);
    public static void SetFlag(string k) => flags.Add(k);
    public static int LetterCount()
    {
        int n = 1;
        foreach (var k in new[] { "letter2", "letter3", "letter4", "letter5" }) if (Flag(k)) n++;
        return n;
    }
    public static int FragCount()
    {
        int n = 0; for (int i = 1; i <= 6; i++) if (Flag("frag" + i)) n++;
        return n;
    }
    public const int TruthHigh = 80;
}

/* ================= 数据结构 ================= */
public class Line
{
    public string sp, tx;
    public List<Choice> ch;
    public Line(string tx, string sp = null) { this.tx = tx; this.sp = sp; }
    public Line(string tx, string sp, List<Choice> ch) { this.tx = tx; this.sp = sp; this.ch = ch; }
}
public class Choice
{
    public string t;
    public System.Action fn;
    public List<Line> then;
    public Choice(string t, System.Action fn, params Line[] then) { this.t = t; this.fn = fn; this.then = new List<Line>(then); }
}

public class BlockDef { public float x, y, w, h; public string gone; }
public class ExitDef { public float x, y, w, h; public string to, need; public float sx, sy; public string sdir; }
public class ItemDef { public string id, label; public float x, y, r; public System.Func<bool> avail; }
public class NPCDef { public string id, map; public float x, y; public bool sit; }

public class BeamDef { public float x, y, scale, len; }
public class LampDef { public float x; public bool flicker; }
public class BoatDef { public float x, y, phase; }

public class RoomDef
{
    public string id, title;
    public float w = 480, bandTop = 200, bandBottom = 238;
    public bool rain, interior;
    public List<BlockDef> blocks = new List<BlockDef>();
    public List<ExitDef> exits = new List<ExitDef>();
    public List<ItemDef> items = new List<ItemDef>();
    public List<NPCDef> npcs = new List<NPCDef>();
    public string onEnter; // 目标提示 key
    public List<BeamDef> beams = new List<BeamDef>();
    public List<LampDef> lamps = new List<LampDef>();
    public List<BoatDef> boats = new List<BoatDef>();
    public bool shopWindow;        // 老街橱窗暖光
    public bool ropeGate;          // 码头缆绳（archiveDone 后消失）
    public bool watcher;           // 满月看灯人（低真相）
    public bool flame;             // 塔顶油灯火苗
    public bool moon;              // 满月（act3）
    public bool frags;             // 档案室碎片
    public bool clouds = true;
}

/* ================= 坐标换算（JS 像素 → Unity 单位） ================= */
public static class U
{
    public const float VH = 270f;
    public static float X(float px) => px / 100f;
    public static float Y(float py) => (VH - py) / 100f;
    public static Vector2 P(float px, float py) => new Vector2(X(px), Y(py));
    public static Sprite Spr(string path) => Resources.Load<Sprite>("Art/" + path);

    static Texture2D _white;
    public static Texture2D White
    {
        get
        {
            if (_white == null) { _white = new Texture2D(1, 1); _white.SetPixel(0, 0, Color.white); _white.Apply(); }
            return _white;
        }
    }
    static Sprite _quad;
    public static Sprite Quad
    {
        get
        {
            if (_quad == null) _quad = Sprite.Create(White, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
            return _quad;
        }
    }
    // 生成 tinted 矩形精灵
    public static GameObject RectProp(Transform parent, string name, float px, float py, float pw, float ph, Color col, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Quad; sr.color = col; sr.sortingOrder = order;
        go.transform.position = new Vector3(X(px), Y(py), 0);
        go.transform.localScale = new Vector3(pw / 100f, ph / 100f, 1);
        return go;
    }
}

using System.Collections.Generic;
using UnityEngine;

/* ================= 动态特效（光束/灯/船/看灯人/火苗/天气/碎片） ================= */
public class BeamFX : MonoBehaviour
{
    public float len = 170, speed = 0.42f;
    SpriteRenderer b1, b2, glow;
    public static BeamFX Create(Transform parent, float px, float py, float scale, float len, int order = -25)
    {
        var root = new GameObject("Beam");
        root.transform.SetParent(parent, false);
        root.transform.position = U.P(px, py);
        var fx = root.AddComponent<BeamFX>();
        fx.len = len;
        var beamSpr = U.Spr("FX/beam");
        for (int i = 0; i < 2; i++)
        {
            var go = new GameObject("b" + i);
            go.transform.SetParent(root.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = beamSpr; sr.sortingOrder = order;
            var c = sr.color; c.a = 0.55f; sr.color = c;
            go.transform.localScale = new Vector3(len / 200f, scale, 1);
            go.transform.localRotation = Quaternion.Euler(0, 0, i * 180);
            if (i == 0) fx.b1 = sr; else fx.b2 = sr;
        }
        var glowGo = new GameObject("glow");
        glowGo.transform.SetParent(root.transform, false);
        var g = glowGo.AddComponent<SpriteRenderer>();
        g.sprite = U.Spr("FX/glow"); g.sortingOrder = order;
        glowGo.transform.localScale = Vector3.one * 0.5f * scale;
        fx.glow = g;
        return fx;
    }
    void Update()
    {
        float a = Time.time * speed * Mathf.Rad2Deg;
        b1.transform.localRotation = Quaternion.Euler(0, 0, -a);
        b2.transform.localRotation = Quaternion.Euler(0, 0, -a + 180);
        float fl = 0.5f + 0.5f * Mathf.Max(0, Mathf.Sin(Time.time * 0.9f));
        var c = glow.color; c.a = 0.35f * fl; glow.color = c;
    }
}

public class LampFX : MonoBehaviour
{
    SpriteRenderer cone, glow, head; public bool flicker; float seed;
    public static void Create(Transform parent, float px, bool flicker)
    {
        var root = new GameObject("Lamp");
        root.transform.SetParent(parent, false);
        root.transform.position = U.P(px, 132);
        var fx = root.AddComponent<LampFX>(); fx.flicker = flicker; fx.seed = px;
        fx.cone = Mk(root, "cone", "FX/cone", 12, Vector3.one);
        fx.cone.transform.localPosition = new Vector3(0, -1.04f, 0);
        fx.glow = Mk(root, "glow", "FX/glow", 12, Vector3.one * 0.24f);
        fx.head = U.RectProp(root.transform, "head", px, 129, 8, 3, new Color(0.94f, 0.86f, 0.63f), 12).GetComponent<SpriteRenderer>();
    }
    static SpriteRenderer Mk(GameObject parent, string name, string spr, int order, Vector3 scale)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = U.Spr(spr); sr.sortingOrder = order;
        go.transform.localScale = scale;
        return sr;
    }
    void Update()
    {
        bool on = !flicker || Random.value > 0.04f;
        float a = on ? 0.6f + 0.3f * Mathf.Sin(Time.time * 6 + seed) : 0.05f;
        var c1 = cone.color; c1.a = a * 0.8f; cone.color = c1;
        var c2 = glow.color; c2.a = a; glow.color = c2;
        var c3 = head.color; c3.a = a; head.color = c3;
    }
}

public class BoatFX : MonoBehaviour
{
    float phase; Vector3 basePos;
    public static void Create(Transform parent, float px, float py, float phase)
    {
        var root = new GameObject("Boat");
        root.transform.SetParent(parent, false);
        root.transform.position = U.P(px, py);
        var fx = root.AddComponent<BoatFX>(); fx.phase = phase; fx.basePos = root.transform.position;
        Color hull = new Color(0.04f, 0.07f, 0.11f);
        U.RectProp(root.transform, "hull", 0, 0, 68, 10, hull, -14).transform.localPosition = new Vector3(0, 0.05f, 0);
        U.RectProp(root.transform, "cabin", 0, 0, 56, 5, hull, -13).transform.localPosition = new Vector3(0, 0.12f, 0);
        U.RectProp(root.transform, "mast", 0, 0, 2, 32, new Color(0.04f, 0.06f, 0.1f), -13).transform.localPosition = new Vector3(0, 0.3f, 0);
        U.RectProp(root.transform, "sail", 0, 0, 14, 8, new Color(0.06f, 0.09f, 0.14f), -13).transform.localPosition = new Vector3(0.07f, 0.34f, 0);
    }
    void Update()
    {
        var p = basePos;
        p.y += Mathf.Round(Mathf.Sin(Time.time * 1.2f + phase) * 1.5f) / 100f;
        transform.position = p;
    }
}

public class WatcherFX : MonoBehaviour
{
    readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
    public static WatcherFX Create(Transform parent, float px, float py)
    {
        var root = new GameObject("Watcher");
        root.transform.SetParent(parent, false);
        root.transform.position = U.P(px, py);
        var fx = root.AddComponent<WatcherFX>();
        Color robe = new Color(0.04f, 0.05f, 0.08f);
        fx.parts.Add(U.RectProp(root.transform, "r1", 0, 0, 8, 26, robe, 16).GetComponent<SpriteRenderer>());
        fx.parts[^1].transform.localPosition = new Vector3(0, 0.13f, 0);
        fx.parts.Add(U.RectProp(root.transform, "r2", 0, 0, 10, 4, robe, 16).GetComponent<SpriteRenderer>());
        fx.parts[^1].transform.localPosition = new Vector3(0, 0.14f, 0);
        fx.parts.Add(U.RectProp(root.transform, "hd", 0, 0, 6, 7, robe, 16).GetComponent<SpriteRenderer>());
        fx.parts[^1].transform.localPosition = new Vector3(0, 0.3f, 0);
        fx.parts.Add(U.RectProp(root.transform, "fc", 0, 0, 4, 4, new Color(0.83f, 0.85f, 0.88f), 17).GetComponent<SpriteRenderer>());
        fx.parts[^1].transform.localPosition = new Vector3(0, 0.3f, 0);
        return fx;
    }
    void Update()
    {
        float a = 0.45f + 0.16f * Mathf.Sin(Time.time * 1.3f);
        foreach (var p in parts) { var c = p.color; c.a = a; p.color = c; }
    }
}

public class FlameFX : MonoBehaviour
{
    SpriteRenderer f1, f2, glow;
    public static void Create(Transform parent, float px, float py)
    {
        var root = new GameObject("Flame");
        root.transform.SetParent(parent, false);
        root.transform.position = U.P(px, py);
        var fx = root.AddComponent<FlameFX>();
        fx.f1 = U.RectProp(root.transform, "f1", 0, 0, 4, 6, new Color(0.91f, 0.59f, 0.24f), 16).GetComponent<SpriteRenderer>();
        fx.f2 = U.RectProp(root.transform, "f2", 0, 0, 2, 3, new Color(0.97f, 0.86f, 0.59f), 17).GetComponent<SpriteRenderer>();
        fx.f1.transform.localPosition = new Vector3(0, 0.03f, 0);
        fx.f2.transform.localPosition = new Vector3(0, 0.035f, 0);
        var g = new GameObject("g");
        g.transform.SetParent(root.transform, false);
        fx.glow = g.AddComponent<SpriteRenderer>();
        fx.glow.sprite = U.Spr("FX/glow"); fx.glow.sortingOrder = 15;
        g.transform.localScale = Vector3.one * 0.24f;
    }
    void Update()
    {
        float fl = Mathf.Sin(Time.time * 9) * 0.5f + Mathf.Sin(Time.time * 23) * 0.5f;
        var c1 = f1.color; c1.a = 0.75f + fl * 0.2f; f1.color = c1;
        var c2 = f2.color; c2.a = 0.8f + fl * 0.2f; f2.color = c2;
        var c3 = glow.color; c3.a = 0.25f + fl * 0.08f; glow.color = c3;
        f1.transform.localPosition = new Vector3(0, fl > 0 ? 0.04f : 0.03f, 0);
    }
}

public class MoonFX : MonoBehaviour
{
    public static void Create(Transform parent, float px, float py)
    {
        var tex = new Texture2D(36, 36, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 36; y++) for (int x = 0; x < 36; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(18, 18));
            Color c = d < 15 ? new Color(0.91f, 0.9f, 0.83f) : (d < 18 ? new Color(0.91f, 0.9f, 0.83f, 0.3f) : Color.clear);
            tex.SetPixel(x, y, c);
        }
        tex.Apply();
        var go = new GameObject("Moon");
        go.transform.SetParent(parent, false);
        go.transform.position = U.P(px, py);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 36, 36), new Vector2(0.5f, 0.5f), 100f);
        sr.sortingOrder = -26;
    }
}

/* ---------------- 雨 / 雾 / 云 ---------------- */
public class WeatherFX : MonoBehaviour
{
    class Drop { public SpriteRenderer sr; public float x, y, v; }
    readonly List<Drop> drops = new List<Drop>();
    readonly List<SpriteRenderer> puffs = new List<SpriteRenderer>();
    readonly List<Vector3> puffBase = new List<Vector3>();
    readonly List<float> puffSpeed = new List<float>();
    readonly List<SpriteRenderer> clouds = new List<SpriteRenderer>();
    readonly List<float> cloudSpeed = new List<float>();
    SpriteRenderer stripSr;
    Sprite rainSpr;
    public bool active, heavy, withClouds = true;

    public static WeatherFX Create(Transform parent)
    {
        var go = new GameObject("Weather");
        go.transform.SetParent(parent, false);
        return go.AddComponent<WeatherFX>();
    }
    void Awake()
    {
        var tex = new Texture2D(2, 8, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 8; y++) { tex.SetPixel(0, y, new Color(0.65f, 0.75f, 0.83f, 0.3f)); tex.SetPixel(1, y, new Color(0.65f, 0.75f, 0.83f, 0.15f)); }
        tex.Apply();
        rainSpr = Sprite.Create(tex, new Rect(0, 0, 2, 8), new Vector2(0.5f, 0.5f), 100f);
        for (int i = 0; i < 60; i++)
        {
            var go = new GameObject("r" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = rainSpr; sr.sortingOrder = 400;
            go.transform.localRotation = Quaternion.Euler(0, 0, 7);
            drops.Add(new Drop { sr = sr, x = Random.Range(0f, 4.8f), y = Random.Range(0f, 2.7f), v = Random.Range(1.9f, 2.8f) });
        }
        for (int i = 0; i < 5; i++)
        {
            var go = new GameObject("p" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = U.Spr("FX/puff"); sr.sortingOrder = 380;
            var c = sr.color; c.a = 0.5f; sr.color = c;
            go.transform.localScale = Vector3.one * Random.Range(0.8f, 1.4f);
            puffs.Add(sr);
            puffBase.Add(new Vector3(Random.Range(-1f, 4.8f), Random.Range(0.5f, 2.2f), 0));
            puffSpeed.Add(Random.Range(0.05f, 0.15f));
        }
        var strip = new GameObject("strip");
        strip.transform.SetParent(transform, false);
        stripSr = strip.AddComponent<SpriteRenderer>();
        stripSr.sprite = U.Spr("FX/fogstrip"); stripSr.sortingOrder = 370;
        stripSr.drawMode = SpriteDrawMode.Tiled;
        stripSr.size = new Vector2(16f, 0.9f);
        strip.transform.position = new Vector3(2.4f, 0.45f, 0);
        var sc = stripSr.color; sc.a = 0.7f; stripSr.color = sc;
        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject("c" + i);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = U.Spr("FX/cloud"); sr.sortingOrder = -24;
            var c = sr.color; c.a = 0.85f; sr.color = c;
            go.transform.position = new Vector3(Random.Range(0f, 4.8f), 2.1f + i * 0.18f, 0);
            clouds.Add(sr);
            cloudSpeed.Add(0.03f + i * 0.02f);
        }
    }
    void Update()
    {
        float camX = Game.CamX;
        if (!active)
        {
            foreach (var d in drops) d.sr.enabled = false;
            foreach (var p in puffs) p.enabled = false;
            foreach (var c in clouds) c.enabled = false;
            stripSr.enabled = false;
            return;
        }
        stripSr.enabled = true;
        foreach (var p in puffs) p.enabled = true;
        stripSr.transform.position = new Vector3(camX + 2.4f, 0.45f, 0);
        int n = heavy ? drops.Count : (int)(drops.Count * 0.65f);
        for (int i = 0; i < drops.Count; i++)
        {
            var d = drops[i];
            d.sr.enabled = i < n;
            if (i >= n) continue;
            d.y -= d.v * Time.deltaTime;
            d.x += d.v * 0.12f * Time.deltaTime;
            if (d.y < 0) { d.y = 2.7f; d.x = camX + Random.Range(-0.2f, 5.2f); }
            d.sr.transform.position = new Vector3(d.x, d.y, 0);
        }
        for (int i = 0; i < puffs.Count; i++)
        {
            var p = puffBase[i];
            p.x += puffSpeed[i] * Time.deltaTime;
            if (p.x > camX + 6.2f) p.x = camX - 2f;
            puffBase[i] = p;
            puffs[i].transform.position = p;
        }
        for (int i = 0; i < clouds.Count; i++)
        {
            clouds[i].enabled = withClouds;
            if (!withClouds) continue;
            var p = clouds[i].transform.position;
            p.x -= cloudSpeed[i] * Time.deltaTime;
            if (p.x < camX - 1.5f) p.x = camX + 6.3f;
            clouds[i].transform.position = p;
        }
    }
}

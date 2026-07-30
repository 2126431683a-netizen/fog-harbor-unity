using UnityEngine;

/* ================= 玩家（移植 JS 移动/动画/纵深） ================= */
public class PlayerController : MonoBehaviour
{
    public float x = 64, y = 230;       // JS 像素坐标
    public string dir = "down";
    public bool moving;
    float ft;

    SpriteRenderer sr, shadow;
    Sprite down0, down1, up0, up1, side0, side1;
    RoomDef room;

    public static PlayerController Create(Transform parent)
    {
        var go = new GameObject("Player");
        go.transform.SetParent(parent, false);
        var p = go.AddComponent<PlayerController>();
        p.sr = go.AddComponent<SpriteRenderer>();
        p.down0 = U.Spr("Sprites/yan_down0"); p.down1 = U.Spr("Sprites/yan_down1");
        p.up0 = U.Spr("Sprites/yan_up0"); p.up1 = U.Spr("Sprites/yan_up1");
        p.side0 = U.Spr("Sprites/yan_side0"); p.side1 = U.Spr("Sprites/yan_side1");
        var sh = new GameObject("Shadow");
        sh.transform.SetParent(go.transform, false);
        p.shadow = sh.AddComponent<SpriteRenderer>();
        p.shadow.sprite = U.Spr("FX/shadow");
        p.shadow.sortingOrder = -1;
        sh.transform.localScale = new Vector3(0.85f, 0.8f, 1);
        sh.transform.localPosition = new Vector3(0, 0.02f, 0);
        p.Sync();
        return p;
    }

    public void SetRoom(RoomDef r) => room = r;
    public void Place(float px, float py, string d) { x = px; y = py; dir = d ?? "down"; moving = false; Sync(); }

    public void Tick(float dt, float dx, float dy, System.Func<float, float, bool> hitBlock)
    {
        moving = dx != 0 || dy != 0;
        if (moving)
        {
            const float sp = 84f;
            float len = Mathf.Sqrt(dx * dx + dy * dy); dx /= len; dy /= len;
            float nx = x + dx * sp * dt;
            float ny = y + dy * sp * 0.55f * dt;
            if (!hitBlock(nx, y)) x = Mathf.Clamp(nx, 10, room.w - 10);
            if (!hitBlock(x, ny)) y = Mathf.Clamp(ny, room.bandTop, room.bandBottom);
            ft += dt;
            if (Mathf.Abs(dx) > Mathf.Abs(dy)) dir = dx < 0 ? "left" : "right";
            else dir = dy < 0 ? "down" : "up"; // JS 屏幕 y 向下；Unity dy>0 = 向上
        }
        Sync();
    }

    void Sync()
    {
        if (room == null) return;
        float k = Mathf.Clamp01((y - room.bandTop) / (room.bandBottom - room.bandTop));
        float s = (1.7f + 0.6f * k) / 4f; // 精灵 PNG 为 4 倍图
        transform.localScale = new Vector3(s, s, 1);
        float bob = moving ? Mathf.Abs(Mathf.Sin(ft * 14)) * -0.01f : 0;
        transform.position = new Vector3(U.X(x), U.Y(y) + bob, 0);
        int fr = moving ? (Mathf.FloorToInt(ft * 7) % 2) : 0;
        sr.flipX = dir == "right";
        sr.sprite = dir == "up" ? (fr == 1 ? up1 : up0)
                  : dir == "left" || dir == "right" ? (fr == 1 ? side1 : side0)
                  : (fr == 1 ? down1 : down0);
        sr.sortingOrder = Mathf.RoundToInt(y);
        shadow.sortingOrder = Mathf.RoundToInt(y) - 1;
    }
}

/* ================= NPC ================= */
public class NPCView : MonoBehaviour
{
    SpriteRenderer sr, shadow;
    Sprite s0, s1;
    float seed; bool sit;
    float baseY;

    public static NPCView Create(Transform parent, NPCDef def)
    {
        var go = new GameObject("NPC_" + def.id);
        go.transform.SetParent(parent, false);
        var n = go.AddComponent<NPCView>();
        n.sr = go.AddComponent<SpriteRenderer>();
        n.s0 = U.Spr("Sprites/" + def.map + "0");
        n.s1 = U.Spr("Sprites/" + def.map + "1");
        if (n.s1 == null) n.s1 = n.s0;
        n.sr.sprite = n.s0;
        n.sit = def.sit;
        n.seed = def.x;
        float s = def.sit ? 1.7f / 4f : 1.9f / 4f;
        go.transform.localScale = new Vector3(s, s, 1);
        n.baseY = U.Y(def.y);
        go.transform.position = new Vector3(U.X(def.x), n.baseY, 0);
        n.sr.sortingOrder = Mathf.RoundToInt(def.y);
        var sh = new GameObject("Shadow");
        sh.transform.SetParent(go.transform, false);
        n.shadow = sh.AddComponent<SpriteRenderer>();
        n.shadow.sprite = U.Spr("FX/shadow");
        n.shadow.sortingOrder = Mathf.RoundToInt(def.y) - 1;
        sh.transform.localPosition = new Vector3(0, 0.02f, 0);
        return n;
    }
    void Update()
    {
        float bob = sit ? 0.03f : Mathf.Round(Mathf.Sin(Time.time * 2 + seed) * 0.6f) / 100f;
        var p = transform.position; p.y = baseY + bob; transform.position = p;
        if (Time.frameCount % 30 == 0 && s1 != s0) sr.sprite = (Time.frameCount % 60 == 0) ? s1 : s0;
    }
}

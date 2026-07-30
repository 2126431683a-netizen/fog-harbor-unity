using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/* ================= 触屏虚拟按键 ================= */
public class TouchUI : MonoBehaviour
{
    public static Dictionary<string, bool> Keys = new Dictionary<string, bool>();

    public static void Create(Canvas canvas, Font font)
    {
        bool isTouch = Input.touchSupported && Application.isMobilePlatform;
        if (!isTouch) return;
        var go = new GameObject("TouchUI");
        go.transform.SetParent(canvas.transform, false);
        var tui = go.AddComponent<TouchUI>();
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        tui.Btn("ArrowUp", "▲", new Vector2(0.09f, 0.24f), font);
        tui.Btn("ArrowLeft", "◀", new Vector2(0.03f, 0.13f), font);
        tui.Btn("ArrowDown", "▼", new Vector2(0.09f, 0.13f), font);
        tui.Btn("ArrowRight", "▶", new Vector2(0.15f, 0.13f), font);
        tui.Btn("E", "E", new Vector2(0.88f, 0.15f), font, 76);
    }

    void Btn(string key, string label, Vector2 pos, Font font, int size = 64)
    {
        var go = new GameObject("btn_" + key);
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = pos;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.05f, 0.09f, 0.14f, 0.45f);
        var tgo = new GameObject("Label");
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var t = tgo.AddComponent<Text>();
        t.font = font; t.text = label; t.fontSize = 24;
        t.color = new Color(0.78f, 0.82f, 0.86f);
        t.alignment = TextAnchor.MiddleCenter;
        var trigger = go.AddComponent<EventTrigger>();
        Add(trigger, EventTriggerType.PointerDown, () => Keys[key] = true);
        Add(trigger, EventTriggerType.PointerUp, () => Keys[key] = false);
        Add(trigger, EventTriggerType.PointerExit, () => Keys[key] = false);
    }
    static void Add(EventTrigger tg, EventTriggerType type, System.Action fn)
    {
        var e = new EventTrigger.Entry { eventID = type };
        e.callback.AddListener(_ => fn());
        tg.triggers.Add(e);
    }
    public static bool Get(string key) => Keys.TryGetValue(key, out var v) && v;
}

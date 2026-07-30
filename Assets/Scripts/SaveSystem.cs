using UnityEngine;

/* ================= 存档（幕级存档点 + 结局图鉴） ================= */
public static class SaveSystem
{
    const string ActKey = "fh_act";
    const string EndKey = "fh_end_";

    public static void SaveAct(int act)
    {
        if (act > PlayerPrefs.GetInt(ActKey, 1))
        {
            PlayerPrefs.SetInt(ActKey, act);
            PlayerPrefs.Save();
        }
    }
    public static int SavedAct => PlayerPrefs.GetInt(ActKey, 1);
    public static bool HasSave => SavedAct > 1;
    public static void ClearSave() { PlayerPrefs.DeleteKey(ActKey); PlayerPrefs.Save(); }

    public static void UnlockEnding(string kind)
    {
        PlayerPrefs.SetInt(EndKey + kind, 1);
        PlayerPrefs.Save();
    }
    public static int EndingCount
    {
        get
        {
            int n = 0;
            foreach (var k in new[] { "A", "B", "C", "D" }) if (PlayerPrefs.GetInt(EndKey + k, 0) == 1) n++;
            return n;
        }
    }
}

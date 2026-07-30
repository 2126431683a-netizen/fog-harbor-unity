using System.Collections.Generic;

/* ================= 房间定义（移植自 JS 场景数据） ================= */
public static class RoomData
{
    static BlockDef B(float x, float y, float w, float h, string gone = null) => new BlockDef { x = x, y = y, w = w, h = h, gone = gone };
    static ExitDef E(float x, float y, float w, float h, string to, float sx, float sy, string sdir, string need = null)
        => new ExitDef { x = x, y = y, w = w, h = h, to = to, sx = sx, sy = sy, sdir = sdir, need = need };
    static ItemDef I(string id, string label, float x, float y, float r, System.Func<bool> avail = null)
        => new ItemDef { id = id, label = label, x = x, y = y, r = r, avail = avail };

    public static Dictionary<string, RoomDef> Build()
    {
        var d = new Dictionary<string, RoomDef>();

        d["station"] = new RoomDef
        {
            id = "station", title = "灰礁镇 · 汽车站", w = 960, bandTop = 200, bandBottom = 238, rain = true,
            onEnter = "雨夜 · 抵达灰礁镇\n沿公路往东走，进镇 →",
            blocks = { B(338, 198, 30, 10), B(94, 198, 8, 24), B(256, 198, 8, 24), B(148, 192, 74, 8) },
            exits = { E(938, 200, 22, 38, "street", 36, 222, "right") },
            items = {
                I("poster", "告示牌", 353, 208, 26),
                I("neon", "霓虹招牌", 170, 208, 42),
                I("bench", "长椅", 185, 208, 24)
            },
            beams = { new BeamDef { x = 428, y = 112, scale = 0.9f, len = 170 } }
        };

        d["street"] = new RoomDef
        {
            id = "street", title = "灰礁镇 · 老街", w = 1240, bandTop = 198, bandBottom = 238, rain = true,
            blocks = { B(684, 194, 32, 8), B(218, 194, 6, 8), B(698, 194, 6, 8), B(1048, 194, 6, 8) },
            exits = {
                E(0, 198, 12, 40, "station", 924, 222, "left"),
                E(1226, 198, 14, 40, "pier", 40, 222, "right"),
                E(792, 190, 34, 12, "home", 240, 238, "up")
            },
            items = {
                I("zhou", "与周婶交谈", 442, 212, 30),
                I("board", "公告栏", 700, 208, 26),
                I("archive", "钉死的木门", 590, 208, 28),
                I("clinicDoor", "卫生院大门", 992, 208, 26)
            },
            npcs = { new NPCDef { id = "zhou", map = "zhou", x = 442, y = 214 } },
            beams = { new BeamDef { x = 1130, y = 114, scale = 0.55f, len = 130 } },
            lamps = { new LampDef { x = 220 }, new LampDef { x = 700, flicker = true }, new LampDef { x = 1050 } },
            shopWindow = true
        };

        d["pier"] = new RoomDef
        {
            id = "pier", title = "灰礁镇 · 码头", w = 1100, bandTop = 200, bandBottom = 240, rain = true,
            blocks = { B(598, 198, 66, 14), B(196, 194, 8, 10), B(1040, 198, 60, 24, "archiveDone"), B(742, 200, 18, 10) },
            exits = {
                E(0, 200, 12, 40, "street", 1196, 222, "left"),
                E(1076, 200, 24, 40, "cape", 60, 222, "right", "archiveDone")
            },
            items = {
                I("wu", "与老吴交谈", 750, 216, 30),
                I("boat", "旧渔船", 330, 216, 38),
                I("whistle", "系缆桩", 120, 216, 24)
            },
            npcs = { new NPCDef { id = "wu", map = "wu", x = 750, y = 220, sit = true } },
            beams = { new BeamDef { x = 520, y = 82, scale = 1.25f, len = 210 } },
            lamps = { new LampDef { x = 198 } },
            boats = { new BoatDef { x = 300, y = 166 }, new BoatDef { x = 520, y = 158, phase = 2 } },
            ropeGate = true
        };

        d["home"] = new RoomDef
        {
            id = "home", title = "沈墨的房间", w = 480, bandTop = 206, bandBottom = 246, interior = true,
            blocks = { B(36, 196, 114, 16), B(200, 190, 80, 12), B(420, 192, 54, 14) },
            exits = { E(430, 240, 46, 8, "street", 810, 216, "down") },
            items = {
                I("diary", "黑皮日记", 242, 214, 26),
                I("shell", "窗台贝壳", 348, 214, 26),
                I("tide", "潮汐表", 170, 214, 26),
                I("pillow", "枕头", 96, 220, 26)
            }
        };

        d["clinic"] = new RoomDef
        {
            id = "clinic", title = "灰礁镇 · 卫生院", w = 560, bandTop = 206, bandBottom = 246, interior = true,
            blocks = { B(60, 196, 90, 14), B(190, 190, 80, 12), B(430, 192, 60, 14) },
            exits = { E(500, 240, 48, 8, "street", 992, 216, "down") },
            items = {
                I("lan", "与阿岚交谈", 340, 218, 30),
                I("records", "体检记录", 230, 212, 26),
                I("cabinet", "药柜", 460, 214, 24)
            },
            npcs = { new NPCDef { id = "lan", map = "lan", x = 340, y = 220 } }
        };

        d["archiveIn"] = new RoomDef
        {
            id = "archiveIn", title = "灰礁镇 · 档案室", w = 720, bandTop = 206, bandBottom = 246, interior = true,
            blocks = { B(300, 196, 120, 14), B(560, 192, 70, 14), B(60, 192, 90, 12) },
            exits = { E(660, 240, 48, 8, "street", 590, 216, "down") },
            items = {
                I("frag1", "报纸碎片", 180, 230, 20), I("frag2", "报纸碎片", 250, 212, 20),
                I("frag3", "报纸碎片", 350, 238, 20), I("frag4", "报纸碎片", 470, 214, 20),
                I("frag5", "报纸碎片", 540, 238, 20), I("frag6", "报纸碎片", 640, 220, 20),
                I("longtable", "长桌", 360, 224, 38),
                I("drawer", "铁皮抽屉", 596, 214, 26)
            },
            frags = true
        };

        d["cape"] = new RoomDef
        {
            id = "cape", title = "灰礁镇 · 海岬", w = 900, bandTop = 200, bandBottom = 240, rain = true,
            blocks = { B(668, 190, 64, 20), B(200, 196, 40, 12), B(480, 196, 30, 10) },
            exits = { E(0, 200, 12, 40, "pier", 1052, 222, "left") },
            items = {
                I("capeSign", "警示牌", 250, 212, 26),
                I("letter5", "石缝里的信封", 640, 226, 24),
                I("towerDoor", "灯塔大门", 700, 214, 30)
            },
            beams = { new BeamDef { x = 700, y = 135, scale = 2.2f, len = 320 } },
            watcher = true, moon = true
        };

        d["tower"] = new RoomDef
        {
            id = "tower", title = "灯塔 · 塔内", w = 480, bandTop = 206, bandBottom = 246, interior = true,
            blocks = { B(330, 196, 90, 16), B(40, 196, 60, 12) },
            exits = { E(214, 240, 52, 8, "cape", 700, 228, "down") },
            items = {
                I("roster", "墙上的刻痕", 250, 214, 34),
                I("stairs", "旋梯", 376, 218, 28)
            }
        };

        d["top"] = new RoomDef
        {
            id = "top", title = "灯塔 · 顶层灯室", w = 480, bandTop = 206, bandBottom = 246, interior = true,
            blocks = { B(210, 190, 60, 16), B(36, 196, 70, 12) },
            exits = { E(52, 240, 44, 8, "tower", 240, 232, "down") },
            items = {
                I("lamp", "油灯", 240, 214, 38),
                I("window", "瞭望窗", 120, 212, 26),
                I("watcherTop", "那道身影", 352, 212, 32, () => GameState.truth < GameState.TruthHigh)
            },
            flame = true, watcher = true, clouds = false
        };

        return d;
    }
}

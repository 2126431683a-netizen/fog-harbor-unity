using System.Collections.Generic;

/* ================= 交互逻辑（移植 JS 全部 act()） ================= */
public partial class Game
{
    void Interact(string id)
    {
        switch (id)
        {
            /* ---------- 汽车站 ---------- */
            case "poster":
                Once("poster", 5);
                Say(new List<Line> {
                    new Line("褪色的寻人启事层层叠叠。最新的一张写着：\n「灰鲸号海难 · 二十周年祭」。"),
                    new Line("（二十周年……下个满月，就是祭日了。）", "沈砚") });
                break;
            case "neon":
                Say(new List<Line> {
                    new Line("霓虹灯管缺了笔画，「灰礁客运站」只剩半边字，\n在雨里明明灭灭。"),
                    new Line("（和记忆里一模一样。\n这个镇子，好像被时间忘在了原地。）", "沈砚") });
                break;
            case "bench":
                Say(new List<Line> { new Line("湿漉漉的长椅，椅背上落着一只躲雨的蛾。\n你没什么心情坐下。") });
                break;

            /* ---------- 老街 ---------- */
            case "zhou": TalkZhou(); break;
            case "board":
                if (GameState.act == 2 && !F("letter4"))
                {
                    Say(new List<Line> { new Line("公告栏的木板松了。\n后面贴着什么东西——一个被雾水浸软的信封。") },
                        () => GainLetter("letter4", StoryContent.Letter4));
                    break;
                }
                Once("board", 5);
                Say(new List<Line> {
                    new Line("停水通知、禁渔期通告……最底下压着一张手写的纸：\n「满月期间，夜间请勿靠近海岬。」"),
                    new Line("（为什么，偏偏是满月？）", "沈砚") });
                break;
            case "archive":
                if (GameState.act < 2)
                {
                    Once("archive", 5);
                    Say(new List<Line> {
                        new Line("镇档案室的门被木板钉死了。\n木板上有一行粉笔字：「一九九六，勿问。」"),
                        new Line("（里面锁着的，恐怕不止是旧报纸。）", "沈砚") });
                }
                else if (!F("hasKey"))
                    Say(new List<Line> { new Line("（木板已经被起掉了，但门还锁着。\n需要一把钥匙——也许阿岚知道些什么。）", "沈砚") });
                else
                    ui.FadeTo(() => LoadRoom("archiveIn", 120, 236, "right"));
                break;
            case "clinicDoor":
                if (GameState.act < 2)
                    Say(new List<Line> { new Line("镇卫生院。百叶窗放了下来，夜里不开诊。\n窗玻璃上贴着一张手写的作息表。") });
                else
                    ui.FadeTo(() => LoadRoom("clinic", 120, 238, "right"));
                break;

            /* ---------- 码头 ---------- */
            case "wu": TalkWu(); break;
            case "boat":
                Once("boat", 5);
                Say(new List<Line> {
                    new Line("「灰鲸号」沉没之后，镇里的渔船都改了名。\n这条船船头的漆字，被人用砂纸磨掉了。"),
                    new Line("（改名，就能当作什么都没发生过吗？）", "沈砚") });
                break;
            case "whistle":
                Once("whistle", 5);
                Say(new List<Line> {
                    new Line("缆绳上缠着一枚旧铜哨——\n是守灯人向海面传信号用的东西。"),
                    new Line("（守灯人的哨子……为什么会缠在这里？）", "沈砚") });
                break;

            /* ---------- 哥哥的房间 ---------- */
            case "diary":
                if (GameState.act >= 3) { Say(new List<Line> { new Line("日记的锁已经锈死了。\n（哥哥真正想说的话——都写在信里了。）") }); break; }
                Say(new List<Line> {
                    new Line("一本黑皮日记，上了锁。\n钥匙不知被哥哥藏去了哪里。"),
                    new Line("（也许镇上有人知道钥匙的事……）", "沈砚"),
                    new Line("【日记将在第三幕开启】") });
                break;
            case "shell":
                Once("shell", 5);
                Say(new List<Line> {
                    new Line("窗台上放着一枚贝壳，湿漉漉的。\n——可这里是二楼，离海足足有两公里。"),
                    new Line("（是谁，把它从海边带到了这里？）", "沈砚") });
                break;
            case "tide":
                Once("tide", 5);
                Say(new List<Line> {
                    new Line("墙上贴着今年的潮汐表。\n满月那三天被红笔圈了又圈，旁边一行小字：\n「光在，他们在外。」"),
                    new Line("（和那封信上，一模一样的话……）", "沈砚") });
                break;
            case "pillow":
                if (F("gotLetter1")) { Say(new List<Line> { new Line("枕头下已经空了。\n那封信，现在就揣在你的口袋里。") }); break; }
                if (!F("metZhou") || !F("metWu"))
                {
                    Say(new List<Line> { new Line("（先别急着翻这里……\n去镇上打听一下哥哥的下落吧。）", "沈砚") });
                    break;
                }
                Say(new List<Line> { new Line("（枕头下面……压着一封信。\n信封上没有邮票，只写了一个「砚」字。）", "沈砚") },
                    () => { Hum(); ui.OpenLetter(StoryContent.Letter1, EndAct1); });
                break;

            /* ---------- 卫生院 ---------- */
            case "lan": TalkLan(); break;
            case "records":
                Once("records", 10);
                Say(new List<Line> {
                    new Line("沈墨的体检记录：\n【长期失眠】【神经性耳鸣】【耳膜异常内陷】\n医生批注：「患者自述夜里能听见海在叫人。」"),
                    new Line("（海在叫人……\n哥，你最后那几个月，到底听见了什么？）", "沈砚") });
                break;
            case "cabinet":
                Say(new List<Line> { new Line("玻璃药柜里，退烧药那一格空了一大半。\n最近镇上说胡话的病人，太多了。") });
                break;

            /* ---------- 档案室 ---------- */
            case "longtable":
                if (F("archiveDone")) { Say(new List<Line> { new Line("拼好的剪报静静躺在桌上。\n被涂掉的那行字，你已经看过了。") }); break; }
                if (GameState.FragCount() < 6)
                {
                    Say(new List<Line> { new Line($"长桌上摊着镇志的残页。\n报纸碎片还不够（{GameState.FragCount()}/6）——再找找。") });
                    break;
                }
                Say(new List<Line> { new Line("你把六张碎片铺在长桌上，按年月对齐……\n一幅不该存在的全貌，慢慢显形。") },
                    () => { Hum(); ui.OpenLetter(StoryContent.News, () =>
                    {
                        SetF("archiveDone"); Truth(15);
                        Obj("从码头尽头去海岬灯塔\n（挡路的缆绳，可以剪开了）");
                    }); });
                break;
            case "drawer":
                if (F("letter3")) { Say(new List<Line> { new Line("抽屉空了。") }); break; }
                Say(new List<Line> { new Line("铁皮抽屉没锁。最底层压着一摞旧档案——\n和一个没有寄出的信封。") },
                    () => GainLetter("letter3", StoryContent.Letter3));
                break;

            /* ---------- 海岬 / 塔内 / 塔顶 ---------- */
            case "capeSign":
                Say(new List<Line> {
                    new Line("「海岬危险 · 满月期间禁止靠近」\n落款不是镇政府，是一只按上去的黑色手印。"),
                    new Line("（手印很小。像是孩子的。）", "沈砚") });
                break;
            case "letter5":
                if (F("letter5")) { Say(new List<Line> { new Line("石缝里只剩潮湿的风。") }); break; }
                Say(new List<Line> { new Line("塔基的石缝里塞着一个信封，\n被一块小石头仔细压着，没被雨打湿。") },
                    () => GainLetter("letter5", StoryContent.Letter5));
                break;
            case "towerDoor":
                Say(new List<Line> { new Line("铁门虚掩着，门轴上的锈被人新近磨掉过。\n——最近，有人经常进出这里。") },
                    () => ui.FadeTo(() => LoadRoom("tower", 240, 238, "up")));
                break;
            case "roster":
                if (F("rosterRead")) { Say(new List<Line> { new Line("「沈墨 —— 永久」。\n那四个字，你看了一遍又一遍。") }); break; }
                Say(new List<Line> {
                    new Line("石墙上的刻痕层层叠叠——\n是守灯人的轮值表：\n「1963 周阿福」「1971 周阿福」「1981 吴守义」……"),
                    new Line("最新的一栏，刻痕还很新，\n连石粉都没来得及落尽：\n\n「沈墨 —— 永久」"),
                    new Line("（永久。\n原来哥哥不是失踪了。\n——他是去值班了。）", "沈砚")
                }, () => { SetF("rosterRead"); Truth(10); EndAct2(); });
                break;
            case "stairs":
                if (GameState.act >= 3) { ui.FadeTo(() => LoadRoom("top", 100, 238, "right")); break; }
                Say(new List<Line> {
                    new Line("旋梯向上，没入漆黑。\n潮湿的空气里有一股很淡的灯油味。"),
                    new Line("（上面还亮着吗……\n满月那天，答案自然会来。）", "沈砚"),
                    new Line("【塔顶将在第三幕开启】") });
                break;
            case "lamp": FinalChoice(); break;
            case "window":
                Say(new List<Line> { new Line("玻璃上结着盐霜。\n海面在满月下泛着不自然的银光——\n像是水下，也有一个月亮。") });
                break;
            case "watcherTop":
                Say(new List<Line> {
                    new Line("你站在光与雾的交界。\n那道身影也在看你——隔着一层玻璃。"),
                    new Line("「……砚。」", "???"),
                    new Line("（这个声音。\n这个声音，我找了整整三个月。）", "沈砚")
                }, () => EndGame("C"));
                break;

            default:
                if (id.StartsWith("frag"))
                {
                    if (F(id)) break;
                    SetF(id); Truth(2);
                    int n = GameState.FragCount();
                    Say(new List<Line> { new Line($"（捡起一张发黄的报纸碎片。{n}/6）\n纸边有烧过的痕迹。") });
                    if (n >= 6) Obj("去长桌上拼合剪报");
                }
                break;
        }
    }

    /* ---------- NPC 对话 ---------- */
    void TalkZhou()
    {
        if (GameState.act == 3)
        {
            Say(new List<Line> { new Line("（她提前关了店门，只在窗台留了一盏灯）\n今晚……别回头。", "周婶") });
            return;
        }
        if (GameState.act == 2 && !F("metZhou2"))
        {
            Say(new List<Line> {
                new Line("（往门外瞅了瞅，压低声音）\n卫生院那丫头是外乡人，心是好的……你去找她，没错。", "周婶"),
                new Line("档案室的事——老婆子我可什么都没说。\n（她把一包烟丝塞进你口袋，像打发一个常客）", "周婶"),
                new Line("（她什么都知道。她只是选择了不说。）", "沈砚")
            }, () => { SetF("metZhou2"); Truth(5); });
            return;
        }
        if (GameState.act == 2) { Say(new List<Line> { new Line("雾要涨了。\n满月前，早点回屋。", "周婶") }); return; }
        Say(new List<Line> {
            new Line("哟——这不是沈家二小子嘛！啥时候回来的？", "周婶"),
            new Line("周婶。我哥……最近来店里过吗？", "沈砚"),
            new Line("你哥啊……早搬走啦，去大城市享福咯。", "周婶"),
            new Line("他三个月没跟家里联系了。", "沈砚"),
            new Line("（擦手的动作停了一下，声音压低）\n……你，没收到他最后一封信？", "周婶",
                new List<Choice> {
                    new Choice("「什么信？」—— 追问下去", () => Truth(15),
                        new Line("没、没什么！老婆子我老糊涂喽……\n（她转身去擦一个已经很干净的柜台）", "周婶"),
                        new Line("（她在撒谎。这个镇上，每个人都在撒谎。）", "沈砚"),
                        new Line("走吧走吧，要打烊了。\n……满月前，别去海岬那边。", "周婶")),
                    new Choice("「可能他只是忙。」—— 装作不在意", () => Trust(15),
                        new Line("（叹了口气，往你手里塞了把旧伞）\n夜里雾大，当心病气。你哥……是个好人。", "周婶"),
                        new Line("（她的眼神躲开了。\n这把伞——是哥哥留在店里的。）", "沈砚"))
                })
        }, () => { SetF("metZhou"); CheckObjective(); });
    }

    void TalkWu()
    {
        if (GameState.act == 3)
        {
            Say(new List<Line> { new Line("（他今晚一滴酒没沾，直直地看着你）\n满月了。……替我向你哥，敬一杯。", "老吴") });
            return;
        }
        if (GameState.act == 2 && !F("metWu2"))
        {
            Say(new List<Line> {
                new Line("（难得没喝酒）海岬那条路，封了二十年。\n……总得有人上去看看。", "老吴"),
                new Line("二十年前那个晚上，我也在船上。\n别以为喝醉了，就能忘掉。", "老吴"),
                new Line("（他的手动了一下，\n像是想去摸那枚铜哨。）", "沈砚")
            }, () => { SetF("metWu2"); Truth(5); });
            return;
        }
        var L = new List<Line> {
            new Line("（酒气）……灯塔？那玩意儿早死了。二十年前就死透了。", "老吴"),
            new Line("那海岬上偶尔亮的光，是什么？", "沈砚"),
            new Line("（酒醒了一半）你看错了。雾大，看什么都像光。", "老吴")
        };
        if (GameState.trust >= 35)
        {
            L.Add(new Line("（他盯着海看了很久很久）\n……你哥啊。他夜里常往那边走。鞋上全是泥，第二天还笑呵呵的。", "老吴"));
            L.Add(new Line("（他夜里去灯塔……做什么？）", "沈砚"));
        }
        else L.Add(new Line("回去吧，城里娃。这镇子，不欢迎刨根问底的人。", "老吴"));
        Say(L, () => { if (GameState.trust >= 35) Truth(10); SetF("metWu"); CheckObjective(); });
    }

    void TalkLan()
    {
        if (!F("metLan"))
        {
            Say(new List<Line> {
                new Line("你就是沈墨的弟弟吧？\n他来做体检的时候，提起过你。", "阿岚"),
                new Line("我哥……他来过卫生院？", "沈砚"),
                new Line("三次。长期失眠，耳膜有异常的内陷——\n像是常年贴着某种很低、很低的声音睡觉。", "阿岚"),
                new Line("（她拉开抽屉，取出一串钥匙和一个信封）\n这些是他落在这儿的。档案室的钥匙……\n他说：「迟早有人会用得上。」", "阿岚"),
                new Line("（钥匙很新。\n——他是早就配好了，等一个人来。）", "沈砚"),
                new Line("还有，满月前后，夜里别出门。\n最近走错夜路的人都在发烧说胡话。\n他们喊的，不是亲人的名字。", "阿岚")
            }, () =>
            {
                SetF("metLan"); SetF("hasKey"); Truth(10);
                Obj("用钥匙打开档案室\n（老街中段 · 钉死的木门）");
                GainLetter("letter2", StoryContent.Letter2);
            });
            return;
        }
        Say(new List<Line> { new Line("档案室在老街中段。\n看到被涂掉的字，别声张——\n有人不想让纸说话。", "阿岚") });
    }

    void CheckObjective()
    {
        if (F("metZhou") && F("metWu") && !F("objHome"))
        {
            SetF("objHome");
            Obj("去哥哥生前的住处看看\n街道北侧，那扇蓝门老屋");
        }
    }

    /* ---------- 终局抉择 ---------- */
    void FinalChoice()
    {
        var chs = new List<Choice> {
            new Choice("添满灯油 —— 继续守灯", () => GameState.pendingEnd = "A",
                new Line("你提起油壶，手很稳。\n火苗窜起的那一刻，光柱刺破雾墙。\n海面，重新安静下来。")),
            new Choice("拧熄它 —— 让真相上岸", () => GameState.pendingEnd = "B",
                new Line("你拧动灯阀。\n火苗挣扎了一下，熄了。\n黑暗里，潮水开始退——退得很远、很远。"))
        };
        if (GameState.LetterCount() >= 5)
            chs.Add(new Choice("把五封信浸进灯油 —— 一起烧了", () => GameState.pendingEnd = "D",
                new Line("五封信，在灯油里浸透。\n你把它们，连同二十年的谎言，一起点燃。\n火光，比灯塔亮得多。")));
        Say(new List<Line> {
            new Line("油灯将尽，灯芯噼啪作响。\n窗外，满月把海面照成一面银镜。"),
            new Line("（光在，他们在外。光灭，他们回家。\n哥——这一次，换我来选。）", "沈砚", chs)
        }, () => EndGame(GameState.pendingEnd));
    }
}

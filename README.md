# 雾港疑云 · Unity 版

像素风悬疑叙事游戏（三幕 + 四结局）的 Unity 重构版。
从 HTML/JS 原型完整移植：美术为原型渲染管线离线烘焙的分层像素图，玩法逻辑全部用 C# 重写。

## 环境

- Unity **2022.3 LTS**（本机验证版本：2022.3.45f1）

## 快速开始

1. 用 Unity Hub 打开本目录（fog-harbor-unity）。
2. 等待首次导入完成，然后执行菜单：**雾港疑云 → 生成游戏场景**
   （会生成 `Assets/Scenes/Game.unity` 并加入 Build Settings）
3. 打开该场景，点 **Play**。

整个游戏由一个 `Game` 组件在运行时从代码装配：房间、UI、角色、天气、对话全部由脚本生成，美术资源通过 `Resources.Load` 加载。

## 操作

- 桌面：方向键 / WASD 移动，`E` / 空格 调查与推进对话，`Esc` 跳过剧情，`R` 幕尾重开
- 手机：自动显示虚拟方向键 + E 键（移动端构建时）

## 内容

- 第一幕 · 抵达：汽车站 → 老街 → 码头 → 哥哥的房间（真相度 / 镇民信任双数值）
- 第二幕 · 潮水：卫生院 · 阿岚 → 档案室拼报解谜 → 海岬灯塔 + 塔内轮值表
- 第三幕 · 满月：塔顶终局抉择 → 结局 A《守灯人》/ B《退潮》/ C《看灯人》/ 隐藏 D《灰礁》
- 收集：哥哥的信 ×5，影响隐藏结局与「看灯人」现身（真相度 < 80）

## 目录结构

```
Assets/
  Resources/Art/
    Rooms/    9 个房间 × 远/中/近/前 四层烘焙像素图（含视差与远景模糊）
    Sprites/  角色精灵（4 向 2 帧 + 描边）
    FX/       光束 / 光锥 / 灯晕 / 雾团 / 云 / 阴影 / 雨雾带
  Scripts/
    Types.cs          数据结构 + 坐标换算
    GameState.cs      （并入 Types.cs）全局状态
    StoryContent.cs   剧情文本（开场/信件/剪报/结局）
    RoomData.cs       房间数据（移植 JS 场景定义）
    Game.cs           主逻辑（房间装配/相机/视差/三幕流程）
    Interactions.cs   全部交互与 NPC 对话（含分支）
    PlayerController.cs 玩家/NPC（移动/动画/纵深缩放）
    UIManager.cs      代码生成的 UI（剧情/对话/信件/HUD/结局）
    FX.cs             光束/路灯/泊船/看灯人/火苗/月亮/雨雾云
    TouchUI.cs        触屏虚拟按键
  Editor/
    ArtImport.cs      像素资产导入设置（Point 过滤/轴心/PPU 100）
    GameBuilder.cs    一键生成游戏场景
```

## 与 JS 原型的差异

- 新增**程序化环境音**（雨声白噪 + 关键线索低频嗡鸣，零音频资产，`M` 静音）
- 新增**幕级存档点**：开场界面出现「继续游戏」按钮，可从第二/三幕继续
- 新增**结局图鉴**：结局页显示「已解锁结局 x/4」（PlayerPrefs 持久化）
- 其余剧情、数值、分支、结局逻辑与在线版一致：
  https://2126431683a-netizen.github.io/fog-harbor/

## 已知限制

- 本机 Unity 批量模式（-batchmode）受许可证限制无法做无人值守编译验证；首次打开若 Console 报错请反馈
- WebGL 构建模块（PlaybackEngines/WebGLSupport）未安装，如需网页版构建请先在 Unity Hub 安装该模块

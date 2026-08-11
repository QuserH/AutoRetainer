# -*- coding: utf-8 -*-
"""
AutoRetainer 汉化补丁脚本：重放 localize.py 无法处理的"手工结构性改动"。

用法（上游同步后）：
    python tools/localize.py     # 1. 词典自动翻译
    python tools/patch_cn.py     # 2. 重放本补丁

匹配原则：
- 普通 UI 文本按"localize 之后"的状态（中文）匹配
- ID / 嵌套插值 / Lang.cs（localize 跳过）按英文原文匹配
- 任何匹配失败都会报错并列出，提示人工处理（说明上游改动过该处）
"""
import io
import os
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

def load_enum_dicts():
    return '''    internal static readonly ReadOnlyDictionary<OpenBellBehavior, string> OpenBellBehaviorNames = new(new Dictionary<OpenBellBehavior, string>()
    {
        { OpenBellBehavior.Do_nothing, "不执行任何操作" },
        { OpenBellBehavior.Enable_AutoRetainer, "启用 AutoRetainer" },
        { OpenBellBehavior.Disable_AutoRetainer, "禁用 AutoRetainer" },
        { OpenBellBehavior.Pause_AutoRetainer, "暂停 AutoRetainer" },
    });

    internal static readonly ReadOnlyDictionary<TaskCompletedBehavior, string> TaskCompletedBehaviorNames = new(new Dictionary<TaskCompletedBehavior, string>()
    {
        { TaskCompletedBehavior.Close_retainer_list_and_disable_plugin, "关闭雇员列表并禁用插件" },
        { TaskCompletedBehavior.Close_retainer_list_and_keep_plugin_enabled, "关闭雇员列表并保持插件启用" },
        { TaskCompletedBehavior.Stay_in_retainer_list_and_disable_plugin, "停留在雇员列表并禁用插件" },
        { TaskCompletedBehavior.Stay_in_retainer_list_and_keep_plugin_enabled, "停留在雇员列表并保持插件启用" },
    });

    internal static readonly ReadOnlyDictionary<CutsceneSkipMode, string> CutsceneSkipModeNames = new(new Dictionary<CutsceneSkipMode, string>()
    {
        { CutsceneSkipMode.Never, "从不" },
        { CutsceneSkipMode.When_Multi_Mode_is_on, "多角色模式开启时" },
        { CutsceneSkipMode.Always, "总是" },
    });

    internal static readonly ReadOnlyDictionary<VesselBehavior, string> VesselBehaviorNames = new(new Dictionary<VesselBehavior, string>()
    {
        { VesselBehavior.Finalize, "收尾（不再部署）" },
        { VesselBehavior.Redeploy, "重新部署" },
        { VesselBehavior.LevelUp, "升级" },
        { VesselBehavior.Unlock, "解锁" },
        { VesselBehavior.Use_plan, "使用方案" },
    });

    internal static readonly ReadOnlyDictionary<GCDeliveryType, string> GCDeliveryTypeNames = new(new Dictionary<GCDeliveryType, string>()
    {
        { GCDeliveryType.Disabled, "禁用" },
        { GCDeliveryType.Hide_Armoury_Chest_Items, "隐藏装备箱物品" },
        { GCDeliveryType.Hide_Gear_Set_Items, "隐藏套装物品" },
        { GCDeliveryType.Show_All_Items, "显示所有物品" },
    });

    internal static readonly ReadOnlyDictionary<MultiModeType, string> MultiModeTypeNames = new(new Dictionary<MultiModeType, string>()
    {
        { MultiModeType.Retainers, "雇员" },
        { MultiModeType.Submersibles, "潜水艇" },
        { MultiModeType.Everything, "全部" },
    });

    internal static readonly ReadOnlyDictionary<UnavailableVentureDisplay, string> UnavailableVentureDisplayNames = new(new Dictionary<UnavailableVentureDisplay, string>()
    {
        { UnavailableVentureDisplay.Hide, "隐藏" },
        { UnavailableVentureDisplay.Display, "显示" },
        { UnavailableVentureDisplay.Allow_selection, "允许选择" },
    });

    internal static readonly ReadOnlyDictionary<PlanCompleteBehavior, string> PlanCompleteBehaviorNames = new(new Dictionary<PlanCompleteBehavior, string>()
    {
        { PlanCompleteBehavior.Restart_plan, "重新开始方案" },
        { PlanCompleteBehavior.Assign_Quick_Venture, "指派快速探险" },
        { PlanCompleteBehavior.Do_nothing, "不执行任何操作" },
        { PlanCompleteBehavior.Repeat_last_venture, "重复上次的探险委托" },
    });

'''

PATCHES = [
    # ============ Lang.cs ============
    {
        "file": r"AutoRetainer\Lang.cs",
        "old": '''        { UnlockMode.MultiSelect, "Pick max amount of destinations" },
        { UnlockMode.SpamOne, "Spam one destination" },
        { UnlockMode.WhileLevelling, "Include one unlock destination while levelling" },''',
        "new": '''        { UnlockMode.MultiSelect, "选择最大数量的目的地" },
        { UnlockMode.SpamOne, "反复部署单个目的地" },
        { UnlockMode.WhileLevelling, "升级期间包含一个解锁目的地" },''',
    },
    {
        "file": r"AutoRetainer\Lang.cs",
        "old": "    internal static readonly (string Normal, string GameFont) Digits =",
        "new": load_enum_dicts() + "    internal static readonly (string Normal, string GameFont) Digits =",
    },
    # ============ ExpertTab.cs：行为设置下拉 ============
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '.EnumComboFullWidth(null, "存取雇员铃铛时若无可用探险任务的动作：", () => ref C.OpenBellBehaviorNoVentures)',
        "new": '.EnumComboFullWidth(null, "存取雇员铃铛时若无可用探险任务的动作：", () => ref C.OpenBellBehaviorNoVentures, null, Lang.OpenBellBehaviorNames)',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '.EnumComboFullWidth(null, "存取雇员铃铛时若有可用探险任务的动作：", () => ref C.OpenBellBehaviorWithVentures)',
        "new": '.EnumComboFullWidth(null, "存取雇员铃铛时若有可用探险任务的动作：", () => ref C.OpenBellBehaviorWithVentures, null, Lang.OpenBellBehaviorNames)',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '.EnumComboFullWidth(null, "存取铃铛后任务完成的行为：", () => ref C.TaskCompletedBehaviorAccess)',
        "new": '.EnumComboFullWidth(null, "存取铃铛后任务完成的行为：", () => ref C.TaskCompletedBehaviorAccess, null, Lang.TaskCompletedBehaviorNames)',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '.EnumComboFullWidth(null, "手动启用后任务完成的行为：", () => ref C.TaskCompletedBehaviorManual)',
        "new": '.EnumComboFullWidth(null, "手动启用后任务完成的行为：", () => ref C.TaskCompletedBehaviorManual, null, Lang.TaskCompletedBehaviorNames)',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '.EnumComboFullWidth(null, "插件运作期间任务完成的行为：", () => ref C.TaskCompletedBehaviorAuto)',
        "new": '.EnumComboFullWidth(null, "插件运作期间任务完成的行为：", () => ref C.TaskCompletedBehaviorAuto, null, Lang.TaskCompletedBehaviorNames)',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\AdvancedEntries\ExpertTab.cs",
        "old": '            if(ImGuiEx.EnumCombo(text, ref C.CutsceneSkipMode))',
        "new": '            if(ImGuiEx.EnumCombo(text, ref C.CutsceneSkipMode, null, Lang.CutsceneSkipModeNames))',
    },
    # ============ VenturePlanner.cs ============
    {
        "file": r"AutoRetainer\UI\Windows\VenturePlanner.cs",
        "old": '                    ImGuiEx.EnumCombo("##cBeh", ref adata.VenturePlan.PlanCompleteBehavior);',
        "new": '                    ImGuiEx.EnumCombo("##cBeh", ref adata.VenturePlan.PlanCompleteBehavior, null, Lang.PlanCompleteBehaviorNames);',
    },
    {
        "file": r"AutoRetainer\UI\Windows\VenturePlanner.cs",
        "old": '                    ImGuiEx.EnumCombo("##unavail", ref C.UnavailableVentureDisplay);',
        "new": '                    ImGuiEx.EnumCombo("##unavail", ref C.UnavailableVentureDisplay, null, Lang.UnavailableVentureDisplayNames);',
    },
    # ============ DeployablesTab.cs ============
    {
        "file": r"AutoRetainer\UI\NeoUI\DeployablesTab.cs",
        "old": '        ImGuiEx.EnumCombo("##behavior", ref MassBehavior);',
        "new": '        ImGuiEx.EnumCombo("##behavior", ref MassBehavior, null, Lang.VesselBehaviorNames);',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\DeployablesTab.cs",
        "old": '                ImGuiEx.EnumCombo($"##behavior{entry.GUID}", ref entry.VesselBehavior);',
        "new": '                ImGuiEx.EnumCombo($"##behavior{entry.GUID}", ref entry.VesselBehavior, null, Lang.VesselBehaviorNames);',
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\DeployablesTab.cs",
        "old": '                    ImGuiEx.EnumCombo($"##firstSubBehavior{entry.GUID}", ref entry.FirstSubVesselBehavior);',
        "new": '                    ImGuiEx.EnumCombo($"##firstSubBehavior{entry.GUID}", ref entry.FirstSubVesselBehavior, null, Lang.VesselBehaviorNames);',
    },
    # ============ WorkshopUI.cs ============
    {
        "file": r"AutoRetainer\UI\MainWindow\WorkshopUI.cs",
        "old": '            ImGuiEx.EnumCombo("##vbeh", ref adata.VesselBehavior);',
        "new": '            ImGuiEx.EnumCombo("##vbeh", ref adata.VesselBehavior, null, Lang.VesselBehaviorNames);',
    },
    # ============ AutoGCHandinOverlay.cs ============
    {
        "file": r"AutoRetainer\UI\Overlays\AutoGCHandinOverlay.cs",
        "old": '            ImGuiEx.EnumCombo("##mode", ref d.GCDeliveryType);',
        "new": '            ImGuiEx.EnumCombo("##mode", ref d.GCDeliveryType, null, Lang.GCDeliveryTypeNames);',
    },
    # ============ GCCharacterConfiguration.cs ============
    {
        "file": r"AutoRetainer\UI\NeoUI\InventoryManagementEntries\GCDeliveryEntries\GCCharacterConfiguration.cs",
        "old": '                ImGuiEx.EnumCombo("##deliveryMode", ref characterData.GCDeliveryType);',
        "new": '                ImGuiEx.EnumCombo("##deliveryMode", ref characterData.GCDeliveryType, null, Lang.GCDeliveryTypeNames);',
    },
    # ============ CharaConfig.cs ============
    {
        "file": r"AutoRetainer\UI\MainWindow\MultiModeTab\CharaConfig.cs",
        "old": '                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType);',
        "new": '                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType, null, Lang.GCDeliveryTypeNames);',
    },
    # ============ AutoRetainerWindow.cs：多角色模式类型 + 会话过期 ============
    {
        "file": r"AutoRetainer\UI\MainWindow\AutoRetainerWindow.cs",
        "old": '                ImGuiEx.EnumCombo("##mode", ref C.MultiModeType);',
        "new": '                ImGuiEx.EnumCombo("##mode", ref C.MultiModeType, null, Lang.MultiModeTypeNames);',
    },
    {
        "file": r"AutoRetainer\UI\MainWindow\AutoRetainerWindow.cs",
        "old": '                return $"Session expires in {time.Days} day{(time.Days == 1 ? "" : "s")}" + (time.Hours > 0 ? $" {time.Hours} hours" : "");',
        "new": '                return $"会话将在 {time.Days} 天后过期" + (time.Hours > 0 ? $"（还有 {time.Hours} 小时）" : "");',
    },
    {
        "file": r"AutoRetainer\UI\MainWindow\AutoRetainerWindow.cs",
        "old": '                    return $"Session expires in {time.Hours} hours";',
        "new": '                    return $"会话将在 {time.Hours} 小时后过期";',
    },
    {
        "file": r"AutoRetainer\UI\MainWindow\AutoRetainerWindow.cs",
        "old": '                    return $"Session expires in less than an hour";',
        "new": '                    return $"会话将在不到一小时后过期";',
    },
    {
        "file": r"AutoRetainer\UI\MainWindow\AutoRetainerWindow.cs",
        "old": '            return "Session expired";',
        "new": '            return "会话已过期";',
    },
    # ============ LoginOverlay.cs：服务账号 ============
    {
        "file": r"AutoRetainer\UI\Overlays\LoginOverlay.cs",
        "old": 'names: userServiceAccounts.ToDictionary(x => x, x => x == -1 ? "All service accounts" : $"Service account {x + 1}"))',
        "new": 'names: userServiceAccounts.ToDictionary(x => x, x => x == -1 ? "所有服务账号" : $"服务账号 {x + 1}"))',
    },
    # ============ AutoRetainer.cs：/autoretainer 命令帮助 ============
    {
        "file": r"AutoRetainer\AutoRetainer.cs",
        "old": '''        EzCmd.Add("/autoretainer", CommandHandler, \"\"\"
            Open plugin interface
            /ays - alias for /autoretainer
            /autoretainer e|enable → Enable plugin
            /autoretainer d|disable - Disable plugin
            /autoretainer t|toggle - toggle plugin
            /autoretainer m|multi - toggle MultiMode
            /autoretainer relog Character Name@WorldName - relog to the targeted character if configured
            /autoretainer b|browser - open venture browser
            /autoretainer expert - toggle expert settings
            /autoretainer debug - toggle debug menu and verbose output
            /autoretainer shutdown <hours> [minutes] [seconds] - schedule a game shutdown in this amount of time
            /autoretainer itemsell - begin selling items to NPC or retainer if possible
            /autoretainer het - enter nearby own house or apartment if possible
            /autoretainer reset - reset all pending tasks
            /autoretainer deliver - deliver expert delivery items
            /autoretainer armoire - deliver all eligible items into armoire
            /autoretainer dresser - deliver all eligible items into glamour dresser (requires Glamour Log plugin)
            \"\"\");''',
        "new": '''        EzCmd.Add("/autoretainer", CommandHandler, \"\"\"
            打开插件界面
            /ays - /autoretainer 的别名
            /autoretainer e|enable - 启用插件
            /autoretainer d|disable - 禁用插件
            /autoretainer t|toggle - 开关插件
            /autoretainer m|multi - 开关多角色模式
            /autoretainer relog 角色名@服务器名 - 重新登录到目标角色（如已配置）
            /autoretainer b|browser - 打开探险委托浏览器
            /autoretainer expert - 开关专家设置
            /autoretainer debug - 开关调试菜单与详细输出
            /autoretainer shutdown <小时> [分钟] [秒] - 计划在此时间后关闭游戏
            /autoretainer itemsell - 开始向 NPC 或雇员出售物品（如可能）
            /autoretainer het - 进入附近自己的房屋或公寓（如可能）
            /autoretainer reset - 重置所有待处理任务
            /autoretainer deliver - 交付专家交付物品
            /autoretainer armoire - 将所有符合条件的物品放入置衣柜
            /autoretainer dresser - 将所有符合条件的物品放入投影柜（需要 Glamour Log 插件）
            \"\"\");''',
    },
    # ============ AutoRetainer.json 清单 ============
    {
        "file": r"AutoRetainer\AutoRetainer.json",
        "old": '''  "Name": "AutoRetainer",
  "InternalName": "AutoRetainer",
  "Punchline": "Collect and assign ventures to your retainers from the comfort of your bed.",
  "Description": "Collect and assign ventures to your retainers from the comfort of your bed.",''',
        "new": '''  "Name": "AutoRetainer (自动雇员)",
  "InternalName": "AutoRetainer",
  "Punchline": "躺在床上就能收取并指派你的雇员冒险委托。",
  "Description": "躺在床上就能收取并指派你的雇员冒险委托。\\n\\n主要功能：\\n\\t- 一键指派/重新指派雇员冒险委托，自动处理繁琐的确认弹窗\\n\\t- 一键部署远征探险（飞空艇/潜水艇）\\n\\t- 创建出售列表，自动清理垃圾物品\\n\\t- 创建存放列表，将贵重物品存入雇员以节省背包空间",''',
    },
    # ============ GCContinuation.cs：修复军票换探险币每次只买 1 个 ============
    # 原实现：打开货币兑换弹窗（默认数量 1），再靠硬编码 UI 节点索引
    # （GrandCompanyExchange.NodeList[52] 读军票数 / ShopExchangeCurrencyDialog.NodeList[8] 写数量）
    # 改弹窗数字。节点错位时该步静默超时跳过（abortOnTimeout:false），按默认值 1 兑换。
    # 修复：学 Deliveroo 把数量直接放进回调第 3 个参数、第 6 参传 false 跳过弹窗，不再依赖节点索引。
    {
        "file": r"AutoRetainer\Modules\GcHandin\GCContinuation.cs",
        "old": "Callback.Fire(addon, true, 0, i, 1, Callback.ZeroAtkValue, canPurchase, itemInfo.OpenCurrencyExchange, itemInfo.ItemID, itemInfo.IconID, itemInfo.Seals);",
        "new": "Callback.Fire(addon, true, 0, i, (int)amount, Callback.ZeroAtkValue, canPurchase, false, Callback.ZeroAtkValue, Callback.ZeroAtkValue, Callback.ZeroAtkValue);",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\GCContinuation.cs",
        "old": '''        List<TaskManagerTask> tasks = [];
        if(listing.ItemID == VentureItem)
        {
            tasks.Add(new(() => SetVenturesExchangeAmount((int)itemCount), conf));
            tasks.Add(new(SelectExchange, conf));
        }
        tasks.Add(new(ConfirmExchange, conf));''',
        "new": '''        List<TaskManagerTask> tasks = [];
        // CN patch: 探险币兑换不再经由货币弹窗（数量已在回调中直接传入）
        tasks.Add(new(ConfirmExchange, conf));''',
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\AutoGCHandin.cs",
        "old": "    internal static bool Operation = false;",
        "new": "    internal static bool Operation = false;\n    private static long HandinDelayUntil;\n    private static uint HandinDelayItem;\n\n    private static bool InvokeHandinWithDelay(AtkUnitBase* addon, int which, uint itemId)\n    {\n        if(HandinDelayItem != itemId)\n        {\n            HandinDelayItem = itemId;\n            HandinDelayUntil = Environment.TickCount64 + GetRandomDelayMilliseconds(C.GCHandinDelayMinSeconds, C.GCHandinDelayMaxSeconds);\n        }\n        if(Environment.TickCount64 < HandinDelayUntil) return false;\n        HandinDelayItem = 0;\n        InvokeHandin(addon, which);\n        return true;\n    }\n\n    internal static long GetRandomDelayMilliseconds(float minimumSeconds, float maximumSeconds)\n    {\n        var minimumCentiseconds = ToCentiseconds(minimumSeconds);\n        var maximumCentiseconds = Math.Clamp(ToCentiseconds(maximumSeconds), minimumCentiseconds, 360000);\n        return Random.Shared.NextInt64(minimumCentiseconds, maximumCentiseconds + 1) * 10L;\n    }\n\n    private static int ToCentiseconds(float seconds)\n    {\n        if(!float.IsFinite(seconds)) return 0;\n        return (int)MathF.Round(Math.Clamp(seconds, 0f, 3600f) * 100f, MidpointRounding.AwayFromZero);\n    }",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\AutoGCHandin.cs",
        "old": "                                    InvokeHandin(addon, nextItem.Value.Index);",
        "new": "                                    InvokeHandinWithDelay(addon, nextItem.Value.Index, nextItem.Value.ItemID);",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\AutoGCHandin.cs",
        "old": "    internal static void Tick()\n    {",
        "new": "    internal static void Tick()\n    {\n        if(!Operation) HandinDelayItem = 0;",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\GCContinuation.cs",
        "old": "        P.TaskManager.Enqueue(GCContinuation.InteractWithShop);\n        P.TaskManager.Enqueue(BeginNewPurchase);",
        "new": "        P.TaskManager.Enqueue(GCContinuation.InteractWithShop);\n        P.TaskManager.Enqueue(() => GCContinuation.ExchangeEntryDelayReady());\n        P.TaskManager.Enqueue(BeginNewPurchase);",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\GCContinuation.cs",
        "old": "    public static bool PurchaseItem(this GCExchangeItem item)",
        "new": "    private static long ExchangeDelayUntil;\n    private static uint ExchangeDelayItem;\n    private static long ExchangeEntryDelayUntil;\n    private static bool ExchangeEntryDelayPending;\n\n    private static bool ExchangeDelayReady(uint itemId)\n    {\n        if(ExchangeDelayItem != itemId)\n        {\n            ExchangeDelayItem = itemId;\n            ExchangeDelayUntil = Environment.TickCount64 + AutoGCHandin.GetRandomDelayMilliseconds(C.GCExchangeDelayMinSeconds, C.GCExchangeDelayMaxSeconds);\n        }\n        if(Environment.TickCount64 < ExchangeDelayUntil) return false;\n        ExchangeDelayItem = 0;\n        return true;\n    }\n\n    private static bool ExchangeEntryDelayReady()\n    {\n        if(!ExchangeEntryDelayPending)\n        {\n            ExchangeEntryDelayPending = true;\n            ExchangeEntryDelayUntil = Environment.TickCount64 + AutoGCHandin.GetRandomDelayMilliseconds(C.GCExchangeDelayMinSeconds, C.GCExchangeDelayMaxSeconds);\n        }\n        if(Environment.TickCount64 < ExchangeEntryDelayUntil) return false;\n        ExchangeEntryDelayPending = false;\n        return true;\n    }\n\n    public static bool PurchaseItem(this GCExchangeItem item)",
    },
    {
        "file": r"AutoRetainer\Modules\GcHandin\GCContinuation.cs",
        "old": "        var amount = item.GetAmountThatCanBePurchased();",
        "new": "        var amount = item.GetAmountThatCanBePurchased();\n        if(item.ItemID == VentureItem && !ExchangeDelayReady(item.ItemID)) return false;",
    },
    # ============ randomized GC delay configuration ============
    {
        "file": r"AutoRetainer\PluginData\Config.cs",
        "old": "    public int FullAutoGCDeliveryDeliverOnVentureLessThan = 50;\n    public bool VendorItemAfterVoyage = false;",
        "new": "    public int FullAutoGCDeliveryDeliverOnVentureLessThan = 50;\n    public float GCHandinDelayMinSeconds = 0;\n    public float GCHandinDelayMaxSeconds = 0;\n    public float GCExchangeDelayMinSeconds = 0;\n    public float GCExchangeDelayMaxSeconds = 0;\n    public bool VendorItemAfterVoyage = false;",
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\InventoryManagementEntries\GCDeliveryEntries\GeneralSettings.cs",
        "old": "        .Checkbox(\"启用自动筹备交换\", () => ref C.AutoGCContinuation)",
        "new": "        .Checkbox(\"启用自动筹备交换\", () => ref C.AutoGCContinuation)\n        .Widget(() =>\n        {\n            ImGuiEx.TextWrapped(\"提交装备与兑换探险币的额外随机延迟（单位：秒，保留两位小数；最小值和最大值均为 0 表示不延迟）\");\n            ImGui.SetNextItemWidth(120f);\n            ImGui.InputFloat(\"提交装备延迟最小值\", ref C.GCHandinDelayMinSeconds, 0.01f, 0.10f, \"%.2f\");\n            ImGui.SetNextItemWidth(120f);\n            ImGui.InputFloat(\"提交装备延迟最大值\", ref C.GCHandinDelayMaxSeconds, 0.01f, 0.10f, \"%.2f\");\n            ImGui.SetNextItemWidth(120f);\n            ImGui.InputFloat(\"兑换探险币延迟最小值\", ref C.GCExchangeDelayMinSeconds, 0.01f, 0.10f, \"%.2f\");\n            ImGui.SetNextItemWidth(120f);\n            ImGui.InputFloat(\"兑换探险币延迟最大值\", ref C.GCExchangeDelayMaxSeconds, 0.01f, 0.10f, \"%.2f\");\n            C.GCHandinDelayMinSeconds = NormalizeDelay(C.GCHandinDelayMinSeconds);\n            C.GCHandinDelayMaxSeconds = Math.Max(C.GCHandinDelayMinSeconds, NormalizeDelay(C.GCHandinDelayMaxSeconds));\n            C.GCExchangeDelayMinSeconds = NormalizeDelay(C.GCExchangeDelayMinSeconds);\n            C.GCExchangeDelayMaxSeconds = Math.Max(C.GCExchangeDelayMinSeconds, NormalizeDelay(C.GCExchangeDelayMaxSeconds));\n        })",
    },
    {
        "file": r"AutoRetainer\UI\NeoUI\InventoryManagementEntries\GCDeliveryEntries\GeneralSettings.cs",
        "old": "        ;\n}",
        "new": "        ;\n\n    private static float NormalizeDelay(float seconds)\n    {\n        if(!float.IsFinite(seconds)) return 0f;\n        return Math.Clamp(MathF.Round(seconds, 2, MidpointRounding.AwayFromZero), 0f, 3600f);\n    }\n}",
    },
]


def main():
    ok = 0
    failed = []
    for p in PATCHES:
        fp = os.path.join(ROOT, p["file"])
        if not os.path.exists(fp):
            failed.append((p["file"], "文件不存在"))
            continue
        with io.open(fp, encoding="utf-8", errors="replace") as f:
            text = f.read()
        if p["old"] not in text:
            # 幂等：目标文本已存在（补丁曾直接提交进源码，或已应用过）
            if p["new"] and p["new"] in text:
                ok += 1
                continue
            failed.append((p["file"], "匹配失败（上游可能改动过此处）"))
            continue
        if p["new"] in text and text.count(p["old"]) == text.count(p["new"]):
            ok += 1
            continue
        text = text.replace(p["old"], p["new"])
        with io.open(fp, "w", encoding="utf-8", newline="") as f:
            f.write(text)
        ok += 1

    print(f"补丁应用成功: {ok}/{len(PATCHES)}")
    if failed:
        print("以下补丁失败，需要人工处理:")
        for f, reason in failed:
            print(f"  - {f}: {reason}")
        sys.exit(1)


if __name__ == "__main__":
    main()

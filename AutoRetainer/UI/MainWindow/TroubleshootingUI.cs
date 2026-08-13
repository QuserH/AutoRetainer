using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainerAPI.Configuration;
using Dalamud.Game;
using ECommons.GameHelpers;
using ECommons.Reflection;

namespace AutoRetainer.UI.MainWindow;
public static unsafe class TroubleshootingUI
{
    private static readonly Config EmptyConfig = new();

    public static bool IsPluginInstalled(string name)
    {
        return Svc.PluginInterface.InstalledPlugins.Any(x => x.IsLoaded && (x.InternalName.EqualsIgnoreCase(name) || x.Name.EqualsIgnoreCase(name)));
    }

    public static void Draw()
    {
        ImGuiEx.TextWrapped("此页面会检查配置中的常见问题，方便你在联系技术支持前自行排查。");

        if(!Player.Available)
        {
            ImGuiEx.TextWrapped($"未登录时无法进行故障排除。");
            return;
        }

        if(C.CutsceneSkipMode != AutoRetainerAPI.Configuration.CutsceneSkipMode.Never)
        {
            Info($"旅馆过场跳过模块已设置为 {C.CutsceneSkipMode}。AutoRetainer 将跳过旅馆过场动画。");
        }

        if(Data == null)
        {
            Error("找不到当前角色的数据。请打开传唤铃或远航探索管制面板，或重新登录以生成数据。");
        }

        if(C.IgnoreGCRankCheck)
        {
            Error("已启用忽略大国防联军（GC）等级检查。若要正常使用插件，请将其禁用。（/ays set IgnoreGCRankCheck false）");
        }

        if(!Svc.ClientState.ClientLanguage.EqualsAny(ClientLanguage.Japanese, ClientLanguage.German, ClientLanguage.French, ClientLanguage.English))
        {
            Warning("检测到非国际服客户端。当前汉化版已针对国服适配，但上游功能更新后仍可能出现国服特有的兼容性问题。");
        }

        if(C.DontLogout)
        {
            Error("已启用 DontLogout 调试选项。");
        }

        if(C.FullAutoGCDelivery) 
        {
            int maxRetainersWhenGcDelivery = 0;
            var warnSub = false;
            foreach(var x in C.OfflineData)
            {
                if(x.Enabled && x.GCDeliveryType != GCDeliveryType.Disabled)
                {
                    maxRetainersWhenGcDelivery = Math.Max(maxRetainersWhenGcDelivery, x.GetEnabledRetainers(false).Length);
                }
                if(x.WorkshopEnabled && x.GetEnabledVesselsData(VoyageType.Submersible).Count > 0)
                {
                    warnSub = true;
                }
            }
            if(warnSub && C.FullAutoGCDeliveryInventory < 50)
            {
                Warning($"已启用潜水艇处理，但多角色模式筹备稀有品的背包剩余触发值仅为 {C.FullAutoGCDeliveryInventory}。建议至少设为 50，避免背包空间不足。");
            }
            if(C.FullAutoGCDeliveryInventory < maxRetainersWhenGcDelivery * 5)
            {
                Warning($"部分角色启用了 {maxRetainersWhenGcDelivery} 名雇员，但筹备稀有品的背包剩余触发值仅为 {C.FullAutoGCDeliveryInventory}。建议至少设为 {maxRetainersWhenGcDelivery * 5}（每名雇员预留 5 格）。");
            }
        }

        foreach(var x in C.OfflineData)
        {
            if(x.WorkshopEnabled)
            {
                var a = x.OfflineSubmarineData.Select(x => x.Name);
                if(a.Count() > a.Distinct().Count())
                {
                    Error($"角色 {Censor.Character(x.Name, x.World)} 有重复的潜水艇名称。潜水艇名称必须唯一。");
                }
            }
        }

        if((C.GlobalTeleportOptions.Enabled || C.OfflineData.Any(x => x.TeleportOptionsOverride.Enabled == true)) && !Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Lifestream" && x.IsLoaded))
        {
            Error("已启用传送功能，但未安装或加载 Lifestream 插件。AutoRetainer 无法在此配置下运行，请禁用传送功能或安装 Lifestream。");
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforcePlan)
            {
                Info($"潜水艇解锁方案 {x.Name.NullWhenEmpty() ?? x.GUID} 被设置为强制生效，如果有任何可解锁内容，它将覆盖所有潜水艇设置。");
            }
        }

        foreach(var x in C.SubmarineUnlockPlans)
        {
            if(x.EnforceDSSSinglePoint)
            {
                Info($"潜水艇解锁方案 {x.Name.NullWhenEmpty() ?? x.GUID} 被设置为在深海点部署单点，并将忽略手动设置的解锁行为。");
            }
        }

        try
        {
            if(DalamudReflector.IsOnStaging())
            {
                Error("检测到非正式版 Dalamud 分支，可能导致兼容性问题。请输入 /xlbranch 打开分支切换器，切换到 release 分支并重启游戏。");
            }
        }
        catch(Exception e)
        {
        }

        if(Player.Available)
        {
            if(Player.CurrentWorld != Player.HomeWorld)
            {
                Error("当前正在跨服。返回所属服务器后，AutoRetainer 才能继续处理此角色。");
            }
            if(C.Blacklist.Any(x => x.CID == Player.CID))
            {
                Error("当前角色已被 AutoRetainer 完全排除。请前往“设置 → 排除项”修改。");
            }
            if(Data?.ExcludeRetainer == true)
            {
                Error("当前角色已被排除在雇员列表外。请前往“设置 → 排除项”修改。");
            }
            if(Data?.ExcludeWorkshop == true)
            {
                Error("当前角色已被排除在远航探索列表外。请前往“设置 → 排除项”修改。");
            }
        }

        {
            var list = C.OfflineData.Where(x => x.GetAreTeleportSettingsOverriden());
            if(list.Any())
            {
                Info("部分角色使用了单独的传送设置。鼠标悬停可查看角色列表。", list.Select(x => $"{x.Name}@{x.World}").Print("\n"));
            }
        }

        if(C.NoTeleportHetWhenNextToBell)
        {
            Warning("当角色靠近传唤铃时，传送或进入房屋/公寓的功能已被禁用。请注意房屋拆除计时器。");
        }



        if(C.AllowSimpleTeleport)
        {
            Warning("已启用简易传送。此方式不如在 Lifestream 中登记房屋可靠；如遇传送问题，请关闭此选项并登记房屋。");
        }

        if(!C.EnableEntrustManager && C.AdditionalData.Any(x => x.Value.EntrustPlan != Guid.Empty))
        {
            Warning("存放管理器已全局禁用，但部分雇员仍分配了存放计划。这些计划只能手动执行。");
        }

        if(C.ExtraDebug)
        {
            Info("已启用额外日志记录选项。这将导致日志大量输出，请仅在收集侦错信息时使用。");
        }

        if(C.UnsyncCompensation > -5)
        {
            Warning("时间不同步补偿值设置过高(>-5)，可能导致问题。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTIdle) < 10)
        {
            Warning("空闲时帧率设置过低(<10)，可能导致问题。");
        }

        if(UIUtils.GetFPSFromMSPT(C.TargetMSPTRunning) < 20)
        {
            Warning("运行时的帧率设置过低(<20)，可能导致问题。");
        }

        if(Data?.GetIMSettings().AllowSellFromArmory == true)
        {
            Info("已启用允许从装备兵装库出售物品选项。请确保将您的零式装备和绝境武器加入保护清单。");
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && !x.Enabled && x.RetainerData.Count > 0);
            if(list.Any())
            {
                Warning("部分角色记录了雇员，但未启用雇员多角色模式。鼠标悬停可查看角色列表。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeRetainer && x.Enabled && x.RetainerData.Count > 0 && C.SelectedRetainers.TryGetValue(x.CID, out var rd) && !x.RetainerData.All(r => rd.Contains(r.Name)));
            if(list.Any())
            {
                Warning("部分角色没有启用全部雇员。鼠标悬停可查看角色列表。", list.Print("\n"));
            }
        }
        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && !x.WorkshopEnabled && (x.OfflineSubmarineData.Count + x.OfflineAirshipData.Count) > 0);
            if(list.Any())
            {
                Warning("部分角色记录了远航探索，但未启用远航探索多角色模式。鼠标悬停可查看角色列表。", list.Print("\n"));
            }
        }

        {
            var list = C.OfflineData.Where(x => !x.ExcludeWorkshop && x.WorkshopEnabled && x.GetEnabledVesselsData(Internal.VoyageType.Airship).Count + x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count < Math.Min(x.OfflineAirshipData.Count + x.OfflineSubmarineData.Count, 4));
            if(list.Any())
            {
                Warning("部分角色没有启用全部远航探索。鼠标悬停可查看角色列表。", list.Print("\n"));
            }
        }

        if(C.MultiModeType != AutoRetainerAPI.Configuration.MultiModeType.Everything)
        {
            Warning($"你的多角色模式类型被设置为 {C.MultiModeType}。这将限制 AutoRetainer 执行的功能。");
        }

        if(C.OfflineData.Any(x => x.MultiWaitForAllDeployables))
        {
            Info("部分角色启用了“等待所有待处理潜艇”。AutoRetainer 会等这些角色的全部潜艇返航后再开始处理。鼠标悬停可查看角色列表。", C.OfflineData.Where(x => x.MultiWaitForAllDeployables).Select(x => $"{x.Name}@{x.World}").Print("\n"));
        }

        if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
        {
            Info("全局选项\"等待探险完成\"已启用。这代表对于所有角色，AutoRetainer 都会等到所有雇员回归后才处理，即使该角色的独立选项已关闭也是如此。");
        }

        if(C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn)
        {
            Info("潜艇已启用“即使已登录也等待”。即使角色当前在线，AutoRetainer 也会等该角色的全部潜艇返航后再处理。");
        }

        if(C.DisableRetainerVesselReturn > 0)
        {
            if(C.DisableRetainerVesselReturn > 10)
            {
                Warning("\"雇员探险处理截止时间\"被设置为异常高值。当雇员即将可用时，你可能会在重新派遣雇员时遇到明显延迟。");
            }
            else
            {
                Info("\"雇员探险处理截止时间\"已启用。当雇员即将可用时，你可能会在重新派遣雇员时遇到明显延迟。");
            }
        }

        if(C.MultiModeRetainerConfiguration.MultiWaitForAll)
        {
            Info("\"等待探险完成\"选项已启用。这代表 AutoRetainer 会等到该角色的所有雇员探险都完成后，才会登录并处理。");
        }

        if(C.MultiModeRetainerConfiguration.WaitForAllLoggedIn)
        {
            Info("雇员已启用“即使已登录也等待”。即使角色当前在线，AutoRetainer 也会等该角色的全部雇员探险完成后再处理。");
        }

        {
            var manualList = new List<string>();
            var deletedList = new List<string>();
            foreach(var x in C.OfflineData)
            {
                foreach(var ret in x.RetainerData)
                {
                    var planId = Utils.GetAdditionalData(x.CID, ret.Name).EntrustPlan;
                    var plan = C.EntrustPlans.FirstOrDefault(s => s.Guid == planId);
                    if(plan != null && plan.ManualPlan) manualList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                    if(plan == null && planId != Guid.Empty) deletedList.Add($"{Censor.Character(x.Name)} - {Censor.Retainer(ret.Name)}");
                }
            }
            if(manualList.Count > 0)
            {
                Info("部分雇员使用手动存放计划。重新派遣后不会自动执行，只能点击覆盖层按钮触发。鼠标悬停可查看列表。", manualList.Print("\n"));
            }
            if(deletedList.Count > 0)
            {
                Warning("部分雇员使用的存放计划已被删除，因此不会存放任何物品。鼠标悬停可查看列表。", deletedList.Print("\n"));
            }
        }

        if(C.No2ndInstanceNotify)
        {
            Info("你启用了\"不针对从相同目录执行的第二个游戏实例进行警告\"，这会让 AutoRetainer 在检测到使用相同 Dalamud 目录的第二个游戏窗口时，自动跳过该窗口的加载。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "SimpleTweaksPlugin" && x.IsLoaded))
        {
            Info("检测到 Simple Tweaks 插件。与雇员或潜水艇有关的调整可能干扰 AutoRetainer，请检查相关设置。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "PandorasBox" && x.IsLoaded))
        {
            Info("检测到 Pandora's Box 插件。请确保 AutoRetainer 运行时，Pandora's Box 不会自动执行动作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "Automaton" && x.IsLoaded))
        {
            Info("检测到 Automaton 插件。请确保 AutoRetainer 运行时，Automaton 不会自动执行动作或输入数值。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName == "RotationSolver" && x.IsLoaded))
        {
            Info("检测到 RotationSolver 插件。请确保 AutoRetainer 运行时，RotationSolver 不会自动执行技能或其他动作。");
        }

        if(Svc.PluginInterface.InstalledPlugins.Any(x => x.InternalName.StartsWith("BossMod") && x.IsLoaded))
        {
            Info("检测到 BossMod 插件。请确保 AutoRetainer 运行时，BossMod 不会自动执行动作。");
        }

        ImGui.Separator();
        ImGuiEx.TextWrapped("专家设置会修改开发者预期的行为。请检查你的问题是否与错误配置的专家设置有关。");
        CheckExpertSetting("无可用派遣任务时存取传唤铃的操作", nameof(C.OpenBellBehaviorNoVentures));
        CheckExpertSetting("有可用派遣任务时存取传唤铃的操作", nameof(C.OpenBellBehaviorWithVentures));
        CheckExpertSetting("访问传唤铃后任务完成行为", nameof(C.TaskCompletedBehaviorAccess));
        CheckExpertSetting("手动启用后任务完成行为", nameof(C.TaskCompletedBehaviorManual));
        CheckExpertSetting("如果 5 分钟内有雇员将完成探险，则停留在雇员菜单中", nameof(C.Stay5));
        CheckExpertSetting("关闭雇员列表时自动禁用插件", nameof(C.AutoDisable));
        CheckExpertSetting("不显示插件状态图标", nameof(C.HideOverlayIcons));
        CheckExpertSetting("显示多角色模式类型选择器", nameof(C.DisplayMMType));
        CheckExpertSetting("在部队工房中显示远航探索", nameof(C.ShowDeployables));
        CheckExpertSetting("启用应急复原模块", nameof(C.EnableBailout));
        CheckExpertSetting("AutoRetainer 尝试解除卡死前的超时时间（秒）", nameof(C.BailoutTimeout));
        CheckExpertSetting("禁用排序和折叠/展开功能", nameof(C.NoCurrentCharaOnTop));
        CheckExpertSetting("在插件界面栏显示多角色模式复选框", nameof(C.MultiModeUIBar));
        CheckExpertSetting("雇员菜单延迟（秒）", nameof(C.RetainerMenuDelay));
        CheckExpertSetting("不检查派遣计划错误", nameof(C.NoErrorCheckPlanner2));
        CheckExpertSetting("启用多角色模式时，尝试进入附近房屋", nameof(C.MultiHETOnEnable));
        CheckExpertSetting("Artisan 整合功能", nameof(C.ArtisanIntegration));
        CheckExpertSetting("使用服务器时间而非本地时间", nameof(C.UseServerTime));
    }

    private static void Error(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.RedBright, "\uf057");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.RedBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Warning(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.OrangeBright, "\uf071");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.OrangeBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void Info(string message, string tooltip = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(EColor.YellowBright, "\uf05a");
        ImGui.PopFont();
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
        ImGui.SameLine();
        ImGuiEx.TextWrapped(EColor.YellowBright, message);
        if(tooltip != null) ImGuiEx.Tooltip(tooltip);
    }

    private static void CheckExpertSetting(string setting, string nameOfSetting)
    {
        var original = EmptyConfig.GetFoP(nameOfSetting);
        var current = C.GetFoP(nameOfSetting);
        if(!original.Equals(current))
        {
            Info($"专家设置 \"{setting}\" 与默认值不同", $"默认值为 \"{original}\"，当前为 \"{current}\"。");
        }
    }
}

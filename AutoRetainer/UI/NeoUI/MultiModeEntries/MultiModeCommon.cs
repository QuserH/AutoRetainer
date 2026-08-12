namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "多角色模式/通用设置";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("通用设置")
        .Checkbox($"在登录界面等待", () => ref C.MultiWaitOnLoginScreen, "如果没有角色可进行探险任务，你将保持登出状态直到有角色可用。启用此选项和多角色模式时，标题画面动画将会禁用。")
        .Checkbox($"手动登录时禁用多角色模式", () => ref C.MultiDisableOnRelog, "使用 AutoRetainer 界面或指令重新登录后，多角色模式将被关闭。")
        .Checkbox($"手动登录时不重置首选角色", () => ref C.MultiNoPreferredReset, "使用 AutoRetainer 界面或指令重新登录后，首选角色保持不变。")
        .Checkbox("允许进入共享房屋", () => ref C.SharedHET)
        .Checkbox("即使多角色模式禁用也尝试在登录时进入房屋", () => ref C.HETWhenDisabled)
        .Checkbox("当已在传唤铃旁时禁止传送或进入房屋", () => ref C.NoTeleportHetWhenNextToBell)

        .Section("游戏启动")
        .Checkbox($"游戏启动时启用多角色模式", () => ref C.MultiAutoStart)
        .Checkbox($"外挂启动时启用多角色模式", () => ref C.MultiOnPluginLoad)
        .Indent()
        .SliderInt(150f, "延迟（秒）", () => ref C.MultiModeOnPluginLoadDelay, 0, 20)
        .Unindent()
        .Widget("游戏启动时自动登录", (x) =>
        {
            ImGui.SetNextItemWidth(150f);
            var names = C.OfflineData.Where(s => !s.Name.IsNullOrEmpty()).Select(s => $"{s.Name}@{s.World}");
            var dict = names.ToDictionary(s => s, s => Censor.Character(s));
            dict.Add("", "禁用");
            dict.Add("~", "上次登录的角色");
            ImGuiEx.Combo(x, ref C.AutoLogin, ["", "~", .. names], names: dict);
        })
        .SliderInt(150f, "延迟", () => ref C.AutoLoginDelay.ValidateRange(0, 60), 0, 20, "设置适当的延迟，让插件在登录前完全加载，并留出时间以便在需要时取消登录")
        .Checkbox("在外挂程式重新载入后维持多角色模式", () => ref C.PreserveMultiModeState)

        .Section("背包空间警告")
        .InputInt(100f, $"雇员清单：剩余背包空格警告", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"雇员清单：剩余探险币警告", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"潜艇清单：剩余背包空格警告", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"潜艇清单：剩余青磷水警告", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"潜艇清单：剩余修理材料警告", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))

        .Section("传送设置")
        .Widget(() => ImGuiEx.Text("需要安装 Lifestream 插件"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("你必须在 Lifestream 插件中为每个角色注册房屋，此选项才会生效，或者启用简易传送")
        .TextWrapped("你可以在角色配置选单中为每个角色自定义这些设置。")
        .Widget(() =>
        {
            if(Data != null && Data.GetAreTeleportSettingsOverriden())
            {
                ImGuiEx.TextWrapped(ImGuiColors.DalamudRed, "目前角色的传送选项已自定义。");
            }
        })
        .Checkbox("启用", () => ref C.GlobalTeleportOptions.Enabled)
        .Indent()
        .Checkbox("为传唤铃传送...", () => ref C.GlobalTeleportOptions.Retainers)
        .Indent()
        .Checkbox("...到私人房屋", () => ref C.GlobalTeleportOptions.RetainersPrivate)
        .Checkbox("...到共享房屋", () => ref C.GlobalTeleportOptions.RetainersShared)
        .Checkbox("...到部队房屋", () => ref C.GlobalTeleportOptions.RetainersFC)
        .Checkbox("...到公寓", () => ref C.GlobalTeleportOptions.RetainersApartment)
        .TextWrapped("如果以上所有选项都禁用或失败，将会传送到旅馆")
        .Unindent()
        .Checkbox("为潜水艇/飞艇传送至部队房屋", () => ref C.GlobalTeleportOptions.Deployables)
        .Checkbox("启用简易传送", () => ref C.AllowSimpleTeleport)
        .Unindent()
        .Widget(() => ImGuiEx.HelpMarker("""
            允许在未向Lifestream注册房屋的情况下传送。传送功能仍需安装Lifestream外挂才能运作。

            警告：此选项比在Lifestream中注册房屋更不可靠。请仅在必要时使用。
            """, EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("紧急逃生模块")
        .Checkbox("发生连线错误时自动关闭并重试登录", () => ref C.ResolveConnectionErrors, "断线时 AutoRetainer 将尝试重新登录。若登录会话已过期，则不会尝试登录。")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}

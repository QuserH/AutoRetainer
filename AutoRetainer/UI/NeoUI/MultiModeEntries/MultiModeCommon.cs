namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeCommon : NeoUIEntry
{
    public override string Path => "多角色模式/通用设置";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("Common Settings")
        .Checkbox($"Wait on login screen", () => ref C.MultiWaitOnLoginScreen, "If no character is available for ventures, you will be logged off until any character is available again. Title screen movie will be disabled while this option and MultiMode are enabled.")
        .Checkbox($"Disable Multi Mode on Manual Login", () => ref C.MultiDisableOnRelog, "Upon relogging via AutoRetainer's UI or command, disable Multi Mode.")
        .Checkbox($"Do not reset Preferred Character on Manual Login", () => ref C.MultiNoPreferredReset, "Upon relogging via AutoRetainer's UI or command, do not reset preferred character.")
        .Checkbox("Allow entering shared houses", () => ref C.SharedHET)
        .Checkbox("Attempt to enter house on login even when Multi Mode is disabled", () => ref C.HETWhenDisabled)
        .Checkbox("Do not teleport or enter house for retainers when already next to bell", () => ref C.NoTeleportHetWhenNextToBell)
        .Checkbox("Never enter houses on characters that do not have neither retainers nor deployables enabled", () => ref C.NoHetWhenNothingEnabled)
        .Checkbox("Do not try to auto-enter houses when enabling multi mode", () => ref C.MultiHETOnEnable, "If you don't have teleportation options enabled you will have problems")

        .Section("游戏启动")
        .Checkbox($"游戏启动时启用多角色模式", () => ref C.MultiAutoStart)
        .Checkbox($"插件加载时启用多角色模式", () => ref C.MultiOnPluginLoad)
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
        .Checkbox("插件重新加载后保持多角色模式", () => ref C.PreserveMultiModeState)

        .Section("Inventory warnings")
        .InputInt(100f, $"Retainer list: remaining inventory slots warning", () => ref C.UIWarningRetSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Retainer list: remaining ventures warning", () => ref C.UIWarningRetVentureNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining inventory slots warning", () => ref C.UIWarningDepSlotNum.ValidateRange(2, 1000))
        .InputInt(100f, $"Deployables list: remaining fuel warning", () => ref C.UIWarningDepTanksNum.ValidateRange(20, 1000))
        .InputInt(100f, $"Deployables list: remaining repair kit warning", () => ref C.UIWarningDepRepairNum.ValidateRange(5, 1000))
        .InputInt(100f, $"Both lists: remaining gil warning", () => ref C.MinGilDisplay)

        .Section("传送设置")
        .Widget(() => ImGuiEx.Text("需要安装 Lifestream 插件"))
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("Lifestream", new Version("2.2.1.1"))]))
        .TextWrapped("请在 Lifestream 中为每个角色登记房屋，或启用简易传送，否则此功能不会生效。")
        .TextWrapped("你可以在角色配置菜单中为每个角色单独设置这些选项。")
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
            允许在未向 Lifestream 登记房屋的情况下传送。传送功能仍需安装 Lifestream 插件。

            警告：此方式不如在 Lifestream 中登记房屋可靠，请仅在必要时使用。
            """, EColor.RedBright, FontAwesomeIcon.ExclamationTriangle.ToIconString()))

        .Section("紧急逃生模块")
        .Checkbox("发生连线错误时自动关闭并重试登录", () => ref C.ResolveConnectionErrors, "断线时 AutoRetainer 将尝试重新登录。若登录会话已过期，则不会尝试登录。")
        .Widget(() => ImGuiEx.PluginAvailabilityIndicator([new("NoKillPlugin")]));
}

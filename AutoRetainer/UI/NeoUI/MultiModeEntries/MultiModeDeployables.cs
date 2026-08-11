using ECommons.Throttlers;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeDeployables : NeoUIEntry
{
    public override string Path => "多角色模式/远征探险";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 潜艇/飞空艇")
        .Checkbox("等待航程完成", () => ref C.MultiModeWorkshopConfiguration.MultiWaitForAll, """
启用后，AutoRetainer 将等待所有远征探险返回后才登录该角色。如果你因其他原因已登录，它仍会重新派遣已完成的潜水艇，除非全局设置“即使已登录也等待”也已开启。
""")
        .Indent()
        .Checkbox("即使已登录也等待", () => ref C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn, """
改变“等待航程完成”（全局和按角色）的行为，使 AutoRetainer 在已登录时不再单独重新派遣潜水艇。相反，它会等待所有潜水艇返回后再采取行动。
""")
        .InputInt(120f, "最大等待时间（分钟）", () => ref C.MultiModeWorkshopConfiguration.MaxMinutesOfWaiting.ValidateRange(0, 9999), 10, 60, """
如果等待其他远征探险返回的时间将超过此分钟数，AutoRetainer 将忽略“等待航程完成”和“即使已登录也等待”设置。
""")
        .Unindent()
        .DragInt(60f, "提前登录阈值（秒）", () => ref C.MultiModeWorkshopConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300, "该角色的潜水艇可重新派遣前，AutoRetainer 提前登录的秒数。")
        .DragInt(120f, "停止处理雇员探险的提前时间（分钟）", () => ref C.DisableRetainerVesselReturn.ValidateRange(0, 60), "设为大于 0 的值时，AutoRetainer 会在任一角色计划重新派遣潜水艇前指定的分钟数停止处理雇员，并综合考虑此前的所有设置。")
        .Checkbox("派遣后立即出售\"无条件出售清单\"中的物品（需要雇员）", () => ref C.VendorItemAfterVoyage)
        .Checkbox("进入部队工作坊时，定期检查部队箱中的金币", () => ref C.FCChestGilCheck, "在进入工作坊时定期检查部队箱，以保持金币计数为最新状态。")
        .Indent()
        .SliderInt(150f, "检查频率（小时）", () => ref C.FCChestGilCheckCd, 0, 24 * 5)
        .Widget("重设冷却时间", (x) =>
        {
            if(ImGuiEx.Button(x, C.FCChestGilCheckTimes.Count > 0)) C.FCChestGilCheckTimes.Clear();
        })
        .Unindent()
        .Checkbox("处理完所有远征探险后关闭游戏", () => ref C.ShutdownOnSubExhaustion)
        .Indent()
        .SliderFloat(150f, "如果有远征探险将在此小时内返回，则不关闭游戏", () => ref C.HoursForShutdown, 0f, 10f)
        .Widget(() =>
        {
            ImGuiEx.HelpMarker($"""
                当前状态：{(Utils.CanShutdownForSubs() ? "可关闭游戏" : "暂不可关闭游戏")}
                强制关闭剩余时间：{EzThrottler.GetRemainingTime("ForceShutdownForSubs")}
                """);
        })
        .Unindent()
        .TextWrapped("进入工房后自动购买青磷水：")
        .Indent()
        .Widget(() =>
        {
            if(Data != null)
            {
                ImGui.Checkbox($"在 {Data.NameWithWorldCensored} 上启用", ref Data.AutoFuelPurchase);
            }
            ImGuiEx.TextWrapped($"若要启用/禁用其他角色的燃料购买，请前往「功能、排除与排序」区块。");
        })
        .InputInt(150f, "触发购买的剩余青磷水数量", () => ref C.AutoFuelPurchaseLow.ValidateRange(100, 99999))
        .InputInt(150f, "购买至背包内达到此数量", () => ref C.AutoFuelPurchaseMax)
        .Checkbox("仅在工作站解锁时进行购买", () => ref C.AutoFuelPurchaseOnlyWsUnlocked)
        .Unindent()
        .Checkbox("部属完成后退出游戏", () => ref C.ExitOnSubCompletion, "重要提示：启用后，多角色模式将仅处理远征探险，不处理雇员。")
        .Indent()
        .InputInt(150f, "等待潜水艇返回的最长时间（分钟）", () => ref C.ExitOnSubCompletionTime)
        .Unindent()
        ;
}

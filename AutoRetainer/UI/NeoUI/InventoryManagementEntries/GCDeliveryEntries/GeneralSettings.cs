using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "大国防联军 - 一般设置";

    public override NuiBuilder Builder => new NuiBuilder()
        .Section("一般设置")
        .Checkbox("启用自动筹备交换", () => ref C.AutoGCContinuation)
        .Widget(() =>
        {
            ImGuiEx.TextWrapped("提交装备与兑换探险币的额外随机延迟（单位：秒，保留两位小数；最小值和最大值均为 0 表示不延迟）");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("提交装备延迟最小值", ref C.GCHandinDelayMinSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("提交装备延迟最大值", ref C.GCHandinDelayMaxSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("兑换探险币延迟最小值", ref C.GCExchangeDelayMinSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("兑换探险币延迟最大值", ref C.GCExchangeDelayMaxSeconds, 0.01f, 0.10f, "%.2f");
            C.GCHandinDelayMinSeconds = NormalizeDelay(C.GCHandinDelayMinSeconds);
            C.GCHandinDelayMaxSeconds = Math.Max(C.GCHandinDelayMinSeconds, NormalizeDelay(C.GCHandinDelayMaxSeconds));
            C.GCExchangeDelayMinSeconds = NormalizeDelay(C.GCExchangeDelayMinSeconds);
            C.GCExchangeDelayMaxSeconds = Math.Max(C.GCExchangeDelayMinSeconds, NormalizeDelay(C.GCExchangeDelayMaxSeconds));
        })
        .TextWrapped($"""
            启用筹备稀有品后续兑换后：
            - 插件将自动消耗可用的军票，从配置的兑换列表中购买物品。
            - 如果兑换列表为空，将只购买探险币。
            - 请确保“角色配置”中的“交付模式”未设为“禁用”。

            军票用完后:
            - 专家交付将自动恢复。
            - 该过程将重复，直到没有符合条件的物品可交付或没有剩余军票。
            """)

        .Section("多角色模式筹备交换")
        .TextWrapped($"""
        启用后:
        - 启用传送的角色将在多角色模式下自动筹备稀有品，并在军衔满足条件时按兑换计划购买物品。
        """)
        .Checkbox("启用多角色筹备交换", () => ref C.FullAutoGCDelivery)
        .Checkbox("仅在工作台未锁定时触发", () => ref C.FullAutoGCDeliveryOnlyWsUnlocked)
        .InputInt(150f, "触发筹备的剩余背包格数 (小于或等于)", () => ref C.FullAutoGCDeliveryInventory, "仅计算主要背包，不包含兵装库")
        .Checkbox("当当探险币耗尽时触发", () => ref C.FullAutoGCDeliveryDeliverOnVentureExhaust, "此选项可能导致每次登录时都会前往军队兑换。请确保已设置足够探险币的方案。")
        .Indent()
        .InputInt(150f, "触发筹备的剩余探险币数量 (小于或等于)", () => ref C.FullAutoGCDeliveryDeliverOnVentureLessThan)
        .Unindent()
        .Checkbox("优先使用军票加成票券，如果可用的话", () => ref C.FullAutoGCDeliveryUseBuffItem)
        .Widget(() =>
        {
            if(C.FullAutoGCDeliveryUseBuffItem)
            {
                ImGui.Indent();
                if(Data != null)
                {
                    ImGuiEx.Checkbox($"排除 {Data.NameWithWorldCensored}##item", ref Data.NoItemBuffUse);
                }
                var cnt = C.OfflineData.Count(x => x.NoItemBuffUse);
                ImGuiEx.Text($"{(cnt > 0 ? $"已排除 {cnt} 个角色使用军票加成票券。" : "")}可在“功能、排除与排序”区块中排除角色。");
                ImGui.Unindent();
            }
        })
        .Checkbox("优先使用部队军票加成BUFF，如果可用的话", () => ref C.FullAutoGCDeliveryUseBuffFCAction)
        .Widget(() =>
        {
            if(C.FullAutoGCDeliveryUseBuffFCAction)
            {
                ImGui.Indent();
                if(Data != null)
                {
                    ImGuiEx.Checkbox($"排除 {Data.NameWithWorldCensored}", ref Data.NoFcBuffUse);
                }
                var cnt = C.OfflineData.Count(x => x.NoFcBuffUse);
                ImGuiEx.Text($"{(cnt > 0 ? $"已排除 {cnt} 个角色使用部队军票加成。" : "")}可在“功能、排除与排序”区块中排除角色。");
                ImGui.Unindent();
            }
        })
        .Checkbox("筹备交换后传送回房屋/旅馆", () => ref C.TeleportAfterGCExchange)
        .Indent()
        .Checkbox("仅在多角色模式启动时", () => ref C.TeleportAfterGCExchangeMulti)
        .Unindent()
        ;

    private static float NormalizeDelay(float seconds)
    {
        if(!float.IsFinite(seconds)) return 0f;
        return Math.Clamp(MathF.Round(seconds, 2, MidpointRounding.AwayFromZero), 0f, 3600f);
    }
}

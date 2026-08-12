using AutoRetainer.Internal.InventoryManagement;
using ECommons.GameHelpers;
using TerraFX.Interop.Windows;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class GeneralSettings : InventoryManagementBase
{
    public override string Name { get; } = "背包清理/一般设置";

    private GeneralSettings()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .Checkbox($"自动打开雇员宝箱", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableCofferAutoOpen, "仅多角色模式。登出前会自动打开所有宝箱，除非背包空间不足。")
            .Indent()
            .InputInt(100f, "单次开启最大数量", () => ref InventoryCleanupCommon.SelectedPlan.MaxCoffersAtOnce)
            .Unindent()
            .Checkbox($"启用将物品出售给雇员", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableAutoVendor, "当 AutoRetainer 将雇员派往任务时，物品将依照背包清理方案自动出售。")
            .Checkbox($"启用向住宅 NPC 出售物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell, "当 AutoRetainer 进入住宅时，物品将依照背包清理方案出售。住宅 NPC 必须放置在住宅入口附近（非工房入口），进入后可立即互动。")
            .Indent()
            .Checkbox($"雇员可用时不使用住宅 NPC", () => ref InventoryCleanupCommon.SelectedPlan.IMSkipVendorIfRetainer)
            .Widget("立即出售", (x) =>
            {
                if(ImGuiEx.Button(x, Player.Interactable && InventoryCleanupCommon.SelectedPlan.IMEnableNpcSell && NpcSaleManager.GetValidNPC() != null && !IsOccupied() && !P.TaskManager.IsBusy))
                {
                    NpcSaleManager.EnqueueIfItemsPresent(true);
                }
            })
            .Unindent()
            .Checkbox($"自动分解物品", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesis)
            .Indent()
            .Widget("兵装库: ", t =>
            {
                ImGuiEx.TextV(t);
                ImGui.SameLine();
                ImGuiEx.RadioButtonBool("分解", "跳过", ref InventoryCleanupCommon.SelectedPlan.IMEnableItemDesynthesisFromArmory, true);
            })
            .Unindent()
            .Checkbox($"启用右键选单整合", () => ref InventoryCleanupCommon.SelectedPlan.IMEnableContextMenu)
            .Checkbox($"允许从兵装库出售/丢弃物品", () => ref InventoryCleanupCommon.SelectedPlan.AllowSellFromArmory)
            .Checkbox("多角色模式下将符合条件的物品放入置衣柜", () => ref InventoryCleanupCommon.SelectedPlan.EnableCabinetAutoDelivery, "不在置衣柜中的物品将被放入其中。符合条件的物品也将被排除在丢弃、分解、存放至雇员或上交大国防联军之外（仅在多角色模式运行时）。此操作将在多角色模式专家交付之前触发。")
            .Checkbox("多角色模式下将符合条件的物品放入投影柜（需要 Glamour Log 插件）", () => ref InventoryCleanupCommon.SelectedPlan.EnableMirageAutoDelivery)
            .Checkbox($"演示模式", () => ref InventoryCleanupCommon.SelectedPlan.IMDry, "不实际出售/丢弃物品，仅在聊天窗口显示哪些物品将被处理")
            ;
    }
}

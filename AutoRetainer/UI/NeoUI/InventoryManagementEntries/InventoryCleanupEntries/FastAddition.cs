using AutoRetainerAPI.Configuration;
using ECommons.Automation;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using ECommons.WindowsFormsReflector;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public unsafe class FastAddition : InventoryManagementBase
{
    public override string Name { get; } = "背包清理/快速添加和移除";

    private FastAddition()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
        .Section(Name)
        .Widget(() =>
        {
            var selectedSettings = InventoryCleanupCommon.SelectedPlan;
            ImGuiEx.TextWrapped(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "显示此说明时，将鼠标移到物品上并按住对应按键：");
            ImGuiEx.Text(!ImGui.GetIO().KeyShift ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Shift - 添加至快速雇员出售清单");
            ImGuiEx.Text($"* 已在「无条件出售」及「丢弃」清单内的物品不会添加至快速雇员出售清单");
            ImGuiEx.Text(!ImGui.GetIO().KeyCtrl ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Ctrl - 添加至无条件出售清单");
            ImGuiEx.Text($"* 物品将会从其他清单「移至」无条件出售清单");
            ImGuiEx.Text(!IsKeyPressed(Keys.Tab) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Tab - 添加至丢弃清单");
            ImGuiEx.Text($"* 物品将会从其他清单「移至」丢弃清单");
            //ImGuiEx.Text(IsKeyPressed(Keys.Space) ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Space - add to Desynthesis List");
            //ImGuiEx.Text($"* Items that already in other lists WILL BE MOVED to Desynthesis List");
            ImGuiEx.Text(!ImGui.GetIO().KeyAlt ? ImGuiColors.DalamudGrey : ImGuiColors.DalamudRed, $"Alt - 从任何清单中删除");
            ImGuiEx.Text("受保护的物品不受此操作影响");
            if(Svc.GameGui.HoveredItem > 0)
            {
                var id = (uint)(Svc.GameGui.HoveredItem % 1000000);
                if(ImGui.GetIO().KeyShift)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) 
                    && !selectedSettings.IMAutoVendorSoft.Contains(id)
                    && !selectedSettings.IMAutoVendorHard.Contains(id)
                    && !selectedSettings.IMDiscardList.Contains(id)
                    && !selectedSettings.IMDesynth.Contains(id)
                    )
                    {
                        if(selectedSettings.AddItemToList(IMListKind.SoftSell, id, out var error))
                        {
                            Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到快速探险出售列表");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(ImGui.GetIO().KeyCtrl)
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMAutoVendorHard.Contains(id) && !selectedSettings.IMAutoVendorSoft.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.HardSell, id, out var error))
                        {
                            Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到无条件出售列表");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Tab))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDiscardList.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Discard, id, out var error))
                        {
                            Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到丢弃列表");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }
                /*if(!CSFramework.Instance()->WindowInactive && IsKeyPressed(Keys.Space))
                {
                    if(!selectedSettings.IMProtectList.Contains(id) && !selectedSettings.IMDesynth.Contains(id))
                    {
                        if(selectedSettings.AddItemToList(IMListKind.Desynth, id, out var error))
                        {
                            Notify.Success($"Added {ExcelItemHelper.GetName(id)} to Desynthesis List");
                        }
                        else
                        {
                            if(EzThrottler.Throttle($"Error_{error}", 1000)) Notify.Error(error);
                        }
                    }
                }*/
                if(ImGui.GetIO().KeyAlt)
                {
                    if(selectedSettings.IMAutoVendorSoft.Remove(id)) Notify.Info($"已将 {ExcelItemHelper.GetName(id)} 从快速探险出售列表移除");
                    if(selectedSettings.IMAutoVendorHard.Remove(id)) Notify.Info($"已将 {ExcelItemHelper.GetName(id)} 从无条件出售列表移除");
                    if(selectedSettings.IMDiscardList.Remove(id)) Notify.Info($"已将 {ExcelItemHelper.GetName(id)} 从丢弃列表移除");
                    if(selectedSettings.IMDesynth.Remove(id)) Notify.Info($"已将 {ExcelItemHelper.GetName(id)} 从分解列表移除");
                }
            }
        });
        DisplayPriority = -10;
    }
}

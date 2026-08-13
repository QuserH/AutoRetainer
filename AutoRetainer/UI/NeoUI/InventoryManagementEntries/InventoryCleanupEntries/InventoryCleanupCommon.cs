using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public static unsafe class InventoryCleanupCommon
{
    public static Guid SelectedPlanGuid = Guid.Empty;

    public static InventoryManagementSettings SelectedPlan
    {
        get
        {
            if(SelectedPlanGuid == Guid.Empty)
            {
                return C.DefaultIMSettings;
            }
            else
            {
                var planIndex = C.AdditionalIMSettings.IndexOf(x => x.GUID == SelectedPlanGuid);
                if(planIndex == -1)
                {
                    SelectedPlanGuid = Guid.Empty;
                    return C.DefaultIMSettings;
                }
                else
                {
                    return C.AdditionalIMSettings[planIndex];
                }
            }
        }
    }

    public static NuiBuilder CreateCleanupHeaderBuilder()
    {
        return new NuiBuilder().Section("背包清理计划选择器").Widget(DrawPlanSelector);
    }

    public static void DrawPlanSelector()
    {
        var selectedPlan = C.AdditionalIMSettings.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selimplan", selectedPlan?.DisplayName ?? "预设计划"))
            {
                if(ImGui.Selectable("预设计划", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalIMSettings)
                {
                    ImGui.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new InventoryManagementSettings()
                {
                    AllowSellFromArmory = C.DefaultIMSettings.AllowSellFromArmory,
                    IMEnableContextMenu = C.DefaultIMSettings.IMEnableContextMenu,
                    IMEnableCofferAutoOpen = C.DefaultIMSettings.IMEnableCofferAutoOpen,
                    IMSkipVendorIfRetainer = C.DefaultIMSettings.IMSkipVendorIfRetainer,
                    IMEnableAutoVendor = C.DefaultIMSettings.IMEnableAutoVendor,
                    IMEnableNpcSell = C.DefaultIMSettings.IMEnableNpcSell,
                };
                C.AdditionalIMSettings.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("添加新的计划");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultIMSettings).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("复制");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<InventoryManagementSettings>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();
                    C.AdditionalIMSettings.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("粘贴");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultIMSettings = selectedPlan.DSFClone();
                    C.DefaultIMSettings.GUID.Regenerate();
                    C.DefaultIMSettings.Name = "";
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("设为预设计划并覆盖当前预设。按住 CTRL 并左键点击。");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalIMSettings.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("删除此计划。按住 CTRL 并左键点击。");
            }
        });
        if(selectedPlan != null)
        {
            ImGuiEx.SetNextItemFullWidth();
            ImGui.InputTextWithHint("##name", "输入计划名称", ref selectedPlan.Name, 100);

            if(Data != null)
            {
                if(Data.InventoryCleanupPlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"当前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消分配"))
                    {
                        Data.InventoryCleanupPlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, $"非当前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("分配"))
                    {
                        Data.InventoryCleanupPlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text($"共有 {charas.Length} 个角色使用");
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored).Print("\n")}");
            }
            else
            {
                ImGuiEx.Text($"没有任何角色使用");
            }

            ImGuiEx.Text("将此方案的清单与预设方案合并:");
            ImGui.Indent();
            ImGui.Checkbox("合并雇员快速出售清单", ref selectedPlan.AdditionModeSoftSellList);
            ImGuiEx.HelpMarker("从快速探索中获得的物品，若同时包含在此方案与预设方案中，将会被出售。");
            ImGui.Checkbox("合并无条件出售清单", ref selectedPlan.AdditionModeHardSellList);
            ImGuiEx.HelpMarker("包含在此方案与预设方案中的物品都将被出售。如果同时包含在两者中，将优先采用此方案的「忽略堆叠」设置与「最大堆叠数量」限制。");
            ImGui.Checkbox("合并丢弃清单", ref selectedPlan.AdditionModeDiscardList);
            ImGuiEx.HelpMarker("包含在此方案与预设方案中的物品都将被丢弃。如果同时包含在两者中，将优先采用此方案的「忽略堆叠」设置与「最大堆叠数量」限制。");
            ImGui.Checkbox("合并保护清单", ref selectedPlan.AdditionModeProtectList);
            ImGuiEx.HelpMarker("包含在此方案与预设方案中的物品将受到保护，不会被自动出售、丢弃或筹备给军队。");
            ImGui.Unindent();
        }
    }
}

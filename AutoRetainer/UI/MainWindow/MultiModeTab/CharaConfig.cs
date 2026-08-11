using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow.MultiModeTab;
public class CharaConfig
{
    public static void Draw(OfflineCharacterData data, bool isRetainer)
    {
        ImGui.PushID(data.CID.ToString());
        SharedUI.DrawMultiModeHeader(data);
        var b = new NuiBuilder()

        .Section("通用角色特定设置")
        .Widget(() =>
        {
            SharedUI.DrawServiceAccSelector(data);
            SharedUI.DrawPreferredCharacterUI(data);
        });
        if(isRetainer)
        {
            b = b.Section("雇员管理").Widget(() =>
            {
                ImGuiEx.Text($"自动筹备稀有品:");
                if(!AutoGCHandin.Operation)
                {
                    ImGuiEx.SetNextItemWidthScaled(200f);
                    ImGuiEx.EnumCombo("##gcHandin", ref data.GCDeliveryType, null, Lang.GCDeliveryTypeNames);
                }
                else
                {
                    ImGuiEx.Text($"目前无法更改此设置");
                }
            });
        }
        else
        {
            b = b.Section("远征探险").Widget(() =>
            {
                ImGui.Checkbox($"等待航程完成", ref data.MultiWaitForAllDeployables);
                ImGuiComponents.HelpMarker("""
此设置类似于全域选项，但应用於单一角色。启用后，AutoRetainer 将在登录角色之前等待所有远征探险返回。如果您因其他原因已经登录，它仍然会重新派遣已完成的潜水艇/飞艇，除非全域设置「即使已登录也等待」也同时开启。
""");
            });
        }
        b = b.Section("传送覆盖设置", data.GetAreTeleportSettingsOverriden() ? ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] with { X = 1f } : null, true)
        .Widget(() =>
        {
            ImGuiEx.Text($"您可以为每个角色覆盖传送设置");
            bool? demo = null;
            ImGuiEx.Checkbox("标记此图示的选项将使用全域配置中的值", ref demo);
            ImGuiEx.Checkbox("启用", ref data.TeleportOptionsOverride.Enabled);
            ImGui.Indent();
            ImGuiEx.Checkbox("为传唤铃传送...", ref data.TeleportOptionsOverride.Retainers);
            ImGui.Indent();
            ImGuiEx.Checkbox("...到私人房屋", ref data.TeleportOptionsOverride.RetainersPrivate);
            ImGuiEx.Checkbox("...到共享房屋", ref data.TeleportOptionsOverride.RetainersShared);
            ImGuiEx.Checkbox("...到部队房屋", ref data.TeleportOptionsOverride.RetainersFC);
            ImGuiEx.Checkbox("...到公寓", ref data.TeleportOptionsOverride.RetainersApartment);
            ImGui.Text("如果以上所有选项都禁用或失败，将会传送到旅馆");
            ImGui.Unindent();
            ImGuiEx.Checkbox("为潜水艇/飞艇传送至部队房屋", ref data.TeleportOptionsOverride.Deployables);
            ImGui.Unindent(); 
        }).Draw();
        SharedUI.DrawExcludeReset(data);
        ImGui.PopID();
    }
}

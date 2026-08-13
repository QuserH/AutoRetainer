using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using PunishLib.ImGuiMethods;

namespace AutoRetainer.UI.MainWindow;

internal static class SharedUI
{
    internal static void DrawLockout(OfflineCharacterData data)
    {
        if(data.IsLockedOut())
        {
            FontAwesome.PrintV(EColor.RedBright, FontAwesomeIcon.Lock);
            ImGuiEx.Tooltip("此角色所在的数据中心已被临时禁用。请前往设置中重新启用。");
            ImGui.SameLine();
        }
    }

    internal static void DrawMultiModeHeader(OfflineCharacterData data, string overrideTitle = null)
    {
        var b = true;
        ImGui.CollapsingHeader($"{Censor.Character(data.Name)} {overrideTitle ?? "配置"}##conf", ref b, ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.OpenOnArrow);
        if(b == false)
        {
            ImGui.CloseCurrentPopup();
        }
        ImGui.Dummy(new(500, 1));
    }

    internal static void DrawServiceAccSelector(OfflineCharacterData data)
    {
        ImGuiEx.Text($"服务帐户选择");
        ImGuiEx.SetNextItemWidthScaled(150);
        if(ImGui.BeginCombo("##Service Account Selection", $"服务账号 {data.ServiceAccount + 1}", ImGuiComboFlags.HeightLarge))
        {
            for(var i = 1; i <= 10; i++)
            {
                if(ImGui.Selectable($"服务账号 {i}"))
                {
                    data.ServiceAccount = i - 1;
                }
            }
            ImGui.EndCombo();
        }
    }

    internal static void DrawPreferredCharacterUI(OfflineCharacterData data)
    {
        if(ImGui.Checkbox("首选角色", ref data.Preferred))
        {
            foreach(var z in C.OfflineData)
            {
                if(z.CID != data.CID)
                {
                    z.Preferred = false;
                }
            }
        }
        ImGuiComponents.HelpMarker("在多角色模式下，当没有其他角色需要收取雇员时，插件会自动切换回您的首选角色。");
    }

    internal static void DrawExcludeReset(OfflineCharacterData data)
    {
        new NuiBuilder().Section("清除/重置角色数据", collapsible: true)
        .Widget(() =>
        {
            if(ImGuiEx.ButtonCtrl("排除角色"))
            {
                C.Blacklist.Add((data.CID, data.Name));
            }
            ImGuiComponents.HelpMarker("排除此角色将立即重置其设置，将其移出角色列表，并停止处理其所有雇员。您仍可手动操作此角色的雇员。可在设置中取消此操作。");
            if(ImGuiEx.ButtonCtrl("重置角色数据"))
            {
                new TickScheduler(() => C.OfflineData.RemoveAll(x => x.CID == data.CID));
            }
        ImGuiComponents.HelpMarker("将删除该角色的已保存数据，但不会将其排除。再次登录该角色后会重新生成数据。");

                if(ImGui.Button("清除部队数据"))
            {
                data.ClearFCData();
            }
            ImGuiComponents.HelpMarker("将清除已保存的角色数据，但不会排除该角色。再次登录后会重新生成数据。");
        }).Draw();
    }
}

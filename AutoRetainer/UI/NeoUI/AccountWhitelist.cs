using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class AccountWhitelist : NeoUIEntry
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped("你可以设置账号白名单。使用不在白名单中的账号登录时，AutoRetainer 不会记录任何角色、雇员或潜水艇数据。");
        if(C.WhitelistedAccounts.Count == 0)
        {
            ImGuiEx.TextWrapped(EColor.GreenBright, "当前白名单已禁用。添加账号即可启用。");
        }
        else
        {
            ImGuiEx.TextWrapped(EColor.YellowBright, "当前白名单已启用。移除全部账号即可禁用。");
        }

        foreach(var x in C.WhitelistedAccounts)
        {
            ImGui.PushID(x.ToString());
            if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
            {
                new TickScheduler(() => C.WhitelistedAccounts.Remove(x));
            }
            ImGui.SameLine();
            ImGuiEx.TextV($"账号 {x}");
            ImGui.PopID();
        }
    }
}

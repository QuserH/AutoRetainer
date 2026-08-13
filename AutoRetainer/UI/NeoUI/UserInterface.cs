using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainer.UI.NeoUI;
public sealed unsafe class UserInterface : NeoUIEntry
{
    public override string Path => "用户界面";

    public override NuiBuilder Builder => new NuiBuilder()

        .Section("用户界面")
        .Checkbox("匿名显示雇员", () => ref C.NoNames, "在常规界面中隐藏雇员名称（调试菜单和插件日志中仍会显示）。启用后，不同插件界面中的角色和雇员编号可能不一致。")
        .Checkbox("在雇员界面显示快捷菜单", () => ref C.UIBar)
        .Checkbox("显示雇员详细信息", () => ref C.ShowAdditionalInfo, "在主界面显示雇员装等/获得力/鉴别力及其当前探险名称")
        .Widget("按下 ESC 键时不关闭 AutoRetainer 窗口", (x) =>
        {
            if(ImGui.Checkbox(x, ref C.IgnoreEsc)) Utils.ResetEscIgnoreByWindows();
        })
        .Checkbox("在状态栏仅显示最高优先级图标", () => ref C.StatusBarMSI)
        .SliderInt(120f, "状态栏图标大小", () => ref C.StatusBarIconWidth, 32, 128)
        .Checkbox("游戏启动时打开 AutoRetainer 窗口", () => ref C.DisplayOnStart)
        //.Checkbox("Skip item sell/trade confirmation while plugin is active", () => ref C.SkipItemConfirmations)
        .Checkbox("启用标题界面按钮（需重启插件）", () => ref C.UseTitleScreenButton)
        .Checkbox("隐藏角色搜索", () => ref C.NoCharaSearch)
        .Checkbox("不为已完成角色显示背景闪烁", () => ref C.NoGradient)
        .Checkbox("不警告同一目录运行多个游戏", () => ref C.No2ndInstanceNotify, "AutoRetainer 会自动跳过第二个游戏客户端，除非在主客户端中关闭此选项。")

        .Section("雇员标签页角色排序")
        .Checkbox("启用", () => ref C.EnableRetainerSort)
        .TextWrapped("此排序仅影响视觉显示顺序，不会影响角色处理逻辑。")
        .Widget(() => UIUtils.DrawSortableEnumList("rorder", C.RetainersVisualOrders))

        .Section("远航探索标签页角色排序")
        .Checkbox("启用", () => ref C.EnableDeployablesSort)
        .TextWrapped("此排序仅影响视觉显示顺序，不会影响角色处理逻辑。")
        .Widget(() => UIUtils.DrawSortableEnumList("dorder", C.DeployablesVisualOrders));



}

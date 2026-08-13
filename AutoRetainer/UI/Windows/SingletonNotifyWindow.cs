namespace AutoRetainer.UI.Windows;
public class SingletonNotifyWindow : NotifyWindow
{
    private bool IAmIdiot = false;
    private WindowSystem ws;
    public SingletonNotifyWindow() : base("AutoRetainer - 警告！")
    {
        IsOpen = true;
        ws = new();
        Svc.PluginInterface.UiBuilder.Draw += ws.Draw;
        ws.AddWindow(this);
    }

    public override void OnClose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= ws.Draw;
    }

    public override void DrawContent()
    {
        ImGuiEx.Text("AutoRetainer 检测到另一个插件实例正在使用相同的数据路径。");
        ImGuiEx.Text("为防止数据丢失，已中止加载插件。");
        if(ImGui.Button("关闭窗口且不加载 AutoRetainer"))
        {
            IsOpen = false;
        }
        if(ImGui.Button("了解如何正确运行2个或更多游戏"))
        {
            ShellStart("https://github.com/PunishXIV/AutoRetainer/issues/62");
        }
        ImGui.Separator();
        ImGui.Checkbox("我了解这可能导致所有 AutoRetainer 数据丢失", ref IAmIdiot);
        if(!IAmIdiot) ImGui.BeginDisabled();
        if(ImGui.Button("加载 AutoRetainer"))
        {
            IsOpen = false;
            new TickScheduler(P.Load);
        }
        if(!IAmIdiot) ImGui.EndDisabled();
    }
}

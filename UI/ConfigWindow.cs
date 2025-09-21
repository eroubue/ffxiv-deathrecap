using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;

namespace DeathRecap.UI;

public class ConfigWindow : Window {
    private readonly DeathRecapPlugin plugin;

    public ConfigWindow(DeathRecapPlugin plugin) : base("死亡回顾配置", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize) {
        this.plugin = plugin;

        Size = new Vector2(580, 340);
    }

    public override void Draw() {
        var conf = plugin.Configuration;

        ImGui.TextUnformatted("捕获设置");
        ImGui.Separator();
        ImGui.Columns(3);
        foreach (var (k, v) in conf.EnumCaptureConfigs()) {
            ImGui.PushID(k);
            var bCapture = v.Capture;
            if (ImGui.Checkbox($"捕获 {k}", ref bCapture)) {
                v.Capture = bCapture;
                conf.Save();
            }

            var notificationStyle = (int)v.NotificationStyle;
            ImGui.TextUnformatted("死亡时");
            if (ImGui.Combo("##2", ref notificationStyle, ["不做任何操作", "聊天消息", "显示弹窗", "打开回顾"])) {
                v.NotificationStyle = (NotificationStyle)notificationStyle;
                conf.Save();
            }

            var bOnlyInstances = v.OnlyInstances;
            if (ImGui.Checkbox("仅在副本中", ref bOnlyInstances)) {
                v.OnlyInstances = bOnlyInstances;
                conf.Save();
            }

            OnlyInInstancesTooltip();

            var bDisableInPvp = v.DisableInPvp;
            if (ImGui.Checkbox("在PvP中禁用", ref bDisableInPvp)) {
                v.DisableInPvp = bDisableInPvp;
                conf.Save();
            }

            ImGui.PopID();
            ImGui.NextColumn();
        }

        ImGui.Columns();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextUnformatted("通用设置");
        ImGui.Spacing();
        var chatTypes = Enum.GetValues<XivChatType>();
        var chatType = Array.IndexOf(chatTypes, conf.ChatType);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("聊天消息类型");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
        if (ImGui.Combo("##3", ref chatType, chatTypes.Select(t => t.GetAttribute<XivChatTypeInfoAttribute>()?.FancyName ?? t.ToString()).ToImmutableList(),
                10)) {
            conf.ChatType = chatTypes[chatType];
            conf.Save();
        }

        ChatMessageTypeTooltip();

        var bShowTip = conf.ShowTip;
        if (ImGui.Checkbox("显示聊天提示", ref bShowTip)) {
            conf.ShowTip = bShowTip;
            conf.Save();
        }

        ChatTipTooltip();
        var keepEventsFor = conf.KeepCombatEventsForSeconds;
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("保留事件时间(秒)");
        ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
        if (ImGui.InputInt("##4", ref keepEventsFor, 10)) {
            conf.KeepCombatEventsForSeconds = keepEventsFor;
            conf.Save();
        }

        var keepDeathsFor = conf.KeepDeathsForMinutes;
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("保留死亡记录时间(分)");
        ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
        if (ImGui.InputInt("##5", ref keepDeathsFor, 10)) {
            conf.KeepDeathsForMinutes = keepDeathsFor;
            conf.Save();
        }
    }

    private static void ChatMessageTypeTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("设置“聊天消息”死亡通知的类别。\n" +
                             "“调试”消息会在所有聊天窗口中显示，无论如何配置。\n" +
                             "注意：这只会影响向你显示通知的方式，其他人永远不会看到。");
        }
    }

    private static void ChatTipTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("在你第一次关闭死亡回顾窗口时，在聊天中打印重新打开窗口的命令。");
        }
    }

    private static void OnlyInInstancesTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("仅在副本中（例如地下城）显示死亡通知");
        }
    }
}

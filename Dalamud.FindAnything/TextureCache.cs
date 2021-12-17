﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Interface;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;

namespace Dalamud.FindAnything
{
    public class TextureCache : IDisposable
    {
        private readonly UiBuilder uiBuilder;
        private readonly DataManager data;

        public IReadOnlyDictionary<uint, TextureWrap> MainCommandIcons { get; init; }
        public IReadOnlyDictionary<uint, TextureWrap> GeneralActionIcons { get; init; }
        public IReadOnlyDictionary<uint, TextureWrap> ContentTypeIcons { get; init; }
        public IReadOnlyDictionary<uint, TextureWrap> EmoteIcons { get; init; }

        public Dictionary<uint, TextureWrap> ExtraIcons { get; private set; }

        public TextureWrap AetheryteIcon { get; init; }
        public TextureWrap WikiIcon { get; init; }
        public TextureWrap PluginInstallerIcon { get; init; }
        public TextureWrap LogoutIcon { get; init; }
        public TextureWrap EmoteIcon { get; init; }
        public TextureWrap HintIcon { get; set; }
        public TextureWrap ChatIcon { get; set; }
        public TextureWrap MathsIcon { get; set; }

        private TextureCache(UiBuilder uiBuilder, DataManager data)
        {
            this.uiBuilder = uiBuilder;
            this.data = data;

            var mainCommands = new Dictionary<uint, TextureWrap>();
            foreach (var mainCommand in data.GetExcelSheet<MainCommand>()!)
            {
                mainCommands.Add(mainCommand.RowId, data!.GetImGuiTextureHqIcon((uint) mainCommand.Icon)!);
            }
            MainCommandIcons = mainCommands;

            var generalActions = new Dictionary<uint, TextureWrap>();
            foreach (var action in data.GetExcelSheet<GeneralAction>()!)
            {
                generalActions.Add(action.RowId, data!.GetImGuiTextureHqIcon((uint) action.Icon)!);
            }
            GeneralActionIcons = generalActions;

            var contentTypes = new Dictionary<uint, TextureWrap>();
            foreach (var cType in data.GetExcelSheet<ContentType>()!)
            {
                if (cType.Icon == 0)
                    continue;

                contentTypes.Add(cType.RowId, data!.GetImGuiTextureHqIcon((uint) cType.Icon)!);
            }
            ContentTypeIcons = contentTypes;

            var emotes = new Dictionary<uint, TextureWrap>();
            foreach (var emote in data.GetExcelSheet<Emote>()!)
            {
                var icon = data!.GetImGuiTextureHqIcon((uint)emote.Icon);
                if (icon == null)
                    continue;

                emotes.Add(emote.RowId, icon);
            }
            EmoteIcons = emotes;

            AetheryteIcon = data.GetImGuiTextureHqIcon(066417)!;
            WikiIcon = data.GetImGuiTextureHqIcon(066404)!;
            PluginInstallerIcon = data.GetImGuiTextureHqIcon(066472)!;
            LogoutIcon = data.GetImGuiTextureHqIcon(066403)!;
            EmoteIcon = data.GetImGuiTextureHqIcon(066420)!;
            HintIcon = data.GetImGuiTextureHqIcon(066453)!;
            ChatIcon = data.GetImGuiTextureHqIcon(066473)!;
            MathsIcon = data.GetImGuiTextureHqIcon(062409)!;

            this.ExtraIcons = new Dictionary<uint, TextureWrap>();

            ReloadMacroIcons();
        }

        public void ReloadMacroIcons()
        {
            foreach (var macroLink in FindAnythingPlugin.Configuration.MacroLinks)
            {
                EnsureExtraIcon((uint) macroLink.IconId);
            }
        }

        public void EnsureExtraIcon(uint iconId)
        {
            if (this.ExtraIcons.ContainsKey(iconId))
                return;

            var tex = this.data.GetImGuiTextureHqIcon(iconId);

            if (tex != null)
                this.ExtraIcons[iconId] = tex;
        }

        public static TextureCache Load(UiBuilder uiBuilder, DataManager data) => new TextureCache(uiBuilder, data);

        public void Dispose()
        {
            foreach (var icon in MainCommandIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in GeneralActionIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in ContentTypeIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in EmoteIcons)
            {
                icon.Value.Dispose();
            }

            foreach (var icon in this.ExtraIcons)
            {
                icon.Value.Dispose();
            }

            WikiIcon.Dispose();
            AetheryteIcon.Dispose();
            PluginInstallerIcon.Dispose();
            LogoutIcon.Dispose();
            EmoteIcon.Dispose();
            HintIcon.Dispose();
            ChatIcon.Dispose();
            MathsIcon.Dispose();
        }
    }
}
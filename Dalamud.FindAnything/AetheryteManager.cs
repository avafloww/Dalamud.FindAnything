﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;

namespace Dalamud.FindAnything {
    public class AetheryteManager {
        public readonly Dictionary<uint, string> AetheryteNames = new(150);
        public readonly Dictionary<uint, string> TerritoryNames = new(80);
        private readonly Dictionary<(int, int), string> m_HouseNames = new(5);
        private string? m_AppartmentName;

        private uint[] m_EstateIds = { 0 };

        private AetheryteManager()
        {
            var lang = FindAnythingPlugin.ClientState.ClientLanguage;
            SetupAetherytes(AetheryteNames, lang);
            SetupMaps(TerritoryNames, lang);
            SetupEstateIds(out m_EstateIds);
        }

        public static AetheryteManager Load() => new();

        public bool IsHousingAetheryte(uint id, byte plot, byte ward, byte subId) {
            if (plot != 0 || ward != 0 || subId != 0)
                return true;
            return m_EstateIds.Contains(id);
        }

        public string GetAetheryteName(AetheryteEntry info) {
            if (info.IsAppartment)
                return m_AppartmentName ??= GetAppartmentName();
            if (info.IsSharedHouse) {
                if (m_HouseNames.TryGetValue((info.Ward, info.Plot), out var house))
                    return house;
                house = GetSharedHouseName(info.Ward, info.Plot);
                m_HouseNames.Add((info.Ward, info.Plot), house);
                return house;
            }

            return AetheryteNames.TryGetValue(info.AetheryteId, out var name) ? name : "NO_DATA";
        }

        private static unsafe string GetAppartmentName() {
            var tm = Framework.Instance()->GetUiModule()->GetRaptureTextModule();
            var sp = tm->GetAddonText(8518);
            var name = Marshal.PtrToStringUTF8(new IntPtr(sp)) ?? string.Empty;
            return FindAnythingPlugin.PluginInterface.Sanitizer.Sanitize(name);
        }

        private static unsafe string GetSharedHouseName(int ward, int plot) {
            if (ward > 30) return $"SHARED_HOUSE_W{ward}_P{plot}";
            var tm = Framework.Instance()->GetUiModule()->GetRaptureTextModule();
            var sp = tm->FormatAddonText2(8519, ward, plot);
            return Marshal.PtrToStringUTF8(new IntPtr(sp)) ?? $"SHARED_HOUSE_W{ward}_P{plot}";
        }

        private static void SetupEstateIds(out uint[] array) {
            var list = new List<uint>(10);
            var sheet = FindAnythingPlugin.Data.GetExcelSheet<Aetheryte>(ClientLanguage.English)!;
            foreach (var aetheryte in sheet) {
                if (aetheryte.PlaceName.Row is 1145 or 1160)
                    list.Add(aetheryte.RowId);
            }
            array = list.ToArray();
        }

        private static void SetupAetherytes(IDictionary<uint, string> dict, ClientLanguage language) {
            var sheet = FindAnythingPlugin.Data.GetExcelSheet<Aetheryte>(language)!;
            dict.Clear();
            foreach (var row in sheet) {
                var name = row.PlaceName.Value?.Name?.ToString();
                if (string.IsNullOrEmpty(name))
                    continue;
                name = FindAnythingPlugin.PluginInterface.Sanitizer.Sanitize(name);
                dict[row.RowId] = name;
            }
        }

        private static void SetupMaps(IDictionary<uint, string> dict, ClientLanguage language) {
            var sheet = FindAnythingPlugin.Data.GetExcelSheet<Aetheryte>(language)!;
            dict.Clear();
            foreach (var row in sheet) {
                var name = row.Territory.Value?.PlaceName.Value?.Name?.ToString();
                if (string.IsNullOrEmpty(name))
                    continue;
                if (row is not { IsAetheryte: true }) continue;
                name = FindAnythingPlugin.PluginInterface.Sanitizer.Sanitize(name);
                dict[row.RowId] = name;
            }
        }
    }
}
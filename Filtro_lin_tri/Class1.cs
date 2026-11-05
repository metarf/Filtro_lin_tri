using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.IO;
using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ValheimTextureFilter
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    public class TextureFilterPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.giacomo.valheim.texturefilter";
        public const string PluginName = "Valheim Texture Filter Enhancer";
        public const string PluginVersion = "0.0.1";

        // Voci di configurazione .cfg
        public static ConfigEntry<bool> ModEnabled;
        public static ConfigEntry<FilterMode> SelectedFilterMode;
        public static ConfigEntry<int> AnisotropicLevel;

        private void Awake()
        {
            // Bind dei parametri di configurazione
            ModEnabled = Config.Bind("1. Generale", "Enabled", true, "Abilita o disabilita il filtraggio avanzato delle texture.");

            SelectedFilterMode = Config.Bind("2. Filtraggio Texture", "FilterMode", FilterMode.Bilinear,
                "Seleziona la modalità di filtraggio:\n" +
                "- Point: Aspetto originale pixel-art (quadrettato)\n" +
                "- Bilinear: Sfuma i pixel rendendo le superfici morbide e omogenee\n" +
                "- Trilinear: Come il Bilinear, ma migliora le transizioni visive e le sfumature in lontananza");

            AnisotropicLevel = Config.Bind("2. Filtraggio Texture", "AnisotropicLevel", 16,
                "Livello di Filtro Anisotropico (da 0 a 16).\n" +
                "Aumenta la nitidezza del terreno e dei muri osservati da angolazioni sbieche o in distanza.");

            // Registrazione dell'evento al caricamento della scena
            SceneManager.sceneLoaded += OnSceneLoaded;

            Logger.LogInfo($"{PluginName} v{PluginVersion} caricato con successo!");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (ModEnabled.Value)
            {
                // Avviamo una coroutine per attendere che gli asset della nuova scena vengano caricati in RAM
                StartCoroutine(ApplyTextureFilteringRoutine());
            }
        }

        private IEnumerator ApplyTextureFilteringRoutine()
        {
            // Breve pausa per assicurarsi che i pacchetti di texture della scena siano allocati
            yield return new WaitForSeconds(1.0f);

            Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            int processedCount = 0;

            int clampedAniso = Mathf.Clamp(AnisotropicLevel.Value, 0, 16);
            FilterMode modeToApply = SelectedFilterMode.Value;

            foreach (var tex in textures)
            {
                if (tex != null)
                {
                    // Applica la modalità di filtraggio scelta (Bilinear / Trilinear)
                    tex.filterMode = modeToApply;

                    // Imposta il livello di anisotropia per la nitidezza prospettica
                    tex.anisoLevel = clampedAniso;

                    processedCount++;
                }
            }

            Logger.LogInfo($"[TextureFilter] Filtro '{modeToApply}' e Anisotropico ({clampedAniso}x) applicati a {processedCount} texture.");
        }
    }
}

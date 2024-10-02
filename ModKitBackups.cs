using Life;
using Life.Network;
using Life.UI;
using ModKit.Helper;
using ModKit.Interfaces;
using ModKit.Internal;
using ModKit.Utils;
using ModKitBackups.Classes;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using _menu = AAMenu.Menu;
using mk = ModKit.Helper.TextFormattingHelper;

namespace ModKitBackups
{
    public class ModKitBackups : ModKit.ModKit
    {
        public static string ConfigBackupPath;
        public static BackupConfig _bakcupConfig;

        public ModKitBackups(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Aarnow");
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            InitConfig();
            _bakcupConfig = LoadConfigFile(ConfigBackupPath);

            InsertMenu();

            Nova.man.StartCoroutine(BackupCoroutine());

            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
        }

        #region Config
        private void InitConfig()
        {
            try
            {
                ConfigBackupPath = Path.Combine(DirectoryPath, "backupConfig.json");

                if (!File.Exists(ConfigBackupPath)) InitBackupConfig();
            }
            catch (IOException ex)
            {
                Logger.LogError("InitDirectory", ex.Message);
            }
        }
        private void InitBackupConfig()
        {
            BackupConfig backupConfig = new BackupConfig();

            backupConfig.IntervalMinutes = 10;

            string json = JsonConvert.SerializeObject(backupConfig);
            File.WriteAllText(ConfigBackupPath, json);
        }
        private BackupConfig LoadConfigFile(string path)
        {
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                BackupConfig backupConfig = JsonConvert.DeserializeObject<BackupConfig>(jsonContent);

                return backupConfig;
            }
            else return null;
        }
        #endregion

        public void InsertMenu()
        {
            _menu.AddAdminPluginTabLine(PluginInformations, 5, "ModKit Backups", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                ModKitBackupsPanel(player);
            });
        }
        public void ModKitBackupsPanel(Player player)
        {
            //Déclaration
            Panel panel = PanelHelper.Create("ModKit Backups", UIPanel.PanelType.TabPrice, player, () => ModKitBackupsPanel(player));

            //Corps
            panel.AddTabLine($"{mk.Color("Appliquer la configuration", mk.Colors.Info)}", _ =>
            {
                _bakcupConfig = LoadConfigFile(ConfigBackupPath);
                panel.Refresh();
            });

            panel.NextButton("Sélectionner", () => panel.SelectTab());
            panel.AddButton("Retour", _ => AAMenu.AAMenu.menu.AdminPluginPanel(player, AAMenu.AAMenu.menu.AdminPluginTabLines));
            panel.CloseButton();

            //Affichage
            panel.Display();
        }

        private IEnumerator BackupCoroutine()
        {
            while (true)
            {
                CreateBackup();
                yield return new UnityEngine.WaitForSeconds(_bakcupConfig.IntervalMinutes * 60);
            }
        }
        private void CreateBackup()
        {
            try
            {
                string timestamp = DateUtils.GetCurrentTime().ToString();
                string backupFileName = $"data_{timestamp}.sqlite";
                string backupFilePath = Path.Combine(BackupPath, backupFileName);

                File.Copy(Path.Combine(DirectoryPath, "data.sqlite"), backupFilePath, true);
            }
            catch (System.Exception ex)
            {
                Logger.LogError("Error creating backup:", ex.Message);
            }
        }
    }
}

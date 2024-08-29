using Life;
using ModKit.Helper;
using ModKit.Interfaces;
using ModKit.Internal;
using ModKit.Utils;
using System.Collections;
using System.IO;

namespace ModKitBackups
{
    public class ModKitBackups : ModKit.ModKit
    {
        public ModKitBackups(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Aarnow");
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();

            Nova.man.StartCoroutine(BackupCoroutine());

            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
        }

        private IEnumerator BackupCoroutine()
        {
            while (true)
            {
                CreateBackup();
                yield return new UnityEngine.WaitForSeconds(10 * 60); // 10 min
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

namespace CryoFall.UpdateChecker
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class UpdateChecker
    {
        private static DialogWindow dialogWindowInstance;

        private static StackPanel modListPanel;

        public static void CheckNewReleases(string modName, Version currentVersion, string rssFeed)
        {

            Api.Client.Core.RequestRssFeed(
                rssFeed,
                (releasesList) =>
            {
                if (releasesList?.Count > 0)
                {
                    var latestRelease = releasesList.First();
                    string versionText = latestRelease.Title.Substring(latestRelease.Title.IndexOf(' ') + 1);
                    if (versionText.Contains('v'))
                    {
                        versionText = versionText.Substring(1);
                    }
                    Version version = new Version(versionText);
                    if(version.CompareTo(currentVersion) > 0)
                    {
                        ShowUpdateDialog(modName, currentVersion.ToString(), versionText, latestRelease);
                    }
                }
            });
        }

        public static void ShowUpdateDialog(string modName, string oldVersion, string newVersion, RssFeedEntry latestRelease)
        {
            if (modListPanel == null)
            {
                modListPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
            }

            string text = string.Format("[*] [b]{0}[/b]    v{1} -> v{2}    [url={5}]update[/url]",
                modName,
                oldVersion,
                newVersion,
                latestRelease.Date.ToString(),
                latestRelease.Description,
                latestRelease.Url);
            
            var textBlock = new FormattedTextBlock()
            {
                Content = text,
                TextWrapping = TextWrapping.Wrap
            };
            modListPanel.Children.Add(textBlock);

            if(dialogWindowInstance != null)
            {
                dialogWindowInstance.Close(DialogResult.OK);
            }

            dialogWindowInstance = DialogWindow.ShowDialog(
                "New version of mods was found",
                modListPanel,
                () => { },
                cancelAction: null,
                closeByEscapeKey: true);
        }
    }
}
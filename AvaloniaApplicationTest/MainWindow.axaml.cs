using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using Avalonia.Animation;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using System.ComponentModel;
using Avalonia.Platform.Storage;
using System.Linq;
using Avalonia.Platform.Storage.FileIO;
using Avalonia;
using DynamicData;
using System.Collections;
using Avalonia.Controls.Selection;
using System.Drawing;

namespace AvaloniaApplicationTest {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private List<string> itemArray = new List<string>();
        private JsonArray? siteArray;
        private int siteIndex = 0;
        protected override void OnOpened(EventArgs e) {
            // 载入配置
            // CurrentFolder.Text = Setting.load();
            var jConfig = Setting.load();
            this.siteArray = jConfig["website"] != null ? JsonNode.Parse(jConfig["website"].AsArray().ToJsonString()).AsArray() : new JsonArray();
            var _siteIndex = jConfig["selected"] != null ? jConfig["selected"].GetValue<int>() : 0;
            // 载入到list
            for (var i = 0; i < siteArray.Count; i++) {
                var siteInfo = siteArray[i];
                if (siteInfo != null) {
                    string siteName = siteInfo["name"].GetValue<string>();
                    itemArray.Add(siteName);
                }
            }
            UpdateSiteArray();
            loadSelected(_siteIndex);
        }

        protected override void OnClosing(CancelEventArgs e) {
            storeSelected();
            base.OnClosing(e);
        }

        private void updateSiteInfo(JsonNode? siteInfo) {
            CurrentName.Text = (siteInfo != null && siteInfo["name"] != null) ? siteInfo["name"].GetValue<string>() : "";
            CurrentFolder.Text = (siteInfo != null && siteInfo["folder"] != null) ? siteInfo["folder"].GetValue<string>() : "";
            CurrentPort.Text = Convert.ToString((siteInfo != null && siteInfo["port"] != null) ? siteInfo["port"].GetValue<int>() : 50080);
        }
        
        public void siteChangeHandle(object sender, SelectionChangedEventArgs e) {
            JsonNode? siteInfo = null;
            if (FolderArray.SelectedIndex >= 0) {
                siteInfo = siteArray?[FolderArray.SelectedIndex];
            }
            siteIndex = FolderArray.SelectedIndex;
            updateSiteInfo(siteInfo);
        }

        private void UpdateSiteArray() {
            FolderArray.Items = new List<string>(itemArray.ToArray());
        }

        public async void SelectFolderHandle(object sender, RoutedEventArgs e) {
            BclStorageFolder basicFolder;
            try {
                basicFolder = new BclStorageFolder(new System.IO.DirectoryInfo(CurrentFolder.Text));
            }
            catch (Exception) {
                basicFolder = new BclStorageFolder(new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
            }
            // var button = (Button)sender;
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions() {
                AllowMultiple = false,
                Title = "选择静态资源文件夹",
                SuggestedStartLocation = basicFolder
            });
            CurrentFolder.Text = result.FirstOrDefault()?.TryGetUri(out var path) == true ? path.LocalPath : "";
        }

        private void loadSelected(int index) {
            if (siteArray == null || siteArray.Count <= 0 || index > siteArray.Count) {
                CurrentPort.Text = CurrentName.Text = CurrentFolder.Text = "";
                return;
            }
            siteIndex = index;
            FolderArray.SelectedIndex = siteIndex;
            JsonNode? siteInfo = null;
            if (index >= 0) {
                siteInfo = siteArray[index];
                if (siteInfo == null) {
                    return;
                }
            }
            updateSiteInfo(siteInfo);
        }

        private void storeSelected() {
            Setting.save(siteArray, siteIndex);
        }

        public async void AddSiteHandle(object sender, RoutedEventArgs e) {
            // 不允许包含同名网站
            if (itemArray.IndexOf(CurrentName.Text) >= 0) {
                await MessageBoxManager.GetMessageBoxStandardWindow("错误", "网站名称已存在", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).Show();
                return;
            }
            // 添加到 combox
            itemArray.Add(CurrentName.Text);
            // 添加到 json
            siteArray.Add(new JsonObject {
                ["name"] = CurrentName.Text,
                ["folder"] = CurrentFolder.Text,
                ["port"] = int.Parse(CurrentPort.Text)
            });
            UpdateSiteArray();
            loadSelected(siteArray.Count - 1);
        }

        public void DeleteSiteHandle(object sender, RoutedEventArgs e) {
            // 从方案列表删除
            var deleteSiteName = CurrentName.Text;
            itemArray.Remove(deleteSiteName);
            var newIndex = (FolderArray.SelectedIndex == itemArray.Count) ?
                FolderArray.SelectedIndex - 1 :
                siteIndex;
            loadSelected(newIndex);
            UpdateSiteArray();
            // 从json删除
            for (var i = 0; i < siteArray.Count; i++) {
                var siteInfo = siteArray[i];
                if ((string)siteInfo["name"] == deleteSiteName) {
                    siteArray.RemoveAt(i);
                }
            }
        }

        private HttpStaticServer? server;
        public async void ServiceControlHandle(object sender, RoutedEventArgs e) {
            var button = (Button)sender;
            if( server != null && server.isRunning() ) {
                server.stop();
                server = null;
                button.Content = "开启服务";
            } else {
                server = new HttpStaticServer(CurrentFolder.Text);
                if(CurrentPort.Text == null) {
                    await MessageBoxManager.GetMessageBoxStandardWindow("错误", "网站端口未设置", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error).Show();
                    return;
                }
                try {
                    server.listen(int.Parse(CurrentPort.Text));
                    button.Content = "关闭服务";
                }
                catch (Exception) {
                    server.stop();
                    server = null;
                }
            }
        }
    }
}
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace pasta5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window {

        public IniSettings ini = new IniSettings();
        public ModelImporter modelImporter = new ModelImporter();
        private object dummyNode = null; // TreeView
        private Collection<ObjFileItem> objFilesModel = new ObservableCollection<ObjFileItem>(); // OBJ list in ListBox

        public MainWindow() {
            InitializeComponent();
            objList.ItemsSource = objFilesModel;
            if( ini.Load() ) {
                //DumpPath.Text = ini.from;
                // TargetPath.Text = ini.to;
                DumpPath.Text = @"S:\ROTTR_DRMDumper_0.0.3a_r7\[MASS UNPACK - go through these in Noesis]";
                TargetPath.Text = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider\[MASS UNPACK]";
            }
        }

        /************************** TreeView (initially loaded through Window.Loaded) */
        public string SelectedImagePath { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem(); // TreeViewItem class instanciation.
                item.Header = s; // Gets or sets the label of the item.
                item.Tag = s; // Gets or sets an arbitrary object value that can be used to store custom information about this element.
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(Folder_Expanded); // Occurs when the IsExpanded property changes from false to true.
                foldersItem.Items.Add(item);
            }
        }

        /************************** TreeView "On Expand" */
        void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        /************************** TreeView Folder "On Select" */
        private void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            /* Build Sub-Folder  */
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }

            /* Load OBS List */
            /* Searches selected TreeView  sfolder for a RenderMesh subfolder. If it exists, load all .OBJs from it, and it's subfolders. */
            var renderMeshFolder = Path.Combine(SelectedImagePath, "RenderMesh"); // Folder to search for .OBJs in.
            objFilesModel.Clear();
            
            if (Directory.Exists(renderMeshFolder))
            {
                var objFiles = Directory.EnumerateFiles(renderMeshFolder, "*.obj", SearchOption.AllDirectories);
                foreach (string currentFile in objFiles)
                {
                    string fileName = Path.GetFileName(currentFile);
                    objFilesModel.Add(new ObjFileItem { filename = fileName, filepath = currentFile });
                }

                // Auto-load first model of TreeView selected folder, if any exists. Else, clear list for visual hint.
                if (objFilesModel.Any())
                {
                    if (objFilesModel.Count() == 1)
                    {
                        loadModel(objFilesModel.First().filepath);
                    }
                    else
                    {
                        objList.SelectedIndex = 0;
                        // objList.Focus(); // Focus() here not working well; Moving up the TreeView, will not select SelectedIndex = 0
                    }
                }
            }
        }

        /************************** OBJ File List */
        private void objList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (ObjFileItem)((ListBox)sender)?.SelectedItem;
            if (item == null)
            {
                return;
            }
            loadModel(item.filepath);
        }

        private void objList_KeyDown(object sender, KeyEventArgs e) {
            var item = (ObjFileItem)((ListBox)sender)?.SelectedItem;
            if( item == null )
                return;

            if( e?.Key == Key.Space ) {
                item.isChecked = !item.isChecked;
            }
        }

        /************************** Toggle All OBJs Selected/Deselected */
        private void Chkbox_selAll_Checked(object sender, RoutedEventArgs e)
        {
            ToggleAllChkBoxes(true);
        }

        private void Chkbox_selAll_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleAllChkBoxes(false);
        }

        private void ToggleAllChkBoxes(bool selectOrNot)
        {
            /*
            foreach (CheckBox chkbox in Controls.OfType<CheckBox>())
            {
                // chkBox.isSelected = selectOrNot;
                Console.WriteLine(chkbox.Name);
            }
            */
            MessageBox.Show(selectOrNot.ToString());
        }

        /************************** Button - Keep */
        /* The keep button, will create a new folder in FBX folder (where all folder are kept with OBJs, which i will then import into Blender).
         * Selected OBJs and all DDSs are moved to that new folder. Dumped folder will move to a processed folder, so i have the list of folders i've went through. */
        private void btn_keepFolder_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = (foldersItem.SelectedItem as TreeViewItem);
            TreeViewItem parentItem = (selectedItem.Parent as TreeViewItem);
            string allowedFolder = "[MASS UNPACK - go through these in Noesis]"; // Set allowed folder to run the discard button.

            if (parentItem.Header.ToString() == allowedFolder)
            {
                StatusLog.Text = "";

                // Variables
                var TargetPathRoot = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider"; // Root folder where all newly created folders exist. Used to check if folder exists in any subfolder.
                var targetPath = Path.Combine(TargetPathRoot, "[MASS UNPACK]"); // Folder where all new created folder will go to.
                var currentFolder = SelectedImagePath.Substring(SelectedImagePath.LastIndexOf("\\") + 1); // Get name of selected folder in TreeView.

                var processedFolder = Path.Combine(@"S:\ROTTR_DRMDumper_0.0.3a_r7\[COPIED TO FBX]", currentFolder); // Where to move the currentFolder to, when .objs and .dds are moved to newFolder.

                // * Check if newFolder has already been created.
                var dirs = Directory.EnumerateDirectories(TargetPathRoot, currentFolder, SearchOption.AllDirectories);
                if (dirs.Any())
                {
                    StatusLog.Text = "Folder already exists!";
                }
                else
                {
                    // * Check if any OBJs are selected. Else, can't create newFolder.
                    var checkedItems = objFilesModel.Where(x => x.isChecked);
                    if (checkedItems.Count() == 0)
                    {
                        StatusLog.Text = "No OBJs are selected!";
                    }
                    else
                    {
                        // * Create newFolder, with textures sub-folder.
                        var newFolder = Path.Combine(targetPath, currentFolder);
                        Directory.CreateDirectory(newFolder);
                        var textureFolder = Path.Combine(targetPath, currentFolder, "textures");
                        Directory.CreateDirectory(textureFolder);

                        // * Move selected .OBJs to newFolder.
                        foreach (ObjFileItem item in checkedItems)
                        {
                            Directory.Move(item.filepath, Path.Combine(newFolder, item.filename));
                        }

                        // Move .DDSs to newFolder\textures.
                        var ddsFiles = Directory.EnumerateFiles(SelectedImagePath, "*.dds", SearchOption.AllDirectories);

                        foreach (string ddsFile in ddsFiles)
                        {
                            string ddsFileName = Path.GetFileName(ddsFile);
                            Directory.Move(ddsFile, Path.Combine(newFolder, "textures", ddsFileName));
                        }

                        // * When moved .OBJs and .DDSs, move currentFolder to processedFolder.
                        Directory.Move(SelectedImagePath, processedFolder);

                        // * Start tree folder auto-select, on discard.
                        int nChildren = parentItem.Items.Count; // Sub-folder count of allowed folder.

                        /* When deleting folders in the Tree, top-to-bottom, the next folder auto-gets this index,
                         * so we can easily auto-select the next folder (will have the same index) without changing any values.
                         * Only when deleting bottom-to-top do we decrement the index.
                         * */
                        int selectedIndex = parentItem.Items.IndexOf(selectedItem);

                        parentItem.Items.Remove(selectedItem); // Remove folder from tree.
                        if (nChildren > 1)
                        { // If folders still exist, auto-select.
                            if (selectedIndex == nChildren - 1) // If last folder is selected.
                            {
                                selectedIndex = --selectedIndex; // Discarding from last folder to top folder, we need to decrease the index.
                            }
                                ((TreeViewItem)parentItem.Items[selectedIndex]).IsSelected = true;
                            ((TreeViewItem)parentItem.Items[selectedIndex]).Focus();
                        }
                        /* End tree folder auto-select, on discard. */

                        StatusLog.Text = "New folder & files created.";
                    }
                }
            }
            else
            {
                MessageBox.Show("You can only proccess folders inside a specific folder.");
            }
        }

        /************************** Button - Discard */
        /* The discard button will move the selected folder to a storage folder, stripping it of it's DDSs first, to save HDD space.
         * The storage folder will store folders i might want to revist for meshes later on. I will need to re-dump each of these folder's DDSs though.*/
        private void btn_discardFolder_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = (foldersItem.SelectedItem as TreeViewItem);
            TreeViewItem parentItem = (selectedItem.Parent as TreeViewItem);
            string allowedFolder = "[MASS UNPACK - go through these in Noesis]"; // Set allowed folder to run the discard button.

            if (parentItem.Header.ToString() == allowedFolder)
            {
                // Variables
                var currentFolder = SelectedImagePath.Substring(SelectedImagePath.LastIndexOf("\\") + 1); // Get name of selected folder in TreeView.
                var discardedFolderPath = Path.Combine(@"S:\ROTTR_DRMDumper_0.0.3a_r7\[NOT INTERESTED]", currentFolder); // Where to move the currentFolder to if i discard it.

                // * Process folder.
                var texturesFolder = Path.Combine(SelectedImagePath, "Texture"); // Where all DDSs are.
                if (Directory.Exists(texturesFolder)) { // If directory doesn't exist, the currentFolder has no meshes, therefore was dumped without a 'textures' folder. Delete it instead of moving it.
                    // * Delete DDS files and folders. // Delete DDSs separately, in case some have no containing folder. They always do, though, i think. Fuck it.
                    var ddsFiles = Directory.EnumerateFiles(texturesFolder, "*.dds", SearchOption.AllDirectories);
                    foreach (string ddsFile in ddsFiles)
                    {
                        File.Delete(ddsFile);
                    }
                    List<string> ddsDirs = new List<string>(Directory.EnumerateDirectories(texturesFolder));
                    foreach (var ddsDir in ddsDirs)
                    {
                        Directory.Delete(ddsDir, true);
                    }
                    // * Move discarded folder to storage.
                    Directory.Move(SelectedImagePath, discardedFolderPath);
                }
                else
                {
                    // * Inform to delete folder, instead of discarding.
                    MessageBox.Show("Folder has no meshes. Delete it rather than discarding.");
                }
            
                // * Start tree folder auto-select, on discard.
                int nChildren = parentItem.Items.Count; // Sub-folder count of allowed folder.
                int selectedIndex = parentItem.Items.IndexOf(selectedItem); 
                parentItem.Items.Remove(selectedItem); // Remove folder from tree.
                if (nChildren > 1) { // If folders still exist, auto-select.
                    if (selectedIndex == nChildren - 1) // If last folder is selected.
                    {
                        selectedIndex = --selectedIndex; // Discarding from last folder to top folder, we need to decrease the index.
                    }
                        ((TreeViewItem)parentItem.Items[selectedIndex]).IsSelected= true;
                        ((TreeViewItem)parentItem.Items[selectedIndex]).Focus();
                }
                /* End tree folder auto-select, on discard. */
                StatusLog.Text = "Folder discarded.";
            }
            else
            {
                MessageBox.Show("You can only process folders inside the 'Dump folder'.");
            }
        }

        /************************** Button - Delete */
        /* The delete button will hard delete the folder. This folder then won't show up in the discarded folder,
            * as they don't hold any interesting meshes, or no meshes at all. Meaning if i wan't to go through the discarded folder,
            i won't dump this folder for it's textures again and go through it again. */
        private void Btn_deleteFolder_Click(object sender, RoutedEventArgs e)
        {
            Directory.Delete(SelectedImagePath, true); // Hard delete current folder.

            // * Start tree folder auto-select, on discard.
            TreeViewItem selectedItem = (foldersItem.SelectedItem as TreeViewItem);
            TreeViewItem parentItem = (selectedItem.Parent as TreeViewItem);
            int nChildren = parentItem.Items.Count; // Sub-folder count of allowed folder.
            int selectedIndex = parentItem.Items.IndexOf(selectedItem); // Get selected folder index.
            parentItem.Items.Remove(selectedItem); // Remove folder from tree.
            if (nChildren > 1)
            { // If folders still exist, auto-select.
                if (selectedIndex == nChildren - 1) // If last folder is selected.
                {
                    selectedIndex = --selectedIndex; // Discarding from last folder to top folder, we need to decrease the index.
                }
                ((TreeViewItem)parentItem.Items[selectedIndex]).IsSelected = true; // Select folder in tree.
                ((TreeViewItem)parentItem.Items[selectedIndex]).Focus(); // Give it focus.
            }
            StatusLog.Text = "Folder deleted.";
        }

        /************************** Save Paths Button */
        private void Btn_save_paths_Click(object sender, RoutedEventArgs e)
        {
            // ini.from = @"C:\Users\Tester\Desktop\ROTTR_DRMDumper_0.0.3a_r7\[MASS UNPACK - go through these in Noesis]";
            // ini.to = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider\[MASS UNPACK]";
            // ini.Save();
        }

        /************************** 3D Preview */
        private void Viewport_DragOver(object sender, DragEventArgs e) {
            e.Handled = true;
            if( e.Data.GetDataPresent(DataFormats.FileDrop) ) {
                var objPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if( objPaths[0].ToLower().EndsWith(".obj") ) {
                    e.Effects = DragDropEffects.All;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
        }

        private void Viewport_Drop(object sender, DragEventArgs e) {
            if( e.Data.GetDataPresent(DataFormats.FileDrop) ) {
                var objPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                loadModel(objPaths[0]);
                //Focus(); // focus window. Fail
            }
        }

        private void loadModel(string path) {
            try {
                Model3DGroup model1 = modelImporter.Load(path);
                Model.Content = model1;
                Viewport.ZoomExtents();
            }
            catch( Exception ex ) {
                Model.Content = null;
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}

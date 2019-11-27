using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Linq; // For Enumerate Any()
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace pasta5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window{

        public IniSettings ini = new IniSettings(); // INI
        public ModelImporter modelImporter = new ModelImporter(); // 3D View

        private object dummyNode = null; // TreeView

        public MainWindow() {
            InitializeComponent();

            if( ini.Load() ) {
                //DumpPath.Text = ini.from;
                // TargetPath.Text = ini.to;
                DumpPath.Text = @"S:\ROTTR_DRMDumper_0.0.3a_r7\[MASS UNPACK - go through these in Noesis]";
                TargetPath.Text = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider\[MASS UNPACK]";
            }
        }

        /* TreeView - Start */
        public string SelectedImagePath { get; set; }

        // Create the tree view with the logical drive letters - this gets called through Window.Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s; // Gets or sets the item that labels the control.
                item.Tag = s; // Gets or sets an arbitrary object value that can be used to store custom information about this element.
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(Folder_Expanded); // Occurs when the IsExpanded property changes from false to true.
                foldersItem.Items.Add(item);
            }
        }

        // "on expand" class
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

        // on select item change
        private void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            // Variables
            // var renderMeshFolder;

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

            /* Load obj list */
            // Folder to search for obj files in.
            var renderMeshFolder = Path.Combine(SelectedImagePath, "RenderMesh");

            // 1 - Search selected folder for a RenderMesh subfolder. If it exists, load all .obj files from it, and it's subfolders.
            if (Directory.Exists(renderMeshFolder))
            {
                
                // MessageBox.Show("has RenderMesh");
                /* TODO */
                var objFiles = Directory.EnumerateFiles(renderMeshFolder, "*.obj", SearchOption.AllDirectories);

                foreach (string currentFile in objFiles)
                {
                    string fileName = Path.GetFileName(currentFile);

                    

                }

            }

            // MessageBox.Show(SelectedImagePath);
        }
        /* TreeView - End */


        /************************** ON TEXTBOX PRESS */
        private void Folder_name_MouseLeftButtonUp(object sender, MouseButtonEventArgs ev)
        {
            Folder_name.SelectAll(); // Select all, readying for next "paste".
            StatusLog.Clear(); // Remove message from status.
        }


        /************************** ON TEXTBOX PASTE */
        private void Folder_name_TextChanged(object sender, TextChangedEventArgs ev)
        {
            try {

                // Check for empty "paste TextBox", or whitespace input
                if (String.IsNullOrEmpty(Folder_name.Text))
                {
                    StatusLog.Text = "Invalid folder name";
                }
                else
                {

                    StatusLog.Clear(); // Remove previous message from status.


                    // Variables
                    var TargetPathRoot = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider"; // Root folder where all newly created folders exist. Used to check if folder has been created.

                    // btn_move_obj_dds.IsEnabled = true; // Re-enable MF&F button, case it was disabled when duplicated folder name was found.

                    // Check If To-Be-Created Folder Has Already Been Created
                    var dirs = Directory.EnumerateDirectories(TargetPathRoot, Folder_name.Text, SearchOption.AllDirectories);
                    if (dirs.Any()) // Folder has already been created! Halt process!
                    {
                        StatusLog.Text = "Folder already exists.";
                        // btn_move_obj_dds.IsEnabled = false; // Disable button, as a visual clue, and not to re-copy files accidentaly.
                    }
                    else
                    {

                        // Folder doesn't exist. Continue process;

                        // TO ADD HERE: Check if i've exported obj and dds files before creating folders

                        // Variables 
                        var dumpFolder = Path.Combine(DumpPath.Text, Folder_name.Text); // Dump folder created when dumping .drm file with DRMDumper. Folder name used to create new folder
                        var objDumpFolder = Path.Combine(dumpFolder, "RenderMesh"); // Folder where exported .obj files are
                        var ddsDumpFolder = Path.Combine(dumpFolder, "Texture"); // Folder where exported .dds files are

                        var newFolder = Path.Combine(TargetPath.Text, Folder_name.Text); // Created folder to move exported .obj and .dds files to

                        var processedFolder = Path.Combine(DumpPath.Text, "[COPIED TO FBX]", Folder_name.Text); // Where to move dumpFolder to, when .objs and .dds are moved to newFolder.


                        // Create new folders and subfolders
                        var path = Path.Combine(TargetPath.Text, Folder_name.Text);
                        Directory.CreateDirectory(path); // Create new folder.

                        var tex = Path.Combine(path, "textures");
                        Directory.CreateDirectory(tex); // Create "textures" sub-folder.

                        StatusLog.Text = "Folders created."; // Success

                        // Move all .OBJ files to newFolder
                        string sourceOBJDirectory = objDumpFolder;
                        string archiveOBJDirectory = newFolder;

                        var objFiles = Directory.EnumerateFiles(sourceOBJDirectory, "*.obj", SearchOption.AllDirectories);

                        foreach (string currentFile in objFiles)
                        {
                            string fileName = Path.GetFileName(currentFile);
                            Directory.Move(currentFile, Path.Combine(archiveOBJDirectory, fileName));
                        }


                        // And all .DDS files to newFolder as well.
                        string sourceDDSDirectory = ddsDumpFolder;
                        string archiveDDSDirectory = Path.Combine(newFolder, "textures");

                        var ddsFiles = Directory.EnumerateFiles(sourceDDSDirectory, "*.dds", SearchOption.AllDirectories);

                        foreach (string currentFile in ddsFiles)
                        {
                            string fileName = Path.GetFileName(currentFile);
                            Directory.Move(currentFile, Path.Combine(archiveDDSDirectory, fileName));
                        }

                        // Console.Error.WriteLine(dumpFolder);
                        // Console.Error.WriteLine(processedFolder);

                        // When files are moved, move dumpFolder to processedFolder.
                        Directory.Move(dumpFolder, processedFolder);

                        StatusLog.Text = "Done."; // Success


                    }
                }
            }
            catch( Exception ex ) {
                Console.Error.WriteLine(ex.ToString());
            }
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

                try {
                    Model3DGroup model1 = modelImporter.Load(objPaths[0]);
                    Model.Content = model1;
                }
                catch( Exception ex ) {
                    Model.Content = null;
                    Console.Error.WriteLine(ex.ToString());
                }
                //Focus(); // focus window. Fail
            }
        }
    }
}

using System;
using System.IO;
using System.Linq; // For Enumerate Any()
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pasta5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>


    public partial class MainWindow : Window {
        
        public IniSettings ini = new IniSettings();

        public MainWindow() {
            InitializeComponent();

            if( ini.Load() ) {
                //DumpPath.Text = ini.from;
                // TargetPath.Text = ini.to;
                DumpPath.Text = @"S:\ROTTR_DRMDumper_0.0.3a_r7\[MASS UNPACK - go through these in Noesis]";
                TargetPath.Text = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider\[MASS UNPACK]";

            }
        }
        

        
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
    }
}

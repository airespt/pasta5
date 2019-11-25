using System;
using System.IO;
using System.Linq;
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

                // Variables
                var TargetPathRoot = @"S:\Dying Light Dev Stuff\FBX\Rise of the Tomb Raider"; // Folder where all created folders exist!

                // Reset Any Previous Error Signals
                btn_move_obj_dds.IsEnabled = true; // Re-enable MF&F button, case it was disabled when duplicated folder name was found.
                StatusLog.Clear(); // Remove error message from status.
                
                // Check If To-Be-Created Directory Already Exists Anywhere
                var dirs = from dir in Directory.EnumerateDirectories(TargetPathRoot, Folder_name.Text, SearchOption.AllDirectories) select dir;
                if (dirs.Count<string>() != 0) // Folder exists. Halt process!
                {
                    StatusLog.Text ="Folder already exists.";
                    btn_move_obj_dds.IsEnabled = false; // Disable button, as a visual clue.
                }
                else
                { 
                    // Folder doesn't exists. Continue process...
                    var path = Path.Combine(TargetPath.Text, Folder_name.Text);
                    Directory.CreateDirectory(path); // Create new folder.

                    var tex = Path.Combine(path, "textures");
                    Directory.CreateDirectory(tex); // Create "textures" sub-folder.
                    
                    StatusLog.Text = "Folders created."; // Success

                    // Clipboard.SetText(path);
                }
            }
            catch( Exception ex ) {
                Console.Error.WriteLine(ex.ToString());
            }
        }


        /************************** MOVE FILES & FOLDERS BUTTON */
        private void Btn_move_obj_dds_Click(object sender, RoutedEventArgs e)
        {
            // Check for empty, or whitespace folder name...
            if (String.IsNullOrEmpty(Folder_name.Text))
            {
                StatusLog.Text ="Bad User!";
            }

            // If good, proceed.
            else
            {
                // Pretty Variables 
                var dumpFolder = Path.Combine(DumpPath.Text, Folder_name.Text); // Dump folder created when dumping .drm file with DRMDumper. Folder name used to create new folder
                var objDumpFolder = Path.Combine(dumpFolder, "RenderMesh"); // Folder where exported .obj files are
                var ddsDumpFolder = Path.Combine(dumpFolder, "Texture"); // Folder where exported .dds files are

                var newFolder = Path.Combine(TargetPath.Text, Folder_name.Text); // Created folder to move exported .obj and .dds files to

                var processedFolder = Path.Combine(DumpPath.Text, "[COPIED TO FBX]", Folder_name.Text); // Folder to move dumpFolder when .objs and .dds are moved to newFolder

                

                // Move all .OBJ files to newFolder
                string sourceOBJDirectory = objDumpFolder;
                string archiveOBJDirectory = newFolder;

                var objFiles = Directory.EnumerateFiles(sourceOBJDirectory, "*.obj", SearchOption.AllDirectories);

                foreach (string currentFile in objFiles)
                {
                   // StatusLog.AppendText(currentFile + Environment.NewLine);

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

                
                Console.Error.WriteLine(dumpFolder);
                Console.Error.WriteLine(processedFolder);

                // When files are moved, move dumpFolder to processedFolder.
                Directory.Move(dumpFolder, processedFolder);

                StatusLog.Text = "Done."; // Success
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

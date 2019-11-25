using System;
using System.IO;
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
                DumpPath.Text = ini.from;
                TargetPath.Text = ini.to;
            }
        }

        private void Folder_name_MouseLeftButtonUp(object sender, MouseButtonEventArgs ev)
        {
            // selecionar o texto, para poder fazer um paste
            Folder_name.SelectAll();


            ini.from = "abc";
            ini.to = "def";
            ini.Save();
        }

        private void Folder_name_TextChanged(object sender, TextChangedEventArgs ev)
        {
            // quando o paste é feito, correr as instruçoes
            /*
            1.Adicionar uma TextBox que receba copy - paste;
            2.Quando a TextBox recebe "paste", criar uma folder num local especifico, nomeada com o texto inserido na TextBox;
            3.Criar uma sub-folder nesta folder nova com o nome "textures";
            4.Copiar a path da folder para o clipboard;
            */
            try {
                var path = Path.Combine(TargetPath.Text, Folder_name.Text);
                Directory.CreateDirectory(path);

                var tex = Path.Combine(path, "textures");
                Directory.CreateDirectory(tex);

                Clipboard.SetText(path);
            }
            catch( Exception ex ) {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private void Btn_move_obj_dds_Click(object sender, RoutedEventArgs e)
        {
            // Pretty Variables 
            var dumpFolder = Path.Combine(DumpPath.Text, Folder_name.Text); // Dump folder created when dumping .drm file with DRMDumper. Folder name used to create new folder
            var objDumpFolder = Path.Combine(dumpFolder, "RenderMesh"); // Folder where exported .obj files are
            var ddsDumpFolder = Path.Combine(dumpFolder, "Texture"); // Folder where exported .dds files are

            var newFolder = Path.Combine(TargetPath.Text, Folder_name.Text); // Created folder to move exported .obj and .dds files to

            var processedFolder = "[exported to obj]"; // Folder to move dumpFolder when .objs and .dds are moved to newFolder
            

            /* Actions */
            
            // Move all .OBJ files to newFolder
            string sourceOBJDirectory = objDumpFolder;
            string archiveOBJDirectory = newFolder;

            StatusLog.Clear();

            var objFiles = Directory.EnumerateFiles(sourceOBJDirectory, "*.obj", SearchOption.AllDirectories);
            foreach (string currentFile in objFiles)
            {
                StatusLog.AppendText(currentFile + Environment.NewLine);

                string fileName = Path.GetFileName(currentFile);
                Directory.Move(currentFile, Path.Combine(archiveOBJDirectory, fileName));
            }

            // And all .DDS files to newFolder as well.
            string sourceDDSDirectory = ddsDumpFolder;
            string archiveDDSDirectory = Path.Combine(newFolder, "textures");

            var ddsFiles = Directory.EnumerateFiles(sourceDDSDirectory, "*.dds", SearchOption.AllDirectories);
            foreach (string currentFile in ddsFiles)
            {
                StatusLog.AppendText(currentFile + Environment.NewLine);

                string fileName = Path.GetFileName(currentFile);
                Directory.Move(currentFile, Path.Combine(archiveDDSDirectory, fileName));
            }

            // When files are moved, move dumpFolder to processedFolder.
            Directory.Move(dumpFolder, processedFolder);

            // End Click.
            StatusLog.AppendText("DONE");
        }
    }
}

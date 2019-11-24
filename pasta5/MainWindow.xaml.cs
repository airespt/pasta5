using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace pasta5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Folder_name_MouseLeftButtonUp(object sender, MouseButtonEventArgs ev)
        {
            // selecionar o texto, para poder fazer um paste
            Folder_name.SelectAll();
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
            var objFolder = Path.Combine(DumpPath.Text, Folder_name.Text, "RenderMesh"); // Folder where .objs are.
            var newFolder = Path.Combine(TargetPath.Text, Folder_name.Text); // Folder to copy files to.
            // On "Mouse Click":
            // 1. Check if folder named with TextBox input text exists;

            // 2. If it does:
            // a. Move all .OBJ files
            string sourceDirectory = objFolder; // @"C:\current";
            string archiveDirectory = newFolder; // @"C:\archive";

            StatusLog.Clear();
            var objFiles = Directory.EnumerateFiles(sourceDirectory, "*.obj", SearchOption.AllDirectories);
            foreach (string currentFile in objFiles)
            {
                StatusLog.AppendText(currentFile + Environment.NewLine);

                string fileName = Path.GetFileName(currentFile);
                Directory.Move(currentFile, Path.Combine(archiveDirectory, fileName));
            }
            StatusLog.AppendText("DONE");

            /*
            string[] objFiles = Directory.EnumerateFiles(objFolder.ToString(), "*.obj");
            var i = 0;
            foreach (var item in objFiles)
            {
                var src = Path.Combine(dumpedFilesFolderPath, folderName, item);
                var dst = Path.Combine(targetPath, item);
                if (i == 0) {
                    Console.Error.WriteLine(item);
                    Console.Error.WriteLine(dumpedFilesFolderPath);
                    Console.Error.WriteLine(folderName);
                    Console.Error.WriteLine(objFolder);
                    Console.Error.WriteLine(targetPath);
                    i++;
                }
                 /*Console.Error.WriteLine("src: " + src);
                Console.Error.WriteLine("dst: " + dst);*/

            // File.Move(src, dst);
            //}

            // b. Move all .DDS files
            // var ddsFolder = Path.Combine(path, dumpedFolderName, "Texture");

        }
    }
}

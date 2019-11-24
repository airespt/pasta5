using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pasta5 {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Folder_name_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // selecionar o texto, para poder fazer um paste
            Folder_name.SelectAll();
        }

        private void Folder_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            // quando o paste é feito, correr as instruçoes
            /*
            1.Adicionar uma TextBox que receba copy - paste;
            2.Quando a TextBox recebe "paste", criar uma folder num local especifico, nomeada com o texto inserido na TextBox;
            3.Criar uma sub-folder nesta folder nova com o nome "textures";
            4.Copiar a path da folder para o clipboard;
            */
        }
    }
}

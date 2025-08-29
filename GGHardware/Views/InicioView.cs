using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GGHardware.Views
{
    public partial class InicioView : UserControl
    {
        private void BtnInicioSesion_Click(object sender, RoutedEventArgs e)
        {
            //lógica de navegación a la vista de Inicio de Sesión
            MessageBox.Show("Ir a Inicio de Sesión");
        }

        private void BtnRegistrarse_Click(object sender, RoutedEventArgs e)
        {
            //lógica de navegación a la vista de Registro
            MessageBox.Show("Ir a Registro");
        }


        public InicioView()
        {
            InitializeComponent();
        }
    }
}
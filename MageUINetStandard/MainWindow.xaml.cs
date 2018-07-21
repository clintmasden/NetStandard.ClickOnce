using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace MageUINetStandard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            txtPublishDirectory.Text = System.AppDomain.CurrentDomain.BaseDirectory;
        }

        private void btnPublishDirectory_Click(object sender, RoutedEventArgs e)
        {
            txtPublishDirectory.Text = FolderDialogDirectory();
        }

        private void btnManifestFile_Click(object sender, RoutedEventArgs e)
        {
            txtSolutionManifestFile.Text = FileDialogManifest();
        }

        private void btnApplicationManifestFile_Click(object sender, RoutedEventArgs e)
        {
            txtApplicationManifestFile.Text = FileDialogManifest();
        }

        private void btnCertificateFile_Click(object sender, RoutedEventArgs e)
        {
            txtCertificateFile.Text = FileDialogCertificate();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (CheckPaths())
            {
                var manifestRoutines = new ManifestRoutines();

                lblStatus.Content = "Updating Manifest File...";
                manifestRoutines.UpdateManifest(txtSolutionManifestFile.Text);

                lblStatus.Content = "Copying .Net Standard DLLs...";
                manifestRoutines.CopyNetStandardDlls(txtPublishDirectory.Text);

                lblStatus.Content = "Launching MageUI for Certificates";
                manifestRoutines.UpdateCertificates(txtCertificateFile.Text, txtSolutionManifestFile.Text, txtApplicationManifestFile.Text);

                lblStatus.Content = "Waiting...";
            }
        }

        private bool CheckPaths()
        {
            if (!File.Exists(txtSolutionManifestFile.Text))
            {
                MessageBox.Show("Solution Manifest file was not found.");
                return false;
            }

            //if (!File.Exists(txtApplicationManifestFile.Text))
            //{
            //    MessageBox.Show("Application Manifest file was not found.");
            //    return false;
            //}

            //if (!File.Exists(txtCertificateFile.Text))
            //{
            //    MessageBox.Show("Certificate file was not found.");
            //    return false;
            //}

            if (!Directory.Exists(txtPublishDirectory.Text))
            {
                MessageBox.Show("Publish directory was not found.");
                return false;
            }

            return true;
        }

        private string FileDialogManifest()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".manifest";
            dlg.Filter = "Dll Files (*.dll.manifest)|*.dll.manifest|Exe Files (*.exe.manifest)|*.exe.maniest|All Files|*";
            dlg.InitialDirectory = txtPublishDirectory.Text;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.FileName;
            }

            return null;
        }

        private string FileDialogCertificate()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".pfx";
            dlg.Filter = "Certicates Files (*.pfx)|*.pfx";
            dlg.InitialDirectory = txtPublishDirectory.Text;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                return dlg.FileName;
            }

            return null;
        }

        private string FolderDialogDirectory()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }

            return null;
        }
    }
}
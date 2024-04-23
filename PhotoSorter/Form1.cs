using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoSorter
{
    public partial class PhotoSorter : Form
    {
        string path = "";
        string[] files;
        string[] directories;
        string fileType;
        string oldPath;
        string newPath;
        List<String> photos;
        ComboBox lstBox;
        Bitmap bmp;
        int index = 0;

        public PhotoSorter()
        {
            InitializeComponent();
            Width = 800;
            Height = 680;
            BackgroundImageLayout = ImageLayout.Zoom;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

            lstBox = new ComboBox();
            lstBox.Width = 100;
            lstBox.Height = 20;
            lstBox.Left = 0;
            lstBox.Top = 0;
            lstBox.DropDownStyle = ComboBoxStyle.DropDownList;
            lstBox.SelectedIndexChanged += MovePhoto;
            Controls.Add(lstBox);
            Controls.Add(CreateButton(100, 20, "Undo/redo", 20, 0, UndoRedo));
            Controls.Add(CreateButton(100, 20, "Again", 40, 0, MovePhoto));
            Shown += FormShown;
        }

        private void FormShown(object sender, EventArgs e)
        {
            do GetPath();
            while (NoPath());
            GetFiles();
            GetDirectories();
        }

        /// <summary>
        /// Selects a path for sorting images
        /// </summary>
        private void GetPath()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            path = fbd.SelectedPath;
        }

        /// <summary>
        /// No Path checks whether a path is set for the images to be sorted from
        /// </summary>
        /// <returns>True if there is no path set (empty string)</returns>
        private bool NoPath()
        {
            if (path == "")
            {
                MessageBox.Show("Path Needed");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the file paths of the files in the directory
        /// </summary>
        private void GetFiles()
        {
            files = Directory.GetFiles(path);
            photos = new List<string>();
            foreach (string file in files)
            {
                fileType = file.Split('.').Last();
                if (fileType.ToUpper() == "PNG" || fileType.ToUpper() == "JPG")
                {
                    photos.Add(file);
                }
            }
            if (photos.Count == 0)
                Close();
            else
            {
                bmp = new Bitmap(photos[0]);
                BackgroundImage = bmp;
            }
        }

        /// <summary>
        /// Gets the subdirectory paths in the directory and adds them as a list option
        /// </summary>
        private void GetDirectories()
        {
            directories = Directory.GetDirectories(path);
            foreach (string s in directories)
            {
                lstBox.Items.Add(s.Split('\\').Last());
            }
        }

        /// <summary>
        /// Moves the photo to the corresponding directory, renaming the file with (i) after the name if the filename already exists in that directory
        /// </summary>
        private void MovePhoto(object sender, EventArgs e)
        {
            oldPath = photos[index];
            newPath = directories[lstBox.Items.IndexOf(lstBox.Text)] + "\\" + photos[index].Split('\\').Last();
            BackgroundImage.Dispose();
            try
            {
                File.Move(oldPath, newPath);
            }
            catch
            {
                int i = 1;
                // If new file path is already in use, iterate until an available new file path is selected and move the file
                while (File.Exists(oldPath))
                {
                    newPath = directories[lstBox.Items.IndexOf(lstBox.Text)] + "\\"
                        + photos[index].Split('\\').Last().Split('.')[0] + "(" + i++ + ")."
                        + photos[index].Split('\\').Last().Split('.')[1];
                    File.Move(oldPath, newPath);
                }
            }
            // Select next photo
            index++;
            SetPhoto();
        }

        /// <summary>
        /// Undo or redo the moving of the last image file
        /// </summary>
        private void UndoRedo(object sender, EventArgs e)
        {
            // If a photo has been moved before
            if (oldPath != null)
            {
                // Swap old and new paths (needed for redo) and move the image file
                string temp = oldPath;
                oldPath = newPath;
                newPath = temp;
                BackgroundImage.Dispose();
                File.Move(oldPath, newPath);
                if (photos.IndexOf(oldPath) > -1)
                    index++; // Increase if redo
                else
                    index--; // Decrease if undo
                SetPhoto();
            }
        }

        /// <summary>
        /// Sets the background of the form to the next photo to sort.
        /// If the last photo sorted was the last photo to sort,
        /// confirm that it was sorted. If correct then close the application,
        /// otherwise undo the image file movement.
        /// </summary>
        private void SetPhoto()
        {
            BackgroundImage.Dispose();
            if (index == photos.Count)
            {
                Close();
                return;
            }
            bmp = new Bitmap(photos[index]);
            BackgroundImage = bmp;
        }

        /// <summary>
        /// Reusable method to create button
        /// </summary>
        /// <param name="w">Width of button</param>
        /// <param name="h">Height of button</param>
        /// <param name="txt">Text of button</param>
        /// <param name="y">Top coordinate of button</param>
        /// <param name="x">Left coordinate of button</param>
        /// <param name="act">Method that is run when button is clicked</param>
        /// <returns></returns>
        private Button CreateButton(int w, int h, string txt, int y, int x, EventHandler act)
        {
            Button btn = new Button();
            btn.Width = w;
            btn.Height = h;
            btn.Text = txt;
            btn.Left = x;
            btn.Top = y;
            btn.Click += act;
            return btn;
        }
    }
}

using Gen;
using mzxrules.OcaLib;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Z64Compresser
{
    public partial class RomCompresserForm : Form
    {
        string modifiedFile;
        string compressedFile;

        public RomCompresserForm()
        {
            InitializeComponent();
        }

        private void RomCompresserForm_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = new ORom.Build[] { ORom.Build.N0, ORom.Build.MQP };

        }

        private void modButton_Click(object sender, EventArgs e)
        {
            modifiedOpenFileDialog.ShowDialog();
        }

        private void comButton_Click(object sender, EventArgs e)
        {
            CompressedOpenFileDialog.ShowDialog();
        }

        private void modifiedOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            modifiedFile = modifiedOpenFileDialog.FileName;
            mRomLabel.Text = modifiedFile;
        }

        private void CompressedOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            compressedFile = CompressedOpenFileDialog.FileName;
            cLabel.Text = compressedFile;
        }

        private void compressButton_Click(object sender, EventArgs e)
        {
            TaskScheduler uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            ORom.Build version = (ORom.Build)comboBox1.SelectedItem;

            if (//version != null            && 
               File.Exists(modifiedFile)
            && File.Exists(compressedFile))
            {
                this.Text = "Compressing Rom...";

                using (FileStream fw = new FileStream("Compressed_Test.z64", FileMode.Create))
                {
                    RomBuilder.CompressRom(new ORom(modifiedFile, version), new ORom(compressedFile, version), fw);
                }
            }
            this.Text = "Compressed!";


            //wip hack
            //FileTable ft = new FileTable();
            //ft.Initialize(modifiedFile, version);
            //var va = ft.GetVirtualAddress(0x02A91000);
            //var br = ft.GetFile(va.Start);

            //CRC.WriteCRC("Compressed_Test2.z64");

            //FileStream fs = new FileStream("test", FileMode.Create);

            //this.Text = "Compressing...";
            //Yaz0.EncodeAsync(br.ReadBytes((int)va.Size), (int)va.Size, fs)
            //    .ContinueWith(t =>
            //    {
            //        this.Text = "Complete";
            //        fs.Close();
            //    },
            //    uiTaskScheduler);
        }
    }
}

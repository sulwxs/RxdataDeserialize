using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rxdatadecoder
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }
        public void  ShowDialog(Point pos) 
        {
            this.Location = pos;
            this.ShowDialog();
        }
        private void Closebutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

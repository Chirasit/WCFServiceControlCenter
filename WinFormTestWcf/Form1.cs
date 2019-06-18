using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormTestWcf.ServiceReference1;

namespace WinFormTestWcf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            ServiceReference1.ServiceControlCenterClient serviceClient = new ServiceControlCenterClient();
            serviceClient.test();
            //AfterLotEndEventArgs afterLotEndEventArgs = new AfterLotEndEventArgs()
            //{
            //    LotNo = "",
            //    McNo = ""
            //};
            //AfterLotEndResult result = serviceClient.AfterLotEnd(afterLotEndEventArgs);
            //if (result.HasError)
            //{
            //    MessageBox.Show(result.ErrorMessage);
            //}
            //else
            //{
            //    MessageBox.Show(result.WarningMessage);
            //}
            //serviceClient.AddSpecialFlow(0);
            
            //MessageBox.Show(tmpStr);
        }

        private void Button2_Click(object sender, EventArgs e)
        {

        }
    }
}

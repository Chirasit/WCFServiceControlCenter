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

            ItemCheckingResult result = serviceClient.JigToolChecking(new ItemCheckingEventArgs());
            MessageBox.Show(result.WarningMessage);
            //MessageBox.Show(tmpStr);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ServiceReference1.ServiceControlCenterClient serviceClient = new ServiceControlCenterClient();
            AfterLotEndEventArgs endEventArgs = new AfterLotEndEventArgs();
            endEventArgs.LotNo = "1940A4125V";
            endEventArgs.McNo = "FT-RAS-001";
            endEventArgs.LotJudge = "FT_BIN19_AUTO3";
            AfterLotEndResult result = serviceClient.AfterLotEnd(endEventArgs);
        }
    }
}

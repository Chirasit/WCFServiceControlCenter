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
            //List<JigDataInfo> tmp = serviceClient.jig("", "");
            //serviceClient.jigto
            JigDataInfo[] tmp = serviceClient.JigGetData("WB-M-284", "");
            ItemCheckingResult result = serviceClient.JigToolChecking(new ItemCheckingEventArgs());
            MessageBox.Show(result.WarningMessage);
            //MessageBox.Show(tmpStr);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ServiceReference1.ServiceControlCenterClient serviceClient = new ServiceControlCenterClient();
            JigDataInfo jigDataInfo = new JigDataInfo();
            jigDataInfo.Type = "Wire";
            jigDataInfo.QrCodeByUser = "JIG151369";

            string[] a = { "WB" };
            serviceClient.JigCheckData("WB-M-164", "005763", "2045E5003V", jigDataInfo, "Wire", a);
            //serviceClient.che
            //AfterLotEndEventArgs endEventArgs = new AfterLotEndEventArgs();
            //endEventArgs.LotNo = "1940A4125V";
            //endEventArgs.McNo = "FT-RAS-001";
            //endEventArgs.LotJudge = "FT_BIN19_AUTO3";
            //AfterLotEndResult result = serviceClient.AfterLotEnd(endEventArgs);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //ServiceReference1.ServiceControlCenterClient serviceClient = new ServiceControlCenterClient();
            //ResultInfo r = new ResultInfo();
            //JigDataInfo[] jigDataInfos = serviceClient.JigGetData("WB-M-000", "");
            //if (jigDataInfos != null)
            //{

            //}
            ServiceReference1.JigDataInfo jigDataInfo = new JigDataInfo();
            jigDataInfo.Type = "HP";
            jigDataInfo.ShortName = "HP";
            jigDataInfo.QrCodeByUser = "VSON008X2020,RC008-M275,H-2104-01";
            jigDataInfo.Value = 0;
            jigDataInfo.Id = 0 ;
            ServiceReference1.ResultInfo ret = new ServiceReference1.ResultInfo();
            string[] param = null;
            //ServiceReference1.ResultInfo result = new ServiceReference1.ResultInfo();
            ServiceReference1.JigDataInfo jig = new JigDataInfo();
            ServiceReference1.ServiceControlCenterClient serviceControlCenterClient = new ServiceReference1.ServiceControlCenterClient();
            //jig = serviceControlCenterClient.JigCheckData("WB-F-142", "005678", "2103A5293V", jigDataInfo, "Capillary", param);
            serviceControlCenterClient.JigSetupData("WB-M-357", "005678", "", jigDataInfo, "HP", param);
            //ret = serviceControlCenterClient.JigUpdateData("WB-M-356", "005678", "2102A5029V", jigDataInfo, "Capillary", null);
            //MessageBox.Show(jig.SmallCode);
            //jigDataInfo.IsPass = !result.HasError;
            //jigDataInfo.Message_Eng = result.ErrorMessage;
            //jigDataInfo.Message_Thai = result.ErrorMessage_Tha;
            //jigDataInfo.Warning = result.WarningMessage;
            //if (result.JigDataInfo != null)
            //{
            //    jigDataInfo.Id = result.JigDataInfo.Id;
            //    jigDataInfo.SmallCode = result.JigDataInfo.SmallCode;
            //}
            //ret.Add(result);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            ServiceReference1.AfterLotEndResult result = new ServiceReference1.AfterLotEndResult();
            ServiceReference1.ServiceControlCenterClient serviceControlCenterClient = new ServiceReference1.ServiceControlCenterClient(); 

            AfterLotEndEventArgs args = new AfterLotEndEventArgs()
            {
                LotNo = "2125A2250V",
                McNo = "PL-X-12",
                OpNo = "005678",   
                LotDataQuantity = new LotData()
                {
                    
                },
            };

           result = serviceControlCenterClient.AfterLotEnd(args);
        }
    }
}

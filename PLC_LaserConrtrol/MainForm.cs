using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace PLC_LaserConrtrol
{
    public partial class MainForm : Form
    {

        // new CLASS
        PLCAction PLCAction = new PLCAction();

        System.Threading.Timer timer4; //手動雷射源專用 timer
        System.Threading.Timer timer3; //手動雷射初始訊號重置專用 timer
        int LaserReq, LaserInterCntl, LaserGuide, LaserOn, LaserProStart, LaserResetErr, LaserAnalogCntl, LaserSCReset;
        int M1209 = -1;
        public int LaserReset = -1;//安全機制:系統交握
        int SWIRedLightReqFlag = -1;
        int SWILaserReqFlag = -1;

        private string _args;
        public MainForm()
        {
            InitializeComponent();
        }
        public MainForm(string value)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(value))
            {
                _args = value;  //1為工研院版本,2為利通版本
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PLCAction.PLC_Connect(0);
            LaserReset = 1; // 交握訊號重置

            //雷射源手動頁面專用 timer
            TimerCallback callback3 = new TimerCallback(_do3);
            timer4 = new System.Threading.Timer(callback3, null, 0, 500);

            //每五百毫秒起來一次
            TimerCallback callback2 = new TimerCallback(PLC_HandShakingSet);
            timer3 = new System.Threading.Timer(callback2, null, 0, 500);

            if (_args == "1")//工研院版本
            {
                this.tabPage2.Parent = null;
                this.tabPage3.Parent = null;
                this.tabPage5.Parent = null;
            }
            else if (_args == "2")//利通版本
            {
                this.tabPage1.Parent = null;
                this.tabPage3.Parent = null;
                this.tabPage5.Parent = null;
            }
            else if (_args == "3")//雷捷版本
            {
                this.tabPage1.Parent = null;
                this.tabPage2.Parent = null;
                this.tabPage4.Parent = null;
            }


        }
        private void PLC_HandShakingSet(object state)
        {
            //-================系統交握===============================
            //** 雷射重置機制,回傳值給PLC表示我還活著
            //** M1200 重置, M1209 傳給 PLC 訊號
            //** M1200 在程式起來時只需要做一次, by LaserReset flag 判段只須做一次
            //** PLC 將 M1209 = 0, PC端將 M1209 =1, 不斷循環

            if (LaserReset == 0)
            {
                PLCAction.axActUtlType1.SetDevice("M1200", 0);
            }
            else if (LaserReset == 1)
            {
                PLCAction.axActUtlType1.SetDevice("M1200", 1);
                LaserReset = 0;
            }
            if (M1209 == 0)
            {
                PLCAction.axActUtlType1.SetDevice("M1209", 1);
            }



        }

        private void _do3(object state)
        {
            this.BeginInvoke(new ManualLaserTag1(ManualLaserTag2));//委派
        }
        delegate void ManualLaserTag1();
        private void ManualLaserTag2()
        {

            //讀回系統M1209交握值
            M1209 = PLCAction.PLC_HandShakingRead();

            double[] _ReadVal = PLCAction.ReadPLCDataRandom("M1220\nM1222\nM1224\nM1226\nM1221\nM1223\nM1225\nM1227\nM1205\nM1206", 10);
            // int X30,X34,X38,X3C,X31,X35,X39,X3D,X32,X36,X3A,X3E,X33,X37,X3B,X3F;

            //IPG 24 雷捷 23
            int[] ReadXYval = new int[47];


            //IPG: 0-23
            PLCAction.axActUtlType1.ReadDeviceRandom("X30\nX34\nX38\nX3C\nX31\nX35\nX39\nX3D\nX32\nX36\n" +
                                                     "X3A\nX3E\nX33\nX37\nX3B\nX3F\n" +
                                                     "Y70\nY71\nY72\nY73\nY74\nY75\nY76\nY77\n" +
                //雷捷 24-46
                                                     "X20\nX21\nX22\nX23\nX24\nX25\nX26\nX27\nX28\nX29\n" +
                                                     "X2A\nX2B\nX2C\nX2D\nX2E\nX2F\n" +
                                                     "Y68\nY69\nY6A\nY6B\nY6C\nY6D\nY6E"
                                                     , 47, out  ReadXYval[0]);

            //Botton status
            if (_ReadVal[0] == 1)
                ManLaserReqBtn.BackColor = Color.Crimson;
            else
                ManLaserReqBtn.BackColor = Color.White;
            if (_ReadVal[1] == 1)
                ManLaserInterCntlBtn.BackColor = Color.Crimson;
            else
                ManLaserInterCntlBtn.BackColor = Color.White;
            if (_ReadVal[2] == 1)
                ManLaserGuideBtn.BackColor = Color.Crimson;
            else
                ManLaserGuideBtn.BackColor = Color.White;

            if (_ReadVal[3] == 1)
                ManLaserOnBtn.BackColor = Color.Crimson;
            else
                ManLaserOnBtn.BackColor = Color.White;
            if (_ReadVal[4] == 1)
                ManLaserProStartBtn.BackColor = Color.Crimson;
            else
                ManLaserProStartBtn.BackColor = Color.White;
            if (_ReadVal[5] == 1)
                ManLaserResetErrBtn.BackColor = Color.Crimson;
            else
                ManLaserResetErrBtn.BackColor = Color.White;

            if (_ReadVal[6] == 1)
                ManLaserAnalogCntlBtn.BackColor = Color.Crimson;
            else
                ManLaserAnalogCntlBtn.BackColor = Color.White;
            if (_ReadVal[7] == 1)
                ManLaserSCResetBtn.BackColor = Color.Crimson;
            else
                ManLaserSCResetBtn.BackColor = Color.White;

            //雷捷測試
            if (_ReadVal[8] == 1)//1205 出光
            {
                ManLaserReqBtn3.BackColor = Color.Green;
                SWILaserReqFlag = 1;
            }
            else
            {
                ManLaserReqBtn3.BackColor = Color.White;
                SWILaserReqFlag = 0;
            }


            if (_ReadVal[9] == 1)//1206 紅光
            {
                ManLaserGuideBtn3.BackColor = Color.Green;
                SWIRedLightReqFlag = 1;
            }
            else
            {
                ManLaserGuideBtn3.BackColor = Color.White;
                SWIRedLightReqFlag = 0;
            }




            //I/O Status
            if (ReadXYval[0] == 1)
                X30Lbl.BackColor = Color.Green;
            else
                X30Lbl.BackColor = Color.Bisque;
            if (ReadXYval[1] == 1)
                X34Lbl.BackColor = Color.Green;
            else
                X34Lbl.BackColor = Color.Bisque;
            if (ReadXYval[2] == 1)
                X38Lbl.BackColor = Color.Green;
            else
                X38Lbl.BackColor = Color.Bisque;
            if (ReadXYval[3] == 1)
                X3CLbl.BackColor = Color.Green;
            else
                X3CLbl.BackColor = Color.Bisque;
            if (ReadXYval[4] == 1)
                X31Lbl.BackColor = Color.Green;
            else
                X31Lbl.BackColor = Color.Bisque;
            if (ReadXYval[5] == 1)
                X35Lbl.BackColor = Color.Green;
            else
                X35Lbl.BackColor = Color.Bisque;
            if (ReadXYval[6] == 1)
                X39Lbl.BackColor = Color.Green;
            else
                X39Lbl.BackColor = Color.Bisque;
            if (ReadXYval[7] == 1)
                X3DLbl.BackColor = Color.Green;
            else
                X3DLbl.BackColor = Color.Bisque;
            if (ReadXYval[8] == 1)
                X32Lbl.BackColor = Color.Green;
            else
                X32Lbl.BackColor = Color.Bisque;
            if (ReadXYval[9] == 1)
                X36Lbl.BackColor = Color.Green;
            else
                X36Lbl.BackColor = Color.Bisque;
            if (ReadXYval[10] == 1)
                X3ALbl.BackColor = Color.Green;
            else
                X3ALbl.BackColor = Color.Bisque;
            if (ReadXYval[11] == 1)
                X3ELbl.BackColor = Color.Green;
            else
                X3ELbl.BackColor = Color.Bisque;
            if (ReadXYval[12] == 1)
                X33Lbl.BackColor = Color.Green;
            else
                X33Lbl.BackColor = Color.Bisque;
            if (ReadXYval[13] == 1)
                X37Lbl.BackColor = Color.Green;
            else
                X37Lbl.BackColor = Color.Bisque;
            if (ReadXYval[14] == 1)
                X3BLbl.BackColor = Color.Green;
            else
                X3BLbl.BackColor = Color.Bisque;
            if (ReadXYval[15] == 1)
                X3FLbl.BackColor = Color.Green;
            else
                X3FLbl.BackColor = Color.Bisque;
            if (ReadXYval[16] == 1)
                Y70Lbl.BackColor = Color.Green;
            else
                Y70Lbl.BackColor = Color.Bisque;

            if (ReadXYval[17] == 1)
                Y71Lbl.BackColor = Color.Green;
            else
                Y71Lbl.BackColor = Color.Bisque;


            if (ReadXYval[18] == 1)
                Y72Lbl.BackColor = Color.Green;
            else
                Y72Lbl.BackColor = Color.Bisque;

            if (ReadXYval[19] == 1)
                Y73Lbl.BackColor = Color.Green;
            else
                Y73Lbl.BackColor = Color.Bisque;

            if (ReadXYval[20] == 1)
                Y74Lbl.BackColor = Color.Green;
            else
                Y74Lbl.BackColor = Color.Bisque;

            if (ReadXYval[21] == 1)
                Y75Lbl.BackColor = Color.Green;
            else
                Y75Lbl.BackColor = Color.Bisque;

            if (ReadXYval[22] == 1)
                Y76Lbl.BackColor = Color.Green;
            else
                Y76Lbl.BackColor = Color.Bisque;

            if (ReadXYval[23] == 1)
                Y77Lbl.BackColor = Color.Green;
            else
                Y77Lbl.BackColor = Color.Bisque;

            X3ELbl.BackColor = Color.LimeGreen;
            X3FLbl.BackColor = Color.LimeGreen;

            //雷捷

            // "X20\nX21\nX22\nX23\nX24\nX25\nX26\nX27\nX28\nX29\n"+
            // "X2A\nX2B\nX2C\nX2D\nX2E\nX2F\n"+
            //"Y68\nY69\nY6A\nY6B\nY6C\nY6D\nY6E"

            if (ReadXYval[24] == 1)
                X20Lbl.BackColor = Color.Green;
            else
                X20Lbl.BackColor = Color.Bisque;

            if (ReadXYval[25] == 1)
                X21Lbl.BackColor = Color.Green;
            else
                X21Lbl.BackColor = Color.Bisque;

            if (ReadXYval[26] == 1)
                X22Lbl.BackColor = Color.Green;
            else
                X22Lbl.BackColor = Color.Bisque;

            if (ReadXYval[27] == 1)
                X23Lbl.BackColor = Color.Green;
            else
                X23Lbl.BackColor = Color.Bisque;

            if (ReadXYval[28] == 1)
                X24Lbl.BackColor = Color.Green;
            else
                X24Lbl.BackColor = Color.Bisque;

            if (ReadXYval[29] == 1)
                X25Lbl.BackColor = Color.Green;
            else
                X25Lbl.BackColor = Color.Bisque;

            if (ReadXYval[30] == 1)
                X26Lbl.BackColor = Color.Green;
            else
                X26Lbl.BackColor = Color.Bisque;

            if (ReadXYval[31] == 1)
                X27Lbl.BackColor = Color.Green;
            else
                X27Lbl.BackColor = Color.Bisque;

            if (ReadXYval[32] == 1)
                X28Lbl.BackColor = Color.Green;
            else
                X28Lbl.BackColor = Color.Bisque;

            if (ReadXYval[33] == 1)
                X29Lbl.BackColor = Color.Green;
            else
                X29Lbl.BackColor = Color.Bisque;

            if (ReadXYval[34] == 1)
                X2ALbl.BackColor = Color.Green;
            else
                X2ALbl.BackColor = Color.Bisque;

            if (ReadXYval[35] == 1)
                X2BLbl.BackColor = Color.Green;
            else
                X2BLbl.BackColor = Color.Bisque;

            if (ReadXYval[36] == 1)
                X2CLbl.BackColor = Color.Green;
            else
                X2CLbl.BackColor = Color.Bisque;

            if (ReadXYval[37] == 1)
                X2DLbl.BackColor = Color.Green;
            else
                X2DLbl.BackColor = Color.Bisque;

            if (ReadXYval[38] == 1)
                X2ELbl.BackColor = Color.Green;
            else
                X2ELbl.BackColor = Color.Bisque;

            if (ReadXYval[39] == 1)
                X2FLbl.BackColor = Color.Green;
            else
                X2FLbl.BackColor = Color.Bisque;


            if (ReadXYval[40] == 1)
                Y68Lbl.BackColor = Color.Green;
            else
                Y68Lbl.BackColor = Color.Bisque;

            if (ReadXYval[41] == 1)
                Y69Lbl.BackColor = Color.Green;
            else
                Y69Lbl.BackColor = Color.Bisque;

            if (ReadXYval[42] == 1)
                Y6ALbl.BackColor = Color.Green;
            else
                Y6ALbl.BackColor = Color.Bisque;

            if (ReadXYval[43] == 1)
                Y6BLbl.BackColor = Color.Green;
            else
                Y6BLbl.BackColor = Color.Bisque;

            if (ReadXYval[44] == 1)
                Y6CLbl.BackColor = Color.Green;
            else
                Y6CLbl.BackColor = Color.Bisque;

            if (ReadXYval[45] == 1)
                Y6DLbl.BackColor = Color.Green;
            else
                Y6DLbl.BackColor = Color.Bisque;

            if (ReadXYval[46] == 1)
                Y6ELbl.BackColor = Color.Green;
            else
                Y6ELbl.BackColor = Color.Bisque;



            X2ELbl.BackColor = Color.LimeGreen;
            X2FLbl.BackColor = Color.LimeGreen;

        }
        private void ManLaserReqBtn_Click(object sender, EventArgs e)
        {
            if (LaserReq == 0)
            {
                PLCAction.OnLaserReq();
                LaserReq = 1;
            }
            else
            {
                PLCAction.OffLaserReq();
                LaserReq = 0;
            }
        }

        private void ManLaserInterCntlBtn_Click(object sender, EventArgs e)
        {
            if (LaserInterCntl == 0)
            {
                PLCAction.OnLaserInterCntl();
                LaserInterCntl = 1;
            }
            else
            {
                PLCAction.OffLaserInterCntl();
                LaserInterCntl = 0;
            }
        }

        private void ManLaserGuideBtn_Click(object sender, EventArgs e)
        {
            if (LaserGuide == 0)
            {
                PLCAction.OnLaserGuide();
                LaserGuide = 1;
            }
            else
            {
                PLCAction.OffLaserGuide();
                LaserGuide = 0;
            }
        }

        private void ManLaserOnBtn_Click(object sender, EventArgs e)
        {
            if (LaserOn == 0)
            {
                PLCAction.OnLaserOn();
                LaserOn = 1;
            }
            else
            {
                PLCAction.OffLaserOn();
                LaserOn = 0;
            }
        }

        private void ManLaserProStartBtn_Click(object sender, EventArgs e)
        {
            if (LaserProStart == 0)
            {
                PLCAction.OnLaserProStart();
                LaserProStart = 1;
            }
            else
            {
                PLCAction.OffLaserProStart();
                LaserProStart = 0;
            }
        }

        private void ManLaserResetErrBtn_Click(object sender, EventArgs e)
        {
            if (LaserResetErr == 0)
            {
                PLCAction.OnLaserResetErr();
                LaserResetErr = 1;
            }
            else
            {
                PLCAction.OffLaserResetErr();
                LaserResetErr = 0;
            }
        }

        private void ManLaserAnalogCntlBtn_Click(object sender, EventArgs e)
        {
            if (LaserAnalogCntl == 0)
            {
                PLCAction.OnLaserAnalogCntl();
                LaserAnalogCntl = 1;
            }
            else
            {
                PLCAction.OffLaserAnalogCntl();
                LaserAnalogCntl = 0;
            }
        }

        private void ManLaserSCResetBtn_Click(object sender, EventArgs e)
        {
            if (LaserSCReset == 0)
            {
                PLCAction.OnLaserSCReset();
                LaserSCReset = 1;
            }
            else
            {
                PLCAction.OffLaserSCReset();
                LaserSCReset = 0;
            }
        }

        private void ManLaserReqBtn2_Click(object sender, EventArgs e)
        {
            ManLaserReqBtn_Click(null, null);
        }

        private void ManLaserOnBtn2_Click(object sender, EventArgs e)
        {
            ManLaserOnBtn_Click(null, null);
        }

        private void ManLaserAnalogCntlBtn2_Click(object sender, EventArgs e)
        {
            ManLaserAnalogCntlBtn_Click(null, null);
        }

        private void ManLaserProStartBtn2_Click(object sender, EventArgs e)
        {
            ManLaserProStartBtn_Click(null, null);
        }

        private void ManLaserGuideBtn2_Click(object sender, EventArgs e)
        {
            ManLaserGuideBtn_Click(null, null);
        }

        private void ManLaserGuideBtn3_Click(object sender, EventArgs e)
        {
            if (SWIRedLightReqFlag == 0)
            {

                PLCAction.axActUtlType1.SetDevice("M1206", 1);
      
            }
            else 
            {
                PLCAction.axActUtlType1.SetDevice("M1206", 0);
              
            }
        }

        private void ManLaserReqBtn3_Click(object sender, EventArgs e)
        {
            if (SWILaserReqFlag == 0)
            {

                PLCAction.axActUtlType1.SetDevice("M1205", 1);
          
            }
            else 
            {
                PLCAction.axActUtlType1.SetDevice("M1205", 0);
          
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }


    }
}

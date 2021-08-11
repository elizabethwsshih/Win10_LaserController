using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace PLC_LaserConrtrol
{
    public partial class PLCAction : Form
    {
        private string LaserReset_;
        public string LaserReset
        {
            set { LaserReset_ = value; }
            get { return LaserReset_; }
        }

        public PLCAction()
        {
            InitializeComponent();
        }
        public int PLC_Connect(int iLogicalStationNumber)//LogicalStationNumber for ActUtlType
        {
            int iReturnCode = 0;				//Return code

            try
            {
                //Set the value of 'LogicalStationNumber' to the property.
                axActUtlType1.ActLogicalStationNumber = iLogicalStationNumber;

                //The Open method is executed.
                iReturnCode = axActUtlType1.Open();

                //When the Open method is succeeded, make the EventHandler of ActProgType Controle.
            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message,
                            Name, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            return iReturnCode;

        }
        //public void PLC_HandShakingSet(int LaserReset_, int M1209_)
        //{
        //    //-================系統交握===============================
        //    //** 雷射重置機制,回傳值給PLC表示我還活著
        //    //** M1200 重置, M1209 傳給 PLC 訊號
        //    //** M1200 在程式起來時只需要做一次, by LaserReset flag 判段只須做一次
        //    //** PLC 將 M1209 = 0, PC端將 M1209 =1, 不斷循環

        //    if (LaserReset_ == 0)
        //    {
        //        axActUtlType1.SetDevice("M1200", 0);
        //    }
        //    else if (LaserReset_ == 1)
        //    {
               
        //        axActUtlType1.SetDevice("M1200", 1);
        //        LaserReset_ = 0;
                
        //    }
        //    if (M1209_ == 0)
        //    {
        //        axActUtlType1.SetDevice("M1209", 1);
        //    }

        
        //   // return LaserReset_;
        //}
        public int PLC_HandShakingRead()
        {
           //系統交握訊號讀回
            double[] _ReadVal1 = ReadPLCDataRandom("M1209", 1);
            int M1209_ = Convert.ToInt32(_ReadVal1[0]); 
            return M1209_;
        }


        public void OnLaserReq()
        {
            axActUtlType1.SetDevice("M1220", 1);
        }
        public void OffLaserReq()
        {
            axActUtlType1.SetDevice("M1220", 0);
        }
        public void OnLaserInterCntl()
        {
            axActUtlType1.SetDevice("M1222", 1);
        }
        public void OffLaserInterCntl()
        {
            axActUtlType1.SetDevice("M1222", 0);
        }
        public void OnLaserGuide()
        {
            axActUtlType1.SetDevice("M1224", 1);
        }
        public void OffLaserGuide()
        {
            axActUtlType1.SetDevice("M1224", 0);
        }
        public void OnLaserOn()
        {
            axActUtlType1.SetDevice("M1226", 1);
        }
        public void OffLaserOn()
        {
            axActUtlType1.SetDevice("M1226", 0);
        }
        public void OnLaserProStart()
        {
            axActUtlType1.SetDevice("M1221", 1);
        }
        public void OffLaserProStart()
        {
            axActUtlType1.SetDevice("M1221", 0);
        }
        public void OnLaserResetErr()
        {
            axActUtlType1.SetDevice("M1223", 1);
        }
        public void OffLaserResetErr()
        {
            axActUtlType1.SetDevice("M1223", 0);
        }
        public void OnLaserAnalogCntl()
        {
            axActUtlType1.SetDevice("M1225", 1);
        }
        public void OffLaserAnalogCntl()
        {
            axActUtlType1.SetDevice("M1225", 0);
        }
        public void OnLaserSCReset()
        {
            axActUtlType1.SetDevice("M1227", 1);
        }
        public void OffLaserSCReset()
        {
            axActUtlType1.SetDevice("M1227", 0);
        }
        public int Short2Int32(short argument1, short argument2)
        {
            int ValueInt32;
            byte[] byteArray1 = BitConverter.GetBytes(argument1);
            byte[] byteArray2 = BitConverter.GetBytes(argument2);
            byte[] byteArray3 = new byte[byteArray1.Length + byteArray2.Length];
            Array.Copy(byteArray1, 0, byteArray3, 0, byteArray1.Length);//合併 2 bytes+ 2 bytes
            Array.Copy(byteArray2, 0, byteArray3, byteArray1.Length, byteArray2.Length);

            ValueInt32 = BitConverter.ToInt32(byteArray3, 0);
            return ValueInt32;
        }
        public short[] Int32Short(double argument1, int ratio)
        {
            // input double=>32bits=>4 bytes=>2 short
            int arg1;
            arg1 = Convert.ToInt32(argument1 * ratio);
            short[] ValueInt16 = new short[2];
            byte[] byteArray1 = BitConverter.GetBytes(arg1);
            byte[] byteArray2 = new byte[2];
            byte[] byteArray3 = new byte[2];
            for (int i = 0; i <= 1; i++)
            {
                byteArray2[i] = byteArray1[i];
            }
            for (int i = 0; i <= 1; i++)
            {
                byteArray3[i] = byteArray1[i + 2];
            }
            ValueInt16[0] = BitConverter.ToInt16(byteArray2, 0);
            ValueInt16[1] = BitConverter.ToInt16(byteArray3, 0);
            return ValueInt16;
        }
        public double[] ReadPLCDataRandom(string DeviceName, int DataSize)//一次讀多個位置
        {

            short[] DataRead = new short[DataSize];
            int ReadValINT32;
            double ReadValSingle = 0.0;
            try
            {
                axActUtlType1.ReadDeviceRandom2(DeviceName, DataSize, out DataRead[0]);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, Name,
                                 MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //解析 DeviceName 有幾個D,幾個R, 幾個M
            Regex rD = new Regex("D");
            MatchCollection MCD;
            MCD = rD.Matches(DeviceName);

            Regex rR = new Regex("R");
            MatchCollection MCR;
            MCR = rR.Matches(DeviceName);

            double[] ReadVal = new double[((MCD.Count + MCR.Count) / 2) + (DataRead.Length - (MCD.Count + MCR.Count))];

          
            string[] SubDevices;
            SubDevices = ParsingDevice(DeviceName);
            int j = 0, k = 0;
            for (int i = 0; i < SubDevices.Length; i++)
            {
                // D or R 要一次讀兩個,兩個一組
                if (SubDevices[i].Substring(0, 1) == "D" || SubDevices[i].Substring(0, 1) == "R")
                {
                    ReadValINT32 = Short2Int32(DataRead[j], DataRead[j + 1]);
                    ReadValSingle = Convert.ToDouble(ReadValINT32) / Convert.ToDouble(10000);
                    ReadVal[k] = ReadValSingle;
                    i++;  // 因為一次讀兩個,i也要跳過增加1
                    k++;
                    j += 2;
                }
                // M 一次讀一個
                else if (SubDevices[i].Substring(0, 1) == "M")
                {
                    ReadValSingle = DataRead[j];
                    ReadVal[k] = ReadValSingle;
                    k++;
                    j++;
                }
            }
           

            return ReadVal;

        }
        public string[] ParsingDevice(string _DeviceName)
        {
            Char delimiter = '\n';
            string[] _SubDevices = _DeviceName.Split(delimiter);
            //foreach (var SubDevice in SubDevices)
            //    Console.WriteLine(SubDevice);
            return _SubDevices;
        }

        private void PLCAction_Load(object sender, EventArgs e)
        {

        }
    }
}

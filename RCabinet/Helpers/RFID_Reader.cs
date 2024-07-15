using System;
using System.Runtime.CompilerServices;
using System.Windows;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;

namespace RCabinet.Helpers
{
    public class RFID_Reader
    {
        public delegate void callBackTips(string value);

        private GClient clientConn = null;

        private eConnectionAttemptEventStatusType status;

        private callBackTips myWatch;

        private bool comIsOpened = false;

        private MsgBaseStop msgBaseStop;

        private MsgBaseInventoryEpc msgBaseInventoryEpc;

        public delegateEncapedTagEpcLog OnReading { get; set; }

        private void initBaseInventoryEpc()
        {
            msgBaseInventoryEpc = new MsgBaseInventoryEpc();
            msgBaseInventoryEpc.AntennaEnable = 15u;
            msgBaseInventoryEpc.InventoryMode = 1;
            msgBaseInventoryEpc.ReadTid = new ParamEpcReadTid();
            msgBaseInventoryEpc.ReadTid.Mode = 0;
            msgBaseInventoryEpc.ReadTid.Len = 6;
        }

        public RFID_Reader(callBackTips watch)
        {
            clientConn = new GClient();
            initBaseInventoryEpc();
            msgBaseStop = new MsgBaseStop();
            myWatch = watch;
        }

        public bool openComPort(string comPort)
        {
            if (!comIsOpened)
            {
                if (clientConn.OpenSerial(comPort + ":115200", 3000, out status))
                {
                    comIsOpened = true;
                    GClient gClient = clientConn;
                    gClient.OnEncapedTagEpcLog = (delegateEncapedTagEpcLog)Delegate.Combine(gClient.OnEncapedTagEpcLog, OnReading);
                    GClient gClient2 = clientConn;
                    gClient2.OnEncapedTagEpcOver = (delegateEncapedTagEpcOver)Delegate.Combine(gClient2.OnEncapedTagEpcOver, new delegateEncapedTagEpcOver(OnEncapedTagEpcOver));
                    return true;
                }
                else
                {
                    MessageBox.Show("Cannot Open Connect to RFID Reader, Device is in use or wrong Port  or the wrong port [" + comPort + "] is set");
                    // myWatch("Unable to connect to the specified device, the device is in use, or the wrong port [" + comPort + "] is set, please check, thank you");
                }
              
            }
            return false;
        }


        public void closeComport()
        {

            if (comIsOpened)
            {
                clientConn.Close();
                comIsOpened = false;
            }
        }
    public void startReading()
        {
            stopReading();
            InventoryEpc();
        }

        public void stopReading()
        {
            try
            {
                if (clientConn!=null && clientConn.SerialNo!=null)
                {
                    clientConn.SendSynMsg(msgBaseStop);
                    if (0 != msgBaseStop.RtCode)
                    {
                        //MessageBox.Show("An exception occurred when stopping tag reading. Please try again later or restart the program");
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void InventoryEpc()
        {
            clientConn.SendSynMsg(msgBaseInventoryEpc);
            if (0 != msgBaseInventoryEpc.RtCode)
            {
               //MessageBox.Show("An exception occurred while reading the tag. Please try again later or restart the program");
                //myWatch("An exception occurred while reading the tag. Please try again later or restart the program");

            }
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            if (msg != null && 0 != msg.logBaseEpcInfo.Result)
            {
            }
        }

        public void OnEncapedTagEpcOver(EncapedLogBaseEpcOver msg)
        {
            if (null != msg)
            {
               // myWatch("RFID Reader has stopped reading tags");
            }
        }





    }
}

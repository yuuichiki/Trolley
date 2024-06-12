using DocumentFormat.OpenXml.Spreadsheet;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCabinet.ViewModels
{
    class MappingCardViewModel : BaseViewModel
    {
        public delegate void callBackTips(string value);
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }

        private void initReader(callBackTips watch)
        {
            clientConn = new GClient();
            initBaseInventoryEpc();
            msgBaseStop = new MsgBaseStop();
            myWatch = watch;
        }

        private void initBaseInventoryEpc()
        {
            msgBaseInventoryEpc = new MsgBaseInventoryEpc();
            msgBaseInventoryEpc.AntennaEnable = 15u;
            msgBaseInventoryEpc.InventoryMode = 1;
            msgBaseInventoryEpc.ReadTid = new ParamEpcReadTid();
            msgBaseInventoryEpc.ReadTid.Mode = 0;
            msgBaseInventoryEpc.ReadTid.Len = 6;
        }

  

        public MappingCardViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
        }
    }
}

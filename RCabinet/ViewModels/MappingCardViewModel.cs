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
    public class CardModel
    {
        public string CustomerColor { get; set; }
        public int ValidQuantity { get; set; }
        public object EpCs { get; set; }
        public int Id { get; set; }
        public string StyleNo { get; set; }
        public string Mo { get; set; }
        public string ColorNo { get; set; }
        public string ColorName { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public string CardNo { get; set; }
        public bool IsActive { get; set; }
        public string WorklayerNo { get; set; }
        public string WorklayerName { get; set; }
        public string Group { get; set; }
        public string GangHao { get; set; }
    }

    class MappingCardViewModel : BaseViewModel
    {

        private List<string> comport = new List<string>();


        public delegate void callBackTips(string value);
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _customerColor { get; set; }
        private int _validQuantity { get; set; }
        private object _epCs { get; set; }
        private int _id { get; set; }
        private string _styleNo { get; set; }
        private string _mo { get; set; }
        private string _colorNo { get; set; }
        private string _colorName { get; set; }
        private string _size { get; set; }
        private int _quantity { get; set; }
        private string _cardNo { get; set; }
        private bool _isActive { get; set; }
        private string _worklayerNo { get; set; }
        private string _worklayerName { get; set; }
        private string _group { get; set; }
        private string _gangHao { get; set; }


        public string CustomerColor
        {
            get { return _customerColor; }
            set { _customerColor = value; NotifyPropertyChanged(); }
        }

        public int ValidQuantity
        {
            get { return _validQuantity; }
            set { _validQuantity = value; NotifyPropertyChanged(); }
        }
        public string ColorNo
        {
            get { return _colorNo; }
            set { _colorNo = value; NotifyPropertyChanged(); }
        }
        public string ColorName
        {
            get { return _colorName; }
            set { _colorName = value; NotifyPropertyChanged(); }
        }
        public string Size
        {
            get { return _size; }
            set { _size = value; NotifyPropertyChanged(); }
        }
        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; NotifyPropertyChanged(); }
        }
        public string CardNo
        {
            get { return _cardNo; }
            set { _cardNo = value; 
                NotifyPropertyChanged();
                LoadCardDataDetail();
            }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; NotifyPropertyChanged(); }
        }
        public string WorklayerNo
        {
            get { return _worklayerNo; }
            set { _worklayerNo = value; NotifyPropertyChanged(); }
        }
        public string WorklayerName
        {
            get { return _worklayerName; }
            set { _worklayerName = value; NotifyPropertyChanged(); }
        }
        public string Group
        {
            get { return _group; }
            set { _group = value; NotifyPropertyChanged(); }
        }
        public string GangHao
        {
            get { return _gangHao; }
            set { _gangHao = value; NotifyPropertyChanged(); }
        }



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



        public ICommand LoadCardDataCommand
        {
            get { return new RelayCommand(LoadCardDataDetail); }
        }



        private void LoadCardDataDetail()
        {

            if (CardNo == null || CardNo == "")
            {
                return;
            }
            // call http get request http://http://172.19.18.35:8103/ETSToEPC/etsCard/nike/{cardNo} to get CardData
            string url = "http://http://172.19.18.35:8103/ETSToEPC/etsCard/nike/"+ CardNo;
            //string response = HttpHelper.Get(url);
            //if (response == null)
            //{
            //    return;
            //}
            //CardData cardData = Newtonsoft.Json.JsonConvert.DeserializeObject<CardData>(response);

        }

    }
}

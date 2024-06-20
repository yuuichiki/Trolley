using DocumentFormat.OpenXml.Spreadsheet;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using Newtonsoft.Json;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using RCabinet.Models;

using System.Windows;
using ControlzEx.Standard;

namespace RCabinet.ViewModels
{
    internal class MappingCardViewModel : BaseViewModel
    {
        private List<string> comport = new List<string>();

        private readonly HttpClient _httpClient;

        public delegate void callBackTips(string value);

        private ObservableCollection<CardGridModel> _cardGridModels;
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _cardId { get; set; }

        private CardModel _card { get; set; }
        private ObservableCollection<PosModel> _pos { get; set; }
        private int totalQuantity;
        private List<string> _comPort { get; set; }

        public string CardId
        {
            get { return _cardId; }
            set
            {
                _cardId = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> ComPort
        {
            get { return _comPort; }
            set { _comPort = value; NotifyPropertyChanged(); }
        }

        public CardModel Card
        {
            get => _card;
            set
            {
                _card = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<PosModel> Pos
        {
            get => _pos;
            set
            {
                _pos = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CardGridModel> CardGridModels
        {
            get => _cardGridModels;
            set
            {
                _cardGridModels = value;
                NotifyPropertyChanged();
            }
        }

        public int TotalQuantity
        {
            get => totalQuantity;
            set
            {
                totalQuantity = value;
                NotifyPropertyChanged();
            }
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
        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }
        private void PopToMainMenu()
        {
            PopViewModel();
        }
        public MappingCardViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _httpClient = new HttpClient();
            totalQuantity = 0;
            CardGridModels = new ObservableCollection<CardGridModel>();
            Pos = new ObservableCollection<PosModel>();
            ComPort = new List<string>();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                ComPort.Add(port);
            }
            ComPort.Add("Select Comport");
        }

        public ICommand LoadCardDataCommand
        {
            get { return new RelayCommand(async () => await LoadCardDataDetail()); }
        }

        private string killZero(string value)
        {
            int num = 0;
            foreach (char c in value)
            {
                if (c == '0')
                {
                    num++;
                    continue;
                }
                break;
            }
            return value.Substring(num, value.Trim().Length - num).Trim();
        }

        private async Task LoadCardDataDetail()
        {
            if (CardId == null || CardId == "")
            {
                return;
            }
            string url = "http://172.19.18.35:8103/ETSToEPC/etsCard/nike/" + killZero(CardId.Trim());
            var response = await _httpClient.GetAsync(url);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ResponseModel>(responseData);
                Card = result.Card;
                

                var cardGridModel = new CardGridModel
                {
                    CardNo = Card.CardNo,
                    ColorNo = Card.ColorNo,
                    Size = Card.Size,
                    IsActive = Card.IsActive,
                    ValidQuantity = Card.ValidQuantity
                };
                //check if cardGridModel is already in the list
                var existingItem = CardGridModels.FirstOrDefault(x => x.CardNo == cardGridModel.CardNo);
                if (existingItem != null)
                {
                    existingItem.ValidQuantity = cardGridModel.ValidQuantity;
                }
                else
                {
                    CardGridModels.Add(cardGridModel);
                    var mPos = new ObservableCollection<PosModel>(result.Pos);
                    foreach (var pos in mPos)
                    {
                        Pos.Add(pos);
                    }

                    TotalQuantity += cardGridModel.ValidQuantity;
                  
                }
                CardId = "";
            }
            else
            {
                MessageBox.Show("Card not found", "Error!", MessageBoxButton.OK);
                CardId = "";
            }
        }
    }
}
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

using System.Windows;
using ControlzEx.Standard;
using System.ComponentModel;
using System.Windows.Controls;

namespace RCabinet.ViewModels
{
    internal class MappingCardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<string> comport = new List<string>();
        private List<string> _myPo = new List<string>();
        private readonly HttpClient _httpClient;
        private RFID_Reader reader = null;
        private bool isReading = false;
        private ObservableCollection<EPCMappingModel> _epcMapingModels;
        private ObservableCollection<CardMappingModel> _epcCardMapping;

        public delegate void callBackTips(string value);

        public event Action RequestFocusOnCardId;

        private ObservableCollection<CardGridModel> _cardGridModels;
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _cardId { get; set; }

        private POEpcModel _poEPCModels { get; set; }
        private CardModel _card { get; set; }
        private ObservableCollection<PosModel> _pos { get; set; }

        private int totalQuantity;

        private int _mappedQuantity;
        private List<string> _comPort { get; set; }
        private string _comportSelectedItem;

        private PosModel _poSelectedItem;

        private string readingStatus;
        private AsyncRelayCommand<Tuple<string, string, string, string, PosModel>> _loadComboBoxCommand;

        public ICommand LoadComboBoxCommand
        {
            get
            {
                if (_loadComboBoxCommand == null)
                {
                    _loadComboBoxCommand = new AsyncRelayCommand<Tuple<string, string,string, string, PosModel>>(LoadComboBox);
                }
                return _loadComboBoxCommand;
            }
        }

        #region Properties

        public string CardId
        {
            get { return _cardId; }
            set
            {
                _cardId = value;
                NotifyPropertyChanged();
            }
        }

        public string ReadingStatus
        {
            get { return readingStatus; }
            set
            {
                readingStatus = value;
                NotifyPropertyChanged();
            }
        }

        public POEpcModel POEPCModels
        {
            get { return _poEPCModels; }
            set
            {
                _poEPCModels = value;
                NotifyPropertyChanged();
            }
        }

        public PosModel POSelectedItem
        {
            get { return _poSelectedItem; }
            set
            {
                _poSelectedItem = value;
                NotifyPropertyChanged();
                //OnPOSelectedItemChanged();
            }
        }

        public string ComPortSelectedItem
        {
            get { return _comportSelectedItem; }
            set
            {
                _comportSelectedItem = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> ComPort
        {
            get { return _comPort; }
            set { _comPort = value; NotifyPropertyChanged(); }
        }

        public List<string> MyPO
        {
            get { return _myPo; }
            set { _myPo = value; NotifyPropertyChanged(); }
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

        public ObservableCollection<EPCMappingModel> EpcMapingModels
        {
            get => _epcMapingModels;
            set
            {
                _epcMapingModels = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CardMappingModel> CardMappingModels
        {
            get => _epcCardMapping;
            set
            {
                _epcCardMapping = value;
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

        public int MappedQuantity
        {
            get => _mappedQuantity;
            set
            {
                _mappedQuantity = value;
                NotifyPropertyChanged();
            }
        }

        #endregion Properties

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
            if (isReading)
            {
                if (reader != null)
                {
                    reader.stopReading();
                    reader.closeComport();
                    isReading = false;
                }
            }
            PopViewModel();
        }

        public MappingCardViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ReadingStatus = "Start Reading EPC";
            _httpClient = new HttpClient();
            totalQuantity = 0;
            CardGridModels = new ObservableCollection<CardGridModel>();
            EpcMapingModels = new ObservableCollection<EPCMappingModel>();
            Pos = new ObservableCollection<PosModel>();
            Pos.Add(new PosModel { Po = "Select PO", ExportDate = DateTime.Now });

            POSelectedItem = new PosModel { Po = "Select PO", ExportDate = DateTime.Now };
            ComPort = new List<string>();

            string[] ports = SerialPort.GetPortNames();
            ComPort.Add("Select Comport");

            foreach (string port in ports)
            {
                ComPort.Add(port);
            }
            ComPort.Sort();
            ComPortSelectedItem = "Select Comport";

            reader = new RFID_Reader(null);
            RFID_Reader rFID_Reader = reader;
            rFID_Reader.OnReading = (delegateEncapedTagEpcLog)Delegate.Combine(rFID_Reader.OnReading, new delegateEncapedTagEpcLog(OnEncapedTagEpcLog));

            CardMappingModels = new ObservableCollection<CardMappingModel>();
            EpcMapingModels = new ObservableCollection<EPCMappingModel>();
            MappedQuantity = 0;
            initReader();
        }

        public ICommand LoadCardDataCommand
        {
            get { return new RelayCommand(async () => await LoadCardDataDetail()); }
        }

        public ICommand StartReadingEPC
        {
            get { return new RelayCommand(async () => await ReadingEPC()); }
        }


        public ICommand ClearEPC
        {
            get { return new RelayCommand(async () => await ClearEPCGrid()); }
        }

        private async Task ClearEPCGrid()
        {
            EpcMapingModels.Clear();
            CardGridModels.Clear();
            TotalQuantity = 0;
        }

        private async Task ReadingEPC()
        {
            isReading = true;
           
            if (readingStatus == "Start Reading EPC")
            {
                if (ComPortSelectedItem != null && ComPortSelectedItem != "Select Comport")
                {

                    if (reader.openComPort(ComPortSelectedItem) == true)
                    {     
                    reader.startReading();
                    ReadingStatus = "Stop Reading EPC";
                     }
                        
                }
                else
                {
                    MessageBox.Show("Please select comport", "Error!", MessageBoxButton.OK);
                }
            }
            else if (readingStatus == "Stop Reading EPC")
            {
                if (isReading != null && isReading==true)
                {
                    reader.stopReading();
                    reader.closeComport();
                    isReading = false;

                }
                ReadingStatus = "Start Reading EPC";
            }
        }

        private async Task LoadEPCOfPO(string po, string colorNo, string size)
        {
            string url = $"http://172.19.18.35:8103/nike/epcs/dummy?po={po}&colorNo={colorNo}&size={size}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<POEpcModel>(content);
                    POEPCModels = result;
                    //EpcMapingModel.EpCs Add EPCs to EpcMapingModel with false value

                   

                    foreach (var epc in POEPCModels.EpCs)
                    {
                        //checking EpcMapingModels if epc not in EpcMapingModels then add it
                        if (!EpcMapingModels.Any((EPCMappingModel e) => e.EPC.Contains(epc)))
                            EpcMapingModels.Add(new EPCMappingModel { EPC = epc, IsMapping = false });
                    }
                }
                await RemapEpc();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task LoadCardDataDetail()
        {
            if (string.IsNullOrWhiteSpace(CardId))
            {
                return;
            }

            string url = "http://172.19.18.35:8103/ETSToEPC/etsCard/nike/" + killZero(CardId.Trim());
            var response = await _httpClient.GetAsync(url);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ResponseModel>(responseData);
                Card = result.Card;
                var cardGridModel = new CardGridModel
                {
                    MO = Card.Mo,
                    Id = Card.Id,
                    CardNo = Card.CardNo,
                    MyPO = result.Pos.ToList(),
                    CustomerColor = Card.CustomerColor,
                    GangHao = Card.GangHao,
                    ColorNo = Card.ColorNo,
                    ColorName = Card.ColorName,
                    Size = Card.Size,
                    IsActive = Card.IsActive,
                    ValidQuantity = Card.ValidQuantity,
                    POSelectedItem = result.Pos.FirstOrDefault()
                };

                if (Card.IsActive == false)
                {
                    MessageBox.Show("Card is not active", "Error!", MessageBoxButton.OK);
                    return;
                }
                var existingItem = CardGridModels.FirstOrDefault(x => x.CardNo == cardGridModel.CardNo && x.GangHao==cardGridModel.GangHao);
                if (existingItem == null)
                {
                   
                    CardGridModels.Add(cardGridModel);
                    TotalQuantity += cardGridModel.ValidQuantity;
                   
                }
                CardId = string.Empty;

            }
            else
            {
                MessageBox.Show("Card not found", "Error!", MessageBoxButton.OK);
                CardId = string.Empty;
            }
        }

        private async Task LoadComboBox(Tuple<string, string,string, string, PosModel> parameter)
        {
            if (parameter == null) return;
            string cardid = parameter.Item1;
            string ganghao = parameter.Item2;
            string colorNo = parameter.Item3;
            var size = parameter.Item4;
            PosModel pos = parameter.Item5;
            if (ganghao == null||colorNo == null || size == null || pos == null)
            {
                MessageBox.Show("Please select color, size and PO", "Error!", MessageBoxButton.OK);
            }
            _ = LoadEPCOfPO(pos.Po, colorNo, size);
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            if (!isReading)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CardMappingModels.Clear();
                });
            }
            else
            {
                if (msg == null || 0 != msg.logBaseEpcInfo.Result)
                {
                    return;
                }
                if (!CardMappingModels.Any((CardMappingModel e) => e.EPC.Contains(msg.logBaseEpcInfo.Epc)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CardMappingModels.Add(new CardMappingModel
                        {
                            EPC = msg.logBaseEpcInfo.Epc,
                            TimeCreate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                            User = CurrentUser.Username
                        });

                        if (EpcMapingModels.Any((EPCMappingModel e) => e.EPC.Contains(msg.logBaseEpcInfo.Epc)) && MappedQuantity < TotalQuantity)
                        {
                            var epc = EpcMapingModels.FirstOrDefault((EPCMappingModel e) => e.EPC.Contains(msg.logBaseEpcInfo.Epc));
                            if (epc != null)
                            {
                                epc.IsMapping = true;
                                MappedQuantity++;
                            }
                        }
                    });
                }
            }
        }


        private async Task RemapEpc()
        {
            if(MappedQuantity< TotalQuantity)
            {
                // foreach (var epc in EpcMapingModels) if is mapping ==false and epc exit in CardMappingModels  then mark ismapping =true
                foreach (var epc in EpcMapingModels)
                {
                    if (epc.IsMapping == false && CardMappingModels.Any((CardMappingModel e) => e.EPC.Contains(epc.EPC)) && MappedQuantity < TotalQuantity)
                    {
                        epc.IsMapping = true;
                        MappedQuantity++;
                    }
                }





            }
        }

        private void initReader()
        {
            clientConn = new GClient();
            initBaseInventoryEpc();
            msgBaseStop = new MsgBaseStop();
           // myWatch = watch;
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
    }
}
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using Newtonsoft.Json;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using static RCabinet.Models.ShaContext;
using System.Threading;
using DocumentFormat.OpenXml.InkML;

namespace RCabinet.ViewModels
{
    internal class MappingCardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<string> comport = new List<string>();
        private List<string> _myPo = new List<string>();
        private readonly HttpClient _httpClient;
        private RFID_Reader reader = null;
        private bool isReading = false;
        //private ObservableCollection<EPCMappingModel> _epcMapingModels;
        private ObservableCollection<TrolleyNikeEPCMapping> _trolleyNikeEPCMapping;
        private ObservableCollection<string> trashEPC;
        private CancellationTokenSource _cancellationTokenSource;
        public delegate void callBackTips(string value);
        public event Action RequestFocusOnCardId;
        private bool enableReadingEPC;
        private ObservableCollection<TrolleyNikeCard> _trolleyNikeCard;
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _cardId { get; set; }

        private POEpcModel _poEPCModels { get; set; }
        private TrolleyNikeCard _card { get; set; }
        private ObservableCollection<PosModel> _pos { get; set; }

        private int totalQuantity;
        private Guid mappingId;
        private int _mappedQuantity;
        private List<string> _comPort { get; set; }
        private string _comportSelectedItem;

        private PosModel _poSelectedItem;

        private string readingStatus;
        private int _countDown;
        private Boolean _displayEPCEnable;
        private bool _isCountingDown;
        private Dictionary<string, string> epcOfPO;
        private string mesageInfo;
        private AsyncRelayCommand<Tuple<string, string, string, string, PosModel>> _loadComboBoxCommand;
        private ZebraConfig zebraConfig;

        private ObservableCollection<TrolleyNikeEPCMapping> _trolleyNikeEPCCheckings;
        private ObservableCollection<TrolleyNikeCard> _trolleyNikeCardCheckings;


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
        public int CycleTime
        {
            get => _cycleTime;
            set
            {
                _cycleTime = value;
                NotifyPropertyChanged();
            }
        }

        private int _cycleTime = 0;

        private string selectedTab;

        private string deviceId;

        public string DeviceId
        {
            get { return deviceId; }
            set
            {
                deviceId = value;
                NotifyPropertyChanged();
            }
        }


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

        public TrolleyNikeCard Card
        {
            get => _card;
            set
            {
                _card = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TrolleyNikeEPCMapping> TrolleyNikeEPCCheckings
        {
            get
            {
                return _trolleyNikeEPCCheckings;
            }
            set
            {
                _trolleyNikeEPCCheckings = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TrolleyNikeCard> TrolleyNikeCardCheckings
        {
            get
            {
                return _trolleyNikeCardCheckings;
            }
            set
            {
                _trolleyNikeCardCheckings = value;
                NotifyPropertyChanged();
            }
        }



        private ObservableCollection<string> messageNoti
        {
            get;
            set;
        }


        public ObservableCollection<string> MessageNotify
        {
            get
            {
                return messageNoti;
            }
            set
            {
                messageNoti = value;
                NotifyPropertyChanged();
            }
        }

    
        
        public string SelectedTab
        {
            get
            {
                return selectedTab;
            }
            set
            {
                selectedTab = value;
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

        public ObservableCollection<TrolleyNikeCard> TrolleyNikeCards
        {
            get => _trolleyNikeCard;
            set
            {
                _trolleyNikeCard = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<TrolleyNikeEPCMapping> TrolleyNikeEPCMappings
        {
            get => _trolleyNikeEPCMapping;
            set
            {
                _trolleyNikeEPCMapping = value;
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
        public bool EnableReadingEPC
        {
            get
            {
                return enableReadingEPC;
            }
            set
            {
                enableReadingEPC = value;
                NotifyPropertyChanged();
            }
        }

        public int CountDown
        {
            get
            {
                return _countDown;
            }
            set
            {
                _countDown = value;
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

        public string MessageInfo
        {
            get
            {
                return mesageInfo;
            }
            set
            {
                mesageInfo = value;
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

        private void InvokeMessage(string message, string soundtype)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MessageInfo = message;
                MessageNotify.Add(MessageInfo);
                if (soundtype == "ok")
                    SoundHelper.PlaySoundOK();
                else if (soundtype == "unmatch")
                    SoundHelper.PlaySoundUnmatch();
                else
                    SoundHelper.PlaySoundError();
            });
        }


        public MappingCardViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ReadingStatus = "Start Reading EPC";
            _httpClient = new HttpClient();
            totalQuantity = 0;
            TrolleyNikeCards = new ObservableCollection<TrolleyNikeCard>();
            Pos = new ObservableCollection<PosModel>();
            Pos.Add(new PosModel { Po = "Select PO", ExportDate = DateTime.Now });
            EnableReadingEPC = false;
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
            ComPortSelectedItem = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];
            TrolleyNikeEPCMappings = new ObservableCollection<TrolleyNikeEPCMapping>();
            MappedQuantity = 0;
            epcOfPO = new Dictionary<string, string>();
            trashEPC= new ObservableCollection<string>();
            messageNoti = new ObservableCollection<string>();
            initReader();
            ComPortSelectedItem = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];
            CycleTime = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CYCLE_TIME"]);
            deviceId = System.Configuration.ConfigurationManager.AppSettings["DEVICE_ID"];
            mappingId = Guid.NewGuid();
            using (var db = new ShaContext())
            {
                zebraConfig = db.ZebraConfigs.Where(x => x.DeviceId == deviceId && x.DeviceType == "Trolley").FirstOrDefault();
                if (zebraConfig != null)
                {
                    CycleTime = zebraConfig.CycleTime;
                }
                else
                {
                    MessageBox.Show("Device not found in database");
                }
            }

            CountDown = CycleTime;


        }

        public ICommand ChangingTabCommand
        {
            get
            {
                return new RelayCommand(async () => await ChangingTabItem());
            }
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                TrolleyNikeEPCMappings.Clear();
                TrolleyNikeCards.Clear();
                TotalQuantity = 0;
                MappedQuantity = 0;
                epcOfPO = new Dictionary<string, string>();
                trashEPC = new ObservableCollection<string>();
                messageNoti = new ObservableCollection<string>();
                mappingId = Guid.NewGuid();
                reader.stopReading();
                reader.closeComport();
                ReadingStatus = "Start Reading EPC";
                isReading = false;
                messageNoti = new ObservableCollection<string>();
                EnableReadingEPC = false;
            });
        }


        private async Task ChangingTabItem()
        {
            Application.Current.Dispatcher.Invoke(() => {
                isReading = false;
                reader.stopReading();
                reader.closeComport();
                ReadingStatus = "Start Reading EPC";
                if (selectedTab == "TabMappingCard")
                {
                    Reset();
                }

            });

        }
        private async Task Reset()
        {
            await ClearEPCGrid();
            Application.Current.Dispatcher.Invoke(() => {
                MessageNotify.Clear();
                MessageNotify = new ObservableCollection<string>();
                mappingId = Guid.NewGuid();
                CountDown = CycleTime;
            });
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

                        if (SelectedTab == "TabMappingCard")
                        {
                            StartCountdown();
                        }
                    }
                    else
                    {
                        SoundHelper.PlaySoundError();
                        MessageBox.Show("Cannot Open Connect to RFID Reader, Device is in use or wrong Port  or the wrong port [" + ComPortSelectedItem + "] is set", "Error!", MessageBoxButton.OK);
                        reader.stopReading();
                    }
                }
                else
                {
                    SoundHelper.PlaySoundError();
                    MessageBox.Show("Please select comport", "Error!", MessageBoxButton.OK);
                }
            }
            else if (readingStatus == "Stop Reading EPC")
            {
                if (selectedTab == "TabMappingCard")
                    StopCountdown();

                if (isReading != null && isReading==true)
                {
                    reader.stopReading();
                    reader.closeComport();
                    isReading = false;
                    CountDown = CycleTime;

                }
                ReadingStatus = "Start Reading EPC";
            }
        }

        public void StopCountdown()
        {
            if (_isCountingDown)
            {
                _cancellationTokenSource.Cancel();
                _isCountingDown = false;
            }
        }
        private async Task MappingEPCData(string epc)
        {
            string url = $"http://172.19.18.35:8103/nike/epcs/";
            try
            {
               //check TrolleyNikeEPCMappings contain EPC= epc

                var epccheckfirst = TrolleyNikeEPCMappings.Any(t => t.EPC == epc);
                if (epccheckfirst ||trashEPC.Contains(epc))
                {
                    return;
                }

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                var jsonContent = $"[\"{epc}\"]";
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
               
                var jsonResponse = await response.Content.ReadAsStringAsync();


                    // Correctly deserialize the JSON string into the RootObject class using Newtonsoft.Json
                    var result = JsonConvert.DeserializeObject<RootObject>(jsonResponse);

                    if (result != null)
                    {
                        if(result.NikeEPCData.Count>0)
                        {
                            foreach (var item in result.NikeEPCData)
                            {

                            if (item.Color == "")
                            {
                                InvokeMessage("EPC:【" + item.EPC + "】Không thể lấy thông tin Màu sắc từ EPC", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Size == "")
                            {
                                InvokeMessage("EPC:【" + item.EPC + "】Không thể lấy thông tin Size từ EPC", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Color != Card.CustomerColor.Trim())
                            {
                                InvokeMessage("EPC:【" + item.EPC + "】Màu sắc thẻ EPC không khớp với thông tin màu sắc của thẻ hàng", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Size != Card.Size.Trim())
                            {
                                InvokeMessage("EPC:【" + item.EPC + "】Size thẻ EPC không khớp với thông tin Size của thẻ hàng", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }

                            if (item.Size==Card.Size && item.Color==Card.CustomerColor)
                                {
                                    using (var db = new ShaContext())
                                    {
                                        var cardInstance = db.TrolleyNikeEPCMappings.FirstOrDefault(e => e.EPC.Contains(epc));
                                        if (cardInstance != null)
                                        {
                                            InvokeMessage("EPC: 【" + epc + "】Đã làm liên kết thẻ, vui lòng kiểm tra lại", "error");
                                            return;
                                        }
                                    }
                                    var epcitem = TrolleyNikeEPCMappings.FirstOrDefault( x=> x.EPC == item.EPC);
                                    if (epcitem == null)
                                    {
                                    var model = new TrolleyNikeEPCMapping
                                    {
                                            Id = Guid.NewGuid(),
                                            Count = totalQuantity,
                                            EmpCode= CurrentUser.Name,
                                            EPC = item.EPC,
                                            Size = item.Size,
                                            Color = item.Color,
                                            GangHao= Card.GangHao,
                                            TimeCreated=DateTime.Now,
                                            
                                        };
                                        if(TotalQuantity> 0 && MappedQuantity < TotalQuantity)
                                        {

                                            string err = "Đã đọc số lượng thẻ:" + TotalQuantity + "Chiếc,Đã vượt quá số lượng " + Convert.ToString(TotalQuantity - MappedQuantity) + " Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn";

                                            if (MessageNotify.Contains(err) == false)
                                            {
                                                InvokeMessage(err, "error");
                                            }

                                        }    
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            TrolleyNikeEPCMappings.Add(model);
                                            trashEPC.Add(item.EPC);
                                        });
                                     TotalQuantity++;
                                    }
                                }
                                else
                                {
                                    trashEPC.Add(item.EPC);
                                    string err = "EPC:【" + epc + "】 Khác Size | Color Xin vui lòng kiểm tra lại";
                                    if (MessageNotify.Contains(err) == false)
                                    {
                                        InvokeMessage(err, "error");
                                    }
                                }
                            }
                    }
                        if (result.NoInfoEPCs.Count > 0)
                        {
                            Console.WriteLine("No Info EPCs:");
                            foreach (var _epc in result.NoInfoEPCs)
                            {
                                Console.WriteLine(_epc);
                            }
                        }
                    }

                if (TotalQuantity > 0) EnableReadingEPC = true;
                CountDown = CycleTime;

                //var content = new StringContent(JsonConvert.SerializeObject(new { epc = epc }), Encoding.UTF8, "application/json");
                //var response = await _httpClient.PostAsync(url, content);
                //if (response != null && response.IsSuccessStatusCode)
                //{
                //    var responseData = await response.Content.ReadAsStringAsync();
                //    var result = JsonConvert.DeserializeObject<ResponseModel>(responseData);
                //    Card = result.Card;
                //    var cardGridModel = new CardGridModel
                //    {
                //        MO = Card.Mo,
                //        Id = Card.Id,
                //        CardNo = Card.CardNo,
                //        //MyPO = result.Pos.ToList(),
                //        CustomerColor = Card.CustomerColor,
                //        GangHao = Card.GangHao,
                //        ColorNo = Card.ColorNo,
                //        ColorName = Card.ColorName,
                //        Size = Card.Size,
                //        IsActive = Card.IsActive,
                //        ValidQuantity = Card.ValidQuantity,
                //        POSelectedItem = result.Pos.FirstOrDefault()
                //    };

                //    if (Card.IsActive == false)
                //    {
                //        MessageBox.Show("Card is not active", "Error!", MessageBoxButton.OK);
                //        return;
                //    }
                //    var existingItem = CardGridModels.FirstOrDefault(x => x.CardNo == cardGridModel.CardNo && x.GangHao == cardGridModel.GangHao);
                //    if (existingItem == null)
                //    {
                //        CardGridModels.Add(cardGridModel);
                //        TotalQuantity += cardGridModel.ValidQuantity;
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("Card not found", "Error!", MessageBoxButton.OK);
                //}

                //await RemapEpc();
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
            Application.Current.Dispatcher.Invoke(() => {
                MessageInfo = string.Empty;
                MessageNotify.Clear();
                MessageNotify = new ObservableCollection<string>();
            });


            using (var db = new ShaContext())
            {
                var cardInstance = db.TrolleyNikeCards.FirstOrDefault(e => e.CardNo.Contains(CardId.Trim()));

                if (cardInstance != null)
                {
                    InvokeMessage("Card: 【" + CardId.Trim() + "】Đã làm liên kết thẻ, vui lòng kiểm tra lại", "error");
                    return;
                }
            }



            string url = "http://172.19.18.35:8103/ETSToEPC/etsCard/nike/" + killZero(CardId.Trim());
            var response = await _httpClient.GetAsync(url);

            if (response != null && response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ResponseModel>(responseData);

                var _card = result.Card;



                Card = new TrolleyNikeCard
                {
                    Id = Guid.NewGuid(),
                    Mo = _card.Mo,
                    MappingId = mappingId,
                    CardNo = _card.CardNo,
                    StyleNo = _card.StyleNo,
                    CustomerColor = _card.CustomerColor,
                    GangHao = _card.GangHao,
                    ColorNo = _card.ColorNo,
                    ColorName = _card.ColorName,
                    Size = _card.Size,
                    IsActive = _card.IsActive,
                    ValidQuantity = _card.ValidQuantity,
                    Quantity = _card.Quantity,
                    WorklayerNo = _card.WorklayerNo,
                    WorklayerName = _card.WorklayerName,
                    Group = _card.Group,
                    Department=_card.Department,
                    LineNo = _card.LineNo,
                    CutType = _card.CutType ?? null,
                    CutTypeName = _card.CutTypeName,
                    DateCreated = DateTime.Now

                };

                var trolleyNikeCard = Card;
                if (Card.IsActive == false)
                {
                    InvokeMessage("Card: 【" + CardId.Trim() + "】Chưa được thiết lập", "error");
                    return;
                }

                var existingGang = TrolleyNikeCards.FirstOrDefault(x => x.GangHao == trolleyNikeCard.GangHao);
                if (existingGang != null || TrolleyNikeCards.Count==0)
                {

                    if (TrolleyNikeCards.FirstOrDefault(x => x.CardNo == trolleyNikeCard.CardNo) == null)
                    {
                        TrolleyNikeCards.Add(trolleyNikeCard);
                        MappedQuantity += trolleyNikeCard.ValidQuantity.Value;
                    }

                }
                else
                {

                    InvokeMessage("Card: 【" + CardId.Trim() + "】Không cùng Gang Hao", "error");
                    return;

                }

                var existingItem = TrolleyNikeCards.FirstOrDefault(x => x.CardNo == trolleyNikeCard.CardNo && x.GangHao== trolleyNikeCard.GangHao);
                if (existingItem == null)
                {

                    TrolleyNikeCards.Add(trolleyNikeCard);
                    MappedQuantity += trolleyNikeCard.ValidQuantity.Value;
                   
                }
                CardId = string.Empty;

            }
            else
            {
                InvokeMessage("Card: 【" + CardId.Trim() + "】Không tìm thấy thông tin", "error");
                return;
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
           // _ = LoadEPCOfPO(pos.Po, colorNo, size);
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            if (isReading == true)
            {
                if (SelectedTab == "TabCheckingTag")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadEPCData(msg.logBaseEpcInfo.Epc);

                    });
                }

                else
                {
                    if (!isReading)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TrolleyNikeEPCMappings.Clear();
                        });
                    }
                    else
                    {
                        if (msg == null || 0 != msg.logBaseEpcInfo.Result)
                        {
                            return;
                        }
                        string logepc = msg.logBaseEpcInfo.Epc;
                        MappingEPCData(msg.logBaseEpcInfo.Epc);
                    }
                }
            }
        }

        private async Task LoadEPCData(string _epc)
        {
            using (var db = new ShaContext())
            {
                Application.Current.Dispatcher.Invoke(() => {
                    TrolleyNikeEPCCheckings.Clear();
                });
                TrolleyNikeEPCCheckings.Clear();
                var epc = db.TrolleyNikeEPCMappings.FirstOrDefault( e=> e.EncodeEPC.Contains(_epc));
                if (epc != null)
                {
                    var mappings = db.TrolleyNikeEPCMappings.Where( e => e.MappingId == (epc.MappingId)).ToList();
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            foreach (var item in mappings)
                            {
                                TrolleyNikeEPCCheckings.Add(item);
                            }
                            TrolleyNikeEPCCheckings.OrderBy(e => e.Count);
                        });
                    }

                    var cards = db.TrolleyNikeCards.Where(e => e.MappingId == epc.MappingId).ToList();
                    Application.Current.Dispatcher.Invoke(() => {
                        foreach (var item in cards)
                        {
                            TrolleyNikeCardCheckings.Add(item);
                        }
                    });

                    SoundHelper.PlaySoundOK();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        MessageBox.Show(_epc + " is not found", "Error!", MessageBoxButton.OK);
                        SoundHelper.PlaySoundError();
                    });
                }
            }
        }


        private async Task RemapEpc()
        {
            if(MappedQuantity< TotalQuantity)
            {
                //foreach (var epc in EpcMapingModels)
                //{
                //    if (epc.IsMapping == false && CardMappingModels.Any((CardMappingModel e) => e.EPC.Contains(epc.EPC)) && MappedQuantity < TotalQuantity)
                //    {
                //        epc.IsMapping = true;
                //        MappedQuantity++;
                //    }
                //}
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



        private async void StartCountdown()
        {
            _isCountingDown = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await Task.Run(async () =>
            {
                while (CountDown > 0 && !_cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(1000); // Wait for 1 second
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CountDown--;
                    });
                }

                if (CountDown == 0)
                {
                    // Call another function when time out
                    OnCountdownComplete();
                }

                _isCountingDown = false;

            }, token);
        }

        private async void OnCountdownComplete()
        {
            // Your logic when the countdown completes


            if (TotalQuantity == MappedQuantity)
            {

                //SaveCard();

            }

            else if (TotalQuantity > MappedQuantity)
            {
                InvokeMessage("Số lượng thẻ EPC đọc được [" + TotalQuantity + "] nhiều hơn số lượng thẻ của đơn [" + MappedQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
                await ClearEPCGrid();

            }

            else if (TotalQuantity < MappedQuantity)
            {
                InvokeMessage("Số lượng thẻ EPC đọc được [" + TotalQuantity + "] ít hơn số lượng thẻ của đơn [" + MappedQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
                await ClearEPCGrid();

            }
        }



        private async Task SaveCard()
        {
            int cardqty = 0;
            int epcqty = 0;
            int totalqty = 0;


            var addedModels = new HashSet<TrolleyNikeEPCMapping>(); // Use a HashSet to store already added models

            using (var db = new ShaContext())
            {
                foreach (var card in TrolleyNikeCards)
                {
                    epcqty = 0;
                    cardqty = Card.ValidQuantity.Value;
                    db.TrolleyNikeCards.Add(card);

                    foreach (var model in TrolleyNikeEPCMappings)
                    {
                        if (epcqty < cardqty && !addedModels.Contains(model))
                        {
                            // check db.TrolleyEPCMappings contains model.epccode
                            epcqty++;
                            model.NikeCardId = card.Id;
                            db.TrolleyNikeEPCMappings.Add(model);
                            addedModels.Add(model); // Mark the model as added
                        }
                    }
                    int changes = await db.SaveChangesAsync();
                    if (changes > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            InvokeMessage("Thẻ:【" + card.CardNo + "】Đã lưu thành công", "ok");
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            InvokeMessage("Thẻ:【" + card.CardNo + "】Không thể lưu, vui lòng thử lại", "error");
                        });
                    }
                }

                await ClearEPCGrid();

                isReading = false;
                Application.Current.Dispatcher.Invoke(() => {
                    ReadingStatus = "Start Reading EPC";
                });

            }
        }

    }
}
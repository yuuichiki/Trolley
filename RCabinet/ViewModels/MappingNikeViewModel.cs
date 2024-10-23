using DocumentFormat.OpenXml.Bibliography;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static RCabinet.Models.ShaContext;

namespace RCabinet.ViewModels
{
    internal class MappingNikeViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _comportSelectedItem;
        private AsyncRelayCommand<Tuple<string, string, string, string, PosModel>> _loadComboBoxCommand;
        private int _mappedQuantity;
        private List<string> _myPo = new List<string>();
        private PosModel _poSelectedItem;
        private int _totalCount = 0;
        private ObservableCollection<TrolleyNikeEPCMapping> _trolleyEPCChecking;
        private ObservableCollection<TrolleyNikeEPCMapping> _trolleyEPCMapping;

        private ObservableCollection<TrolleyNikeEPCMapping> _validEPCs;

        private CardKey cardkey = (CardKey)null;
        private ObservableCollection<TrolleyNikeCard> trolleyNikeCard;
        private ObservableCollection<TrolleyNikeCard> cardNikeCheckingModels;
        private CancellationTokenSource _cancellationTokenSource;
        private GClient clientConn = null;
        private List<string> comport = new List<string>();
        private int count = 1;
        private int checkCount = 0;
        private string checkingEPCTag;
        private bool enableReadingEPC;
        private bool enableSwipeCard = true;
        private string EPC_Color = "";
        private string EPC_Size = "";
        private string epc_token;
        private string epc_uri;
        private bool isReading = false;
        private bool lastChecked = false;
        private Guid mappingId;
        private string mesageInfo;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        private MsgBaseStop msgBaseStop;
        private callBackTips myWatch;
        private RFID_Reader reader = null;
        private string readingStatus;
        private CardUQModel _selectedItem;
        private bool saving = false;
        private int _cycleTime = 0;
        private string selectedTab;
        private string deviceId;
        private int _countDown;
        private bool _isCountingDown;
        private bool markError = false;
        private int totalQuantity;
        private bool enableChekingEPC;
        private ZebraConfig zebraConfig;


        public MappingNikeViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            CountDown = 5;

            ReadingStatus = "Start Reading EPC";
            _httpClient = new HttpClient();
            saving = false;
            totalQuantity = 0;
            count = 1;
            EnableReadingEPC = false;
            EnableChekingEPC = false;
            TrolleyNikeCards = new ObservableCollection<TrolleyNikeCard>();
            CardNikeCheckingModels = new ObservableCollection<TrolleyNikeCard>();
            ValidEPCs = new ObservableCollection<TrolleyNikeEPCMapping>();
            markError = false;
            mappingId = Guid.NewGuid();
            ComPort = new List<string>();
            EPC_Color = String.Empty;
            EPC_Size = String.Empty;
            string[] ports = SerialPort.GetPortNames();
            ComPort.Add("Select Comport");
            foreach (string port in ports)
            {
                ComPort.Add(port);
            }
            ComPort.Sort();
            ComPortSelectedItem = "Select Comport";

            this.cardkey = new CardKey();
            this.clearCardKey();

            reader = new RFID_Reader(null);
            RFID_Reader rFID_Reader = reader;
            rFID_Reader.OnReading = (delegateEncapedTagEpcLog)Delegate.Combine(rFID_Reader.OnReading, new delegateEncapedTagEpcLog(OnEncapedTagEpcLog));
            messageNoti = new ObservableCollection<string>();
            TrolleyNikeEPCMappings = new ObservableCollection<TrolleyNikeEPCMapping>();
            TrolleyEPCCheckings = new ObservableCollection<TrolleyNikeEPCMapping>();
            //get epc token from app.config
            epc_uri = System.Configuration.ConfigurationManager.AppSettings["API_EPC_URI"];
            epc_token = System.Configuration.ConfigurationManager.AppSettings["API_EPC_TOKEN"];
            ComPortSelectedItem = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];
            CycleTime = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CYCLE_TIME"]);
            deviceId = System.Configuration.ConfigurationManager.AppSettings["DEVICE_ID"];


            if (epc_uri == null || epc_token == null)
            {
                SoundHelper.PlaySoundError();
                MessageBox.Show("Please set EPC API URI and Token in Config", "Error!", MessageBoxButton.OK);
                return;
            }
            EnableSwipeCard = true;
            TotalCount = 0;
            CheckingEPCTag = "";
            lastEPCCheck = "";



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

        public delegate void callBackTips(string value);

        public event Action RequestFocusOnCardId;

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


        public ICommand ClearEPC
        {
            get
            {
                return new RelayCommand(async () => await Reset());
            }
        }

        public ICommand CheckEPCCommand
        {
            get
            {
                return new RelayCommand(async () => await RecheckEPCData());
            }
        }

        public ICommand CheckingTagDataCommand
        {
            get
            {
                return new RelayCommand(async () => await CheckingTagData());
            }
        }

        public ICommand ItemSelectedCommand
        {
            get
            {
                return new RelayCommand(async () => await GridItemSelected());
            }
        }


        public ICommand ChangingTabCommand
        {
            get
            {
                return new RelayCommand(async () => await ChangingTabItem());
            }
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


        private async Task GridItemSelected()
        {
            if (SelectedItem != null)
            {
                TrolleyEPCCheckings.Clear();
                using (var db = new ShaContext())
                {
                    var epcs = db.TrolleyNikeEPCMappings.Where((TrolleyNikeEPCMapping e) => e.NikeCardId == SelectedItem.Id).ToList();
                    foreach (var epc in epcs)
                    {
                        TrolleyEPCCheckings.Add(epc);
                    }
                }

            }
        }

        public ICommand GoToMainMenu
        {
            get
            {
                return new RelayCommand(PopToMainMenu);
            }
        }

        public ICommand LoadCardDataCommand
        {
            get
            {
                return new RelayCommand(async () => await LoadCardDataDetail());
            }
        }

        public delegateEncapedTagEpcLog OnReading
        {
            get;
            set;
        }

        public ICommand StartReadingEPC
        {
            get
            {
                return new RelayCommand(async () => await ReadingEPC());
            }
        }

        private string lastEPCCheck;
        private TrolleyNikeCard _card
        {
            get;
            set;
        }
        private string _cardId
        {
            get;
            set;
        }
        private List<string> _comPort
        {
            get;
            set;
        }
        private POEpcModel _poEPCModels
        {
            get;
            set;
        }
        private ObservableCollection<PosModel> _pos
        {
            get;
            set;
        }
        private ObservableCollection<string> messageNoti
        {
            get;
            set;
        }

        public CardUQModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                NotifyPropertyChanged();
            }
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

        public string CardId
        {
            get
            {
                return _cardId;
            }
            set
            {
                _cardId = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TrolleyNikeCard> TrolleyNikeCards
        {
            get => trolleyNikeCard;
            set
            {
                trolleyNikeCard = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TrolleyNikeCard> CardNikeCheckingModels
        {
            get
            {
                return cardNikeCheckingModels;
            }
            set
            {
                cardNikeCheckingModels = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> ComPort
        {
            get
            {
                return _comPort;
            }
            set
            {
                _comPort = value;
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
        public string ComPortSelectedItem
        {
            get
            {
                return _comportSelectedItem;
            }
            set
            {
                _comportSelectedItem = value;
                NotifyPropertyChanged();
            }
        }

        public string CheckingEPCTag
        {
            get => checkingEPCTag;
            set
            {
                checkingEPCTag = value;
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
        public bool EnableChekingEPC
        {
            get
            {
                return enableChekingEPC;
            }
            set
            {
                enableChekingEPC = value;
                NotifyPropertyChanged();
            }
        }
        public bool EnableSwipeCard
        {
            get
            {
                return enableSwipeCard;
            }
            set
            {
                enableSwipeCard = value;
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

        public POEpcModel POEPCModels
        {
            get
            {
                return _poEPCModels;
            }
            set
            {
                _poEPCModels = value;
                NotifyPropertyChanged();
            }
        }

        public PosModel POSelectedItem
        {
            get
            {
                return _poSelectedItem;
            }
            set
            {
                _poSelectedItem = value;
                NotifyPropertyChanged();
                //OnPOSelectedItemChanged();
            }
        }

        public string ReadingStatus
        {
            get
            {
                return readingStatus;
            }
            set
            {
                readingStatus = value;
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

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
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

        public ObservableCollection<TrolleyNikeEPCMapping> TrolleyEPCCheckings
        {
            get
            {
                return _trolleyEPCChecking;
            }
            set
            {
                _trolleyEPCChecking = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<TrolleyNikeEPCMapping> TrolleyNikeEPCMappings
        {
            get
            {
                return _trolleyEPCMapping;
            }
            set
            {
                _trolleyEPCMapping = value;
                NotifyPropertyChanged();
            }
        }
        public ObservableCollection<TrolleyNikeEPCMapping> ValidEPCs
        {
            get
            {
                return _validEPCs;
            }
            set
            {
                _validEPCs = value;
                NotifyPropertyChanged();
            }
        }
        

        #endregion Properties

        private async Task LoadEPCData1(string _epc)
        {
            using (var db = new ShaContext())
            {

                var matchingEPC = TrolleyEPCCheckings.FirstOrDefault(t => t.EPC == _epc);
                if (matchingEPC == null)
                {
                    var epc = db.TrolleyNikeEPCMappings.FirstOrDefault( e => e.EPC.Contains(_epc));
                    if (epc != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {

                            TrolleyEPCCheckings.Add(epc);
                            var card = db.TrolleyNikeCards.Where(e => e.Id == epc.NikeCardId).ToList().FirstOrDefault();
                            if (card != null)
                            {
                                // checking CardUQCheckingModels not contains card
                                if (! CardNikeCheckingModels.Any((TrolleyNikeCard e) => e.CardNo.Contains(card.CardNo)))
                                {
                                    CardNikeCheckingModels.Add(card);
                                }

                            }


                        });

                    }
                }

            }
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            if (isReading == true)
            {
                if (SelectedTab == "TabCheckingTag")
                {
                    LoadEPCData1(msg.logBaseEpcInfo.Epc);
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

 
                        if (!TrolleyNikeEPCMappings.Any(e => e.EPC.Contains(logepc)))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var model =
                                  new TrolleyNikeEPCMapping
                                  {
                                      Count = count,
                                      Id = Guid.NewGuid(),
                                      EPC = msg.logBaseEpcInfo.Epc,
                                      EncodeEPC = Utilities.EncodeData(msg.logBaseEpcInfo.Epc),
                                      MappingId = mappingId,
                                      TimeCreated = DateTime.Now,
                                      Size = "",
                                      Color ="",
                                      EmpCode = CurrentUser.Name
                                  };


                                Application.Current.Dispatcher.Invoke(() =>
                                {

                                    TrolleyNikeEPCMappings.Add(model);
                                    TotalCount = count;
                                    if (EnableChekingEPC == false) EnableChekingEPC = true;
                                    count++;
                                });


                            });
                        }



                    }
                }
            }
        }

        private static bool RemoteCertificateValidate(
          object sender,
          X509Certificate cert,
          X509Chain chain,
          SslPolicyErrors error)
        {
            return true;
        }

        private void addCardList(TrolleyNikeCard card)
        {
            var existingItem = TrolleyNikeCards.FirstOrDefault(x => x.CardNo == card.CardNo);
            if (existingItem == null)
            {
                TrolleyNikeCards.Add(card);
                TotalQuantity += card.ValidQuantity.Value;
            }
        }

        private void clearCardKey()
        {
            this.cardkey.zdcode = string.Empty;
            this.cardkey.colorno = string.Empty;
            this.cardkey.size = string.Empty;
            this.cardkey.ganghao = string.Empty;
            this.cardkey.countrycode = string.Empty;
        }

        private async Task ClearEPCMappingGrid()
        {
            Application.Current.Dispatcher.Invoke(() => {
                CardNikeCheckingModels.Clear();
                TrolleyNikeEPCMappings.Clear();
                ValidEPCs.Clear();
                EnableChekingEPC = false;
                count = 1;
                TotalCount = 0;
            });
        
        } 
        private async Task ClearEPCGrid()
        {
            Application.Current.Dispatcher.Invoke(() => {
                TrolleyNikeCards.Clear();
                CardNikeCheckingModels.Clear();
                TrolleyNikeEPCMappings.Clear();
                ValidEPCs.Clear();
                TotalQuantity = 0;
                EnableReadingEPC = false;
                count = 1;
                checkCount = 0;
                mappingId = Guid.NewGuid();
                saving = false;
                EPC_Color = string.Empty;
                EPC_Size = string.Empty;
                Card = null;
                clearCardKey();
                reader.stopReading();
                reader.closeComport();
                ReadingStatus = "Start Reading EPC";
                isReading = false;
                EnableSwipeCard = true;
                TotalCount = 0;
                messageNoti = new ObservableCollection<string>();
                markError = false;
                EnableChekingEPC = false;
            });
        }

        private async Task CheckingTagData()
        {
            //if (CheckingEPCTag == null || CheckingEPCTag == "")
            //{
            //    SoundHelper.PlaySoundError();
            //    MessageBox.Show("Please input EPC Tag", "Error!", MessageBoxButton.OK);
            //    return;
            //}
            //else
            //{
            //    CheckingEPCTag = CheckingEPCTag.Trim();
            //    LoadEPCData(CheckingEPCTag);
            //}
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

        private void initReader()
        {
            clientConn = new GClient();
            initBaseInventoryEpc();
            msgBaseStop = new MsgBaseStop();
            // myWatch = watch;
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
            Application.Current.Dispatcher.Invoke(() => {
                MessageInfo = string.Empty;
                MessageNotify.Clear();
                MessageNotify = new ObservableCollection<string>();
            });


            if (string.IsNullOrWhiteSpace(CardId))
            {
                return;
            }
            string cardid = killZero(CardId.Trim());

            using (var db = new ShaContext())
            {
                var cardInstance = db.TrolleyNikeCards.FirstOrDefault(e => e.CardNo.Contains(cardid));

                if (cardInstance != null)
                {
                    InvokeMessage("Card: 【" + CardId.Trim() + "】Đã làm liên kết thẻ, vui lòng kiểm tra lại", "error");
                    return;
                }
            }


            string url = "http://172.19.18.35:8103/ETSToEPC/etsCard/nike/" + cardid;
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
                if (existingGang != null || TrolleyNikeCards.Count == 0)
                {

                    if (TrolleyNikeCards.FirstOrDefault(x => x.CardNo == trolleyNikeCard.CardNo) == null)
                    {
                        TrolleyNikeCards.Add(trolleyNikeCard);
                        TotalQuantity += trolleyNikeCard.ValidQuantity.Value;
                    }

                }
                else
                {

                    InvokeMessage("Card: 【" + CardId.Trim() + "】Không cùng Gang Hao", "error");
                    return;

                }

                var existingItem = TrolleyNikeCards.FirstOrDefault(x => x.CardNo == trolleyNikeCard.CardNo && x.GangHao == trolleyNikeCard.GangHao);
                if (existingItem == null)
                {

                    TrolleyNikeCards.Add(trolleyNikeCard);
                    TotalQuantity += trolleyNikeCard.ValidQuantity.Value;

                }
                CardId = string.Empty;

            }
            else
            {
                InvokeMessage("Card: 【" + CardId.Trim() + "】Không tìm thấy thông tin", "error");
                return;
            }
            if(TotalQuantity>0) EnableReadingEPC = true;
            CardId = string.Empty;
        }

        private async Task LoadEPCData(string _epc)
        {
            using (var db = new ShaContext())
            {
                Application.Current.Dispatcher.Invoke(() => {
                    TrolleyEPCCheckings.Clear();
                });
                CardNikeCheckingModels.Clear();
                var epc = db.TrolleyNikeEPCMappings.FirstOrDefault((TrolleyNikeEPCMapping e) => e.EncodeEPC.Contains(_epc));
                if (epc != null)
                {
                    var mappings = db.TrolleyNikeEPCMappings.Where((TrolleyNikeEPCMapping e) => e.MappingId == (epc.MappingId)).ToList();
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            foreach (var item in mappings)
                            {
                                TrolleyEPCCheckings.Add(item);
                            }
                            TrolleyEPCCheckings.OrderBy(e => e.Count);
                        });
                    }

                    var cards = db.TrolleyNikeCards.Where( e => e.MappingId == epc.MappingId).ToList();
                    Application.Current.Dispatcher.Invoke(() => {
                        foreach (var item in cards)
                        {
                            CardNikeCheckingModels.Add(item);
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

        private void PopToMainMenu()
        {
            if (isReading)
            {
                if (reader != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        reader.stopReading();
                        reader.closeComport();
                        ReadingStatus = "Start Reading EPC";
                        isReading = false;
                    });
                }
            }
            PopViewModel();
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
                    RecheckEPCData();
                }

                _isCountingDown = false;

            }, token);
        }


        private async Task RecheckEPCData()
        {

            ValidEPCs.Clear();
            var epcList = TrolleyNikeEPCMappings.Select(t => t.EPC).ToList();
            string jsonEpcArray = JsonConvert.SerializeObject(epcList, Formatting.Indented);
            await MappingEPCData(jsonEpcArray);

            if (ValidEPCs.Count>0 && ValidEPCs.Count == TotalQuantity)
            {
                await SaveCard();
            }
           
            else if (ValidEPCs.Count > TotalQuantity)
            {
                InvokeMessage("Số lượng thẻ EPC hợp lệ đọc được [" + ValidEPCs.Count + "] nhiều hơn số lượng thẻ của đơn ["+ TotalQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
                await ClearEPCMappingGrid();
            }

            else if (ValidEPCs.Count < TotalQuantity)
            {
                InvokeMessage("Số lượng thẻ EPC hợp lệ đọc được [" + ValidEPCs.Count + "] ít hơn số lượng thẻ của đơn [" + TotalQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
            }
        }



        private async Task MappingEPCData(string epc)
        {
            string url = $"http://172.19.18.35:8103/nike/epcs/";
            try
            {
                var epccheckfirst = TrolleyNikeEPCMappings.Any(t => t.EPC == epc);
                //if (epccheckfirst || trashEPC.Contains(epc))
                //{
                //    return;
                //}

                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                var jsonContent = $"{epc}";
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();


                // Correctly deserialize the JSON string into the RootObject class using Newtonsoft.Json
                var result = JsonConvert.DeserializeObject<RootObject>(jsonResponse);
                int count = 1;
                if (result != null)
                {
                    if (result.NikeEPCData.Count > 0)
                    {
                        foreach (var item in result.NikeEPCData)
                        {

                            if (item.Color == "")
                            {
                                InvokeMessage("EPC:【" + Helpers.Utilities.EncodeData(item.EPC) + "】Không thể lấy thông tin Màu sắc từ EPC", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Size == "")
                            {
                                InvokeMessage("EPC:【" + Helpers.Utilities.EncodeData(item.EPC) + "】Không thể lấy thông tin Size từ EPC", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Color != Card.CustomerColor.Trim())
                            {
                                InvokeMessage("EPC:【" + Helpers.Utilities.EncodeData(item.EPC) + "】Màu sắc thẻ EPC không khớp với thông tin màu sắc của thẻ hàng", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }
                            if (item.Size != Card.Size.Trim())
                            {
                                InvokeMessage("EPC:【" + Helpers.Utilities.EncodeData(item.EPC) + "】Size thẻ EPC không khớp với thông tin Size của thẻ hàng", "error");
                                SoundHelper.PlaySoundError();
                                return;
                            }

                            if (item.Size == Card.Size && item.Color == Card.CustomerColor)
                            {
                                using (var db = new ShaContext())
                                {
                                    var cardInstance = db.TrolleyNikeEPCMappings.FirstOrDefault(e => e.EPC.Contains(epc));
                                    if (cardInstance != null)
                                    {
                                        InvokeMessage("EPC: 【" + Helpers.Utilities.EncodeData(epc) + "】Đã làm liên kết thẻ, vui lòng kiểm tra lại", "error");
                                        return;
                                    }
                                }
                                var epcitem = ValidEPCs.FirstOrDefault(x => x.EPC == item.EPC);
                                if (epcitem == null)
                                {
                                    var model = new TrolleyNikeEPCMapping
                                    {
                                        Id = Guid.NewGuid(),
                                        Count = count,
                                        EmpCode = CurrentUser.Name,
                                        EPC = item.EPC,
                                        EncodeEPC=Helpers.Utilities.EncodeData(item.EPC),
                                        Size = item.Size,
                                        Color = item.Color,
                                        GangHao = Card.GangHao,
                                        TimeCreated = DateTime.Now,
                                        MappingId= mappingId,
                                    };
                       
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        ValidEPCs.Add(model);
                                        count++;
                                    });
                                }
                            }
                            else
                            {
                                string err = "EPC:【" + Helpers.Utilities.EncodeData(epc) + "】 Khác Size | Color Xin vui lòng kiểm tra lại";
                                if (MessageNotify.Contains(err) == false)
                                {
                                    InvokeMessage(err, "error");
                                }
                            }
                        }
                    }
                    if (result.NoInfoEPCs.Count > 0)
                    {

                        foreach (var _epc in result.NoInfoEPCs)
                        {
                            InvokeMessage("EPC:【" + Helpers.Utilities.EncodeData(_epc) + "】Không thể lấy được thông tin từ hệ thống", "error");
                            SoundHelper.PlaySoundError();
                            return;
                        }
                    }
                }

                if (TotalQuantity > 0) EnableReadingEPC = true;
                CountDown = CycleTime;

            }
            catch (Exception ex)
            {

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
                        EnableSwipeCard = false;
                        ReadingStatus = "Stop Reading EPC";
                        
                   
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
                if(selectedTab== "TabMappingCard")
                     //StopCountdown();
                if (isReading != null && isReading == true)
                {
                    // reader.stopReading();
                    reader.closeComport();
                    isReading = false;
                    CountDown = CycleTime;
                }
                ReadingStatus = "Start Reading EPC";
            }
        }

        private async Task Reset()
        {
            await ClearEPCGrid();
            Application.Current.Dispatcher.Invoke(() => {
                MessageNotify.Clear();
                MessageNotify = new ObservableCollection<string>();
                markError = false;
            });
        }

        private async Task SaveCard()
        {
            saving = true;
            int cardqty = 0;
            int epcqty = 0;
            int totalqty = 0;

            if(ValidEPCs.Count == 0)
            {
                InvokeMessage("Không có dữ liệu thẻ EPC để lưu", "error");
                return;
            }

            foreach (var card in TrolleyNikeCards)
            {
                totalqty += card.ValidQuantity.Value;
            }


            if (totalqty != ValidEPCs.Count)
            {
                InvokeMessage("Số lượng trên thẻ và số lượng EPC hợp lệ không khớp nhau, xin vui lòng điều chỉnh lại", "unmatch");
                await ClearEPCGrid();
                return;
            }

            var addedModels = new HashSet<TrolleyNikeEPCMapping>(); // Use a HashSet to store already added models

            bool error = false;
            foreach (var model in ValidEPCs)
            {

                    using (var db = new ShaContext())
                    {
                        var epc = db.TrolleyNikeEPCMappings.FirstOrDefault((TrolleyNikeEPCMapping e) => e.EPC.Contains(model.EPC));
                        if (epc != null)
                        {
                            //check if MessageNotify contains
                            string err = "EPC:【" + Utilities.EncodeData(model.EPC) + "】 đã làm liên kết";
                            if (MessageNotify.Contains(err) == false)
                            {
                                InvokeMessage(err, "error");
                               
                            }
                            error = true;
                        }
                        else
                        {


                            if (ValidEPCs.Count > TotalQuantity)
                            {
                                InvokeMessage("Đã đọc số lượng thẻ hợp lệ :" + ValidEPCs.Count.ToString() + "Chiếc,Đã vượt quá số lượng " + Convert.ToString(ValidEPCs.Count - Convert.ToInt32(TotalQuantity)) + " Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn", "error");
                                 error = true;
                            }
                            

                        }
                    }

            }

            if (error == true)
            {
                await ClearEPCMappingGrid();
                return;
            }


            using (var db = new ShaContext())
            {
                foreach (var card in TrolleyNikeCards)
                {
                    epcqty = 0;
                    cardqty = card.ValidQuantity.Value;
                    card.DateCreated=DateTime.Now;
                    db.TrolleyNikeCards.Add(card);

                    foreach (var model in ValidEPCs)
                    {
                        if (epcqty < cardqty && !addedModels.Contains(model))
                        {
                            // check db.TrolleyNikeEPCMappings contains model.epccode
                            epcqty++;
                            model.NikeCardId = card.Id;
                            model.TimeCreated=DateTime.Now;
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

                saving = false;
                EnableSwipeCard = true;
            }
        }

    }
}
﻿using DocumentFormat.OpenXml.Bibliography;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
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
using System.Windows.Interop;
using static RCabinet.Models.ShaContext;

namespace RCabinet.ViewModels
{
    internal class MappingUQViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _comportSelectedItem;
        private AsyncRelayCommand<Tuple<string, string, string, string, PosModel>> _loadComboBoxCommand;
        private int _mappedQuantity;
        private List<string> _myPo = new List<string>();
        private PosModel _poSelectedItem;
        private int _totalCount = 0;
        private ObservableCollection<TrolleyEPCMapping> _trolleyEPCChecking;
        private ObservableCollection<TrolleyEPCMapping> _trolleyEPCMapping;
        private CardKey cardkey = (CardKey)null;
        private ObservableCollection<CardUQModel> cardUQModels;
        private ObservableCollection<CardUQModel> cardUQCheckingModels;
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
        private string Country = "";
        private string SAPStyle = "";
        private string CustName = "";
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
        private string _department;
        private string _lineno;
        private string _sapstyle;
        private string _custname;
        public class RFID
        {
            public string EPC { get; set; }
            public string SKU { get; set; }
            public string PO { get; set; }
            public string DO { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
            public string SampleCode { get; set; }
            public string Country { get; set; }
        }

        public class RFIDOutput
        {
            public string EPC { get; set; }
            public string SKU { get; set; }
            public string PO { get; set; }
            public string DO { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
            public string Country { get; set; }
        }



        public MappingUQViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ReadingStatus = "Start Reading EPC";
            saving = false;
            totalQuantity = 0;
            count = 1;
            EnableReadingEPC = false;
            EnableChekingEPC = false;
            CardUQModels = new ObservableCollection<CardUQModel>();
            CardUQCheckingModels = new ObservableCollection<CardUQModel>();
            markError = false;
            mappingId = Guid.NewGuid();
            ComPort = new List<string>();
            EPC_Color = String.Empty;
            EPC_Size = String.Empty;
            SAPStyle = String.Empty;
            CustName = String.Empty;
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
            TrolleyEPCMappings = new ObservableCollection<TrolleyEPCMapping>();
            TrolleyEPCCheckings = new ObservableCollection<TrolleyEPCMapping>();
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
                if (selectedTab == "TabCheckingTag")
                {
                    CardUQCheckingModels.Clear();
                    TrolleyEPCCheckings.Clear();
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
                    var epcs = db.TrolleyEPCMappings.Where((TrolleyEPCMapping e) => e.UQCardId == SelectedItem.Id).ToList();
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
        private CardUQModel _card
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
        public CardUQModel Card
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
        public string Department
        {
            get
            {
                return _department;
            }
            set
            {
                _department = value;
                NotifyPropertyChanged();
            }
        }

        public string LineNo
        {
            get
            {
                return _lineno;
            }
            set
            {
                _lineno = value;
                NotifyPropertyChanged();
            }
        }


        public ObservableCollection<CardUQModel> CardUQModels
        {
            get
            {
                return cardUQModels;
            }
            set
            {
                cardUQModels = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CardUQModel> CardUQCheckingModels
        {
            get
            {
                return cardUQCheckingModels;
            }
            set
            {
                cardUQCheckingModels = value;
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

        public ObservableCollection<TrolleyEPCMapping> TrolleyEPCCheckings
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

        public ObservableCollection<TrolleyEPCMapping> TrolleyEPCMappings
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

        #endregion Properties

        private async Task LoadEPCData1(string _epc)
        {
            using (var db = new ShaContext())
            {
                //Application.Current.Dispatcher.Invoke(() => {
                //    TrolleyEPCCheckings.Clear();
                //});
                // CardUQCheckingModels.Clear();


                var matchingEPC = TrolleyEPCCheckings.FirstOrDefault(t => t.EPC == _epc);
                if (matchingEPC == null)
                {
                    var epc = db.TrolleyEPCMappings.FirstOrDefault((TrolleyEPCMapping e) => e.EPC.Contains(_epc));
                    if (epc != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {

                            TrolleyEPCCheckings.Add(epc);


                            var card = db.TrolleyUQCards.Where((CardUQModel e) => e.Id == epc.UQCardId).ToList().FirstOrDefault();
                            if (card != null)
                            {
                                // checking CardUQCheckingModels not contains card
                                if (!CardUQCheckingModels.Any((CardUQModel e) => e.CardNo.Contains(card.CardNo)))
                                {
                                    CardUQCheckingModels.Add(card);
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
                            TrolleyEPCMappings.Clear();
                        });
                    }
                    else
                    {
                        if (msg == null || 0 != msg.logBaseEpcInfo.Result)
                        {
                            return;
                        }
                        string logepc = msg.logBaseEpcInfo.Epc;

 
                        if (!TrolleyEPCMappings.Any(e => e.EPC.Contains(logepc)))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var model =
                                  new TrolleyEPCMapping
                                  {
                                      Count = count,
                                      Id = Guid.NewGuid(),
                                      EPC = msg.logBaseEpcInfo.Epc,
                                      EncodeEPC = Utilities.EncodeData(msg.logBaseEpcInfo.Epc),
                                      MappingId = mappingId,
                                      TimeCreated = DateTime.Now,
                                      SKU = "",
                                      CountryCode="",
                                      Size = Card.Size,
                                      Color = Card.ColorNo,
                                      EmpCode = CurrentUser.Name
                                  };


                                Application.Current.Dispatcher.Invoke(() =>
                                {

                                    TrolleyEPCMappings.Add(model);
                                    TotalCount = count;
                                    if (EnableChekingEPC == false) EnableChekingEPC = true;
                                    count++;
                                });

                                //var epcModel = updateEPCData(model).Result;
                                //if (epcModel != null)
                                //{
                                //    // check epcModel.Color ==null || epc.Color =""
                                //    if (epcModel.Color == "")
                                //    {
                                //        InvokeMessage("EPC:【" + epcModel.EPC + "】Không thể lấy thông tin Màu sắc từ EPC", "error");
                                //        SoundHelper.PlaySoundError();
                                //        return;
                                //    }
                                //    if (epcModel.Size == "")
                                //    {
                                //        InvokeMessage("EPC:【" + epcModel.EPC + "】Không thể lấy thông tin Size từ EPC", "error");
                                //        SoundHelper.PlaySoundError();
                                //        return;
                                //    }
                                //    if (epcModel.Color != EPC_Color.Trim())
                                //    {
                                //        InvokeMessage("EPC:【" + epcModel.EPC + "】Màu sắc thẻ EPC không khớp với thông tin màu sắc của thẻ hàng", "error");
                                //        SoundHelper.PlaySoundError();
                                //        return;
                                //    }
                                //    if (epcModel.Size != EPC_Size.Trim())
                                //    {
                                //        InvokeMessage("EPC:【" + epcModel.EPC + "】Size thẻ EPC không khớp với thông tin Size của thẻ hàng", "error");
                                //        SoundHelper.PlaySoundError();
                                //        return;
                                //    }

                                //    using (var db = new ShaContext())
                                //    {
                                //        var epc = db.TrolleyEPCMappings.FirstOrDefault((TrolleyEPCMapping e) => e.EPC.Contains(model.EPC));
                                //        if (epc != null)
                                //        {
                                //            //check if MessageNotify contains
                                //            string err = "EPC:【" + model.EPC + "】 đã làm liên kết";
                                //            if (MessageNotify.Contains(err) == false)
                                //            {
                                //                InvokeMessage(err, "error");

                                //            }
                                //        }
                                //        else
                                //        {


                                //            if (TrolleyEPCMappings.Count > TotalQuantity)
                                //            {
                                //                //SoundHelper.PlaySoundUnmatch();
                                //                InvokeMessage("Đã đọc số lượng thẻ:" + this.TrolleyEPCMappings.Count.ToString() + "Chiếc,Đã vượt quá số lượng " + Convert.ToString(TrolleyEPCMappings.Count - Convert.ToInt32(TotalQuantity)) + " Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn", "error");
                                //            }


                                //        }
                                //    }
                                //}

                                //if (TrolleyEPCMappings.Count == TotalQuantity)
                                //{
                                //    checkCount++;
                                //    if (checkCount > 2 && lastChecked)
                                //    {
                                //        //SaveCard();
                                //        // MessageBox.Show("Save Card");
                                //    }
                                //}
                                //if (!lastChecked && checkCount == 2)
                                //{
                                //    Application.Current.Dispatcher.Invoke(() =>
                                //    {
                                //        TrolleyEPCMappings.Clear();
                                //        count = 1;

                                //    });
                                //    lastChecked = true;
                                //}


                                //if (TrolleyEPCMappings.Count == TotalQuantity && checkCount > 2 && lastChecked)
                                //{
                                //    //SaveCard();
                                //    MessageBox.Show("Save Card");

                                //}


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

        private void addCardList(CardUQModel card)
        {
            var existingItem = CardUQModels.FirstOrDefault(x => x.CardNo == card.CardNo);
            if (existingItem == null)
            {
                CardUQModels.Add(card);
                TotalQuantity += card.AdjustQuantity;
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
                CardUQCheckingModels.Clear();
                TrolleyEPCMappings.Clear();
                EnableChekingEPC = false;
                count = 1;
                TotalCount = 0;
            });
        
        } 
        private async Task ClearEPCGrid()
        {
            Application.Current.Dispatcher.Invoke(() => {
                CardUQModels.Clear();
                CardUQCheckingModels.Clear();
                TrolleyEPCMappings.Clear();
                TotalQuantity = 0;
                EnableReadingEPC = false;
                count = 1;
                checkCount = 0;
                mappingId = Guid.NewGuid();
                saving = false;
                EPC_Color = string.Empty;
                EPC_Size = string.Empty;
                SAPStyle = string.Empty;
                CustName = string.Empty;
                LineNo = string.Empty;
                Department = string.Empty;
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

            // lấy thông tin chi tiết của thẻ từ database
            string cardid = killZero(CardId.Trim());
            DataSet dataSet = SqlHelper.getDataSet("exec [P_RFIDReader_GetDataByCardNO_UQ] " + SqlHelper.quotedStr(cardid),"ets");

            if (dataSet.Tables[0].Rows.Count < 1)
            {
                InvokeMessage("ID: 【" + cardid + "】Thẻ không hợp lệ hoặc không có ID này, vui lòng kiểm tra lại", "error");
                CardId = string.Empty;
                return;
            }
            if (this.cardkey.zdcode != string.Empty && (dataSet.Tables[0].Rows[0]["ZDcode"].ToString().Trim() != this.cardkey.zdcode || this.cardkey.colorno != dataSet.Tables[0].Rows[0]["ColorNo"].ToString().Trim() || this.cardkey.size != dataSet.Tables[0].Rows[0]["sSize"].ToString().Trim() || this.cardkey.ganghao != dataSet.Tables[0].Rows[0]["GangHao"].ToString().Trim() || this.cardkey.countrycode != dataSet.Tables[0].Rows[0]["Country"].ToString().Trim()))
            {
                InvokeMessage("ID:【" + cardid + "】Đơn đặt hàng, màu sắc, kích thước, số gang hao, Mã quốc gia đến có thể khác với số thẻ đã đọc, hoạt động này không hợp lệ", "unmatch");
                CardId = string.Empty;
                return;
            }
            if (dataSet.Tables[0].Rows[0]["SaleNo"] == "" || dataSet.Tables[0].Rows[0]["SoItem"] == "")
            {
                InvokeMessage("ID:【" + cardid + "】Không thể lấy số mặt hàng và số đơn hàng bán hàng tương ứng với đơn đặt hàng", "error");
                CardId = string.Empty;
                return;
            }

            using (var db = new ShaContext())
            {
                var cardInstance = db.TrolleyUQCards.FirstOrDefault((CardUQModel e) => e.CardNo.Equals(cardid));

                if (cardInstance != null)
                {
                    InvokeMessage("Thẻ: 【" + cardid + "】Đã làm liên kết thẻ, vui lòng kiểm tra lại", "error");
                    CardId = string.Empty;
                    return;
                }
            }

            //Load dataSet to CardUqModel
            Card = new CardUQModel
            {
                Id = Guid.NewGuid(),
                StyleNo = dataSet.Tables[0].Rows[0]["StyleNO"].ToString().Split('_')[0],
                MappingId = mappingId,
                SO = dataSet.Tables[0].Rows[0]["SO"].ToString(),
                Mo = killZero(dataSet.Tables[0].Rows[0]["ZDcode"].ToString()),
                FeatureName = dataSet.Tables[0].Rows[0]["FeatureName"].ToString(),
                ColorNo = dataSet.Tables[0].Rows[0]["ColorNo"].ToString(),
                ColorName = dataSet.Tables[0].Rows[0]["ColorName"].ToString(),
                Size = dataSet.Tables[0].Rows[0]["sSize"].ToString(),
                CutQuantity = Convert.ToInt32(dataSet.Tables[0].Rows[0]["CutQty"].ToString()),
                BundleQuantity = Convert.ToInt32(dataSet.Tables[0].Rows[0]["cCount"].ToString()),
                BundleNo = Convert.ToInt32(dataSet.Tables[0].Rows[0]["GroupNO"].ToString()),
                Country = dataSet.Tables[0].Rows[0]["Country"].ToString(),
                AdjustQuantity = Convert.ToInt32(dataSet.Tables[0].Rows[0]["cCount"].ToString()),
                GangHao = dataSet.Tables[0].Rows[0]["GangHao"].ToString(),
                CardNo = killZero(dataSet.Tables[0].Rows[0]["CardNo"].ToString()),
                SoItem = killZero(dataSet.Tables[0].Rows[0]["SoItem"].ToString()),
                SaleNo = dataSet.Tables[0].Rows[0]["SaleNo"].ToString(),
                Department = dataSet.Tables[0].Rows[0]["Department"].ToString(),
                LineNo = dataSet.Tables[0].Rows[0]["LineNo"].ToString(),
                
                DateCreated = DateTime.Now
            };

            DataSet dsEPCcolorsize = SqlHelper.getDataSet("exec P_Hanlde_RFIDReadert_GetCustColorSize @sapstyle= " + SqlHelper.quotedStr(Card.StyleNo)+ ",@color= "+ SqlHelper.quotedStr(Card.ColorNo) +" ,@size= "+ SqlHelper.quotedStr(Card.Size) , "sha");
            if (dsEPCcolorsize.Tables[0].Rows.Count < 1)
            {
                InvokeMessage("ID:【" + Card.CardNo + "】 Thông tin Mapping thẻ hàng ETS và [Size; Màu] SKU của EPC chưa được upload lên ORP", "error");
                SoundHelper.PlaySoundError();
                return;
            }
            else
            {
                EPC_Color = dsEPCcolorsize.Tables[0].Rows[0]["CustColorNo"].ToString();
                EPC_Size = dsEPCcolorsize.Tables[0].Rows[0]["CustSize"].ToString();
                SAPStyle = dsEPCcolorsize.Tables[0].Rows[0]["SAPStyle"].ToString();
                CustName = dsEPCcolorsize.Tables[0].Rows[0]["CustName"].ToString();
                Card.CustName = CustName;
                Card.SAPStyle = SAPStyle;
            }

            if (this.cardkey.zdcode == string.Empty)
            {
                this.cardkey.zdcode = dataSet.Tables[0].Rows[0]["ZDcode"].ToString();
                this.cardkey.colorno = dataSet.Tables[0].Rows[0]["ColorNo"].ToString();
                this.cardkey.size = dataSet.Tables[0].Rows[0]["sSize"].ToString();
                this.cardkey.ganghao = dataSet.Tables[0].Rows[0]["GangHao"].ToString();
                this.cardkey.countrycode = dataSet.Tables[0].Rows[0]["Country"].ToString();
            }

            if (Convert.ToInt32(dataSet.Tables[0].Rows[0]["cCount"].ToString()) != 0)
            {
                addCardList(Card);
               
            }
            if (TotalQuantity > 0)   EnableReadingEPC = true;


            CountDown = CycleTime;
            CardId = string.Empty;
        }

        private async Task LoadEPCData(string _epc)
        {
            using (var db = new ShaContext())
            {
                Application.Current.Dispatcher.Invoke(() => {
                    TrolleyEPCCheckings.Clear();
                });
                CardUQCheckingModels.Clear();
                var epc = db.TrolleyEPCMappings.FirstOrDefault((TrolleyEPCMapping e) => e.EncodeEPC.Contains(_epc));
                if (epc != null)
                {
                    var mappings = db.TrolleyEPCMappings.Where((TrolleyEPCMapping e) => e.MappingId == (epc.MappingId)).ToList();
                    {
                        Application.Current.Dispatcher.Invoke(() => {
                            foreach (var item in mappings)
                            {
                                TrolleyEPCCheckings.Add(item);
                            }
                            TrolleyEPCCheckings.OrderBy(e => e.Count);
                        });
                    }

                    var cards = db.TrolleyUQCards.Where((CardUQModel e) => e.MappingId == epc.MappingId).ToList();
                    Application.Current.Dispatcher.Invoke(() => {
                        foreach (var item in cards)
                        {
                            CardUQCheckingModels.Add(item);
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
            // Your logic when the countdown completes


            if (TrolleyEPCMappings.Count>0 && TrolleyEPCMappings.Count == TotalQuantity)
            {
                SaveCard();
                
            }
           
            else if (TrolleyEPCMappings.Count > TotalQuantity)
            {
                //SaveCard();
                InvokeMessage("Số lượng thẻ EPC đọc được ["+ TrolleyEPCMappings.Count + "] nhiều hơn số lượng thẻ của đơn ["+ TotalQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
                await ClearEPCMappingGrid();

            }

            else if (TrolleyEPCMappings.Count < TotalQuantity)
            {
                //SaveCard();
                InvokeMessage("Số lượng thẻ EPC đọc được [" + TrolleyEPCMappings.Count + "] ít hơn số lượng thẻ của đơn [" + TotalQuantity + "] !. Xin vui lòng kiểm tra lại", "error");
      

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

          

            if (markError == true)
            {
                Reset();
            }

            foreach (var card in CardUQModels)
            {
                totalqty += card.AdjustQuantity;
            }

            if (totalqty != TrolleyEPCMappings.Count)
            {
                InvokeMessage("Số lượng trên thẻ và số lượng EPC đọc được không khớp nhau, xin vui lòng điều chỉnh lại", "unmatch");
                await ClearEPCGrid();
                return;
            }

            var addedModels = new HashSet<TrolleyEPCMapping>(); // Use a HashSet to store already added models

            bool error = false;
            // foreach (var model in TrolleyEPCMappings)
            // {
            var cardCountry = CardUQModels.FirstOrDefault().Country.Trim();
            var epcModels = updateEPCData(TrolleyEPCMappings).Result;
                if (epcModels != null)
                foreach(var epcModel in epcModels)
                {
                    // check epcModel.Color ==null || epc.Color =""
                    if (epcModel.Color == "")
                    {
                        InvokeMessage("EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】Không thể lấy thông tin Màu sắc từ EPC", "error");
                        SoundHelper.PlaySoundError();
                        error = true;
                        return;
                    }
                    if (epcModel.Size == "")
                    {
                        InvokeMessage("EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】Không thể lấy thông tin Size từ EPC", "error");
                        SoundHelper.PlaySoundError();
                        error = true;
                        return;
                    }
                    if (epcModel.Color != EPC_Color.Trim())
                    {
                        InvokeMessage("EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】Màu sắc thẻ EPC không khớp với thông tin màu sắc của thẻ hàng", "error");
                        error = true;
                        SoundHelper.PlaySoundError();
                        return;
                    }
                    if (epcModel.Size != EPC_Size.Trim())
                    {
                        InvokeMessage("EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】Size thẻ EPC không khớp với thông tin Size của thẻ hàng", "error");
                        error = true;
                        SoundHelper.PlaySoundError();
                        return;
                    }
     
                    if (epcModel.Country.Trim() != cardCountry)
                    {
                        InvokeMessage("EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】Mã Country thẻ EPC ["+ epcModel.Country.Trim() + "] không khớp với mã Country ["+ cardCountry + "] của thẻ hàng", "error");
                        error = true;
                        SoundHelper.PlaySoundError();
                        return;
                    }


                    using (var db = new ShaContext())
                    {
                       // var epc = db.TrolleyEPCMappings.FirstOrDefault((TrolleyEPCMapping e) => e.EPC.Contains(model.EPC));
                        //if (epc != null)
                        //{
                        //    //check if MessageNotify contains
                        //    string err = "EPC:【" + Utilities.EncodeData(epcModel.EPC) + "】 đã làm liên kết";
                        //    if (MessageNotify.Contains(err) == false)
                        //    {
                        //        InvokeMessage(err, "error");

                        //    }
                        //    error = true;
                        //}
                        //else
                        //{


                        //    if (TrolleyEPCMappings.Count > TotalQuantity)
                        //    {
                        //        //SoundHelper.PlaySoundUnmatch();
                        //        InvokeMessage("Đã đọc số lượng thẻ:" + this.TrolleyEPCMappings.Count.ToString() + "Chiếc,Đã vượt quá số lượng " + Convert.ToString(TrolleyEPCMappings.Count - Convert.ToInt32(TotalQuantity)) + " Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn", "error");
                        //        error = true;
                        //    }
                            

                        //}
                    }
                }

           //}

            if (error == true)
            {
                await ClearEPCMappingGrid();
                return;
            }


            using (var db = new ShaContext())
            {
                foreach (var card in CardUQModels)
                {
                    epcqty = 0;
                    cardqty = card.AdjustQuantity;
                    card.EPCQuantity = TrolleyEPCMappings.Count;
                    card.DeviceName=deviceId;
                    card.DateCreated=DateTime.Now;
                    db.TrolleyUQCards.Add(card);

                    foreach (var model in TrolleyEPCMappings)
                    {
                        if (epcqty < cardqty && !addedModels.Contains(model))
                        {
                            // check db.TrolleyEPCMappings contains model.epccode
                            epcqty++;
                            model.UQCardId = card.Id;
                            model.TimeCreated=DateTime.Now;
                            db.TrolleyEPCMappings.Add(model);
                            addedModels.Add(model); // Mark the model as added
                        }
                    }
                    int changes = await db.SaveChangesAsync();
                    if (changes > 0)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InvokeMessage("Thẻ:【" + card.CardNo + "】Đã lưu thành công", "ok");
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
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

        private async Task<List<RFIDOutput>> updateEPCData(ObservableCollection<TrolleyEPCMapping> models)
        {
              
            string epcs = string.Join(", ", models.Select(m => m.EPC));

            string token = epc_token;
            string type = "RFID";
            string url = epc_uri + $"?epcs={epcs}&token={token}&Type={type}";

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                byte[] bytes = Encoding.UTF8.GetBytes(url);
                webClient.Headers.Add("ContentLength", bytes.Length.ToString());
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidate);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                string json = Encoding.UTF8.GetString(webClient.UploadData(url, "POST", bytes));
                if (JToken.Parse(json)[(object)
                    "Status"].ToString().ToUpper().Trim() != "OK")
                {
                    //this.tips(" There is no any SAP result to the EPC: " + epc);
                    return null;
                }
                else
                {
                    using (var db = new ShaContext())
                    {
                        var countrys = db.CountryCodeMappings.ToList();


                        JObject jobject = JObject.Parse(json);
                        var rfid = jobject["RFID"].ToList();
                        var rfidList = rfid.Select(rfidToken => new RFID
                        {
                            EPC = rfidToken["EPC"].ToString(),
                            SKU = rfidToken["SKU"].ToString(),
                            PO = rfidToken["PO"].ToString(),
                            DO = rfidToken["DO"].ToString(),
                            Color = rfidToken["Color"].ToString(),
                            Size = rfidToken["Size"].ToString(),
                            SampleCode = rfidToken["SampleCode"].ToString(),
                            Country = countrys.Where(x => x.Code.Trim() == rfidToken["SampleCode"].ToString().Substring(0, 2)).FirstOrDefault().CountryCode
                        }).ToList();

                        var groupedRfids = rfidList
                           .GroupBy(r => new { r.EPC, r.SKU, r.DO, r.Color, r.Size, r.Country })
                           .Where(g => g.Select(r => r.PO).Distinct().Count() > 1)
                           .Select(g => new RFIDOutput
                           {
                               //GroupKey = g.Key,
                               //FirstPO = g.First().PO,
                               //Rfids = g.ToList()

                               EPC = g.Key.EPC,
                               SKU = g.Key.SKU,
                               PO=g.First().PO,
                               DO = g.Key.DO,
                               Color = g.Key.Color,
                               Size = g.Key.Size,
                               Country = g.Key.Country
                           })
                           .ToList();

                    }




                    // model.SKU = jobject["RFID"][(object)0][(object)
                    //   "SKU"
                    // ].ToString();
                    // model.Color = jobject["RFID"][(object)0][(object)
                    //   "Color"
                    // ].ToString();
                    // model.Size = jobject["RFID"][(object)0][(object)
                    //   "Size"
                    // ].ToString();
                    // model.PO = jobject["RFID"][(object)0][(object)
                    //   "PO"
                    // ].ToString();
                    // string samplecode= jobject["RFID"][(object)0][(object)
                    //  "SampleCode"
                    //].ToString();

                    // if (samplecode.Length > 0) {
                    //     using (var db = new ShaContext())
                    //     {
                    //         var cl = db.CountryCodeMappings.ToList();
                    //         var countryEntity = db.CountryCodeMappings.Where(x => x.Code.Trim()== samplecode.Substring(0,2)).FirstOrDefault();
                    //         if (countryEntity!=null)
                    //         {
                    //             model.CountryCode = countryEntity.CountryCode;

                    //         }    
                    //     }
                    // }

                    // return model;
                }
                return null;
            }
        }
    }
}
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
using ClosedXML.Excel;
using System.Data;
using RCabinet.Views;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Net;
using static RCabinet.Models.ShaContext;
using DocumentFormat.OpenXml.InkML;
using System.Windows.Interop;
using Microsoft.EntityFrameworkCore.Internal;
using System.Security.Cryptography.X509Certificates;
using System.Data.Entity.Core.Metadata.Edm;

using RCabinet.Helpers;

namespace RCabinet.ViewModels
{
    internal class MappingUQViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private List<string> comport = new List<string>();
        private List<string> _myPo = new List<string>();
        private readonly HttpClient _httpClient;
        private RFID_Reader reader = null;
        private bool isReading = false;

        private ObservableCollection<TrolleyEPCMapping> _trolleyEPCMapping;
        private ObservableCollection<CardUQModel> cardUQModels;

        public delegate void callBackTips(string value);

        public event Action RequestFocusOnCardId;

        private CardKey cardkey = (CardKey)null;
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _cardId { get; set; }
        private POEpcModel _poEPCModels { get; set; }
        private CardUQModel _card { get; set; }
        private ObservableCollection<PosModel> _pos { get; set; }
        private ObservableCollection<string> messageNoti { get; set; }
        private string epc_token;
        private string epc_uri;
        private Guid mappingId;
        private int totalQuantity;
        private int _mappedQuantity;
        private List<string> _comPort { get; set; }
        private string mesageInfo;
        private string _comportSelectedItem;
        private int checkCount = 0;
        private PosModel _poSelectedItem;
        private int count = 1;
        private string readingStatus;
        private AsyncRelayCommand<Tuple<string, string, string, string, PosModel>> _loadComboBoxCommand;
        private bool enableReadingEPC;
        private bool lastChecked = false;
        private bool saving = false;
        private string EPC_Color = "";
        private string EPC_Size = "";
        private bool enableSwipeCard = true;
        private int _totalCount = 0;

        #region Properties

        public string MessageInfo
        {
            get { return mesageInfo; }
            set
            {
                mesageInfo = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> MessageNotiy
        {
            get { return messageNoti; }
            set
            {
                messageNoti = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableSwipeCard
        {
            get { return enableSwipeCard; }
            set
            {
                enableSwipeCard = value;
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

        public bool EnableReadingEPC
        {
            get { return enableReadingEPC; }
            set
            {
                enableReadingEPC = value;
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

        public ObservableCollection<CardUQModel> CardUQModels
        {
            get { return cardUQModels; }
            set
            {
                cardUQModels = value;
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

        public ObservableCollection<TrolleyEPCMapping> TrolleyEPCMappings
        {
            get { return _trolleyEPCMapping; }
            set
            {
                _trolleyEPCMapping = value;
                NotifyPropertyChanged();
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

        public CardUQModel Card
        {
            get => _card;
            set
            {
                _card = value;
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

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
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
                    Application.Current.Dispatcher.Invoke(() =>
                                                                                                    {
                                                                                                        reader.stopReading();
                                                                                                        reader.closeComport();
                                                                                                        ReadingStatus = "Start Reading EPC";
                                                                                                        isReading = false;
                                                                                                    });
                }
            }
            PopViewModel();
        }

        public MappingUQViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            ReadingStatus = "Start Reading EPC";
            saving = false;
            totalQuantity = 0;
            count = 1;
            EnableReadingEPC = false;
            CardUQModels = new ObservableCollection<CardUQModel>();
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
            TrolleyEPCMappings = new ObservableCollection<TrolleyEPCMapping>();
            //get epc token from app.config
            epc_uri = System.Configuration.ConfigurationManager.AppSettings["API_EPC_URI"];
            epc_token = System.Configuration.ConfigurationManager.AppSettings["API_EPC_TOKEN"];
            ComPortSelectedItem = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];

            if (epc_uri == null || epc_token == null)
            {
                SoundHelper.PlaySoundError();
                MessageBox.Show("Please set EPC API URI and Token in Config", "Error!", MessageBoxButton.OK);
                return;
            }
            EnableSwipeCard = true;
            TotalCount = 0;
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
            get { return new RelayCommand(async () => await Reset()); }
        }


        private async Task Reset()
        {
            await ClearEPCGrid();
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageNotiy.Clear();
                MessageNotiy= new ObservableCollection<string>();
            });
        }
        private async Task ClearEPCGrid()
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    CardUQModels.Clear();
                    TrolleyEPCMappings.Clear();
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
                    TotalQuantity = 0;
                    reader.stopReading();
                    reader.closeComport();
                    ReadingStatus = "Start Reading EPC";
                    isReading = false;
                    EnableSwipeCard = true;
                    TotalCount = 0;
                    messageNoti = new ObservableCollection<string>();
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
                if (isReading != null && isReading == true)
                {
                    // reader.stopReading();
                    reader.closeComport();
                    isReading = false;
                }
                ReadingStatus = "Start Reading EPC";
            }
        }

        private async Task LoadCardDataDetail()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageInfo = string.Empty;
                MessageNotiy.Clear();
                MessageNotiy = new ObservableCollection<string>();
            });
             
  
            if (string.IsNullOrWhiteSpace(CardId))
            {
                return;
            }

            // lấy thông tin chi tiết của thẻ từ database
            string cardid = killZero(CardId.Trim());
            DataSet dataSet = SqlHelper.getDataSet("exec P_RFIDReader_GetDataByCardNOForUQ_Haki " + SqlHelper.quotedStr(cardid));

            if (dataSet.Tables[0].Rows.Count < 1)
            {
                InvokeMessage("ID: 【" + cardid + "】Thẻ không hợp lệ hoặc không có ID này, vui lòng kiểm tra lại", "error");
                CardId = string.Empty;
                return;
            }
            if (this.cardkey.zdcode != string.Empty && (dataSet.Tables[0].Rows[0]["ZDcode"].ToString().Trim() != this.cardkey.zdcode || this.cardkey.colorno != dataSet.Tables[0].Rows[0]["ColorNo"].ToString().Trim() || this.cardkey.size != dataSet.Tables[0].Rows[0]["sSize"].ToString().Trim() || this.cardkey.ganghao != dataSet.Tables[0].Rows[0]["GangHao"].ToString().Trim()))
            {
                InvokeMessage("ID:【" + cardid + "】Đơn đặt hàng, màu sắc, kích thước, số gang hao khác với số thẻ đã đọc, hoạt động này không hợp lệ", "unmatch");
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
                var t = db.TrolleyUQCards.ToList();
                var cardInstance = db.TrolleyUQCards.FirstOrDefault((CardUQModel e) => e.CardNo.Contains(cardid));

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
                StyleNo = dataSet.Tables[0].Rows[0]["StyleNO"].ToString(),
                MappingId = mappingId,
                SO = dataSet.Tables[0].Rows[0]["SO"].ToString(),
                Mo = dataSet.Tables[0].Rows[0]["ZDcode"].ToString(),
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
                CardNo = dataSet.Tables[0].Rows[0]["CardNo"].ToString(),
                SoItem = dataSet.Tables[0].Rows[0]["SoItem"].ToString(),
                SaleNo = dataSet.Tables[0].Rows[0]["SaleNo"].ToString()
            };

            DataSet dsEPCcolorsize = SqlHelper.getDataSet("exec P_Hanlde_RFIDReadert_GetCustColorSize @saleno= " + SqlHelper.quotedStr(Card.SaleNo) + ",@soitem=" + SqlHelper.quotedStr(Card.SoItem) + ",@color=" + SqlHelper.quotedStr(Card.ColorNo) + ",@size=" + SqlHelper.quotedStr(Card.Size));
            if (dsEPCcolorsize.Tables[0].Rows.Count < 1)
            {
                InvokeMessage("ID:【" + Card.CardNo + "】 [MK280] Thông tin Mapping thẻ hàng ETS và [Size; Màu] SKU của EPC chưa được upload lên ETS", "error");
                SoundHelper.PlaySoundError();
                return;
            }
            else
            {
                EPC_Color = dsEPCcolorsize.Tables[0].Rows[0]["CustColorNo"].ToString();
                EPC_Size = dsEPCcolorsize.Tables[0].Rows[0]["CustSize"].ToString();
            }

            if (this.cardkey.zdcode == string.Empty)
            {
                this.cardkey.zdcode = dataSet.Tables[0].Rows[0]["ZDcode"].ToString();
                this.cardkey.colorno = dataSet.Tables[0].Rows[0]["ColorNo"].ToString();
                this.cardkey.size = dataSet.Tables[0].Rows[0]["sSize"].ToString();
                this.cardkey.ganghao = dataSet.Tables[0].Rows[0]["GangHao"].ToString();
            }

            if (Convert.ToInt32(dataSet.Tables[0].Rows[0]["cCount"].ToString()) != 0)
            {
                addCardList(Card);
                CardId = string.Empty;
            }
            if (TotalQuantity > 0) EnableReadingEPC = true;
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
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
                if (!TrolleyEPCMappings.Any((TrolleyEPCMapping e) => e.EPC.Contains(msg.logBaseEpcInfo.Epc)))
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
                                                         PO = "",
                                                         SKU = "",
                                                         Size = "",
                                                         Color = "",
                                                         EmpCode = CurrentUser.Name
                                                     };

                                      var epcModel = updateEPCData(model).Result;
                                      if (epcModel != null)
                                      {
                                          // check epcModel.Color ==null || epc.Color =""
                                          if (epcModel.Color == "")
                                          {
                                              InvokeMessage("EPC:【" + epcModel.EPC + "】Không thể lấy thông tin Màu sắc từ EPC", "error");
                                              SoundHelper.PlaySoundError();
                                              return;
                                          }
                                          if (epcModel.Size == "")
                                          {
                                              InvokeMessage("EPC:【" + epcModel.EPC + "】Không thể lấy thông tin Size từ EPC", "error");
                                              SoundHelper.PlaySoundError();
                                              return;
                                          }
                                          if (epcModel.Color != EPC_Color.Trim())
                                          {
                                              InvokeMessage("EPC:【" + epcModel.EPC + "】Màu sắc thẻ EPC không khớp với thông tin màu sắc của thẻ hàng", "error");
                                              SoundHelper.PlaySoundError();
                                              return;
                                          }
                                          if (epcModel.Size != EPC_Size.Trim())
                                          {
                                              InvokeMessage("EPC:【" + epcModel.EPC + "】Size thẻ EPC không khớp với thông tin Size của thẻ hàng", "error");
                                              SoundHelper.PlaySoundError();
                                              return;
                                          }

                                          using (var db = new ShaContext())
                                          {
                                              var epc = db.TrolleyEPCMappings.FirstOrDefault((TrolleyEPCMapping e) => e.EPC.Contains(model.EPC));
                                              if (epc != null)
                                              {
                                                  //check if MessageNotiy contains  
                                                  string err = "EPC:【" + model.EPC + "】 đã làm liên kết";
                                                  if (MessageNotiy.Contains(err) == false)
                                                  {
                                                      InvokeMessage(err,"error");
                                                  }
                                              }
                                              else
                                              {
                                                  TrolleyEPCMappings.Add(epcModel);
                                                  TotalCount = count;
                                                  count++;
                                              }
                                          }
                                      }
                                  });
                }
                if (TrolleyEPCMappings.Count > TotalQuantity)
                {
                    //SoundHelper.PlaySoundUnmatch();
                    InvokeMessage("Đã đọc số lượng thẻ:" + this.TrolleyEPCMappings.Count.ToString() + "Chiếc,Đã vượt quá số lượng " + Convert.ToString(TrolleyEPCMappings.Count - Convert.ToInt32(TotalQuantity)) + " Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn", "error");
                    //resetAll();
                }

                if (!lastChecked && checkCount == 2)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                                              {
                                                  //CardMappingModels.Clear();
                                              });
                    lastChecked = true;
                }

                if (TrolleyEPCMappings.Count == TotalQuantity)
                {
                    ++checkCount;
                }
                if (TrolleyEPCMappings.Count == TotalQuantity && checkCount > 2 && lastChecked && saving == false)
                {
                    SaveCard();
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

        private async Task<TrolleyEPCMapping> updateEPCData(TrolleyEPCMapping model)
        {
            var epc = model.EPC;
            string token = epc_token;
            string type = "RFID";
            string url = epc_uri + $"?epc={epc}&token={token}&Type={type}";

            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                byte[] bytes = Encoding.UTF8.GetBytes(url);
                webClient.Headers.Add("ContentLength", bytes.Length.ToString());
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidate);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
                string json = Encoding.UTF8.GetString(webClient.UploadData(url, "POST", bytes));
                if (JToken.Parse(json)[(object)"Status"].ToString().ToUpper().Trim() != "OK")
                {
                    //this.tips(" There is no any SAP result to the EPC: " + epc);
                    return null;
                }
                else
                {
                    JObject jobject = JObject.Parse(json);
                    model.SKU = jobject["RFID"][(object)0][(object)"SKU"].ToString();
                    model.Color = jobject["RFID"][(object)0][(object)"Color"].ToString();
                    model.Size = jobject["RFID"][(object)0][(object)"Size"].ToString();
                    model.PO = jobject["RFID"][(object)0][(object)"PO"].ToString();
                    return model;
                }
                return null;
            }
        }

        private async Task SaveCard()
        {
            saving = true;
            int cardqty = 0;
            int epcqty = 0;
            int totalqty = 0;

            isReading = false;
            Application.Current.Dispatcher.Invoke(() =>
                                                  {
                                                      reader.stopReading();
                                                      reader.closeComport();
                                                      ReadingStatus = "Start Reading EPC";
                                                  });

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

            using (var db = new ShaContext())
            {
                foreach (var card in CardUQModels)
                {
                    epcqty = 0;
                    cardqty = card.AdjustQuantity;
                    db.TrolleyUQCards.Add(card);

                    foreach (var model in TrolleyEPCMappings)
                    {
                        if (epcqty < cardqty && !addedModels.Contains(model))
                        {
                            // check db.TrolleyEPCMappings  contains  model.epccode
                            epcqty++;
                            model.UQCardId = card.Id;
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

                saving = false;
                EnableSwipeCard = true;
            }
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

        private void InvokeMessage(string message,string soundtype)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageInfo = message;
            MessageNotiy.Add(MessageInfo);
                if (soundtype == "ok")
                    SoundHelper.PlaySoundOK();
                else if(soundtype=="unmatch")
                    SoundHelper.PlaySoundUnmatch();
                else
                    SoundHelper.PlaySoundError();




            });
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

        private void clearCardKey()
        {
            this.cardkey.zdcode = string.Empty;
            this.cardkey.colorno = string.Empty;
            this.cardkey.size = string.Empty;
            this.cardkey.ganghao = string.Empty;
        }
    }
}
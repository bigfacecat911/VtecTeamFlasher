﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ClientAndServerCommons;
using ClientAndServerCommons.DataClasses;
using ClientAndServerCommons.Statuses;
using Commons.Helpers;
using Commons.Serialization.Binary;
using VtecTeamFlasher.Helpers;

namespace VtecTeamFlasher
{
    public partial class VTFlasher : Form
    {
        private IntPtr pcmMainWindow;
        private Process pcmProcess;
        private List<IntPtr> pcmChildren;

        private IntPtr pcmComboBoxModules;
        private IntPtr pcmComboBoxAdapters;
        private IntPtr pcmTextBoxFilePath;
        private IntPtr pcmButtonExit;
        private IntPtr pcmButtonSettings;
        private IntPtr pcmKeyNumber;
        private IntPtr pcmTextBoxStatus;
        private IntPtr pcmButtonInitialize;
        private IntPtr pcmButtonIdentify;
        private IntPtr pcmButtonReadErrors;
        private IntPtr pcmButtonRestart;
        private IntPtr pcmButtonEraseErrors;
        private IntPtr pcmButtonRestartAdaptation;
        private IntPtr pcmButtonRead;
        private IntPtr pcmButtonWrite;
        private IntPtr pcmButtonCheckCorrectCS;

        private List<string> CarManufacture = XMLHelper.GetCARManufacture();
        private List<string> CarModel = new List<string>();

        private Thread txtStatusThread;
        private StringBuilder sb = new StringBuilder(1024, 90000);

        private BinarySerializationHelper serializationHelper = new BinarySerializationHelper();
        private string savedPassword = "";

        public VTFlasher()
        {
            InitializeComponent();
            panelLogin.BringToFront();
        }

        private void VTFlasher_Load(object sender, EventArgs e)
        {

            if (File.Exists(Path.Combine(Application.StartupPath, FilePathProvider.PasswordFilePath)))
            {
                savedPassword = FileHelper.ReadText(Path.Combine(Application.StartupPath, FilePathProvider.PasswordFilePath));
                if (!string.IsNullOrEmpty(savedPassword))
                {
                    txtPassword.Text = (string)serializationHelper.DeserializeObject(savedPassword);
                    checkBoxSavePassword.Checked = true;
                }
            }
           
            
            var info = new ProcessStartInfo
            {
                FileName = AppDomain.CurrentDomain.BaseDirectory + "\\PcmFlasher\\pcmflash.exe",
                UseShellExecute = true,
                //CreateNoWindow = true,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                // WindowStyle = ProcessWindowStyle.Hidden
            };
            pcmProcess = Process.Start(info);
            pcmProcess.WaitForInputIdle();

            pcmMainWindow = WinAPIHelper.FindWindowByCaption(IntPtr.Zero, "PCMflash");

            WinAPIHelper.SetParent(pcmProcess.MainWindowHandle, this.Handle);
            
            pcmChildren = GetChildWindows(pcmMainWindow);

            pcmComboBoxModules = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[6], null, null);
            pcmComboBoxAdapters = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[0], null, null);
            pcmTextBoxFilePath = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[14], null, null);
            pcmButtonExit = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[22], null, null);
            pcmButtonSettings = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[4], null, null);
            pcmKeyNumber = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[3], null, null);
            pcmTextBoxStatus = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[20], null, null);
            pcmButtonInitialize = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[10], null, null);
            pcmButtonIdentify = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[7], null, null);
            pcmButtonReadErrors = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[8], null, null);
            pcmButtonRestart = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[9], null, null);
            pcmButtonEraseErrors = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[11], null, null);
            pcmButtonRestartAdaptation = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[12], null, null);
            pcmButtonRead = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[17], null, null);
            pcmButtonWrite = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[18], null, null);
            pcmButtonCheckCorrectCS = WinAPIHelper.FindWindowEx(pcmMainWindow, pcmChildren[16], null, null);

            InitializeComboBoxControl(pcmComboBoxAdapters, cbAdapter);
            //InitializeComboBoxControl(pcmComboBoxModules, cbModules);
            SetButtonStatus();


            WinAPIHelper.SendMessage(pcmTextBoxStatus, WinAPIHelper.WM_GETTEXT, 10000, sb);
            if (sb.ToString().Contains("Электронный ключ недоступен"))
            {
                pcmProcess.Kill();
                pcmMainWindow = IntPtr.Zero;
                panelKeyUnavailible.BringToFront();
                panelKeyUnavailible.Visible = true;
                return;
            }
            else
            {
                panelKeyUnavailible.Visible = false;
            }

            txtStatusThread = new Thread(() =>
                    {
                        string pcmTextData = "";
                        while (true)
                        {

                            WinAPIHelper.SendMessage(pcmTextBoxStatus, WinAPIHelper.WM_GETTEXT, 10000, sb);
                            pcmTextData = sb.ToString();
                            var stringPcmText = pcmTextData.Replace("\n","").Split('\r');
                            var denyStringsList = new List<string>();

                            denyStringsList.Add("Версия программы");

                            foreach (string denyStrings in denyStringsList)
                            {
                                stringPcmText = stringPcmText.Where(t => !t.StartsWith(denyStrings)).ToArray<string>();
                            }

                            
                            //this.Invoke(()=>txtStatus.Text = sb.ToString());
                            this.Invoke(() => tbReflashText.Text = String.Join("\r\n", stringPcmText));
                            Thread.Sleep(100);
                        }
                    });
            txtStatusThread.Start();
        }

        # region ComboBoxes
        private void InitializeComboBoxControl(IntPtr controlHandle, ComboBox comboBox)
        {
            var ssb = new StringBuilder(256, 256);
            comboBox.Items.Clear();

            if (controlHandle != IntPtr.Zero)
            {
                var count = WinAPIHelper.SendMessage(controlHandle, WinAPIHelper.CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);
                for (int i = 0; i < (int)count; i++)
                {
                    if (WinAPIHelper.SendMessage(controlHandle, WinAPIHelper.CB_GETLBTEXT, i, ssb) != (IntPtr)(-1))
                        comboBox.Items.Add(ssb.ToString());
                }
                var pcmSelectedIndex = (int)WinAPIHelper.SendMessage(controlHandle, WinAPIHelper.CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
                if (pcmSelectedIndex != -1)
                    WinAPIHelper.SendMessage(controlHandle, WinAPIHelper.CB_GETLBTEXT, pcmSelectedIndex, ssb);
                comboBox.SelectedIndex = pcmSelectedIndex;
            }
        }

        private void cbAdapter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pcmComboBoxAdapters != IntPtr.Zero)
            {
                WinAPIHelper.SendMessage(pcmComboBoxAdapters, WinAPIHelper.CB_SETCURSEL, cbAdapter.SelectedIndex, "");
            }
        }

        private void cbModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pcmComboBoxModules != IntPtr.Zero)
            {
                changePCMModules(cbModules.SelectedItem.ToString());
               // WinAPIHelper.SendMessage(pcmComboBoxModules, WinAPIHelper.CB_SETCURSEL, cbModules.SelectedIndex, "");

                //WinAPIHelper.SendMessage(pcmComboBoxModules, 0x014F/*CB_SHOWDROPDOWN*/, 1, "");
                //WinAPIHelper.SendMessage(pcmComboBoxModules, 0x014E/*CB_SETCURSEL*/, cbModules.SelectedIndex, "");
                //WinAPIHelper.SendMessage(pcmComboBoxModules, 0x0201/*WM_LBUTTONDOWN*/, 0, "-1");
                //WinAPIHelper.SendMessage(pcmComboBoxModules, 0x0202/*WM_LBUTTONUP*/, 0, "-1");
                //WinAPIHelper.SendMessage(pcmComboBoxModules, 0x014F/*CB_SHOWDROPDOWN*/, 0, "0");
                //WinAPIHelper.SendMessage(pcmComboBoxModules, WinAPIHelper.CB_SETCURSEL, cbModules.SelectedIndex, "");
                SetButtonStatus();
            }
        }

        # endregion

        #region Buttons
        private void SetButtonStatus()
        {
            cbControlSum.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonCheckCorrectCS);
            btnInitialize.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonInitialize);
            btnIdentify.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonIdentify);
            btnReadErrors.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonReadErrors);
            btnRestart.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonRestart);
            btnEraseErrors.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonEraseErrors);
            btnResetAdaptation.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonRestartAdaptation);
            btnReadFromECU.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonRead);
            btnWrite.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonWrite);
            btnSettings.Enabled = WinAPIHelper.IsWindowEnabled(pcmButtonSettings);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonSettings);
        }

        private void btnIdentify_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonIdentify);
        }

        private void btnReadErrors_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonReadErrors);
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonRestart);
        }

        private void btnInitialize_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonInitialize);
        }

        private void btnEraseErrors_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonEraseErrors);
        }

        private void btnResetAdaptation_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonRestartAdaptation);
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonRead);
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonWrite);
        }

        private void cbControlSum_CheckedChanged(object sender, EventArgs e)
        {
            ButtonClick(pcmButtonCheckCorrectCS);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void ButtonClick(IntPtr button)
        {
            if (pcmMainWindow != IntPtr.Zero)
                WinAPIHelper.SendMessage(button, WinAPIHelper.BN_CLICKED, IntPtr.Zero, IntPtr.Zero);
        }
        #endregion

        private void VTFlasher_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pcmMainWindow != IntPtr.Zero)
            {
                WinAPIHelper.SendMessage(pcmButtonExit, WinAPIHelper.BN_CLICKED, IntPtr.Zero, IntPtr.Zero);
                txtStatusThread.Abort();
            }
        }

       

        private static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            var result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new WinAPIHelper.EnumWindowProc(EnumWindow);
                WinAPIHelper.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        /// <summary>
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            var gch = GCHandle.FromIntPtr(pointer);
            var list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        private void btnOpenFileDialog_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "Файлы прошивки|*.bin|Все файлы|*.*" };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            txtFilePath.Text = fileDialog.FileName;
            if (pcmTextBoxFilePath != IntPtr.Zero)
                WinAPIHelper.SendMessage(pcmTextBoxFilePath, WinAPIHelper.WM_SETTEXT, IntPtr.Zero, fileDialog.FileName);
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            lblErrLogin.Text = "";
            pbLogin.Image = Properties.Resources.Animation;


            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                lblErrLogin.Text = "Необходимо ввести логин и пароль.";
                return;
            }

            var currentStatus = PanelRefresh.StartRefresh(panelLogin, pbLogin);

            await Task.Run(() =>
            {
                var service = WCFServiceFactory.CreateVtecTeamService();
                var result = service.Authenticate(txtUsername.Text, txtPassword.Text.ComputeSHA256Hash());

                if (result.Code == (int)AuthenticationCode.Success)
                {
                    if (checkBoxSavePassword.Checked && string.IsNullOrEmpty(savedPassword))
                        FileHelper.SaveText(serializationHelper.SerializeObject(txtPassword.Text), Path.Combine(Application.StartupPath, FilePathProvider.PasswordFilePath));

                    Session.CurrentUser = result.User;
                    loadCbModules();
                    this.Invoke(()=>panelLogin.Dispose());
                }
                else
                {
                    this.Invoke(()=> txtPassword.Text = "");
                    this.Invoke(()=>lblErrLogin.Text = result.Message);
                    this.Invoke(()=>pbLogin.Image = Properties.Resources.Error);
                }
                    
            });

            PanelRefresh.StopRefresh(currentStatus);
            
            
        }

        private void CleanBinaryDescriptionData()
        {
            cbBinaryToLoad.DataSource = null;
            cbBinaryToLoad.Items.Clear();
            txtBinaryDescription.Text = "";
            cbBinaryDescriptionCS.Checked = false;
            cbBinaryDescriptionEGROff.Checked = false;
            cbBinaryDescriptionEuro2.Checked = false;
            cbBinaryDescriptionImmoOff.Checked = false;
            btnBinaryDescriptionOK.Enabled = false;
            btnBinaryDescriptionCancel.Enabled = false;
        }
        private void lbModule_Click(object sender, EventArgs e)
        {

        }

        private void btnBinaryDescriptionCancel_Click(object sender, EventArgs e)
        {
            CleanBinaryDescriptionData();
            panelLoadBinary.Visible = false;

        }

        #region UsserDetails
        private void tabControlMain_Click(object sender, EventArgs e)
        {
            tbUserName.Text = Session.CurrentUser.FirstName;
            tbUserSecondName.Text = Session.CurrentUser.LastName;
            tbUserCity.Text = Session.CurrentUser.City;
            tbUserPhone.Text = Session.CurrentUser.Phone;
            tbUserSkype.Text = Session.CurrentUser.Skype;
            tbUserVK.Text = Session.CurrentUser.VK;
            cbUserViber.Checked = Session.CurrentUser.Viber;
            cbUserWhatsapp.Checked = Session.CurrentUser.WhatsApp;
        }

        private async void btnUpdateUserDetails_Click(object sender, EventArgs e)
        {
            var currentStatus = PanelRefresh.StartRefresh(tabPerson, pbPersonalInfo);

            await Task.Run(() =>
            {
                Session.CurrentUser.FirstName = tbUserName.Text;
                Session.CurrentUser.LastName = tbUserSecondName.Text;
                Session.CurrentUser.City = tbUserCity.Text;
                Session.CurrentUser.Phone = tbUserPhone.Text;
                Session.CurrentUser.Skype = tbUserSkype.Text;
                Session.CurrentUser.VK = tbUserVK.Text;
                Session.CurrentUser.Viber = cbUserViber.Checked;
                Session.CurrentUser.WhatsApp = cbUserWhatsapp.Checked;

                RequestExecutor.Execute(() =>
                {
                    var result = WCFServiceFactory.CreateVtecTeamService().UpdateUserPersonalData(Session.CurrentUser);
                    this.Invoke(() => pbPersonalInfo.Image = !result ? Properties.Resources.Error : null);
                }); 
            });

            PanelRefresh.StopRefresh(currentStatus);

        }
        #endregion

        private void btnReloadFlasher_Click(object sender, EventArgs e)
        {
            VTFlasher_Load(sender,e);
        }

        #region HistoryAndRequests

        private async void btnSendRequest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRequestCarDescription.Text))
            {
                MessageBox.Show("Поле Номер Машина обязательно для заполнения");
                return;
            }
            
            var currentStatus = PanelRefresh.StartRefresh(pnlSendRequest, pbRequest);
            await Task.Run(() =>
            {
                var request = new ReflashRequest
                {
                    RequestDetails = txtAdditionalInfo.Text,
                    EcuNumber = txtEcuNumber.Text,
                    BinaryNumber = txtEcuBinaryNumber.Text,
                    UserId = Session.CurrentUser.Id,
                    RequestDate = DateTime.Now,
                    Status = (int) RequestStatuses.New,
                    CarDescription = txtRequestCarDescription.Text,
                    //User = Session.CurrentUser,
                };

                if (File.Exists(txtStockFilePath.Text))
                {
                    request.StockBinary = File.ReadAllBytes(txtStockFilePath.Text);
                    request.StockBinaryName = Path.GetFileName(txtStockFilePath.Text);
                }

                if (File.Exists(txtEcuPhotoStatus.Text))
                {
                    request.EcuPhoto = File.ReadAllBytes(txtEcuPhotoStatus.Text);
                    request.EcuPhotoFilename = Path.GetFileName(txtEcuPhotoStatus.Text);
                }

                RequestExecutor.Execute(() =>
                {
                    var result = WCFServiceFactory.CreateVtecTeamService().SendRequest(request);

                    this.Invoke(() => pbRequest.Image = !result ? Properties.Resources.Error : null);
                    MessageBox.Show(result ? "Запрос успешно отправлен" : "Не удалось отправить запрос.");
                });
            });

            PanelRefresh.StopRefresh(currentStatus);
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "Файлы прошивки|*.bin|Все файлы|*.*" };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;
            txtStockFilePath.Text = fileDialog.FileName;
        }

        private async void btnRefreshRequests_Click(object sender, EventArgs e)
        {
            var currentStatus = PanelRefresh.StartRefresh(pnlRequestsHistory, pbRequestHistory);
            await Task.Run(() => RequestExecutor.Execute(() =>
            {
                var result = WCFServiceFactory.CreateVtecTeamService().GetReflashRequests(Session.CurrentUser.Id);
                this.Invoke(() => dgRequests.DataSource = result);
            }));
            pbRequestHistory.Visible = false;
            PanelRefresh.StopRefresh(currentStatus);

        }

        private async void btnRefreshHistory_Click(object sender, EventArgs e)
        {
            var currentStatus = PanelRefresh.StartRefresh(tabHistory, pbReflashHistory);
            await Task.Run(() => RequestExecutor.Execute(() =>
            {
                var result = WCFServiceFactory.CreateVtecTeamService().GetReflashHistory(Session.CurrentUser.Id);
                this.Invoke(() => dgReflashHistory.DataSource = result);
            }));
            pbReflashHistory.Visible = false;
            PanelRefresh.StopRefresh(currentStatus);
        }

        private void tabHistory_Click(object sender, EventArgs e)
        {
            RequestExecutor.Execute(()=>dgReflashHistory.DataSource = WCFServiceFactory.CreateVtecTeamService().GetReflashHistory(Session.CurrentUser.Id));
        }

        private void dgReflashHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                var historyWithReviewForm = new ReflashHistoryWithReviewForm((ReflashHistory)senderGrid.Rows[e.RowIndex].DataBoundItem);
                historyWithReviewForm.ShowDialog();
                return;
            }

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewTextBoxColumn && e.RowIndex >= 0 && senderGrid.Columns[e.ColumnIndex].DataPropertyName == "PaymentStatus")
            {
                var dialogResult = MessageBox.Show("Вы действиельно хотите подтвердить факт отправки денег?", "Подтверждение отправки денег за прошивку",MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    var obj = (ReflashHistory)senderGrid.Rows[e.RowIndex].DataBoundItem;
                    obj.Status = (int)PaymentStatuses.Paid;
                   RequestExecutor.Execute(() =>
                   {
                       var result = WCFServiceFactory.CreateVtecTeamService().UpdateReflashHistory(obj);
                       MessageBox.Show(result ? "Данне успешно отправлены" : "Не удалось отправить данные");
                   });
                }
            }
           
        }

        private async void btnUploadBinary_Click(object sender, EventArgs e)
        {
            tabControlReflash.SelectTab(tabReflashUpload);

            panelLoadBinary.BringToFront();
            pbReflash.BringToFront();
            panelLoadBinary.Visible = true;

            var currentStatus = PanelRefresh.StartRefresh(panelLoadBinary, pbReflash);

            //// TODO Load binary descriptions
            RequestExecutor.Execute(() =>
            {
                //var reflashFile = WCFServiceFactory.CreateVtecTeamService().GetReflashFile(new ReflashRequest());
            }); 

            pbReflash.Visible = false;
            PanelRefresh.StopRefresh(currentStatus);
        }

        private void dgReflashHistory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            //if (e.RowIndex != -1 && e.ColumnIndex != -1 && senderGrid.Columns[e.ColumnIndex].DataPropertyName == "PaymentStatus")
            //{
            //    var paymentStatus = (int) senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            //    switch (paymentStatus)
            //    {
            //        case 1:
            //            e.Value = "Обрабатывается(клик для отправки денег)";
            //            e.CellStyle.BackColor = Color.Red;
            //            break;
            //        case 2:
            //            e.Value = "Оплачено";
            //            break;
            //        case 3:
            //            e.Value = "Возвращено";
            //            break;
            //    }
            //    e.FormattingApplied = true;

            //}

            if (e.RowIndex != -1 && e.ColumnIndex != -1 && senderGrid.Columns[e.ColumnIndex].DataPropertyName == "Status")
            {
                var reflashStatus = (int)senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                e.Value = StatusResolver.ResolveReflashStatus(reflashStatus);
                
                e.FormattingApplied = true;
            }

        }

        private void dgRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.RowIndex != -1 && e.ColumnIndex != -1 && senderGrid.Columns[e.ColumnIndex].DataPropertyName == "Status")
            {
                var requestStatus = (int)senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                e.Value = StatusResolver.ResolveRequestStatus(requestStatus);
                e.FormattingApplied = true;

            }
        }

        private void dgRequests_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                var watchReviewWithComments = new RequestWithCommentsForm((ReflashRequest)senderGrid.Rows[e.RowIndex].DataBoundItem);
                watchReviewWithComments.ShowDialog();
            }
        }

        private void btnRequestUploadEcuPhoto_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "Все картинки|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|Все файлы|*.*" };

            if (fileDialog.ShowDialog() != DialogResult.OK)
                return;

            txtEcuPhotoStatus.Text = fileDialog.FileName;

        }
        #endregion
        private void button1_Click(object sender, EventArgs e)
        {
            var a = txtStatus.Text;
            var stringPcmText = a.Replace("\n", "").Split('\r');
            stringPcmText = stringPcmText.Where(t => !t.StartsWith("Версия программы")).ToArray<string>();
            //stringPcmText = (string[])stringPcmText.Where(t => t.StartsWith("asd"));
            a = a + "1";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var pcmSelectedIndex = (int)WinAPIHelper.SendMessage(pcmComboBoxModules, WinAPIHelper.CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
            txtStatus.Text = pcmSelectedIndex.ToString();
        }


        #region News
        private void treeList1_NodeCellStyle(object sender, DevExpress.XtraTreeList.GetCustomNodeCellStyleEventArgs e)
        {
            if (e.Node.ParentNode == null)
                e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
        }
        
        private void AddNode(string newsCaption, string newsText, DateTime date)
        {
            var nameNode = tlNews.AppendNode(null, null);
            nameNode.SetValue("info", string.Format("{0}     {1}", newsCaption, date));
            var commentNode = tlNews.AppendNode(null, nameNode);
            commentNode.SetValue("info", newsText);
            tlNews.ExpandAll();
        }

        private async void btnRefreshNews_Click(object sender, EventArgs e)
        {
            var currentStatus = PanelRefresh.StartRefresh(this, pbNews);
            await Task.Run(() => RequestExecutor.Execute(() =>
            {
                var allNews = WCFServiceFactory.CreateVtecTeamService().GetNews();
                this.Invoke(() =>
                {
                    foreach (var news in allNews)
                        AddNode(news.Caption, news.Text, news.Date);
                });
            }));
            pbNews.Visible = false;
            PanelRefresh.StopRefresh(currentStatus);
        }
        #endregion

        private async void btnSearchReflashFile_Click(object sender, EventArgs e)
        {
            var currentStatus = PanelRefresh.StartRefresh(panelLoadBinary, pbReflash);
            pbReflash.Visible = true;
            cbBinaryToLoad.DataSource = null;
            cbBinaryToLoad.Items.Clear();
            bool controlVisibility = false;

            await Task.Run(() => RequestExecutor.Execute(() =>
                {
                    var result = WCFServiceFactory.CreateVtecTeamService().GetInformationListOfReflashBinaries(txtEcuNumbertoSearch.Text);
                    if (result.Length != 0)
                    {
                        var items = new List<ComboBoxItem>();
                        foreach (var reflashInformation in result)
                        {
                            var altCodes = reflashInformation.AltEcuCode.Split(',');
                            items.AddRange(altCodes.Select(altCode => new ComboBoxItem{Value = reflashInformation.ReflashStorageId,
                                                                                       Text = altCode, XmlDescription = reflashInformation.Description}));
                        }
                        this.Invoke(() => cbBinaryToLoad.DataSource = items);
                        this.Invoke(() => cbBinaryToLoad.DisplayMember = "Text");
                        
                    }

                    controlVisibility = result.Length != 0;

                }));

            PanelRefresh.StopRefresh(currentStatus);

            gbBinaryDescription.Enabled = controlVisibility;
            lblChooseBinary.Enabled = controlVisibility;
            txtBinaryDescription.Enabled = controlVisibility;
            btnBinaryDescriptionOK.Enabled = controlVisibility;
            btnBinaryDescriptionCancel.Enabled = controlVisibility;
            //cbBinaryDescriptionImmoOff.Enabled = controlVisibility;
            //cbBinaryDescriptionEGROff.Enabled = controlVisibility;
            //cbBinaryDescriptionCS.Enabled = controlVisibility;
            //cbBinaryDescriptionEuro2.Enabled = controlVisibility;
            cbBinaryToLoad.Enabled = true;
            pbReflash.Visible = false;
        }

        private void cbBinaryToLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBinaryToLoad.DataSource != null)
            {
                if (!string.IsNullOrEmpty(((ComboBoxItem) cbBinaryToLoad.SelectedItem).XmlDescription))
                {
                    var xmlDescription = ((ComboBoxItem) cbBinaryToLoad.SelectedItem).XmlDescription.ToXmlDocument();
                    cbBinaryDescriptionImmoOff.Checked = xmlDescription.GetCheckedStatus("DisabledImmo");
                    cbBinaryDescriptionEuro2.Checked = xmlDescription.GetCheckedStatus("Euro2");
                    cbBinaryDescriptionEGROff.Checked = xmlDescription.GetCheckedStatus("DisabledEGR");
                    cbBinaryDescriptionCS.Checked = xmlDescription.GetCheckedStatus("CheckSum");


                    txtBinaryDescription.Text = xmlDescription.GetDescription();

                }
            };
        }

        private void btnBinaryDescriptionOK_Click(object sender, EventArgs e)
        {
            if (cbBinaryToLoad.SelectedItem != null)
            {
                RequestExecutor.Execute(() =>
                                {
                                    var reflashFile = WCFServiceFactory.CreateVtecTeamService().GetReflashFile(((ComboBoxItem)cbBinaryToLoad.SelectedItem).Value, Session.CurrentUser.Id);
                                    MessageBox.Show("Прошивка успешно загружена в память программы.");
                                });
            }
            else
            {
                MessageBox.Show("Не выбран номер прошивки для скачивания!");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity.Core.Objects;
using SetupOptimaToolkit.Models;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Documents.Serialization;
using System.IO;
using System.Collections;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MahApps.Metro.Controls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using SetupOptimaToolkit.Properties;


namespace SetupOptimaToolkit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        public string ErrorMessage;

        private List<Customer> Customers;
        private List<Detail> Details;
        public ObservableCollection<Document> Documents;
        CDN_Prezentacja_KHEntities dataEntities =  null;
       

        List<DatabaseModel> Databases = new List<DatabaseModel>();

        public MainWindow()
        {
            InitializeComponent();
            GetSettings();
            datepickerFromDate.SelectedDate = DateTime.Now.AddDays(-365);
            datepickerToDate.SelectedDate = DateTime.Now;

        }
        //private void MainWindow_Load(object sender, EventArgs e)
        //{
            
        //}
        //Łączenie 
        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", textBoxSerwer.Text, textBoxKonfig.Text, textBoxUser.Text, textBoxPassword.Password);
            try
            {
                SqlHelper helper = new SqlHelper(connectionString);
                if (helper.IsConnection)
                {

                    this.Databases = helper.GetDatabases(connectionString);
                }

                this.comboboxBazy.ItemsSource = this.Databases;


                SaveSettings();
                flyoutLogowanie.IsOpen = false;
                buttomBazaWybór.Visibility = Visibility.Visible;
                Login.Content = textBoxUser.Text;
                flyoutBaza.IsOpen = true;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Sortowanie po Datach
        private void showDocumnts_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate);
        }


        private void RefreshDataGridDocuments(DateTime? dateFrom = null, DateTime? dateTo = null,
                                              string documentNumber = null, int? customerID = null)
        {
            Documents = new ObservableCollection<Document>();

            List<int> documentsIDs = new List<int>();

            List<int> idsDate = new List<int>();
            List<int> idsDocument = new List<int>();
            List<int> idsCustomer = new List<int>();

            if (dateFrom != null && dateTo != null)
            {
                idsDate = dataEntities.TraNags.Where(x => x.TrN_DataWys >= dateFrom.Value && x.TrN_DataWys <= dateTo.Value).Select(x => x.TrN_TrNID).ToList();
                documentsIDs = idsDate;
            }

            if (!string.IsNullOrWhiteSpace(documentNumber))
            {
                idsDocument = dataEntities.TraNags.Where(x => x.TrN_NumerPelny.ToLower().Contains(documentNumber.ToLower())).Select(x => x.TrN_TrNID).ToList();

                if (idsDate.Count > 0)
                {
                    documentsIDs = idsDate.Where(x => idsDocument.Where(y => y == x).Any()).ToList();
                }
            }

            if (customerID != null)
            {
                idsCustomer = dataEntities.TraNags.Where(x => x.TrN_PodID == customerID.Value).Select(x => x.TrN_TrNID).ToList();

                if (documentsIDs.Count > 0)
                {
                    documentsIDs = idsCustomer.Where(x => documentsIDs.Where(y => y == x).Any()).ToList();
                }
                else
                    documentsIDs = idsCustomer;
            }

            var query =
           from TraNag in dataEntities.TraNags
           where documentsIDs.Where(c => c == TraNag.TrN_TrNID).Any()
           select new { TraNag.TrN_OdbID, TraNag.TrN_TrNID, TraNag.TrN_NumerPelny, TraNag.TrN_DataWys, TraNag.TrN_PodID, TraNag.TrN_PlatnikID, TraNag.TrN_Opis };


            if (query.Count() > 0)
            {
                foreach (var item in query)
                {
                    Document document = new Document()
                    {
                        DocumentID = item.TrN_TrNID,
                        DocumentNumber = item.TrN_NumerPelny,
                        DocumentDate = item.TrN_DataWys,
                        TargetCustomerID = item.TrN_OdbID,
                        TargetCustomerName = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == item.TrN_OdbID).FirstOrDefault()?.Knt_Kod,
                        PayerID = item.TrN_PlatnikID,
                        PayerName = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == item.TrN_PlatnikID).FirstOrDefault()?.Knt_Kod,
                        MainCustomerID = item.TrN_PodID,
                        MainCustomerName = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == item.TrN_PodID).FirstOrDefault()?.Knt_Kod,
                        DocumentDescription = item.TrN_Opis



                    };

                    Documents.Add(document);
                }
            }

            dataGrid1.ItemsSource = Documents;
        }


        
        

        //Drukowanie
        private void Drukuj(object sender, RoutedEventArgs e)
        {

            Print_WPF_Preview(dataGrid1);

        }

        //Podgląd obecnego DataGrida
        public void Print_WPF_Preview(FrameworkElement wpf_Element)
        {
            if (File.Exists("print_preview.xps") == true) File.Delete("print_preview.xps");

            XpsDocument doc = new XpsDocument("print_preview.xps", FileAccess.ReadWrite);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
            SerializerWriterCollator preview_Document = writer.CreateVisualsCollator();
            preview_Document.BeginBatchWrite();
            preview_Document.Write(wpf_Element);
            preview_Document.EndBatchWrite();

            FixedDocumentSequence preview = doc.GetFixedDocumentSequence();
            var window = new Window();
            window.Content = new DocumentViewer { Document = preview };
            window.ShowDialog();
            doc.Close();
        }

        //Sortowanie po wszystkich 3 kategoriach
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (this.comboBoxKontrahent.SelectedItem == null || !(this.comboBoxKontrahent.SelectedItem is Customer))
                return;

            int customerID = (comboBoxKontrahent.SelectedItem as Customer).ID.Value;

            if (customerID > 0)
            {
                RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate, this.TextBoxDocumentName.Text,
                                    customerID);
            }
            else
            {
                RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate, this.TextBoxDocumentName.Text);
            }


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.comboBoxKontrahent.SelectedItem == null || !(this.comboBoxKontrahent.SelectedItem is Customer))
                return;

            int customerID = (comboBoxKontrahent.SelectedItem as Customer).ID.Value;
            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate, this.TextBoxDocumentName.Text,
                                                customerID);
        }

        //Wczytywanie danych z wybranego row w datagridzie
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.dataGrid1.SelectedItem != null)
            {
                Document selectedDocument = (this.dataGrid1.SelectedItem as Document);
                if (selectedDocument != null)
                {
                    platnikInput.Text = selectedDocument.PayerName;
                    docelowyInput.Text = selectedDocument.TargetCustomerName;
                    glownyInput.Text = selectedDocument.MainCustomerName;
                    textBoxOpis.Text = selectedDocument.DocumentDescription;
                }
            }

        }
        public IEnumerable<DataGridRow> GetDataGridRows(DataGrid grid)
        {

            var itemsSource = grid.ItemsSource as IEnumerable;
            if (null == itemsSource) yield return null;
            foreach (var item in itemsSource)
            {
                var row = grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (null != row) yield return row;
            }
        }

       //zamiana głównego
        private void glReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid1.SelectedItem == null || this.chooseCombobox.SelectedItem == null)
                return;

            Document helper = this.dataGrid1.SelectedItem as Document;

            int? documentID = helper.DocumentID;
            var customerToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            if (customerToChange != null)
            {
                customerToChange.TrN_PodID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();
            }
            var customer = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == customerToChange.TrN_PodID).FirstOrDefault();
            if (customer != null)
            {
                helper.MainCustomerID = customer.Knt_GIDNumer;
                helper.MainCustomerName = customer.Knt_Kod;
            }




        }
       //zamiana docelowego
        private void docReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid1.SelectedItem == null || this.chooseCombobox.SelectedItem == null)
                return;

            Document helper = this.dataGrid1.SelectedItem as Document;

            int? documentID = helper.DocumentID;
            var docelToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            if (docelToChange != null)
            {
                docelToChange.TrN_OdbID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();
            }

            var customer = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == docelToChange.TrN_OdbID).FirstOrDefault();
            if (customer != null)
            {
                helper.TargetCustomerID = customer.Knt_GIDNumer;
                helper.TargetCustomerName = customer.Knt_Kod;
            }
        }
        //Zamiana płatnika
        private void platReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid1.SelectedItem == null || this.chooseCombobox.SelectedItem == null)
                return;

            Document helper = this.dataGrid1.SelectedItem as Document;

            int? documentID = helper.DocumentID;
            var PayerToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            if (PayerToChange != null)
            {
                PayerToChange.TrN_PlatnikID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();

            }

            var customer = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == PayerToChange.TrN_PlatnikID).FirstOrDefault();
            if (customer != null)
            {
                helper.PayerID = customer.Knt_GIDNumer;
                helper.PayerName = customer.Knt_Kod;
            }
        }
       //Zamiana według operacyjnego
        private void opReplace_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid1.SelectedItem == null || this.chooseCombobox.SelectedItem == null)
                return;

            Document helper = this.dataGrid1.SelectedItem as Document;

            int? documentID = helper.DocumentID;
            var PayerToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            var CustomerToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            var TargetToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            if (PayerToChange != null)
            {
                PayerToChange.TrN_PlatnikID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();
            }
            if (CustomerToChange != null)
            {
                CustomerToChange.TrN_PodID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();
            }
            if (TargetToChange != null)
            {
                TargetToChange.TrN_OdbID = (this.chooseCombobox.SelectedItem as Customer).ID;
                dataEntities.SaveChanges();
            }
            var customer = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == CustomerToChange.TrN_PodID).FirstOrDefault();
            if (customer != null)
            {
                helper.MainCustomerID = customer.Knt_GIDNumer;
                helper.MainCustomerName = customer.Knt_Kod;
            }
            var customer1 = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == TargetToChange.TrN_OdbID).FirstOrDefault();
            if (customer != null)
            {
                helper.TargetCustomerID = customer1.Knt_GIDNumer;
                helper.TargetCustomerName = customer1.Knt_Kod;
            }
            var customer2 = dataEntities.Kontrahencis.Where(x => x.Knt_GIDNumer == PayerToChange.TrN_PlatnikID).FirstOrDefault();
            if (customer != null)
            {
                helper.PayerID = customer2.Knt_GIDNumer;
                helper.PayerName = customer2.Knt_Kod;
            }
        }


        //Odświeżanie DataGrida
        private void RefreshGridButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate);
        }
       //Filtrowanie według dat
        private void OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datepickerFromDate.SelectedDate > datepickerToDate.SelectedDate)
            {
                datepickerFromDate.SelectedDate = datepickerToDate.SelectedDate.Value;
                MessageBox.Show("FromDate can't be higher than ToDate", "Put correct Date", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            else if (datepickerToDate.SelectedDate < datepickerFromDate.SelectedDate)
            {
                datepickerToDate.SelectedDate = datepickerFromDate.SelectedDate.Value;
                MessageBox.Show("ToDate can't be lower than FromDate", "Put correct Date", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        //Zapisywanie nnowego Opisu
        private void buttonSaveOpis_Click(object sender, RoutedEventArgs e)
        {
           
            if (this.dataGrid1.SelectedItem == null)
                return;

            Document helper = this.dataGrid1.SelectedItem as Document;

            int? documentID = helper.DocumentID;
            var opisToChange = dataEntities.TraNags.Where(x => x.TrN_TrNID == documentID).FirstOrDefault();
            if (opisToChange != null)
            {
                opisToChange.TrN_Opis = textBoxOpis.Text;
                dataEntities.SaveChanges();
                
            }


        }

     
        //Wybór Bazy danych
        private void comboboxBazy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.InitialCatalog = (comboboxBazy.SelectedItem as DatabaseModel).DbName;
            Settings.Default.DataSource = textBoxSerwer.Text;
            Settings.Default.UserID = textBoxUser.Text;
            Settings.Default.Password = textBoxPassword.Password;
            Settings.Default.konfig = textBoxKonfig.Text;

            Settings.Default.Save();

            GetConnectionString("Model1", Settings.Default);
           

            flyoutBaza.IsOpen = false;
            buttomBazaWybór.Content = (comboboxBazy.SelectedItem as DatabaseModel).DbName;
            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate);
            InitializeComarchData();

        }

        public void SaveSettings()
        {
            
            Settings.Default.DataSource = textBoxSerwer.Text;
            Settings.Default.UserID = textBoxUser.Text;
            Settings.Default.Password = textBoxPassword.Password;
            Settings.Default.konfig = textBoxKonfig.Text;
            Settings.Default.Save();
        }
        public void GetSettings()
        {
            textBoxUser.Text = Settings.Default.UserID;
            textBoxSerwer.Text = Settings.Default.DataSource;
            textBoxKonfig.Text = Settings.Default.konfig;
        }

        //Tworzenie ConnectionString na podstawie danych logowania
        private void GetConnectionString(string Model1, Settings settings)
        {
            // Build the provider connection string with configurable settings
            var providerSB = new SqlConnectionStringBuilder
            {
                // You can also pass the sql connection string as a parameter instead of settings
                InitialCatalog = settings.InitialCatalog,
                DataSource = settings.DataSource,
                UserID = settings.UserID,
                Password = settings.Password,
                MultipleActiveResultSets = true,

            };

            var efConnection = new EntityConnectionStringBuilder();
            // or the config file based connection without provider connection string
            // var efConnection = new EntityConnectionStringBuilder(@"metadata=res://*/model1.csdl|res://*/model1.ssdl|res://*/model1.msl;provider=System.Data.SqlClient;");
            efConnection.Provider = "System.Data.SqlClient";
            efConnection.ProviderConnectionString = providerSB.ConnectionString;
            // based on whether you choose to supply the app.config connection string to the constructor
            //*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;provider=System.Data.SqlClient;
            efConnection.Metadata = "res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl";
            string connection = efConnection.ToString();

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");
            connectionStringsSection.ConnectionStrings["CDN_Prezentacja_KHEntities"].ConnectionString = connection;
            config.AppSettings.SectionInformation.ForceSave = true;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("appSettings");


            if (dataEntities != null)
                dataEntities.Dispose();


            dataEntities = new CDN_Prezentacja_KHEntities();
        }
       //Inicjalizacja Query
        private void InitializeComarchData()
        {
            var query =
           from TraNag in dataEntities.TraNags
           join Kontrahenci in dataEntities.Kontrahencis on TraNag.TrN_PodID equals Kontrahenci.Knt_GIDNumer
           select new { TraNag.TrN_TrNID, TraNag.TrN_NumerPelny, TraNag.TrN_DataWys, TraNag.TrN_PodID, TraNag.TrN_PlatnikID, Kontrahenci.Knt_KntId, Kontrahenci.Knt_Kod };

            dataGrid1.ItemsSource = query.ToList();


            var query2 =
                from Kontrahenci in dataEntities.Kontrahencis
                select new { Kontrahenci.Knt_Kod, Kontrahenci.Knt_GIDNumer };



            this.Customers = new List<Customer>();
            this.Customers.Add(new Customer() { Code = "<wszyscy>", ID = -1 });

            foreach (var item in query2.ToList())
            {
                this.Customers.Add(new Customer()
                {
                    ID = item.Knt_GIDNumer,
                    Code = item.Knt_Kod
                });
            }

            comboBoxKontrahent.ItemsSource = this.Customers;
            chooseCombobox.ItemsSource = this.Customers;

            var query3 =
                from TraNag in dataEntities.TraNags
                join Kontrahenci in dataEntities.Kontrahencis on TraNag.TrN_PodID equals Kontrahenci.Knt_GIDNumer
                select new { TraNag.TrN_DataWys, TraNag.TrN_PodID, TraNag.TrN_PlatnikID, Kontrahenci.Knt_KntId };

            this.Details = new List<Detail>();

            foreach (var item in query3.ToList())
            {
                this.Details.Add(new Detail()
                {
                    KntId = item.Knt_KntId,
                    PlatId = item.TrN_PlatnikID,
                    PodId = item.TrN_PodID
                });
            }

            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate);
        }
        //Auto Zamykanie/otwieranie flyouta z bazami
        private void buttomBazaWybór_Click(object sender, RoutedEventArgs e)
        {
            this.flyoutBaza.IsOpen = !this.flyoutBaza.IsOpen;
        }

        //Sortowanie po wpisywaniu numeru dokumentu w textboxa
        private void KontNameSearchButton_Click(object sender, RoutedEventArgs e)
        {
            int? customerID = null;
            if (this.chooseCombobox.SelectedItem != null)
            {
                customerID = (this.chooseCombobox.SelectedItem as Customer).ID;

                if (customerID <= 0)
                    customerID = null;
            }
            RefreshDataGridDocuments(this.datepickerFromDate.SelectedDate, this.datepickerToDate.SelectedDate, this.TextBoxDocumentName.Text,
                                    customerID);
        }
    }
}









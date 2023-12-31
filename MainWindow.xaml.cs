﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiscordIdentifier
{
     /// <summary>
     /// Interaktionslogik für MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          Boolean onlyPermitted = false;
          private string getConStr()
          {
               string resourceFilePath = "DiscordIdentifier.Properties.Resources";
               ResourceManager resourceManager = new ResourceManager(resourceFilePath, typeof(MainWindow).Assembly);


               string serverValue = resourceManager.GetString("Server");
               string databaseValue = resourceManager.GetString("Database");


               return "Server=" + serverValue + ";Database="+ databaseValue + ";Integrated Security=True;";
          }

          public MainWindow()
          {
               InitializeComponent();
               //LoadComboBoxItems();
               LoadDataFromDatabase();

          }

          private List<string> LoadComboBoxItems()
          {
               List<string> comboBoxItems = new List<string>();

               // Hier führst du deine SQL-Abfrage aus, um die ComboBox-Elemente abzurufen
               string connectionString = getConStr();
               
               string query = "SELECT id, bezeichnung FROM description";

               if (onlyPermitted) {
                    query += " WHERE id=12";
							 }
							 else
							 {
                    query += " WHERE id!=12";
							 }

               using (SqlConnection connection = new SqlConnection(connectionString))
               {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                         while (reader.Read())
                         {
                              string bezeichnung = reader["bezeichnung"].ToString();
                              comboBoxItems.Add(bezeichnung);
                         }
                    }
                    connection.Close();
               }

               return comboBoxItems;
          }

          private void LoadDataFromDatabase()
          {
               string query = "SELECT ID,discord_ID,Expr1 AS bID,bezeichnung FROM idHasStatus";

               if (onlyPermitted)
               {
                    query += " WHERE Expr1=12";
               }
               else
               {
                    query += " WHERE Expr1!=12";
               }

               List<id_is_view> dataList = new List<id_is_view>();
               int counter = 0;
               using (SqlConnection connection = new SqlConnection(getConStr()))
               {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                         while (reader.Read())
                         {
                              counter += 1;
                              int thisid = (int)reader["ID"];
                              string discordID = reader["discord_ID"].ToString();
                              int id_bezeichnung = (int)reader["bID"];
                              string hatStatus = reader["bezeichnung"].ToString();
                              id_is_view item = new id_is_view { thisid = thisid, discord_ID = discordID, id_bezeichnung = id_bezeichnung, bezeichnung = hatStatus };
                              dataList.Add(item);
                         }
                    }
                    connection.Close();
               }
               lbCounter.Content = counter;
               List<string> comboBoxItems = LoadComboBoxItems();
               DataContext = new { ComboBoxItems = comboBoxItems };
               discordStatus.ItemsSource = dataList;
               ComboBox_newStatus.ItemsSource = LoadComboBoxItems();
          }

					private void ComboBox_status_SelectionChanged(object sender, SelectionChangedEventArgs e)
					{
               ComboBox comboBox = (ComboBox)sender;
               if ((comboBox.DataContext is DiscordIdentifier.id_is_view) && comboBox.IsDropDownOpen)
               {
                    id_is_view rowData = (id_is_view)comboBox.DataContext;
                    rowData.thisid = GetNewElementID(rowData.discord_ID);


                    if (comboBox.SelectedIndex > 0 && rowData.thisid > 0)
                    {
                         string query = "UPDATE ID_Status SET hatStatus=" + ((int)comboBox.SelectedIndex + 1) + " WHERE discord_ID=" + rowData.thisid;

                         using (SqlConnection connection = new SqlConnection(getConStr()))
                         {
                              connection.Open();

                              using (SqlCommand command = new SqlCommand(query, connection))
                              {
                                   command.ExecuteNonQuery();
                              }

                         }
                    }
               }
               
          }

          private int GetNewElementID(string discordID)
          {
               string query = "SELECT ID FROM ID_Discord WHERE discord_ID='" + discordID + "'";
               int thisid = -1;

               using (SqlConnection connection = new SqlConnection(getConStr()))
               {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                         while (reader.Read())
                         {
                              thisid = (int)reader["ID"];
                         }
                    }
                    connection.Close();
               }
               return thisid;
          }

          private void discordStatus_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
          {
               DataGrid dataGrid = (DataGrid)sender;

               // Überprüfe, ob die bearbeitete Spalte den Header "ID" hat
               if (e.Column.Header.ToString() == "ID")
               {
                    // Holen Sie die bearbeitete Zeile
                    var editedRow = e.Row.Item as id_is_view;

                    // Holen Sie die TextBox, die in der Zelle angezeigt wird
                    if (e.EditingElement is TextBox textBox)
                    {
                         // Hier kannst du auf den Wert in der bearbeiteten Zelle zugreifen
                         if (textBox.Text.Length > 17)
                         {
                              string query = "";
                              if ( CheckIfExist(textBox.Text) == 0 )
                              {
                                   //INSERT
                                   query = "INSERT INTO ID_Discord (discord_ID) VALUES('" + textBox.Text + "');";
                                   using (SqlConnection connection = new SqlConnection(getConStr()))
                                   {
                                        connection.Open();
                                        int offsetValue = 1;
                                        using (SqlCommand command = new SqlCommand(query, connection))
                                        {
                                             command.ExecuteNonQuery();
                                        }
                                        int newID = GetNewElementID(textBox.Text);
                                        if (onlyPermitted)
                                        {
                                             offsetValue = 0;
                                        }

                                        query = "INSERT INTO ID_Status (discord_ID, hatStatus) VALUES(" + newID + ", " + ((int)editedRow.id_bezeichnung + offsetValue) + ")";
                                        using (SqlCommand command = new SqlCommand(query, connection))
                                        {
                                             command.ExecuteNonQuery();
                                        }
                                        connection.Close();
                                   }
                              }
															else
															{
                                   //UPDATE
                                   id_is_view selectedData = (id_is_view)discordStatus.SelectedItem;
                                   query = "UPDATE ID_Discord SET discord_ID='" + textBox.Text + "' WHERE ID=" + selectedData.thisid;
                                   using (SqlConnection connection = new SqlConnection(getConStr()))
                                   {
                                        connection.Open();
                                        int offsetValue = 1;
                                        using (SqlCommand command = new SqlCommand(query, connection))
                                        {
                                             command.ExecuteNonQuery();
                                        }
                                        int newID = GetNewElementID(textBox.Text);
                                        if (onlyPermitted)
                                        {
                                             offsetValue = 0;
                                        }

                                        query = "UPDATE ID_Status SET hatStatus=" + ((int)editedRow.id_bezeichnung + offsetValue) + " WHERE discord_ID=" + selectedData.thisid;
                                        using (SqlCommand command = new SqlCommand(query, connection))
                                        {
                                             command.ExecuteNonQuery();
                                        }
                                        connection.Close();
                                   }
                              }
                         }
                    }
               }
          }

          private int CheckIfExist(string discordID)
          {
               string query = "SELECT * FROM ID_Discord WHERE discord_ID='" + discordID + "'";
               int rowCount = 0;

               using (SqlConnection connection = new SqlConnection(getConStr()))
               {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                         // Durch die Zeilen der Abfrageergebnisse iterieren und die Anzahl der Zeilen zählen
                         while (reader.Read())
                         {
                              rowCount++;
                         }
                    }

                    connection.Close();
               }

               return rowCount;
          }

     //     private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
					//{

     //          if(CheckIfExist(searchBox.Text) > 0)
					//		 {
					//					searchButton.IsEnabled = false;
					//					string searchText = searchBox.Text;

					//					// Durchsuchen die Zeilen des DataGrids nach dem gesuchten Text
					//					SearchAndFocus(searchText);

					//		 }
					//		 else
					//		 {
     //               searchButton.IsEnabled = true;
     //          }
					//}

					private void SearchAndFocus(string searchText)
					{
							 foreach (var item in discordStatus.Items)
							 {
										if (item is id_is_view row)
										{
												 //auf das entsprechende Datenfeld zugreifen, in diesem Fall bezeichnung
												 if (row.discord_ID == searchText)
												 {
															// Selektieren der Zeile
															discordStatus.SelectedItem = item;

															// Setzen Sie den Fokus auf die ausgewählte Zeile
															
                              Style selectedRowStyle = new Style(typeof(DataGridRow));
                              selectedRowStyle.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(Colors.White)));
                              discordStatus.RowStyle = selectedRowStyle;
                              discordStatus.ScrollIntoView(item);

                              break; // Schleife verlassen, sobald die Zeile gefunden wurde
												 }
										}
							 }
					}

					private void searchButton_Click(object sender, RoutedEventArgs e)
					{
               
               string query = "INSERT INTO ID_Discord (discord_ID) VALUES('" + searchBox.Text + "');";
               int newID = -1;
               using (SqlConnection connection = new SqlConnection(getConStr()))
               {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                         command.ExecuteNonQuery();
                    }
                    

                    int newStatus = 1;


                    if (onlyPermitted)
                    {
                         newStatus = 12;
                    }
                    else
                    {
                         if (ComboBox_newStatus.SelectedIndex > 1)
                         {
                              newStatus = ComboBox_newStatus.SelectedIndex + 1;
                         }
                    }

                    newID = GetNewElementID(searchBox.Text);
                    query = "INSERT INTO ID_Status (discord_ID, hatStatus) VALUES(" + newID + ", " + newStatus + ")";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                         command.ExecuteNonQuery();
                    }

                    connection.Close();
                    
               }
               if (newID > 0)
							 {
                    LeadInfobox(newID); //GetNewElementID()    
               }
               SearchAndFocus(searchBox.Text);
               searchBox.Clear();
               infoBox.Clear();
               LoadDataFromDatabase();

               
          }
          private void CopyRowsWithSpaceToClipboard(Boolean withTags = false)
          {
               // Überprüfen Sie, ob ein DataGridView-Steuerelement auf Ihrem Formular vorhanden ist und den Namen dataGridView1 hat.
               if (discordStatus != null && discordStatus.Items.Count > 0)
               {
                    StringBuilder clipboardContent = new StringBuilder();
                    clipboardContent.Clear();

                    foreach (id_is_view item in discordStatus.Items)
                    {

                         if (item.discord_ID != null)
                         {
                              clipboardContent.Append(item.discord_ID + " ");
                              if (withTags)
															{
                                   clipboardContent.AppendLine("\t" + item.bezeichnung);
															}
                         }
                    }

                    Clipboard.SetText(clipboardContent.ToString());
               }
          }

          private void clipboardButton_Click(object sender, RoutedEventArgs e)
          {
               CopyRowsWithSpaceToClipboard();
          }
          private void clipboardButtonTag_Click(object sender, RoutedEventArgs e)
          {
               CopyRowsWithSpaceToClipboard(true);

          }
          private void discordStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
					{
               infoBox.Visibility = Visibility.Visible;
               id_is_view thisSelection = (id_is_view)discordStatus.SelectedItem;
               //GetNewElementID()
               if (thisSelection != null) {
                    LeadInfobox(thisSelection.thisid);
               }
               
               //DataGrid comboBox = (DataGrid)sender;
               //ComboBox comboBox = (ComboBox)comboBox.SelectedItem
               //if ((comboBox.DataContext is DiscordIdentifier.id_is_view))
               //{
               //     id_is_view rowData = (id_is_view)comboBox.DataContext;
               //     rowData.thisid = GetNewElementID(rowData.discord_ID);


               //     if (comboBox.SelectedIndex > 0 && rowData.thisid > 0)
               //     {
               //          string query = "UPDATE ID_Status SET hatStatus=" + ((int)comboBox.SelectedIndex + 1) + " WHERE discord_ID=" + rowData.thisid;

               //          using (SqlConnection connection = new SqlConnection(getConStr()))
               //          {
               //               connection.Open();

               //               using (SqlCommand command = new SqlCommand(query, connection))
               //               {
               //                    command.ExecuteNonQuery();
               //               }

               //          }
               //     }
               //}
          }

          private void unbanUser_Click(object sender, RoutedEventArgs e)
					{
               

               if (discordStatus.SelectedItems.Count == 1){
                    id_is_view delThis = (id_is_view)discordStatus.SelectedItems[0];
                    using (SqlConnection connection = new SqlConnection(getConStr()))
                    {
                         connection.Open();
                         string query = "DELETE FROM ID_Status WHERE discord_ID=" + delThis.thisid;
                         using (SqlCommand command = new SqlCommand(query, connection))
                         {
                              command.ExecuteNonQuery();
                         }
                         query = "DELETE FROM ID_Discord WHERE ID=" + delThis.thisid;
                         using (SqlCommand command = new SqlCommand(query, connection))
                         {
                              command.ExecuteNonQuery();
                         }
                         connection.Close();

                         LoadDataFromDatabase();
                         searchBox.Clear();
                         infoBox.Clear();
                         searchButton.IsEnabled = true;
                         ComboBox_newStatus.SelectedIndex = 0;
                    }
               }

          }

					private void searchBox_PreviewKeyUp(object sender, KeyEventArgs e = null)
					{
               infoBox.Visibility = Visibility.Visible;
               if (CheckIfExist(searchBox.Text) > 0)
               {
                    searchButton.IsEnabled = false;
                    string searchText = searchBox.Text;

                    // Durchsuchen die Zeilen des DataGrids nach dem gesuchten Text
                    SearchAndFocus(searchText);

               }
               else
               {
                    searchButton.IsEnabled = true;
               }
          }
          private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
          {
               searchBox_PreviewKeyUp(sender);
          }

          private void clearButton_Click(object sender, RoutedEventArgs e)
					{
               searchBox.Clear();
               infoBox.Clear();
               ComboBox_newStatus.SelectedIndex = 0;
               discordStatus.UnselectAll();

          }

					private void toggleButton_Checked(object sender, RoutedEventArgs e)
					{
               if (toggleButton.IsChecked == true)
               {
                    // Der ToggleButton ist eingeschaltet
                    onlyPermitted = true;
                    toggleButton.Content = "zeige Blocklist";
                    toggleButton.Background = Brushes.Black;
                    toggleButton.Foreground = Brushes.White;
                    
                    ComboBox_newStatus.IsEnabled = false;
                    LoadDataFromDatabase();
                    ComboBox_newStatus.SelectedIndex = 0;
               }
               else
               {
                    // Der ToggleButton ist ausgeschaltet
                    onlyPermitted = false;
                    toggleButton.Content = "zeige Permits";
                    toggleButton.Background = Brushes.Yellow;
                    toggleButton.Foreground = Brushes.Black;
                    ComboBox_newStatus.IsEnabled = true;
                    LoadDataFromDatabase();
               }
          }

          private int CheckIfNoteExist(int dbID)
          {
               string query = "SELECT * FROM infoText WHERE tableID=" + dbID;
               int rowCount = 0;

               using (SqlConnection connection = new SqlConnection(getConStr()))
               {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                         // Durch die Zeilen der Abfrageergebnisse iterieren und die Anzahl der Zeilen zählen
                         while (reader.Read())
                         {
                              rowCount++;
                         }
                    }

                    connection.Close();
               }

               return rowCount;
          }

          private void LeadInfobox(int thisID)
					{
               //infoBox.Text = "";
               string connectionString = getConStr();
               if (CheckIfNoteExist(thisID)>0){
                    // info exists allready
                    

                    string query = "SELECT note FROM infoText WHERE tableID=" + thisID;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                         connection.Open();
                         using (SqlCommand command = new SqlCommand(query, connection))
                         using (SqlDataReader reader = command.ExecuteReader())
                         {
                              while (reader.Read())
                              {
                                   string bezeichnung = reader["note"].ToString();
                                   infoBox.Text = bezeichnung.Trim();
                              }
                         }
                         connection.Close();
                    }
               }
							 else
							 {
                    using (SqlConnection connection = new SqlConnection(getConStr()))
                    {
                         connection.Open();
                         string query = "INSERT INTO infoText (tableID,note) VALUES(" + thisID + ",'" + infoBox.Text.Trim() + "')";
                         using (SqlCommand command = new SqlCommand(query, connection))
                         {
                              command.ExecuteNonQuery();
                         }
                         connection.Close();
                    }
               }
               

          }
          private void SaveInfoBox()
					{
               id_is_view thisSelection = (id_is_view)discordStatus.SelectedItem;
               //GetNewElementID()
               if (thisSelection != null)
               {
                    
                    using (SqlConnection connection = new SqlConnection(getConStr()))
                    {
                         connection.Open();
                         string infoText = infoBox.Text.Trim();
                         string query = "UPDATE infoText SET note='" + infoText + "' WHERE tableID=" + thisSelection.thisid;
                         using (SqlCommand command = new SqlCommand(query, connection))
                         {
                              command.ExecuteNonQuery();
                         }
                         connection.Close();
                    }
               }
               

          }


					private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
					{
               MessageBox.Show("Hier folgt ein Fenster zum Editieren der DB Zugangsdaten.", "DB Settings", MessageBoxButton.OK, MessageBoxImage.Information);
          }

					private void infoBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
					{
               //save InfoBox
               SaveInfoBox();

          }
		 }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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

namespace DiscordIdentifier.pages
{
		 /// <summary>
		 /// Interaktionslogik für ServerSelect.xaml
		 /// </summary>
		 public partial class ServerSelect : Page
		 {
					
					public ServerSelect()
					{
							 InitializeComponent();

							 string resourceFilePath = "DiscordIdentifier.Properties.Resources";
							 ResourceManager resourceManager = new ResourceManager(resourceFilePath, typeof(MainWindow).Assembly);
							 tbServer.Text = resourceManager.GetString("Server");
							 tbDatabase.Text = resourceManager.GetString("Database");
					}

					private void Button_Click(object sender, RoutedEventArgs e)
					{
							 string resourceFilePath = "DiscordIdentifier.Properties.Resources";
							 ResourceManager resourceManager = new ResourceManager(resourceFilePath, typeof(MainWindow).Assembly);
							 //resourceManager.GetString("Server") = tbServer.Text 
							 //resourceManager("Database") = tbDatabase.Text;
					}
		 }
}

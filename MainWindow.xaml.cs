using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace Socket_Server_Wpf_Thread
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Socket socketGestore;

        public MainWindow()
        {
            InitializeComponent();
            Connessione();
            txt_Ricevi.IsEnabled = false;
        }

        private void Connessione()
        {
            //Se viene passata una stringa vuota come argomento hostNameOrAddress, questo metodo restituisce gli indirizzi dell'host locale
            IPHostEntry host = Dns.GetHostEntry("localhost");

            //prendo il primo elemento restituito dall'host 
            IPAddress ipAddress = host.AddressList[0];

            //associo ip e porta e creo un nuovo IPEndPoint
            IPEndPoint ipServer = new IPEndPoint(ipAddress, 11000);

            //genero una socket che lavorerà tramite TCP    
            Socket socketAscolto = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //associo la socket all'IPEndPoint 
            socketAscolto.Bind(ipServer);

            //definisco il numero di richieste che un socket può soddisfare prima di definirsi occupato
            socketAscolto.Listen(10);

            //accetto una richiesta di connessione da un client
            socketGestore = socketAscolto.Accept();

            //Creo il nuovo thread che verrà utilizzato per ricevere i messaggi provenienti dal client
            Thread ricevitoreMessaggi = new Thread(RicezioneMessaggiClient);

            //faccio partire il thread
            ricevitoreMessaggi.Start();

        }

        private void RicezioneMessaggiClient()
        {
            //definisco la stringa che mi permetterà di visualizzare il mess del client
            string messaggioClient = null;

            //definisco l'array di byte che verrà rimepito con i byte inviati dal client 
            byte[] bytes = null;

            do
            {
                while (true)
                {
                    //inizializzo l'array
                    bytes = new byte[1024];

                    //ricevo dalla socket i byte e definisco il numero di quanti ne ho ricevuti
                    int bytesRec = socketGestore.Receive(bytes);

                    //il Get.String mi permette di decodificare la sequenza di byte inviata dal client in una stringa nella quale verrà inserito il messaggio ricevuto
                    messaggioClient += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    //controllo che all'interno del messaggio sia contenuta la stringa che mi identifica la fine (<EOF>) e verifico che l'indice in cui si trova sia maggiore di -1, così capisco se è contenuto o meno
                    if (messaggioClient.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                //per rendere migliore l'interfaccia, rimuovo dal messaggio finale la sigla che mi indica la fine del messaggio
                string messaggioFinale = messaggioClient.Remove(messaggioClient.IndexOf("<"));

                //delego il lavoro di aggiornamento dell'interfaccia all'oggetto DispatcherObject che è associato al thread creato precedentemente
                Dispatcher.Invoke(() => { txt_Ricevi.Text = messaggioFinale; });

                //resetto il campo data 
                messaggioClient = "";

            } while (true);

        }
        private void SendMessage()
        {
            try
            {
                //inserisco all'interno dell'array i byte che sono stati catturati dalla TextBox e ci aggiungo la sigla che mi identifica la fine del messaggio 
                byte[] msg = Encoding.ASCII.GetBytes(txt_Invia.Text + "<EOF>");

                //invio tramite la socket il messaggio al client
                socketGestore.Send(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void btn_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
            txt_Ricevi.Text = "";
        }

       
    }
}
